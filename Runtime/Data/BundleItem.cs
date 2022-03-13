using GameWarriors.ResourceDomain.Abstraction;
using UnityEngine;

namespace GameWarriors.ResourceDomain.Data
{
    public class BundleItem
    {

        public string Path { get; }
        public EBundleType BundleType { get; }
        public uint CRC { get; }
        public AssetBundle Bundle { get; set; }
        public bool IsBundleLoad => Bundle != null;

        public BundleItem(EBundleType bundleType, string path, uint crc)
        {
            BundleType = bundleType;
            Path = path;
            CRC = crc;
        }
    }
}