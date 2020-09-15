using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace PSO.Database
{
    public class Fetch
    {
        private static MethodInfo DeriveParameters = Router.DeriveParameters;

        ///<summary>
        ///Executes a stored procedure and returns the result as a DataTable.
        ///</summary>
        public static DataTable Execute(PSOQuery Q)
        {
            var ds = Fetch.FetchTables(Q.ConnectionName, Q.CommandType, Q.TimeOutSeconds, Q.CommandString, Q.ParameterValues);  
            return ds.Tables.Count == 0 ? new DataTable() : ds.Tables[0];
        }

        ///<summary>
        ///Executes a stored procedure and returns the result as a DataTable.
        ///</summary>
        public static DataTable Execute(string ConnectionName, string Procedure, params object[] ParameterValues)
        {
            PSOQuery Q = new PSOQuery(ConnectionName, Procedure, ParameterValues.ToArray());
            return Execute(Q);
        }

        ///<summary>
        ///Executes a stored procedure and returns the result as a DataTable. In order to avoid confusion between the parameters and the timeout integer, 
        ///the parameters must be "bundled" and passed in as some enumerable collection
        ///</summary>
        public static DataTable Execute(string ConnectionName, int TimeOutSeconds, string Procedure, IEnumerable<object> ParameterValues)
        {
            PSOQuery Q = new PSOQuery(ConnectionName, Procedure, TimeOutSeconds, CommandType.StoredProcedure, ParameterValues);
            return Execute(Q);
        }

        ///<summary>
        ///Executes a stored procedure and returns the result as a DataSet.
        ///</summary>
        public static DataSet DataSet(string ConnectionName, string Procedure, params object[] ParameterValues)
        {
            return FetchTables(ConnectionName, CommandType.StoredProcedure, Properties.Settings.Default.TimeoutSeconds, Procedure, ParameterValues);
        }

        ///<summary>
        ///Executes a stored procedure and returns the result as a DataSet.
        ///</summary>
        public static DataSet DataSet(string ConnectionName, int TimeOutSeconds, string Procedure, params object[] ParameterValues)
        {
            return FetchTables(ConnectionName, CommandType.StoredProcedure, TimeOutSeconds, Procedure, ParameterValues);
        }

        ///<summary>
        ///Returns an entire table from the database; executes Select * From 'TableName'
        ///</summary>
        public static DataTable Table(string ConnectionName, string TableName)
        {
            var cmdText = "Select * From " + TableName;
            return FetchTables(ConnectionName, CommandType.Text, Properties.Settings.Default.TimeoutSeconds, cmdText, new object[0]).Tables[0];
        }

        ///<summary>
        ///Executes a query passed in as a string and returns the result as a DataTable.
        ///</summary>
        public static DataTable WithText(string ConnectionName, string CommandString, params object[] ParameterValues)
        {
            return FetchTables(ConnectionName, CommandType.Text, Properties.Settings.Default.TimeoutSeconds, CommandString, ParameterValues).Tables[0];
        }

        /// <summary>
        /// Executes a stored procedure.
        /// </summary>
        public static PsoDataReader Reader(string ConnectionName, string Procedure, params object[] ParameterValues)
        {
            DataTable Table = Execute(ConnectionName, Properties.Settings.Default.TimeoutSeconds, Procedure, ParameterValues);
            return new PsoDataReader(Table.CreateDataReader());
        }

        /// <summary>
        /// Executes a stored procedure.
        /// </summary>
        public static PsoDataReader Reader(string ConnectionName, int TimeOutSeconds, string Procedure, IEnumerable<object> ParameterValues)
        {
            DataTable Table = Execute(ConnectionName, TimeOutSeconds, Procedure, ParameterValues);
            return new PsoDataReader(Table.CreateDataReader());
        }

        ///<summary>
        ///Executes a query passed in as a string and returns the result as a DataTable.
        ///</summary>
        public static PsoDataReader ReaderWithText(string ConnectionName, string CommandString, params object[] ParameterValues)
        {
            DataTable Table = WithText(ConnectionName, CommandString, ParameterValues);
            return new PsoDataReader(Table.CreateDataReader());
        }

        private static DataSet FetchTables(string connectionName, CommandType type, int TimeOutSeconds, string commandString, IEnumerable<object> ParameterValues)
        {
            var connection = Router.NewConnection(connectionName);
            connection.Open();
            var command = Router.NewCommand(connection, type, TimeOutSeconds, commandString, ParameterValues, out Parameters output);

            DataSet ds = new DataSet();
            var adapter = Router.NewAdapter(command);
            adapter.Fill(ds);

            connection.Close();
            return ds;
        }
    }
}
