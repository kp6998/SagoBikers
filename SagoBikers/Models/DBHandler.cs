using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace SagoBikers.Models
{
    public class DBHandler
    {
        public static string Connection(string strType)
        {
            string strtype = string.Empty;
            string strConnectionString = string.Empty;
            switch (strType)
            {
                case "A":
                    strConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString() != null ? ConfigurationManager.AppSettings["ConnectionString"].ToString() : "";
                    break;
                case "L":
                    strConnectionString = ConfigurationManager.AppSettings["LogConnectionString"].ToString() != null ? ConfigurationManager.AppSettings["LogConnectionString"].ToString() : "";
                    break;
            }
            return strConnectionString;
        }
        public static DataSet ExecProcedureReturnsDataset(string ProcedureName, Hashtable Parameters, ref string strErrorMsg)
        {
            SqlConnection my_connection = null;
            DataSet my_dataset = new DataSet();
            string my_procedure = ProcedureName;
            try
            {
                my_connection = new SqlConnection(Connection("A"));
                SqlCommand my_command = my_connection.CreateCommand();
                my_command.CommandText = ProcedureName;
                my_command.CommandType = CommandType.StoredProcedure;
                my_command.CommandTimeout = 0;
                AssignParameters(my_command, Parameters);
                SqlDataAdapter my_adapter = new SqlDataAdapter();
                my_adapter.SelectCommand = my_command;
                my_adapter.Fill(my_dataset, "Temp");
            }
            catch (Exception ex)
            {
                strErrorMsg = ex.Message.ToString();
            }
            finally
            {
                if (my_connection != null)
                {
                    my_connection.Close();
                }
            }
            return (my_dataset);
        }
        public static bool InsertUpdateDelete(string ProcedureName, Hashtable Parameters, ref string strErrorMsg, ref int ResultCount)
        {
            SqlConnection my_connection = null;
            try
            {
                my_connection = new SqlConnection(Connection("A"));
                SqlCommand my_command = my_connection.CreateCommand();
                my_command.CommandText = ProcedureName;
                my_command.CommandType = CommandType.StoredProcedure;
                my_command.CommandTimeout = 0;
                AssignParameters(my_command, Parameters);
                my_connection.Open();
                ResultCount = my_command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ResultCount = 0;
                strErrorMsg = ex.Message.ToString();
                return false;
            }
            finally
            {
                if (my_connection != null)
                {
                    my_connection.Close();
                }
            }
            return true;
        }

        public static void AssignParameters(SqlCommand Command, Hashtable Parameters)
        {
            if (Parameters == null) return;
            foreach (object key in Parameters.Keys)
            {
                Command.Parameters.Add(("@" + key.ToString().ToUpper()), Parameters[key]);
            }
        }

        public class StoreProcedure
        {
            public const string P_LOGIN_DETAILS = "P_LOGIN_DETAILS";
            public const string P_POSTS = "P_POSTS";
            public const string P_MANAGE_POSTS = "P_MANAGE_POSTS";
            public const string P_MANAGE_LIKES_COMMENTS = "P_MANAGE_LIKES_COMMENTS";
            public const string P_PROFILE = "P_PROFILE";
            public const string P_MANAGE_BIKES = "P_MANAGE_BIKES";
            public const string P_SEARCH_USERS = "P_SEARCH_USERS";
            public const string P_NOTIFICATIONS = "P_NOTIFICATIONS";
            public const string P_MANAGE_CLUBS = "P_MANAGE_CLUBS";
            public const string P_CLUBS_ACTIVITIES = "P_CLUBS_ACTIVITIES";
        }
    }
}