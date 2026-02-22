namespace SummaryLists.Mobile.Config;

public static class AppConfig
{
    // Firebase Web API key from Project Settings -> General.
    public const string FirebaseWebApiKey = "AIzaSyAelKFhywM-A-lmw92y7ryuaxBj318N8lo";

    // Cloud Functions API base URL, including /v1.
    public const string ApiBaseUrl = "https://us-central1-summary-lists.cloudfunctions.net/api/v1";

    // Ensure Firebase Console -> Authentication -> Sign-in method enables Email/Password.
}