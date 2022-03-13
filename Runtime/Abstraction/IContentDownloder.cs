using System;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    public interface IContentDownloder
    {
        int DownloadTableCount { get; }

        void RefreshContentsDownloadState();
        float GetDownloadProgress(string bundleName);
        void ForceDownload(string bundleName, Action<bool> onStart);
        bool IsContentExist(string bundleName);
        void Initialization(string mainServerAddress, bool isAutoDownload);
        void RegisterForCallbacks(Action<string> downloadComplete, Action<string> downloadStop);
        bool RequestContentAsync(string bundleName, uint crc, Action<AssetBundle, string> onRemoteBundleLoadDone);
        void RemoveContext(string bundleName);
    }
}