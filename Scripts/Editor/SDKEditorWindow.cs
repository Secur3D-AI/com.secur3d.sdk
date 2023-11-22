using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class SDKEditorWindow : EditorWindow
    {
        static List<string> menuNames = new List<string>()
        {
            "Asset Library",
            "Submit New Asset",
            "API Key",
        };

        static List<SDKMenu> menus = new List<SDKMenu>()
        {
            new FileRetrievalMenu(),
            new UploadMenu(),
            new GetKeyWindow(),
        };

        int selectedMenu = 0;

        //Authentication
        private AccountAPI api;
        private static string Username = "";
        private static string Password = "";

        private string[] profiles = new string[0];
        private int selectedProfile = 0;

        private Texture logo = null;
        private GUIStyle style;

        [MenuItem("Window/Secur3D/Web Portal")]
        static void OpenMyWebsite()
        {
            Application.OpenURL("http://www.app.secur3d.ai/");
        }

        [MenuItem("Window/Secur3D/Content Utility")]
        private static void Init()
        {
            var window = CreateWindow<SDKEditorWindow>();
            window.titleContent = new GUIContent("Secur3D Content Utility");
            window.minSize = new Vector2(1000, 500);
            window.maxSize = new Vector2(2000, 1000);
            window.Show();
        }

        #region UI
        private void OnGUI()
        {
            if (logo == null)
                logo = (Texture)AssetDatabase.LoadAssetAtPath("Packages/com.secur3d.sdk/Images/Logo.png", typeof(Texture));

            float w = Mathf.Min(512, position.width);
            float h = 136 * (w / 512);
            GUI.DrawTexture(new Rect(position.width * 0.5f - w * 0.5f, 30, w, h), logo);
            EditorGUILayout.Space(50 + h);

            style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.Space(10 + position.width * 0.05f);

            EditorGUILayout.BeginVertical(style, GUILayout.MinWidth(200), GUILayout.MaxWidth(300));

            if (api == null)
            {
                DrawLogin();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10 + position.width * 0.05f);
                EditorGUILayout.EndHorizontal();
                return;
            }
            else
            {
                DrawProfileOptions();
                EditorGUILayout.Space(20);
                if (api != null)
                    DrawFunctionOptions();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");

            if (api != null)
                menus[selectedMenu].Draw(api, profiles[selectedProfile]);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10 + position.width * 0.05f);

            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }

        void DrawLogin()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Username: ");
            Username = EditorGUILayout.TextField("", Username);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Password: ");
            Password = EditorGUILayout.PasswordField("", Password);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Login"))
            {
                api = AccountAPI.CreateInstance(Username, Password);

                if (api == null)
                {
                    UnityEngine.Debug.LogError("Secur3D login failed");
                    return;
                }

                if (profiles.Length <= 0)
                {
                    string[] temp = api.GetOrganizations();
                    profiles = new string[temp.Length + 1];
                    profiles[0] = Username;
                    Array.Copy(temp, 0, profiles, 1, temp.Length);
                }
                Password = "";
            }
        }

        void DrawProfileOptions()
        {
            int option = EditorGUILayout.Popup(selectedProfile, profiles);
            if (option != selectedProfile)
            {
                menus[selectedMenu].ApiFunction(api, profiles[option]);
                selectedProfile = option;
            }

            if (GUILayout.Button("Logout"))
            {
                api = null;
                selectedMenu = 0;
                selectedProfile = 0;
                profiles = new string[0];
            }

        }

        void DrawFunctionOptions()
        {
            GUILayout.Label("Functions:");
            for (int i = 0; i < menuNames.Count; i++)
            {
                if (GUILayout.Button(menuNames[i]))
                {
                    selectedMenu = i;
                    menus[i].Draw(api, profiles[selectedProfile]);
                }
            }
        }
        #endregion
    }
}
#endif