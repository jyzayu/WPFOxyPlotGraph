using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Repositories
{
    public class VisitRepository
    {
        public void EnsureTables()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
        }

        public IEnumerable<PatientVisit> GetByPatientId(int patientId)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.visit_get_by_patient";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_patient_id", patientId));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReaderTimed(tag: "api.visit_get_by_patient");
            while (reader.Read())
            {
                yield return new PatientVisit
                {
                    Id = reader.GetInt32(0),
                    PatientId = reader.GetInt32(1),
                    VisitDate = reader.GetDateTime(2),
                    DoctorName = reader.GetString(3),
                    Diagnosis = reader.GetString(4),
                    Treatment = reader.GetString(5),
                    Notes = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.GetDateTime(8),
                };
            }
        }

        public IEnumerable<PatientVisit> GetByPatientIdPage(int patientId, int offset, int pageSize)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.visit_get_by_patient_page";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_patient_id", patientId));
            cmd.Parameters.Add(new OracleParameter("p_offset", offset));
            cmd.Parameters.Add(new OracleParameter("p_page_size", pageSize));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            using var reader = cmd.ExecuteReaderTimed(tag: "api.visit_get_by_patient_page");
            while (reader.Read())
            {
                yield return new PatientVisit
                {
                    Id = reader.GetInt32(0),
                    PatientId = reader.GetInt32(1),
                    VisitDate = reader.GetDateTime(2),
                    DoctorName = reader.GetString(3),
                    Diagnosis = reader.GetString(4),
                    Treatment = reader.GetString(5),
                    Notes = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.GetDateTime(8),
                };
            }
        }

        public int Insert(PatientVisit visit)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.visit_insert";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add(new OracleParameter("p_patient_id", visit.PatientId));
            cmd.Parameters.Add(new OracleParameter("p_visit_date", visit.VisitDate));
            cmd.Parameters.Add(new OracleParameter("p_doctor_name", visit.DoctorName));
            cmd.Parameters.Add(new OracleParameter("p_diagnosis", visit.Diagnosis));
            cmd.Parameters.Add(new OracleParameter("p_treatment", visit.Treatment));
            cmd.Parameters.Add(new OracleParameter("p_notes", string.IsNullOrWhiteSpace(visit.Notes) ? (object)System.DBNull.Value : visit.Notes));

            var idParam = new OracleParameter("p_id_out", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(idParam);
            cmd.ExecuteNonQueryTimed(tag: "api.visit_insert");
            return System.Convert.ToInt32(idParam.Value);
        }
    }
}

