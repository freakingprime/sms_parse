using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Simple1.MVVMBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private string _txtXMLFileName;

        public string TxtXMLFileName
        {
            get { return _txtXMLFileName; }
            set { SetValue(ref _txtXMLFileName, value); }
        }


        private string _txtXMLFilePath;

        public string TxtXMLFilePath
        {
            get { return _txtXMLFilePath; }
            set
            {
                SetValue(ref _txtXMLFilePath, value);
                TxtXMLFileName = Path.GetFileName(value);
                MySetting.Default.LastOpenedFile = value;
                MySetting.Default.Save();
            }
        }

        private string _txtVietcomFolder;

        public string TxtVietcomFolder
        {
            get { return _txtVietcomFolder; }
            set { SetValue(ref _txtVietcomFolder, value); }
        }

        private string _txtNewVietcomFolder;

        public string TxtNewVietcomFolder
        {
            get { return _txtNewVietcomFolder; }
            set { SetValue(ref _txtNewVietcomFolder, value); }
        }

        private string _txtOutputFolder;

        public string TxtOutputFolder
        {
            get { return _txtOutputFolder; }
            set { SetValue(ref _txtOutputFolder, value); }
        }

        private string _txtExcelColumnWidth;

        public string TxtExcelColumnWidth
        {
            get { return _txtExcelColumnWidth; }
            set { SetValue(ref _txtExcelColumnWidth, value); }
        }

        private string _txtFilenamePrefix;

        public string TxtFilenamePrefix
        {
            get { return _txtFilenamePrefix; }
            set { SetValue(ref _txtFilenamePrefix, value); }
        }

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set { SetValue(ref _isButtonEnabled, value); }
        }

        #endregion

        #region Normal properties

        private const string FILE_PATH = @"D:\DOWNLOAD\sms-20200824033231.xml";
        private List<SmsInfo> listSms = new List<SmsInfo>();

        #endregion

        #region Button command

        public void BtnLoadLatestFile()
        {
            string lastFile = MySetting.Default.LastOpenedFile;
            if (!File.Exists(lastFile))
            {
                while (!Directory.Exists(lastFile) && lastFile.LastIndexOf(Path.DirectorySeparatorChar) > 0)
                {
                    lastFile = lastFile.Substring(0, lastFile.LastIndexOf(Path.DirectorySeparatorChar));
                }
            }
            else
            {
                lastFile = Path.GetDirectoryName(lastFile);
            }
            if (Directory.Exists(lastFile))
            {
                string[] files = Directory.GetFiles(lastFile);
                Array.Sort(files);
                for (int i = files.Length - 1; i >= 0; --i)
                {
                    if (files[i].Contains("sms"))
                    {
                        TxtXMLFilePath = files[i];
                        break;
                    }
                }
            }
        }

        public void BtnBrowseXmlFileClick()
        {
            log.Info("Clicked button browse XML file");
            string lastFile = MySetting.Default.LastOpenedFile;
            if (!File.Exists(lastFile))
            {
                while (!Directory.Exists(lastFile) && lastFile.LastIndexOf(Path.DirectorySeparatorChar) > 0)
                {
                    lastFile = lastFile.Substring(0, lastFile.LastIndexOf(Path.DirectorySeparatorChar));
                }
            }
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "SMS files|*.txt;*.xml",
                Title = "Select SMS file",
                Multiselect = false,
                InitialDirectory = lastFile
            };
            if (dialog.ShowDialog() == true)
            {
                log.Debug("Selected file: " + dialog.FileName);
                TxtXMLFilePath = dialog.FileName;
                MySetting.Default.LastOpenedFile = dialog.FileName;
                MySetting.Default.Save();
            }
        }

        public void BtnBrowseOutputFolderClick()
        {
            log.Info("Clicked button browse Output folder");
            string folder = MySetting.Default.LastOutputFolder;
            while (!Directory.Exists(folder) && folder.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                folder = folder.Substring(0, folder.LastIndexOf(Path.DirectorySeparatorChar));
            }
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select where to save output file",
                SelectedPath = folder,
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                TxtOutputFolder = dialog.SelectedPath;
                MySetting.Default.LastOutputFolder = dialog.SelectedPath;
                MySetting.Default.Save();
            }
        }

        public void BtnBrowseVietcomFolderClick()
        {
            log.Info("Clicked button browse Output folder");
            string folder = MySetting.Default.LastVietcomFolder;
            while (!Directory.Exists(folder) && folder.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                folder = folder.Substring(0, folder.LastIndexOf(Path.DirectorySeparatorChar));
            }
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select where to save output file",
                SelectedPath = folder,
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                TxtVietcomFolder = dialog.SelectedPath;
                MySetting.Default.LastVietcomFolder = dialog.SelectedPath;
                MySetting.Default.Save();
            }
        }

        public void BtnBrowseNewVietcomFolderClick()
        {
            log.Info("Clicked button browse new vietcom folder");
            string folder = MySetting.Default.LastNewVietcomFolder;
            while (!Directory.Exists(folder) && folder.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                folder = folder.Substring(0, folder.LastIndexOf(Path.DirectorySeparatorChar));
            }
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select where to save output file",
                SelectedPath = folder,
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                TxtNewVietcomFolder = dialog.SelectedPath;
                MySetting.Default.LastNewVietcomFolder = dialog.SelectedPath;
                MySetting.Default.Save();
            }
        }

        public async void BtnExportVietcomClick()
        {
            log.Info("Clicked button export to Excel file");
            TxtOutputFolder = TxtOutputFolder.Trim();
            TxtFilenamePrefix = TxtFilenamePrefix.Trim();
            TxtVietcomFolder = TxtVietcomFolder.Trim();
            TxtNewVietcomFolder = TxtNewVietcomFolder.Trim();

            string outputFolder = TxtOutputFolder;
            while (outputFolder.EndsWith("\\"))
            {
                outputFolder = outputFolder.Remove(outputFolder.Length - 1);
            }

            if (!int.TryParse(TxtExcelColumnWidth.Trim(), out int width) || width < 5)
            {
                width = 60;
            }

            if (Directory.Exists(outputFolder) && TxtFilenamePrefix.Length > 0)
            {
                MySetting.Default.LastOutputFolder = outputFolder;
                MySetting.Default.FileNamePrefix = TxtFilenamePrefix;
                MySetting.Default.BodyColumnWidth = width;
                MySetting.Default.LastVietcomFolder = TxtVietcomFolder;
                MySetting.Default.LastNewVietcomFolder = TxtNewVietcomFolder;
                MySetting.Default.Save();

                IsButtonEnabled = false;
                var t = Task.Run(() =>
                {
                    //read vietcombank data
                    List<VietcomInfo> list = new List<VietcomInfo>();
                    if (Directory.Exists(TxtVietcomFolder))
                    {
                        string[] files = Directory.GetFiles(TxtVietcomFolder);
                        foreach (var f in files)
                        {
                            string name = Path.GetFileName(f);
                            if (!name.Contains("~") && (name.Contains("vietcombank", StringComparison.OrdinalIgnoreCase) || name.Contains("lich-su-giao-dich", StringComparison.OrdinalIgnoreCase)) && name.Contains(".xls", StringComparison.OrdinalIgnoreCase))
                            {
                                list.AddRange(ReadExcelFileVietcom(f));
                            }
                        }
                    }
                    if (Directory.Exists(TxtNewVietcomFolder))
                    {
                        string[] files = Directory.GetFiles(TxtNewVietcomFolder);
                        foreach (var f in files)
                        {
                            string name = Path.GetFileName(f);
                            if (!name.Contains("~") && name.ToLower().Contains("vietcombank") && name.ToLower().Contains(".xls"))
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
                    writer.ExportVietcomInfo(list2, outputFolder + "\\" + TxtFilenamePrefix + "_Vietcom_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
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
            TxtXMLFilePath = TxtXMLFilePath.Trim();
            TxtOutputFolder = TxtOutputFolder.Trim();
            TxtFilenamePrefix = TxtFilenamePrefix.Trim();

            string inputFilePath = TxtXMLFilePath;
            string outputFolder = TxtOutputFolder;
            while (outputFolder.EndsWith("\\"))
            {
                outputFolder = outputFolder.Remove(outputFolder.Length - 1);
            }

            if (!int.TryParse(TxtExcelColumnWidth.Trim(), out int width) || width < 5)
            {
                width = 60;
            }

            if (File.Exists(inputFilePath) && Directory.Exists(outputFolder) && TxtFilenamePrefix.Length > 0)
            {
                MySetting.Default.LastOpenedFile = inputFilePath;
                MySetting.Default.LastOutputFolder = outputFolder;
                MySetting.Default.FileNamePrefix = TxtFilenamePrefix;
                MySetting.Default.BodyColumnWidth = width;
                MySetting.Default.Save();

                IsButtonEnabled = false;
                var t = Task.Run(() =>
                {
                    ReadSMSFile(inputFilePath);
                    //logBankInfo();

                    log.Info("Process data to folder: " + outputFolder);
                    ExcelWriter writer = new ExcelWriter(SmsInfo.EXCEL_HEADER);
                    log.Info("Created new excel writer");

                    //if you need to do something to test the writer
                    writer.TestFunction();

                    //2021.06.08: Disable date suffix
                    writer.ExportSmsInfo(listSms, outputFolder + "\\" + TxtFilenamePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
                    log.Info("Finish process data");
                });
                await t;
                _ = MessageBox.Show("Exported to " + outputFolder, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                IsButtonEnabled = true;
            }
        }

        #endregion

        public void LoadOldSettings()
        {
            log.Info("Load old settings");
            if (File.Exists(MySetting.Default.LastOpenedFile))
            {
                TxtXMLFilePath = MySetting.Default.LastOpenedFile;
            }
            if (Directory.Exists(MySetting.Default.LastOutputFolder))
            {
                TxtOutputFolder = MySetting.Default.LastOutputFolder;
            }
            if (Directory.Exists(MySetting.Default.LastVietcomFolder))
            {
                TxtVietcomFolder = MySetting.Default.LastVietcomFolder;
            }
            if (Directory.Exists(MySetting.Default.LastNewVietcomFolder))
            {
                TxtNewVietcomFolder = MySetting.Default.LastNewVietcomFolder;
            }
            TxtExcelColumnWidth = MySetting.Default.BodyColumnWidth.ToString();
            TxtFilenamePrefix = MySetting.Default.FileNamePrefix;
            log.Info(string.Format("XML {0} | Output {1} | Column width {2} | Prefix {3}", TxtXMLFilePath, TxtOutputFolder, TxtExcelColumnWidth, TxtFilenamePrefix));
            log.Info("Load setting successfully");
        }

        private void ReadSMSFile(string filePath)
        {
            log.Debug("Read data from file: " + filePath);
            string str = File.ReadAllText(filePath);
            Regex regexXml = new Regex(@"<sms.+\/>");
            MatchCollection matchSmsTag = regexXml.Matches(str);
            listSms.Clear();
            foreach (Match match in matchSmsTag)
            {
                SmsInfo info = new SmsInfo(match.Value);
                listSms.Add(info);
            }
            listSms.Sort((x, y) => y.DateAsNumber.CompareTo(x.DateAsNumber));
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

        private void LogBankInfo()
        {
            var list = listSms.Select(i => i.MyBankInfo).Where(i => i != null && i.ParseStatus != StatusBankInfo.Ignored);
            foreach (BankInfoBase item in list)
            {
                log.Debug(item.ToString());
            }
        }

    }
}
