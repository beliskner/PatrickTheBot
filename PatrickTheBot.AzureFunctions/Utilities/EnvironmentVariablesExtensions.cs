namespace PatrickTheBot.AzureFunctions.Utilities;

public static class EnvironmentVariablesExtensions
{
    private static string? GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    public static string? SuperAdminEnvironmentVariableId => GetEnvironmentVariable("SuperAdmin");
}