# Shelf operational decisions

## Series and volumes

A book belongs to a `Series` (a cycle) or stands alone; there is no separate "volume" concept — each volume is its
own `Book` row with `SeriesId` and `VolumeNumber` set. `VolumeNumber` is required and unique within a series when
`SeriesId` is set, and both fields must be null together on a standalone book (enforced in `Book.UpdateCatalog`).
Reading status is tracked per book (`UserBookStatus`), so per-volume progress ("reading volume 2, finished volume 3")
falls out of the existing per-book status table with no extra modeling. `GET /api/series/{slug}` returns a series
with all of its volumes, each carrying the caller's status for that volume.

## Cover retention

Shelf covers are immutable, content-addressed blobs. Uploading identical processed bytes reuses the same blob key.
Request handlers intentionally do not delete covers when a book is updated or deleted: a different book or a
concurrent request may still reference the same blob. Unreferenced covers are retained until an offline garbage
collector can compare a consistent database snapshot with blob inventory. Blob storage growth must be monitored.

## Initial administrator

`Authorization:BootstrapAdminEmails` (a JSON array of email addresses) identifies the initial Shelf administrators.
Set it via User Secrets or environment variables — never commit real email addresses to `appsettings.json`, since a
disclosed target identifier lets an attacker race-register that exact address before the real owner signs up.

At least one configured account must already exist when the application first creates the `Admin` role. Bootstrap
never creates a user, so register the account through the normal sign-up flow first. On every startup the app
retries against the configured emails until at least one resolves to an existing user; once that happens the role
gets a completion claim and bootstrap stops running. Removing the role from a user afterwards is an explicit
administrative action and must not be silently reversed by a later restart.

After a role change, the user must sign in again or refresh the access token so the JWT contains current roles.
