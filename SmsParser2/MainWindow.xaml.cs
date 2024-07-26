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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SmsParser2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        private static readonly LogController oldLog = LogController.Instance;

        public MainWindow()
        {
            InitializeComponent();
            log.Debug("Application start at: " + DateTime.Now.ToString());
            this.Title = Resource1.TITLE + " " + Resource1.VERSION + "." + Resource1.BuildTime.Trim();
#if DEBUG
            this.Title = this.Title + " debug";
#endif
            oldLog.SetTextBox(TxtLog);
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            string text = "Test log at: " + DateTime.Now.ToString();
            log.Debug(text);
        }

        private void TxtLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtLog.ScrollToEnd();
        }
    }
}
