namespace GameWarriors.ResourceDomain.Abstraction
{
    /// <summary>
    /// The base abstraction to passing required configurations value to resource system
    /// </summary>
    public interface IResourceConfig
    {
        /// <summary>
        /// The number of bits count which will apply left shift bitwise operator to integer values.
        /// </summary>
        int ShiftCount { get; }
        /// <summary>
        /// The Flag which indicate the downloaded or cached bundles is loading in start up and pending loading process to finish 
        /// </summary>
        bool IsPreloadBundles { get; }
    }
}
