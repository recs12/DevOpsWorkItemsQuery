using System;
using System.Threading.Tasks;

namespace DevOpsWorkItemsQuery
{

    public class Program
    {

        static void Main()
        {
            string __version__ = "0.0.0";
            string __author__ = "recs";
            string __update__ = "2020-10-23";
            string __project__ = "DevOpsWorkItemsQuery";


            Console.WriteLine(
                $"{__project__}  --author: {__author__} --version: {__version__} --last-update :{__update__} ");

            Task.Run(async () =>
            {
                string orgName = @"premiertech-ptsa";
                string personalAccessToken = "eyesvfstaw3ltabndvfbpmelzmyaxjm3gzeksfazaxlpl3pkkpuq";
                string wiqlQuery = @$"SELECT *      
                                FROM workitems
                                WHERE (
                                    [System.IterationPath] = @CurrentIteration('[PTSA - LiveOrder Projects]\Team ALBP') - 1 
                                    AND [System.AssignedTo] <> ''
                                )
                                ORDER BY [System.Id]";

                var liveOrder = new QueryExecutor(orgName, personalAccessToken);
                await liveOrder.CsvOpenBugsAsync(wiqlQuery);
            }).GetAwaiter().GetResult();
        }
    }
}
