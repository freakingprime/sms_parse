﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser
{
    public class ShinhanInfo : BankInfoBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public ShinhanInfo(SmsInfo sms)
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
            Match changeMatch = regexChange1.Match(lower);
            if (changeMatch.Success)
            {
                //giao dich thanh cong
                GroupCollection groups = changeMatch.Groups;
                bool okay = long.TryParse(groups["amount"].Value.Replace(",", ""), out Delta);
                Delta = -Delta;
                TimeString = groups["date"].Value + " " + groups["time"].Value;
                Balance = 0;
                Ref = groups["ref"].Value.Trim();
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
                    TimeString = groups["date"].Value + " " + groups["time"].Value;
                    Balance = 0;
                    Ref = groups["ref"].Value.Trim();
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
                        //giao dich tai khoan
                        GroupCollection groups = changeMatch.Groups;
                        bool okay = long.TryParse(groups["amount"].Value.Replace(",", ""), out Delta);
                        if (okay)
                        {
                            okay = long.TryParse(groups["sodu"].Value.Replace(",", ""), out Balance);
                        }
                        if (groups["sign"].Value.Equals("-"))
                        {
                            Delta = -Delta;
                        }
                        TimeString = "None";
                        Ref = groups["ref"].Value.Trim();
                        if (okay)
                        {
                            ParseStatus = StatusBankInfo.Okay;
                        }
                    }
                }
            }
        }

        private readonly Regex regexChange1 = new Regex(@"giao dich duoc chap nhan.+?(?<date>\d\d-\d\d-\d\d\d\d)\/(?<time>\d\d:\d\d)\/(?<amount>[\d,]+)\/(?<ref>.+),han muc.+?(?<hanmuc>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange2 = new Regex(@"giao dich bi huy.+?(?<date>\d\d-\d\d-\d\d\d\d)\/(?<time>\d\d:\d\d)\/(?<amount>[\d,]+)\/(?<ref>.+),han muc.+?(?<hanmuc>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange3 = new Regex(@"tk.+thay doi\s+(?<sign>[+-])\s+VND\s+(?<amount>[\d,]+).+?so du kha dung.+?(?<sodu>[\d,]+)[;.\s]+(?<ref>.+)", RegexOptions.IgnoreCase);

        private readonly string[] ignoredKeywords = { "otp", "du no the cuoi ky", "thanh toan the cua", "napas", "bit.ly", "ma xac thuc", "1900 1577", "19001577", "Giao dich bi tu choi", " So bi mat dang nhap Ngan hang", "Ap dung tai CGV CINEMAS", "Yeu cau kich hoat Samsung", "So dien thoai cua quy khach dang ki", "Smail Service Authentication", "To register Shinhan Bank Vietnam" };

        public const string SENDER_NAME = "shinhanbank";
    }
}
