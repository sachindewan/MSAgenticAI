using System.ComponentModel;
using AgentApi.Data;
using AgentApi.Models;

namespace AgentApi.Agent.Tools;

public class OrderTool(InMemoryStore store)
{
    [Description("Get an order by its ID")]
    public Order? GetOrder([Description("The order ID")] string orderId)
        => store.Orders.FirstOrDefault(o => o.Id == orderId);

    [Description("Get all orders for a specific user")]
    public List<Order> GetOrdersByUser([Description("The user ID")] string userId)
        => store.Orders.Where(o => o.UserId == userId).ToList();

    [Description("Create a new order")]
    public Order CreateOrder(
        [Description("The user ID placing the order")] string userId,
        [Description("List of item names")] List<string> items,
        [Description("Total price of the order")] decimal total)
    {
        var order = new Order { UserId = userId, Items = items, Total = total };
        store.Orders.Add(order);
        return order;
    }

    [Description("Update the status of an order")]
    public object UpdateOrderStatus(
        [Description("The order ID")] string orderId,
        [Description("New status: Pending, Processing, Shipped, Delivered, Cancelled")] string status)
    {
        var order = store.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order is null) return new { error = "Order not found" };
        order.Status = status;
        return order;
    }

    [Description("List all orders")]
    public List<Order> ListOrders()
        => store.Orders;
}
