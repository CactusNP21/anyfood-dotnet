# Food Diary: frontend integration guide

This guide is for implementing the Food Diary feature in the Angular frontend against the AnyFood .NET API. Backend source: `API/Controllers/FoodDiaryController.cs`, `Application/FoodDiary/`.

The food diary tracks what the user ate on each calendar day, totals calories, protein, fat and carbs, and compares them with that day's calorie goal.

## Conventions

- **Base route:** `/api/diary`. Every endpoint requires `Authorization: Bearer <accessToken>`, the same token the existing auth flow already attaches. Without it the API returns `401` with no body.
- **JSON** uses camelCase.
- **Dates** are plain calendar dates, `yyyy-MM-dd` (e.g. `2026-09-14`), in both URLs and JSON. There is no time component and no timezone. Always build them from the user's **local** date. Never use `new Date().toISOString().slice(0, 10)`, which gives the UTC date and is off by one around midnight.
- **Errors:**
  - Business errors return `{ "message": string }` with status `400` (invalid input) or `404` (product or recipe not found). The message is in Ukrainian and can be shown to the user as-is.
  - Malformed requests (bad date in the URL, wrong JSON types) return ASP.NET `ProblemDetails` with status `400` and an `errors` object. Treat these as programming errors, not user-facing messages.

## TypeScript models

```ts
export interface FoodDiaryEntryRequest {
  productId: number | null; // exactly one of productId / recipeId must be set
  recipeId: number | null;
  weight: number;           // grams, > 0
  time: number;             // same convention as DayPlan entry `time`; reuse the day-plan helpers
}

export interface FoodDiaryEntry {
  id: number;               // changes on every save, don't keep references to it
  productId: number | null;
  recipeId: number | null;
  name: string;
  imageUrl: string | null;
  weight: number;
  time: number;
  calories: number;         // already scaled to `weight`
  protein: number;
  fat: number;
  carbs: number;
}

export interface FoodDiaryDay {
  date: string;             // yyyy-MM-dd
  calorieGoal: number | null; // null = user never set a goal on or before this date
  isGoalOverridden: boolean;  // true = goal was set manually for this specific day
  totalCalories: number;
  totalProtein: number;
  totalFat: number;
  totalCarbs: number;
  entries: FoodDiaryEntry[];  // sorted by time
}

export type FoodDiaryDaySummary = Omit<FoodDiaryDay, 'entries'>;

export interface SetCalorieGoalRequest {
  calories: number;         // integer > 0
  effectiveFrom: string;    // yyyy-MM-dd, normally the user's local "today"
}

export interface SetDayCalorieGoalRequest {
  calories: number;         // integer > 0
}
```

## Endpoints

| Method | URL | Body | Response |
|---|---|---|---|
| GET | `/api/diary/{date}` | none | `FoodDiaryDay` |
| GET | `/api/diary?from={date}&to={date}` | none | `FoodDiaryDaySummary[]` |
| PUT | `/api/diary/{date}/entries` | `FoodDiaryEntryRequest[]` | `FoodDiaryDay` |
| PUT | `/api/diary/{date}/goal` | `SetDayCalorieGoalRequest` | `FoodDiaryDay` |
| DELETE | `/api/diary/{date}/goal` | none | `FoodDiaryDay` |
| PUT | `/api/diary/goal` | `SetCalorieGoalRequest` | `204 No Content` |

### GET `/api/diary/{date}`
Returns the day even if nothing has been logged, with empty `entries`, zero totals and the goal that applies to that date. A `404` is never returned for an empty day.

### GET `/api/diary?from=&to=`
Returns **one item for every date** from `from` to `to` inclusive, in ascending order, including empty days. Use it for calendar or week views and charts.
- `from` must not be after `to`. The range can be at most **366 days**. Breaking either rule returns `400 { message }`.

### PUT `/api/diary/{date}/entries`: the only way to change entries
There are **no** endpoints to add, edit or delete a single entry. The request body is the **complete** list of entries for the day, and the server replaces whatever was there before.
- Add: send the current entries plus the new one.
- Edit: send the list with the changed item.
- Remove: send the list without the item.
- Clear the day: send `[]`.

The response is the full updated `FoodDiaryDay`. Replace local state with it; don't merge.

To build the request from the current state, map each `FoodDiaryEntry` to `{ productId, recipeId, weight, time }` and drop `id` and the calculated fields.

Validation (returns `400` or `404` with `{ message }`, and nothing is saved):
- Each entry must have exactly one of `productId` and `recipeId`.
- `weight` must be > 0.
- Every referenced product and recipe must exist (`404`).

### Calorie goals

Every day has a goal, worked out as follows:
1. If the day has a manual override (`isGoalOverridden: true`), that value is used.
2. Otherwise, the latest default goal whose `effectiveFrom` is on or before that date is used.
3. If neither exists, `calorieGoal` is `null`.

**Changing the default goal** (`PUT /api/diary/goal`) only affects dates from `effectiveFrom` onward. Earlier days keep the goal they had. Example: the goal is 2000 from Sep 1 and the user changes it to 1800 on Sep 15. Sep 14 still shows 2000; Sep 15 and later show 1800. Changing it twice on the same `effectiveFrom` date overwrites the earlier value. Send the user's local today as `effectiveFrom`, unless the UI deliberately lets them choose a start date.

**Per-day override:** `PUT /api/diary/{date}/goal` sets the goal for that one day only. `DELETE /api/diary/{date}/goal` removes the override, and the day goes back to the default goal (rule 2). Both return the updated `FoodDiaryDay`.

There is no endpoint that returns the current default goal on its own. To show it, for example on a settings screen, read `calorieGoal` from `GET /api/diary/{today}`. If `isGoalOverridden` is true, today's value is a manual override, not the default.

## Behavior to account for in the UI

- **Values are calculated live.** Entry and total calories and macros come from the product's or recipe's *current* nutrition values. If a product's data is edited later, past days change too. This is intended; don't cache old values as if they were fixed.
- **Product and recipe values are per 100 g.** The API already scales them to `weight`, so the client doesn't need to calculate anything.
- **Entry `id`s are regenerated on every PUT.** Use them only for `trackBy` within one response.
- **Products used in the diary can't be deleted.** `DELETE /api/products/{id}` now returns `400 { message }` if any diary entry references the product. Show that message on the product screens.
- **Concurrent edits:** the last PUT wins. If the same day is edited in two tabs, the later save overwrites the earlier one. Re-fetch the day when the diary view gains focus or is reopened.
- **Remaining calories** are not returned. Calculate `calorieGoal - totalCalories` on the client when `calorieGoal` isn't null.

## Suggested implementation

1. Create a `FoodDiaryService` (Angular `HttpClient`) with one method per endpoint above, using the models in this guide.
2. Add a date utility that formats a local `Date` as `yyyy-MM-dd`, and use it everywhere a date is sent.
3. Diary day screen: load `GET /api/diary/{date}`, keep the entries in local state, and send the full list with `PUT .../entries` after each add, edit or remove (or on an explicit save). Then render the returned day.
4. For adding food, reuse the existing product and recipe pickers from the day-plan feature. They already produce `productId` or `recipeId`, `weight` and `time`.
5. Goal UI: one control for editing the default goal (`PUT /api/diary/goal` with today as `effectiveFrom`), and on the day screen an "edit goal for this day" action plus a "reset to default" action shown when `isGoalOverridden` is true.
6. Calendar or history view: `GET /api/diary?from=&to=` for the visible range, comparing `totalCalories` with `calorieGoal` for each day.
