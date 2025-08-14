using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    public class StorySpoilerTests
    {
        private RestClient client;
        private const string BASEURL = "https://d3s5nxhwblsjbi.cloudfront.net";
        private const string USERNAME = "oyep";
        private const string PASSWORD = "123123";

        private static string storyID;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(USERNAME, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            {
                username,
                password
            });

            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type {response.StatusCode} with data {response.Content}");
            }
        }

        [OneTimeTearDown] public void TearDown() { client.Dispose(); }

        [Test, Order(1)]
        public void CreateNewStory_WithCorrectData_ShouldSucceed()
        {

            var newStory = new StoryDTO
            {
                Title = "Story Nameee",
                Description = "Description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(data.Message, Is.EqualTo("Successfully created!"));

            storyID = data.storyId;
            Console.WriteLine(storyID);
            
        }

        [Test, Order(3)]
        public void EditTheCreatedStory_WithCorrectData_ShouldSucceed()
        {
            var editedStory = new StoryDTO
            {
                Title = "Edited Story Nameee",
                Description = "Description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{storyID}", Method.Put);
            request.AddJsonBody(editedStory);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.Message, Is.EqualTo("Successfully edited"));


        }

        [Test, Order(2)]
        public void SearchForTheCreatedStorySpoilerByTitle_ShouldSucceed()
        {
            var request = new RestRequest("/api/Story/Search?keyword=Story Nameee");
            
            var response = client.Execute(request, Method.Get);
            //request.AddQueryParameter("ideaId", lastIdeaId);
            /*request.AddJsonBody(requestData);*/
            var responseDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(responseDataArray.Length, Is.GreaterThan(0));
            Assert.That(response.Content, Does.Contain("Story Nameee"));
        }

        [Test, Order(4)]
        public void DeleteTheEditedStory_ShouldSucceed()
        {
            var request = new RestRequest($"/api/Story/Delete/{storyID}");


            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStoryWithoutRequoredFields_ShouldFail()
        {
            var newStory = new StoryDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditStory_WithWrongId_ShouldFail()
        {
            var editedStory = new StoryDTO
            {
                Title = "Edited Story Nameee",
                Description = "Description",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/999", Method.Put);
            request.AddJsonBody(editedStory);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test, Order(7)]
        public void DeleteStory_WithWrongId_ShouldFail()
        {
            var request = new RestRequest("/api/Story/Delete/555");
            

            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
        }

    }
}