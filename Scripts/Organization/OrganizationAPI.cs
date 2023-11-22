using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Secur3D.SDK
{

    public class OrganizationAPI
    {
        private const string ApiUrlUpload = @"https://api.secur3d.ai/organizations/upload-model";

        string apiKey = "";
        string orgName = "";

        public string log = "";

        public static OrganizationAPI CreateInstance(string orgName, string apiKey)
        {
            return new OrganizationAPI(apiKey, orgName);
        }

        private OrganizationAPI(string apiKey, string orgName)
        {
            this.apiKey = apiKey;
            this.orgName = orgName;
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
        /// Will upload a GameObject to the Secur3D servers as the specified user, this will start the extract process.
        /// The results will be avalible in around 2 minutes.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="selectedProfile"></param>
        /// <returns>
        /// Uploaded files hash, the hash is used as a UID for all other requests involving that file
        /// </returns>
        public async Task<bool> UploadGameObjectAsync(GameObject gameObject)
        {
            // Remove unhandled characters 
            string safeName = GetSafeString(Path.GetFileName(gameObject.name));

            MemoryStream stream = new MemoryStream();
            if (!await Converter.ConvertGameObjectToGlbStream(gameObject, stream))
                return false;

            string assetID = safeName;

            string res = await OrganizationHttpHandler.UploadAsset(ApiUrlUpload, stream, $"{assetID}.glb", assetID, apiKey, orgName);

            log = $"Uploaded File: {res}";
            return true;
        }
    }
}
