using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Secur3D.SDK
{
    public class SDKRuntimeOrganizationWindow : MonoBehaviour
    {
        OrganizationAPI api;
        public GUISkin skin;

        string orgName = "Organization Name";
        string apiKey = "API Key";

        GameObject uploadObject = null;
        bool uploadDropdown = false;
        IEnumerator UploadRoutineAsync(OrganizationAPI api)
        {
            Task<bool> uploadTask = api.UploadGameObjectAsync(uploadObject);
            yield return new WaitUntil(() => uploadTask.IsCompleted);
            Debug.Log($"Returned: {uploadTask.Result}");
        }
        private void OnGUI()
        {
            SetGUISkin();

            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (OrganizationData.OrganizationName == "")
            {
                orgName = CreateTextField(orgName);
                apiKey = CreateTextField(apiKey);

                if (CreateButton("Save Config"))
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
                if (CreateButton(uploadObject != null ? uploadObject.name : "Select GameObject"))
                {
                    uploadDropdown = !uploadDropdown;
                }

                if (uploadDropdown)
                {
                    MeshRenderer[] meshes = GameObject.FindObjectsOfType<MeshRenderer>();

                    foreach (MeshRenderer mesh in meshes)
                    {
                        if (CreateButton(mesh.name))
                        {
                            uploadObject = mesh.gameObject;
                            uploadDropdown = !uploadDropdown;
                        }
                    }
                }

                if (CreateButton("Upload file") && uploadObject != null)
                {
                    StartCoroutine(UploadRoutineAsync(api));
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.EndArea();
        }

        bool CreateButton(string text)
        {
            return GUILayout.Button(text,
                GUILayout.MinHeight((int)(Screen.height * 0.052)),
                GUILayout.MaxWidth((int)(Screen.width * 0.4166)));
        }

        string CreateTextField(string text)
        {
            return GUILayout.TextField(text,
                GUILayout.MinHeight((int)(Screen.height * 0.052)),
                GUILayout.MaxWidth((int)(Screen.width * 0.4166)));
        }

        void SetGUISkin()
        {
            GUI.skin = skin;
            GUI.skin.textField.fontSize = (int)(Screen.height * 0.026);
            GUI.skin.textArea.fontSize = (int)(Screen.height * 0.026);
            GUI.skin.label.fontSize = (int)(Screen.height * 0.03);
            GUI.skin.box.fontSize = (int)(Screen.height * 0.048);
            GUI.skin.button.fontSize = (int)(Screen.height * 0.03);
        }
    }

}