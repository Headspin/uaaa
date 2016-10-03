namespace Uaaa
{
    /// <summary>
    /// Represents object that contains another object as value.
    /// </summary>
    public interface IValueContainer
    {
        /// <summary>
        /// Returns value.
        /// </summary>
        /// <returns></returns>
        object GetValue();
        /// <summary>
        /// Sets value.
        /// </summary>
        /// <param name="value"></param>
        void SetValue(object value);
    }
    /// <summary>
    /// Represents object that contains another T object as value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IValueContainer<TValue> : IValueContainer
    {
        /// <summary>
        /// Value.
        /// </summary>
        TValue Value { get; set; }
    }

    /// <summary>
    /// Defines extensions for IValueContainer types.
    /// </summary>
    public static class ValueContainerExtensions
    {
        /// <summary>
        /// Returns value from IValueContainer instance.
        /// </summary>
        /// <param name="container"></param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue Get<TValue>(this IValueContainer<TValue> container) 
            => container.Value;

        /// <summary>
        /// Sets value to IValueContainer instance.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="value"></param>
        /// <typeparam name="TValue"></typeparam>
        public static void Set<TValue>(this IValueContainer<TValue> container, TValue value)
            => container.Value = value;
    }
}
