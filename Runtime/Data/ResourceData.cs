using System;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Data
{
    [Serializable]
    public struct StringVariable
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _variable;

        public string Name => _name;
        public string Variable => _variable;

        public StringVariable(string name, string value)
        {
            _name = name;
            _variable = value;
        }
    }

    [Serializable]
    public struct FloatVariable
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private float _variable;

        public string Name => _name;
        public float Variable => _variable;

        public FloatVariable(string name, float variable)
        {
            _name = name;
            _variable = variable;
        }
    }

    [Serializable]
    public struct IntVariable
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private int _variable;

        public string Name => _name;
        public int Variable => _variable;

        public IntVariable(string name, int variable)
        {
            _name = name;
            _variable = variable;
        }
    }

    //[Serializable]
    //public struct VariableItem<T>
    //{
    //    [SerializeField]
    //    private string _name;
    //    [SerializeField]
    //    private T _variable;

    //    public string Name => _name;
    //    public T Variable => _variable;
    //}

    public class ResourceData : ScriptableObject
    {
        public const string RESOURCES_PATH = "ResourceData";
        public const string ASSET_PATH = "Assets/AssetData/Resources/ResourceData.asset";

        [SerializeField]
        private string _androidMainServerAddress;
        [SerializeField]
        private string _androidTestServerAddress;
        [SerializeField]
        private string _iosMainServerAddress;
        [SerializeField]
        private string _iosTestServerAddress;
        [SerializeField]
        private bool _isAutoDownload;
        [SerializeField]
        private UnityEngine.Object[] _assetObjects;
        [SerializeField]
        private StringVariable[] _stringVars;
        [SerializeField]
        private FloatVariable[] _floatVars;
        [SerializeField]
        private IntVariable[] _intVars;
        [SerializeField]
        private Sprite[] _persistSprites;

#if UNITY_ANDROID || UNITY_EDITOR
        public string MainServerAddess => _androidMainServerAddress;
        public string TestServerAddess => _androidTestServerAddress;
#elif UNITY_IOS
        public string MainServerAddess => _iosMainServerAddress;
        public string TestServerAddess => _iosTestServerAddress;
#else
        public string MainServerAddess => _androidMainServerAddress;
        public string TestServerAddess => _androidTestServerAddress;
#endif


        public UnityEngine.Object[] AssetObjects => _assetObjects;
        public StringVariable[] StringVars => _stringVars;
        public FloatVariable[] FloatVars => _floatVars;
        public IntVariable[] IntVars => _intVars;
        public Sprite[] PersistSprites => _persistSprites;

        public bool IsAutoDonwload { get => _isAutoDownload; set => _isAutoDownload = value; }

        public void SetAssetObjects(UnityEngine.Object[] assetObjects)
        {
            _assetObjects = assetObjects;
        }

        public void SetStringVariables(StringVariable[] stringVariables)
        {
            _stringVars = stringVariables;
        }

        public void SetFloatVariable(FloatVariable[] floatVariables)
        {
            _floatVars = floatVariables;
        }

        public void SetIntVariable(IntVariable[] intVariables)
        {
            _intVars = intVariables;
        }

        public void SetSpriteAssets(Sprite[] persistSprites)
        {
            _persistSprites = persistSprites;
        }


        public void SetServerAddress(string mainServerAddress, string testServerAddress)
        {
#if UNITY_ANDROID
            _androidMainServerAddress = mainServerAddress;
            _androidTestServerAddress = testServerAddress;
#elif UNITY_IOS
            _iosMainServerAddress = mainServerAddress;
            _iosTestServerAddress = testServerAddress;
#endif
        }

    }
}
