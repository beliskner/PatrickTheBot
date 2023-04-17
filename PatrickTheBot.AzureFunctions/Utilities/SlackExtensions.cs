using PatrickTheBot.AzureFunctions.Endpoints.Points;
using PatrickTheBot.AzureFunctions.Models;

namespace PatrickTheBot.AzureFunctions.Utilities;

public static class SlackExtensions
{
    public static string ToUserMention(this SlackUser slackUser) => $"<@{slackUser.Id}|{slackUser.Name}>";

    public static string ToAdminBackendLevel(this SlackUser slackUser) => slackUser.Id switch
    {
        "00000000001" or "00000000002" => "a Backend Intern",
        "00000000003" or "00000000004" or "00000000005" => "a Junior Backend Developer",
        "00000000006" => "the Lead Backend Developer",
        // This default can't possibly go wrong, can it? 🤡
        _ => "a Backend Developer"
    };

    public static string ToAdminFrontendLevel(this SlackUser slackUser) => slackUser.Id switch
    {
        "00000000007" or "00000000008" => "a Junior Frontend Developer",
        "00000000009" => "a Senior Frontend Developer",
        "00000000010" => "the Lead Frontend Developer",
        // This default can't possibly go wrong, can it? 🤡
        _ => "a Frontend Developer"
    };

    public static string ToPointsBackendLevel(this WalletTableEntity walletTableEntity) => walletTableEntity.BackendPoints switch
    {
        > 0 and <= 25 => "a human being with a set of hands and a heartbeat is our best guess",
        > 25 and <= 50 => "a Backend lackey, get back to work slacker",
        > 50 and <= 75 => "a baby Backend developer, aren't you cute!",
        > 75 and <= 100 => "an aspiring honorary Backend developer, keep trying your best little code monkey",
        > 100 => "an honorary Backend developer, we're proud of you colleague!",
        _ => $"a master Backend developer! Psyche! You haven't scored any {PointsSystem.BackendPointsName} so you tell me what that makes you"
    };

    public static string ToPointsFrontendLevel(this WalletTableEntity walletTableEntity) => walletTableEntity.FrontendPoints switch
    {
        // TODO: Change these levels or just flatten the function together with the BackendLevel function
        > 0 and <= 25 => "a human being with a set of hands and a heartbeat is our best guess",
        > 25 and <= 50 => "a Frontend lackey, get back to work slacker",
        > 50 and <= 75 => "a baby Frontend developer, aren't you cute!",
        > 75 and <= 100 => "an aspiring honorary Frontend developer, keep trying your best little code monkey",
        > 100 => "an honorary Frontend developer, we're proud of you colleague!",
        _ => $"a master Frontend developer! Psyche! You haven't scored any {PointsSystem.FrontendPointsName} so you tell me what that makes you"
    };
}