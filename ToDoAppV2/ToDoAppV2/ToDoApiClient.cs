using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
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
            var payload = new Dictionary<string, string>
            {
                ["first_name"] = firstName,
                ["last_name"] = lastName,
                ["email"] = email,
                ["password"] = password,
                ["confirm_password"] = confirmPassword
            };

            using var response = await Http.PostAsync("/signup_action.php", new FormUrlEncodedContent(payload));
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSimpleStatusMessage(body, response.IsSuccessStatusCode, "Unable to create account.");
            return (parsed.Success, NormalizeSignUpMessage(parsed.Message, parsed.Success));
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

            if (!TryParseJson(body, out var doc))
            {
                return (false, ExtractPlainTextMessage(body, "Sign in failed."), null);
            }

            using (doc)
            {
                var root = doc.RootElement;
                var status = ReadInt(root, "status");
                var message = NormalizeSignInMessage(ReadString(root, "message") ?? "Sign in failed.", status == 200);

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

            if (!TryParseJson(body, out var doc))
            {
                return (false, ExtractPlainTextMessage(body, "Unable to fetch tasks."), []);
            }

            using var _ = doc;
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
            var payload = new Dictionary<string, string>
            {
                ["item_name"] = title,
                ["item_description"] = detail,
                ["user_id"] = userId.ToString()
            };

            using var response = await Http.PostAsync("/addItem_action.php", new FormUrlEncodedContent(payload));
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseAddItemResponse(body, "Unable to add task.");
            if (parsed.Success)
            {
                return parsed;
            }

            // Some backend builds expect JSON body for this route and incorrectly return
            // "fill up all fields" for form-encoded payloads. Retry once with JSON.
            if (ContainsAny(parsed.Message, "fill all fields", "fill up all fields", "use up all fields", "complete all fields"))
            {
                var jsonPayload = new
                {
                    item_name = title,
                    item_description = detail,
                    user_id = userId
                };

                using var jsonResponse = await Http.PostAsJsonAsync("/addItem_action.php", jsonPayload);
                var jsonBody = await jsonResponse.Content.ReadAsStringAsync();
                return ParseAddItemResponse(jsonBody, "Unable to add task.");
            }

            return parsed;
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}", null);
        }
    }

    private static (bool Success, string Message, ToDoClass? Item) ParseAddItemResponse(string responseBody, string defaultError)
    {
        if (!TryParseJson(responseBody, out var doc))
        {
            return (false, ExtractPlainTextMessage(responseBody, defaultError), null);
        }

        using var _ = doc;
        var root = doc.RootElement;
        var responseStatus = ReadInt(root, "status");
        var message = NormalizeAddItemMessage(ReadString(root, "message") ?? defaultError, responseStatus == 200);

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

    public static async Task<(bool Success, string Message)> UpdateItemAsync(int itemId, string title, string detail)
    {
        try
        {
            var jsonPayload = new
            {
                item_name = title,
                item_description = detail,
                item_id = itemId
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/editItem_action.php")
            {
                Content = JsonContent.Create(jsonPayload)
            };

            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSimpleStatusMessage(body, response.IsSuccessStatusCode, "Unable to update task.");
            var normalized = (Success: parsed.Success, Message: NormalizeUpdateItemMessage(parsed.Message, parsed.Success));
            if (normalized.Success)
            {
                return normalized;
            }

            // Fallback for backend variants that read only form payloads for PUT.
            if (ShouldRetryPutWithForm(normalized.Message))
            {
                var formPayload = new Dictionary<string, string>
                {
                    ["item_name"] = title,
                    ["item_description"] = detail,
                    ["item_id"] = itemId.ToString()
                };

                using var fallbackRequest = new HttpRequestMessage(HttpMethod.Put, "/editItem_action.php")
                {
                    Content = new FormUrlEncodedContent(formPayload)
                };

                using var fallbackResponse = await Http.SendAsync(fallbackRequest);
                var fallbackBody = await fallbackResponse.Content.ReadAsStringAsync();
                var fallbackParsed = ParseSimpleStatusMessage(fallbackBody, fallbackResponse.IsSuccessStatusCode, "Unable to update task.");
                return (fallbackParsed.Success, NormalizeUpdateItemMessage(fallbackParsed.Message, fallbackParsed.Success));
            }

            return normalized;
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
            var jsonPayload = new
            {
                status,
                item_id = itemId
            };

            using var request = new HttpRequestMessage(HttpMethod.Put, "/statusItem_action.php")
            {
                Content = JsonContent.Create(jsonPayload)
            };

            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            var parsed = ParseSimpleStatusMessage(body, response.IsSuccessStatusCode, "Unable to update task status.");
            var normalized = (Success: parsed.Success, Message: NormalizeStatusMessage(parsed.Message, parsed.Success));
            if (normalized.Success)
            {
                return normalized;
            }

            if (ShouldRetryPutWithForm(normalized.Message))
            {
                var formPayload = new Dictionary<string, string>
                {
                    ["status"] = status,
                    ["item_id"] = itemId.ToString()
                };

                using var fallbackRequest = new HttpRequestMessage(HttpMethod.Put, "/statusItem_action.php")
                {
                    Content = new FormUrlEncodedContent(formPayload)
                };

                using var fallbackResponse = await Http.SendAsync(fallbackRequest);
                var fallbackBody = await fallbackResponse.Content.ReadAsStringAsync();
                var fallbackParsed = ParseSimpleStatusMessage(fallbackBody, fallbackResponse.IsSuccessStatusCode, "Unable to update task status.");
                return (fallbackParsed.Success, NormalizeStatusMessage(fallbackParsed.Message, fallbackParsed.Success));
            }

            return normalized;
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
            var parsed = ParseSimpleStatusMessage(body, response.IsSuccessStatusCode, "Unable to delete task.");
            return (parsed.Success, NormalizeDeleteMessage(parsed.Message, parsed.Success));
        }
        catch (Exception ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
    }

    private static (bool Success, string Message) ParseSimpleStatusMessage(string responseBody, bool httpSuccess, string defaultError)
    {
        if (!TryParseJson(responseBody, out var doc))
        {
            var fallbackMessage = ExtractPlainTextMessage(responseBody, defaultError);
            return (httpSuccess, fallbackMessage);
        }

        using (doc)
        {
            var root = doc.RootElement;
            var status = ReadInt(root, "status");
            var message = CleanMessage(ReadString(root, "message")) ?? defaultError;

            if (status == 200)
            {
                return (true, message);
            }

            if (status != 0)
            {
                return (false, message);
            }

            return (httpSuccess, message);
        }
    }

    private static bool TryParseJson(string value, out JsonDocument doc)
    {
        try
        {
            doc = JsonDocument.Parse(value);
            return true;
        }
        catch
        {
            var extracted = TryExtractJson(value);
            if (string.IsNullOrWhiteSpace(extracted))
            {
                doc = null!;
                return false;
            }

            try
            {
                doc = JsonDocument.Parse(extracted);
                return true;
            }
            catch
            {
                doc = null!;
                return false;
            }
        }
    }

    private static string ExtractPlainTextMessage(string? body, string fallback)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return fallback;
        }

        var condensed = CleanMessage(body) ?? string.Empty;
        if (condensed.Length == 0)
        {
            return fallback;
        }

        return condensed.Length <= 200 ? condensed : condensed[..200];
    }

    private static string? TryExtractJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var start = value.IndexOf('{');
        var end = value.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return value.Substring(start, end - start + 1);
    }

    private static string? CleanMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var noHtml = Regex.Replace(message, "<.*?>", " ", RegexOptions.Singleline);
        var condensed = Regex.Replace(noHtml, "\\s+", " ").Trim();
        return condensed.Length == 0 ? null : condensed;
    }

    private static string NormalizeSignUpMessage(string message, bool success)
    {
        var m = CleanMessage(message) ?? string.Empty;
        if (ContainsAny(m, "email already exists", "already exists"))
        {
            return "Email already exists.";
        }

        // Backend can return this generic text for duplicate email despite all fields being provided.
        if (ContainsAny(m, "fill up all fields", "use up all fields", "complete all fields"))
        {
            return "Email already exists.";
        }

        if (success || ContainsAny(m, "account created"))
        {
            return "Account created successfully.";
        }

        return m.Length == 0 ? "Unable to create account." : m;
    }

    private static string NormalizeSignInMessage(string message, bool success)
    {
        var m = CleanMessage(message) ?? string.Empty;
        if (ContainsAny(m, "does not exists", "does not exist", "no account"))
        {
            return "Account does not exists.";
        }

        if (success)
        {
            return m.Length == 0 ? "Success" : m;
        }

        return m.Length == 0 ? "Sign in failed." : m;
    }

    private static string NormalizeUpdateItemMessage(string message, bool success)
    {
        var m = CleanMessage(message) ?? string.Empty;
        if (success || ContainsAny(m, "item updated"))
        {
            return "Item updated";
        }

        return m.Length == 0 ? "Unable to update task." : m;
    }

    private static string NormalizeAddItemMessage(string message, bool success)
    {
        var m = CleanMessage(message) ?? string.Empty;
        if (success || ContainsAny(m, "item added"))
        {
            return "Item added successfully";
        }

        return m.Length == 0 ? "Unable to add task." : m;
    }

    private static string NormalizeStatusMessage(string message, bool success)
    {
        var m = CleanMessage(message) ?? string.Empty;
        if (ContainsAny(m, "activated"))
        {
            return "To do item activated.";
        }

        if (ContainsAny(m, "to do item done", "done"))
        {
            return "To do item done.";
        }

        if (success)
        {
            return "Status updated.";
        }

        return m.Length == 0 ? "Unable to update task status." : m;
    }

    private static string NormalizeDeleteMessage(string message, bool success)
    {
        var m = CleanMessage(message) ?? string.Empty;
        if (success || ContainsAny(m, "item deleted"))
        {
            return "Item deleted";
        }

        return m.Length == 0 ? "Unable to delete task." : m;
    }

    private static bool ContainsAny(string source, params string[] fragments)
    {
        foreach (var fragment in fragments)
        {
            if (source.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ShouldRetryPutWithForm(string message)
    {
        return ContainsAny(
            message,
            "fill all fields",
            "fill up all fields",
            "use up all fields",
            "complete all fields",
            "id does not exist",
            "invalid id",
            "missing"
        );
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
