using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiWebTest
{
   public sealed class BejingBankMessageSystem:BankMessageSystem
    {
        public override void Add(Depositor depositor)
        {
            obervers.Add(depositor);
        }
        public override void Delete(Depositor depositor)
        {
            obervers.Remove(depositor);
        }
    }
}
