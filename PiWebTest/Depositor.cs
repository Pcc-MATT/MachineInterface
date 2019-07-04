using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiWebTest
{
   public abstract class Depositor
    {
        private string _name;
        private int _balance;
        private int _total;
        private bool _isChanged;

        protected Depositor (string name,int total)
        {
            this._name = name;
            this._balance = total;
            this._isChanged = false;
        }

        public string Name
        {
            get { return _name; }
            set { this._name = value; }

        }
        public int Balance
        {
            get { return this._balance; }
        }
        public void GetMoney(int num)
        {
            if (num <= this._balance && num > 0)
            {
                this._balance = this._balance - num;
                this._isChanged = true;
                OperationDateTime = DateTime.Now;
            }
        }
        public DateTime OperationDateTime { get; set; }
        public bool AccountIsChanged
        {
            get { return this._isChanged; }
            set { this._isChanged = value; }
        }
        public abstract void Update(int currentBalance, DateTime dateTime);
    }
}
