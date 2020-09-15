using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace PSO.Database
{
    public class StoredProcedures
    {
        private static MethodInfo DeriveParameters = Router.DeriveParameters;

        ///<summary>
        ///Executes a stored procedure. ParametersVals contains the values for the parameters of the procedure and the order must match.
        ///Commonly used to delete rows and do updates.
        ///</summary>
        public static Parameters Execute(string ConnectionName, string StoredProc, params object[] ParameterVals)
        {
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();
            IDbCommand command = Router.NewCommand(connection, CommandType.StoredProcedure, Properties.Settings.Default.TimeoutSeconds, StoredProc, ParameterVals, out Parameters output);
            command.ExecuteNonQuery();
            connection.Close();
            return output;
        }

        ///<summary>
        ///Executes a stored procedure. ParametersVals contains the values for the parameters of the procedure and the order must match.
        ///This overload accepts a (open) transaction.
        ///</summary>
        public static Parameters Execute(IDbTransaction Trans, string StoredProc, params object[] ParameterVals)
        {
            return Execute(Trans, Properties.Settings.Default.TimeoutSeconds, StoredProc, ParameterVals);
        }

        ///<summary>
        ///Executes a stored procedure. ParametersVals contains the values for the parameters of the procedure and the order must match.
        ///This overload accepts a (open) transaction.
        ///</summary>
        public static Parameters Execute(IDbTransaction Trans, int TimeOutSeconds, string StoredProc, IEnumerable<object> ParameterValues)
        {
            IDbCommand command = Router.NewCommand(Trans, CommandType.StoredProcedure, TimeOutSeconds, StoredProc, ParameterValues, out Parameters output);
            command.ExecuteNonQuery();
            return output;
        }

        ///<summary>
        ///Executes a stored procedure. Values is and array of values for the parameters of the procedure and the order must match.
        ///Commonly used to delete rows and do updates.
        ///</summary>
        public static Parameters Execute(string ConnectionName, string StoredProc, IEnumerable<object> ParameterValues)
        {
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();
            IDbCommand command = Router.NewCommand(connection, CommandType.StoredProcedure, StoredProc, ParameterValues.ToArray(), out Parameters output);
            command.ExecuteNonQuery();
            connection.Close();
            return output;
        }

        ///<summary>
        ///Executes query passed in as a string. ParametersVals contains the values for the parameters of the procedure and the order must match.
        ///</summary>
        public static Parameters ExecuteText(string ConnectionName, string CommandString, params object[] ParameterVals)
        {
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();
            IDbCommand command = Router.NewCommand(connection, CommandType.Text, CommandString, ParameterVals, out Parameters output);
            command.ExecuteNonQuery();
            connection.Close();
            return output;
        }

        ///<summary>
        ///Executes a stored procedure. ParametersVals contains the values for the parameters of the procedure and the order must match.
        ///Commonly used to delete rows and do updates.
        ///</summary>
        public static Parameters Execute(string ConnectionName, int TimeOutSeconds, string StoredProc, IEnumerable<object> ParameterValues)
        {
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();
            IDbCommand command = Router.NewCommand(connection, CommandType.StoredProcedure, TimeOutSeconds, StoredProc, ParameterValues, out Parameters output);
            command.ExecuteNonQuery();
            connection.Close();
            return output;
        }

        ///<summary>
        ///Very similar to execute, but each object in ParameterVals is treated as the parameter(s) for an individual procedure execution.
        ///</summary>
        public static void ExecuteMany(string ConnectionName, string StoredProc, IEnumerable<object> ParameterVals)
        { 
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();

            var command = Router.NewCommand(connection, CommandType.StoredProcedure, StoredProc);

            if (ParameterVals.Any())
            {
                foreach (var value in ParameterVals)
                {
                    object[] parameters = { value };
                    DeriveParameters.Invoke("", new object[] { command } );
                    Parameters.SetParameterValues(command, parameters);
                    command.ExecuteNonQuery();
                }
            }

            connection.Close();
        }

        ///<summary>
        ///Builds a command and calls .ExecuteScalar to execute the query.
        ///</summary>
        ///<param name="CommandString">The text content of the query to be run.</param>
        public static object ExecuteScalarText(string ConnectionName, string CommandString, params object[] ParameterVals)
        {
            return ExScalar(ConnectionName, CommandString, Properties.Settings.Default.TimeoutSeconds, CommandType.Text, ParameterVals);
        }

        ///<summary>
        ///Builds a command and calls .ExecuteScalar to execute the query.
        ///</summary>
        ///<param name="CommandString">The text content of the query to be run.</param>
        public static object ExecuteScalarText(string ConnectionName, int TimeOutSeconds, string CommandString, params object[] ParameterVals)
        {
            return ExScalar(ConnectionName, CommandString, TimeOutSeconds, CommandType.Text, ParameterVals);
        }

        ///<summary>
        ///Builds a command and calls .ExecuteScalar to execute the query.
        ///</summary>
        ///<param name="Procedure">The name of the stored procedure to be executed.</param>
        public static object ExecuteScalar(string ConnectionName, string Procedure, params object[] ParameterVals)
        {
            return ExScalar(ConnectionName, Procedure, Properties.Settings.Default.TimeoutSeconds, CommandType.StoredProcedure, ParameterVals);
        }

        ///<summary>
        ///Builds a command and calls .ExecuteScalar to execute the query.
        ///</summary>
        ///<param name="Procedure">The name of the stored procedure to be executed.</param>
        public static object ExecuteScalar(string ConnectionName, int TimeOutSeconds, string Procedure, IEnumerable<object> ParameterVals)
        {
            return ExScalar(ConnectionName, Procedure, TimeOutSeconds, CommandType.StoredProcedure, ParameterVals);
        }

        private static object ExScalar(string ConnectionName, string Command, int TimeOutSeconds, CommandType type, IEnumerable<object> Vals)
        {
            var connection = Router.NewConnection(ConnectionName);
            connection.Open();

            IDbCommand command = Router.NewCommand(connection, type, TimeOutSeconds, Command, Vals);

            object Obj = command.ExecuteScalar();
            connection.Close();
            return Obj;
        }

        internal static Parameters Execute(ref IDbCommand command, IEnumerable<object> ItemArray)
        {
            var output = new Parameters();
            if (command.Parameters.Count > 0)
                output = Parameters.SetParameterValues(command, ItemArray);

            command.ExecuteNonQuery();
            return output;
        }

        internal static void ExecuteCommand(ref IDbCommand cmd, IDbConnection connection)
        {
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}
