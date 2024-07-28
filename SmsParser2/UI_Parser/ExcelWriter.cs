using Microsoft.Office.Interop.Excel;
using SmsParser2.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace SmsParser2.UI_Parser
{
    public class ExcelWriter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        private static readonly LogController oldLog = LogController.Instance;

        public ExcelWriter(string[] arrHeader)
        {
            header = arrHeader;
            colHash = new Dictionary<string, int>(header.Length + 5);
            for (int i = 0; i < header.Length; ++i)
            {
                colHash[header[i].ToLower()] = i;
            }
        }

        public ExcelWriter()
        {

        }

        private static readonly object misValue = System.Reflection.Missing.Value;

        private string[] header;
        private Dictionary<string, int> colHash;

        private void DumpArrayToSheet(Worksheet sheet, object[,] data)
        {
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
                log.Error("Too large", ex);
            }
            catch (Exception e2)
            {
                log.Error("Other error", e2);
            }
        }

        public void TestFunction()
        {
            log.Debug("test test");
        }

        public void ExportSmsInfo(List<SmsInfo> listSmsInfo, string filePath)
        {
            //log.Debug("Writing: " + filePath);
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //Application excel = new Application();
            //if (excel == null)
            //{
            //    log.Error("Excel is not properly installed");
            //    return;
            //}
            //excel.DisplayAlerts = false;
            //Workbooks workbooks = excel.Workbooks;
            //Workbook workbook = workbooks.Add(misValue);
            //Worksheet sheet = (Worksheet)workbook.Worksheets.get_Item(1);
            //sheet.Name = DateTime.Now.ToString("All");

            //int numRows = listSmsInfo.Count + 1;
            //int numCols = header.Length;
            //var data = new object[numRows, numCols];

            //for (int j = 0; j < numCols; ++j)
            //{
            //    data[0, j] = header[j];
            //}

            //int rowIndex = 1;

            //foreach (SmsInfo info in listSmsInfo)
            //{
            //    object[] col = info.GetValueArray();
            //    for (int j = 0; j < col.Length; ++j)
            //    {
            //        data[rowIndex, j] = col[j];
            //    }
            //    ++rowIndex;
            //}

            //DumpArrayToSheet(sheet, data);

            //// Format file

            //sheet.Application.ActiveWindow.SplitRow = 1;
            //sheet.Application.ActiveWindow.FreezePanes = true;

            //sheet.Range[getColumnRangeText(1, numCols)].VerticalAlignment = XlVAlign.xlVAlignTop;

            //Range firstRow = (Range)sheet.Rows[1];
            //firstRow.AutoFilter(1);
            //firstRow.Font.Bold = true;

            //sheet.UsedRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            //sheet.Columns.AutoFit();
            //((Range)sheet.Columns[colHash["body"] + 1]).ColumnWidth = MySetting.Default.BodyColumnWidth;
            //((Range)sheet.Columns[colHash["body"] + 1]).WrapText = true;
            //((Range)sheet.Columns[colHash["address"] + 1]).ColumnWidth = MySetting.Default.BodyColumnWidth;
            //((Range)sheet.Columns[colHash["address"] + 1]).HorizontalAlignment = XlHAlign.xlHAlignLeft;

            //sheet.Rows.AutoFit();

            ////process bank info
            //Worksheet sheet2 = (Worksheet)workbook.Worksheets.get_Item(2);
            //sheet2.Name = DateTime.Now.ToString("Bank");

            //numRows = listSmsInfo.Count(x => x.MyBankInfo != null && x.MyBankInfo.ParseStatus != StatusBankInfo.Ignored) + 1;
            //numCols = SmsInfo.BANK_HEADER.Length;
            //var data2 = new object[numRows, numCols];

            //for (int j = 0; j < numCols; ++j)
            //{
            //    data2[0, j] = SmsInfo.BANK_HEADER[j];
            //}

            //rowIndex = 1;

            //foreach (SmsInfo item in listSmsInfo)
            //{
            //    if (item.MyBankInfo != null && item.MyBankInfo.ParseStatus != StatusBankInfo.Ignored)
            //    {
            //        object[] col = item.GetBankArray();
            //        for (int j = 0; j < col.Length; ++j)
            //        {
            //            data2[rowIndex, j] = col[j];
            //        }
            //        ++rowIndex;
            //    }
            //}

            //DumpArrayToSheet(sheet2, data2);

            ////format bank sheet
            //sheet2.Activate();
            //sheet2.Application.ActiveWindow.SplitRow = 1;
            //sheet2.Application.ActiveWindow.FreezePanes = true;

            //sheet2.Range[getColumnRangeText(1, numCols)].VerticalAlignment = XlVAlign.xlVAlignTop;

            //Range firstRowBank = (Range)sheet2.Rows[1];
            //firstRowBank.AutoFilter(1);
            //firstRowBank.Font.Bold = true;

            //((Range)sheet2.Columns[3]).NumberFormat = "#,##0";
            //((Range)sheet2.Columns[4]).Style = "Comma [0]";

            //sheet2.UsedRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            //sheet2.Columns.AutoFit();
            //((Range)sheet2.Columns[6]).ColumnWidth = MySetting.Default.BodyColumnWidth;
            //((Range)sheet.Columns[6]).WrapText = true;

            //sheet.Rows.AutoFit();

            //workbook.Password = "q";
            //workbook.SaveAs(filePath, XlFileFormat.xlOpenXMLWorkbook);
            //workbook.Close();
            //excel.Quit();

            //// Release our resources.
            //Marshal.ReleaseComObject(workbook);
            //Marshal.ReleaseComObject(workbooks);
            //Marshal.ReleaseComObject(excel);
            //Marshal.FinalReleaseComObject(excel);

            //log.Debug("Finish writing in " + stopwatch.ElapsedMilliseconds + " ms");
            //stopwatch.Stop();
        }

        public static Application GlobalExcel;

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

            int numRows = listBank.Count + 1;
            int numCols = header.Length;
            var data = new object[numRows, numCols];

            for (int j = 0; j < numCols; ++j)
            {
                data[0, j] = header[j];
            }

            int rowIndex = 1;

            foreach (VietcomInfo info in listBank)
            {
                object[] col = info.GetValueArray();
                for (int j = 0; j < col.Length; ++j)
                {
                    data[rowIndex, j] = col[j];
                }
                ++rowIndex;
            }

            DumpArrayToSheet(sheet, data);

            // Format file

            sheet.Application.ActiveWindow.SplitRow = 1;
            sheet.Application.ActiveWindow.FreezePanes = true;
            sheet.Range[getColumnRangeText(1, numCols)].VerticalAlignment = XlVAlign.xlVAlignTop;

            //first row with filter and bold text
            Range firstRow = (Range)sheet.Rows[1];
            firstRow.AutoFilter(1);
            firstRow.Font.Bold = true;

            sheet.UsedRange.Borders.LineStyle = XlLineStyle.xlContinuous;

            //format number columns
            ((Range)sheet.Columns[colHash["amount"] + 1]).NumberFormat = "#,##0";
            ((Range)sheet.Columns[colHash["balance"] + 1]).Style = "Comma [0]";

            sheet.UsedRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            sheet.Columns.AutoFit();

            //set ref column width after auto fit
            ((Range)sheet.Columns[colHash["ref"] + 1]).ColumnWidth = MySetting.Default.BodyColumnWidth;
            ((Range)sheet.Columns[colHash["ref"] + 1]).WrapText = true;

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

            log.Debug("Finish writing in " + sw.ElapsedMilliseconds + " ms");
            sw.Stop();
        }

        private string GetColumnRangeText(int x, int y)
        {
            char begin = (char)((x + 64) % 255);
            char end = (char)((y + 64) % 255);
            return begin + ":" + end;
        }

        private string GetColumnRangeText(int x)
        {
            return GetColumnRangeText(x, x);
        }
    }
}
