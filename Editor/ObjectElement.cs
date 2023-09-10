using GameWarriors.ResourceDomain.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Editor
{
    public class ObjectElement : IResourceTabElement
    {
        private List<Object> _assetObjects;

        private List<int> _searchIndex;

        public int Count => IsInSearch ? _searchIndex.Count : (_assetObjects?.Count ?? 0);
        public bool IsInSearch => !string.IsNullOrEmpty(SearchPattern);
        public string SearchPattern { get; set; }
        public int CurrentIndex { get; private set; }

        public ObjectElement(Object[] objects)
        {
            _searchIndex?.Clear();
            _assetObjects = new List<Object>();
            if (objects != null)
                _assetObjects.AddRange(objects);
        }

        public void AddNewElement()
        {
            if (_assetObjects == null)
            {
                _assetObjects = new List<Object>();
            }
            _assetObjects.Add(null);
            ClearSearchPatten();
        }

        public void DrawElement(int width, int height)
        {
            int index = 0;
            if (IsInSearch)
            {
                index = _searchIndex[CurrentIndex];
            }
            else
                index = CurrentIndex;
            EditorGUILayout.LabelField("Name: " + _assetObjects[index]?.name);
            System.Type type = null;
            if (_assetObjects[index] != null)
                type = (_assetObjects[index]).GetType();
            else
                type = typeof(UnityEngine.Object);
            _assetObjects[index] = EditorGUILayout.ObjectField("Asset Object", _assetObjects[index], type, false);
            GUILayout.Space(5);
            if (GUILayout.Button("Remove"))
            {
                _assetObjects.RemoveAt(index);
                ClearSearchPatten();
            }
            else
                ++CurrentIndex;
        }

        public void SaveElement<T>(T input)
        {
            var config = input as ResourceData;
            config.SetAssetObjects(_assetObjects.Where((item) => item != null)
                .ToArray());
        }

        public void ResetDraw()
        {
            CurrentIndex = 0;
        }

        public void ApplySearchPatten(string newPattern)
        {
            SearchPattern = newPattern;
            int count = Mathf.Max(_assetObjects.Count, 5);
            _searchIndex ??= new List<int>(count);
            _searchIndex.Clear();
            int length = _assetObjects.Count;
            for (int i = 0; i < length; ++i)
            {
#if UNITY_2021_1_OR_NEWER
                if (_assetObjects[i] != null && _assetObjects[i].name.Contains(newPattern, System.StringComparison.OrdinalIgnoreCase))
#else
                if (_assetObjects[i] != null && _assetObjects[i].name.Contains(newPattern))
#endif
                    _searchIndex.Add(i);
            }
        }

        public void ClearSearchPatten()
        {
            _searchIndex?.Clear();
            SearchPattern = string.Empty;
        }
    }
}
