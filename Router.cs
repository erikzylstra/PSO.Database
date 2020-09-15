
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace PSO.Database
{
    ///<summary>
    ///This class exists to avoid the need to reference the Oracle.DataAccess dll in pure SQL applications. Connection string providers are inspected to decide whether the reference is required. 
    ///If the dll is needed it is reflected from the uri stored in the config file. Queries are written using interfaces IDbCommand, IDataAdapter, etc. (see e.g. the Fetch class).
    ///IDb object instances are created by calling the appropriate "constructor" in this class which will assign the correct type at runtime.
    ///</summary>
    public static class Router
    {
        private static Type _connection = typeof(SqlConnection);
        private static Type _adapter = typeof(SqlDataAdapter);
        private static Type _parameter = typeof(SqlParameter);
        internal static MethodInfo DeriveParameters = typeof(SqlCommandBuilder).GetMethod("DeriveParameters");


        /// <summary>
        /// The types of the database objects used in the queries are set here
        /// according to the type of provider given in the connection string settings.
        /// Default is SQL.
        /// </summary>
        private static void SetDbObjTypes(string connectionName)
        {
            if (!IsSql(connectionName))
            {
                string OracleDLL = Properties.Settings.Default.DatabaseDLL;
                Assembly assembly = Assembly.LoadFrom(OracleDLL);
                _connection = assembly.GetType("Oracle.DataAccess.Client.OracleConnection");
                _adapter = assembly.GetType("Oracle.DataAccess.Client.OracleDataAdapter");
                _parameter = assembly.GetType("Oracle.DataAccess.Client.OracleParameter");
                Type builder = assembly.GetType("Oracle.DataAccess.Client.OracleCommandBuilder");
                DeriveParameters = builder.GetMethod("DeriveParameters");
            }
        }

        #region Instance Constructors
        public static IDbConnection NewConnection(string connectionName)
        {
            SetDbObjTypes(connectionName);
            var encryptedConnection = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            string connectionString = ""; //= Encryption.Decrypt(encryptedConnection); relied on external static method
            var connection = (IDbConnection)Activator.CreateInstance(_connection);
            connection.ConnectionString = connectionString;
            return connection;
        }

        public static IDbCommand NewCommand(IDbConnection connection, CommandType type, string commandText)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandType = type;
            cmd.CommandText = commandText;
            return cmd;
        }

        public static IDbCommand NewCommand(IDbConnection connection, CommandType type, string commandText, IEnumerable<object> ParameterValues, out Parameters outputParams)
        {
            var cmd = NewCommand(connection, type, commandText);
            outputParams = SetParameters(cmd, ParameterValues);
            return cmd;
        }

        public static IDbCommand NewCommand(IDbConnection connection, CommandType type, int timeout, string commandText, IEnumerable<object> ParameterValues, out Parameters outputParams)
        {
            var cmd = NewCommand(connection, type, commandText, ParameterValues.ToArray(), out outputParams);
            cmd.CommandTimeout = timeout;
            return cmd;
        }

        public static IDbCommand NewCommand(IDbConnection connection, CommandType type, int timeout, string commandText, IEnumerable<object> ParameterValues)
        {
            var cmd = NewCommand(connection, type, commandText, ParameterValues.ToArray(), out Parameters output);
            cmd.CommandTimeout = timeout;
            return cmd;
        }

        public static IDbCommand NewCommand(IDbTransaction trans, CommandType type, int timeout, string commandText, IEnumerable<object> ParameterValues, out Parameters outputParams)
        {
            IDbConnection cn = trans.Connection;
            IDbCommand cmd = NewCommand(cn, type, commandText);
            cmd.Transaction = trans;
            cmd.CommandTimeout = timeout;
            outputParams = SetParameters(cmd, ParameterValues);
            return cmd;
        }

        public static IDataAdapter NewAdapter(IDbCommand command)
        {
            return (IDataAdapter)Activator.CreateInstance(_adapter, command);
        }

        public static IDbDataParameter NewParameter()
        {
            return (IDbDataParameter)Activator.CreateInstance(_parameter);
        }

        #endregion

        private static Parameters SetParameters(IDbCommand cmd, IEnumerable<object> ParameterValues)
        {
            Parameters output = new Parameters();
            if (ParameterValues.Any())
            {
                DeriveParameters.Invoke("", new IDbCommand[] { cmd });
                output = Parameters.SetParameterValues(cmd, ParameterValues);
            }
            return output;
        }

        ///<summary>
        ///Returns whether the provider of the connection string is of SQL type, or not.
        ///Called externally to conditionally pass down different parameter values or procedure names for Oracle/Sql
        ///</summary>
        public static bool IsSql(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                throw new ArgumentNullException("Connection string name passed to PSO.Database.Router.IsSql was null or white space.");
            else if (string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[connectionName]?.ConnectionString))
                throw new System.Exception("Connection string " + connectionName + " not specified in configuration file.");

            return ConfigurationManager.ConnectionStrings[connectionName].ProviderName.ToLower().Contains("sql");
        }

        /// <summary>
        /// Returns true if the transaction is a SqlTransaction.
        /// Called externally to conditionally pass down different parameter values or procedure names for Oracle/Sql
        /// </summary>
        public static bool IsSql(IDbTransaction trans)
        {
            return (trans is SqlTransaction);
        }

        /// <summary>
        /// Change the path to Oracle.DataAccess.dll during run time.
        /// </summary>
        /// <param name="persist">If persist is true, the change in file path will persist between application sessions.</param>
        public static void SetOracleAssemblyUrl(string dllPath, bool persist = false)
        {
            Properties.Settings.Default.DatabaseDLL = dllPath;
            if (persist)
                Properties.Settings.Default.Save();
        }

    }
}