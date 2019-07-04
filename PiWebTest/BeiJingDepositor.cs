using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiWebTest
{
   public sealed class BeiJingDepositor:Depositor
    {
        public BeiJingDepositor(string name, int total) : base(name, total) { }
        public override void Update(int currentBalance, DateTime dateTime)
        {
            Console.WriteLine(Name + ":账户发生变化,变化时间" + dateTime.ToShortDateString() + "，当前余额" + currentBalance.ToString());
        }
    }
}
