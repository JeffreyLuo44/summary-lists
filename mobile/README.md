# Mobile App (.NET MAUI)

Implemented project: `mobile/SummaryLists.Mobile`

Implemented features:

- MVVM structure with `Views/` + `ViewModels/`
- Views implemented as C# markup files (`.cs`) without `xaml/xaml.cs`
- `CommunityToolkit.Mvvm` observable properties and relay commands
- Fluent MAUI property extension methods in `Extensions/MauiPropertyExtensions.cs`
- Firebase Auth sign-in and registration via REST API
- Persisted auth token/session using local preferences
- Lists screen with horizontal list tabs
- List create/rename/delete
- Item create/edit/delete
- Summary card pinned at top of selected list with regenerate action

## Configure

Edit `mobile/SummaryLists.Mobile/Config/AppConfig.cs`:

- `FirebaseWebApiKey`
- `ApiBaseUrl` (include `/v1`)

Registration flow details:

- Registration mode requires password confirmation.
- Firebase auth errors are mapped to user-friendly messages.
- Firebase Email/Password sign-in method must be enabled in Firebase Console.

## Visual Studio (Solution)

Open `mobile/SummaryLists.Mobile.sln`.

- Startup project: `SummaryLists.Mobile`
- Recommended first run target: `Windows Machine`

## Run

```bash
cd mobile/SummaryLists.Mobile
dotnet build -f net9.0-windows10.0.19041.0
```

## Tests

```bash
dotnet test mobile/SummaryLists.Mobile.Tests/SummaryLists.Mobile.Tests.csproj -f net9.0-windows10.0.19041.0
```

For Android/iOS, build with installed MAUI workloads and platform SDKs.
