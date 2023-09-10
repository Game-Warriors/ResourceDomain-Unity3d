using GameWarriors.ResourceDomain.Data;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstraction which presents sprite fetching operations.
    /// </summary>
    public interface ISpriteDatabase
    {
        Sprite GetSpriteFromCollection(string key, int index);
        int SpriteCollectionItemCount(string key);
        Sprite GetPersistSprite(string key);
        ISpriteCollection FindSpriteCollection(string key);

    }
}
