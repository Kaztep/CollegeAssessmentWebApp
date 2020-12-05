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
                ID = reader.GetInt32(0),
                IndicatorID = reader.GetInt32(1),
                Name = reader.GetString(2),
                Level = Convert.ToChar(reader.GetString(3)),
                Course = reader.GetString(4),
                DateCreated = reader.GetDateTime(5)
            };

            return assignment;
        }
    }
}