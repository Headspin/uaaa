using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Uaaa
{
    /// <summary>
    /// Base class for defining ambient operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Operation<T> : IDisposable
    {
        private bool isDisposed = false;
        /// <summary>
        /// Adds operation to running operations list based on provided context.
        /// </summary>
        protected abstract void BeginOperationAction();
        /// <summary>
        /// Removes operation from running operations list based on provided context.
        /// </summary>
        protected abstract void EndOperationAction();
        /// <summary>
        /// Perform dispose actions.
        /// </summary>
        protected virtual void DoDispose() { }

        /// <summary>
        /// Context which defines operation.
        /// </summary>
        protected T Context;
        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="context"></param>
        protected Operation(T context)
        {
            Context = context;
            Initialize();
        }
        /// <summary>
        /// Disposes the instance.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                EndOperationAction();
                DoDispose();
            }
        }

        private void Initialize() => BeginOperationAction();
    }
    /// <summary>
    /// Defines concurrent ambient operation on given context.
    /// </summary>
    /// <typeparam name="TContext">Context type.</typeparam>
    public class AmbientOperation<TContext> : Operation<TContext>
    {
        /// <summary>
        /// Context marker that separates same TContexts by operation type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected sealed class ContextMarker<T> : IValueContainer<T>
        {
            private readonly Type operationType = null;
            /// <summary>
            /// Creates new instance of context marker.
            /// </summary>
            /// <param name="operation"></param>
            /// <param name="value"></param>
            public ContextMarker(AmbientOperation<T> operation, T value) : this(operation.GetType(), value) { }
            /// <summary>
            /// Creates new instance of context marker.
            /// </summary>
            /// <param name="operationType"></param>
            /// <param name="value"></param>
            public ContextMarker(Type operationType, T value)
            {
                this.operationType = operationType;
                this.Set(value);
            }
            #region -=IValueContainer members=-
            /// <see cref="IValueContainer{TValue}.Value"/>
            T IValueContainer<T>.Value { get; set; }
            /// <see cref="IValueContainer.GetValue"/>
            object IValueContainer.GetValue() => this.Get<T>();

            /// <see cref="IValueContainer.SetValue"/>
            void IValueContainer.SetValue(object value) => this.Set((T)value);
            #endregion
            /// <summary>
            /// Compares two objects.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                ContextMarker<T> source = obj as ContextMarker<T>;
                return source != null && source.Get<T>().Equals(this.Get<T>());
            }
            /// <summary>
            /// Returns objects hashcode.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode() 
                => operationType.GetHashCode() ^ this.Get<T>().GetHashCode();
        }
        #region -=Instance members=-
        /// <summary>
        /// Signals operation end.
        /// </summary>
        public readonly AutoResetEvent Finished = new AutoResetEvent(false);
        /// <summary>
        /// Operation is the first running operation with its context.
        /// When value is false, operation is not marked as running operation
        /// for its context. Another operation with same context is already running.
        /// </summary>
        public bool IsPrimary { get; private set; }
        /// <summary>
        /// Create new instance.
        /// </summary>
        /// <param name="context"></param>
        public AmbientOperation(TContext context) : base(context) { }

        /// <summary>
        /// Enlists operation to running operation list.
        /// </summary>
        protected override void BeginOperationAction()
        {

            ContextMarker<TContext> marker = new ContextMarker<TContext>(this, Context);

            Counter.AddOrUpdate(marker, 1, (key, value) => value + 1);
            bool isPrimary = true;
            RunningOperations.AddOrUpdate(marker, this, (key, value) =>
            {
                isPrimary = false; // another operation is already running
                return value;      // do not overwrite.
            });
            this.IsPrimary = isPrimary;
        }
        /// <summary>
        /// Removes operation from running operations list.
        /// </summary>
        protected override void EndOperationAction()
        {
            ContextMarker<TContext> marker = new ContextMarker<TContext>(this, Context);
            int count = 0;
            Counter.AddOrUpdate(marker, 0, (key, value) =>
            {
                count = value - 1;
                return count;
            });
            AmbientOperation<TContext> operation;
            if (RunningOperations.TryGetValue(marker, out operation))
            {
                // remove only if running operation is current instance.
                if (operation.Equals(this))
                    RunningOperations.TryRemove(marker, out operation);
            }
            if (count <= 0)
                Counter.TryRemove(marker, out count);
            Finished.Set();

        }
        /// <summary>
        /// Perform dispose actions.
        /// </summary>
        protected override void DoDispose()
        {
            base.DoDispose();
            Finished.Dispose();
        }

        /// <summary>
        /// Waits for Primary operation Finished AutoResetEvent.
        /// </summary>
        public void WaitPrimary<TOperation>() where TOperation : AmbientOperation<TContext>
        {
            TOperation primary = GetOperation<TOperation>(Context);
            if (primary != null && !primary.Equals(this))
                primary.Finished.WaitOne();
        }
        #endregion
        #region -=Static members=-
        /// <summary>
        /// Running operations.
        /// </summary>
        protected static readonly ConcurrentDictionary<ContextMarker<TContext>, AmbientOperation<TContext>> RunningOperations = new ConcurrentDictionary<ContextMarker<TContext>, AmbientOperation<TContext>>();
        /// <summary>
        /// Counts number of operations for given context.
        /// </summary>
        protected static readonly ConcurrentDictionary<ContextMarker<TContext>, int> Counter = new ConcurrentDictionary<ContextMarker<TContext>, int>();
        /// <summary>
        /// Returns TRUE if operation is running for given context, FALSE otherwise.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsRunning<TOperation>(TContext context) where TOperation : AmbientOperation<TContext>
        {
            ContextMarker<TContext> marker = new ContextMarker<TContext>(typeof(TOperation), context);
            return RunningOperations.ContainsKey(marker);
        }
        /// <summary>
        ///  Returns number for concurrent operations running for given context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int GetCount<TOperation>(TContext context) where TOperation : AmbientOperation<TContext>
        {
            ContextMarker<TContext> marker = new ContextMarker<TContext>(typeof(TOperation), context);
            int value;
            Counter.TryGetValue(marker, out value);
            return value;
        }
        /// <summary>
        /// Returns operation for given context.
        /// </summary>
        /// <param name="context">Context instance.</param>
        /// <returns></returns>
        public static TOperation GetOperation<TOperation>(TContext context) where TOperation : AmbientOperation<TContext>
        {
            ContextMarker<TContext> marker = new ContextMarker<TContext>(typeof(TOperation), context);
            AmbientOperation<TContext> operation;
            RunningOperations.TryGetValue(marker, out operation);
            return operation as TOperation;
        }
        #endregion
    }
}
