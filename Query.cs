// nuget:Microsoft.TeamFoundationServer.Client
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryExecutor" /> class.
    /// </summary>
    /// <param name="orgName">
    ///     An organization in Azure DevOps Services. If you don't have one, you can create one for free:
    ///     <see href="https://go.microsoft.com/fwlink/?LinkId=307137" />.
    /// </param>
    /// <param name="personalAccessToken">
    ///     A Personal Access Token, find out how to create one:
    ///     <see href="https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops" />.
    /// </param>
    public QueryExecutor(string orgName, string personalAccessToken)
    {
        this.uri = new Uri("https://dev.azure.com/" + orgName);
        this.personalAccessToken = personalAccessToken;
    }

    /// <summary>
    ///     Execute a WIQL (Work Item Query Language) query to return a list of open bugs.
    /// </summary>
    /// <param name="project">The name of your project within your organization.</param>
    /// <returns>A list of <see cref="WorkItem"/> objects representing all the open bugs.</returns>
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
            var fields = new[] { "System.State", "System.Id", "System.Title", "System.AssignedTo" };

            // get work items for the ids found in query
            return await httpClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Execute a WIQL (Work Item Query Language) query to print a list of open bugs.
    /// </summary>
    /// <param name="project">The name of your project within your organization.</param>
    /// <returns>An async task.</returns>
    public async Task PrintOpenBugsAsync(string project)
    {
        var workItems = await this.QueryOpenBugs(project).ConfigureAwait(false);

        Console.WriteLine("Query Results: {0} items found", workItems.Count);
        Console.WriteLine("State, ID, Title, Assigned To, Tags, Original Estimate, Completed Work, Remaining Work");
        // loop though work items and write to console
        foreach (var workItem in workItems)
        {
            Console.WriteLine(
                "{0},{1},{2},{3}",
                workItem.Fields["System.State"],
                workItem.Fields["System.Id"],
                workItem.Fields["System.Title"],
                ((IdentityRef)workItem.Fields["System.AssignedTo"]).DisplayName
            );
        }
    }
    //public async Task CsvOpenBugsAsync(string project)
    //{
        
    //    StringBuilder csvContent = new StringBuilder();
    //    var workItems = await this.QueryOpenBugs(project).ConfigureAwait(false);

    //    Console.WriteLine("State, ID, Title, Assigned To, Tags, Original Estimate, Completed Work, Remaining Work");
    //    // loop though work items and write to console
    //    foreach (var workItem in workItems)
    //    {
    //        csvContent.AppendLine(
    //            "{0},{1},{2},{3}",
    //            workItem.Fields["System.State"],
    //            workItem.Fields["System.Id"],
    //            workItem.Fields["System.Title"],
    //            ((IdentityRef)workItem.Fields["System.AssignedTo"]).DisplayName
    //        );
    //    }


    //    string superviserPath = @"C:\Users\recs\Downloads\csv"; 
    //    File.AppendAllText(superviserPath, csvContent.ToString());
    //}

}