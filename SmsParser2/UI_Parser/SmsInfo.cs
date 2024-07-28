using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmsParser2.UI_Parser
{
    public class SmsInfo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public int ID;
        public string Address = string.Empty;
        public string Body = string.Empty;
        public DateTime Date = DateTime.MinValue;
        public int Type;
        public string ContactName = string.Empty;
        public BankInfoBase MyBankInfo = null;

        private string GetValue(string key, string text)
        {
            Regex regex = new Regex(key + @"=\""(.+?)\""");
            Match match = regex.Match(text);
            if (match.Success && match.Groups[1].Value.Length > 8100)
            {
                log.Error("Data too long");
            }
            return match.Success ? match.Groups[1].Value : "null";
        }

        public SmsInfo()
        {
            //do nothing
        }

        public SmsInfo(string xmlText)
        {
            //log.Debug("Create new object from text: " + xmlText);
            Address = GetValue("address", xmlText).ToLower();
            if (Address.StartsWith("+84")) Address = Address.Replace("+84", "0");
            if (long.TryParse(GetValue("date", xmlText), out long unix))
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds(unix).LocalDateTime;
            }
            Type = int.Parse(GetValue("type", xmlText));
            Body = GetValue("body", xmlText).Trim();
            ContactName = GetValue("contact_name", xmlText);
        }
    }
}
