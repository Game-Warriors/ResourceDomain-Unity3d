using System.Collections.Generic;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstraction for sprite data collection
    /// </summary>
    public interface ISpriteCollection
    {
        /// <summary>
        /// The sprite count which is exist in sprite collection.
        /// </summary>
        int ItemCount { get; }
        /// <summary>
        /// Iterate on exist sprite in sprite collection.
        /// </summary>
        IEnumerable<Sprite> GetSprites { get; }
        /// <summary>
        /// Find the target sprite by name.
        /// </summary>
        /// <param name="name">name od sprite asset</param>
        /// <returns>return sprite object if exist, otherwise return null</returns>
        Sprite FindSprite(string name);
        /// <summary>
        /// Find the target sprite by index. this method may work if sprite collection be indexable.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Sprite GetSpriteByIndex(int index);
    }
}