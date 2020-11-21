using System;
using System.Collections.Generic;

namespace CollegeAssessmentWebApp
{
    public class Outcome : DataObject
    {
        public int curriculumMapID;

        /// <summary>
        /// Assignments are in the Indicator objects
        /// </summary>
        public List<Indicator> Indicators = new List<Indicator>();
    }

}
