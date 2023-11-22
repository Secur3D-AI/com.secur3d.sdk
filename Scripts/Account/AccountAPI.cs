using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Secur3D.SDK
{
    public class AccountAPI
    {

        private const string BaseApiUrl = @"https://api.secur3d.ai/";
        private const string ModelFunctionSegment = "model-function/";
        private const string OrganizationsSegment = "organizations/";

        private const string ApiUrlUpload = BaseApiUrl + ModelFunctionSegment + "upload-model";
        private const string ApiUrlReqestFiles = BaseApiUrl + ModelFunctionSegment + "request-all-files";
        private const string ApiUrlDownload = BaseApiUrl + ModelFunctionSegment + "request-results";
        private const string ApiUrlSQS = BaseApiUrl + ModelFunctionSegment + "submit-comparison";
        private const string ApiUrlGetData = BaseApiUrl + ModelFunctionSegment + "request-file-data";

        private const string ApiUrlGetOrgs = BaseApiUrl + OrganizationsSegment + "get-organizations";
        private const string ApiUrlGetOrgKey = BaseApiUrl + OrganizationsSegment + "get-organization-key";


        private string idToken;

        public string log = "";

        public static AccountAPI CreateInstance(string username, string password)
        {
            string idToken = Task.Run(() => AuthorizationHandler.Login(username, password)).Result;
            if (idToken != null)
                return new AccountAPI(idToken);
            else
                return null;
        }

        private AccountAPI(string idToken)
        {
            this.idToken = idToken;
        }

        const string kInvalidChars = @"[:\*\?""<>|#{}^\~\[\]']";

        /// <summary>
        /// Checks to make sure file name wont cause errors
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Bool, true if it contains invalid characters
        /// </returns>
        static bool ContainsInvalidChars(string input)
        {
            return Regex.IsMatch(input, kInvalidChars);
        }
        static string GetSafeString(string input)
        {
            return Regex.Replace(input, kInvalidChars, "");
        }
        /// <summary>
        /// Will request all the organizations the current user is associated with
        /// The results will be avalible in around 2 minutes.
        /// </summary>
        /// <returns>
        /// List of organization names, these can be used for the selected user arguments of other functions
        /// </returns>
        public string[] GetOrganizations()
        {
            try
            {
                string[] organizations = Task.Run(() => AccountHttpHandler.RequestUserOrganizations(ApiUrlGetOrgs, idToken)).Result;
                return organizations;
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Will request the api key of the selected organization
        /// </summary>
        /// <returns>
        /// Will return a api key to be used with the OrganizationAPI
        /// </returns>
        public string GetOrganizationKey(string selectedUser)
        {
            try
            {
                string organizationKey = Task.Run(() => AccountHttpHandler.RequestOrganizationKey(ApiUrlGetOrgKey, idToken, selectedUser)).Result;
                return organizationKey;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Will upload a GameObject to the Secur3D servers as the specified user, this will start the extract process.
        /// The results will be avalible in around 2 minutes.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="selectedProfile"></param>
        public async Task<bool> UploadGameObjectAsync(GameObject gameObject, string selectedProfile)
        {
            // Remove unhandled characters 
            string safeName = GetSafeString(Path.GetFileName(gameObject.name));
                
            MemoryStream stream = new MemoryStream();
            if (!await Converter.ConvertGameObjectToGlbStream(gameObject, stream))
                return false;

            string assetID = safeName;

            string res = await AccountHttpHandler.UploadAsset(ApiUrlUpload, idToken, stream, assetID + ".glb", assetID, selectedProfile);

            log = $"Uploaded File: {res}";
            return true;
        }

        /// <summary>
        /// Will get all the data of all uploaded files
        /// </summary>
        /// <returns>
        /// List of JObjects with information of each file
        /// </returns>
        public (JObject[], int) RequestFiles(int page, string selectedProfile)
        {
            try
            {
                JObject response = Task.Run(() => AccountHttpHandler.RequestFileNames(ApiUrlReqestFiles, idToken, selectedProfile, page)).Result;
                List<JObject> fileNames = JsonConvert.DeserializeObject<List<JObject>>(response["files"].Value<string>());
                int totalPages = response["total_pages"].Value<int>();

                //Used only for testing
                string result = "\nFiles\n";
                foreach (JObject obj in fileNames)
                {
                    result += $"Name: {obj["file_name"].Value<string>()}, Hash: {obj["hash"].Value<string>()}\n";
                }

                log = result;
                return (fileNames.ToArray(), totalPages);
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Will start a comparison of the 2 files, the hashes refer to the model UID's of files you want to compare
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <returns></returns>
        public JObject StartComparisonRequest(string hash1, string hash2, string selectedProfile)
        {
            try
            {
                string result = Task.Run(() => AccountHttpHandler.UploadComaprison(hash1, hash2, ApiUrlSQS, idToken, selectedProfile)).Result;
                log = result;
                return JObject.Parse(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }

        }

        /// <summary>
        /// Will get the results of a comparison between 2 files, the hashes refer to the models UID's
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <returns>
        /// Returns the results in JSON format as a JObject
        /// </returns>
        public JObject DownloadResults(string hash1, string hash2)
        {
            try
            {
                JObject file = Task.Run(() => AccountHttpHandler.DownloadJson(ApiUrlDownload, idToken, hash1, hash2)).Result;
                string outputResults = $"File Contents:\n{file.Root.ToString()}";
                Console.WriteLine(outputResults);
                log = outputResults;

                return file;
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Will poll for the results of a comparison between 2 files, the hashes refer to the models UID's
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <param name="pollingDurationMilliseconds"></param>
        /// <param name="pollingIntervalMilliseconds"></param>
        /// <returns>
        /// Returns the results in JSON format as a JObject
        /// </returns>
        public JObject PollForResults(string hash1, string hash2, int pollingDurationMilliseconds, int pollingIntervalMilliseconds)
        {
            try
            {
                JObject jsonDocument = Task.Run(() => AccountHttpHandler.PollForJson(ApiUrlDownload, idToken,
                    hash1, hash2,
                    pollingDurationMilliseconds, pollingIntervalMilliseconds)).Result;

                string results = "Results Couldn't Be Found";
                if (jsonDocument != null)
                    results = $"File Contents:\n{jsonDocument.Root.ToString()}";

                log = results;
                return jsonDocument;
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets file data from a hash, the hash refers to the model's UID
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>
        /// File data such as name, hash and timestamp as a JSON in a JObject
        /// </returns>
        public JObject GetFileDataFromHash(string hash, string selectedProfile)
        {
            try
            {
                JObject fileData = Task.Run(() => AccountHttpHandler.RequestFileData(ApiUrlGetData, idToken, hash, "hash", selectedProfile)).Result;
                string outputResults = $"File Contents:\n{fileData.Root.ToString()}";
                log = outputResults;
                return fileData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets file data from a file name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        /// File data such as name, hash and timestamp as a JSON in a JObject
        /// </returns>
        public JObject GetFileDataFromName(string name, string selectedProfile)
        {
            try
            {
                JObject fileData = Task.Run(() => AccountHttpHandler.RequestFileData(ApiUrlGetData, idToken, name, "file_name", selectedProfile)).Result;
                string outputResults = $"File Contents:\n{fileData.Root.ToString()}";
                log = outputResults;
                return fileData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Message: {ex.Message}\nSource: {ex.Source}\nStack Trace: {ex.StackTrace}");
            }
        }
        
    }
}