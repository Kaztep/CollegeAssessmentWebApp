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
    }
}