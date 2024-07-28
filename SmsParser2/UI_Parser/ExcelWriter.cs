using Microsoft.Office.Interop.Excel;
using SmsParser2.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace SmsParser2.UI_Parser
{
    public class ExcelWriter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        private static readonly LogController oldLog = LogController.Instance;

        public ExcelWriter()
        {

        }

        private static readonly object misValue = System.Reflection.Missing.Value;

        public static Application GlobalExcel;

        private void DumpArrayToSheet(Worksheet sheet, object[,] data)
        {
            oldLog.Debug("Dump array to sheet");
            int numRows = data.GetLength(0);
            int numCols = data.GetLength(1);
            Range beginWrite = (Range)sheet.Cells[1, 1];
            // Range endWrite = (Range)sheet.Cells[numRows, numCols];
            // Range sheetData = sheet.Range[beginWrite, endWrite];
            try
            {
                Range sheetData = beginWrite.get_Resize(numRows, numCols);
                sheetData.Value2 = data;
            }
            catch (OutOfMemoryException ex)
            {
                oldLog.Error("Too large", ex);
            }
            catch (Exception e2)
            {
                oldLog.Error("Other error", e2);
            }
        }

        public void ExportVietcomInfo(List<DbBank> listBank, string filePath)
        {
            oldLog.Debug("Begin writing to Excel: " + filePath);
            Stopwatch sw = Stopwatch.StartNew();
            if (GlobalExcel == null)
            {
                GlobalExcel = new Application();
                if (GlobalExcel == null)
                {
                    log.Error("Excel is not properly installed");
                    return;
                }
            }
            GlobalExcel.DisplayAlerts = false;
            Workbooks workbooks = GlobalExcel.Workbooks;
            Workbook workbook = workbooks.Add(misValue);
            Worksheet sheet = (Worksheet)workbook.Worksheets.get_Item(1);
            sheet.Name = "Bank";

            //set header
            string[] header = new string[] { "Name", "Date", "Amount", "Balance", "Time", "Ref" };
            Dictionary<string, int> colHash = new Dictionary<string, int>(header.Length + 5);
            for (int i = 0; i < header.Length; ++i)
            {
                colHash[header[i].ToLower()] = i + 1;
            }

            int numRows = listBank.Count + 1;
            int numCols = header.Length;
            var data = new object[numRows, numCols];

            for (int j = 0; j < numCols; ++j)
            {
                data[0, j] = header[j];
            }

            int rowIndex = 1;

            foreach (var info in listBank)
            {
                List<string> listValue = new List<string>();
                //"Name", "Date", "Amount", "Balance", "Time", "Ref"
                listValue.Add(info.BankName);
                listValue.Add(info.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                listValue.Add(info.Delta + "");
                listValue.Add(info.Balance + "");
                listValue.Add("T " + info.TimeString);
                listValue.Add(info.Ref);
                object[] col = listValue.ToArray();
                for (int j = 0; j < col.Length; ++j)
                {
                    data[rowIndex, j] = col[j];
                }
                ++rowIndex;
            }

            DumpArrayToSheet(sheet, data);

            //Format file

            sheet.Application.ActiveWindow.SplitRow = 1;
            sheet.Application.ActiveWindow.FreezePanes = true;
            sheet.Range[GetColumnRangeText(1, numCols)].VerticalAlignment = XlVAlign.xlVAlignTop;

            //first row with filter and bold text
            Range firstRow = (Range)sheet.Rows[1];
            firstRow.AutoFilter(1);
            firstRow.Font.Bold = true;

            sheet.UsedRange.Borders.LineStyle = XlLineStyle.xlContinuous;

            //format number columns
            ((Range)sheet.Columns[colHash["amount"]]).NumberFormat = "#,##0";
            ((Range)sheet.Columns[colHash["balance"]]).Style = "Comma [0]";

            sheet.UsedRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            sheet.Columns.AutoFit();

            //set ref column width after auto fit
            ((Range)sheet.Columns[colHash["ref"]]).ColumnWidth = MySetting.Default.BodyColumnWidth;
            ((Range)sheet.Columns[colHash["ref"]]).WrapText = true;

            sheet.Rows.AutoFit();

            workbook.Password = "q";
            workbook.SaveAs(filePath, XlFileFormat.xlOpenXMLWorkbook);
            workbook.Close();
            GlobalExcel.Quit();

            // Release our resources.
            _ = Marshal.ReleaseComObject(workbook);
            _ = Marshal.ReleaseComObject(workbooks);
            _ = Marshal.ReleaseComObject(GlobalExcel);
            _ = Marshal.FinalReleaseComObject(GlobalExcel);

            oldLog.Debug("Finish writing in " + sw.ElapsedMilliseconds + " ms");
            sw.Stop();
        }

        private static string GetColumnRangeText(int x, int y)
        {
            char begin = (char)((x + 64) % 255);
            char end = (char)((y + 64) % 255);
            return begin + ":" + end;
        }
    }
}
