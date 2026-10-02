using LMS.Blazor.Services.Authentication.Tokens.Helpers;
using LMS.Shared.DTOs.AuthDtos;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LMS.Blazor.Services.Authentication.Tokens;

// Simple file-based token storage for teaching/demo purposes.
// Not recommended for production because tokens are stored unencrypted on disk.
public sealed class TokenStorageService : ITokenStorage, IDisposable
{
    private readonly ConcurrentDictionary<string, TokenSession> _tokenStore = new();
    private readonly ILogger<TokenStorageService> _logger;
    private readonly string _storageFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public TokenStorageService(ILogger<TokenStorageService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _storageFilePath = Path.Combine(environment.ContentRootPath, "App_Data", "tokens.json");

        Directory.CreateDirectory(Path.GetDirectoryName(_storageFilePath)!);
        LoadTokensFromFile();
    }

    private void LoadTokensFromFile()
    {
        try
        {
            if (!File.Exists(_storageFilePath))
            {
                return;
            }

            var json = File.ReadAllText(_storageFilePath);
            var sessions = JsonSerializer.Deserialize<Dictionary<string, TokenSession>>(json);

            if (sessions is null)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            foreach (var session in sessions.Where(entry => IsValid(entry.Value, now)))
            {
                _tokenStore[session.Key] = session.Value;
            }

            if (_tokenStore.Count != sessions.Count)
            {
                // Discard expired sessions and entries from older storage formats.
                var jsonWithoutInvalidSessions = JsonSerializer.Serialize(
                    _tokenStore.ToDictionary(entry => entry.Key, entry => entry.Value),
                    _jsonOptions);
                File.WriteAllText(_storageFilePath, jsonWithoutInvalidSessions);
            }

            _logger.LogInformation("Loaded {Count} token sessions from storage", _tokenStore.Count);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to load token sessions. Existing cookies will require a new login.");
        }
    }

    private static bool IsValid(TokenSession? session, DateTimeOffset now) =>
        session is not null &&
        !string.IsNullOrWhiteSpace(session.UserId) &&
        session.Tokens is not null &&
        !string.IsNullOrWhiteSpace(session.Tokens.AccessToken) &&
        !string.IsNullOrWhiteSpace(session.Tokens.RefreshToken) &&
        session.Tokens.RefreshTokenExpiresAtUtc > now;

    public async Task StoreTokensAsync(string sessionId, string userId, TokenDto tokens)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(tokens);

        // The demo API stores one refresh token on the Employee record. A new
        // login therefore invalidates all older sessions for the same user.
        var previousSessionIds = _tokenStore
            .Where(entry =>
                entry.Key != sessionId &&
                string.Equals(entry.Value.UserId, userId, StringComparison.Ordinal))
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var previousSessionId in previousSessionIds)
        {
            _tokenStore.TryRemove(previousSessionId, out _);
        }

        _tokenStore[sessionId] = new TokenSession(userId, tokens);
        await SaveTokensToFileAsync();
    }

    public Task<TokenSession?> GetSessionAsync(string sessionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);

        _tokenStore.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public async Task RemoveTokensAsync(string sessionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);

        if (_tokenStore.TryRemove(sessionId, out _))
        {
            await SaveTokensToFileAsync();
        }
    }

    private async Task SaveTokensToFileAsync()
    {
        await _fileLock.WaitAsync();

        try
        {
            var snapshot = _tokenStore.ToDictionary(entry => entry.Key, entry => entry.Value);
            var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
            await File.WriteAllTextAsync(_storageFilePath, json);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save token sessions to file");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void Dispose()
    {
        _fileLock.Dispose();
    }
}
