
namespace GameWarriors.ResourceDomain.Core
{
    public readonly struct DownloadPack
    {
        public byte [] Data { get; }
        public int DataLength { get; }

        public DownloadPack(byte[] data, int dataLength) : this()
        {
            Data = data;
            DataLength = dataLength;
        }
    }
}
