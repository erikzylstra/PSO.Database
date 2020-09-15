using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PSO.Database
{
    public sealed class PsoDataReader : IDisposable
    {   //this class essentially wraps a IDataReader and avoids the need for DbNull checks everywhere
        public IDataReader Reader { get; set; }

        public PsoDataReader() { }
        public PsoDataReader(IDataReader dtr)
        {
            Reader = dtr;
        }

        public bool Read()
        {
            if (Reader == null)
                return false;
            else
                return Reader.Read();
        }

        public void Close()
        {
            if (Reader == null) return;
            Reader.Close();
        }

        public DataTable GetSchemaTable()
        {
            return Reader.GetSchemaTable();
        }

        #region Indexers

        public object this[string colName]
        {
            get { return GetValue(colName); }
        }

        #endregion

        #region Gets

        public object GetValue(string colName)
        {
            var value = Reader[colName];
            return IsDbNull(value) ? null : value;
        }

        public DateTime GetDateTime(string colName)
        {
            var value = Reader[colName];
            return IsDbNull(value) ? new DateTime() : Convert.ToDateTime(value);
        }

        public DateTime? GetNullableDateTime(string colName)
        {
            var value = Reader[colName];
            return IsDbNull(value) ? new DateTime?() : Convert.ToDateTime(value);
        }

        public string GetString(string colName)
        {
            var value = Reader[colName];
            return IsDbNull(value) ? null : value.ToString();
        }

        public double GetDecimal(string colName)
        {
            var value = Reader[colName];
            return IsDbNull(value) ? 0.0d : Convert.ToDouble(value);
        }

        public int GetInt32(string colName)
        {
            var value = Reader[colName];
            return IsDbNull(value) ? 0 : Convert.ToInt32(value);
        }

        #endregion

        private bool IsDbNull(object value) { return value == DBNull.Value; }

        #region IDisposable
        public void Dispose()
        {
            if (Reader != null)
                Reader.Dispose();
            GC.SuppressFinalize(this);
        }

        ~PsoDataReader()
        {
            Dispose();
        }

        #endregion
    }
}
