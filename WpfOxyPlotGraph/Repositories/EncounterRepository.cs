using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Repositories
{
	public class EncounterRepository
	{
		public IEnumerable<Encounter> GetByPatientId(int patientId)
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "api.encounter_get_by_patient";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.BindByName = true;
			cmd.Parameters.Add(new OracleParameter("p_patient_id", patientId));
			cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				yield return new Encounter
				{
					Id = reader.GetInt32(0),
					PatientId = reader.GetInt32(1),
					VisitAt = reader.GetDateTime(2),
					Diagnosis = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
					Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
				};
			}
		}

		public int Insert(Encounter encounter)
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "api.encounter_insert";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.BindByName = true;
			cmd.Parameters.Add(new OracleParameter("p_patient_id", encounter.PatientId));
			cmd.Parameters.Add(new OracleParameter("p_visit_at", encounter.VisitAt));
			cmd.Parameters.Add(new OracleParameter("p_diagnosis", encounter.Diagnosis));
			cmd.Parameters.Add(new OracleParameter("p_notes", encounter.Notes));
			var idOut = new OracleParameter("p_id_out", OracleDbType.Int32, ParameterDirection.Output);
			cmd.Parameters.Add(idOut);
			cmd.ExecuteNonQuery();
			return System.Convert.ToInt32(idOut.Value);
		}

		public void Update(Encounter encounter)
		{
			using var connection = DbConnectionFactory.CreateOpenConnection();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "api.encounter_update";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.BindByName = true;
			cmd.Parameters.Add(new OracleParameter("p_id", encounter.Id));
			cmd.Parameters.Add(new OracleParameter("p_patient_id", encounter.PatientId));
			cmd.Parameters.Add(new OracleParameter("p_visit_at", encounter.VisitAt));
			cmd.Parameters.Add(new OracleParameter("p_diagnosis", encounter.Diagnosis));
			cmd.Parameters.Add(new OracleParameter("p_notes", encounter.Notes));
			cmd.ExecuteNonQuery();
		}
	}
}


