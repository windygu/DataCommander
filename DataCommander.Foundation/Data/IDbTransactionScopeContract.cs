namespace DataCommander.Foundation.Data
{
    using System.Data;

    //[ContractClassFor(typeof (IDbTransactionScope))]
    internal abstract class IDbTransactionScopeContract : IDbTransactionScope
    {
        IDbConnection IDbTransactionScope.Connection
        {
            get
            {
#if CONTRACTS_FULL
                Contract.Ensures(Contract.Result<IDbConnection>() != null);
#endif
                return null;
            }
        }

        IDbTransaction IDbTransactionScope.Transaction => null;
    }
}