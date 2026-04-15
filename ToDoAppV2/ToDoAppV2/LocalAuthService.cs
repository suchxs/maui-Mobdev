using System.Text.Json;
using Microsoft.Maui.Storage;

namespace listView_Corsega;

public static class LocalAuthService
{
    private const string CurrentUserPreferenceKey = "todoapp_current_user_v2";

    public static AuthUser? CurrentUser
    {
        get
        {
            var json = Preferences.Default.Get(CurrentUserPreferenceKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<AuthUser>(json);
            }
            catch
            {
                return null;
            }
        }
    }

    public static void SetCurrentUser(AuthUser user)
    {
        var normalizedUser = new AuthUser
        {
            Id = user.Id,
            FirstName = user.FirstName.Trim(),
            LastName = user.LastName.Trim(),
            Email = user.Email.Trim().ToLowerInvariant()
        };

        var json = JsonSerializer.Serialize(normalizedUser);
        Preferences.Default.Set(CurrentUserPreferenceKey, json);
    }

    public static void ClearCurrentUser()
    {
        Preferences.Default.Remove(CurrentUserPreferenceKey);
    }
}
