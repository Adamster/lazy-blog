# Shelf operational decisions

## Cover retention

Shelf covers are immutable, content-addressed blobs. Uploading identical processed bytes reuses the same blob key.
Request handlers intentionally do not delete covers when a book is updated or deleted: a different book or a
concurrent request may still reference the same blob. Unreferenced covers are retained until an offline garbage
collector can compare a consistent database snapshot with blob inventory. Blob storage growth must be monitored.

## Initial administrator

`Authorization:BootstrapAdminUserName` identifies the initial Shelf administrator. The configured user must exist
when the application first creates the `Admin` role. Bootstrap never creates a user. Removing the role from a user
is an explicit administrative action and must not be silently reversed by unrelated request processing.

After a role change, the user must sign in again or refresh the access token so the JWT contains current roles.
