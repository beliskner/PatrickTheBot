namespace PatrickTheBot.AzureFunctions.Endpoints.Bucks;

public static class TransferFunds
{
    private const string Route = "wallet";
    private const string CurrencyName = "Backend Bucks";

    [FunctionName("TransferFunds")]
    public async static Task<IActionResult> RunTransferFundsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/transfer")] HttpRequest req,
        [Table(WalletTableUtilities.TableName, Connection = "AzureWebJobsStorage")] TableClient walletTable, ILogger log)
    {
        await walletTable.CreateIfNotExistsAsync();
        var canFindText = req.Form.TryGetValue("text", out var textFormValue);

        if (!canFindText)
            return new OkObjectResult(new SlackResponse("You didn't fill out a command and now here we are..."));

        var commandText = textFormValue.ToString().Split(' ');
        if (commandText.Length != 3 || !int.TryParse(commandText[0], out var transferAmount))
            return new OkObjectResult(new SlackResponse("I mean there's hints on how to use the command for a reason..."));
        
        if (transferAmount <= 0)
            return new OkObjectResult(new SlackResponse($"Sure buddy, I totally just transferred {transferAmount} {CurrencyName}"));

        var sender = SlackUser.FromForm(req.Form);
        var recipient = SlackUser.FromString(commandText[2]);
        
        if (sender is null || recipient is null)
            return new OkObjectResult(new SlackResponse("No clue what you did, but you did bad"));
        
        if (sender == recipient || string.Equals(sender.Id, recipient.Id))
            return new OkObjectResult(new SlackResponse("Wow! So clever! Send yourself money! Galaxy brain move"));

        var senderWalletTask = WalletTableUtilities.GetWalletForSlackUser(sender, walletTable, log);
        var recipientWalletTask = WalletTableUtilities.GetWalletForSlackUser(recipient, walletTable, log);

        await Task.WhenAll(senderWalletTask, recipientWalletTask);

        var senderWallet = senderWalletTask.Result;
        var recipientWallet = recipientWalletTask.Result;
        
        if (senderWallet is null || recipientWallet is null)
            return new OkObjectResult(new SlackResponse("Come on buddy, to be able to transfer money both of you gotta have wallets. Get your act together."));
        
        if (senderWallet.BackendBucks - transferAmount < 0 && !UserIds.IsSuperAdmin(sender.Id))
            return new OkObjectResult(new SlackResponse($"Seriously? Transferring {transferAmount} {CurrencyName} would put you in debt. Did you really think we do loans here?"));

        var transferSuccessful = await WalletTableUtilities.TransferBackendBucks(senderWallet, recipientWallet, transferAmount, walletTable, log);

        if (!transferSuccessful)
            return new InternalServerErrorResult();

        log.LogInformation("Transferred {Amount} from {SenderId} to {RecipientId}", transferAmount.ToString(), sender.Id, recipient.Id);
        log.LogInformation("Sender {RecipientId} balance changed from {OldBalance} to {NewBalance}", sender.Id, senderWallet.BackendBucks.ToString(), (senderWallet.BackendBucks + transferAmount).ToString());
        log.LogInformation("Recipient {RecipientId} balance changed from {OldBalance} to {NewBalance}", recipient.Id, recipientWallet.BackendBucks.ToString(), (recipientWallet.BackendBucks + transferAmount).ToString());
        return new OkObjectResult(new SlackResponse($"Got you covered bud, transferred {transferAmount} {CurrencyName} to {recipient.Name}'s wallet."));
    }

    [FunctionName("CheckBalance")]
    public async static Task<IActionResult> RunCheckBalanceAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route + "/balance")] HttpRequest req,
        [Table(WalletTableUtilities.TableName, Connection = "AzureWebJobsStorage")] TableClient walletTable, ILogger log)
    {
        await walletTable.CreateIfNotExistsAsync();
        
        var user = SlackUser.FromForm(req.Form);
        
        if (user is null)
            return new InternalServerErrorResult();

        var userWallet = await WalletTableUtilities.GetWalletForSlackUser(user, walletTable, log);

        if (userWallet.BackendBucks is 0)
            return new OkObjectResult(new SlackResponse("Your broke ass is in possession of a grand total of 💲0 buckeroonies"));
        
        return new OkObjectResult(new SlackResponse($"Your current balance is 💲{userWallet.BackendBucks} {CurrencyName} {userWallet.PrintAlternativeCurrencies()}"));
    }


}