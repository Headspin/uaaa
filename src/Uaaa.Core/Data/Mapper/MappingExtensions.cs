using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Mapper
{
    /// <summary>
    /// Defines mapping extensions for dictionary type.
    /// </summary>
    public static class DictionaryMappingExtensions
    {
        /// <summary>
        /// Writes dictionary data to target object using mapping schema for target object type.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="data"></param>
        /// <param name="target"></param>
        public static void WriteTo<TTarget>(this Dictionary<string, object> data, TTarget target)
            => MappingSchema.Get<TTarget>().Write(target, name => data.ContainsKey(name) ? data[name] : System.Type.Missing);

        /// <summary>
        /// Reads data from source object instance using mapping schema defined for source object type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="data"></param>
        /// <param name="source"></param>
        public static void ReadFrom<TSource>(this Dictionary<string, object> data, TSource source)
            => MappingSchema.Get<TSource>().Read(source, (name, value) => data[name] = value);
    }
}
