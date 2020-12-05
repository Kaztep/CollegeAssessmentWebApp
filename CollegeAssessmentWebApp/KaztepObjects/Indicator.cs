using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class Indicator : DataObject
    {
        public int OutcomeID { get; set; }
        public List<Assignment> Assignments { get; set; }

        public override DataObject GetFromReader(SqlDataReader reader)
        {
            Indicator indicator = new Indicator()
            {
                ID = reader.GetInt32(0),
                OutcomeID = reader.GetInt32(1),
                Name = reader.GetString(2),
                DateCreated = reader.GetDateTime(3)
            };

            return indicator;
        }
    }
}
