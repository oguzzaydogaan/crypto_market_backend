using Microsoft.AspNetCore.SignalR;
using Services.Helpers;

namespace Services.Hubs
{
    public class CryptoHub : Hub
    {
        private readonly BinanceStreamManager _streamManager;

        public CryptoHub(BinanceStreamManager streamManager)
        {
            _streamManager = streamManager;
        }

        public async Task JoinCoinGroup(string symbol)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, symbol.ToUpper());
            await _streamManager.JoinStreamAsync(symbol);
        }

        public async Task LeaveCoinGroup(string symbol)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol.ToUpper());
            await _streamManager.LeaveStreamAsync(symbol);
        }
    }
}
