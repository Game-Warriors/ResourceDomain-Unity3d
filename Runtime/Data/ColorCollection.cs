using UnityEngine;

namespace GameWarriors.ResourceDomain.Data
{
    [CreateAssetMenu(fileName = "ColorCollection", menuName = "AssetObjects/Create Color Collection")]
    public class ColorCollection : ScriptableObject
    {
        [SerializeField]
        private Color[] _colorItems;

        public int ItemCount => _colorItems?.Length ?? 0;

        public Color this[int index]
        {
            get
            {
                if (index < _colorItems.Length)
                    return _colorItems[index];
                else
                    Debug.LogError($"Index Out of range in {name} color collection");
                return default;
            }
        }
    }
}
