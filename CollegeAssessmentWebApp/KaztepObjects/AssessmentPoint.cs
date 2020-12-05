using System;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    /// <summary>
    /// Gathered from the Data Collection tab.
    /// </summary>
    public class AssessmentPoint : DataObject
    {
        public int CurriculumMapID { get; set; }
        public string Outcome { get; set; }
        public string Indicator { get; set; }
        public string Assignment { get; set; }
        public string Year { get; set; }
        public int NumberAssessed { get; set; }
        public int NumberPassing { get; set; }
        public int PassRate { get; set; }

        public override DataObject GetFromReader(SqlDataReader reader)
        {
            AssessmentPoint point = new AssessmentPoint()
            {
                ID = reader.GetInt32(0),
                CurriculumMapID = reader.GetInt32(1),
                Name = reader.GetString(2),
                Outcome = reader.GetString(3),
                Indicator = reader.GetString(4),
                Assignment = reader.GetString(5),
                Year = reader.GetString(6),
                NumberAssessed = reader.GetInt32(7),
                NumberPassing = reader.GetInt32(8),
                PassRate = reader.GetInt32(9),
                DateCreated = reader.GetDateTime(10)
            };

            return point;
        }
    }
}