using Oracle.ManagedDataAccess.Client;
using System.Data;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Repositories
{
	public class PurchaseOrderRepository
	{
		public void EnsureTables()
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();
			// Intentionally left blank; schema managed via SQL scripts
		}

		public int InsertOrIncreasePending(int medicationId, int quantity)
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "api.purchase_order_insert_or_increase_pending";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.BindByName = true;
			cmd.Parameters.Add(new OracleParameter("p_medication_id", medicationId));
			cmd.Parameters.Add(new OracleParameter("p_quantity", quantity));
			var idOut = new OracleParameter("p_id_out", OracleDbType.Int32) { Direction = ParameterDirection.Output };
			cmd.Parameters.Add(idOut);
			cmd.ExecuteNonQueryTimed(tag: "api.purchase_order_insert_or_increase_pending");
			return ((Oracle.ManagedDataAccess.Types.OracleDecimal)idOut.Value).ToInt32();
		}

		public PurchaseOrder? GetById(int id)
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "api.purchase_order_get_by_id";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.BindByName = true;
			cmd.Parameters.Add(new OracleParameter("p_id", id));
			cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
			using var reader = cmd.ExecuteReaderTimed(tag: "api.purchase_order_get_by_id");
			if (reader.Read())
			{
				return new PurchaseOrder
				{
					Id = reader.GetInt32(0),
					MedicationId = reader.GetInt32(1),
					Quantity = reader.GetInt32(2),
					Status = reader.GetString(3),
					CreatedAt = reader.GetDateTime(4),
					UpdatedAt = reader.GetDateTime(5)
				};
			}
			return null;
		}

		public void UpdateStatus(int id, string status)
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "api.purchase_order_update_status";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.BindByName = true;
			cmd.Parameters.Add(new OracleParameter("p_id", id));
			cmd.Parameters.Add(new OracleParameter("p_status", status));
			cmd.ExecuteNonQueryTimed(tag: "api.purchase_order_update_status");
		}
	}
}


