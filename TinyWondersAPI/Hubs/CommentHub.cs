using Microsoft.AspNetCore.SignalR;

namespace TinyWondersAPI.Hubs;

public class CommentHub : Hub
{
    public async Task JoinArticle(string articleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, articleId);
    }

    public async Task LeaveArticle(string articleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, articleId);
    }
}
