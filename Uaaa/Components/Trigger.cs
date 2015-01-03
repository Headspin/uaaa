using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa {
    /// <summary>
    /// Defines condition and action that should be executed if condition is met.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public sealed class Trigger<TModel> {
        /// <summary>
        /// Trigger condition.
        /// </summary>
        private readonly Predicate<TModel> _condition = null;
        /// <summary>
        /// Trigger action.
        /// </summary>
        private readonly Action<TModel> _action = null;
        /// <summary>
        /// Creates new unconditional trigger.
        /// </summary>
        /// <param name="action"></param>
        public Trigger(Action<TModel> action) : this(model => true, action) { }
        /// <summary>
        /// Creates new Trigger instance.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="action"></param>
        public Trigger(Predicate<TModel> condition, Action<TModel> action) {
            _condition = condition;
            _action = action;
        }
        /// <summary>
        /// Invokes the trigger.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Invoke(TModel model) {
            if (_condition(model)) {
                _action(model);
                return true;
            }
            return false;
        }
    }
}
