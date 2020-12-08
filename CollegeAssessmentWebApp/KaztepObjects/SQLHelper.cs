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

        /// <summary>
        /// Returns an open SqlConnection
        /// </summary>
        private static SqlConnection GetConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

        public static int ExecuteNonQuery(string sqlQuery)
        {
            int rowsAffected = 0;
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    rowsAffected = command.ExecuteNonQuery();
            }
            return rowsAffected;
        }

        public static int TryExecuteNonQuery(string sqlQuery)
        {
            try
            {
                return ExecuteNonQuery(sqlQuery);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// Creates a table for a DataObject. All non-list properties are dynamically included as columns in the table.
        /// </summary>
        public static void CreateTable(DataObject dataObject)
        {
            string sqlQuery = $"CREATE TABLE {dataObject.TableName} (";

            var properties = dataObject.GetType().GetProperties().ToList();

            // Don't create columns for TableName or List properties
            properties.RemoveAll(p => p.Name == "TableName" || p.PropertyType.FullName.StartsWith("System.Collections.Generic.List"));

            ReorderPropertyInfoList(properties);

            foreach (PropertyInfo pi in properties)
                sqlQuery += GetColumnDataType(pi);

            sqlQuery = sqlQuery.TrimEnd().TrimEnd(',') + ");";

            if (TableExists(dataObject.TableName))
                DropTable(dataObject.TableName);

            ExecuteNonQuery(sqlQuery);
        }

        /// <summary>
        /// Returns the SQL column type associated with a DataObject's field type
        /// </summary>
        private static string GetColumnDataType(PropertyInfo propertyInfo)
        {
            Type type = propertyInfo.PropertyType;
            string propName = propertyInfo.Name;
            string dataType = String.Empty;

            // TODO: Add support for VARCHAR(x/MAX), NVARCHAR, Date (w/o time), Bytes, Image, etc.

            if (type == typeof(string))
                dataType = "VARCHAR(500)";
            else if (type == typeof(DateTime))
                dataType = "DATETIME";
            else if (type == typeof(bool))
                dataType = "BIT";
            else if (type == typeof(decimal))
                dataType = "DECIMAL";
            else if (type == typeof(double))
                dataType = "DECIMAL(18, 2)";
            else if (type == typeof(int))
                dataType = "INT";
            else if (type == typeof(char))
                dataType = "VARCHAR(1)";

            // Mark ID as primary key
            if (propName == "ID")
            {
                dataType += " NOT NULL PRIMARY KEY";
            }
            // Assume all other IDs are foreign keys (for now)
            else if (propName.EndsWith("ID"))
            {
                string foreignTable = propName.Remove(propName.Length - 2, 2);

                // Check if foreign ID column exists, then add FK reference
                if (TableExists(foreignTable) && GetColumns(foreignTable).Contains("ID"))
                    dataType += $" FOREIGN KEY REFERENCES {foreignTable}(ID)";
            }

            return $"{propName} {dataType},";
        }

        /// <summary>
        /// Reorders list of properties to desired column order.
        /// ID > Foreign IDs > Name > anything else > DateCreated
        /// </summary>
        private static void ReorderPropertyInfoList(List<PropertyInfo> properties)
        {
            // Move ID to front
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

            // Foreign IDs next
            int nameID = 1;
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (properties[i].Name != "ID" && properties[i].Name.EndsWith("ID"))
                {
                    var temp = properties[i];
                    properties.RemoveAt(i);
                    properties.Insert(1, temp);
                    nameID++;
                }
            }

            // Name next
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (properties[i].Name == "Name")
                {
                    var temp = properties[i];
                    properties.RemoveAt(i);
                    properties.Insert(nameID, temp);
                    break;
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
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            exists = reader.GetInt32(0) == 1;
                    }
                }
            }

            return exists;
        }

        private static void DropTable(string tableName)
        {
            DeleteTableConstraints(tableName);
            string sqlQuery = $"DROP TABLE {tableName};";
            ExecuteNonQuery(sqlQuery);
        }

        private static int ClearTable(string tableName)
        {
            string sqlQuery = $"DELETE FROM {tableName};";
            return ExecuteNonQuery(sqlQuery);
        }

        private static int DeleteWhere(string tableName, string column, string val)
        {
            string sqlQuery = $"DELETE FROM {tableName} WHERE {column} = '{val}';";
            return ExecuteNonQuery(sqlQuery);
        }

        public static void AddColumn(string tableName, string column, string columnType)
        {
            string sqlQuery = $"ALTER TABLE {tableName} ADD {column} {columnType};";
            ExecuteNonQuery(sqlQuery);
        }

        public static void DropColumn(string tableName, string column)
        {
            DeleteColumnRestraints(tableName, column);
            string sqlQuery = $"ALTER TABLE {tableName} DROP COLUMN {column};";
            ExecuteNonQuery(sqlQuery);
        }

        /// <summary>
        /// Find and delete foreign key references to all columns in a table
        /// </summary>
        private static void DeleteTableConstraints(string tableName)
        {
            var tables = new List<string>();
            var constraints = new List<string>();

            string sqlQuery = $"EXEC sp_fkeys '{tableName}'";

            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // FKTABLE_NAME column
                            tables.Add(reader.GetString(6));
                            // FK_NAME column
                            constraints.Add(reader.GetString(11));
                        }
                    }
                }
            }

            for (int i = 0; i < tables.Count; i++)
            {
                sqlQuery = $"ALTER TABLE {tables[i]} DROP CONSTRAINT {constraints[i]}";
                ExecuteNonQuery(sqlQuery);
            }
        }

        /// <summary>
        /// Delete all foreign key references on a column
        /// </summary>
        private static void DeleteColumnRestraints(string tableName, string column)
        {
            string sqlQuery = $"SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                              $"WHERE TABLE_NAME = '{tableName}' " +
                              $"AND COLUMN_NAME = '{column}' " +
                              $"AND CONSTRAINT_NAME LIKE 'FK%'";

            var constraints = new List<string>();
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            constraints.Add(reader.GetString(0));
                    }
                }
            }

            foreach (string constraint in constraints)
            {
                sqlQuery = $"ALTER TABLE {tableName} DROP CONSTRAINT {constraint}";
                ExecuteNonQuery(sqlQuery);
            }
        }

        /// <summary>
        /// Returns the (highest number + 1) in a specified column. Defaults to ID column
        /// </summary>
        private static int GetNextNumber(string tableName, string column = "ID")
        {
            int i = -1;
            string sqlQuery = $"SELECT {column} from {tableName}";

            using (SqlConnection connection = GetConnection())
            {
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
            }

            return i + 1;
        }

        /// <summary>
        /// Load all DataObjects from a table.
        /// </summary>
        /// <param name="dbo">An instance of the type of DataObject to load</param>
        public static List<DataObject> LoadAll(DataObject dataObject)
        {
            string sqlQuery = $"SELECT * FROM {dataObject.TableName}";
            return LoadList(dataObject, sqlQuery);
        }

        public static List<DataObject> LoadWhere(DataObject dataObject, string column, string val)
        {
            string sqlQuery = $"SELECT * FROM {dataObject.TableName} WHERE {column} = '{val}'";
            return LoadList(dataObject, sqlQuery);
        }

        public static List<DataObject> LoadList(DataObject dataObject, string sqlQuery)
        {
            var objects = new List<DataObject>();
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // TODO: Figure out how to do this without using an instance of the DataObject
                        while (reader.Read())
                            objects.Add(dataObject.GetFromReader(reader));
                    }
                }
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
            ExecuteNonQuery(sqlQuery);
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

            string values = String.Empty;
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

            using (SqlConnection connection = GetConnection())
            {
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
            }
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
        /// Appends the value of the specified column to the INSERT VALUES statement
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
            //if (type == typeof(DateTime))
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
            string sqlQuery = $"SELECT * FROM {tableName}";
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            columns.Add(reader.GetName(i));
                    }
                }
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
            var outcomes = curriculumMaps.SelectMany(o => (o as CurriculumMap).Outcomes).OfType<DataObject>().ToList();
            var indicators = outcomes.SelectMany(o => (o as Outcome).Indicators).OfType<DataObject>().ToList();
            var assignments = indicators.SelectMany(o => (o as Indicator).Assignments).OfType<DataObject>().ToList();

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

            int nextCmID = GetNextNumber("CurriculumMap");
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
            var outcomes = LoadAll(new Outcome()).OfType<Outcome>();
            var indicators = LoadAll(new Indicator()).OfType<Indicator>();
            var assignments = LoadAll(new Assignment()).OfType<Assignment>();

            // Add lists to the parent objects
            // TODO: Find a way to load these using SQL Joins on the IDs
            foreach (Indicator i in indicators)
                i.Assignments = assignments.Where(a => a.IndicatorID == i.ID).ToList();

            foreach (Outcome o in outcomes)
                o.Indicators = indicators.Where(i => i.OutcomeID == o.ID).ToList();

            foreach (CurriculumMap map in maps)
                map.Outcomes = outcomes.Where(o => o.CurriculumMapID == map.ID).ToList();

            return maps;
        }

        /* Select all rows and columns from Assignment where CurriculumMapID = 0 (Example)
         
            SELECT Assignment.* FROM Assignment
            JOIN Indicator as i on Assignment.IndicatorID = i.ID
            JOIN Outcome as o on i.OutcomeID = o.ID
            JOIN CurriculumMap as c on o.CurriculumMapID = c.ID
            WHERE c.ID = 0
         */

        public static List<DataObject> LoadAllPoints()
        {
            var points = LoadAll(new AssessmentPoint());

            return points;
        }

        #endregion
    }
}
