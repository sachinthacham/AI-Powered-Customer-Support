namespace SupportIQ.Application.Abstractions;

/// <summary>Hashes and verifies agent passwords. Implemented with BCrypt in Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
