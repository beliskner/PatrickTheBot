using System.Threading.Tasks;
using System.Web.Http;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PatrickTheBot.AzureFunctions.Enums;
using PatrickTheBot.AzureFunctions.Models;
using PatrickTheBot.AzureFunctions.Resources;
using PatrickTheBot.AzureFunctions.Utilities;

namespace PatrickTheBot.AzureFunctions.Endpoints.Points;

public static partial class PointsSystem
{
    private static string GetExampleCommand(Department department) => department is Department.Backend ? AwardBackendCommand : AwardFrontendCommand;
    private static string GetPointsName(Department department) => department is Department.Backend ? BackendPointsName : FrontendPointsName;
    private const int MaxAwardAmount = 5;

    [FunctionName("AwardBackendPoints")]
    public static async Task<IActionResult> RunAwardBackendPointsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/award-backend")] HttpRequest req,
        [Table(WalletTableUtilities.TableName, Connection = "AzureWebJobsStorage")] TableClient walletTable,
        ILogger log)
    {
        return await BuildAwardResponse(req, walletTable, log, Department.Backend);
    }
    
    [FunctionName("AwardFrontendPoints")]
    public static async Task<IActionResult> RunAwardFrontendPointsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/award-frontend")] HttpRequest req,
        [Table(WalletTableUtilities.TableName, Connection = "AzureWebJobsStorage")] TableClient walletTable,
        ILogger log)
    {
        return await BuildAwardResponse(req, walletTable, log, Department.Frontend);
    }

    private static async Task<IActionResult> BuildAwardResponse(HttpRequest req, TableClient walletTable, ILogger log,
        Department department)
    {
        await walletTable.CreateIfNotExistsAsync();
        
        var sender = SlackUser.FromForm(req.Form);

        if (sender is null)
            return new InternalServerErrorResult();
        
        if (!SlackUser.IsAuthorizedForCommand(sender, department, out var unauthorizedReturnMessage))
            return new OkObjectResult(new SlackResponse(unauthorizedReturnMessage));
        
        var canFindText = req.Form.TryGetValue("text", out var textFormValue);
        var exampleCommand = GetExampleCommand(department);
        var pointsName = GetPointsName(department);

        if (!canFindText)
            return new OkObjectResult(new SlackResponse($"You didn't fill out a command and now here we are... Try again like this: {exampleCommand} {MaxAwardAmount} to {ExampleModels.ExampleSlackUserMention}"));

        var commandText = textFormValue.ToString().Split(' ');
        if (commandText.Length != 3 || !int.TryParse(commandText[0], out var awardAmount))
            return new OkObjectResult(new SlackResponse($"I mean there's hints on how to use the command for a reason... Try again like this: {exampleCommand} {MaxAwardAmount} to {ExampleModels.ExampleSlackUserMention}"));
        
        if (awardAmount <= 0)
            return new OkObjectResult(new SlackResponse("You've got to at least award SOMETHING, you scrooge"));
        if (awardAmount > MaxAwardAmount)
            return new OkObjectResult(new SlackResponse($"Whilst generous, you're giving out too many points at once, you can only give {MaxAwardAmount} {pointsName} at a time"));

        var recipient = SlackUser.FromString(commandText[2]);
        
        if (recipient is null)
            return new OkObjectResult(new SlackResponse($"You've got to use the mention function to award someone {pointsName}... Try again like this: {exampleCommand} {MaxAwardAmount} to {ExampleModels.ExampleSlackUserMention}"));
        
        if (sender == recipient || string.Equals(sender.Id, recipient.Id))
            return new OkObjectResult(new SlackResponse("Wow! So clever! Award yourself points! Galaxy brain move"));

        var recipientWallet = await WalletTableUtilities.GetWalletForSlackUser(recipient, walletTable, log);

        if (recipientWallet is null)
            return new InternalServerErrorResult();

        switch (department)
        {
            case Department.Backend:
                recipientWallet.BackendPoints += awardAmount;
                break;
            case Department.Frontend:
                recipientWallet.FrontendPoints += awardAmount;
                break;
        }

        var updateRecipientWalletTask = await walletTable.UpdateEntityAsync(recipientWallet, ETag.All);

        if (updateRecipientWalletTask.IsError)
            return new InternalServerErrorResult();

        return new OkObjectResult(new SlackResponse($"Awarded {recipient.ToUserMention()} with {awardAmount} {pointsName}! Job well done"));
    }
}