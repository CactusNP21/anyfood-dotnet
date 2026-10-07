# Recipe update and delete: frontend integration guide

This guide is for implementing recipe editing and deletion in the Angular frontend against the AnyFood .NET API. Backend source: `API/Controllers/RecipeController.cs` (`Update`, `Delete`), `API/MultipartFormModels/UpdateRecipeFormRequest.cs`, `Application/Recipes/Services/RecipeService.cs` (`UpdateAsync`).

## Endpoint

| Method | URL | Body | Response |
|---|---|---|---|
| PUT | `/api/recipe/{id}` | `multipart/form-data` | `200` with `RecipeDto` |
| DELETE | `/api/recipe/{id}` | none | `204 No Content` |

Everything below up to **Delete** is about PUT.

- **Requires** `Authorization: Bearer <accessToken>`.
- **Body must be `multipart/form-data`.** JSON bodies are rejected with `415 Unsupported Media Type`. This is what causes the 415 you saw before.
- Build the body with `FormData` and pass it straight to `HttpClient.put`. **Do not set `Content-Type` yourself.** The browser has to add the multipart boundary, and setting the header by hand breaks the request. If an HTTP interceptor adds `Content-Type: application/json` to every request, it must skip `FormData` bodies.

## Who can edit

Only the recipe's author (`recipe.userId === currentUser.id`) or a user with the `Admin` role can edit or delete. Anyone else gets `401 { message }`. Hide the edit and delete buttons for other users.

## Semantics: full replacement

PUT replaces the whole recipe. Every request must contain the complete, final state:
- `RecipeProducts`, `RecipeCategories` and `Steps` are **complete lists**. Anything left out is deleted. An empty `Steps` list removes all steps.
- Nutrition (`calories`, `protein`, `fat`, `carbs`, `salt`) and `price` are recalculated on the server from the ingredients. Don't send them.

### Images

| What the user did | What to send |
|---|---|
| Kept the main photo | Don't send `Image` at all |
| Replaced the main photo | `Image` = the new `File` |
| Kept a step's photo | `Steps[i].ImageUrl` = that step's current `imageUrl` from `RecipeDto`, exactly as received |
| Added or replaced a step's photo | `Steps[i].Image` = the new `File` (don't send `ImageUrl` for this step) |
| Removed a step's photo, or the step has none | Send neither `Image` nor `ImageUrl` for this step |

Rules:
- The main photo can't be removed, only kept or replaced.
- A step photo is **not** kept automatically. If you leave out `ImageUrl`, the step's photo is removed.
- `Steps[i].ImageUrl` must be a URL that already belongs to one of **this recipe's** steps. Any other URL returns `400`. It may come from a different step of the same recipe, so reordering steps works: send each photo's URL with the step's new `Order`.
- If a step has both `Image` and `ImageUrl`, `Image` wins.
- Files must be valid images (JPEG, PNG, WebP, …). A broken or unsupported file returns `400`.

## Form fields

Field names are case-insensitive. Use bracket indexes for lists, starting at `0`, with no gaps.

| Field | Type | Required | Notes |
|---|---|---|---|
| `Name` | string | yes | |
| `Description` | string | no | default `""` |
| `Duration` | int | no | minutes, default `0` |
| `Image` | file | no | new main photo; omit to keep the current one |
| `RecipeProducts[i].ProductId` | int | yes, at least 1 item | each product at most once |
| `RecipeProducts[i].Weight` | number | yes | grams, > 0 |
| `RecipeCategories[i].Id` | int | yes, for each category sent | unknown ids are ignored; send no `RecipeCategories` fields when the recipe has no categories |
| `Steps[i].Order` | int | yes | unique within the request; steps are returned sorted by it |
| `Steps[i].Description` | string | no | |
| `Steps[i].Timer` | int | no | default `0` |
| `Steps[i].Image` | file | no | new step photo |
| `Steps[i].ImageUrl` | string | no | existing step photo to keep (see above) |

Use `.` for decimal weights (`12.5`), not `,`.

## Errors

All return `{ "message": string }` in Ukrainian, which can be shown as-is. Nothing is saved when any of them is returned.

| Status | When |
|---|---|
| 400 | no ingredients; weight ≤ 0; duplicate product; duplicate step `Order`; invalid image file; `ImageUrl` not from this recipe |
| 401 | not logged in (no body), or not the author and not an admin |
| 404 | recipe not found; product not found |
| 415 | body is not `multipart/form-data` |

A missing required field (`Name`) returns an ASP.NET `ProblemDetails` `400` with an `errors` object. Treat that as a programming error.

## Response

