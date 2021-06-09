using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser.Model
{
    public class HsbcInfo : BankInfoBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public HsbcInfo(string text)
        {
            string lower = text.ToLower();
            foreach (string s in ignoredKeywords)
            {
                if (lower.Contains(s))
                {
                    ParseStatus = StatusBankInfo.Ignored;
                    return;
                }
            }
            Message = text;
            Match changeMatch = regexChange1.Match(lower);
            if (changeMatch.Success)
            {
                //giao dich thanh cong
                GroupCollection groups = changeMatch.Groups;
                bool okay = long.TryParse(groups["amount"].Value.Replace(",", ""), out Delta);
                Delta = -Delta;
                if (okay)
                {
                    ParseStatus = StatusBankInfo.Okay;
                }
            }
        }

        private Regex regexChange1 = new Regex(@"the td.+?6291.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private Regex regexChange2 = new Regex(@"giao dich bi huy.+?(?<date>\d\d-\d\d-\d\d\d\d)\/(?<time>\d\d:\d\d)\/(?<amount>[\d,]+)\/(?<ref>.+),han muc.+?(?<hanmuc>[\d,]+)", RegexOptions.IgnoreCase);
        private Regex regexChange3 = new Regex(@"tk.+thay doi\s+(?<sign>[+-])\s+VND\s+(?<amount>[\d,]+).+?so du kha dung.+?(?<sodu>[\d,]+)[.\s]+(?<ref>.+)", RegexOptions.IgnoreCase);

        private string[] ignoredKeywords = { "otp", "du no cuoi ky", "card.apply.hsbc" };

        public const string SENDER_NAME = "hsbc";
    }
}
