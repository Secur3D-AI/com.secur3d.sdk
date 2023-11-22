using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class ComparisonMenu : SDKMenu
    {
        private string UID1 = "UID 1";
        private string UID2 = "UID 2";
        bool compared = false;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            if (!compared)
                DrawComparison(api, selectedProfile);
            else
                DrawResults(api);
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            api.StartComparisonRequest(UID1, UID2, selectedProfile);
        }

        private void DrawComparison(AccountAPI api, string selectedProfile)
        {
            UID1 = EditorGUILayout.TextField("UID 1: ", UID1);
            UID2 = EditorGUILayout.TextField("UID 2: ", UID2);

            if (GUILayout.Button("Start Comparison"))
            {
                ApiFunction(api, selectedProfile);
                compared = true;
            }
        }

        private void DrawResults(AccountAPI api)
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Result:", api.log);

            if (GUILayout.Button("Back"))
                compared = false;

            GUILayout.EndVertical();
        }
    }
}
#endif