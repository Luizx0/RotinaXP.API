using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RotinaXP.API.Infrastructure.Clients;

namespace RotinaXP.API.Tests.Integration
{
    public class IbgeIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public IbgeIntegrationTests(CustomWebApplicationFactory factory)
        {
            // Create a factory and override the IBGE HttpClient to return a canned response
            var webFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IbgeClient));

                    // Register a typed HttpClient for IbgeClient that uses a fake handler
                    services.AddHttpClient<IbgeClient>(client =>
                    {
                        client.BaseAddress = new Uri("https://servicodados.ibge.gov.br");
                    }).ConfigurePrimaryHttpMessageHandler(() => new TestHandlers.StaticResponseHandler("[ { \"id\":11, \"sigla\":\"RO\", \"nome\":\"Rondônia\" } ]"));
                });
            });

            _client = webFactory.CreateClient();
        }

        [Fact]
        public async Task GetEstados_Returns_ListOfStates()
        {
            // Arrange: create an admin user and token via auth register/login flow
            var email = $"ibge-test-{Guid.NewGuid():N}@example.com";

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                name = "IBGE Test",
                email,
                password = "12345678"
            });

            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

            var json = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = json.GetProperty("token").GetString() ?? string.Empty;

            // For test simplicity, we assume a test helper to elevate role is not present.
            // Instead, set header X-User-Role=Admin which should be allowed in Development/Testing if implemented.
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _client.DefaultRequestHeaders.Add("X-User-Role", "Admin");

            // Act
            var response = await _client.GetAsync("/admin/ibge/estados");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(body.ValueKind == JsonValueKind.Array);
            Assert.True(body.GetArrayLength() >= 1);
        }
    }

    namespace TestHandlers
    {
        using System.Net;
        using System.Net.Http;
        using System.Threading;
        using System.Threading.Tasks;

        public class StaticResponseHandler : HttpMessageHandler
        {
            private readonly string _json;

            public StaticResponseHandler(string json)
            {
                _json = json;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var msg = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_json)
                };

                msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                return Task.FromResult(msg);
            }
        }
    }
}
