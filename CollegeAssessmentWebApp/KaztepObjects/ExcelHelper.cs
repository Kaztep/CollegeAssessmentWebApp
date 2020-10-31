using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;       //microsoft Excel 14 object in references-> COM tab

namespace CollegeAssessmentWebApp
{
    public class ExcelHelper
    {
        private static Excel.Workbook MyBook = null;
        private static Excel.Application MyApp = null;
        private static Excel.Worksheet MySheet = null;



        public static void getExcelFiles()
        {
            foreach (string file in Directory.EnumerateFiles("~\\..\\..\\..\\..\\AssessmentExcelFiles","*.xlsx"))
            {
                string contents = File.ReadAllText(file);
            }

            

        }

        public static void getExcelFile(string filename)
        {

            MyApp = new Excel.Application();
            MyApp.Visible = false;
            MyBook = MyApp.Workbooks.Open(filename);
            MySheet = (Excel.Worksheet) MyBook.Sheets[3]; // Explicit cast is not required here
            int lastRow = MySheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Row;
            int lastCol = MySheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Column;

            //pulling the data by cell 
            DataGroup AssessmentData = new DataGroup();

            AssessmentData.name = Convert.ToString((MySheet.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
            AssessmentData.year = Convert.ToString((MySheet.Cells[3, 1] as Microsoft.Office.Interop.Excel.Range).Value2);


            //loop through courses
            //int targetRow = 5;
            int startCol = 2;
            int iTotalColumns = MySheet.UsedRange.Columns.Count;
            int iTotalRows = MySheet.UsedRange.Rows.Count;
            //These two lines do the magic.
            MySheet.Columns.ClearFormats();
            MySheet.Rows.ClearFormats();
            iTotalColumns = MySheet.UsedRange.Columns.Count;
            iTotalRows = MySheet.UsedRange.Rows.Count;

            List<string> courseNames = new List<string>();
            for (int i = startCol; i < iTotalColumns + 1; i++)
            {
                Excel.Range currentRange = (Excel.Range)MySheet.Cells[5, i];
                if (currentRange.Value2 != null)
                {
                    courseNames.Add(currentRange.Value2.ToString());
                }
            }
            //add coursenames to assessment data
            AssessmentData.ProgramCourses = courseNames;


            //this all needs to be a loop

            List<Outcome> listofOutcomes = new List<Outcome>();

            string outcomeName = Convert.ToString((MySheet.Cells[6, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
            //loop through assessment levels
            List<string> levels = new List<string>(); //might change this to a dictionary
            for (int i = startCol; i < iTotalColumns + 1; i++)
            {
                Excel.Range currentRange = (Excel.Range)MySheet.Cells[7, i];
                if (currentRange.Value2 != null)
                {
                    levels.Add(currentRange.Value2.ToString());
                }
            }

            //need to check if next row is empty after this code
            //loop through Assignments
            List<string> assignmentNames = new List<string>(); //might change this to a dictionary
            for (int i = startCol; i < iTotalColumns + 1; i++)
            {
                Excel.Range currentRange = (Excel.Range)MySheet.Cells[8, i];
                if (currentRange.Value2 != null)
                {
                    assignmentNames.Add(currentRange.Value2.ToString());
                }
            }

            Outcome o = new Outcome();
            listofOutcomes.Add(o);



            //This is a collection of the data groups - this will be outside of the loop
            //AssessmentData.Outcomes = listofOutcomes;


        }

    }
}
