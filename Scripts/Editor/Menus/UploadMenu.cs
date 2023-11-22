using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class UploadMenu : SDKMenu
    {
        private GameObject file;
        private bool uploaded = false;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            if (!uploaded)
                DrawUpload(api, selectedProfile);
            else
                DrawResults(api);
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            if(file)
                EditorCoroutineUtility.StartCoroutine(UploadRoutineAsync(api, selectedProfile), this);
        }
        IEnumerator UploadRoutineAsync(AccountAPI api, string selectedProfile)
        {
            Task<bool> uploadTask = api.UploadGameObjectAsync(file, selectedProfile);
            yield return new WaitUntil(() => uploadTask.IsCompleted);
            Debug.Log($"Returned: {uploadTask.Result}");
        }
        private void DrawUpload(AccountAPI api, string selectedProfile)
        {
            file = (GameObject)EditorGUILayout.ObjectField("Upload GameObject", file, typeof(GameObject), true);
            if (GUILayout.Button("Upload"))
            {
                ApiFunction(api, selectedProfile);
                uploaded = true;
            }
        }

        private void DrawResults(AccountAPI api)
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Result:", api.log);

            if (GUILayout.Button("Back"))
                uploaded = false;

            GUILayout.EndVertical();
        }
    }
}
#endif