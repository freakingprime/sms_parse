using SmsParser2.UI_Parser.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmsParser2
{
    public class SmsInfo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public SmsInfo()
        {
            //nothing
        }

        public string Address = String.Empty;
        public long DateAsNumber;
        public int Type;
        public string Subject = String.Empty;
        public string Body = String.Empty;
        public string DateSent = String.Empty;
        public string ReadableDate = String.Empty;
        public DateTime Date = new DateTime();
        public string ContactName = String.Empty;
        public CultureInfo enUS = new CultureInfo("en-US");
        public BankInfoBase MyBankInfo = null;

        private string getValue(string key, string text)
        {
            Regex regex = new Regex(key + "=\\\"(.+?)\\\"");
            Match match = regex.Match(text);
            if (match.Success && match.Groups[1].Value.Length > 8100) log.Error("Data too long");
            if (match.Success) return match.Groups[1].Value;
            else return "null";
        }

        public SmsInfo(string xmlText)
        {
            //log.Debug("Create new object from text: " + xmlText);
            Address = getValue("address", xmlText).ToLower();
            if (Address.StartsWith("+84")) Address = Address.Replace("+84", "0");
            DateAsNumber = long.Parse(getValue("date", xmlText));
            Type = int.Parse(getValue("type", xmlText));
            Subject = getValue("subject", xmlText);
            Body = getValue("body", xmlText).Trim();
            DateSent = getValue("date_sent", xmlText);
            ReadableDate = getValue("readable_date", xmlText);
            if (!DateTime.TryParseExact(ReadableDate, "yyyy/MM/dd HH:mm:ss", enUS, DateTimeStyles.None, out Date))
            {
                log.Error("Cannot parse to DateTime: " + ReadableDate);
            }
            ContactName = getValue("contact_name", xmlText);
            if (Address.Equals(VietcomInfo.SENDER_NAME))
            {
                MyBankInfo = new VietcomInfo(Body);
            }
            else if (Address.Contains(ShinhanInfo.SENDER_NAME))
            {
                MyBankInfo = new ShinhanInfo(Body);
            }
            else if (Address.Contains(HsbcInfo.SENDER_NAME))
            {
                MyBankInfo = new HsbcInfo(Body);
            }
            else if (Address.Contains(VpbankInfo.SENDER_NAME))
            {
                MyBankInfo = new VpbankInfo(Body);
            }
            if (MyBankInfo != null && MyBankInfo.ParseStatus == StatusBankInfo.Error)
            {
                log.Error("Cannot parse BankInfo from " + Address + ": " + Body);
            }
        }

        public static readonly string[] EXCEL_HEADER = { "Address", "Date", "Name", "Type", "Body" };
        public static readonly string[] BANK_HEADER = { "Address", "Date", "Amount", "SD", "Time", "Ref" };

        public string[] GetValueArray()
        {
            List<string> list = new List<string>();
            list.Add(Address);
            list.Add(ReadableDate);
            list.Add(ContactName);
            if (Type == 1)
            {
                list.Add("Received");
            }
            else
            {
                list.Add("Sent");
            }
            list.Add("'" + Body);
            return list.ToArray();
        }

        public string[] GetBankArray()
        {
            List<string> list = new List<string>();
            list.Add(Address);
            list.Add(ReadableDate);
            list.Add(MyBankInfo.Delta + "");
            list.Add(MyBankInfo.Total + "");
            list.Add("T " + MyBankInfo.TimeString);
            if (MyBankInfo.Reference.Length > 0)
            {
                list.Add(MyBankInfo.Reference);
            }
            else
            {
                list.Add("Full: " + Body);
            }
            return list.ToArray();
        }
    }
}
