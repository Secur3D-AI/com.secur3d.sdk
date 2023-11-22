using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class RequestFileData : SDKMenu
    {
        string[] searchOptions = new string[] { "Hash", "Name" };
        int selectedOption = 0;
        string fileIdentifier = "File";
        JObject foundFile;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            if (foundFile == null)
            {
                selectedOption = EditorGUILayout.Popup(selectedOption, searchOptions);
                DrawGetDetails(api, selectedProfile);
            }
            else
            {
                DrawResults(api);
            }
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            if (searchOptions[selectedOption] == "Hash")
                foundFile = api.GetFileDataFromHash(fileIdentifier, selectedProfile);
            else if (searchOptions[selectedOption] == "Name")
                foundFile = api.GetFileDataFromName(fileIdentifier, selectedProfile);
        }

        private void DrawGetDetails(AccountAPI api, string selectedProfile)
        {
            fileIdentifier = EditorGUILayout.TextField("File: ", fileIdentifier);

            if (GUILayout.Button("Get Details"))
            {
                ApiFunction(api, selectedProfile);
            }
        }

        private void DrawResults(AccountAPI api)
        {
            FileData file = FileData.CreateFromJSON(foundFile.ToString());
            file.DisplayCard();

            if (GUILayout.Button("Back"))
                foundFile = null;
        }
    }
}
#endif