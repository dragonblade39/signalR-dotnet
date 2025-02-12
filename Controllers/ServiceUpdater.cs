using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Hubs;

namespace WebApplication1.Controllers
{
    public class ServiceUpdater : BackgroundService
    {
        private readonly IHubContext<TreeNodeHub> _hubContext;
        private readonly ILogger<ServiceUpdater> _logger;
        private readonly HttpClient _httpClient;
        public ServiceUpdater(IHubContext<TreeNodeHub> hubContext, HttpClient httpClient,ILogger<ServiceUpdater> logger)
        {
            _hubContext = hubContext;
            _httpClient = httpClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            while(!cancellation.IsCancellationRequested){
                _logger.LogInformation("Method Called");
                var response = await _httpClient.GetAsync("https://localhost:7068/api/TreeNodes");
                await _hubContext.Clients.All.SendAsync("ReceiveTreeNode", response);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellation);
            }
        }
        
    }
}
