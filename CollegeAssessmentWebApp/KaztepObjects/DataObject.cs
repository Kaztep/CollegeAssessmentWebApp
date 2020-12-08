using System;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    /// <summary>
    /// Base class for Sql records.
    /// </summary>
    public abstract class DataObject
    {
        /// <summary>
        /// Primary key for all tables. 
        /// </summary>
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }

        public string TableName
        {
            get
            {
                return GetType().Name;
            }
        }

        public virtual DataObject GetFromReader(SqlDataReader reader)
        {
            return null;
        }
    }
}