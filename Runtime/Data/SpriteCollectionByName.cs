using UnityEngine;

namespace ResourceDomain.Data
{
    [CreateAssetMenu(fileName = "SpriteCollectionByName", menuName = "AssetObjects/Create Sprite Collection by  Name")]
    public class SpriteCollectionByName : ScriptableObject
    {
        private SpriteData[] _spriteItems;

        public int ItemCount => _spriteItems?.Length ?? 0;

        public SpriteData this[int index]
        {
            get
            {
                if (index < _spriteItems.Length)
                    return _spriteItems[index];
                else
                    Debug.LogError($"Index Out of range in {name} sprite collection");
                return null;
            }
        }

        public Sprite this[string spriteName]
        {
            get
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (string.Compare(_spriteItems[i].SpriteName, spriteName) == 0)
                        return _spriteItems[i].IconSprite;
                }

                Debug.LogError($"sprite {name} not in sprite collection");
                return null;
            }
        }
    }
    [System.Serializable]
    public class SpriteData
    {
        [SerializeField]
        private string _spriteName;
        [SerializeField]
        private Sprite _iconSprite;

        public string SpriteName => _spriteName;
        public Sprite IconSprite => _iconSprite;
    }
}
