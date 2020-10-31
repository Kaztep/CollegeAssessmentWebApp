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

            BindingList<DataGroup> AssessmentData = new BindingList<DataGroup>();
            string programName = Convert.ToString((MySheet.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
            string year = Convert.ToString((MySheet.Cells[3, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
            string outcome1 = Convert.ToString((MySheet.Cells[6, 1] as Microsoft.Office.Interop.Excel.Range).Value2);



            //for (int index = 2; index <= lastRow; index++)
            //{
            //    System.Array MyValues = (System.Array)MySheet.get_Range("A" + index.ToString(), "D" + index.ToString()).Cells.Value;
            //    AssessmentData.Add(new DataGroup
            //    {
            //        Name = MyValues.GetValue(1, 1).ToString(),
            //        Employee_ID = MyValues.GetValue(1, 2).ToString(),
            //        Email_ID = MyValues.GetValue(1, 3).ToString(),
            //        Number = MyValues.GetValue(1, 4).ToString()
            //    });
            //}



        }

    }
}
