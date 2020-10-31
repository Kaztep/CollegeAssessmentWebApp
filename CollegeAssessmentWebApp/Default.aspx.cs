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
            List<string> fileNames = GetFileNames();

            foreach (string fileName in fileNames)
            {
                ExcelHelper.getExcelFile(fileName);
            }

        }

        public List<string> GetFileNames()
        {
            string[] hold = Directory.GetFiles(@"C:\Users\Kellon\source\repos\AssessmentExcelFiles");
            List<string> fileEntries = hold.ToList();

            for (int i = fileEntries.Count - 1; i >= 0; i--)
            {
                if (fileEntries[i].Contains("~$"))
                    fileEntries.RemoveAt(i);
                //else
                //   listBox1.Items.Add(fileEntries[i]);
            }

            return fileEntries;
        }
    }
}