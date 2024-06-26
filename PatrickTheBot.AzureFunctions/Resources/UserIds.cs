﻿namespace PatrickTheBot.AzureFunctions.Resources;

public static class UserIds
{
    // Actual User IDs omitted for obvious reasons
    public static readonly string[] BackendInternIds =
    {
        "00000000001",
        "00000000002"
    };
    
    public static readonly string[] BackendAdminIds =
    {
        "00000000003",
        "00000000004",
        "00000000005",
        "00000000006"
    };
    
    public static readonly string[] FrontendAdminIds =
    {
        "00000000007",
        "00000000008",
        "00000000009",
        "00000000010",
    };

    public static bool IsSuperAdmin(string? userId) => EnvironmentVariablesExtensions.SuperAdminEnvironmentVariableId is { Length: > 0 } superAdminId && string.Equals(userId, superAdminId);
}