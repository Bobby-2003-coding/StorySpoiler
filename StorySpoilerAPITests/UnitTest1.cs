using System;
using System.Net;
using NUnit.Framework;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;


namespace StorySpoilerAPITests
{

    [TestFixture]
    public class Tests
    {
        private RestClient? client;
        private static string? lastCreatedStoryId;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        private JsonElement DeserializeResponse(RestResponse response)
        {
            if (string.IsNullOrEmpty(response.Content))
            {
                throw new ArgumentException("Response content is null or empty.");
            }
            return JsonSerializer.Deserialize<JsonElement>(response.Content);
        }

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetToken("Tester2", "tester2");

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token),
            };

            client = new RestClient(options);
        }

        private string GetToken(string username, string password)
        {
            var ClientLogin = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            var response = ClientLogin.Execute(request);

            var json = DeserializeResponse(response);
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreateNewStorySpoilerWithAllRequiermentFields()
        {
            var newStory = new
            {
                title = "Test Story",
                description = "This is a test story.",
                url = ""
            };

            var request = new RestRequest($"/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var json = DeserializeResponse(response);
            Assert.That(json.GetProperty("storyId").GetString(), Is.Not.Null.And.Not.Empty);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));
            lastCreatedStoryId = json.GetProperty("storyId").GetString();
        }

        [Test, Order(2)]
        public void EditTheCreatedStorySpoiler()
        {
            var updatedStory = new
            {
                title = "Updated Test Story",
                description = "This is an updated test story.",
                url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = DeserializeResponse(response);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStorySpoilers()
        {
            var request = new RestRequest($"/api/Story/All", Method.Get);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = DeserializeResponse(response);
            Assert.That(json.GetArrayLength(), Is.GreaterThan(0), "The response was null");
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler()
        {
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = DeserializeResponse(response);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStorySpoilerWithoutRequiredFields()
        {
            var story = new
            {
                title = "",
                description = "",
                url = ""
            };

            var request = new RestRequest($"/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistentStorySpoiler()
        {
            var updateStory = new
            {
                title = "Updated Story",
                description = "Upadated",
                url = ""
            };

            var NonExistentStoryId = "123";
            var request = new RestRequest($"/api/Story/Edit/{NonExistentStoryId}", Method.Put);
            request.AddJsonBody(updateStory);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var json = DeserializeResponse(response);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistentStorySpoiler()
        {
            var NonExistentStoryId = "999";
            var request = new RestRequest($"/api/Story/Delete/{NonExistentStoryId}", Method.Delete);
            var response = client!.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
            var json = DeserializeResponse(response);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}