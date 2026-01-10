using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Repositories
{
    public class AuditRepository
    {
        public IEnumerable<DbAuditEntry> GetRecent(int limit = 200)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.audit_get_recent";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_limit", limit));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReaderTimed(tag: "api.audit_get_recent");
            while (reader.Read())
            {
                yield return Map(reader);
            }
        }

        public IEnumerable<DbAuditEntry> GetByTable(string tableName, int limit = 200)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.audit_get_by_table";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_table_name", tableName));
            cmd.Parameters.Add(new OracleParameter("p_limit", limit));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReaderTimed(tag: "api.audit_get_by_table");
            while (reader.Read())
            {
                yield return Map(reader);
            }
        }

			public IEnumerable<DbAuditEntry> GetEncounterHistory(int encounterId, int limit = 500)
			{
				foreach (var e in GetByTable("ENCOUNTERS", limit))
				{
					if (e.PrimaryKeyId == encounterId) yield return e;
				}
			}

        private static DbAuditEntry Map(OracleDataReader reader)
        {
            return new DbAuditEntry
            {
                AuditId = reader.GetInt32(0),
                TableName = reader.GetString(1),
                Operation = reader.GetString(2),
                PrimaryKeyId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                ChangedAt = reader.GetDateTime(4),
                ChangedBy = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                OldValues = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                NewValues = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            };
        }
    }
}


