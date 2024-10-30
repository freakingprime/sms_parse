using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsParser2.UI_Parser
{
    public class HsbcInfo : BankInfoBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        public HsbcInfo(SmsInfo sms)
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
                Balance = 0;
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
                    Balance = 0;
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
                        Balance = 0;
                        if (okay)
                        {
                            ParseStatus = StatusBankInfo.Okay;
                        }
                    }
                }
            }
        }
        private readonly Regex regexChange1 = new Regex(@"the.+?6291.+?thuc hien.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange2 = new Regex(@"the.+?6291.+?bi huy.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);
        private readonly Regex regexChange3 = new Regex(@"the.+?6291.+?VND(?<amount>[\d,]+)", RegexOptions.IgnoreCase);

        private readonly string[] ignoredKeywords = { "otp", "du no cuoi ky", "card.apply.hsbc", "QK duoc lua chon de", "Chuyen doi du no The Tin Dung sang tra gop", "Quy khach da duoc tang len", "The x6291 den han ngay", "The X6291 sap den han", "can duoc thanh toan cham nhat", "Hoan tien", "Vui long thanh toan", "one-time activation code", "Chi tieu bang the HSBC Visa", "Do anh huong cua lich nghi Tet", "Kinh moi QK tham du Zoom webinar", "Nhan den 5 trieu VND khi", "The HSBC 7616 cua QK da duoc kich hoat", "Thong Bao Lich Bao Tri He Thong", "Tin nhan yeu cau khong hop le", "HSBC PGD Giang Vo se chuyen den", "null" };

        public const string SENDER_NAME = "hsbc";
    }
}
