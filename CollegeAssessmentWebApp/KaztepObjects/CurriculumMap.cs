using System;
using System.Collections.Generic;

namespace CollegeAssessmentWebApp
{
    public class CurriculumMap : DataObject
    {
        public string Year;
        public List<string> ProgramCourses = new List<string>(11);
        
        /// <summary>
        /// Indicators are in the Outcome objects
        /// </summary>
        public List<Outcome> Outcomes = new List<Outcome>(5);
    }
}
