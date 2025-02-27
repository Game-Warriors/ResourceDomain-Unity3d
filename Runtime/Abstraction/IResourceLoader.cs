using GameWarriors.ResourceDomain.Data;
using System;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The abstraction which using by Resource system to load require data.
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// Loading Resouce data asynchronously and trigger the callback when its done.
        /// </summary>
        /// <param name="onLoadDone"></param>
        void LoadResourceAsync(Action<ResourceData> onLoadDone);
    }
}