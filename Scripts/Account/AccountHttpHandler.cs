using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Secur3D.SDK
{
    static class AccountHttpHandler
    {
        public static async Task<string[]> RequestUserOrganizations(string url, string idToken)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth-token", idToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");

            var response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject parsedResponse = JObject.Parse(responseContent);

            int statusCode = (int)response.StatusCode;
            if (statusCode != 200)
                throw new Exception($"download request failed with status code: {statusCode}");

            return parsedResponse.SelectToken("userGroups").ToObject<string[]>();
        }

        public static async Task<string> RequestOrganizationKey(string url, string idToken, string selectedUser)
        {
            url = $"{url}?selectedUser={selectedUser}";

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth-token", idToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");

            var response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();

            int statusCode = (int)response.StatusCode;
            if (statusCode != 200)
                throw new Exception($"download request failed with status code: {statusCode}\nReason: {responseContent}");

            return responseContent;
        }

        public static async Task<string> UploadAsset(string url, string idToken, MemoryStream stream, string fileName, string assetID, string selectedProfile)
        {
            stream.Position = 0;
            if (!stream.CanRead)
                throw new Exception("Stream cant be read");
            
            HttpClient client = new HttpClient();

            SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(stream);
            string hashBase64 = Convert.ToBase64String(hashBytes);

            stream.Position = 0;
            
            var requestData = new
            {
                object_key = fileName,
                asset_id = assetID,
                shaHash = hashBase64,
                selectedUser = selectedProfile
            };
            
            var response = await client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Headers =
                    {
                        { "auth-token", idToken },
                        { "User-Agent", "Secur3D Unity SDK/0.1"}
                    },
                Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
            });
            UnityEngine.Debug.Log(response);
            UnityEngine.Debug.Log(response.Content.ToString());
            JObject parsedResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            int statusCode = (int)response.StatusCode;
            if (statusCode != 200)
                throw new Exception($"pre-signed url request failed with status code: {statusCode}");

            JObject urlResponse = parsedResponse["url"].Value<JObject>();
            JObject fields = urlResponse["fields"].Value<JObject>();

            string preSignedUrl = urlResponse["url"].Value<string>();
            if (preSignedUrl == null)
                throw new Exception("pre-signed url request failed, is null");

            string assetHashID = parsedResponse["file_hash"].Value<string>();

            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            foreach (var field in fields)
                multipartFormDataContent.Add(new StringContent(field.Value.ToString()), field.Key);
            StreamContent fileContent = new StreamContent(stream);
            multipartFormDataContent.Add(fileContent, "file", fileName);

            HttpResponseMessage uploadResponse = await client.PostAsync(preSignedUrl, multipartFormDataContent);
            HttpStatusCode uploadStatusCode = uploadResponse.StatusCode;
            return uploadStatusCode == HttpStatusCode.NoContent
                ? assetHashID
                : throw new Exception($"Upload failed with status code: {uploadStatusCode}");
        }

        public static async Task<string> UploadComaprison(string hash1, string hash2, string url, string idToken, string selectedProfile)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("auth-token", idToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");

            // Prepare the JSON content from the hashes
            var jsonObject = new { hash1, hash2 };
            var jsonContent = JsonConvert.SerializeObject(jsonObject);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        public static async Task<JObject> RequestFileNames(string url, string idToken, string selectedProfile, int page)
        {
            url = $"{url}?selectedUser={selectedProfile}&page={page}";

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth-token", idToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");

            var response = await client.GetAsync(url);

            string responseContent = await response.Content.ReadAsStringAsync();
            JObject parsedResponse = JObject.Parse(responseContent);

            int statusCode = (int)response.StatusCode;
            if (statusCode != 200)
                throw new Exception($"download request failed with status code: {statusCode}");

            return parsedResponse;
        }

        public static async Task<JObject> RequestFileData(string url, string idToken, string request, string requestType, string selectedProfile)
        {
            url = $"{url}?{requestType}={request}&selectedUser={selectedProfile}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("auth-token", idToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");
            var response = await client.GetAsync(url);

            string responseContent = await response.Content.ReadAsStringAsync();
            JObject parsedResponse = JObject.Parse(responseContent);

            int statusCode = parsedResponse["statusCode"].Value<int>();
            if (statusCode != 200)
                throw new Exception($"download request failed with status code: {statusCode}");

            return parsedResponse;
        }

        public static async Task<JObject> DownloadJson(string url, string idToken, string hash1, string hash2)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("auth-token", idToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Secur3D Unity SDK/0.1");

            var response = await client.GetAsync($"{url}?hash1={hash1}&hash2={hash2}");

            string responseContent = await response.Content.ReadAsStringAsync();
            JObject parsedResponse = JObject.Parse(responseContent);

            int statusCode = parsedResponse["statusCode"].Value<int>();
            if (statusCode != 200)
                throw new Exception($"download request failed with status code: {statusCode}");

            byte[] data = Convert.FromBase64String(parsedResponse["fileContent"].Value<string>());
            string jsonString = Encoding.UTF8.GetString(data);
            JObject jsonDocument = JObject.Parse(jsonString);

            return jsonDocument;
        }

        public static async Task<JObject> PollForJson(string url, string idToken, string hash1, string hash2, int pollingDurationInMilliseconds, int pollingIntervalInMilliseconds)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < pollingDurationInMilliseconds)
            {
                try
                {
                    // Call the DownloadJson function
                    JObject jsonDocument = await DownloadJson(url, idToken, hash1, hash2);

                    if (jsonDocument != null)
                        return jsonDocument;

                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log(ex);
                }

                // Delay before the next iteration
                await Task.Delay(pollingIntervalInMilliseconds);
            }

            return null;
        }
    }
}
