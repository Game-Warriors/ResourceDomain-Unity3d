using GameWarriors.ResourceDomain.Abstraction;
using GameWarriors.ResourceDomain.Data;
using System;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Core
{
    /// <summary>
    /// This class provider resource loading for pool system.
    /// </summary>
    public class DefaultResourceLoader : IResourceLoader
    {
        public void LoadResourceAsync(Action<ResourceData> onLoadDone)
        {
            string assetName = ResourceData.RESOURCES_PATH;
            ResourceRequest operation = Resources.LoadAsync<ResourceData>(assetName);
            operation.completed += (asyncOperation) =>
            {
                ResourceData config = (asyncOperation as ResourceRequest)?.asset as ResourceData;
                onLoadDone(config);
                Resources.UnloadAsset(config);
            };
        }
    }
}