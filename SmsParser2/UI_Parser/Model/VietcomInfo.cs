using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser.Model
{
    public class VietcomInfo : BankInfoBase, IComparable<VietcomInfo>, IEquatable<VietcomInfo>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public VietcomInfo(string text)
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
            if (timeMatch.Success) TimeString = timeMatch.Groups[1].Value.Trim();

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

        public VietcomInfo()
        {

        }

        private readonly Regex regexChange = new Regex(@"(so du tk vcb|sd tk)\s*\d+.*?([+-][\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private readonly Regex regexTime = new Regex(@"luc ([\d\s-:]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexTotal = new Regex(@"\.\s*(sd|so du)\s+([\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private readonly Regex regexRefer = new Regex(@"\.\s*ref\s*(.+)", RegexOptions.IgnoreCase);

        private readonly string[] ignoredKeywords = { "quy khach", "thu phi", "ma otp", "the vcb visa", "huy giao dich tren", "smartotp", "1900545413" };

        public const string SENDER_NAME = "vietcombank";

        public DateTime Date = DateTime.MinValue;

        public static readonly string[] VIETCOM_HEADER = { "Date", "ID", "Amount", "Balance", "Ref" };
        public string[] GetValueArray()
        {
            List<string> list = new List<string>();
            list.Add(Date.ToString("yyyy/MM/dd"));
            list.Add(Message);
            list.Add(Delta + "");
            list.Add(Total + "");
            list.Add(Reference);
            return list.ToArray();
        }

        public int CompareTo([AllowNull] VietcomInfo other)
        {
            int t = Date.CompareTo(other.Date);
            if (t == 0)
            {
                t = Message.CompareTo(other.Message);
            }
            return t;
        }

        public bool Equals([AllowNull] VietcomInfo other)
        {
            return Date.Equals(other.Date) && Message.Equals(other.Message);
        }
    }
}
