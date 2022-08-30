using GameWarriors.ResourceDomain.Abstraction;
using GameWarriors.ResourceDomain.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Core
{
    public class ResourceSystem : ISpriteDatabase, IVariableDatabase, IContentDatabase
    {
        private readonly IResourceConfig _resourceConfig;
        private readonly IContentDownloder _downloadContent;
        private Dictionary<string, BundleItem> _bundlesTable;
        private Dictionary<string, BundleItem> _assetBundleObjectTable;
        private Dictionary<string, UnityEngine.Object> _objectCollectionTable;
        private Dictionary<string, IConvertible> _variableTable;
        private Dictionary<string, Sprite> _persistSpriteTable;
        private int _counter = 0;
        public event Action<string> OnContentDownloadComplete;
        public event Action<string> OnContentDownloadFailed;

        private bool IsLoad => _counter < 1;
        public int DownloadingCount => _downloadContent?.DownloadTableCount ?? 0;


        [UnityEngine.Scripting.Preserve]
        public ResourceSystem(IContentDownloder downloadContentHandler, IResourceConfig resourceConfig)
        {
            _resourceConfig = resourceConfig;
            _downloadContent = downloadContentHandler;
            LoadAssetBundles();
            LoadResourceData();
        }

        public void DownloadStateCheck()
        {
            _downloadContent?.RefreshContentsDownloadState();
        }

        [UnityEngine.Scripting.Preserve]
        public async Task WaitForLoading()
        {
            while (!IsLoad)
            {
                await Task.Delay(100);
            }
        }

        [UnityEngine.Scripting.Preserve]
        public IEnumerator WaitForLoadingCoroutine()
        {
            yield return new WaitUntil(() => IsLoad);
        }

        Sprite ISpriteDatabase.GetSpriteFromCollection(string key, int index)
        {
            if (_objectCollectionTable.TryGetValue(key, out var collection))
                return ((SpriteCollection)collection)[index];
            else
                Debug.LogError($"{key} , SpriteCollection not found");
            return null;
        }

        int ISpriteDatabase.SpriteCollectionItemCount(string key)
        {
            if (_objectCollectionTable.TryGetValue(key, out var collection))
                return ((SpriteCollection)collection).ItemCount;
            else
                Debug.LogError($"{key} , SpriteCollection not found");
            return -1;
        }

        SpriteCollection ISpriteDatabase.FindSpriteCollection(string key)
        {
            if (_objectCollectionTable.TryGetValue(key, out var collection))
                return ((SpriteCollection)collection);
            return null;
        }

        T IVariableDatabase.GetDataObject<T>(string key)
        {
            if (_objectCollectionTable.TryGetValue(key, out var collection))
                return collection as T;
            else
            {
                Debug.LogError($"{key} , Data Collection not found");
            }
            return null;
        }

        T IVariableDatabase.GetVariable<T>(string variableKey)
        {
            if (_variableTable.TryGetValue(variableKey, out var convertible))
            {
                if (convertible is int)
                {
                    IConvertible tmp = (int)convertible >> _resourceConfig.ShiftCount;
                    return (T)(tmp);
                }
                return (T)convertible;
            }
            return default;
        }

        //public void LoadConfigDataAsync(string assetName, Action<ScriptableObject> onAssetLoad)
        //{
        //    var operation = RequestConfigDataAsync(assetName);
        //    operation.completed += (input) => OnLoadAssetDone(input, onAssetLoad);
        //}

        AsyncOperation IVariableDatabase.RequestConfigDataAsync(string assetName)
        {
            AsyncOperation operation = TryGetAssetFromBundleAsync<ScriptableObject>(assetName);
            if (operation == null)
                return Resources.LoadAsync(assetName);
            else
                return operation;
        }

        T IVariableDatabase.RequestConfigData<T>(string assetName)
        {
            T asset = TryGetAssetFromBundle<T>(assetName);
            if (!asset)
                return Resources.Load<T>(assetName);
            else
                return asset;
        }

        void IVariableDatabase.UnloadConfigData(string assetName)
        {
            bool isUnload = TryUnloadAssetFromBundles(assetName, true);
            if (!isUnload)
            {
                var assetObject = Resources.Load(assetName);
                if (assetObject != null)
                    Resources.UnloadAsset(assetObject);
            }
        }

        bool IContentDatabase.IsContentLoaded(string contentName)
        {
            return IsAssetContentLoaded(contentName) || _objectCollectionTable.ContainsKey(contentName);
        }

        void IContentDatabase.LoadBundleAsync(string bundleName, Action<string> onLoadDone)
        {
            if (_bundlesTable.TryGetValue(bundleName, out var bundleItem))
            {
                if (bundleItem.IsBundleLoad)
                    onLoadDone?.Invoke(bundleName);
                else
                {
                    LoadBundleItem(bundleName, bundleItem, onLoadDone);
                }
            }
            else
                onLoadDone?.Invoke(null);
        }

        T IContentDatabase.GetContent<T>(string contentName)
        {
            T operation = TryGetAssetFromBundle<T>(contentName);
            if (!operation)
                return Resources.Load<T>(contentName);
            else
                return operation;
        }

        void IContentDatabase.GetContentAsync<T>(string contentName, Action<T> onLoad)
        {
            AssetBundleRequest operation = TryGetAssetFromBundleAsync<T>(contentName);
            if (operation == null)
            {
                ResourceRequest tmp = Resources.LoadAsync<T>(contentName);
                tmp.completed += (input) => onLoad?.Invoke(tmp.asset as T);
            }
            else
            {
                operation.completed += (input) => onLoad?.Invoke(operation.asset as T);
            }
        }

        void IContentDatabase.UnloadContent(string contentName, bool unloadAllObjects)
        {
            bool isUnload = TryUnloadAssetFromBundles(contentName, unloadAllObjects);
            if (!isUnload)
            {
                //Resources.un(contentName);
            }
        }

        void IContentDatabase.RemoveBundle(string bundleName)
        {
            _downloadContent?.RemoveContext(bundleName);
            if (_bundlesTable.TryGetValue(bundleName, out var bundleItem))
            {
                bundleItem.Bundle.Unload(true);
                bundleItem.Bundle = null;
            }
        }

        bool IContentDatabase.IsBundleExist(string bundleName)
        {
            return _bundlesTable.ContainsKey(bundleName);
        }

        bool IContentDatabase.IsBundleLoaded(string bundleName)
        {
            if (_bundlesTable.TryGetValue(bundleName, out var bundleItem) && bundleItem.IsBundleLoad)
            {
                return true;
            }
            return false;
        }

        float IContentDatabase.DownloadProgress(string bundleName)
        {
            return _downloadContent?.GetDownloadProgress(bundleName) ?? -1;
        }

        void IContentDatabase.ForceStartDownload(string bundleName, Action<bool> onStart)
        {
            _downloadContent?.ForceDownload(bundleName, onStart);
        }

        bool IContentDatabase.IsBundleDownloaded(string bundleName)
        {
            return _downloadContent?.IsContentExist(bundleName) ?? false;
        }

        private bool IsAssetContentLoaded(string assetName)
        {
            if (_assetBundleObjectTable.TryGetValue(assetName, out var bundleItem) && bundleItem.IsBundleLoad)
            {
                return true;
            }
            return false;
        }

        private void OnPersistDataLoad(AsyncOperation operation)
        {
            ResourceRequest request = operation as ResourceRequest;
            ResourceData data = request.asset as ResourceData;
#if UNITY_EDITOR && DEVELOPMENT
            _downloadContent?.Initialization(data.TestServerAddess, data.IsAutoDonwload);
#else
            _downloadContent?.Initialization(data?.MainServerAddess, data?.IsAutoDonwload ?? false);
#endif
            _persistSpriteTable = new Dictionary<string, Sprite>();
            int length = data?.PersistSprites?.Length ?? 0;
            for (int i = 0; i < length; ++i)
            {
                _persistSpriteTable.Add(data.PersistSprites[i].name, data.PersistSprites[i]);
            }

            length = data?.AssetObjects?.Length ?? 10;
            _objectCollectionTable = new Dictionary<string, UnityEngine.Object>(length);
            _variableTable = new Dictionary<string, IConvertible>(data?.StringVars?.Length ?? 0 + data?.FloatVars?.Length ?? 0 + data?.IntVars?.Length ?? 0);
            for (int i = 0; i < length; ++i)
            {
                _objectCollectionTable.Add(data.AssetObjects[i].name, data.AssetObjects[i]);
            }

            length = data?.StringVars?.Length ?? 10;
            for (int i = 0; i < length; ++i)
            {
                _variableTable.Add(data.StringVars[i].Name, data.StringVars[i].Variable);
            }

            length = data?.FloatVars?.Length ?? 10;
            for (int i = 0; i < length; ++i)
            {
                _variableTable.Add(data.FloatVars[i].Name, data.FloatVars[i].Variable);
            }

            length = data?.IntVars?.Length ?? 0;
            for (int i = 0; i < length; ++i)
            {
                _variableTable.Add(data.IntVars[i].Name, data.IntVars[i].Variable << _resourceConfig.ShiftCount);
            }
            if (data)
                Resources.UnloadAsset(data);
            --_counter;
        }

        private void LoadResourceData()
        {
            ++_counter;
            ResourceRequest request = Resources.LoadAsync<ResourceData>(ResourceData.RESOURCES_PATH);
            request.completed += OnPersistDataLoad;
        }

        private void LoadAssetBundles()
        {
            _downloadContent?.RegisterForCallbacks(DownloadComplete, DownloadStop);

#if UNITY_EDITOR
            string platformName = "android";
            string manifestName = "Android";
#elif UNITY_ANDROID
            string platformName = "android";
            string manifestName = "Android";
#elif UNITY_IOS
            string platformName = "ios";
            string manifestName = "iOS";
#else
            string platformName = "standalone";
            string manifestName = "Standalone";
#endif
            string manifestPath = Application.streamingAssetsPath + $"/{platformName}/{manifestName}";
            if (!File.Exists(manifestPath))
                return;
            AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestPath);
            if (manifestBundle != null)
            {
                AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                string[] assetNames = manifest.GetAllAssetBundles();
                int length = assetNames.Length;
                _bundlesTable = new Dictionary<string, BundleItem>(length);
                _assetBundleObjectTable = new Dictionary<string, BundleItem>(length);
                for (int i = 0; i < length; ++i)
                {
                    string bundleName = assetNames[i];
                    string path = Application.streamingAssetsPath + $"/{platformName}/{bundleName}";
                    if (File.Exists(path))
                    {
                        _bundlesTable.Add(bundleName, new BundleItem(EBundleType.Local, path, 0));
                    }
                    else
                    {
                        uint crc = RetrieveCrcFromManifest(path + ".manifest");
                        _bundlesTable.Add(bundleName, new BundleItem(EBundleType.Remote, null, crc));
                    }
                }

                if (!_resourceConfig.IsPreloadBundles)
                    return;

                foreach (var item in _bundlesTable)
                {
                    LoadBundleItem(item.Key, item.Value);
                }
                PlayerPrefs.Save();
            }
        }



        private uint RetrieveCrcFromManifest(string fullPath)
        {
            if (!File.Exists(fullPath))
                return 0;

            using StreamReader reader = new StreamReader(fullPath);
            reader.ReadLine();
            string text = reader.ReadLine();
            string crcString = text.Substring(5, text.Length - 5);
            return uint.Parse(crcString);
        }

        private void OnRemoteBundleLoadDone(AssetBundle bundle, string bundleName)
        {
            Debug.Log(bundle.name);
            Debug.Log(bundleName);
            if (bundle != null && _bundlesTable.ContainsKey(bundleName))
            {
                BundleItem item = _bundlesTable[bundleName];
                if (item.Bundle != null)
                {
                    OnContentDownloadComplete?.Invoke(bundleName);
                    return;
                }
                string[] names = bundle.GetAllAssetNames();
                int length = names.Length;
                item.Bundle = bundle;
                for (int i = 0; i < length; ++i)
                {
                    string name = names[i];
                    int index = name.LastIndexOf('/');
                    ++index;
                    name = name.Substring(index, name.Length - index);
                    //if (!_assetBundleObjectTable.ContainsKey(name))
                    _assetBundleObjectTable.Add(name, item);
                }
                OnContentDownloadComplete?.Invoke(bundleName);
            }
            else
            {
                (this as IContentDatabase).RemoveBundle(bundleName);
                OnContentDownloadFailed?.Invoke(bundleName);
            }
            --_counter;
        }

        private void AssetLocalBundleLoadDone(AsyncOperation operation)
        {
            AssetBundleCreateRequest bundleOperation = operation as AssetBundleCreateRequest;
            AssetBundle bundle = bundleOperation.assetBundle;

            if (bundle != null && _bundlesTable.ContainsKey(bundle.name))
            {
                string bundleName = bundle.name;
                BundleItem item = _bundlesTable[bundleName];
                string[] names = bundle.GetAllAssetNames();
                int length = names.Length;
                for (int i = 0; i < length; ++i)
                {
                    string name = names[i];
                    int index = name.LastIndexOf('/');
                    ++index;
                    name = name.Substring(index, name.Length - index);
                    //if (!_assetBundleObjectTable.ContainsKey(name))
                    _assetBundleObjectTable.Add(name, item);
                }
            }
            --_counter;
        }

        private void DownloadStop(string bundleName)
        {
            OnContentDownloadFailed?.Invoke(bundleName);
        }

        private void DownloadComplete(string bundleName)
        {
            if (_bundlesTable.TryGetValue(bundleName, out var bundleItem))
                _downloadContent?.RequestContentAsync(bundleName, bundleItem.CRC, OnRemoteBundleLoadDone);
        }

        private T TryGetAssetFromBundle<T>(string assetName) where T : UnityEngine.Object
        {

            if (_assetBundleObjectTable != null && _assetBundleObjectTable.TryGetValue(assetName, out var bundle) && bundle.IsBundleLoad)
            {
                return bundle.Bundle.LoadAsset<T>(assetName);
            }
            return null;
        }

        private AssetBundleRequest TryGetAssetFromBundleAsync<T>(string assetName) where T : UnityEngine.Object
        {
            if (_assetBundleObjectTable == null)
                return null;
            assetName = assetName.ToLower();
            if (_assetBundleObjectTable.TryGetValue(assetName, out var bundle) && bundle != null && bundle.IsBundleLoad)
            {
                return bundle.Bundle.LoadAssetAsync<T>(assetName);
            }
            return null;
        }

        private bool TryUnloadAssetFromBundles(string assetName, bool unloadAllObjects)
        {
            assetName = assetName.ToLower();
            if (_assetBundleObjectTable.TryGetValue(assetName, out var bundleItem) && bundleItem.Bundle != null)
            {
                bundleItem.Bundle.Unload(unloadAllObjects);
                bundleItem.Bundle = null;
                return true;
            }
            return false;
        }

        private void LoadBundleItem(string bundleName, BundleItem bundleItem, Action<string> onDone = null)
        {
            if (bundleItem.BundleType == EBundleType.Local)
            {
                ++_counter;
                AssetBundleCreateRequest loadOperation = AssetBundle.LoadFromFileAsync(bundleItem.Path);
                if (onDone == null)
                {
                    loadOperation.completed += AssetLocalBundleLoadDone;
                }
                else
                {
                    loadOperation.completed += (input) => { AssetLocalBundleLoadDone(input); onDone?.Invoke(bundleName); };
                }

            }
            else
            {
                if (_downloadContent != null)
                {
                    Action<AssetBundle, string> action = OnRemoteBundleLoadDone;
                    if (onDone != null)
                        action += (T1, T2) => onDone?.Invoke(T2);

                    if (_downloadContent.RequestContentAsync(bundleName, bundleItem.CRC, action))
                        ++_counter;
                }
            }
        }

        public Sprite GetPersistSprite(string key)
        {
            if (_persistSpriteTable.TryGetValue(key, out var sprite))
            {
                return sprite;
            }
            return null;
        }



    }
}
