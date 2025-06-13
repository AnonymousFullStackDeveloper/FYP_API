using FYP_BACKEND_API.Controllers.DB;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

public class LocationHub : Hub
{
    private static Dictionary<string, string> _userConnections = new();
    private static Dictionary<string, (double Latitude, double Longitude)> _userLocations = new();

    public override async Task OnConnectedAsync()
    {
        string userId = Context.GetHttpContext().Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections[userId] = Context.ConnectionId;
            Console.WriteLine($"✅ User {userId} connected with ID: {Context.ConnectionId}");

            // Send last known location when user reconnects
            if (_userLocations.ContainsKey(userId))
            {
                var location = _userLocations[userId];
                await Clients.Caller.SendAsync("ReceiveLocation", userId, location.Latitude, location.Longitude);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (!string.IsNullOrEmpty(userId))
        {
            _userConnections.Remove(userId);
            _userLocations.Remove(userId);
            Console.WriteLine($"❌ User {userId} disconnected.");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendLocation(string userId, double latitude, double longitude)
    {
        _userLocations[userId] = (latitude, longitude);
        Console.WriteLine($"📍 Received location from {userId}: ({latitude}, {longitude})");

        // Get friends from the database
        List<string> friends = GetFriends(userId);

        foreach (var friendId in friends)
        {
            if (_userConnections.ContainsKey(friendId))
            {
                await Clients.Client(_userConnections[friendId])
                    .SendAsync("ReceiveLocation", userId, latitude, longitude);
            }
        }
    }

    DatabaseService database = new DatabaseService();

    private List<string> GetFriends(string userId)
    {
        List<string> userFriendList = new();
        try
        {
            DataTable dataTable = database.GetData($"SELECT Friend_Id FROM UserFriends WHERE User_Id = {int.Parse(userId)};");
            foreach (DataRow row in dataTable.Rows)
            {
                userFriendList.Add(row["Friend_Id"].ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching friends: {ex.Message}");
        }
        return userFriendList;
    }
}
