using System.Net.Http.Json;
using System.Text.Json;
using ToDoMaui_Listview;

namespace listView_Corsega;

public sealed class AuthUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public static class ToDoApiClient
{
    private const string RootUrl = "https://todo-list.dcism.org";
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri(RootUrl)
    };

    public static async Task<(bool Success, string Message)> SignUpAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword)
    {
        try
        {
            var payload = new
            {
                first_name = firstName,
                last_name = lastName,
                email,
                password,
                confirm_password = confirmPassword
            };

            using var response = await Http.PostAsJsonAsync("/signup_action.php", payload);
            var body = await response.Content.ReadAsStringAsync();
            return ParseSimpleStatusMessage(body, "Unable to create account.");
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
    }

    public static async Task<(bool Success, string Message, AuthUser? User)> SignInAsync(string email, string password)
    {
        try
        {
            var route = $"/signin_action.php?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
            using var response = await Http.GetAsync(route);
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var status = ReadInt(root, "status");
            var message = ReadString(root, "message") ?? "Sign in failed.";

            if (status != 200)
            {
                return (false, message, null);
            }

            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            {
                return (false, "Sign in response is missing user data.", null);
            }

            var user = new AuthUser
            {
                Id = ReadInt(data, "id"),
                FirstName = ReadString(data, "fname") ?? string.Empty,
                LastName = ReadString(data, "lname") ?? string.Empty,
                Email = ReadString(data, "email") ?? email.Trim().ToLowerInvariant()
            };

            if (user.Id <= 0)
            {
                return (false, "Sign in response returned an invalid user id.", null);
            }

            return (true, message, user);
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}", null);
        }
    }

    public static async Task<(bool Success, string Message, List<ToDoClass> Items)> GetItemsAsync(int userId, string status)
    {
        try
        {
            var route = $"/getItems_action.php?status={Uri.EscapeDataString(status)}&user_id={userId}";
            using var response = await Http.GetAsync(route);
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var responseStatus = ReadInt(root, "status");
            var message = ReadString(root, "message") ?? "Unable to fetch tasks.";

            if (responseStatus != 200)
            {
                return (false, message, []);
            }

            var items = new List<ToDoClass>();
            if (!root.TryGetProperty("data", out var data))
            {
                return (true, "Success", items);
            }

            if (data.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in data.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var item = ParseTask(property.Value);
                    if (item is not null)
                    {
                        items.Add(item);
                    }
                }
            }
            else if (data.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in data.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var item = ParseTask(element);
                    if (item is not null)
                    {
                        items.Add(item);
                    }
                }
            }

            return (true, message, items);
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}", []);
        }
    }

    public static async Task<(bool Success, string Message, ToDoClass? Item)> AddItemAsync(string title, string detail, int userId)
    {
        try
        {
            var payload = new
            {
                item_name = title,
                item_description = detail,
                user_id = userId
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/addItem_action.php")
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var responseStatus = ReadInt(root, "status");
            var message = ReadString(root, "message") ?? "Unable to add task.";

            if (responseStatus != 200)
            {
                return (false, message, null);
            }

            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            {
                return (true, message, null);
            }

            return (true, message, ParseTask(data));
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}", null);
        }
    }

    public static async Task<(bool Success, string Message)> UpdateItemAsync(int itemId, string title, string detail)
    {
        try
        {
            var payload = new
            {
                item_name = title,
                item_description = detail,
                item_id = itemId
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/editItem_action.php")
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return ParseSimpleStatusMessage(body, "Unable to update task.");
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
    }

    public static async Task<(bool Success, string Message)> UpdateItemStatusAsync(int itemId, string status)
    {
        try
        {
            var payload = new
            {
                status,
                item_id = itemId
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/statusItem_action.php")
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return ParseSimpleStatusMessage(body, "Unable to update task status.");
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
    }

    public static async Task<(bool Success, string Message)> DeleteItemAsync(int itemId)
    {
        try
        {
            var route = $"/deleteItem_action.php?item_id={itemId}";
            using var response = await Http.DeleteAsync(route);
            var body = await response.Content.ReadAsStringAsync();
            return ParseSimpleStatusMessage(body, "Unable to delete task.");
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
    }

    private static (bool Success, string Message) ParseSimpleStatusMessage(string json, string defaultError)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var status = ReadInt(root, "status");
            var message = ReadString(root, "message") ?? defaultError;
            return status == 200 ? (true, message) : (false, message);
        }
        catch
        {
            return (false, defaultError);
        }
    }

    private static ToDoClass? ParseTask(JsonElement obj)
    {
        var id = ReadInt(obj, "item_id");
        if (id <= 0)
        {
            return null;
        }

        return new ToDoClass
        {
            id = id,
            title = ReadString(obj, "item_name") ?? string.Empty,
            detail = ReadString(obj, "item_description") ?? string.Empty
        };
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var asInt))
        {
            return asInt;
        }

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }
}
