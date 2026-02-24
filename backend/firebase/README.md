# Firebase Backend

This backend provides:

- Firebase Auth-based user identity
- Firestore persistence for lists and items
- HTTP Cloud Functions for CRUD and summary regeneration
- Firestore rules for per-user authorization

## Prerequisites

- Node.js 20+
- Firebase CLI (`npm i -g firebase-tools`)
- Firebase project

## Configure

1. Update `.firebaserc` with your Firebase project id.
2. Install deps:
   - `cd functions && npm install`
3. Configure summary generation environment variables (functions runtime):
   - Required for AI summaries: `GEMINI_API_KEY`
   - Optional model override: `GEMINI_MODEL` (default: `gemini-1.5-flash`)
   - If `GEMINI_API_KEY` is not set, backend falls back to deterministic non-AI summaries.

## Run locally

```bash
cd backend/firebase/functions
npm install
npm run build
cd ..
firebase emulators:start
```

## Deploy

```bash
firebase deploy --only "functions,firestore"
```

## API

The HTTP function is `api` and serves routes under `/v1`.

Examples:

- `GET /v1/lists`
- `POST /v1/lists`
- `GET /v1/lists/:listId/items`

See `docs/api-contract.md` for full route details.
