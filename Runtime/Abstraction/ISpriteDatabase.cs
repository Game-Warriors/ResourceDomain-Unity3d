using GameWarriors.ResourceDomain.Data;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    public interface ISpriteDatabase
    {
        Sprite GetSpriteFromCollection(string key, int index);
        int SpriteCollectionItemCount(string key);
        Sprite GetPersistSprite(string key);
        SpriteCollection FindSpriteCollection(string key);

    }
}
