using GameWarriors.ResourceDomain.Abstraction;

namespace GameWarriors.ResourceDomain.Data
{
    public class BundleItem
    {

        public string Path { get; }
        public EBundleType BundleType { get; }
        public uint CRC { get; }
        //public AssetBundle Bundle { get; set; }
        public bool IsBundleLoad => false; //=> Bundle != null;

        public BundleItem(EBundleType bundleType, string path, uint crc)
        {
            BundleType = bundleType;
            Path = path;
            CRC = crc;
        }
    }
}