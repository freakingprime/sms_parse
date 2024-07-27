using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsParser2.DbModels
{
    public class DbTransaction
    {
        public DbTransaction()
        {
        }

        public int ID;
        public string BankName;
        public DateTime Date;
        public long Delta;
        public long Balance;
        public string TimeString;
        public int SmsID;
        public string Ref;
    }
}
