using System;
using System.Data.SqlClient;

namespace CollegeAssessmentWebApp
{
    public class Assignment : DataObject
    {
        public int IndicatorID;
        // I, R, or S
        public char Level;
        public string Course;
    }
}