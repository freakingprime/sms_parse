using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsParser2.DbModels
{
    public class DbSms
    {
        public DbSms()
        {
        }

        public int ID;
        public string Address;
        public string Body;
        public DateTime Date;
        public string ContactName;
        public int Type;
    }
}
