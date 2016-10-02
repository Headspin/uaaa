namespace Uaaa.Data.Initializers
{
    /// <summary>
    /// Provides access to specific initializer instance.
    /// </summary>
    /// <typeparam name="TInitializer"></typeparam>
    public interface IInitializerProvider<out TInitializer>
    {
        /// <summary>
        /// Retrieves initializer instance.
        /// </summary>
        /// <returns></returns>
        TInitializer GetInitializer();
    }
}
