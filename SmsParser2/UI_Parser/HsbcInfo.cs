using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser
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
                Total = 0;
                if (okay)
                {
                    ParseStatus = StatusBankInfo.Okay;
                }
            }
            else
            {
                changeMatch = regexChange2.Match(lower);
                if (changeMatch.Success)
                {
                    //giao dich bi huy
                    GroupCollection groups = changeMatch.Groups;
                    bool okay = long.TryParse(groups["amount"].Value.Replace(",", ""), out Delta);
                    Total = 0;
                    if (okay)
                    {
                        ParseStatus = StatusBankInfo.Okay;
                    }
                }
                else
                {
                    changeMatch = regexChange3.Match(lower);
                    if (changeMatch.Success)
                    {
                        //giao dich khac
                        GroupCollection groups = changeMatch.Groups;
                        bool okay = long.TryParse(groups["amount"].Value.Replace(",", ""), out Delta);
                        Delta = -Delta;
                        Total = 0;
                        if (okay)
                        {
                            ParseStatus = StatusBankInfo.Okay;
                        }
                    }
                }
            }
        }
        private readonly Regex regexChange1 = new Regex(@"the td.+?6291.+?thuc hien.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange2 = new Regex(@"the td.+?6291.+?bi huy.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange3 = new Regex(@"the td.+?6291.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);

        private readonly string[] ignoredKeywords = { "otp", "du no cuoi ky", "card.apply.hsbc" };

        public const string SENDER_NAME = "hsbc";
    }
}
