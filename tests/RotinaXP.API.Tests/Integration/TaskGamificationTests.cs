using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RotinaXP.API.Tests.Integration;

public class TaskGamificationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TaskGamificationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompletingTask_AwardsPoints_AndCreatesDailyProgress()
    {
        var registerPayload = new
        {
            name = "Test User",
            email = "test-user@example.com",
            password = "12345678"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerJson = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = registerJson.GetProperty("user").GetProperty("id").GetInt32();

        var createTaskPayload = new
        {
            title = "Task 1",
            isCompleted = false,
            userId
        };

        var createTaskResponse = await _client.PostAsJsonAsync("/api/tasks", createTaskPayload);
        Assert.Equal(HttpStatusCode.Created, createTaskResponse.StatusCode);

        var createTaskJson = await createTaskResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = createTaskJson.GetProperty("id").GetInt32();

        var updatePayload = new
        {
            isCompleted = true
        };

        var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userResponse = await _client.GetAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
        var userJson = await userResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(10, userJson.GetProperty("points").GetInt32());

        var progressResponse = await _client.GetAsync($"/api/dailyprogress/user/{userId}");
        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);
        var progressJson = await progressResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(progressJson.ValueKind == JsonValueKind.Array);
        Assert.True(progressJson.GetArrayLength() >= 1);
        Assert.Equal(1, progressJson[0].GetProperty("completedTasksCount").GetInt32());
    }
}
