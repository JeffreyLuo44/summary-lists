# API Contract (v1)

Base path: `/v1`

Authentication: Firebase ID token in `Authorization: Bearer <token>`

## Models

### List

```json
{
  "id": "list_123",
  "userId": "uid_abc",
  "title": "Books to Read",
  "summary": "This list tracks books I want to read in 2026.",
  "createdAt": "2026-02-21T10:00:00.000Z",
  "updatedAt": "2026-02-21T10:00:00.000Z"
}
```

### ListItem

```json
{
  "id": "item_456",
  "listId": "list_123",
  "userId": "uid_abc",
  "text": "Atomic Habits",
  "note": "Start in March",
  "position": 1,
  "createdAt": "2026-02-21T10:00:00.000Z",
  "updatedAt": "2026-02-21T10:00:00.000Z"
}
```

## Endpoints

- `GET /lists`
- `POST /lists`
- `PATCH /lists/:listId`
- `DELETE /lists/:listId`
- `GET /lists/:listId/items`
- `POST /lists/:listId/items`
- `PATCH /lists/:listId/items/:itemId`
- `DELETE /lists/:listId/items/:itemId`
- `POST /lists/:listId/regenerate-summary`

## Notes

- Every request resolves user from Firebase token.
- All list/item operations are user-owned only.
- Summary endpoint rewrites `lists/{listId}.summary`.
