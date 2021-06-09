﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser.Model
{
    public class VietcomInfo : BankInfoBase
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

        private Regex regexChange = new Regex(@"(so du tk vcb|sd tk)\s*\d+.*?([+-][\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private Regex regexTime = new Regex(@"luc ([\d\s-:]+)", RegexOptions.IgnoreCase);
        private Regex regexTotal = new Regex(@"\.\s*(sd|so du)\s+([\d,]+)\s*vnd", RegexOptions.IgnoreCase);
        private Regex regexRefer = new Regex(@"\.\s*ref\s*(.+)", RegexOptions.IgnoreCase);

        private string[] ignoredKeywords = { "quy khach", "thu phi", "ma otp", "the vcb visa", "huy giao dich tren", "smartotp", "1900545413" };

        public const string SENDER_NAME = "vietcombank";
    }
}