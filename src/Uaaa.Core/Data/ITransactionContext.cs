namespace Uaaa.Data
{
    /// <summary>
    /// Defines objects that support transactions
    /// </summary>
    public interface ITransactionContext
    {
        /// <summary>
        /// Starts transaction.
        /// </summary>
        void StartTransaction();
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
