namespace PatrickTheBot.AzureFunctions.Models;

public record SlackUser
{
    public SlackUser() { }

    private SlackUser(string? id, string? name, Department department)
    {
        Id = id;
        Name = name;
        Department = department;
    }

    public string? Id { get; init; }
    public string? Name { get; init; }
    public Department Department { get; }

    public static SlackUser? FromForm(IFormCollection formCollection)
    {
        var canFindUserId = formCollection.TryGetValue("user_id", out var userId);
        var canFindUserName = formCollection.TryGetValue("user_name", out var userName);

        if (!canFindUserId || !canFindUserName) return null;
        var userIdString = userId.ToString();

        return SlackUserOrNull(userIdString, userName.ToString(), GetDepartment(userIdString));
    }

    public static SlackUser? FromString(string commandString)
    {
        var splitUser = commandString.Split('|');
        var userIdString = splitUser[0].Trim('<', '@');
        return splitUser.Length != 2 ? null : SlackUserOrNull(userIdString, splitUser[1].Trim('>'), GetDepartment(userIdString));
    }

    public static bool IsAuthorizedForCommand(SlackUser slackUser, Department department, out string unauthorizedReturnMessage)
    {
        unauthorizedReturnMessage = "If you're seeing this message you probably broke something and I'm gonna have to report you to the backend authorities";
        if (UserIds.IsSuperAdmin(slackUser.Id))
            return true;
        
        switch (department)
        {
            case Department.Backend:
                unauthorizedReturnMessage = $"The point of awarding {PointsSystem.BackendPointsName} is that a backend developer awards them I mean come on";
                return slackUser.Department is Department.Backend;
            case Department.Frontend:
                unauthorizedReturnMessage = $"The point of awarding {PointsSystem.FrontendPointsName} is that a frontend developer awards them I mean come on";
                return slackUser.Department is Department.Frontend;
            default:
                return false;
        }
    }

    private static SlackUser? SlackUserOrNull(string userId, string userName, Department department)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userName)) return null;
        
        return new SlackUser(id: userId, name: userName, department: department);
    }

    private static Department GetDepartment(string userId)
    {
        if (UserIds.IsSuperAdmin(userId) || UserIds.BackendAdminIds.Contains(userId) || UserIds.BackendInternIds.Contains(userId))
            return Department.Backend;
        if (UserIds.FrontendAdminIds.Contains(userId))
            return Department.Frontend;
        return Department.Other;
    }
}