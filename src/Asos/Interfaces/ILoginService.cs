namespace Asos.Interfaces;

public interface ILoginService
{
    Task<string?> LoginAsync(string username, string password);
}