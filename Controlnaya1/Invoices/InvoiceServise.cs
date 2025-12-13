using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controlnaya1.Invoices
{
    internal class InvoiceServise
    {
        private readonly string _connectionString;
        private readonly int interval;
        private readonly int maxErrorRetry;
        private CancellationTokenSource cts;
        public InvoiceServise(string connStr, int intervalProcessing, int maxErrorRetries)
        {
            _connectionString = connStr;
            interval = intervalProcessing;
            maxErrorRetry = maxErrorRetries;
            StartCheckStatus();
        }
        private bool HasInvoicesToProcess()
        {
            var orm = new ORMContext(_connectionString);
            return orm.ReadByAll<Model>("invoices")
                .Any(i => (i.Status == "pending" || i.Status == "error") && i.RetryCount < maxErrorRetry);
        }
        private async Task StartCheckStatus()
        {
            cts = new CancellationTokenSource();
            while (!cts.Token.IsCancellationRequested)
            {
                if (!HasInvoicesToProcess())
                {
                    Console.WriteLine("Обрабатывать нечего. Остановка.");
                    return;
                }
                CheckStatusInvoice();
                await Task.Delay(interval, cts.Token);
            }
        }
        public void CheckStatusInvoice()
        {
            var orm = new ORMContext(_connectionString);
            List<Model> invoices = orm.ReadByAll<Model>("invoices");
            
            foreach (var invoice in invoices)
            {
                if (invoice.Status == "pending" || invoice.Status == "error" && invoice.RetryCount < maxErrorRetry)
                {
                    
                    int rand = new Random().Next(100);
                    if (rand < 70)
                    {
                        Console.WriteLine("Invoice status: error");
                        invoice.Status = "error";
                        invoice.LastAttemptAt = DateTime.Now;
                        invoice.RetryCount++;
                    }
                    else
                    {
                        Console.WriteLine("Invoice status: success");
                        invoice.Status = "success";
                    }
                    invoice.UpdatedAt = DateTime.Now;
                    orm.Update(invoice.Id, invoice, "invoices");
                }
            }
        }
    }
}
