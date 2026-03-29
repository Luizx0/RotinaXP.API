using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RotinaXP.API.Tests.Integration;

public class ResponseContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResponseContractTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthRegister_AndLogin_DoNotExposePasswordHash()
    {
        var email = $"contract-auth-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Contract Auth User",
            email,
            password = "12345678"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerJson = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var registerUser = registerJson.GetProperty("user");

        Assert.False(registerUser.TryGetProperty("passwordHash", out _));

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "12345678"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var loginUser = loginJson.GetProperty("user");

        Assert.False(loginUser.TryGetProperty("passwordHash", out _));
    }

    [Fact]
    public async Task UsersEndpoint_DoesNotExposePasswordHash()
    {
        var (userId, token) = await RegisterUserAsync($"contract-users-{Guid.NewGuid():N}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(userJson.TryGetProperty("passwordHash", out _));
    }

    [Fact]
    public async Task TasksResponses_DoNotExposeUserNavigation()
    {
        var (userId, token) = await RegisterUserAsync($"contract-tasks-{Guid.NewGuid():N}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Contract Task",
            isCompleted = false,
            userId
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdTaskJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = createdTaskJson.GetProperty("id").GetInt32();

        Assert.False(createdTaskJson.TryGetProperty("user", out _));

        var getResponse = await _client.GetAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var taskJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(taskJson.TryGetProperty("user", out _));
    }

    [Fact]
    public async Task RewardsAndDailyProgressResponses_DoNotExposeUserNavigation()
    {
        var (userId, token) = await RegisterUserAsync($"contract-rewards-{Guid.NewGuid():N}@example.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRewardResponse = await _client.PostAsJsonAsync("/api/rewards", new
        {
            title = "Contract Reward",
            pointsCost = 5,
            userId
        });

        Assert.Equal(HttpStatusCode.Created, createRewardResponse.StatusCode);

        var rewardJson = await createRewardResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(rewardJson.TryGetProperty("user", out _));

        var createTaskResponse = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Task For Daily Progress",
            isCompleted = false,
            userId
        });

        Assert.Equal(HttpStatusCode.Created, createTaskResponse.StatusCode);

        var taskJson = await createTaskResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = taskJson.GetProperty("id").GetInt32();

        var completeTaskResponse = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            isCompleted = true
        });

        Assert.Equal(HttpStatusCode.OK, completeTaskResponse.StatusCode);

        var progressResponse = await _client.GetAsync($"/api/dailyprogress/user/{userId}");
        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);

        var progressJson = await progressResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(progressJson.ValueKind == JsonValueKind.Array);
        Assert.True(progressJson.GetArrayLength() >= 1);
        Assert.False(progressJson[0].TryGetProperty("user", out _));
    }

    private async Task<(int UserId, string Token)> RegisterUserAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Contract User",
            email,
            password = "12345678"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (
            json.GetProperty("user").GetProperty("id").GetInt32(),
            json.GetProperty("token").GetString() ?? string.Empty);
    }
}
