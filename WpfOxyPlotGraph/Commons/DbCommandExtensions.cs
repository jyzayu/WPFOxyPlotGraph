using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;

namespace WpfOxyPlotGraph.Commons
{
    public static class DbCommandExtensions
    {
        public static OracleDataReader ExecuteReaderTimed(this OracleCommand command, int? fetchSizeMultiplier = 1024, string? tag = null)
        {
            var sw = Stopwatch.StartNew();
            var reader = command.ExecuteReader();
            sw.Stop();

            var label = tag ?? (command.CommandType == System.Data.CommandType.StoredProcedure ? command.CommandText : command.CommandText?.Substring(0, System.Math.Min(80, command.CommandText?.Length ?? 0)));
            Debug.WriteLine($"[DB] ExecuteReader {command.CommandType} {label} -> {sw.ElapsedMilliseconds} ms");
            Trace.WriteLine($"[DB] ExecuteReader {command.CommandType} {label} -> {sw.ElapsedMilliseconds} ms");

            if (fetchSizeMultiplier.HasValue && fetchSizeMultiplier.Value > 0 && reader.RowSize > 0)
            {
                try { reader.FetchSize = reader.RowSize * fetchSizeMultiplier.Value; } catch { }
            }

            return reader;
        }

        public static int ExecuteNonQueryTimed(this OracleCommand command, string? tag = null)
        {
            var sw = Stopwatch.StartNew();
            var affected = command.ExecuteNonQuery();
            sw.Stop();
            var label = tag ?? (command.CommandType == System.Data.CommandType.StoredProcedure ? command.CommandText : command.CommandText?.Substring(0, System.Math.Min(80, command.CommandText?.Length ?? 0)));
            Debug.WriteLine($"[DB] ExecuteNonQuery {command.CommandType} {label} -> {sw.ElapsedMilliseconds} ms (rows:{affected})");
            Trace.WriteLine($"[DB] ExecuteNonQuery {command.CommandType} {label} -> {sw.ElapsedMilliseconds} ms (rows:{affected})");
            return affected;
        }

        public static object? ExecuteScalarTimed(this OracleCommand command, string? tag = null)
        {
            var sw = Stopwatch.StartNew();
            var val = command.ExecuteScalar();
            sw.Stop();
            var label = tag ?? (command.CommandType == System.Data.CommandType.StoredProcedure ? command.CommandText : command.CommandText?.Substring(0, System.Math.Min(80, command.CommandText?.Length ?? 0)));
            Debug.WriteLine($"[DB] ExecuteScalar {command.CommandType} {label} -> {sw.ElapsedMilliseconds} ms");
            Trace.WriteLine($"[DB] ExecuteScalar {command.CommandType} {label} -> {sw.ElapsedMilliseconds} ms");
            return val;
        }
    }
}


