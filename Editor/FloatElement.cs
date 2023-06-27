using GameWarriors.ResourceDomain.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Editor
{
    public class FloatElement : IResourceTabElement
    {
        [SerializeField]
        private List<FloatVariable> _floatVariables;

        private List<int> _searchIndex;

        public int Count => IsInSearch ? _searchIndex.Count : _floatVariables?.Count ?? 0;
        public bool IsInSearch => !string.IsNullOrEmpty(SearchPattern);
        public string SearchPattern { get; set; }
        public int CurrentIndex { get; private set; }

        public FloatElement(FloatVariable[] floatVariables)
        {
            _floatVariables = new List<FloatVariable>();
            if (floatVariables != null)
                _floatVariables.AddRange(floatVariables);
        }

        public void AddNewElement()
        {
            _searchIndex?.Clear();
            _floatVariables ??= new List<FloatVariable>();
            _floatVariables.Add(default);
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
            FloatVariable tmp = _floatVariables[index];
            string name = EditorGUILayout.TextField("Name", tmp.Name);
            float variable = EditorGUILayout.FloatField("Value", tmp.Variable);
            _floatVariables[index] = new FloatVariable(name, variable);
            GUILayout.Space(5);
            if (GUILayout.Button("Remove"))
            {
                _floatVariables.RemoveAt(index);
                ClearSearchPatten();
            }
            else
                ++CurrentIndex;
        }

        public void SaveElement<T>(T input)
        {
            var config = input as ResourceData;
            config.SetFloatVariable(_floatVariables.Where((item) => item.Name != null)
                .ToArray());
        }
        public void ResetDraw()
        {
            CurrentIndex = 0;
        }

        public void ApplySearchPatten(string newPattern)
        {
            SearchPattern = newPattern;
            int count = Mathf.Max(_floatVariables.Count, 5);
            _searchIndex ??= new List<int>(count);
            _searchIndex.Clear();
            int length = _floatVariables.Count;
            for (int i = 0; i < length; ++i)
            {
#if UNITY_2021_1_OR_NEWER
                if (_floatVariables[i].Name.Contains(newPattern, System.StringComparison.OrdinalIgnoreCase))
#else
                    if (_floatVariables[i].Name.Contains(newPattern))
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
