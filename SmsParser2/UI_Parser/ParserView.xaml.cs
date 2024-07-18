using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
        }

        #region Normal properties

        private ParserVm context = null;

        #endregion

        private void BtnBrowseXMLFile_Click(object sender, RoutedEventArgs e)
        {
            context.BtnBrowseXmlFileClick();
        }

        private void BtnBrowseOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            context.BtnBrowseOutputFolderClick();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            context.BtnExportClick();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                if (e.NewValue is ParserVm)
                {
                    log.Info("Data context is set");
                    context = (ParserVm)e.NewValue;
                    context.LoadOldSettings();
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
            context.BtnLoadLatestFile();
        }

        private void BtnBrowseVietcomFolder_Click(object sender, RoutedEventArgs e)
        {

            context.BtnBrowseVietcomFolderClick();
        }

        private void BtnBrowseNewVietcomFolder_Click(object sender, RoutedEventArgs e)
        {
            context.BtnBrowseNewVietcomFolderClick();
        }

        private void BtnExportVietcom_Click(object sender, RoutedEventArgs e)
        {
            context.BtnExportVietcomClick();
        }
    }
}
