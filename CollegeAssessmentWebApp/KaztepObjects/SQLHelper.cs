using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;

namespace CollegeAssessmentWebApp
{
    public class SQLHelper
    {
        public static string ConnectionString = "Data Source=GRR-PETZAK;Initial Catalog=Record;Integrated Security=True";

        public static void ExecuteNonQuery(string dbString, string query)
        {
            using (SqlConnection connection = new SqlConnection(dbString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                    command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void CreateTable(DataObject dataObject)
        {
            string query = $"CREATE TABLE {dataObject.TableName}";
            // TODO: finish
            ExecuteNonQuery(ConnectionString, query);
        }

        public static void CreateTable(string tableName, Dictionary<string, string> properties)
        {
            string query = $"CREATE TABLE {tableName} (";

            foreach (KeyValuePair<string, string> property in properties)
            {
                query += property.Key + " ";

                if (property.Value == "string")
                    query += "VARCHAR(255)";
                else if (property.Value == "string(max)")
                    query += "VARCHAR(MAX)";
                else if (property.Value == "date")
                    query += "DATE";
                else if (property.Value == "datetime")
                    query += "DATETIME";
                else if (property.Value == "bool")
                    query += "BIT";
                else if (property.Value == "decimal")
                    query += "DECIMAL";
                else if (property.Value == "int")
                    query += "INT";
                query += ", ";
            }

            query = query.TrimEnd().TrimEnd(',');
            query += ");";

            ExecuteNonQuery(ConnectionString, query);
        }

        private static int GetNextNumber(string tableName, string column = "ID")
        {
            int i = 0;
            string sqlQuery = $"SELECT {column} from {tableName}";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int val = reader.GetInt32(0);
                            if (val > i)
                                i = val;
                        }
                    }
                }
                connection.Close();
            }

            return i;
        }

        /// <summary>
        /// Load all DataObjects from a table.
        /// </summary>
        /// <param name="dbo">An instance of the type of DBObject to load</param>
        public static List<DataObject> LoadAll(DataObject dataObject)
        {
            string queryString = "SELECT * FROM " + dataObject.TableName;
            return LoadList(dataObject, queryString);
        }

        public static List<DataObject> LoadWhere(DataObject dataObject, string column, string val)
        {
            string sqlQuery = $"SELECT * FROM {dataObject.TableName} WHERE {column} = '{val}'";
            return LoadList(dataObject, sqlQuery);
        }

        public static List<DataObject> LoadList(DataObject dataObject, string sqlQuery)
        {
            var objects = new List<DataObject>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            objects.Add(dataObject.GetFromReader(reader));
                    }
                }
                connection.Close();
            }
            return objects;
        }

        /// <summary>
        /// Update every field on the DBObject except for the ID
        /// </summary>
        public static void Update(DataObject dataObject)
        {
            DoubleApostraphies(dataObject);
            string sql = GetUpdateStatement(dataObject);
            ExecuteNonQuery(ConnectionString, sql);
        }

        /// <summary>
        /// Returns the SQL statement for updating a DBObject
        /// </summary>
        /// <param name="dataObject">Object to update</param>
        private static string GetUpdateStatement(DataObject dataObject)
        {
            Type t = dataObject.GetType();
            List<string> columns = GetColumns(dataObject.TableName);
            columns.Remove("ID");

            string updateStatement = $"UPDATE {dataObject.TableName} SET ";

            string values = "";
            foreach (string col in columns)
                values += (col + $" = '{t.GetProperty(col).GetValue(dataObject)}', ");

            string whereClause = $"WHERE ID = '{t.GetProperty("ID").GetValue(dataObject)}'";

            return $"{updateStatement}{values}{whereClause}";
        }

        /// <summary>
        /// Insert a list of DBObjects to the database (must all have same type)
        /// </summary>
        public static void Insert(List<DataObject> objects)
        {
            if (objects == null || objects.Count == 0)
                return;

            // TODO: Check for and/or create table

            List<string> columns = GetColumns(objects[0].TableName);
            string insertStatement = GetInsertStatement(objects[0].TableName, columns);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                foreach (DataObject o in objects)
                {
                    string values = "VALUES (";
                    foreach (string col in columns)
                        values = GetValueStatement(o, col, values);
                    string sql = String.Format("{0}{1})", insertStatement, values.TrimEnd().TrimEnd(','));
                    using (SqlCommand command = new SqlCommand(sql, connection))
                        command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Returns INSERT statement with the columns specified
        /// </summary>
        private static string GetInsertStatement(string table, List<string> columns)
        {
            string s = "INSERT INTO " + table + " (";
            foreach (string c in columns)
                s += c + ", ";
            return s = s.Remove(s.Length - 2) + ") ";
        }

        /// <summary>
        /// Appends the value of the specified column to the VALUES statement
        /// </summary>
        private static string GetValueStatement(DataObject dataObject, string col, string values)
        {
            Type t = dataObject.GetType();
            PropertyInfo pi = t.GetProperty(col);

            if (pi.GetValue(dataObject) == null)
                return values += "'', ";

            string val = pi.GetValue(dataObject).ToString();
            string prop = pi.ToString();

            if (prop.Contains("DateTime"))
                val = val.Split(' ')[0]; // Remove time from date

            if (prop.Contains("String") || prop.Contains("DateTime"))
                values += "'" + val.Replace("'", "''") + "', ";
            else if (prop.Contains("Double") || prop.Contains("Int32") || prop.Contains("Decimal"))
                values += val + ", ";
            else if (prop.Contains("Boolean"))
                values += (val == "False" ? "0" : "1") + ", ";

            return values;
        }

        /// <summary>
        /// Returns a list of all columns names from a table
        /// </summary>
        public static List<string> GetColumns(string tableName)
        {
            List<string> columns = new List<string>();
            string sql = "SELECT * FROM " + tableName;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            columns.Add(reader.GetName(i));
                    }
                }
                connection.Close();
            }
            return columns;
        }

        /// <summary>
        /// Doubles up the single quotes contained in every string field on the object.
        /// So the sql statement doesn't brake.
        /// </summary>
        public static void DoubleApostraphies(DataObject dataObject)
        {
            PropertyInfo[] properties = dataObject.GetType().GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                if (pi.ToString().Contains("String") && pi.GetSetMethod() != null)
                    pi.SetValue(dataObject, pi.GetValue(dataObject)?.ToString().Replace("'", "''"));
            }
        }
    }
}
