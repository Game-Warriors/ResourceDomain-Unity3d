using GameWarriors.ResourceDomain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Editor
{
    public class SpriteElement : IResourceTabElement
    {
        [SerializeField]
        private List<Sprite> _spriteAssets;

        private List<int> _searchIndex;

        public int Count => IsInSearch ? _searchIndex.Count : _spriteAssets?.Count ?? 0;
        public bool IsInSearch => !string.IsNullOrEmpty(SearchPattern);
        public string SearchPattern { get; set; }
        public int CurrentIndex { get; private set; }

        public SpriteElement(Sprite[] sprites)
        {
            _spriteAssets = new List<Sprite>();
            if (sprites != null)
                _spriteAssets.AddRange(sprites);
        }

        public void AddNewElement()
        {
            _searchIndex?.Clear();
            _spriteAssets ??= new List<Sprite>();
            _spriteAssets.Add(default);
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
            Sprite tmp = _spriteAssets[index];
            EditorGUILayout.LabelField("Name: " + tmp?.name);
            _spriteAssets[index] = EditorGUILayout.ObjectField("Asset Object", tmp, typeof(Sprite), false) as Sprite;

            GUILayout.Space(5);
            if (GUILayout.Button("Remove"))
            {
                _spriteAssets.RemoveAt(index);
                ClearSearchPatten();
            }
            else
                ++CurrentIndex;
        }

        public void SaveElement<T>(T input)
        {
            var config = input as ResourceData;
            config.SetSpriteAssets(_spriteAssets.Where((item) => item != null)
                .ToArray());
        }

        public void ResetDraw()
        {
            CurrentIndex = 0;
        }

        public void ApplySearchPatten(string newPattern)
        {
            SearchPattern = newPattern;
            int count = Mathf.Max(_spriteAssets.Count, 5);
            _searchIndex ??= new List<int>(count);
            _searchIndex.Clear();
            int length = _spriteAssets.Count;
            for (int i = 0; i < length; ++i)
            {
#if UNITY_2021_1_OR_NEWER
                if (_spriteAssets[i] != null && _spriteAssets[i].name.Contains(newPattern, System.StringComparison.OrdinalIgnoreCase))
#else
                if (_spriteAssets[i] != null &&_spriteAssets[i].name.Contains(newPattern))
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
