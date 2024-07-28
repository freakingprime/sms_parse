using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmsParser2.UI_Parser
{
    public enum StatusBankInfo
    {
        Okay, Ignored, Error
    }

    public abstract class BankInfoBase : IComparable<BankInfoBase>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public BankInfoBase()
        {

        }

        public DateTime Date = DateTime.MinValue;
        public long Delta = -2;
        public long Balance = -2;
        public string TimeString = "none";
        public string Ref = string.Empty;
        public StatusBankInfo ParseStatus = StatusBankInfo.Error;

        public override string ToString()
        {
            return "Delta " + Delta + " | Balance: " + Balance + " | Time: " + TimeString + " | Ref: " + Ref;
        }

        public int CompareTo(BankInfoBase other)
        {
            if (Date.Equals(other.Date))
            {
                return Ref.CompareTo(other.Ref);
            }
            return Date.CompareTo(other.Date);
        }
    }
}
