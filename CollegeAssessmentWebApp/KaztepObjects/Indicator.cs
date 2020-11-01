using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeAssessmentWebApp
{
    public class Indicator
    {
        public string name;

        //these three are parallel lists
        //I, R ,or S
        public List<char> levels = new List<char>();
        public List<string> assignments = new List<string>();
        public List<string> courseNames = new List<string>();
    }
}
