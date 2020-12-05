using System;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class Assignment : DataObject
    {
        public int IndicatorID { get; set; }
        // I, R, or S
        public char Level { get; set; }
        public string Course { get; set; }

        public override DataObject GetFromReader(SqlDataReader reader)
        {
            Assignment assignment = new Assignment()
            {
                IndicatorID = reader.GetInt32(0),
                Level = Convert.ToChar(reader.GetString(1)),
                Course = reader.GetString(2),
                ID = reader.GetInt32(3),
                Name = reader.GetString(4),
                DateCreated = reader.GetDateTime(5)
            };

            return assignment;
        }
    }
}