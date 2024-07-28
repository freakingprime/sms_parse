using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SmsParser2.UI_Parser
{
    /// <summary>
    /// Interaction logic for ParserView.xaml
    /// </summary>
    public partial class ParserView : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public ParserView()
        {
            InitializeComponent();
            if (File.Exists(MySetting.Default.DatabasePath))
            {
                TxtDatabasePath.Text = MySetting.Default.DatabasePath;
            }
            if (Directory.Exists(MySetting.Default.OutputFolder))
            {
                TxtOutputFile.Text = MySetting.Default.OutputFolder;
            }
            if (Directory.Exists(MySetting.Default.NewVietcomFolder))
            {
                TxtNewVietcomFolder.Text = MySetting.Default.NewVietcomFolder;
            }
            if (File.Exists(MySetting.Default.XMLFilePath))
            {
                TxtXmlFile.Text = MySetting.Default.XMLFilePath;
            }
            TxtColumnWidth.Text = MySetting.Default.BodyColumnWidth.ToString();
            TxtPrefix.Text = MySetting.Default.FileNamePrefix;
        }

        #region Normal properties

        private ParserVm context = null;

        #endregion

        private void BtnBrowseXMLFile_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked button browse XML file");
            string lastFile = MySetting.Default.XMLFilePath;
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
                TxtXmlFile.Text = dialog.FileName;
            }
        }

        private void BtnBrowseOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            string folder = MySetting.Default.OutputFolder;
            while (!Directory.Exists(folder) && folder.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                folder = folder.Substring(0, folder.LastIndexOf(Path.DirectorySeparatorChar));
            }
            OpenFolderDialog dialog = new OpenFolderDialog()
            {
                InitialDirectory = folder,
            };
            if (dialog.ShowDialog() == true)
            {
                TxtOutputFile.Text = dialog.FolderName;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                if (e.NewValue is ParserVm vm)
                {
                    log.Info("Data context is set");
                    context = vm;
                }
                else
                {
                    log.Error("Data context is changed but it's not ViewModel object");
                }
            }
            else
            {
                log.Error("Data context is changed but it's null");
            }
        }

        private void BtnLoadLatest_Click(object sender, RoutedEventArgs e)
        {
            string lastFile = MySetting.Default.XMLFilePath;
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
                    if (files[i].Contains("sms-20", StringComparison.OrdinalIgnoreCase))
                    {
                        TxtXmlFile.Text = files[i];
                        MySetting.Default.Save();
                        break;
                    }
                }
            }
        }

        private void BtnBrowseNewVietcomFolder_Click(object sender, RoutedEventArgs e)
        {
            string folder = MySetting.Default.NewVietcomFolder;
            while (!Directory.Exists(folder) && folder.LastIndexOf(Path.DirectorySeparatorChar) > 0)
            {
                folder = folder.Substring(0, folder.LastIndexOf(Path.DirectorySeparatorChar));
            }
            OpenFolderDialog dialog = new OpenFolderDialog()
            {
                InitialDirectory = folder,
            };
            if (dialog.ShowDialog() == true)
            {
                TxtNewVietcomFolder.Text = dialog.FolderName;
            }
        }

        private void BtnExportVietcom_Click(object sender, RoutedEventArgs e)
        {
            context.BtnExportVietcomClick();
        }

        private void BtnBrowseDatabase_Click(object sender, RoutedEventArgs e)
        {
            string oldDir = Path.GetDirectoryName(MySetting.Default.DatabasePath);
            var dialog = new OpenFileDialog()
            {
                InitialDirectory = Directory.Exists(oldDir) ? oldDir : "",
                Multiselect = false,
                CheckFileExists = false,
            };
            if (dialog.ShowDialog() == true)
            {
                TxtDatabasePath.Text = dialog.FileName;
            }
        }

        private void TxtDatabasePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySetting.Default.DatabasePath = ((TextBox)sender).Text;
            MySetting.Default.Save();
        }

        private void TxtOutputFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySetting.Default.OutputFolder = ((TextBox)sender).Text;
            MySetting.Default.Save();
        }

        private void TxtColumnWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(((TextBox)sender).Text, out int val))
            {
                MySetting.Default.BodyColumnWidth = val;
                MySetting.Default.Save();
            }
        }

        private void TxtPrefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySetting.Default.FileNamePrefix = ((TextBox)sender).Text;
            MySetting.Default.Save();
        }

        private void TxtNewVietcomFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySetting.Default.NewVietcomFolder = ((TextBox)sender).Text;
            MySetting.Default.Save();
        }

        private void TxtXmlFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySetting.Default.XMLFilePath = ((TextBox)sender).Text;
            MySetting.Default.Save();
            TxtXmlName.Content = Path.GetFileNameWithoutExtension(((TextBox)sender).Text);
        }

        private void BtnImportSms_Click(object sender, RoutedEventArgs e)
        {
            context?.ButtonImportSmsToDB();
        }
    }
}
