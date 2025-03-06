using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kiosk_snapprint
{

    internal class TransactionData
    {
        public static decimal InsertedAmount { get; private set; } = 0.00m;

        public static void InsertAmount(decimal amount)
        {
            InsertedAmount += amount; // Add money when inserted
        }

        public static decimal GetAndResetInsertedAmount()
        {
            decimal amount = InsertedAmount;
            InsertedAmount = 0.00m; // Reset for the next transaction
            return amount;
        }

    }
}
