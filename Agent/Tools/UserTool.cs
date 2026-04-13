using System.ComponentModel;
using AgentApi.Data;
using AgentApi.Models;

namespace AgentApi.Agent.Tools;

public class UserTool(InMemoryStore store)
{
    [Description("Get a user by their ID")]
    public User? GetUser([Description("The user ID")] string userId)
        => store.Users.FirstOrDefault(u => u.Id == userId);

    [Description("List all users")]
    public List<User> ListUsers()
        => store.Users;

    [Description("Create a new user")]
    public User CreateUser(
        [Description("Full name")] string name,
        [Description("Email address")] string email)
    {
        var user = new User { Name = name, Email = email };
        store.Users.Add(user);
        return user;
    }

    [Description("Delete a user by ID")]
    public object DeleteUser([Description("The user ID to delete")] string userId)
    {
        var user = store.Users.FirstOrDefault(u => u.Id == userId);
        if (user is null) return new { error = "User not found" };
        store.Users.Remove(user);
        return new { success = true, message = $"User {userId} deleted" };
    }
}
