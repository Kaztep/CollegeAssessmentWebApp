using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class Outcome : DataObject
    {
        public int CurriculumMapID { get; set; }

        /// <summary>
        /// Assignments are in the Indicator objects
        /// </summary>
        public List<Indicator> Indicators { get; set; }

        public override DataObject GetFromReader(SqlDataReader reader)
        {
            Outcome outcome = new Outcome()
            {
                ID = reader.GetInt32(0),
                CurriculumMapID = reader.GetInt32(1),
                Name = reader.GetString(2),
                DateCreated = reader.GetDateTime(3)
            };

            return outcome;
        }
    }
}
