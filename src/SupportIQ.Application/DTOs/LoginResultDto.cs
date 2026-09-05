namespace SupportIQ.Application.DTOs;

public record LoginResultDto(string Token, DateTime ExpiresAtUtc, string Email, string Name, string Role);
