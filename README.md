# Summary Lists

Cross-platform summary list app with:

- `web/`: React web client
- `mobile/`: .NET MAUI mobile client
- `backend/firebase/`: Firebase backend (Auth, Firestore, Cloud Functions)

## Implemented

- Firebase Authentication for user login.
- Firestore for user-owned list and item storage.
- Cloud Functions (HTTP) for CRUD and AI summary generation.
- Firestore Security Rules enforce per-user access.
- React web app with login, list tabs, summary card, and list/item CRUD.
- MAUI app with MVVM (`Views` + `ViewModels`) including login/registration, list tabs, summary card, and list/item CRUD.

## Repository Layout

- `backend/firebase/README.md`
- `backend/firebase/firebase.json`
- `backend/firebase/firestore.rules`
- `backend/firebase/firestore.indexes.json`
- `backend/firebase/functions/`
- `docs/api-contract.md`
- `web/README.md`
- `mobile/README.md`
- `mobile/SummaryLists.Mobile.Tests/`

## Quick Start

1. Create a Firebase project and update `backend/firebase/.firebaserc`.
2. Set backend secret (optional, enables AI summary generation):
   - `firebase functions:secrets:set OPENAI_API_KEY`
3. Backend setup:
   - `cd backend/firebase/functions`
   - `npm install`
   - `npm run build`
4. Web setup:
   - `cd web`
   - Copy `.env.example` to `.env` and set values.
   - `npm install`
   - `npm run dev`
5. Mobile setup:
   - Edit `mobile/SummaryLists.Mobile/Config/AppConfig.cs`.
   - Open `mobile/SummaryLists.Mobile.sln` in Visual Studio (startup project: `SummaryLists.Mobile`).
   - `dotnet build mobile/SummaryLists.Mobile/SummaryLists.Mobile.csproj -f net9.0-windows10.0.19041.0`
