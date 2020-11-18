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
using System.Web.UI.WebControls.WebParts;





namespace CollegeAssessmentWebApp
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnReport_Click(object sender, EventArgs e)
        {
            //get all Excel file names uploaded in the folder
            List<string> fileNames = GetFileNames();
            //load the Excel workbooks data
            foreach (string fileName in fileNames)
            {
                ExcelHelper.getExcelFile(fileName);
            }

        }
     
        public List<string> GetFileNames()
        {
            string[] hold = Directory.GetFiles(@"C:\Users\mrako\Desktop\Keztap Solutions\GitHub\AssessmentExcelFiles");
            List<string> fileEntries = hold.ToList();

            for (int i = fileEntries.Count - 1; i >= 0; i--)
            {
                //remove temp files
                if (fileEntries[i].Contains("~$"))
                    fileEntries.RemoveAt(i);
                else
                    lstbFileNames.Items.Add(fileEntries[i]);
            }

            return fileEntries;
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (this.FileUpLoad1.HasFile)
            {

                this.FileUpLoad1.SaveAs(@"C:\Users\mrako\Desktop\Keztap Solutions\CollegeAssessmentWebApp\FileUploadTesters\NewTest\" + FileUpLoad1.FileName);
                Label1.Text = "File Uploaded: " + FileUpLoad1.FileName + DateTime.Now.ToString(" mm/dd/yyyy: hh:mm:ss tt");
            }
            else
            {
                Label1.Text = "No File Uploaded.";
            }
        }
    }
}