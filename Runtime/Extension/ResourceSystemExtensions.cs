using UnityEngine;

namespace ResourceDomain.Extension
{
    public static class ResourceSystemExtensions
    {
        public static T GetOperationConfigData<T>(this AsyncOperation operation) where T : ScriptableObject
        {
            if (operation is ResourceRequest resource)
            {
                if (resource.asset != null)
                    return resource.asset as T;
            }
            if (operation is AssetBundleRequest bundleRequest)
            {
                return bundleRequest.asset as T;
            }
            return null;
        }
    }
}
