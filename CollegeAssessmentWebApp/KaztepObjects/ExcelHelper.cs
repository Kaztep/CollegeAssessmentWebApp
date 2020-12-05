using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel; // Microsoft Excel 14 object in references-> COM tab

namespace CollegeAssessmentWebApp
{
    public class ExcelHelper
    {
        private static Excel.Workbook MyBook = null;
        private static Excel.Application MyApp = null;
        private static Excel.Worksheet MySheet = null;
        private static int iTotalRows;
        private static int iTotalColumns;
        private static int startCol;    

        public static void GetExcelFiles()
        {
            foreach (string file in Directory.EnumerateFiles("~\\..\\..\\..\\..\\AssessmentExcelFiles","*.xlsx"))
            {
                string contents = File.ReadAllText(file);
            }         
        }

        public static CurriculumMap PullFromCurriculumMap(string fileName)
        {
            LoadExcelObjects(fileName, 3);

            // Pulling the data by cell 
            CurriculumMap curriculumMap = new CurriculumMap();

            curriculumMap.FileName = fileName;
            curriculumMap.Name = Convert.ToString((MySheet.Cells[1, 1] as Excel.Range).Value2);
            curriculumMap.Year = Convert.ToString((MySheet.Cells[3, 1] as Excel.Range).Value2);
            
            // This is at 1000 due to the setup of the sheet so I have to use function below to count down from the original total (which, in this case is 1000)
            iTotalRows = GetLastRowFromEnd(MySheet.UsedRange.Rows.Count);
            iTotalColumns = MySheet.UsedRange.Columns.Count;
            startCol = 2;

            curriculumMap.ProgramCourses = GetCourseNames();
            curriculumMap.Outcomes = GetOutcomes();

            return curriculumMap;
        }

        private static void LoadExcelObjects(string fileName, int index)
        {
            MyApp = new Excel.Application();
            MyApp.Visible = false;
            MyBook = MyApp.Workbooks.Open(fileName);
            // Explicit cast is not required here
            MySheet = (Excel.Worksheet)MyBook.Sheets[index]; 
            // These two lines do the magic.
            MySheet.Columns.ClearFormats();
            MySheet.Rows.ClearFormats();
        }

        public static int GetLastRowFromEnd(int totalRows)
        {
            int row;
            for (row = totalRows; row > 1; row--)
            {
                if (Convert.ToString((MySheet.Cells[row, 1] as Excel.Range).Value2) != null)
                    return row;
            }
            return totalRows;
        }

        private static string GetCourseNames()
        {
            var courseNames = String.Empty;
            for (int i = startCol; i < iTotalColumns + 1; i++)
            {
                Excel.Range currentRange = (Excel.Range)MySheet.Cells[5, i];
                if (currentRange.Value2 != null)
                    courseNames += currentRange.Value2 + ",";
            }        
            return courseNames.TrimEnd(',');
        }

        private static List<Outcome> GetOutcomes()
        {
            var outcomes = new List<Outcome>();

            for (int outcomeNum = 6; outcomeNum < iTotalRows; outcomeNum++)
            {
                string value2 = Convert.ToString((MySheet.Cells[outcomeNum, 1] as Excel.Range).Value2);

                if (!String.IsNullOrEmpty(value2) && value2.StartsWith("Program Outcome"))
                {
                    Outcome outcome = new Outcome();
                    outcome.Name = value2;
                    outcome.Indicators = GetIndicators(outcomeNum);

                    // Need to check if next row is empty after this code
                    outcomes.Add(outcome);
                }
            }

            return outcomes;
        }

        private static List<Indicator> GetIndicators(int outcomeNum)
        {
            var indicators = new List<Indicator>();
            int indicatorNum = outcomeNum + 1;
            string indicatorName;

            do
            {
                indicatorName = Convert.ToString((MySheet.Cells[indicatorNum, 1] as Excel.Range).Value2);
                // Checks to see if the next line is blank 
                if (indicatorName == null)
                    break;

                Indicator indicator = new Indicator();
                indicator.Name = indicatorName;
                indicator.Assignments = GetAssignments(indicatorNum);
                indicators.Add(indicator);
                indicatorNum += 2;
            } while (indicatorName != null);

            return indicators;
        }

        private static List<Assignment> GetAssignments(int indicatorNum)
        {
            var assignments = new List<Assignment>();

            // Loop through assessment levels and assignments
            for (int i = startCol; i < iTotalColumns + 1; i++)
            {
                Excel.Range currentRange = (Excel.Range)MySheet.Cells[indicatorNum, i];
                Excel.Range currentRange2 = (Excel.Range)MySheet.Cells[indicatorNum + 1, i];
                Excel.Range currentRange3 = (Excel.Range)MySheet.Cells[5, i];

                // TODO: Are assignments valid if they don't have a level?
                // Currently we only add ones with a level
                if (currentRange.Value2 != null && currentRange2.Value2 != null)
                {
                    Assignment assignment = new Assignment();
                    assignment.Level = currentRange.Value2.ToCharArray()[0];
                    assignment.Name = currentRange2.Value2.ToString();
                    assignment.Course = currentRange3.Value2.ToString();
                    assignments.Add(assignment);
                }
            }
            return assignments;
        }
    }
}
