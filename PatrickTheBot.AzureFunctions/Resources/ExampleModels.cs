namespace PatrickTheBot.AzureFunctions.Resources;

public static class ExampleModels
{
    private static readonly SlackUser ExampleSlackUser = new() {Id = "00000000003", Name = "Wilmar de Hoogd"};
    public static readonly string ExampleSlackUserMention = ExampleSlackUser.ToUserMention();
}