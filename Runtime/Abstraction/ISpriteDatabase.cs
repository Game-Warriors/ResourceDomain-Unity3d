using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstraction which presents sprite fetching operations.
    /// </summary>
    public interface ISpriteDatabase
    {
        /// <summary>
        /// Finding the sprit collection in sprite container by the input key and return the specific sprite at the requested index.
        /// </summary>
        /// <param name="key">the name of the sprite collection asset</param>
        /// <param name="index">index of the sprite in sprite collection</param>
        /// <returns></returns>
        Sprite GetSpriteFromCollection(string key, int index);
        /// <summary>
        /// retrieving the sprite count in the specific sprite collection.
        /// </summary>
        /// <param name="key">the name of the sprite collection asset</param>
        /// <returns>the exist sprite count in collection</returns>
        int SpriteCollectionItemCount(string key);
        /// <summary>
        /// Finding the specific sprite in sprite container which have been added to system by editor.
        /// </summary>
        /// <param name="key">the name of the sprite asset</param>
        /// <returns>the sprite which loaded in memory and ready to use</returns>
        Sprite GetPersistSprite(string key);
        /// <summary>
        /// Finding the specific sprite collection that needs to be added in the system editor in the "Unity Object" tab.
        /// </summary>
        /// <param name="key">the name of the sprite collection asset</param>
        /// <returns>the collection of multiply sprites</returns>
        ISpriteCollection FindSpriteCollection(string key);
    }
}
