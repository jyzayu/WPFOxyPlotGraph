using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Repositories
{
    public class MedicationRepository
    {
        public void EnsureTables()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();

            // Intentionally left blank; schema managed via SQL scripts
        }

        public IEnumerable<Medication> GetAll()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.medication_get_all";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, System.Data.ParameterDirection.Output);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Medication
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sku = reader.GetString(2),
                    MinimumStock = reader.GetInt32(3),
                    ReorderPoint = reader.GetInt32(4),
                    ReorderQuantity = reader.GetInt32(5),
                    CreatedAt = reader.GetDateTime(6),
                    UpdatedAt = reader.GetDateTime(7)
                };
            }
        }

        public Medication? GetById(int id)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.medication_get_by_id";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_id", id));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, System.Data.ParameterDirection.Output);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Medication
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sku = reader.GetString(2),
                    MinimumStock = reader.GetInt32(3),
                    ReorderPoint = reader.GetInt32(4),
                    ReorderQuantity = reader.GetInt32(5),
                    CreatedAt = reader.GetDateTime(6),
                    UpdatedAt = reader.GetDateTime(7)
                };
            }
            return null;
        }

        public int Insert(Medication medication)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.medication_insert";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_name", medication.Name));
            cmd.Parameters.Add(new OracleParameter("p_sku", medication.Sku));
            cmd.Parameters.Add(new OracleParameter("p_min_stock", medication.MinimumStock));
            cmd.Parameters.Add(new OracleParameter("p_reorder_point", medication.ReorderPoint));
            cmd.Parameters.Add(new OracleParameter("p_reorder_qty", medication.ReorderQuantity));
            var idParam = new OracleParameter("p_id_out", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
            cmd.Parameters.Add(idParam);
            cmd.ExecuteNonQuery();
            return ((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).ToInt32();
        }

        public void Update(Medication medication)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.medication_update";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_id", medication.Id));
            cmd.Parameters.Add(new OracleParameter("p_name", medication.Name));
            cmd.Parameters.Add(new OracleParameter("p_sku", medication.Sku));
            cmd.Parameters.Add(new OracleParameter("p_min_stock", medication.MinimumStock));
            cmd.Parameters.Add(new OracleParameter("p_reorder_point", medication.ReorderPoint));
            cmd.Parameters.Add(new OracleParameter("p_reorder_qty", medication.ReorderQuantity));
            cmd.ExecuteNonQuery();
        }
    }
}



