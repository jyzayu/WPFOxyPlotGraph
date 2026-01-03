using Oracle.ManagedDataAccess.Client;
using System.Data;
using WpfOxyPlotGraph.Commons;

namespace WpfOxyPlotGraph.Repositories
{
    public class InventoryRepository
    {
        public void EnsureTables()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            // Intentionally left blank; schema managed via SQL scripts
        }

        public int GetQuantity(int medicationId)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.inventory_get_quantity";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_medication_id", medicationId));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, System.Data.ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (reader.IsDBNull(0)) return 0;
                var any = reader.GetValue(0);
                return (int)System.Convert.ChangeType(any, typeof(int));
            }
            return 0;
        }

        public void UpsertQuantity(int medicationId, int quantity)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.inventory_upsert_quantity";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_medication_id", medicationId));
            cmd.Parameters.Add(new OracleParameter("p_quantity", quantity));
            cmd.ExecuteNonQuery();
        }

        public void Increase(int medicationId, int delta)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.inventory_increase";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_medication_id", medicationId));
            cmd.Parameters.Add(new OracleParameter("p_delta", delta));
            cmd.ExecuteNonQuery();
        }

        public void Decrease(int medicationId, int delta)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.inventory_decrease";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_medication_id", medicationId));
            cmd.Parameters.Add(new OracleParameter("p_delta", delta));
            cmd.ExecuteNonQuery();
        }
    }
}



