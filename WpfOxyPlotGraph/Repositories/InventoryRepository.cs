using Oracle.ManagedDataAccess.Client;
using WpfOxyPlotGraph.Commons;

namespace WpfOxyPlotGraph.Repositories
{
    public class InventoryRepository
    {
        public void EnsureTables()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText =
@"CREATE TABLE inventory (
  medication_id NUMBER PRIMARY KEY,
  quantity NUMBER DEFAULT 0 NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT fk_inventory_medication FOREIGN KEY (medication_id) REFERENCES medications(id) ON DELETE CASCADE
)";
                try { cmd.ExecuteNonQuery(); } catch (OracleException ex) when (ex.Number == 955) { }
            }
        }

        public int GetQuantity(int medicationId)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT quantity FROM inventory WHERE medication_id=:mid";
            cmd.Parameters.Add(new OracleParameter("mid", medicationId));
            var result = cmd.ExecuteScalar();
            if (result == null || result == System.DBNull.Value) return 0;
            return (int)System.Convert.ChangeType(result, typeof(int));
        }

        public void UpsertQuantity(int medicationId, int quantity)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText =
@"MERGE INTO inventory i
USING (SELECT :mid AS medication_id, :qty AS quantity FROM dual) src
ON (i.medication_id = src.medication_id)
WHEN MATCHED THEN UPDATE SET i.quantity = src.quantity, i.updated_at = CURRENT_TIMESTAMP
WHEN NOT MATCHED THEN INSERT (medication_id, quantity) VALUES (src.medication_id, src.quantity)";
            cmd.Parameters.Add(new OracleParameter("mid", medicationId));
            cmd.Parameters.Add(new OracleParameter("qty", quantity));
            cmd.ExecuteNonQuery();
        }

        public void Increase(int medicationId, int delta)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            // initialize if missing
            var current = GetQuantity(medicationId);
            var next = current + delta;
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"MERGE INTO inventory i
USING (SELECT :mid AS medication_id, :qty AS quantity FROM dual) src
ON (i.medication_id = src.medication_id)
WHEN MATCHED THEN UPDATE SET i.quantity = i.quantity + src.quantity, i.updated_at = CURRENT_TIMESTAMP
WHEN NOT MATCHED THEN INSERT (medication_id, quantity) VALUES (src.medication_id, src.quantity)";
            cmd.Parameters.Add(new OracleParameter("mid", medicationId));
            cmd.Parameters.Add(new OracleParameter("qty", delta));
            cmd.ExecuteNonQuery();
        }

        public void Decrease(int medicationId, int delta)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE inventory SET quantity = quantity - :qty, updated_at = CURRENT_TIMESTAMP WHERE medication_id=:mid";
            cmd.Parameters.Add(new OracleParameter("qty", delta));
            cmd.Parameters.Add(new OracleParameter("mid", medicationId));
            var rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"INSERT INTO inventory (medication_id, quantity) VALUES (:mid, :qty)";
                insertCmd.Parameters.Add(new OracleParameter("mid", medicationId));
                insertCmd.Parameters.Add(new OracleParameter("qty", -delta));
                insertCmd.ExecuteNonQuery();
            }
        }
    }
}



