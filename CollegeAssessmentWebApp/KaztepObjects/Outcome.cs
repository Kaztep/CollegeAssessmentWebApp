using System;
using System.Collections.Generic;

namespace CollegeAssessmentWebApp
{
    public class Outcome : DataObject
    {
        public int CurriculumMapID { get; set; }

        /// <summary>
        /// Assignments are in the Indicator objects
        /// </summary>
        public List<Indicator> Indicators { get; set; }
    }

}
