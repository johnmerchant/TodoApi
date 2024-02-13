using System.Net;
using System.Net.Http.Headers;

namespace Todo.Web.Client.Server;

public class TodoClient
{
    private readonly HttpClient _client;
    
    public TodoClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<TodoItem?> AddTodoAsync(string? title, string? token = default)
    {
        if (string.IsNullOrEmpty(title))
        {
            return null;
        }
        
        SetAuthHeader(token);

        TodoItem? createdTodo = null;

        var response = await _client.PostAsJsonAsync("todos", new TodoItem { Title = title });

        if (response.IsSuccessStatusCode)
        {
            createdTodo = await response.Content.ReadFromJsonAsync<TodoItem>();
        }

        return createdTodo;
    }

    public async Task UpdateTodoAsync(TodoItem item, string? token = default)
    {
        SetAuthHeader(token);

        await _client.PutAsJsonAsync($"todos/{item.Id}", item);
    }

    public async Task<bool> DeleteTodoAsync(int id, string? token = default)
    {
        SetAuthHeader(token);
        
        var response = await _client.DeleteAsync($"todos/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(HttpStatusCode, List<TodoItem>?)> GetTodosAsync(string? token = default)
    {
        SetAuthHeader(token);
        
        var response = await _client.GetAsync("todos");
        var statusCode = response.StatusCode;
        List<TodoItem>? todos = null;

        if (response.IsSuccessStatusCode)
        {
            todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        }

        return (statusCode, todos);
    }

    public async Task<bool> LogoutAsync(string? token = default)
    {
        SetAuthHeader(token);
        
        var response = await _client.PostAsync("auth/logout", content: null);
        return response.IsSuccessStatusCode;
    }
    
    private void SetAuthHeader(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

}