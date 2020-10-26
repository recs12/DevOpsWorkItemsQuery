using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace DevOpsWorkItemsQuery
{
    public class QueryExecutor
    {
        private readonly Uri _uri;
        private readonly string _personalAccessToken;
        private const string CsvHeader = "State,ID,Title,Assigned To,Tags,Original Estimate,Completed Work,Remaining Work";

        public QueryExecutor(string orgName, string personalAccessToken)
        {
            this._uri = new Uri("https://dev.azure.com/" + orgName);
            this._personalAccessToken = personalAccessToken;
        }

        public async Task<IList<WorkItem>> QueryOpenBugs(string wiqlQ)
        {
            var credentials = new VssBasicCredential(string.Empty, this._personalAccessToken);

            // create a wiql object and build our query
            var wiql = new Wiql()
            {
                // NOTE: Even if other columns are specified, only the ID & URL will be available in the WorkItemReference
                Query = wiqlQ,
            };

            // create instance of work item tracking http client
            using var httpClient = new WorkItemTrackingHttpClient(this._uri, credentials);
            // execute the query to get the list of work items in the results
            var result = await httpClient.QueryByWiqlAsync(wiql).ConfigureAwait(false);
            var ids = result.WorkItems.Select(item => item.Id).ToArray();

            // some error handling
            if (ids.Length == 0)
            {
                return Array.Empty<WorkItem>();
            }

            // build a list of the fields we want to see
            var fields = new[] {
                "System.State",
                "System.Id",
                "System.Title",
                "System.AssignedTo",
                "System.Tags",
                "Microsoft.VSTS.Scheduling.OriginalEstimate",
                "Microsoft.VSTS.Scheduling.CompletedWork",
                "Microsoft.VSTS.Scheduling.RemainingWork"
            };

            // get work items for the ids found in query
            return await httpClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
        }

        public async Task CsvOpenBugsAsync(string project)
        {

            var csvContent = new StringBuilder();
            var workItems = await this.QueryOpenBugs(project).ConfigureAwait(false);

            // Add header in the csv file.
            csvContent.AppendLine(CsvHeader);

            // loop though work items and write to console
            foreach (var workItem in workItems)
            {
                try
                {
                    csvContent.AppendFormat(
                        "{0},{1},\"{2}\",{3} <{4}>,{5},{6},{7},{8}\n",
                        workItem.Fields["System.State"], //{0}
                        workItem.Fields["System.Id"], //{1}
                        workItem.Fields["System.Title"], //{2}
                        ((IdentityRef)workItem.Fields["System.AssignedTo"]).DisplayName, //{3}
                        ((IdentityRef)workItem.Fields["System.AssignedTo"]).UniqueName, //{4}
                        workItem.Fields.GetValueOrDefault("System.Tags"), //{5}
                        workItem.Fields.GetValueOrDefault("Microsoft.VSTS.Scheduling.OriginalEstimate"), //{6}
                        workItem.Fields.GetValueOrDefault("Microsoft.VSTS.Scheduling.CompletedWork"), //{7}
                        workItem.Fields.GetValueOrDefault("Microsoft.VSTS.Scheduling.RemainingWork") //{8}
                    );

                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // date of today
            var localDate = DateTime.Now;
            var formatLocalDate = localDate.ToString("yyyy-MM-dd #hhmmss");

            //user id
            var userId = System.Environment.UserName.ToLower();

            var superviserPath = @$"C:\Users\{userId}\Downloads\AzureQuery @{userId} {formatLocalDate}.csv";
            await File.AppendAllTextAsync(superviserPath, csvContent.ToString(), Encoding.UTF8);
            Console.WriteLine($"CSV downloaded in the following folder: {superviserPath}");
        }
    }
}