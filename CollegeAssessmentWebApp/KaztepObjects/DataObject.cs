using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class DataObject
    {
        public int ID;
        public string Name;

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