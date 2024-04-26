namespace PatrickTheBot.AzureFunctions.Endpoints.Points;

public static partial class PointsSystem
{
    [FunctionName("GetBackendLevel")]
    public async static Task<IActionResult> RunGetBackendLevelAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/backend-level")] HttpRequest req,
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
    public async static Task<IActionResult> RunGetFrontendLevelAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/frontend-level")] HttpRequest req,
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

    private async static Task<string?> ConstructLevelString(SlackUser user, TableClient walletTable, ILogger log, Department department)
    {
        await walletTable.CreateIfNotExistsAsync();

        var wallet = await WalletTableUtilities.GetWalletForSlackUser(user, walletTable, log);
        
        if (wallet is null || department is Department.Other)
            return null;

        return wallet.ToPointsLevel(department);
    }
}