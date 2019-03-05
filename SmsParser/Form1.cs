using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace SmsParser
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public Form1()
        {
            InitializeComponent();
            if (File.Exists(Properties.Settings.Default.LastOpenedFile)) txtPathXml.Text = Properties.Settings.Default.LastOpenedFile;
            if (Directory.Exists(Properties.Settings.Default.LastOutputFolder)) txtPathOutput.Text = Properties.Settings.Default.LastOutputFolder;
            txtBodyWidth.Text = Properties.Settings.Default.BodyColumnWidth.ToString();
            txtFileNamePrefix.Text = Properties.Settings.Default.FileNamePrefix;
            Text = "SmsParse " + Properties.Resources.VERSION + " build " + Properties.Resources.BuildTime;
        }

        private const string FILE_PATH = "D:\\DOWNLOAD\\sms-20190305032455.xml";
        private List<SmsInfo> listSms = new List<SmsInfo>();

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

        private void setButtonStatus(bool status)
        {
            btnBrowseOutput.Enabled = status;
            btnBrowseXml.Enabled = status;
            btnExportExcel.Enabled = status;
            txtBodyWidth.Enabled = status;
            txtPathOutput.Enabled = status;
            txtPathXml.Enabled = status;
            txtFileNamePrefix.Enabled = status;
        }

        private void process(string folder)
        {
            log.Debug("Process data");
            ExcelWriter writer = new ExcelWriter(SmsInfo.EXCEL_HEADER);
            writer.ExportSmsInfo(listSms, folder + "\\" + txtFileNamePrefix.Text + " _" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
        }

        private void readBankingInfo()
        {
            var list = listSms.Select(i => i.Bank).Where(i => i != null && i.ParseStatus == StatusBankInfo.Okay);
            foreach (BankInfo item in list)
            {
                log.Debug(item.ToString());
            }
        }

        private void btnBrowseXml_Click(object sender, EventArgs e)
        {
            string lastFile = Properties.Settings.Default.LastOpenedFile;
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
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                log.Debug("Selected file: " + dialog.FileName);
                txtPathXml.Text = dialog.FileName;
                Properties.Settings.Default.LastOpenedFile = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folder = Properties.Settings.Default.LastOutputFolder;
            while (!Directory.Exists(folder) && folder.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                folder = folder.Substring(0, folder.LastIndexOf(Path.DirectorySeparatorChar));
            }
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = folder;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPathOutput.Text = dialog.SelectedPath;
                Properties.Settings.Default.LastOutputFolder = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            string input = txtPathXml.Text.Trim();
            string output = txtPathOutput.Text.Trim();
            while (output.EndsWith("\\")) output = output.Remove(output.Length - 1);

            int width = 60;
            int.TryParse(txtBodyWidth.Text.Trim(), out width);

            if (File.Exists(input) && Directory.Exists(output) && txtFileNamePrefix.Text.Trim().Length > 0)
            {
                Properties.Settings.Default.LastOpenedFile = input;
                Properties.Settings.Default.LastOutputFolder = output;
                Properties.Settings.Default.FileNamePrefix = txtFileNamePrefix.Text.Trim();
                Properties.Settings.Default.BodyColumnWidth = width;
                Properties.Settings.Default.Save();

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (ws, we) =>
                {
                    readFile(input);
                    readBankingInfo();
                    process(output);
                };
                worker.RunWorkerCompleted += (ws, we) =>
                {
                    setButtonStatus(true);
                    MessageBox.Show("Exported to " + output, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                setButtonStatus(false);
                worker.RunWorkerAsync();
            }
        }
    }
}
