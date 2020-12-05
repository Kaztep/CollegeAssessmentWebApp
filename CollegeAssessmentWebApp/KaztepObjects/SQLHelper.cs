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
        public static string ConnectionString = "Data Source=GRR-PETZAK;Initial Catalog=CollegeWebAssessmentApp;Integrated Security=True";

        public static int ExecuteNonQuery(string dbString, string query)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(dbString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                    rowsAffected = command.ExecuteNonQuery();
                connection.Close();
            }
            return rowsAffected;
        }

        public static void CreateTable(DataObject dataObject)
        {
            string query = $"CREATE TABLE {dataObject.TableName} (";

            var properties = dataObject.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
                // Skip over TableName and List properties
                if (pi.Name == "TableName" || pi.PropertyType.FullName.StartsWith("System.Collections.Generic.List"))
                    continue;

                query += pi.Name + " ";

                Type type = pi.PropertyType;

                if (type == typeof(string))
                    query += "VARCHAR(500)";
                else if (type == typeof(DateTime))
                    query += "DATETIME";
                else if (type == typeof(bool))
                    query += "BIT";
                else if (type == typeof(decimal))
                    query += "DECIMAL";
                else if (type == typeof(int))
                    query += "INT";
                else if (type == typeof(char))
                    query += "VARCHAR(1)";
                query += ", ";
            }

            query = query.TrimEnd().TrimEnd(',');
            query += ");";

            if (TableExists(dataObject.TableName))
                DropTable(dataObject.TableName);

            ExecuteNonQuery(ConnectionString, query);
        }

        private static bool TableExists(string tableName)
        {
            // Query returns 1 if table exists, otherwise 0
            string sqlQuery = "IF EXISTS (SELECT 1 " +
                              "FROM INFORMATION_SCHEMA.TABLES " +
                              "WHERE TABLE_TYPE='BASE TABLE' " +
                             $"AND TABLE_NAME='{tableName}') " +
                              "BEGIN SELECT 1 END " +
                              "ELSE BEGIN SELECT 0 END";

            bool exists = false;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            exists = reader.GetInt32(0) == 1;
                    }
                }
                connection.Close();
            }

            return exists;
        }

        private static void DropTable(string tableName)
        {
            string sqlQuery = $"DROP TABLE {tableName};";
            ExecuteNonQuery(ConnectionString, sqlQuery);
        }

        private static int ClearTable(string tableName)
        {
            string sqlQuery = $"DELETE FROM {tableName};";
            return ExecuteNonQuery(ConnectionString, sqlQuery);
        }

        private static int GetNextNumber(string tableName, string column = "ID")
        {
            int i = -1;
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

            return i + 1;
        }

        /// <summary>
        /// Load all DataObjects from a table.
        /// </summary>
        /// <param name="dbo">An instance of the type of DataObject to load</param>
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
        /// Update every field on the DataObject except for the ID
        /// </summary>
        public static void Update(DataObject dataObject)
        {
            DoubleApostraphies(dataObject);
            string sql = GetUpdateStatement(dataObject);
            ExecuteNonQuery(ConnectionString, sql);
        }

        /// <summary>
        /// Returns the SQL statement for updating a DataObject
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
        /// Insert a list of DataObject to the database (must all have same type)
        /// </summary>
        public static void Insert(List<DataObject> objects)
        {
            if (objects == null || objects.Count == 0)
                return;

            string tableName = objects[0].TableName;

            if (!TableExists(tableName))
                CreateTable(objects[0]);

            List<string> columns = GetColumns(tableName);
            string insertStatement = GetInsertStatement(tableName, columns);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                foreach (DataObject o in objects)
                {
                    o.DateCreated = DateTime.Now;
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
        /// Return the Type of a List<> Property in a DataObject
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static Type GetChildListType(DataObject o)
        {
            var properties = o.GetType().GetProperties();

            foreach (PropertyInfo pi in properties)
            {
                var fullName = pi.PropertyType.FullName;

                if (fullName.StartsWith("System.Collections.Generic.List"))
                {
                    string className = fullName.Split('[')[2].Split(',')[0].ToString();
                    return Type.GetType(className);
                }
            }

            return null;
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
            Type type = pi.PropertyType;

            // Remove time from date
            //if (prop.Contains("DateTime"))
            //    val = val.Split(' ')[0];

            if (type == typeof(string) || type == typeof(char) || type == typeof(DateTime))
                values += "'" + val.Replace("'", "''") + "', ";
            else if (type == typeof(Int32) || type == typeof(double) || type == typeof(decimal))
                values += val + ", ";
            else if (type == typeof(bool))
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

        #region Helper methods for Curriculum maps and all associated child objects

        public static void InsertAll(List<DataObject> curriculumMaps)
        {
            SetIDs(curriculumMaps);

            var outcomes = curriculumMaps.SelectMany(o => (o as CurriculumMap).Outcomes).ToList().OfType<DataObject>().ToList();
            var indicators = outcomes.SelectMany(o => (o as Outcome).Indicators).ToList().OfType<DataObject>().ToList();
            var assignments = indicators.SelectMany(o => (o as Indicator).Assignments).ToList().OfType<DataObject>().ToList();

            Insert(curriculumMaps);
            Insert(outcomes);
            Insert(indicators);
            Insert(assignments);
        }

        public static void CreateTables()
        {
            CreateTable(new CurriculumMap());
            CreateTable(new Outcome());
            CreateTable(new Indicator());
            CreateTable(new Assignment());
        }

        public static void ClearTables()
        {
            ClearTable("CurriculumMap");
            ClearTable("Outcome");
            ClearTable("Indicator");
            ClearTable("Assignment");
        }


        public static void SetIDs(List<DataObject> objects)
        {
            if (objects == null || objects.Count == 0 || objects[0].GetType() != typeof(CurriculumMap))
                return;

            int nextCmID = GetNextNumber(objects[0].TableName);
            int nextOutcomeID = GetNextNumber("Outcome");
            int nextIndicatorID = GetNextNumber("Indicator");
            int nextAssignmentID = GetNextNumber("Assignment");

            foreach (CurriculumMap cm in objects)
            {
                cm.ID = nextCmID++;

                foreach (Outcome o in cm.Outcomes)
                {
                    o.ID = nextOutcomeID++;
                    o.CurriculumMapID = cm.ID;

                    foreach (Indicator i in o.Indicators)
                    {
                        i.ID = nextIndicatorID++;
                        i.OutcomeID = o.ID;

                        foreach (Assignment a in i.Assignments)
                        {
                            a.ID = nextAssignmentID++;
                            a.IndicatorID = i.ID;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
