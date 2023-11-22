using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

#if UNITY_EDITOR
namespace Secur3D.SDK
{
    public class OrganizationUI : EditorWindow
    {
        OrganizationAPI api;

        string orgName = "";
        string apiKey = "";

        GameObject file;

        [MenuItem("Window/Secur3D/Organization Interface")]
        private static void Init()
        {
            var window = CreateWindow<OrganizationUI>();
            window.titleContent = new GUIContent("SDK Organization Interface");
            window.Show();
        }

        IEnumerator UploadRoutineAsync(OrganizationAPI api)
        {
            Task<bool> uploadTask = api.UploadGameObjectAsync(file);
            yield return new WaitUntil(() => uploadTask.IsCompleted);
            Debug.Log($"Returned: {uploadTask.Result}");
        }
        private void OnGUI()
        {
            if (OrganizationData.OrganizationName == "")
            {
                orgName = EditorGUILayout.TextField("Organization Name: ", orgName);
                apiKey = EditorGUILayout.TextField("Api Key: ", apiKey);

                if (GUILayout.Button("Save Config"))
                {
                    OrganizationData.OrganizationName = orgName;
                    orgName = "";
                    OrganizationData.OrganizationApiKey = apiKey;
                    apiKey = "";
                }
            }
            else
            {
                if (api == null)
                    api = OrganizationAPI.CreateInstance(OrganizationData.OrganizationName, OrganizationData.OrganizationApiKey);
                file = (GameObject)EditorGUILayout.ObjectField("Upload GameObject", file, typeof(GameObject), true);
                if (GUILayout.Button("Upload file") && file != null)
                {
                    EditorCoroutineUtility.StartCoroutine(UploadRoutineAsync(api), this);
                }
            }
        }
    }
}
#endif