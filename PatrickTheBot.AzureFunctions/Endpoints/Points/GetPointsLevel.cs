using System.Threading.Tasks;
using System.Web.Http;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PatrickTheBot.AzureFunctions.Enums;
using PatrickTheBot.AzureFunctions.Models;
using PatrickTheBot.AzureFunctions.Utilities;

namespace PatrickTheBot.AzureFunctions.Endpoints.Points;

public static partial class PointsSystem
{
    [FunctionName("GetBackendLevel")]
    public static async Task<IActionResult> RunGetBackendLevelAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/backend-level")] HttpRequest req,
        [Table(WalletTableUtilities.TableName, Connection = "AzureWebJobsStorage")] TableClient walletTable,
        ILogger log)
    {
        var user = SlackUser.FromForm(req.Form);

        if (user is null)
            return new InternalServerErrorResult();
        
        if (user.Department is Department.Backend)
            return new OkObjectResult(new SlackResponse($"You're currently {user.ToAdminBackendLevel()}, not sure why you needed me to tell you that..."));
        
        var levelString = await ConstructLevelString(user, walletTable, log, Department.Backend);

        return string.IsNullOrWhiteSpace(levelString) ? new InternalServerErrorResult() : new OkObjectResult(new SlackResponse(levelString));
    }
    
    [FunctionName("GetFrontendLevel")]
    public static async Task<IActionResult> RunGetFrontendLevelAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/frontend-level")] HttpRequest req,
        [Table(WalletTableUtilities.TableName, Connection = "AzureWebJobsStorage")] TableClient walletTable,
        ILogger log)
    {
        var user = SlackUser.FromForm(req.Form);

        if (user is null)
            return new InternalServerErrorResult();
        
        if (user.Department is Department.Frontend)
            return new OkObjectResult(new SlackResponse($"You're currently {user.ToAdminFrontendLevel()}, not sure why you needed me to tell you that..."));
        
        var levelString = await ConstructLevelString(user, walletTable, log, Department.Frontend);

        return string.IsNullOrWhiteSpace(levelString) ? new InternalServerErrorResult() : new OkObjectResult(new SlackResponse(levelString));
    }

    private static async Task<string?> ConstructLevelString(SlackUser user, TableClient walletTable, ILogger log, Department department)
    {
        await walletTable.CreateIfNotExistsAsync();

        var wallet = await WalletTableUtilities.GetWalletForSlackUser(user, walletTable, log);
        
        if (wallet is null)
            return null;

        return department switch
        {
            Department.Backend => $"You're currently {wallet.ToPointsBackendLevel()}",
            Department.Frontend => $"You're currently {wallet.ToPointsFrontendLevel()}",
            _ => null
        };
    }
}