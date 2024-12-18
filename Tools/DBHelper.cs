﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace fyp
{
    public class DBHelper
    {
        private static string defaultConnectionString = ConfigurationManager.AppSettings["conn"];
        public static string DefaultConnectionString
        {
            get
            {
                return defaultConnectionString;
            }
        }

        public static DataTable ExecuteProcedure(string PROC_NAME, params object[] parameters)
        {
            try
            {
                if (parameters.Length % 2 != 0)
                    throw new ArgumentException("Wrong number of parameters sent to procedure. Expected an even number.");
                DataTable a = new DataTable();
                List<SqlParameter> filters = new List<SqlParameter>();

                string query = "EXEC " + PROC_NAME;

                bool first = true;
                for (int i = 0; i < parameters.Length; i += 2)
                {
                    filters.Add(new SqlParameter(parameters[i] as string, parameters[i + 1]));
                    query += (first ? " " : ", ") + ((string)parameters[i]);
                    first = false;
                }

                a = Query(query, filters);
                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable ExecuteQuery(string query, params object[] parameters)
        {
            try
            {
                DataTable a = new DataTable();

                List<SqlParameter> filters = new List<SqlParameter>();
                //SqlParameter filters = new SqlParameter();

                if (parameters != null)
                {
                    if (parameters.Length % 2 != 0)
                        throw new ArgumentException("Wrong number of parameters sent to procedure. Expected an even number.");
                    for (int i = 0; i < parameters.Length; i += 2)
                        filters.Add(new SqlParameter(parameters[i] as string, parameters[i + 1]));
                    a = Query(query, filters);
                }
                else
                {
                    a = Query(query, null);
                }

                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static int ExecuteNonQuery(string query, params object[] parameters)
        {
            try
            {
                if (parameters.Length % 2 != 0)
                    throw new ArgumentException("Wrong number of parameters sent to procedure. Expected an even number.");
                List<SqlParameter> filters = new List<SqlParameter>();

                for (int i = 0; i < parameters.Length; i += 2)
                    filters.Add(new SqlParameter(parameters[i] as string, parameters[i + 1]));
                return NonQuery(query, filters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static object ExecuteScalar(string query, params object[] parameters)
        {
            try
            {
                if (parameters != null)
                {
                    if (parameters.Length % 2 != 0)
                        throw new ArgumentException("Wrong number of parameters sent to query. Expected an even number.");
                    List<SqlParameter> filters = new List<SqlParameter>();

                    for (int i = 0; i < parameters.Length; i += 2)
                        filters.Add(new SqlParameter(parameters[i] as string, parameters[i + 1]));
                    return Scalar(query, filters);
                }
                else
                {
                    return Scalar(query, null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Private Methods

        public static DataTable Query(String consulta, List<SqlParameter> parametros)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConnection connection = new SqlConnection(defaultConnectionString);
                SqlCommand command = new SqlCommand();
                SqlDataAdapter da;

                try
                {
                    command.Connection = connection;
                    command.CommandText = consulta;
                    if (parametros != null)
                    {
                        for (int i = 0; i < parametros.Count; i += 1)
                            command.Parameters.Add(parametros[i]);
                    }
                    da = new SqlDataAdapter(command);
                    da.Fill(dt);
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }
                return dt;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static int NonQuery(string query, IList<SqlParameter> parametros)
        {
            try
            {
                DataSet dt = new DataSet();
                SqlConnection connection = new SqlConnection(defaultConnectionString);
                SqlCommand command = new SqlCommand();

                try
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = query;
                    command.Parameters.AddRange(parametros.ToArray());
                    return command.ExecuteNonQuery();

                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static object Scalar(string query, List<SqlParameter> parametros)
        {
            try
            {
                DataSet dt = new DataSet();
                SqlConnection connection = new SqlConnection(defaultConnectionString);
                SqlCommand command = new SqlCommand();

                try
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = query;
                    if (parametros != null)
                    {
                        command.Parameters.AddRange(parametros.ToArray());
                    }
                    return command.ExecuteScalar();
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
