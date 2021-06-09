using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmsParser2
{
    public enum StatusBankInfo
    {
        Okay, Ignored, Error
    }

    public abstract class BankInfoBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public BankInfoBase()
        {

        }

        public long Delta = -2;
        public long Total = -2;
        public string Message = string.Empty;
        public string From = string.Empty;
        public StatusBankInfo ParseStatus = StatusBankInfo.Error;
        public string TimeString = "none";
        public string Reference = string.Empty;

        public override string ToString()
        {
            return "Delta " + Delta + " | SD: " + Total + " | Time: " + TimeString + " | Ref: " + Reference;
        }
    }
}
