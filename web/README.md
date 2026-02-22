# Web App (React)

Implemented:

- Firebase Auth email/password login and registration
- List tabs with select/create/rename/delete
- List item create/edit/delete
- Summary card pinned at top of selected list
- "Regenerate Summary" action through backend API

## Configure

1. Copy `.env.example` to `.env`.
2. Fill Firebase app config values.
3. Set `VITE_API_BASE_URL`.
   - Local emulator example:
     - `http://127.0.0.1:5001/<project-id>/us-central1/api/v1`
   - Deployed example:
     - `https://us-central1-<project-id>.cloudfunctions.net/api/v1`

## Run

```bash
npm install
npm run dev
```

## Deploy To GitHub Pages

From `web/`:

```bash
npm install
npm run deploy
```

This deploy script:

- builds with `VITE_BASE_PATH=/summary-lists/`
- publishes `dist/` to the `gh-pages` branch

In GitHub repository settings, set Pages source to `gh-pages` branch (root).
