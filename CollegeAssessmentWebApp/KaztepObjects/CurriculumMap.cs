using System;
using System.Collections.Generic;

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
    }
}
