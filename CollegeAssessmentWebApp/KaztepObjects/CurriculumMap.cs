using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class CurriculumMap : DataObject
    {
        public string Year { get; set; }
        /// <summary>
        /// Program courses delimited by comma
        /// Maybe change this to a List<string>
        /// </summary>
        public string ProgramCourses { get; set; }
        
        /// <summary>
        /// Indicators are in the Outcome objects
        /// </summary>
        public List<Outcome> Outcomes { get; set; }

        public override DataObject GetFromReader(SqlDataReader reader)
        {
            CurriculumMap curriculumMap = new CurriculumMap()
            {
                Year = reader.GetString(0),
                ProgramCourses = reader.GetString(1),
                ID = reader.GetInt32(2),
                Name = reader.GetString(3),
                DateCreated = reader.GetDateTime(4)
            };

            return curriculumMap;
        }
    }
}
