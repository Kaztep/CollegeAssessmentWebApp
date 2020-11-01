using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeAssessmentWebApp
{
    public class DataGroup
    {
        public string name;
        public string year;
        public List<string> ProgramCourses = new List<string>(11);
        //indicators are in the outcome object
        public List<Outcome> Outcomes = new List<Outcome>(5);




    }
}
