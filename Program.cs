using System;

namespace DevOpsWorkItemsQuery
{
    class Program
    {
        static void Main(string[] args)
        {
            var liveOrder = new QueryExecutor();
            liveOrder.PrintOpenBugsAsync("development_reports");
            Console.ReadKey();
        }
    }
}
