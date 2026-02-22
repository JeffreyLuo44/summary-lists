namespace SummaryLists.Mobile.Services;

public interface IAuthService
{
    Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthSession> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthSession?> GetSessionAsync();

    Task SignOutAsync();
}
