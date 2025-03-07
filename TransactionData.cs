using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kiosk_snapprint
{

    internal class TransactionData
    {
        public static decimal InsertedAmount { get; private set; } = 0.00m; // From coins/bills
        public static decimal EmailPaymentAmount { get; private set; } = 0.00m; // From email payment

        public static decimal TotalAmount => InsertedAmount + EmailPaymentAmount; // Combine both

        public static void InsertAmount(decimal amount)
        {
            InsertedAmount = amount; // Add money when inserted.
            Debug.WriteLine($"[TransactionData] InsertedAmount updated: {InsertedAmount}"); // ✅ Log amount
        }

        // Called when an email payment is received
        public static void AddEmailPayment(decimal amount)
        {
            EmailPaymentAmount += amount; // Add new email payment
            Debug.WriteLine($"[TransactionData] EmailPaymentAmount updated: {EmailPaymentAmount}");
        }

        public static void Reset()
        {
            InsertedAmount = 0.00m;
            EmailPaymentAmount = 0.00m;
            Debug.WriteLine($"[TransactionData] Reset: InsertedAmount={InsertedAmount}, EmailPaymentAmount={EmailPaymentAmount}");
        }

    }
}
