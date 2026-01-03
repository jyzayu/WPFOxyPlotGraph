using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Repositories
{
    public class AppointmentRepository
    {
        public void EnsureTables()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            // Intentionally left blank; schema managed via SQL scripts
        }

        public IEnumerable<string> GetDistinctDoctorNames()
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.appointment_get_distinct_doctor_names";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, System.Data.ParameterDirection.Output);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return reader.GetString(0);
            }
        }

        public IEnumerable<Appointment> GetByPatientId(int patientId)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.appointment_get_by_patient";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_patient_id", patientId));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, System.Data.ParameterDirection.Output);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Appointment
                {
                    Id = reader.GetInt32(0),
                    PatientId = reader.GetInt32(1),
                    AppointmentDate = reader.GetDateTime(2),
                    DoctorName = reader.GetString(3),
                    Reason = reader.GetString(4),
                    Status = reader.GetString(5),
                    Notes = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.GetDateTime(8),
                };
            }
        }

        public IEnumerable<Appointment> GetByDoctorAndDate(string doctorName, System.DateTime date)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.appointment_get_by_doctor_and_date";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_doctor_name", doctorName));
            cmd.Parameters.Add(new OracleParameter("p_date", date));
            cmd.Parameters.Add("p_cursor", OracleDbType.RefCursor, System.Data.ParameterDirection.Output);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                yield return new Appointment
                {
                    Id = reader.GetInt32(0),
                    PatientId = reader.GetInt32(1),
                    AppointmentDate = reader.GetDateTime(2),
                    DoctorName = reader.GetString(3),
                    Reason = reader.GetString(4),
                    Status = reader.GetString(5),
                    Notes = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    UpdatedAt = reader.GetDateTime(8),
                };
            }
        }

        public int Insert(Appointment appointment)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.appointment_insert";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_patient_id", appointment.PatientId));
            cmd.Parameters.Add(new OracleParameter("p_appointment_date", appointment.AppointmentDate));
            cmd.Parameters.Add(new OracleParameter("p_doctor_name", appointment.DoctorName));
            cmd.Parameters.Add(new OracleParameter("p_reason", appointment.Reason));
            cmd.Parameters.Add(new OracleParameter("p_status", string.IsNullOrWhiteSpace(appointment.Status) ? "Scheduled" : appointment.Status));
            cmd.Parameters.Add(new OracleParameter("p_notes", string.IsNullOrWhiteSpace(appointment.Notes) ? (object)System.DBNull.Value : appointment.Notes));
            var idParam = new OracleParameter("p_id_out", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
            cmd.Parameters.Add(idParam);
            cmd.ExecuteNonQuery();
            //tem.InvalidCastException: 'Unable to cast object of type 'Oracle.ManagedDataAccess.Types.OracleDecimal' to type 'System.IConvertible'.'
            //return System.Convert.ToInt32(idParam.Value);
            return ((Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value).ToInt32();
            
        }

        public void Update(Appointment appointment)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.appointment_update";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_id", appointment.Id));
            cmd.Parameters.Add(new OracleParameter("p_appointment_date", appointment.AppointmentDate));
            cmd.Parameters.Add(new OracleParameter("p_doctor_name", appointment.DoctorName));
            cmd.Parameters.Add(new OracleParameter("p_reason", appointment.Reason));
            cmd.Parameters.Add(new OracleParameter("p_status", string.IsNullOrWhiteSpace(appointment.Status) ? "Scheduled" : appointment.Status));
            cmd.Parameters.Add(new OracleParameter("p_notes", string.IsNullOrWhiteSpace(appointment.Notes) ? (object)System.DBNull.Value : appointment.Notes));
            cmd.ExecuteNonQuery();
        }

        public void Cancel(int appointmentId)
        {
            using var connection = DbConnectionFactory.CreateOpenConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "api.appointment_cancel";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("p_id", appointmentId));
            cmd.ExecuteNonQuery();
        }
    }
}



