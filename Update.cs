using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace PSO.Database
{
    public class Update
    {
        private static MethodInfo DeriveParameters = Router.DeriveParameters;

        ///<summary>
        ///Executes a stored procedure that accepts a table-valued parameter.
        ///</summary>
        public static void WithTableValueParameter(DataTable Table, string Procedure, string ConnectionName)
        {
            var connection = Router.NewConnection(ConnectionName);
            var command = Router.NewCommand(connection, CommandType.StoredProcedure, Procedure);
            ExecuteOnTable(Table, command, connection);
        }

        ///<summary>
        ///Executes a stored procedure that accepts a table-valued parameter.
        ///</summary>
        public static void WithTableValueParameter(DataTable Table, string Procedure, string ConnectionName, int TimeOutSeconds)
        {
            var connection = Router.NewConnection(ConnectionName);
            var command = Router.NewCommand(connection, CommandType.StoredProcedure, Procedure);
            command.CommandTimeout = TimeOutSeconds;
            ExecuteOnTable(Table, command, connection);
        }

        ///<summary>
        ///Executes a stored procedure that accepts a table-valued parameter Takes place in an open transaction.
        ///</summary>
        public static void WithTableValueParameter(DataTable Table, string Procedure, IDbTransaction transaction, int TimeOutSeconds)
        {
            IDbConnection connection = transaction.Connection;
            var command = Router.NewCommand(connection, CommandType.StoredProcedure, Procedure);
            command.Transaction = transaction;
            command.CommandTimeout = TimeOutSeconds;

            var parameter = Router.NewParameter();
            parameter.Value = Table;
            parameter.ParameterName = "@P_tbl";
            command.Parameters.Add(parameter);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Like Update.WithTableValueParameter but returns an IDataReader to read back out of the updated table.
        /// A transaction is passed down so we can call this multiple times and roll everything back at once if neccessary.
        /// </summary>
        public static PsoDataReader BulkUpsert(IDbTransaction Transaction, DataTable Table, string Procedure, int TimeOutSeconds)
        {
            IDbConnection Connection = Transaction.Connection;
            var command = Router.NewCommand(Connection, CommandType.StoredProcedure, Procedure);
            command.Transaction = Transaction;
            command.CommandTimeout = TimeOutSeconds;

            var parameter = Router.NewParameter();
            parameter.Value = Table;
            parameter.ParameterName = "@P_tbl";
            command.Parameters.Add(parameter);

            return new PsoDataReader(command.ExecuteReader());
        }

        private static void ExecuteOnTable(DataTable Table, IDbCommand cmd, IDbConnection connection)
        {
            connection.Open();

            var command = cmd;
            var parameter = Router.NewParameter();
            parameter.Value = Table;
            parameter.ParameterName = "@P_tbl";
            command.Parameters.Add(parameter);

            command.ExecuteNonQuery();
            connection.Close();
        }

        ///<summary> 
        ///Executes a stored procedure on each row where the parameters are the cells of the row. Order of columns must match order of parameters.
        ///</summary>
        public static void RowByRow(IEnumerable<DataRow> Rows, string Procedure, string ConnectionName)
        {   
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();

            var command = Router.NewCommand(connection, CommandType.StoredProcedure, Procedure);
            DeriveParameters.Invoke("", new object[] { command });

            foreach (DataRow Row in Rows)
            {
                if (Row.RowState != DataRowState.Deleted)
                    StoredProcedures.Execute(ref command, Row.ItemArray);
            }

            connection.Close();
        }

    }
}
