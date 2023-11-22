using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    public class FileRetrievalMenu : SDKMenu
    {
        Vector2 scrollPosition;
        private List<FileData> fileList = new List<FileData>();
        private JObject fileDetails;
        private int page = 1;
        private int maxPage = 1;

        //Comparison variables
        string UID;
        bool compared = false;

        public override void Draw(AccountAPI api, string selectedProfile)
        {
            if (fileList.Count == 0)
                ApiFunction(api, selectedProfile);

            if (compared)
                DrawResults(api);
            else if (fileDetails == null)
                DrawFileList(api, selectedProfile);
            else
                DrawFileDetails(api, selectedProfile);
        }

        public override void ApiFunction(AccountAPI api, string selectedProfile)
        {
            fileList.Clear();
            JObject[] files;
            (files, maxPage) = api.RequestFiles(page, selectedProfile);

            foreach (var file in files)
            {
                FileData data = FileData.CreateFromJSON(file.ToString());
                fileList.Add(data);
            }
        }

        private void DrawFileList(AccountAPI api, string selectedProfile)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginVertical(GUILayout.MinHeight(500f));
            foreach (var file in fileList)
                if (file.DisplayCard())
                    fileDetails = api.GetFileDataFromHash(file.hash, selectedProfile);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                if (page > 1)
                {
                    page--;
                    ApiFunction(api, selectedProfile);
                }
            }
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea($"Page: {page}/{maxPage}");
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(">"))
            {
                if (page < maxPage)
                {
                    page++;
                    ApiFunction(api, selectedProfile);
                }
            }
            if (GUILayout.Button("Refresh"))
            {
                ApiFunction(api, selectedProfile);
            }
            EditorGUILayout.EndHorizontal();

            if (UID != null)
                if (GUILayout.Button("Reset Comparison"))
                    UID = null;
        }

        private void DrawFileDetails(AccountAPI api, string selectedProfile)
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("File Name:", fileDetails["file_name"].Value<string>());
            EditorGUILayout.LabelField("Hash:", fileDetails["file_hash"].Value<string>());
            EditorGUILayout.LabelField("Upload Timestamp:", fileDetails["upload_timestamp"].Value<string>());
            EditorGUILayout.LabelField("File Creator:", fileDetails["creator"].Value<string>());
            EditorGUILayout.LabelField("File Owner:", fileDetails["file_owner"].Value<string>());
            EditorGUILayout.LabelField("Processed:", fileDetails["extract_progress"].Value<bool>().ToString());
            EditorGUILayout.LabelField("Valid Model:", fileDetails["valid"].Value<bool>().ToString());

            GUILayout.BeginHorizontal();
            if (UID == null)
            {
                if (GUILayout.Button("Select For Comparison Or Download"))
                {
                    UID = fileDetails["file_hash"].Value<string>();
                    fileDetails = null;
                }
            }
            else
            {
                if (GUILayout.Button("Start Comparison"))
                {
                    api.StartComparisonRequest(UID, fileDetails["file_hash"].Value<string>(), selectedProfile);
                    compared = true;
                }
                if (GUILayout.Button("Download Comparison"))
                {
                    api.DownloadResults(UID, fileDetails["file_hash"].Value<string>());
                    compared = true;
                }
            }

            if (GUILayout.Button("Back"))
                fileDetails = null;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawResults(AccountAPI api)
        {
            GUILayout.BeginVertical("box");

            GUILayout.TextArea("Result:\n" + api.log, GUILayout.ExpandHeight(true));

            if (GUILayout.Button("Back"))
            {
                compared = false;
                UID = null;
            }

            GUILayout.EndVertical();
        }
    }
}
#endif