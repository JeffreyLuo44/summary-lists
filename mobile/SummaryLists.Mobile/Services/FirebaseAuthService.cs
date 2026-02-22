using System.Net.Http.Json;
using System.Text.Json;
using SummaryLists.Mobile.Config;

namespace SummaryLists.Mobile.Services;

public sealed class FirebaseAuthService : IAuthService
{
    private const string IdTokenKey = "auth.idToken";
    private const string RefreshTokenKey = "auth.refreshToken";
    private const string EmailKey = "auth.email";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public FirebaseAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var endpoint =
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={AppConfig.FirebaseWebApiKey}";
        return await AuthenticateAsync(endpoint, email, password, cancellationToken);
    }

    public async Task<AuthSession> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var endpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={AppConfig.FirebaseWebApiKey}";
        return await AuthenticateAsync(endpoint, email, password, cancellationToken);
    }

    public Task<AuthSession?> GetSessionAsync()
    {
        var token = Preferences.Get(IdTokenKey, string.Empty);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<AuthSession?>(null);
        }

        return Task.FromResult<AuthSession?>(new AuthSession
        {
            IdToken = token,
            RefreshToken = Preferences.Get(RefreshTokenKey, string.Empty),
            Email = Preferences.Get(EmailKey, string.Empty),
        });
    }

    public Task SignOutAsync()
    {
        Preferences.Remove(IdTokenKey);
        Preferences.Remove(RefreshTokenKey);
        Preferences.Remove(EmailKey);
        return Task.CompletedTask;
    }

    private async Task<AuthSession> AuthenticateAsync(
        string endpoint,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        if (AppConfig.FirebaseWebApiKey.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Set AppConfig.FirebaseWebApiKey before signing in.");
        }

        var normalizedEmail = email.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var payload = new
        {
            email = normalizedEmail,
            password,
            returnSecureToken = true,
        };

        using var response = await _httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = ParseFirebaseError(content);
            throw new InvalidOperationException(message);
        }

        var data = JsonSerializer.Deserialize<AuthResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Invalid auth response.");

        var session = new AuthSession
        {
            IdToken = data.IdToken ?? string.Empty,
            RefreshToken = data.RefreshToken ?? string.Empty,
            Email = data.Email ?? string.Empty,
        };

        if (string.IsNullOrWhiteSpace(session.IdToken))
        {
            throw new InvalidOperationException("Missing auth token.");
        }

        Preferences.Set(IdTokenKey, session.IdToken);
        Preferences.Set(RefreshTokenKey, session.RefreshToken);
        Preferences.Set(EmailKey, session.Email);

        return session;
    }

    private static string ParseFirebaseError(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var errorObj) &&
                errorObj.TryGetProperty("message", out var message))
            {
                var code = message.GetString() ?? string.Empty;
                return code switch
                {
                    "EMAIL_EXISTS" => "That email is already registered.",
                    "INVALID_EMAIL" => "Email format is invalid.",
                    "INVALID_LOGIN_CREDENTIALS" => "Email or password is incorrect.",
                    "EMAIL_NOT_FOUND" => "No account exists for that email.",
                    "INVALID_PASSWORD" => "Email or password is incorrect.",
                    "USER_DISABLED" => "This account has been disabled.",
                    "TOO_MANY_ATTEMPTS_TRY_LATER" => "Too many attempts. Try again later.",
                    _ when code.StartsWith("WEAK_PASSWORD", StringComparison.OrdinalIgnoreCase)
                        => "Password is too weak. Use at least 6 characters.",
                    _ => "Authentication failed.",
                };
            }
        }
        catch
        {
            // ignored
        }

        return "Authentication failed.";
    }

    private sealed class AuthResponse
    {
        public string? IdToken { get; set; }

        public string? RefreshToken { get; set; }

        public string? Email { get; set; }
    }
}
