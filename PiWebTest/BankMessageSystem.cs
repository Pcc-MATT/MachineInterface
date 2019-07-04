using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiWebTest
{
    public abstract class BankMessageSystem
    {
        protected IList<Depositor> obervers;
        protected BankMessageSystem()
        {
            obervers = new List<Depositor>();
        }
        public abstract void Add(Depositor depositor);
        public abstract void Delete(Depositor depositor);
        public void Notify()
        {
            foreach(Depositor depositor in obervers)
            {
                if (depositor.AccountIsChanged)
                {
                    depositor.Update(depositor.Balance, depositor.OperationDateTime);
                    depositor.AccountIsChanged = false;
                }
            }
        }
    }
}