The full updated `RecipeDto`, the same shape as `GET /api/recipe/{id}`, with `products` (each including `weight`) and `steps` sorted by `order`. Replace the local state with it, including step `imageUrl`s: newly uploaded photos come back as URLs, and those are what you send as `ImageUrl` in the next edit.

## TypeScript

```ts
export interface RecipeStepEdit {
  order: number;
  description: string;
  timer: number;
  imageUrl: string | null; // existing photo to keep
  newImage: File | null;   // photo picked in this edit; takes priority over imageUrl
}

export interface RecipeEdit {
  name: string;
  description: string;
  duration: number;
  newImage: File | null;   // null = keep current main photo
  products: { productId: number; weight: number }[];
  categories: { id: number; name: string }[];
  steps: RecipeStepEdit[];
}

// Initialise the edit form from the loaded recipe
export function toRecipeEdit(r: RecipeDto): RecipeEdit {
  return {
    name: r.name,
    description: r.description,
    duration: r.duration,
    newImage: null,
    products: r.products.map(p => ({ productId: p.id, weight: p.weight ?? 0 })), // weight is always set on recipe products
    categories: r.recipeCategories.map(c => ({ id: c.id, name: c.name })),
    steps: r.steps.map(s => ({
      order: s.order, description: s.description, timer: s.timer,
      imageUrl: s.imageUrl, newImage: null,
    })),
  };
}

export function toUpdateFormData(e: RecipeEdit): FormData {
  const fd = new FormData();
  fd.append('Name', e.name);
  fd.append('Description', e.description ?? '');
  fd.append('Duration', String(e.duration ?? 0));
  if (e.newImage) fd.append('Image', e.newImage);

  e.products.forEach((p, i) => {
    fd.append(`RecipeProducts[${i}].ProductId`, String(p.productId));
    fd.append(`RecipeProducts[${i}].Weight`, String(p.weight));
  });

  e.categories.forEach((c, i) => {
    fd.append(`RecipeCategories[${i}].Id`, String(c.id));
  });

  e.steps.forEach((s, i) => {
    fd.append(`Steps[${i}].Order`, String(s.order));
    fd.append(`Steps[${i}].Description`, s.description ?? '');
    fd.append(`Steps[${i}].Timer`, String(s.timer ?? 0));
    if (s.newImage) fd.append(`Steps[${i}].Image`, s.newImage);
    else if (s.imageUrl) fd.append(`Steps[${i}].ImageUrl`, s.imageUrl);
  });

  return fd;
}

// RecipeService
update(id: number, edit: RecipeEdit) {
  return this.http.put<RecipeDto>(`/api/recipe/${id}`, toUpdateFormData(edit));
}
```

## Example: keep old photos, add a step with a new photo

The recipe currently has a main photo `/images/3fd6e6be528c182d/1200.webp`, step 1 with photo `/images/ab28b4a64e82968a/500.webp`, and step 2 without a photo. The user adds step 3 with a photo:

```
Name=Soup
RecipeProducts[0].ProductId=1
RecipeProducts[0].Weight=100
Steps[0].Order=1
Steps[0].Description=Chop
Steps[0].ImageUrl=/images/ab28b4a64e82968a/500.webp   ← keep step 1 photo
Steps[1].Order=2
Steps[1].Description=Boil
Steps[2].Order=3
Steps[2].Description=Serve
Steps[2].Image=<File>                                  ← new step 3 photo
                                                       (no Image field → main photo kept)
```

## UI notes

- Preview newly picked files with `URL.createObjectURL(file)`. Show existing photos from `imageUrl`.
- When the user removes a step photo, set both `imageUrl` and `newImage` to `null`.
- When steps are reordered, renumber `order` as `1..n` before saving, and keep each step's `imageUrl` and `newImage` with the step.
- Disable the save button while the request is running. Uploads can take a moment because images are resized on the server.

## Delete

`DELETE /api/recipe/{id}` permanently removes the recipe together with its steps, ingredients and category links. It also disappears from every user's saved recipes. There is no undo, so ask for confirmation first.

| Status | When |
|---|---|
| 204 | deleted |
| 400 `{ message }` | the recipe is used in someone's day plan or food diary; nothing is deleted |
| 401 | not logged in (no body), or not the author and not an admin (`{ message }`) |
| 404 `{ message }` | recipe not found or already deleted |

Show the `400` message as-is. To delete such a recipe, its entries have to be removed from the day plans and diary first. Day plans and diaries can belong to other users, so the author may not be able to do that alone.

After `204`, remove the recipe from any local lists and navigate away from its detail page.

```ts
// RecipeService
delete(id: number) {
  return this.http.delete<void>(`/api/recipe/${id}`);
}
```
