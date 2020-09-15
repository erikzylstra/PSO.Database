using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;

namespace PSO.Database
{
    public class Parameters : List<IDbDataParameter>
    {
        #region Indexers
        public IDbDataParameter this[string paramName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(paramName))
                    throw new ArgumentNullException("Null or empty value passed to PSO.Database.Parameters indexer.");
                else if (this.Count == 0)
                    throw new InvalidOperationException("Cannot find parameter " + paramName + " in PSO.Database.Parameters instance because it is an empty sequence.");

                return FindParameterByName(paramName);
            }
            set
            {
                var param = FindParameterByName(paramName);
                param = value;
            }
        }


        /// <param name="paramName">The name of the parameter to find.</param>
        /// <param name="T">The type of the value of the parameter.
        /// An Exception is thrown if the value is not of type T.</param>
        public IDbDataParameter this[string paramName, Type T]
        {
            get
            {
                var param = this[paramName];
                if (param.Value.GetType() != T)
                    throw new ArgumentException("The value of the parameter " + paramName + " is not of type " + T + ".");
                return param;
            }
        }

        /// <param name="index">The index of the parameter to return.</param>
        /// <param name="T">The value of the parameter found must be of type T or an exception is thrown</param>
        public IDbDataParameter this[int index, Type T]
        {
            get
            {
                var param = this[index];
                if (param?.Value == null & T.IsValueType)
                    throw new NullReferenceException("The value of the PSO.Database parameter at index " + index + " was null, but a value type was requested.");
                if (param.Value.GetType() != T)
                    throw new ArgumentException("The value of the parameter at index " + index + " is not of type " + T + ".");
                return param;
            }
        }

        #endregion

        ///<summary>
        ///Assigns values to input parameters. The order of input values must match the order of the parameters of the command.
        ///Output parameters are assigned values by the stored procedure and then returned to the user as a list.
        ///</summary>
        public static Parameters SetParameterValues(IDbCommand command, IEnumerable<object> input)
        {
            var InputOutput = (from IDbDataParameter parm in command.Parameters
                              where (parm.Direction == ParameterDirection.Input || parm.Direction == ParameterDirection.InputOutput)
                              select parm).ToList();

            var Input = (from IDbDataParameter parm in command.Parameters
                         where (parm.Direction == ParameterDirection.Input)
                         select parm).ToList();

            var Output = (from IDbDataParameter parm in command.Parameters
                            where (parm.Direction == ParameterDirection.Output || parm.Direction == ParameterDirection.InputOutput)
                            select parm).ToList();

            //First check if the values supplied match the number of strictly input parameters of the command. In this case if there are input/output parameters associated
            //with the command, they are assumed to be defined with a default value; otherwise an exception will be thrown.
            if (Input.Count() == input.Count())
            {
                int i = 0;
                foreach (var value in input)
                {
                    var val = (value == null) ? DBNull.Value : value; //assign DBNull value if null
                    Input[i].Value = val;
                    i++;
                }
            }
            //On the other hand if the input/output parameters are not given a default value, then they will need to have values
            //passed to the command.
            else if (InputOutput.Count() == input.Count())
            {
                int i = 0;
                foreach (var value in input)
                {
                    var val = (value == null) ? DBNull.Value : value; //assign DBNull value if null
                    InputOutput[i].Value = val;
                    i++;
                }
            }
            else
            {
                string msg = "Incorrect number of parameters passed to procedure " + command.CommandText;
                throw new ArgumentException(msg);
            }

            Parameters output = new Parameters();
            output.AddRange(Output);
            return output;
        }

        private IDbDataParameter FindParameterByName(string name)
        {
            IDbDataParameter param;
            try { param = this.Where(p => p.ParameterName.ToLower() == name.ToLower()).First(); }
            catch (InvalidOperationException ex) { throw new IndexOutOfRangeException("An attempt was made to access parameter " + name + " which cannot be found."); }
            return param;
        }

    }
}
