using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser
{
    public class VpbankInfo : BankInfoBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public VpbankInfo(string text, DateTime date)
        {
            this.Date = date;
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
            }

            //find time
            Match timeMatch = regexTime.Match(Message);
            if (timeMatch.Success)
            {
                if (DateTime.TryParseExact(timeMatch.Groups[1].Value, "HH:mm dd/MM", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime dateValue))
                {
                    TimeString = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }
        private readonly Regex regexChange1 = new Regex(@"the vpbank 5.+?4985.+?chi tieu\s+(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange2 = new Regex(@"the vpbank 5.+?4985.+?ghi co\s+(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexTime = new Regex(@"luc ([\d\s/-:]+)", RegexOptions.IgnoreCase);

        private readonly string[] ignoredKeywords = { "otp", "ky han", "ma xac thuc", "khong giao dich", "031090010681", "1900545415", "mat khau", "phe duyet", "phat hanh" };

        public const string SENDER_NAME = "vpbank";
    }
}
