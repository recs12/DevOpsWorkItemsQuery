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
                //string orgName = "recs0164";
                //string personalAccessToken = "kmlp7mnvuqt7nzar63tna565qy6e5kzczcsi3igei5c3oky3miyq";
                //string projectIterationPath = @"development_reports\Sprint 1";
                //string wiqlQ = "SELECT [Id] FROM WorkItems WHERE [System.IterationPath] = '" + projectIterationPath + "' ";

                string orgName = "premiertech-ptsa";
                string personalAccessToken = "eyesvfstaw3ltabndvfbpmelzmyaxjm3gzeksfazaxlpl3pkkpuq";
                string wiqlQ = @$"SELECT 
                                        [System.Id], 
                                        [System.WorkItemType], 
                                        [System.Title], 
                                        [System.AssignedTo], 
                                        [System.State], 
                                        [System.Tags]
                                FROM workitems
                                WHERE ([System.WorkItemType] <> [Any] 
                                AND [System.IterationPath] = @CurrentIteration-1) 
                                ORDER BY System.ID asc";

                //'[PTSA - LiveOrder Projects]\Team ALBP'

                var liveOrder = new QueryExecutor(orgName, personalAccessToken);
                await liveOrder.PrintOpenBugsAsync(wiqlQ);
                Console.ReadKey();
            }).GetAwaiter().GetResult();
        }
    }
}
