using GameWarriors.ResourceDomain.Abstraction;
using System.Collections.Generic;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Data
{
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "AssetObjects/Create Sprite Collection")]
    public class SpriteCollection : ScriptableObject, ISpriteCollection
    {
        [SerializeField]
        private Sprite[] _spriteitems;

        public int ItemCount => _spriteitems?.Length ?? 0;

        public IEnumerable<Sprite> GetSprites => _spriteitems;

        public Sprite this[int index]
        {
            get
            {
                if (index < _spriteitems.Length)
                    return _spriteitems[index];
                else
                    Debug.LogError($"Index Out of range in {name} sprite collection");
                return null;
            }
        }

        public Sprite FindSprite(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            int length = ItemCount;
            for (int i = 0; i < length; ++i)
            {
                if (_spriteitems[i].name == name)
                    return _spriteitems[i];
            }
            return null;
        }

        public Sprite GetSpriteByIndex(int index)
        {
            return this[index];
        }
    }
}
