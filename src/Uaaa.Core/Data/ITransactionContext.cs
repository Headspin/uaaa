using System;
using System.Threading.Tasks;

namespace Uaaa.Data
{
    /// <summary>
    /// Defines objects that support transactions
    /// </summary>
    public interface ITransactionContext : IDisposable
    {
        /// <summary>
        /// Starts transaction.
        /// </summary>
        Task StartTransaction();
        /// <summary>
        /// Commits transaction.
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// Performs transaction rollback.
        /// </summary>
        void RollbackTransaction();
    }
}
