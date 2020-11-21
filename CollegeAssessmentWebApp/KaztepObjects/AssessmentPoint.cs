using System;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    /// <summary>
    /// Gathered from the Data Collection tab
    /// </summary>
    public class AssessmentPoint : DataObject
    {
        public int IndicatorID;
        public int NumberAssessed;
        public int NumberPassing;
        public int PassRate;

        public override DataObject GetFromReader(SqlDataReader reader)
        {
            return base.GetFromReader(reader);
        }
    }
}