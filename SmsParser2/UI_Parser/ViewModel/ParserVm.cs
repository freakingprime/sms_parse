using Microsoft.Win32;
using Simple1.MVVMBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SmsParser2.UI_Parser.ViewModel
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

        private string _txtXMLFilePath;

        public string TxtXMLFilePath
        {
            get { return _txtXMLFilePath; }
            set { SetValue(ref _txtXMLFilePath, value); }
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

        public void BtnBrowseXmlFileClick()
        {
            log.Info("Clicked button browse XML file");
            string lastFile = MySetting.Default.LastOpenedFile;
            while (!File.Exists(lastFile) && lastFile.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                lastFile = lastFile.Substring(0, lastFile.LastIndexOf(Path.DirectorySeparatorChar));
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

        public void BtnExportClick()
        {
            log.Info("Clicked button export to Excel file");
            TxtXMLFilePath = TxtXMLFilePath.Trim();
            TxtOutputFolder = TxtOutputFolder.Trim();
            TxtFilenamePrefix = TxtFilenamePrefix.Trim();

            string input = TxtXMLFilePath;
            string output = TxtOutputFolder;
            while (output.EndsWith("\\")) output = output.Remove(output.Length - 1);

            int width = 60;
            int.TryParse(TxtExcelColumnWidth.Trim(), out width);
            if (width < 5) width = 60;

            if (File.Exists(input) && Directory.Exists(output) && TxtFilenamePrefix.Length > 0)
            {
                MySetting.Default.LastOpenedFile = input;
                MySetting.Default.LastOutputFolder = output;
                MySetting.Default.FileNamePrefix = TxtFilenamePrefix;
                MySetting.Default.BodyColumnWidth = width;
                MySetting.Default.Save();

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (ws, we) =>
                {
                    readFile(input);
                    //logBankInfo();
                    process(output);
                };
                worker.RunWorkerCompleted += (ws, we) =>
                {
                    IsButtonEnabled = true;
                    MessageBox.Show("Exported to " + output, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                IsButtonEnabled = false;
                worker.RunWorkerAsync();
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
            TxtExcelColumnWidth = MySetting.Default.BodyColumnWidth.ToString();
            TxtFilenamePrefix = MySetting.Default.FileNamePrefix;
            log.Info(string.Format("XML {0} | Output {1} | Column width {2} | Prefix {3}", TxtXMLFilePath, TxtOutputFolder, TxtExcelColumnWidth, TxtFilenamePrefix));
        }

        private void readFile(string path)
        {
            log.Debug("Read data from file: " + path);
            string str = File.ReadAllText(path);
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

        private void process(string folder)
        {
            log.Debug("Process data to folder: " + folder);
            ExcelWriter writer = new ExcelWriter(SmsInfo.EXCEL_HEADER);
            log.Debug("Created new excel writer");
            writer.TestFunction();
            writer.ExportSmsInfo(listSms, folder + "\\" + TxtFilenamePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
            log.Debug("Finish process data");
        }

        private void logBankInfo()
        {
            var list = listSms.Select(i => i.Bank).Where(i => i != null && i.ParseStatus != StatusBankInfo.Ignored);
            foreach (BankInfo item in list)
            {
                log.Debug(item.ToString());
            }
        }

    }
}
