using Dapper;
using log4net.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Simple1.MVVMBase;
using SmsParser2.DbModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;

namespace SmsParser2.UI_Parser
{
    public class ParserVm : ViewModelBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        private static readonly LogController oldLog = LogController.Instance;
        public ParserVm()
        {
            log.Info("New ViewModel is created");
            IsButtonEnabled = true;
            IsExportError = false;
        }

        #region Bind properties

        private bool _isExportError;

        public bool IsExportError { get => _isExportError; set => SetValue(ref _isExportError, value); }

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set { SetValue(ref _isButtonEnabled, value); }
        }

        #endregion

        #region Normal properties

        #endregion

        public async void ButtonImportVietcomExcel()
        {
            string outputFolder = MySetting.Default.OutputFolder;
            while (outputFolder.EndsWith('\\'))
            {
                outputFolder = outputFolder.Remove(outputFolder.Length - 1);
            }
            if (Directory.Exists(outputFolder) && MySetting.Default.FileNamePrefix.Length > 0)
            {
                MySetting.Default.BodyColumnWidth = Math.Max(MySetting.Default.BodyColumnWidth, 60);
                MySetting.Default.Save();

                IsButtonEnabled = false;
                await Task.Run(() =>
                {
                    //read vietcombank data
                    List<VietcomInfo> list = new List<VietcomInfo>();
                    if (Directory.Exists(MySetting.Default.NewVietcomFolder))
                    {
                        string[] files = Directory.GetFiles(MySetting.Default.NewVietcomFolder);
                        var listVietcomFiles = files.Where(f =>
                        {
                            string name = Path.GetFileName(f);
                            return (!name.Contains("~") && (name.Contains("vietcombank", StringComparison.OrdinalIgnoreCase) || name.Contains("lich-su-giao-dich", StringComparison.OrdinalIgnoreCase)) && name.Contains(".xls", StringComparison.OrdinalIgnoreCase));
                        }).ToArray();
                        for (int i = 0; i < listVietcomFiles.Length; ++i)
                        {
                            list.AddRange(ReadExcelFileVietcom(listVietcomFiles[i]));
                            oldLog.Debug("Processed " + (i + 1) + "/" + listVietcomFiles.Length + ": " + Path.GetFileNameWithoutExtension(listVietcomFiles[i]));
                        }
                    }
                    List<VietcomInfo> listUnique = new List<VietcomInfo>();
                    HashSet<string> hashID = new HashSet<string>();
                    foreach (var item in list)
                    {
                        string key = item.Ref + item.TimeString;
                        if (hashID.Add(key))
                        {
                            listUnique.Add(item);
                        }
                    }

                    //write to database
                    int rowAffected = 0;
                    Stopwatch sw = Stopwatch.StartNew();
                    using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
                    {
                        connection.Open();
                        var transaction = connection.BeginTransaction();
                        foreach (VietcomInfo bank in listUnique)
                        {
                            rowAffected += connection.Execute(@"INSERT OR IGNORE INTO bank (BankName,Date,Delta,Balance,TimeString,Ref) VALUES (@BankName,@Date,@Delta,@Balance,@TimeString,@Ref)", new { BankName = VietcomInfo.SENDER_NAME.ToUpper(), bank.Date, bank.Delta, bank.Balance, bank.TimeString, bank.Ref });
                        }
                        transaction.Commit();
                    }
                    sw.Stop();
                    oldLog.Debug("Insert " + rowAffected + " Vietcombank Info to database in: " + sw.ElapsedMilliseconds + " ms");
                });
                IsButtonEnabled = true;
            }
        }

        private List<VietcomInfo> ReadExcelFileVietcom(string excelPath)
        {
            log.Debug("Read transaction from Excel file: " + excelPath);
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
            Workbook workbook = excel.Workbooks.Open(excelPath);
            Worksheet sheet = (Worksheet)workbook.Worksheets.get_Item(1);
            Microsoft.Office.Interop.Excel.Range excelRange = sheet.UsedRange;
            var arr = (object[,])excelRange.Value2;
            int numRow = arr.GetLength(0);
            int numCol = arr.GetLength(1);

            int startRow = 1;
            int endRow = 1;

            int colDate = 0;
            int colSoGhiCo = 0;
            int colSoGhiNo = 0;
            int colSoDu = 0;
            int colNoiDung = 0;

            for (startRow = 1; startRow <= numRow; ++startRow)
            {
                if (arr[startRow, 1] != null)
                {
                    if (arr[startRow, 1].ToString().Contains("stt", StringComparison.OrdinalIgnoreCase))
                    {
                        for (int i = 1; i <= numCol; ++i)
                        {
                            if (arr[startRow, i] != null)
                            {
                                var text = arr[startRow, i].ToString().Trim().ToLower();
                                if (text.Contains("doc no") || text.Contains("date"))
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
                    if (arr[startRow, 2].ToString().Contains("stt", StringComparison.OrdinalIgnoreCase))
                    {
                        for (int i = 1; i <= numCol; ++i)
                        {
                            if (arr[startRow, i] != null)
                            {
                                var text = arr[startRow, i].ToString().Trim().ToLower();
                                if (text.Contains("doc no") || text.Contains("date"))
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

            if (colDate < 1 || colSoGhiNo < 1 || colSoGhiCo < 1 || colNoiDung < 1 || colSoDu < 1)
            {
                oldLog.Error("Vietcombank excel is not in correct format: " + excelPath);
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
                VietcomInfo info = new VietcomInfo()
                {
                    ParseStatus = StatusBankInfo.Okay,
                };
                for (int j = 1; j <= numCol; ++j)
                {
                    if (arr[i, j] != null)
                    {
                        var text = arr[i, j].ToString().Trim();
                        if (j == colNoiDung)
                        {
                            info.Ref = text;
                        }
                        else if (j == colSoDu)
                        {
                            text = text.Replace(",", "").Replace(".", "");
                            if (long.TryParse(text, out long value))
                            {
                                info.Balance = value;
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
                            int cut = Math.Min(text.IndexOf(' '), text.IndexOf('\n'));
                            if (cut > 0)
                            {
                                info.TimeString = text.Substring(0, cut);
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

        public async void ButtonImportSmsToDB()
        {
            oldLog.Debug("Begin import SMS to database");
            string inputFilePath = MySetting.Default.XMLFilePath;
            if (File.Exists(inputFilePath))
            {
                IsButtonEnabled = false;
                await Task.Run(() =>
                {
                    var tempSms = ReadSMSFile(inputFilePath);
                    ImportSmsToDatabase(tempSms);
                    var listSmsFull = LoadSmsFromDatabase();
                    var hashParsedSms = LoadParsedSmsIdFromDatabase();
                    ConvertSmsToBank(listSmsFull, hashParsedSms);
                    log.Info("haha");
                });
                IsButtonEnabled = true;
            }
            oldLog.Debug("Import is finished");
        }

        private void ImportSmsToDatabase(List<SmsInfo> listSms)
        {
            Stopwatch sw = Stopwatch.StartNew();
            int rowAffected = 0;
            using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                foreach (var item in listSms)
                {
                    rowAffected += connection.Execute(@"INSERT OR IGNORE INTO sms (Address,Body,Date,Type,ContactName) VALUES (@Address,@Body,@Date,@Type,@ContactName)", new { item.Address, item.Body, item.Date, item.Type, item.ContactName });
                }
                transaction.Commit();
            }
            sw.Stop();
            oldLog.Debug("Insert " + rowAffected + " SMS to database in: " + sw.ElapsedMilliseconds + " ms");
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
            oldLog.Debug("Loaded " + ret.Count + " SMS from database in: " + sw.ElapsedMilliseconds + " ms");
            return ret;
        }

        private HashSet<int> LoadParsedSmsIdFromDatabase()
        {
            HashSet<int> ret = new HashSet<int>();
            Stopwatch sw = Stopwatch.StartNew();
            using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
            {
                try
                {
                    var list = connection.Query<int?>(@"SELECT SmsID from bank");
                    foreach (var item in list.Where(i => i != null))
                    {
                        _ = ret.Add((int)item);
                    }
                }
                catch (Exception e1)
                {
                    oldLog.Error("Cannot get parsed SmsID from database", e1);
                }

            }
            sw.Stop();
            oldLog.Debug("Loaded " + ret.Count + " SmsID from database in: " + sw.ElapsedMilliseconds + " ms");
            return ret;
        }

        private void ConvertSmsToBank(List<SmsInfo> listSmsInfo, HashSet<int> hashIgnored)
        {
            List<string> listError = new List<string>();
            Stopwatch sw = Stopwatch.StartNew();
            int rowAffected = 0;
            using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                foreach (SmsInfo item in listSmsInfo)
                {
                    if (hashIgnored.Contains(item.ID))
                    {
                        // continue;
                    }
                    BankInfoBase bank = null;
                    switch (item.Address)
                    {
                        case VietcomInfo.SENDER_NAME:
                            bank = new VietcomInfo(item);
                            break;
                        case ShinhanInfo.SENDER_NAME:
                            bank = new ShinhanInfo(item);
                            break;
                        case HsbcInfo.SENDER_NAME:
                            bank = new HsbcInfo(item);
                            break;
                        case VpbankInfo.SENDER_NAME:
                            bank = new VpbankInfo(item);
                            break;
                    }
                    if (bank != null)
                    {
                        switch (bank.ParseStatus)
                        {
                            case StatusBankInfo.Error:
                                listError.Add(item.Address + "\t" + item.Body);
                                break;
                            case StatusBankInfo.Ignored:
                                break;
                            case StatusBankInfo.Okay:
                                //write to db
                                rowAffected += connection.Execute(@"INSERT OR IGNORE INTO bank (BankName,Date,Delta,Balance,TimeString,SmsID,Ref) VALUES (@BankName,@Date,@Delta,@Balance,@TimeString,@SmsID,@Ref)", new { BankName = item.Address.ToUpper(), bank.Date, bank.Delta, bank.Balance, bank.TimeString, SmsID = item.ID, bank.Ref });
                                break;
                        }
                    }
                }
                transaction.Commit();
            }
            if (listError.Count > 0)
            {
                oldLog.Error("Cannot parse SMS: " + listError.Count);
                if (IsExportError)
                {
                    listError.Sort();
                    listError.Insert(0, "Cannot parse:");
                    string output = @"D:\DOWNLOADED\cannotparse.txt";
                    File.WriteAllText(output, string.Join(Environment.NewLine, listError));
                    oldLog.Error("Error (" + (listError.Count - 1) + ") is exported to: " + output);
                }
            }
            sw.Stop();
            oldLog.Debug("Insert " + rowAffected + " bank info to database in: " + sw.ElapsedMilliseconds + " ms");
        }

        private List<SmsInfo> ReadSMSFile(string filePath)
        {
            oldLog.Debug("Read SMS data from file: " + filePath);
            List<SmsInfo> ret = new List<SmsInfo>();
            string str = File.ReadAllText(filePath);
            Regex regexXml = new Regex(@"<sms.+\/>");
            MatchCollection matchSmsTag = regexXml.Matches(str);
            foreach (Match match in matchSmsTag)
            {
                SmsInfo info = new SmsInfo(match.Value);
                ret.Add(info);
            }
            ret.Sort((x, y) => y.Date.CompareTo(x.Date));
            oldLog.Debug("SMS count: " + ret.Count);
            return ret;
        }


        public static string GetDatabasePath()
        {
            string ret = "";
            FileInfo templateFile = new FileInfo("finance.db");
            string targetFilePath = MySetting.Default.DatabasePath;
            string targetFolder = Path.GetDirectoryName(targetFilePath);
            if (templateFile.Exists && targetFolder.Length > 0 && Directory.Exists(targetFolder) && !File.Exists(targetFilePath))
            {
                try
                {
                    File.Copy(templateFile.FullName, targetFilePath, false);
                }
                catch (Exception e1)
                {
                    log.Error("Cannot create database file at: " + targetFilePath, e1);
                }
            }
            if (File.Exists(targetFilePath))
            {
                ret = targetFilePath;
            }
            return ret;
        }

        public async void ButtonExport()
        {
            oldLog.Debug("Export data to Excel");
            string outputPath = "";
            if (Directory.Exists(MySetting.Default.OutputFolder))
            {
                outputPath = MySetting.Default.OutputFolder + "\\" + MySetting.Default.FileNamePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
            }
            else
            {
                oldLog.Error("Directory does not exist: " + MySetting.Default.OutputFolder);
                return;
            }
            IsButtonEnabled = false;
            await Task.Run(() =>
            {
                List<DbBank> listBank = new List<DbBank>();
                using (var connection = new SqliteConnection("Data Source=\"" + GetDatabasePath() + "\""))
                {
                    var list = connection.Query<DbBank>(@"SELECT * from bank");
                    foreach (var item in list)
                    {
                        listBank.Add(item);
                    }
                }
                oldLog.Debug("Number of Bank Info: " + listBank.Count);
                listBank.Sort((x, y) =>
                {
                    if (x.Date.Equals(y.Date))
                    {
                        return x.Ref.CompareTo(y.Ref);
                    }
                    return x.Date.CompareTo(y.Date);
                });
                listBank.Reverse();
                ExcelWriter writer = new ExcelWriter();
                writer.ExportVietcomInfo(listBank, outputPath);
            });
            IsButtonEnabled = true;
        }
    }
}
