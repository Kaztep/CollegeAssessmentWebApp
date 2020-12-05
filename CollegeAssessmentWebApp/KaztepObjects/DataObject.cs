using System;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class DataObject
    {
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
            return new DataObject();
        }
    }
}