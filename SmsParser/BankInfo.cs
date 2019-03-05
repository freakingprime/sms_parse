using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmsParser
{
    public enum StatusBankInfo
    {
        Okay, Ignored, Error
    }
    public class BankInfo
    {
        public long Delta = 0;
        public long Total = 0;
        public string Message = string.Empty;
        public string From = string.Empty;
        public StatusBankInfo ParseStatus = StatusBankInfo.Error;
        public string Time = "none";
        public string Reference = string.Empty;

        public const long SO_TAI_KHOAN = 0351000777576;
        public const string NAME_VIETCOMBANK = "vietcombank";
        public const string NAME_HSBC = "hsbc";

        private Regex regexChange = new Regex(@"(so du tk vcb|sd tk)\s*\d+.*?([+-][\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private Regex regexTime = new Regex(@"luc ([\d\s-:]+)", RegexOptions.IgnoreCase);
        private Regex regexTotal = new Regex(@"\.\s*(sd|so du)\s+([\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private Regex regexRefer = new Regex(@"\.\s*ref\s*(.+)", RegexOptions.IgnoreCase);

        private string[] ignoredKeywords = {
            "quy khach",
             "thu phi",
            "ma otp",
            "the vcb visa",
            "huy giao dich tren",
            "smartotp"
        };

        public BankInfo(string text)
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
            Match changeMatch = regexChange.Match(lower);
            Match totalMatch = regexTotal.Match(lower);

            Match timeMatch = regexTime.Match(lower);
            if (timeMatch.Success) Time = timeMatch.Groups[1].Value.Trim();

            Match referMatch = regexRefer.Match(text);
            if (referMatch.Success) Reference = referMatch.Groups[1].Value.Trim();

            if (changeMatch.Success && totalMatch.Success)
            {
                if (long.TryParse(changeMatch.Groups[2].Value.Replace(",", ""), out Delta)
                    && long.TryParse(totalMatch.Groups[2].Value.Replace(",", ""), out Total))
                {
                    ParseStatus = StatusBankInfo.Okay;
                }
            }
        }

        public override string ToString()
        {
            return "Delta " + Delta + " | SD: " + Total + " | Time: " + Time + " | Ref: " + Reference;
        }
    }
}
