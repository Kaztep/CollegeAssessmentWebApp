using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Creates a table for a DataObject. All non-list properties are dynamically included as columns in the table.
        /// </summary>
        public static void CreateTable(DataObject dataObject)
        {
            string query = $"CREATE TABLE {dataObject.TableName} (";

            var properties = dataObject.GetType().GetProperties().ToList();

            // Don't create columns for TableName or List properties
            properties.RemoveAll(p => p.Name == "TableName" || p.PropertyType.FullName.StartsWith("System.Collections.Generic.List"));

            ReorderPropertyInfoList(properties);

            foreach (PropertyInfo pi in properties)
            {
                query += pi.Name + " ";

                Type type = pi.PropertyType;

                // TODO: Mark columns as Primary/Foreign Keys

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

            query = query.TrimEnd().TrimEnd(',') + ");";

            if (TableExists(dataObject.TableName))
                DropTable(dataObject.TableName);

            ExecuteNonQuery(ConnectionString, query);
        }

        /// <summary>
        /// Reorders list of properties to desired column order.
        /// ID > Foreign IDs > Name > anything else > DateCreated
        /// </summary>
        private static void ReorderPropertyInfoList(List<PropertyInfo> properties)
        {
            // Swap ID first
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (properties[i].Name == "ID")
                {
                    var temp = properties[i];
                    properties.RemoveAt(i);
                    properties.Insert(0, temp);
                    break;
                }
            }

            // Swap Foreign IDs
            int nameID = 1;
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (properties[i].Name != "ID" && properties[i].Name.Contains("ID"))
                {
                    var temp = properties[i];
                    properties.RemoveAt(i);
                    properties.Insert(1, temp);
                    nameID++;
                }
            }

            // Swap Name
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (properties[i].Name == "Name")
                {
                    var temp = properties[i];
                    properties.RemoveAt(i);
                    properties.Insert(nameID, temp);
                }
            }
        }

        /// <summary>
        /// Returns true if the table exists
        /// </summary>
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

        /// <summary>
        /// Returns the (highest number + 1) in a specified column. Defaults to ID column
        /// </summary>
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
                        // TODO: Figure out how to do this without using an instance of the DataObject
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
            string sqlQuery = GetUpdateStatement(dataObject);
            ExecuteNonQuery(ConnectionString, sqlQuery);
        }

        /// <summary>
        /// Returns the SQL statement for updating a DataObject
        /// </summary>
        private static string GetUpdateStatement(DataObject dataObject)
        {
            Type t = dataObject.GetType();
            List<string> columns = GetColumns(dataObject.TableName);

            // Don't update ID
            if (columns.Contains("ID"))
                columns.Remove("ID");

            string updateStatement = $"UPDATE {dataObject.TableName} SET ";

            string values = "";
            foreach (string col in columns)
                values += $"{col} = '{t.GetProperty(col).GetValue(dataObject)}', ";

            // Remove the trailing comma
            values = values.Remove(values.Length - 2, 1);

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
                    string sqlQuery = String.Format("{0}{1})", insertStatement, values.TrimEnd().TrimEnd(','));
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Return the Type of a List<> Property in a DataObject
        /// </summary>
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
        private static string GetInsertStatement(string tableName, List<string> columns)
        {
            string s = $"INSERT INTO {tableName} (";
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
            string sqlQuery = "SELECT * FROM " + tableName;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
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
        /// So the sql statement doesn't brake. Execute this before updating an object.
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

        /// <summary>
        /// Create the tables for each DataObject
        /// </summary>
        public static void CreateTables()
        {
            CreateTable(new CurriculumMap());
            CreateTable(new Outcome());
            CreateTable(new Indicator());
            CreateTable(new Assignment());
            CreateTable(new AssessmentPoint());
        }

        /// <summary>
        /// Clear the tables for each DataObject
        /// </summary>
        public static void ClearTables()
        {
            ClearTable("CurriculumMap");
            ClearTable("Outcome");
            ClearTable("Indicator");
            ClearTable("Assignment");
            ClearTable("AssessmentPoint");
        }


        /// <summary>
        /// Insert all data inside a list of CurriculumMaps
        /// </summary>
        public static void InsertAll(List<DataObject> curriculumMaps)
        {
            SetIDs(curriculumMaps);

            // Extract the collections of child objects from the CurriculumMaps
            var outcomes = curriculumMaps.SelectMany(o => (o as CurriculumMap).Outcomes).ToList().OfType<DataObject>().ToList();
            var indicators = outcomes.SelectMany(o => (o as Outcome).Indicators).ToList().OfType<DataObject>().ToList();
            var assignments = indicators.SelectMany(o => (o as Indicator).Assignments).ToList().OfType<DataObject>().ToList();

            // Insert all the collections
            Insert(curriculumMaps);
            Insert(outcomes);
            Insert(indicators);
            Insert(assignments);
        }

        /// <summary>
        /// Set the IDs on a list of Curriculum Maps and all child objects. Execute this before inserting into DB
        /// </summary>
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

        /// <summary>
        /// Returns a list of all CurriculumMaps from the database
        /// </summary>
        public static List<DataObject> LoadAllMaps()
        {
            // Load objects from each table
            var maps = LoadAll(new CurriculumMap());
            var outcomes = LoadAll(new Outcome());
            var indicators = LoadAll(new Indicator());
            var assignments = LoadAll(new Assignment());

            // Add lists to the parent objects
            // TODO: Find a better way to load these using SQL Joins on the IDs
            foreach (Indicator i in indicators)
                i.Assignments = assignments.Where(a => (a as Assignment).IndicatorID == i.ID).ToList().OfType<Assignment>().ToList();

            foreach (Outcome o in outcomes)
                o.Indicators = indicators.Where(i => (i as Indicator).OutcomeID == o.ID).ToList().OfType<Indicator>().ToList();

            foreach (CurriculumMap map in maps)
                map.Outcomes = outcomes.Where(o => (o as Outcome).CurriculumMapID == map.ID).ToList().OfType<Outcome>().ToList();

            return maps;
        }

        public static List<DataObject> LoadAllPoints()
        {
            var points = LoadAll(new AssessmentPoint());

            return points;
        }

        #endregion
    }
}
