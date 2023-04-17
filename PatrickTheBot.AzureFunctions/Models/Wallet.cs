namespace PatrickTheBot.AzureFunctions.Models;

public class Wallet
{
    public string HolderId { get; init; }
    public string HolderName { get; init; }
    public int BackendBucks { get; init; }
    public int BackendPoints { get; init; }
    public int FrontendPoints { get; init; }

    public Wallet(SlackUser slackUser)
    {
        HolderId = slackUser.Id;
        HolderName = slackUser.Name;
        BackendBucks = 0;
        BackendPoints = 0;
        FrontendPoints = 0;
    }
    
    public Wallet(){}
}