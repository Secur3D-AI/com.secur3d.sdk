using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class DownloadResultWindow : SDKMenu
    {
        private string UID1 = "UID 1";
        private string UID2 = "UID 2";
        bool downloaded = false;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            if (!downloaded)
                DrawDownload(api, selectedProfile);
            else
                DrawResults(api);
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            api.DownloadResults(UID1, UID2);
        }

        private void DrawDownload(AccountAPI api, string selectedProfile)
        {
            UID1 = EditorGUILayout.TextField("UID 1: ", UID1);
            UID2 = EditorGUILayout.TextField("UID 2: ", UID2);

            if (GUILayout.Button("Download"))
            {
                ApiFunction(api, selectedProfile);
                downloaded = true;
            }
        }

        private void DrawResults(AccountAPI api)
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Result:", api.log);

            if (GUILayout.Button("Back"))
                downloaded = false;

            GUILayout.EndVertical();
        }
    }
}
#endif