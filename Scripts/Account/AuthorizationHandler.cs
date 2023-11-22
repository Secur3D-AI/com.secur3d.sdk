using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Secur3D.SDK
{

    static class AuthorizationHandler
    {
        public static async Task<string> Login(string username, string password)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");

            var jsonObject = new { 
                username = username, 
                password = password 
            };

            var jsonContent = JsonConvert.SerializeObject(jsonObject);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.secur3d.ai/authentication/login", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject authenticationResult = JObject.Parse(responseContent);
            
            if (authenticationResult.TryGetValue("idToken", out JToken idTokenToken))
            {
                string idToken = idTokenToken.Value<string>();
                return idToken;
            }
            
            return null;
        }
    }
}
