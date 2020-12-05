using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class CurriculumMap : DataObject
    {
        public string FileName { get; set; }
        public string Year { get; set; }
        /// <summary>
        /// Program courses delimited by comma. 
        /// Change this to a List<string> if/when SQLHelper support is added.
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
                ID = reader.GetInt32(0),
                Name = reader.GetString(1),
                FileName = reader.GetString(2),
                Year = reader.GetString(3),
                ProgramCourses = reader.GetString(4),
                DateCreated = reader.GetDateTime(5)
            };

            return curriculumMap;
        }
    }
}
