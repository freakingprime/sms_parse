using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser
{
    public class VietcomInfo : BankInfoBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public VietcomInfo(SmsInfo sms)
        {
            this.Date = sms.Date.AddMilliseconds(-sms.Date.Millisecond);
            foreach (string s in ignoredKeywords)
            {
                if (sms.Body.Contains(s, StringComparison.OrdinalIgnoreCase))
                {
                    ParseStatus = StatusBankInfo.Ignored;
                    return;
                }
            }
            string lower = sms.Body.ToLower();
            Match changeMatch = regexChange.Match(lower);
            Match totalMatch = regexTotal.Match(lower);

            Match timeMatch = regexTime.Match(lower);
            if (timeMatch.Success)
            {
                TimeString = timeMatch.Groups[1].Value.Trim();
            }
            Match referMatch = regexRefer.Match(sms.Body);
            if (referMatch.Success)
            {
                Ref = referMatch.Groups[1].Value.Trim();
            }
            if (changeMatch.Success && totalMatch.Success)
            {
                if (long.TryParse(changeMatch.Groups[2].Value.Replace(",", ""), out Delta)
                    && long.TryParse(totalMatch.Groups[2].Value.Replace(",", ""), out Balance))
                {
                    ParseStatus = StatusBankInfo.Okay;
                }
            }
        }

        public VietcomInfo()
        {

        }

        private readonly Regex regexChange = new Regex(@"(so du tk vcb|sd tk)\s*\d+.*?([+-][\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private readonly Regex regexTime = new Regex(@"luc ([\d\s-:]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexTotal = new Regex(@"\.\s*(sd|so du)\s+([\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private readonly Regex regexRefer = new Regex(@"\.\s*ref\s*(.+)", RegexOptions.IgnoreCase);

        private readonly string[] ignoredKeywords = { "quy khach", "thu phi", "ma otp", "the vcb visa", "huy giao dich tren", "smartotp", "1900545413", "tinh nang an toan bao mat 3D secure" };

        public const string SENDER_NAME = "vietcombank";
    }
}
