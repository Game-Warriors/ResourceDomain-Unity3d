using UnityEngine;

namespace ResourceDomain.Data
{
    [CreateAssetMenu(fileName = "MaterialCollectionByName", menuName = "AssetObjects/Create material Collection by  Name")]
    public class MaterialCollectionByName : ScriptableObject
    {

        [SerializeField]
        private MaterialData[] _materialItems;

        public int ItemCount => _materialItems?.Length ?? 0;

        public MaterialData this[int index]
        {
            get
            {
                if (index < _materialItems.Length)
                    return _materialItems[index];
                else
                    Debug.LogError($"Index Out of range in {name} material collection");
                return null;
            }
        }

        public Material this[string materialName]
        {
            get
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    if (string.Compare(_materialItems[i].MaterialName, materialName) == 0)
                        return _materialItems[i].Material;
                }

                Debug.LogError($"material {name} not in material collection");
                return null;
            }
        }
    }
    [System.Serializable]
    public class MaterialData
    {
        [SerializeField]
        private string _materialName;
        [SerializeField]
        private Material _material;

        public string MaterialName => _materialName;
        public Material Material => _material;
    }
}
