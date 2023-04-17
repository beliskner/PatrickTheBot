using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using PatrickTheBot.AzureFunctions.Models;

namespace PatrickTheBot.AzureFunctions.Utilities;

public static class WalletTableUtilities
{
    public const string TableName = "wallets";
    private const string PartitionKey = "WALLET";
    
        public static async Task<bool> TransferBackendBucks(WalletTableEntity senderWallet, WalletTableEntity recipientWallet,
        int amount, TableClient walletTable, ILogger log)
    {
        var transferSucceeded = true;
        senderWallet.BackendBucks -= amount;
        recipientWallet.BackendBucks += amount;

        var updateSenderTask = walletTable.UpdateEntityAsync(senderWallet, ETag.All);
        var updateRecipientTask = walletTable.UpdateEntityAsync(recipientWallet, ETag.All);

        await Task.WhenAll(updateSenderTask, updateRecipientTask);

        if (updateRecipientTask.Result.IsError)
        {
            log.LogInformation("Error when updating {RecipientName}'s wallet with id {WalletId} with reason: {Reason}", recipientWallet.WalletHolder, recipientWallet.RowKey, updateRecipientTask.Result.ReasonPhrase);
            transferSucceeded = false;
        }

        if (updateSenderTask.Result.IsError)
        {
            log.LogInformation("Error when updating {RecipientName}'s wallet with id {WalletId} with reason: {Reason}", senderWallet.WalletHolder, senderWallet.RowKey, updateSenderTask.Result.ReasonPhrase);
            transferSucceeded = false;
        }

        return transferSucceeded;
    }

    public static async Task<WalletTableEntity?> GetWalletForSlackUser(SlackUser slackUser, TableClient walletTable, ILogger log)
    {
        log.LogInformation("Getting wallet by id {Id}", slackUser.Id);
        WalletTableEntity wallet;
        try
        {
            var walletTask = await walletTable.GetEntityAsync<WalletTableEntity>(rowKey: slackUser.Id, partitionKey: PartitionKey);
            wallet = walletTask.Value;
        }
        catch
        {
            log.LogInformation("Wallet {Id} not found, attempting to create", slackUser.Id);
            wallet = await CreateWalletForSlackUser(slackUser, walletTable, log);
        }

        return wallet;
    }

    private static async Task<WalletTableEntity> CreateWalletForSlackUser(SlackUser slackUser, TableClient walletTable, ILogger log)
    {
        var wallet = new Wallet(slackUser).ToTable();
        await walletTable.AddEntityAsync(wallet);
        log.LogInformation("Wallet with {Id} created", slackUser.Id);
        return wallet;
    }
}