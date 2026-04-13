using AgentApi.Models;

namespace AgentApi.Data;

public class InMemoryStore
{
    public List<User> Users { get; } =
    [
        new User { Id = "u1", Name = "Alice Johnson", Email = "alice@example.com" },
        new User { Id = "u2", Name = "Bob Smith", Email = "bob@example.com" },
        new User { Id = "u3", Name = "Carol White", Email = "carol@example.com" }
    ];

    public List<Order> Orders { get; } =
    [
        new Order { Id = "o1", UserId = "u1", Items = ["Laptop", "Mouse"], Total = 1200.00m, Status = "Delivered" },
        new Order { Id = "o2", UserId = "u2", Items = ["Keyboard"], Total = 80.00m, Status = "Pending" }
    ];
}
