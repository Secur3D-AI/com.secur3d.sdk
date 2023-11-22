namespace Secur3D.SDK
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "OrganizationData", menuName = "Secur3D/OrganizationData", order = 0)]
    public class OrganizationData : SingletonScriptableObject<OrganizationData>
    {
        [SerializeField]
        private string organizationName = "";
        public static string OrganizationName { 
            get { return Instance.organizationName; } 
            set { Instance.organizationName = value; } 
        }

        [SerializeField]
        private string organizationApiKey = "";
        public static string OrganizationApiKey { 
            get { return Instance.organizationApiKey; } 
            set { Instance.organizationApiKey = value; } 
        }
    }
}
