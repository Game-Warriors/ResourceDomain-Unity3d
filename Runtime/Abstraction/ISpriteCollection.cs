using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstraction for sprite data collection
    /// </summary>
    public interface ISpriteCollection
    {
        int ItemCount { get; }
        IEnumerable<Sprite> GetSprites { get; }
        Sprite FindSprite(string name);
        Sprite GetSpriteByIndex(int index);
    }
}