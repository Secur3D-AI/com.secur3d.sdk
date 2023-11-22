using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class GetKeyWindow : SDKMenu
    {
        string apiKey;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            DrawResults(api, selectedProfile);
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            try
            {
                apiKey = api.GetOrganizationKey(selectedProfile);
                JObject json = JObject.Parse(apiKey);
                apiKey = json.GetValue("apiKey").ToString();
            }
            catch (Exception ex)
            {
                apiKey = ex.Message;
            }
        }

        private void DrawResults(AccountAPI api, string selectedUser)
        {
            GUILayout.BeginVertical("box");

            if (GUILayout.Button("Get Key"))
                ApiFunction(api, selectedUser);
            GUILayout.TextArea("Key: " + apiKey);

            GUILayout.EndVertical();
        }
    }
}
#endif