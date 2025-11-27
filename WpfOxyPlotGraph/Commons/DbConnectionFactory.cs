using Oracle.ManagedDataAccess.Client;

namespace WpfOxyPlotGraph.Commons
{
    public static class DbConnectionFactory
    {
        // TODO: Move to configuration if needed
        private const string ConnectionString = "User Id=system;Password=dnarl!0930;Data Source=localhost:1521/XEPDB1;";

        public static OracleConnection CreateOpenConnection()
        {
            var connection = new OracleConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}

