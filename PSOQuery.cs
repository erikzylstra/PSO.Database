using System.Collections.Generic;
using System.Data;

namespace PSO.Database
{
    public class PSOQuery
    {
        /// <summary>
        /// The name of the connection string to use. 
        /// The router class will search the app.config for the corresponding string.
        /// </summary>
        public string ConnectionName { get; set; } = "";

        /// <summary>
        /// Either the text of the command to execute or the name of the stored procedure to execute.
        /// </summary>
        public string CommandString { get; set; } = "";

        /// <summary>
        /// Default value of IDbCommand execution timeout is 30s.
        /// </summary>
        public int TimeOutSeconds { get; set; } = 30;

        /// <summary>
        /// The values of the parameters passed to a stored procedure. 
        /// The order of values must match the order of the parameters specified in the procedure.
        /// </summary>
        public List<object> ParameterValues { get; set; } = new List<object>();

        public CommandType CommandType { get; set; } = CommandType.StoredProcedure;


        #region Constructors

        public PSOQuery(string connectionName, string commandString)
        {
            ConnectionName = connectionName;
            CommandString = commandString;
        }

        public PSOQuery(string connectionName, string commandString, IEnumerable<object> parameterValues) : this(connectionName, commandString)
        {
            ParameterValues.AddRange(parameterValues);
        }

        public PSOQuery(string connectionName, string commandString, CommandType type) : this(connectionName, commandString)
        {
            CommandType = type;
        }

        public PSOQuery(string connectionName, string commandString, CommandType type, IEnumerable<object> parameterValues) : this(connectionName, commandString, parameterValues)
        {
            CommandType = type;
        }

        public PSOQuery(string connectionName, string commandString, int timeOutSeconds, CommandType type, IEnumerable<object> parameterValues) : this(connectionName, commandString, type, parameterValues)
        {
            TimeOutSeconds = timeOutSeconds;
        }

        #endregion
    }
}
