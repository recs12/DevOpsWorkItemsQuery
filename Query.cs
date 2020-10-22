using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text;

public class QueryExecutor
{
    private readonly Uri uri;
    private readonly string personalAccessToken;
    private readonly string csvHeader = "State,ID,Title,Assigned To,Tags,Original Estimate,Completed Work,Remaining Work";

    public QueryExecutor(string orgName, string personalAccessToken)
    {
        this.uri = new Uri("https://dev.azure.com/" + orgName);
        this.personalAccessToken = personalAccessToken;
    }

    public async Task<IList<WorkItem>> QueryOpenBugs(string wiqlQ)
    {
        var credentials = new VssBasicCredential(string.Empty, this.personalAccessToken);

        // create a wiql object and build our query
        var wiql = new Wiql()
        {
            // NOTE: Even if other columns are specified, only the ID & URL will be available in the WorkItemReference
            Query = wiqlQ,
        };

        // create instance of work item tracking http client
        using (var httpClient = new WorkItemTrackingHttpClient(this.uri, credentials))
        {
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
    }

    public async Task CsvOpenBugsAsync(string project)
    {

        StringBuilder csvContent = new StringBuilder();
        var workItems = await this.QueryOpenBugs(project).ConfigureAwait(false);

        // loop though work items and write to console
        csvContent.AppendLine(csvHeader);

        foreach (var workItem in workItems)
        {
            try
            {
                csvContent.AppendFormat(
                    "{0},{1},{2},{3} <{4}>,{5},{6},{7},{8}\n",
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

            }
        }

        // date of today
        DateTime localDate = DateTime.Now;
        string formatLocalDate = localDate.ToString("yyyy-MM-dd #hhmmss");

        //user id
        string userID = System.Environment.UserName.ToLower();

        string superviserPath = @$"C:\Users\{userID}\Downloads\AzureQuery @{userID} {formatLocalDate}.csv";
        File.AppendAllText(superviserPath, csvContent.ToString(), Encoding.UTF8);
    }

}