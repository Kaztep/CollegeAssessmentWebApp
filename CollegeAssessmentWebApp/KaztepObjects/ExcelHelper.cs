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

        public static int getLastRowFromEnd(int totalRows)
        {
            int row;
            for (row = totalRows; row > 1; row--)
            {
                if (Convert.ToString((MySheet.Cells[row, 1] as Microsoft.Office.Interop.Excel.Range).Value2) != null)
                {
                    totalRows = row;
                    break;
                }
            }
            return totalRows;
        }

        //Should break this down later
        public static DataGroup pullFromCurriculumMap(string filename)
        {
            //load Excel 
            MyApp = new Excel.Application();
            MyApp.Visible = false;
            MyBook = MyApp.Workbooks.Open(filename);
            MySheet = (Excel.Worksheet) MyBook.Sheets[3]; // Explicit cast is not required here

            //pulling the data by cell 
            DataGroup AssessmentData = new DataGroup();

            AssessmentData.name = Convert.ToString((MySheet.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
            AssessmentData.year = Convert.ToString((MySheet.Cells[3, 1] as Microsoft.Office.Interop.Excel.Range).Value2);


            //loop through courses
            int startCol = 2;
            //These two lines do the magic.
            MySheet.Columns.ClearFormats();
            MySheet.Rows.ClearFormats();
            int iTotalColumns = MySheet.UsedRange.Columns.Count;
            //this is at 1000 due to the setup of the sheet so I have to use function below to count down from the origial total (which, in this case is 1000)
            int iTotalRows = MySheet.UsedRange.Rows.Count;
            iTotalRows = getLastRowFromEnd(iTotalRows);


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

            List<Outcome> listofOutcomes = new List<Outcome>();
            //loop through outcomes
            for (int outcomeNum = 6; outcomeNum < iTotalRows; outcomeNum = outcomeNum + 5)
            {
                //start with a new outcome each time
                Outcome outcome = new Outcome();
                //add outcome name
                outcome.name = Convert.ToString((MySheet.Cells[outcomeNum, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
                
                int indicatorNum = outcomeNum + 1;
                
                //loop through indicators
                string indicatorName;
                do
                {
                    Indicator indicator = new Indicator();
                    indicatorName = Convert.ToString((MySheet.Cells[indicatorNum, 1] as Microsoft.Office.Interop.Excel.Range).Value2);
                    //checks to see if the next line is blank 
                    if (indicatorName == null)
                    {
                        outcomeNum++;
                        break;
                    }


                    //loop through assessment levels and assignments
                    List<string> levels = new List<string>(); //might change this to a dictionary
                    List<string> assignmentNames = new List<string>(); //might change this to a dictionary

                    for (int i = startCol; i < iTotalColumns + 1; i++)
                    {
                        Excel.Range currentRange = (Excel.Range)MySheet.Cells[7, i];
                        Excel.Range currentRange2 = (Excel.Range)MySheet.Cells[8, i];
                        if (currentRange.Value2 != null && currentRange2.Value2 != null)
                        {
                            indicator.levels.Add(currentRange.Value2.ToCharArray()[0]);
                            indicator.assignments.Add(currentRange2.Value2.ToString());
                        }
                    }

                    //add indicator to an outcome
                    indicator.name = indicatorName;
                    outcome.indicators.Add(indicator);
                    indicatorNum = indicatorNum + 2;
                } while (indicatorName != null);


                //need to check if next row is empty after this code
                listofOutcomes.Add(outcome);

            }
            //This is a collection of the data groups
            AssessmentData.Outcomes = listofOutcomes;
            return AssessmentData;

        }

    }
}
