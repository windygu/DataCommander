namespace DataCommander.Foundation.Data
{
    using System;
    using System.Data;
    using DataCommander.Foundation.Diagnostics.Contracts;

    public sealed class ExecuteCommandRequest
    {
        public ExecuteCommandRequest(InitializeCommandRequest initializeCommandRequest, Action<IDbCommand> execute)
        {
            FoundationContract.Requires<ArgumentNullException>(initializeCommandRequest != null);
            FoundationContract.Requires<ArgumentNullException>(execute != null);

            InitializeCommandRequest = initializeCommandRequest;
            Execute = execute;
        }

        public readonly InitializeCommandRequest InitializeCommandRequest;
        public readonly Action<IDbCommand> Execute;
    }
}