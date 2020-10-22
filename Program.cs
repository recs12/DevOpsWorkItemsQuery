using System;
using System.Threading.Tasks;

namespace DevOpsWorkItemsQuery
{

    public class Program
    {
        static void Main()
        {
            Task.Run(async () =>
            {
                string orgName = "premiertech-ptsa";
                string personalAccessToken = "eyesvfstaw3ltabndvfbpmelzmyaxjm3gzeksfazaxlpl3pkkpuq";
                string wiqlQ = @$"SELECT *      
                                FROM workitems
                                WHERE ([System.IterationPath] = @CurrentIteration('[PTSA - LiveOrder Projects]\Team ALBP') - 1 AND [System.AssignedTo] <> '')
                                ORDER BY [System.Id]";

                var liveOrder = new QueryExecutor(orgName, personalAccessToken);
                await liveOrder.CsvOpenBugsAsync(wiqlQ);

                Console.WriteLine("Query completed, the report is in your downloads folder. ");
                Console.WriteLine("Press any key to exit... ");
            }).GetAwaiter().GetResult();
        }
    }
}
