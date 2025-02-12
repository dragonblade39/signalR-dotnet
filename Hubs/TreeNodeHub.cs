using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Model;

namespace WebApplication1.Hubs
{
    public class TreeNodeHub : Hub
    {
        private static readonly ConcurrentDictionary<string, int> Node = new ConcurrentDictionary<string, int>();
        private static string node = string.Empty;
        public async Task JoinCityGroupAsync(string _node)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, _node);
            Node.AddOrUpdate(_node, 1, (key, count) => count + 1);
            //City = city;
        }

        public async Task LeaveCityGroupAsync(string _node)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _node);
            Node.AddOrUpdate(_node, 0, (key, count) => count > 1 ? count - 1 : 0);
            if (Node[_node] == 0)
            {
                Node.TryRemove(node, out _);
            }
            //city = string.Empty;
        }

        public static IEnumerable<string> GetCities()
        {
            lock (Node)
            {
                return Node.Keys.ToList();
            }
        }
    }
}
