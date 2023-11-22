using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class PollMenu : SDKMenu
    {
        private string UID1 = "UID 1";
        private string UID2 = "UID 2";
        private int pollDurationMilliseconds = 0;
        private int pollIntervalMilliseconds = 0;
        bool polling = false;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            if (!polling)
                DrawComparison(api, selectedProfile);
            else
                DrawResults(api);
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            api.PollForResults(UID1, UID2, pollDurationMilliseconds, pollIntervalMilliseconds);
        }

        private void DrawComparison(AccountAPI api, string selectedProfile)
        {
            UID1 = EditorGUILayout.TextField("UID 1: ", UID1);
            UID2 = EditorGUILayout.TextField("UID 2: ", UID2);

            pollDurationMilliseconds = EditorGUILayout.IntField("Poll Duration In Milliseconds", 1000);
            pollIntervalMilliseconds = EditorGUILayout.IntField("Poll Interval In Milliseconds", 100);

            if (GUILayout.Button("Poll For Results"))
            {
                ApiFunction(api, selectedProfile);
                polling = true;
            }
        }

        private void DrawResults(AccountAPI api)
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Result:", api.log);

            if (GUILayout.Button("Back"))
                polling = false;

            GUILayout.EndVertical();
        }
    }
}
#endif