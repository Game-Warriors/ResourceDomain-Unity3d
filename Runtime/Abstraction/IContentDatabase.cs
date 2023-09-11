using System;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// the type of asset bundle origin
    /// </summary>
    public enum EBundleType { Local, Remote };

    /// <summary>
    /// The base abstraction which presents content providing features like, downloading contents from remote, loading local or downloaded contents and unload, remove downloaded contents.
    /// </summary>
    public interface IContentDatabase
    {
        /// <summary>
        /// The count of current in progress download in download manager.
        /// </summary>
        int DownloadingCount { get; }
        event Action<string> OnContentDownloadComplete;
        event Action<string> OnContentDownloadFailed;

        bool IsContentLoaded(string bundleName);
        /// <summary>
        /// This method retrieves unity object in content container. Start searching for target object in exists assetbundles, if item doesn’t find then start searching in object which is in unity resources folder. the operation may be IO bound.
        /// </summary>
        /// <typeparam name="T">main type of unity object</typeparam>
        /// <param name="contentName">the name of object asset</param>
        /// <returns>the loaded object</returns>
        T GetContent<T>(string contentName) where T : UnityEngine.Object;
        /// <summary>
        /// This method asynchronously retrieves unity object in content container. Start searching for target object in exists assetbundles, if item doesn’t find then start searching in object which is in unity resources folder. the operation may be IO bound.
        /// </summary>
        /// <typeparam name="T">main type of unity object</typeparam>
        /// <param name="contentName">the name of object asset</param>
        /// <param name="onLoad">the call back will trigger when object loaded</param>
        void GetContentAsync<T>(string contentName, Action<T> onLoad) where T : UnityEngine.Object;
        float DownloadProgress(string bundleName);
        void ForceStartDownload(string bundleName, Action<bool> onStart);
        bool IsBundleDownloaded(string bundleName);
        /// <summary>
        /// This method will unallocated memory usage for specific content.
        /// </summary>
        /// <param name="contentName">the name of asset bundle or the name of the asset which exist in unity resources folder</param>
        /// <param name="unloadAllObjects">if the target content be as asset bundle this flag indicating to unload all related asset to assetbundle</param>
        void UnloadContent(string contentName, bool unloadAllObjects);
        void RemoveBundle(string bundleName);
        bool IsBundleExist(string bundleName);
        bool IsBundleLoaded(string bundleName);
        void LoadBundleAsync(string bundleName, Action<string> onLoadDone);
    }
}
