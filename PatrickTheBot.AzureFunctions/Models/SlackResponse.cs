namespace PatrickTheBot.AzureFunctions.Models;

public class SlackResponse
{
    public string response_type { get; set; }
    public string? text { get; set; }

    public SlackResponse(string? commandText)
    {
        response_type = "in_channel";
        text = commandText;
    }
}