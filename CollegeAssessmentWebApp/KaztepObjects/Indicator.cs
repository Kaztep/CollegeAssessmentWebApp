﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeAssessmentWebApp
{
    public class Indicator : DataObject
    {
        public int OutcomeID;
        public List<Assignment> Assignments = new List<Assignment>();
    }
}
