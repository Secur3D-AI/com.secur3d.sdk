using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Secur3D.SDK
{
    static class OrganizationHttpHandler
    {
        public static async Task<string> UploadAsset(string url, MemoryStream stream, string fileName, string assetID, string apiKey, string orgName)
        {
            url = $"{url}?selectedUser={System.Uri.EscapeDataString(orgName)}";
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
                selectedUser = orgName
            };

            var response = await client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Headers =
                    {
                        { "X-Api-Key", apiKey },
                        { "User-Agent", "Secur3D Unity SDK/0.1"}
                    },
                Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
            });

            JObject parsedResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            int statusCode = (int)response.StatusCode;
            if (statusCode != 200)
                throw new Exception($"pre-signed url request failed with status code: {statusCode}");

            JObject urlResponse = parsedResponse["url"].Value<JObject>();
            JObject fields = urlResponse["fields"].Value<JObject>();

            string preSignedUrl = urlResponse["url"].Value<string>();
            if (preSignedUrl == null)
                throw new Exception("pre-signed url request failed, is null");

            string assetHashID = parsedResponse["hash"].Value<string>();

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
    }
}
