# Body parameters: backend implementation spec

This spec is for implementing user body parameters in the AnyFood .NET API. The frontend uses them on the profile page to estimate a daily calorie goal. Right now the frontend keeps them only in the browser's `localStorage`, so they are lost on another device or browser. The backend needs to store them per user and return them with the profile.

**The backend only stores and validates the data.** The frontend calculates the calorie estimate. When the user applies the estimate, the frontend saves it with the existing `PUT /api/diary/goal` endpoint, which does not change.

## Conventions

These follow the existing API (see `docs/food-diary-frontend-guide.md`):

- Every endpoint requires `Authorization: Bearer <accessToken>`. Without it the API returns `401` with no body.
- JSON uses camelCase.
- Dates are plain calendar dates, `yyyy-MM-dd`, with no time and no timezone (`DateOnly`).
- Business validation errors return `400` with `{ "message": string }` in Ukrainian. The frontend shows this message to the user as-is.
- Malformed requests, such as wrong JSON types or unknown enum strings, return the standard ASP.NET `ProblemDetails` `400`.

## Data model

Add these fields to the user, either as columns on the user table or as a 1:1 `UserBodyParams` table. All of them are nullable, because existing users have none of them set.

| Field | Type | Meaning |
|---|---|---|
| `sex` | enum `Sex`: `male`, `female` | Used by the BMR formula |
| `birthDate` | `DateOnly?` | Store this instead of age, so age stays correct over time |
| `heightCm` | `decimal?` | Height in centimeters |
| `weightKg` | `decimal?` | Current weight in kilograms |
| `activityLevel` | enum `ActivityLevel`: `sedentary`, `light`, `moderate`, `active`, `veryActive` | Daily activity |
| `weightGoal` | enum `WeightGoal`: `lose`, `maintain`, `gain` | What the user wants to do with their weight |
| `bodyParamsUpdatedAt` | `DateTimeOffset?` | Set by the server on every save. Not writable by the client |

Serialize enums as **camelCase strings**, exactly as listed above, not as numbers. The frontend uses these string values directly:

```csharp
// e.g. in Program.cs, if not already configured globally
options.JsonSerializerOptions.Converters.Add(
    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
```

Add an EF Core migration for the new fields.

## Endpoints

| Method | URL | Body | Response |
|---|---|---|---|
| GET | `/api/users/self` | none | `UserProfile`, now including `bodyParams` |
| PUT | `/api/users/self/body-params` | `BodyParamsRequest` | `BodyParams` |
| DELETE | `/api/users/self/body-params` | none | `204 No Content` |

### Models

```ts
// Response: every field is null until the user saves their parameters
export interface BodyParams {
  sex: 'male' | 'female' | null;
  birthDate: string | null;      // yyyy-MM-dd
  heightCm: number | null;
  weightKg: number | null;
  activityLevel: 'sedentary' | 'light' | 'moderate' | 'active' | 'veryActive' | null;
  weightGoal: 'lose' | 'maintain' | 'gain' | null;
  updatedAt: string | null;      // ISO 8601 date-time
}

// Existing profile response, extended
export interface UserProfile {
  id: string;
  name: string;
  username: string;
  email: string;
  avatarUrl?: string;
  bodyParams: BodyParams;        // NEW: always present; fields are null when not set
}

// Request: all fields required
export interface BodyParamsRequest {
  sex: 'male' | 'female';
  birthDate: string;             // yyyy-MM-dd
  heightCm: number;
  weightKg: number;
  activityLevel: 'sedentary' | 'light' | 'moderate' | 'active' | 'veryActive';
  weightGoal: 'lose' | 'maintain' | 'gain';
}
```

### GET `/api/users/self`

This is the existing endpoint. Add the `bodyParams` object to its response. Always include the object, even when no parameters are saved; in that case every field inside it is `null`. Do not omit it and do not return `bodyParams: null`.

### PUT `/api/users/self/body-params`

Replaces all body parameters of the current user. The frontend always sends the full set of fields, so this is not a partial update. The server sets `updatedAt`. The response is the saved `BodyParams`.

Validation rules. Each rule returns `400 { message }` with the message shown below:

| Rule | Message |
|---|---|
| `birthDate` gives an age from 15 to 100 years on the server's current date | `Вік має бути від 15 до 100 років` |
| `birthDate` is not in the future | `Дата народження не може бути в майбутньому` |
| `heightCm` is from 100 to 250 | `Зріст має бути від 100 до 250 см` |
| `weightKg` is from 30 to 300 | `Вага має бути від 30 до 300 кг` |

A missing field or an unknown enum string is a malformed request and returns `ProblemDetails`.

These ranges match the frontend's `isValidBodyParams` in `apps/anyfood/src/app/apps/profile/utils/calorie-calculator.util.ts`. Keep them the same, because the frontend validates first and shows no estimate for values outside the ranges.

Store `heightCm` and `weightKg` with up to one decimal place, for example `72.5`.

### DELETE `/api/users/self/body-params`

Sets all body parameter fields, including `updatedAt`, back to `null`. The user's calorie goals are not changed. Returns `204`.

## Out of scope

Do not implement these now:

- **Server-side calorie calculation.** The frontend does it. For reference, it uses Mifflin–St Jeor BMR × an activity factor (1.2 / 1.375 / 1.55 / 1.725 / 1.9), then ×0.8 to lose weight (never below BMR), ×1.0 to maintain, or ×1.1 to gain, rounded to the nearest 10 kcal.
- **Changing the calorie goal automatically when the weight changes.** The user applies a new goal explicitly through `PUT /api/diary/goal`.
- **Weight history.** Only the current weight is stored.

## Acceptance checklist

- [ ] Migration adds the nullable fields. Existing users load without errors and get `bodyParams` with all fields `null`.
- [ ] `GET /api/users/self` includes `bodyParams`.
- [ ] `PUT /api/users/self/body-params` saves the parameters, sets `updatedAt` and returns `BodyParams`.
- [ ] Each validation rule returns `400 { message }` with the Ukrainian message from the table.
- [ ] Enums are serialized and accepted as camelCase strings.
- [ ] `DELETE /api/users/self/body-params` clears the fields and returns `204`.
- [ ] Every endpoint returns `401` without a token, and users can only read and change their own parameters.
- [ ] Tests cover the validation rules and the empty `bodyParams` response for a user who has saved nothing.
