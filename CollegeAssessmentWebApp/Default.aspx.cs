using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace CollegeAssessmentWebApp
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnReport_Click(object sender, EventArgs e)
        {
            // Get all Excel file names uploaded in the folder
            List<string> fileNames = GetFileNames();
            // Collect curriculum maps from all courses
            List<CurriculumMap> CurriculumMaps = new List<CurriculumMap>();
            // Load the Excel workbooks data
            foreach (string fileName in fileNames)
            {
                // Add datagroup from course curriculum map
               CurriculumMaps.Add(ExcelHelper.PullFromCurriculumMap(fileName));
            }

        }

        public List<string> GetFileNames()
        {
            string[] hold = Directory.GetFiles(@"D:\Projects\Kaztep\CollegeAssessmentWebApp\AssessmentExcelFiles");
            List<string> fileEntries = hold.ToList();

            for (int i = fileEntries.Count - 1; i >= 0; i--)
            {
                // Remove temp files
                if (fileEntries[i].Contains("~$"))
                    fileEntries.RemoveAt(i);
                else
                    lstbFileNames.Items.Add(fileEntries[i]);
            }

            return fileEntries;
        }
    }
}