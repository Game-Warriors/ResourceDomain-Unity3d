
namespace GameWarriors.ResourceDomain.Abstraction
{
    public interface IResourceConfig
    {
        int ShiftCount { get; }
        bool IsPreloadBundles { get; }
    }
}
