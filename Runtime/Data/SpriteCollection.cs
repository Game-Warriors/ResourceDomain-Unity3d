using UnityEngine;

namespace GameWarriors.ResourceDomain.Data
{
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "AssetObjects/Create Sprite Collection")]
    public class SpriteCollection : ScriptableObject
    {
        [SerializeField]
        private Sprite[] _spriteitems;

        public int ItemCount => _spriteitems?.Length ?? 0;

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
    }
}
