using GameWarriors.ResourceDomain.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Core
{

    public class DefaultContentDownloader : IContentDownloder
    {
        private readonly string Save_Download_Path = Application.dataPath;

        private readonly int Parallel_Download_Count = 4;

        private string _serverUrl;
        private Action<string> _onDownloadedComplete = null;
        private Action<string> _onDownloadFailed = null;
        private Dictionary<string, DownloadHandler> _downloadTable;
        private List<string> _downloadList;
        private Stack<DownloadHandler> _handlerPool;
        private bool _isAutoDownload;

        public int DownloadTableCount => _downloadTable.Count;

        public DefaultContentDownloader(int parrallelDownloadCount = 4)
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            Save_Download_Path = Application.persistentDataPath + "/DownloadContent/";
#elif UNITY_IOS
            Save_Download_Path = Application.persistentDataPath + "/DownloadContent/";
#else
            Save_Download_Path = Application.persistentDataPath + "/DownloadContent/";
#endif           
            if (!Directory.Exists(Save_Download_Path))
            {
                Directory.CreateDirectory(Save_Download_Path);
            }
            _downloadTable = new Dictionary<string, DownloadHandler>();
            _handlerPool = new Stack<DownloadHandler>();
            _downloadList = new List<string>();
            Parallel_Download_Count = parrallelDownloadCount;

        }

        public void Initialization(string serverAddress, bool isAutoDownload)
        {
            _serverUrl = serverAddress;
            _isAutoDownload = isAutoDownload;
            if (_isAutoDownload)
            {
                int count = Mathf.Min(Parallel_Download_Count, _downloadList.Count);
                for (int i = 0; i < count; ++i)
                {
                    string bundleName = _downloadList[i];
                    DownloadHandler downloadHandler = PopDownloadHandler();
                    _downloadTable.Add(bundleName, downloadHandler);
                    if (Application.internetReachability != NetworkReachability.NotReachable)
                    {
                        StartDownload(bundleName, downloadHandler, null);
                    }
                }
            }
        }

        public bool RequestContentAsync(string bundleName, uint crc, Action<AssetBundle, string> onDone)
        {
            if (IsBundleComplete(bundleName))
            {
                string path = Save_Download_Path + bundleName + ".octet-stream";
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path, crc);
                request.completed += (input) =>
                {
                    if (request.assetBundle != null)
                    {
                        onDone?.Invoke(request.assetBundle, bundleName);
                    }
                    else
                    {
                        File.Delete(path);
                        PlayerPrefs.DeleteKey(bundleName);
                        _downloadList.Insert(0, bundleName);
                        onDone?.Invoke(null, bundleName);
                    }
                };
                return true;
            }
            else
            {
                _downloadList.Add(bundleName);
            }
            return false;
        }

        private bool IsBundleComplete(string bundleName)
        {
            int state = PlayerPrefs.GetInt(bundleName, 0);
            return state > 0;
        }

        public void RegisterForCallbacks(Action<string> onDownloadedComplete = null, Action<string> onDownloadFailed = null)
        {
            if (onDownloadedComplete != null)
                _onDownloadedComplete += onDownloadedComplete;
            if (onDownloadFailed != null)
                _onDownloadFailed += onDownloadFailed;
        }

        public void RefreshContentsDownloadState()
        {
            if (_downloadTable != null && _downloadList?.Count > 0 && Application.internetReachability != NetworkReachability.NotReachable)
            {
                int checkCount = Parallel_Download_Count;
                int length = _downloadList.Count;
                for (int i = 0; i < length; ++i)
                {
                    string bundleName = _downloadList[i];
                    if (_downloadTable.TryGetValue(bundleName, out var handler))
                    {
                        if (handler.IsStop)
                            StartDownload(bundleName, handler, null);
                        --checkCount;
                    }
                    else if (_isAutoDownload && _downloadTable.Count < Parallel_Download_Count)
                    {
                        DownloadHandler downloadHandler = PopDownloadHandler();
                        _downloadTable.Add(bundleName, downloadHandler);
                        StartDownload(bundleName, downloadHandler, null);
                        --checkCount;
                    }
                    if (checkCount < 1)
                        break;
                }
            }
        }

        public float GetDownloadProgress(string bundleName)
        {
            if (_downloadTable.TryGetValue(bundleName, out var handler))
            {
                if (handler.FinalPos == 0)
                    return 0;
                if (handler.IsComplete)
                    return 1;
                return handler.CurrentPos / (float)handler.FinalPos;
            }
            else
            {
                if (PlayerPrefs.HasKey(bundleName))
                {
                    return PlayerPrefs.GetInt(bundleName, 0);
                }
                return -1;
            }
        }

        public void RemoveContext(string bundleName)
        {
            PlayerPrefs.SetInt(bundleName, 0);
        }

        public bool IsContentExist(string bundleName)
        {
            return IsBundleComplete(bundleName);
        }

        public void StopAndClearAll()
        {
            foreach (var item in _downloadTable.Values)
            {
                item.StopDownload();
            }
            _downloadTable.Clear();
        }

        public void ForceDownload(string bundleName, Action<bool> onStart)
        {
            if (_downloadTable.TryGetValue(bundleName, out var handler))
            {
                if (handler.IsStop)
                    StartDownload(bundleName, handler, onStart);
                return;
            }

            DownloadHandler downloadHandler = null;
            if (_downloadTable.Count > Parallel_Download_Count)
            {
                string key = default;
                DateTime date = DateTime.MaxValue;
                foreach (var item in _downloadTable)
                {
                    if (item.Value.StartDate < date)
                    {
                        key = item.Key;
                        date = item.Value.StartDate;
                    }
                }
                downloadHandler = _downloadTable[key];
                _downloadTable.Remove(key);
                downloadHandler.StopDownload();
            }
            DownloadHandler tmp = PopDownloadHandler();
            _downloadTable.Add(bundleName, tmp);
            StartDownload(bundleName, tmp, onStart);
            if (downloadHandler != null)
                PushDownloadHandler(downloadHandler);
        }

        private async void StartDownload(string fileName, DownloadHandler downloadHandler, Action<bool> onStart)
        {
            string url = _serverUrl + fileName;
            string filePath = Save_Download_Path + fileName;
            downloadHandler.IsAutoReconnect = false;
            await downloadHandler.StarProcess(url, filePath, onStart);
            bool isComplete = downloadHandler.IsComplete;
            //Debug.Log("progress : " + GetDownloadProgress(fileName));
            //Debug.Log(isComplete);
            if (isComplete)
            {
                SetContentAsComplete(fileName, downloadHandler);
                _onDownloadedComplete?.Invoke(fileName);
                CheckForPendingContent();
            }
            else
            {
                _onDownloadFailed?.Invoke(fileName);
            }
        }

        private void SetContentAsComplete(string bundleName, DownloadHandler downloadHandler)
        {
            PlayerPrefs.SetInt(bundleName, 1);
            PlayerPrefs.Save();
            _downloadList.Remove(bundleName);
            _downloadTable.Remove(bundleName);
            PushDownloadHandler(downloadHandler);
        }

        private void CheckForPendingContent()
        {
            int length = _downloadList.Count;
            for (int i = 0; i < length; ++i)
            {
                if (_downloadTable.Count >= Parallel_Download_Count)
                    return;
                string bundleName = _downloadList[i];
                if (_isAutoDownload)
                {
                    if (!_downloadTable.ContainsKey(bundleName))
                    {
                        DownloadHandler downloadHandler = PopDownloadHandler();
                        _downloadTable.Add(bundleName, downloadHandler);
                        StartDownload(bundleName, downloadHandler, null);
                    }
                }
                else
                {
                    //if (_downloadTable.TryGetValue(bundleName,out var downloadHandler))
                    //{
                    //    StartDownload(bundleName, downloadHandler, null);
                    //}
                }
            }
        }

        private DownloadHandler PopDownloadHandler()
        {
            if (_handlerPool.Count > 0)
            {
                return _handlerPool.Pop();
            }
            return new DownloadHandler(2048);
        }

        private void PushDownloadHandler(DownloadHandler downloadHandler)
        {
            downloadHandler.Reset();
            _handlerPool.Push(downloadHandler);
        }
    }
}
