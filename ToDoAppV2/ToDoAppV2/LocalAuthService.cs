using System.Text.Json;
using Microsoft.Maui.Storage;

namespace listView_Corsega;

public static class LocalAuthService
{
    private const string UsersPreferenceKey = "todoapp_users_v1";
    private const string CurrentUserPreferenceKey = "todoapp_current_user_v1";

    private sealed class StoredUser
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public static bool TrySignUp(string username, string email, string password, out string message)
    {
        var normalizedEmail = NormalizeEmail(email);
        var users = LoadUsers();

        if (users.Any(u => string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
        {
            message = "An account with this email already exists.";
            return false;
        }

        users.Add(new StoredUser
        {
            Username = username.Trim(),
            Email = normalizedEmail,
            Password = password
        });

        SaveUsers(users);
        message = "Your account has been created.";
        return true;
    }

    public static bool TrySignIn(string email, string password, out string message)
    {
        var normalizedEmail = NormalizeEmail(email);
        var users = LoadUsers();

        var user = users.FirstOrDefault(u => string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            message = "No account found for this email. Please sign up first.";
            return false;
        }

        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            message = "Incorrect password.";
            return false;
        }

        message = $"Welcome back, {user.Username}!";
        return true;
    }

    public static string? CurrentUserEmail
    {
        get
        {
            var email = Preferences.Default.Get(CurrentUserPreferenceKey, string.Empty);
            return string.IsNullOrWhiteSpace(email) ? null : email;
        }
    }

    public static void SetCurrentUser(string email)
    {
        Preferences.Default.Set(CurrentUserPreferenceKey, NormalizeEmail(email));
    }

    public static void ClearCurrentUser()
    {
        Preferences.Default.Remove(CurrentUserPreferenceKey);
    }

    private static List<StoredUser> LoadUsers()
    {
        var json = Preferences.Default.Get(UsersPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<StoredUser>>(json) ?? [];
        }
        catch
        {
            // If storage gets corrupted, recover with an empty in-app user list.
            return [];
        }
    }

    private static void SaveUsers(List<StoredUser> users)
    {
        var json = JsonSerializer.Serialize(users);
        Preferences.Default.Set(UsersPreferenceKey, json);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
