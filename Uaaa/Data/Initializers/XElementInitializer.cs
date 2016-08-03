using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Uaaa.Data.Initializers
{
    /// <summary>
    /// Defines object initialization/serialization from/to XElement.
    /// </summary>
    public interface IXElementInitializer
    {
        /// <summary>
        /// Initializes object instance with provided XElement data.
        /// </summary>
        /// <param name="data"></param>
        void Init(XElement data);

        /// <summary>
        /// Serializes instance data to XElement.
        /// </summary>
        /// <returns></returns>
        XElement ToXElement(XScope scope = null);
    }
    /// <summary>
    /// Provides base IXElementInitializer implementation.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class XInitializerBase<TModel> : IXElementInitializer
    {
        protected readonly TModel Model;

        protected XInitializerBase(TModel model)
        {
            this.Model = model;
        }

        #region -=IXElementInitializer members=-
        void IXElementInitializer.Init(XElement data)
        {
            Initialize(data);
        }

        XElement IXElementInitializer.ToXElement(XScope scope) => ToXElement(scope);

        #endregion
        protected virtual void Initialize(XElement data) { }
        protected abstract XElement ToXElement(XScope scope);
    }

    /// <summary>
    /// Provides scope for XElement creation.
    /// Manages preferred prefix namespace values.
    /// </summary>
    public class XScope
    {
        #region -=Properties/Fields=-
        private readonly ConcurrentDictionary<XNamespace, string> namespaces = new ConcurrentDictionary<XNamespace, string>();
        private readonly object prefixesLockObject = new object();
        private readonly HashSet<string> prefixes = new HashSet<string>();
        /// <summary>
        /// Creator object instance that created this instance of scoped namespace.
        /// </summary>
        public object Creator { get; }
        #endregion
        #region -=Constructors=-
        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="creator">Object that created the instance.</param>
        public XScope(object creator)
        {
            Creator = creator;
        }
        #endregion
        #region -=Public methods=-
        /// <summary>
        /// Registers namespace with preferred prefix. 
        /// If one namespace is registered multiple times, then preferred
        /// prefix from first registration is used. If multiple namespaces are
        /// registered with same preferred prefix, later namespaces get enumerated
        /// prefix values.
        /// </summary>
        /// <param name="xnamespace">XNamespace instance.</param>
        /// <param name="preferredPrefix"></param>
        public void Register(XNamespace xnamespace, string preferredPrefix)
        {
            if (string.IsNullOrEmpty(preferredPrefix))
                throw new ArgumentNullException(nameof(preferredPrefix));
            if (!namespaces.ContainsKey(xnamespace))
            {
                lock (prefixesLockObject)
                {
                    string prefix = GetUniquePrefix(preferredPrefix);
                    namespaces.AddOrUpdate(xnamespace, prefix, (key, value) => value);
                    prefixes.Add(prefix);
                }
            }
        }

        /// <summary>
        /// Adds registered prefixed namespaces to provided XElement and returns altered element instance.
        /// If invoker is provided, namespaces are applied only if invoker is creator object (same reference).
        /// </summary>
        /// <param name="element">XElement to which namespace registrations are added.</param>
        /// <param name="invoker">Object instance that invoked the method.</param>
        /// <returns>Altered XElement.</returns>
        public XElement ApplyNamespaces(XElement element, object invoker = null)
        {
            if (invoker == null || ReferenceEquals(Creator, invoker))
                foreach (KeyValuePair<XNamespace, string> namespacePair in namespaces)
                {
                    element.SetAttributeValue(XNamespace.Xmlns + namespacePair.Value, namespacePair.Key);
                }
            return element;
        }
        #endregion
        #region -=Private helper methods=-
        /// <summary>
        /// Method returns instance specific unique prefix value.
        /// Duplicate prefix is enumerated to get unique value.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private string GetUniquePrefix(string prefix)
        {
            string uniquePrefix = prefix;
            int idx = 0;
            while (prefixes.Contains(uniquePrefix))
                uniquePrefix = $"{prefix}{++idx}";
            return uniquePrefix;
        }
        #endregion
    }

    /// <summary>
    /// Extensions for IInitializerProvider(IXElementInitializer).
    /// </summary>
    public static class XElementInitializerProviderExtensions
    {
        /// <summary>
        /// Initializes the model with provided data.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="data"></param>
        public static void Init(this IInitializerProvider<IXElementInitializer> model, XElement data)
        {
            model.GetInitializer().Init(data);
        }

        public static XElement ToXElement(this IInitializerProvider<IXElementInitializer> model,
            XScope scope = null) => model.GetInitializer().ToXElement(scope);
        /// <summary>
        /// Returns items as list of corresponding XElements.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> ToXElements<TItem>(this IList<TItem> items,
            XScope scope = null) where TItem : IInitializerProvider<IXElementInitializer>
        {
            foreach (IInitializerProvider<IXElementInitializer> provider in items)
            {
                yield return provider.GetInitializer().ToXElement(scope);
            }
        }
        /// <summary>
        /// Creates new TModel instance from provided data.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TModel Create<TModel>(this XElement data)
            where TModel : IInitializerProvider<IXElementInitializer>, new()
        {
            TModel model = Activator.CreateInstance<TModel>();
            model.Init(data);
            return model;
        }
    }
}
