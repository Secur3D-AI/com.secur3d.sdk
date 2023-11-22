using UnityEngine;

#if UNITY_EDITOR
namespace Secur3D.SDK.Editor
{
    [System.Serializable]
    public class FileData
    {
        public string file_name;
        public string hash;
        public ulong upload_timestamp;

        public static FileData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<FileData>(jsonString);
        }

        public bool DisplayCard()
        {
            bool returnedBool = false;

            if (GUILayout.Button($"{file_name} - {hash}"))
                returnedBool = true;

            return returnedBool;
        }
    }
}
#endif