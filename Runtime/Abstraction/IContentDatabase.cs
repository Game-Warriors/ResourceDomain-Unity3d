using System;

namespace GameWarriors.ResourceDomain.Abstraction
{
    public enum EBundleType { Local, Remote };

    /// <summary>
    /// The base abstraction which presents content providing features like, downloading contents from remote, loading local or downloaded contents and unload, remove downloaded contents.
    /// </summary>
    public interface IContentDatabase
    {
        int DownloadingCount { get; }
        event Action<string> OnContentDownloadComplete;
        event Action<string> OnContentDownloadFailed;

        bool IsContentLoaded(string bundleName);
        T GetContent<T>(string contentName) where T : UnityEngine.Object;
        void GetContentAsync<T>(string contentName, Action<T> onLoad) where T : UnityEngine.Object;
        float DownloadProgress(string bundleName);
        void ForceStartDownload(string bundleName, Action<bool> onStart);
        bool IsBundleDownloaded(string bundleName);
        void UnloadContent(string contentName, bool unloadAllObjects);
        void RemoveBundle(string bundleName);
        bool IsBundleExist(string bundleName);
        bool IsBundleLoaded(string bundleName);
        void LoadBundleAsync(string bundleName, Action<string> onLoadDone);
    }
}
