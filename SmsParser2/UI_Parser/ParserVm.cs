using Dapper;
using log4net.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Simple1.MVVMBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SmsParser2.UI_Parser
{
    public class ParserVm : ViewModelBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        public ParserVm()
        {
            log.Info("New ViewModel is created");
            IsButtonEnabled = true;
        }

        #region Bind properties

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set { SetValue(ref _isButtonEnabled, value); }
        }

        #endregion

        #region Normal properties

        #endregion

        public async void BtnExportVietcomClick()
        {
            log.Info("Clicked button export to Excel file");

            string outputFolder = MySetting.Default.OutputFolder;
            while (outputFolder.EndsWith("\\"))
            {
                outputFolder = outputFolder.Remove(outputFolder.Length - 1);
            }

            int width = Math.Max(MySetting.Default.BodyColumnWidth, 60);
            if (Directory.Exists(outputFolder) && MySetting.Default.FileNamePrefix.Length > 0)
            {
                MySetting.Default.OutputFolder = outputFolder;
                MySetting.Default.BodyColumnWidth = width;
                MySetting.Default.Save();

                IsButtonEnabled = false;
                var t = Task.Run(() =>
                {
                    //read vietcombank data
                    List<VietcomInfo> list = new List<VietcomInfo>();
                    if (Directory.Exists(MySetting.Default.NewVietcomFolder))
                    {
                        string[] files = Directory.GetFiles(MySetting.Default.NewVietcomFolder);
                        foreach (var f in files)
                        {
                            string name = Path.GetFileName(f);
                            if (!name.Contains("~") && (name.Contains("vietcombank", StringComparison.OrdinalIgnoreCase) || name.Contains("lich-su-giao-dich", StringComparison.OrdinalIgnoreCase)) && name.Contains(".xls", StringComparison.OrdinalIgnoreCase))
                            {
                                list.AddRange(ReadExcelFileVietcom(f));
                            }
                        }
                    }
                    List<VietcomInfo> list2 = new List<VietcomInfo>();
                    HashSet<string> hashID = new HashSet<string>();
                    foreach (var item in list)
                    {
                        string key = item.Message + item.TimeString;
                        if (hashID.Add(key))
                        {
                            list2.Add(item);
                        }
                    }
                    list2.Sort();
                    list2.Reverse();

                    //print output to excel and text file
                    log.Info("Process data to folder: " + outputFolder);
                    ExcelWriter writer = new ExcelWriter(VietcomInfo.VIETCOM_HEADER);
                    writer.ExportVietcomInfo(list2, outputFolder + "\\" + MySetting.Default.FileNamePrefix + "_Vietcom_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
                    log.Info("Finish process data");
                });
                await t;
                _ = MessageBox.Show("Exported to " + outputFolder, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                IsButtonEnabled = true;
            }
        }

        public async void BtnExportClick()
        {
            log.Info("Clicked button export to Excel file");
            string inputFilePath = MySetting.Default.XMLFilePath;
            string outputFolder = MySetting.Default.OutputFolder;
            while (outputFolder.EndsWith("\\"))
            {
                outputFolder = outputFolder.Remove(outputFolder.Length - 1);
            }

            int width = Math.Max(MySetting.Default.BodyColumnWidth, 60);

            if (File.Exists(inputFilePath) && Directory.Exists(outputFolder) && MySetting.Default.FileNamePrefix.Length > 0)
            {
                IsButtonEnabled = false;
                var t = Task.Run(() =>
                {
                    var list = ReadSMSFile(inputFilePath);
                    //LogBankInfo(list);

                    ImportSmsToDatabase(list);
                    var test = LoadSmsFromDatabase();
                    log.Info("Number of loaded SMS: " + test.Count);

                    //log.Info("Process data to folder: " + outputFolder);
                    //ExcelWriter writer = new ExcelWriter(SmsInfo.EXCEL_HEADER);
                    //log.Info("Created new excel writer");

                    ////if you need to do something to test the writer
                    //writer.TestFunction();

                    ////2021.06.08: Disable date suffix
                    //writer.ExportSmsInfo(ReadSMSFile(inputFilePath), outputFolder + "\\" + MySetting.Default.FileNamePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
                    //log.Info("Finish process data");
                });
                await t;
                _ = MessageBox.Show("Exported to " + outputFolder, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                IsButtonEnabled = true;
            }
        }

        private void ImportSmsToDatabase(List<SmsInfo> listSms)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                foreach (var item in listSms)
                {
                    connection.Execute(@"INSERT OR IGNORE INTO sms (Address,Body,Date) VALUES (@Address,@Body,@Date)", new { item.Address, item.Body, item.Date });
                }
                transaction.Commit();
            }
            sw.Stop();
            log.Info("Insert to database in: " + sw.ElapsedMilliseconds + " ms");
        }

        private List<SmsInfo> LoadSmsFromDatabase()
        {
            List<SmsInfo> ret = new List<SmsInfo>();
            Stopwatch sw = Stopwatch.StartNew();
            using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
            {
                var list = connection.Query<SmsInfo>(@"SELECT * from sms");
                foreach (var item in list)
                {
                    ret.Add(item);
                }
            }
            sw.Stop();
            log.Info("Load SMS from database in: " + sw.ElapsedMilliseconds + " ms");
            return ret;
        }

        private List<SmsInfo> ReadSMSFile(string filePath)
        {
            log.Debug("Read data from file: " + filePath);
            List<SmsInfo> ret = new List<SmsInfo>();
            string str = File.ReadAllText(filePath);
            Regex regexXml = new Regex(@"<sms.+\/>");
            MatchCollection matchSmsTag = regexXml.Matches(str);
            foreach (Match match in matchSmsTag)
            {
                SmsInfo info = new SmsInfo(match.Value);
                ret.Add(info);
            }
            ret.Sort((x, y) => y.DateAsNumber.CompareTo(x.DateAsNumber));
            return ret;
        }

        private List<VietcomInfo> ReadExcelFileVietcom(string filePath)
        {
            log.Debug("Read transaction from Excel file: " + filePath);
            List<VietcomInfo> listVietcom = new List<VietcomInfo>();

            if (ExcelWriter.GlobalExcel == null)
            {
                ExcelWriter.GlobalExcel = new Microsoft.Office.Interop.Excel.Application();
                if (ExcelWriter.GlobalExcel == null)
                {
                    log.Error("Excel is not properly installed");
                    return listVietcom;
                }
            }
            var excel = ExcelWriter.GlobalExcel;
            excel.DisplayAlerts = false;
            Workbook workbook = excel.Workbooks.Open(filePath);
            Worksheet sheet = (Worksheet)workbook.Worksheets.get_Item(1);
            Microsoft.Office.Interop.Excel.Range excelRange = sheet.UsedRange;
            var arr = (object[,])excelRange.Value2;
            int numRow = arr.GetLength(0);
            int numCol = arr.GetLength(1);

            int startRow = 1;
            int endRow = 1;

            int colSTT = 0;
            int colDate = 0;
            int colSoGhiCo = 0;
            int colSoGhiNo = 0;
            int colSoDu = 0;
            int colNoiDung = 0;

            for (startRow = 1; startRow <= numRow; ++startRow)
            {
                if (arr[startRow, 1] != null)
                {
                    if (arr[startRow, 1].ToString().ToLower().Contains("stt"))
                    {
                        for (int i = 1; i <= numCol; ++i)
                        {
                            if (arr[startRow, i] != null)
                            {
                                var text = arr[startRow, i].ToString().Trim().ToLower();
                                if (text.Contains("stt") || text.Contains("no."))
                                {
                                    colSTT = i;
                                }
                                else if (text.Contains("doc no") || text.Contains("date"))
                                {
                                    colDate = i;
                                }
                                else if (text.Contains("debit"))
                                {
                                    colSoGhiNo = i;
                                }
                                else if (text.Contains("credit"))
                                {
                                    colSoGhiCo = i;
                                }
                                else if (text.Contains("transactions") || text.Contains("detail"))
                                {
                                    colNoiDung = i;
                                }
                                else if (text.Contains("balance"))
                                {
                                    colSoDu = i;
                                }
                            }
                        }
                        break;
                    }
                }
                else if (arr[startRow, 2] != null)
                {
                    if (arr[startRow, 2].ToString().ToLower().Contains("stt"))
                    {
                        for (int i = 1; i <= numCol; ++i)
                        {
                            if (arr[startRow, i] != null)
                            {
                                var text = arr[startRow, i].ToString().Trim().ToLower();
                                if (text.Contains("stt") || text.Contains("no."))
                                {
                                    colSTT = i;
                                }
                                else if (text.Contains("doc no") || text.Contains("date"))
                                {
                                    colDate = i;
                                }
                                else if (text.Contains("debit"))
                                {
                                    colSoGhiNo = i;
                                }
                                else if (text.Contains("credit"))
                                {
                                    colSoGhiCo = i;
                                }
                                else if (text.Contains("transactions") || text.Contains("detail"))
                                {
                                    colNoiDung = i;
                                }
                                else if (text.Contains("balance"))
                                {
                                    colSoDu = i;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (colSTT < 1 || colDate < 1 || colSoGhiNo < 1 || colSoGhiCo < 1 || colNoiDung < 1 || colSoDu < 1)
            {
                log.Error("Vietcombank excel is not in correct format: " + filePath);
            }

            for (endRow = startRow + 1; endRow <= numRow; ++endRow)
            {
                if ((arr[endRow, 1] == null || arr[startRow, 1].ToString().Trim().Length == 0) && (arr[endRow, 2] == null || arr[startRow, 2].ToString().Trim().Length == 0))
                {
                    break;
                }
            }

            for (int i = startRow + 1; i < endRow; ++i)
            {
                VietcomInfo info = new VietcomInfo("")
                {
                    From = VietcomInfo.SENDER_NAME,
                    ParseStatus = StatusBankInfo.Okay,
                };
                for (int j = 1; j <= numCol; ++j)
                {
                    if (arr[i, j] != null)
                    {
                        var text = arr[i, j].ToString().Trim();
                        if (j == colSTT)
                        {

                        }
                        else if (j == colNoiDung)
                        {
                            info.Reference = text;
                        }
                        else if (j == colSoDu)
                        {
                            text = text.Replace(",", "").Replace(".", "");
                            if (long.TryParse(text, out long value))
                            {
                                info.Total = value;
                            }
                        }
                        else if (j == colSoGhiCo)
                        {
                            text = text.Replace(",", "").Replace(".", "");
                            if (long.TryParse(text, out long value))
                            {
                                info.Delta = value;
                            }
                        }
                        else if (j == colSoGhiNo)
                        {
                            text = text.Replace(",", "").Replace(".", "");
                            if (long.TryParse(text, out long value))
                            {
                                info.Delta = -value;
                            }
                        }
                        else if (j == colDate)
                        {
                            int cut = Math.Min(text.IndexOf(" "), text.IndexOf("\n"));
                            if (cut > 0)
                            {
                                info.TimeString = text.Substring(0, cut);
                                info.Message = text.Substring(cut).Trim(); //used as ID
                                if (!DateTime.TryParse(info.TimeString, out info.Date))
                                {
                                    log.Error("Cannot parse date for vietcombank: " + info.TimeString);
                                };
                            }
                        }
                    }
                }
                listVietcom.Add(info);
            }
            return listVietcom;
        }

        private void LogBankInfo(List<SmsInfo> listSms)
        {
            var list = listSms.Select(i => i.MyBankInfo).Where(i => i != null && i.ParseStatus != StatusBankInfo.Ignored);
            foreach (BankInfoBase item in list)
            {
                log.Debug(item.ToString());
            }
        }

        public static string GetDatabasePath()
        {
            string ret = "";
            FileInfo templateFile = new FileInfo("finance.db");
            string targetFilePath = MySetting.Default.DatabasePath;
            string targetFolder = Path.GetDirectoryName(targetFilePath);
            if (templateFile.Exists && targetFolder.Length > 0 && Directory.Exists(targetFolder))
            {
                try
                {
                    File.Copy(templateFile.FullName, targetFilePath, false);
                }
                catch (Exception e1)
                {
                    log.Error("Cannot create database file at: " + targetFilePath, e1);
                }
                if (File.Exists(targetFilePath))
                {
                    ret = targetFilePath;
                }
            }
            return ret;
        }
    }
}
