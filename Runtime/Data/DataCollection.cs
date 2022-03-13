using UnityEngine;

namespace GameWarriors.ResourceDomain.Data
{
    [CreateAssetMenu(fileName = "DataCollection", menuName = "DataCollection")]
    public class DataCollection : ScriptableObject
    {
        [SerializeField]
        private ScriptableObject[] _collection;

        public ScriptableObject this[int index]
        {
            get
            {
                if (index < 0 || index >= DataCount)
                {
                    Debug.LogError("Index out of range");
                    return null;
                }
                return _collection[index];
            }
        }
        public int DataCount => _collection != null ? _collection.Length : 0;
    }
}