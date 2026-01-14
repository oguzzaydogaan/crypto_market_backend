using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Repositories;
using Services.Helpers;
using Services.Hubs;

namespace Services
{
    public class CoinService
    {
        public CoinRepository _coinRepository;
        public BinanceHelper _binanceHelper;
        private readonly BinanceTickerManager _tickerManager;
        private readonly IHubContext<CryptoHub> _hubContext;
        public IMapper _mapper;
        public CoinService(CoinRepository coinRepository, IMapper mapper, BinanceHelper binancehelper, IHubContext<CryptoHub> hubContext, BinanceTickerManager tickerManager)
        {
            _coinRepository = coinRepository;
            _mapper = mapper;
            _binanceHelper = binancehelper;
            _hubContext = hubContext;
            _tickerManager = tickerManager;
        }
        public async Task<List<Coin>> GetAllCoinsAsync()
        {
            return await _coinRepository.GetAllCoinsAsync();
        }
        public async Task<Coin?> GetCoinByIdAsync(int id)
        {
            return await _coinRepository.GetCoinByIdAsync(id);
        }
        public async Task<Coin?> AddCoinAsync(CoinCreateDTO dto)
        {
            if (await VerifyCoinAsync(dto.Symbol))
            {
                var coin = _mapper.Map<CoinCreateDTO, Coin>(dto);
                var added = await _coinRepository.AddCoinAsync(coin);
                _tickerManager.SubscribeToCoin(dto.Symbol);
                return added;
            }
            return null;
            
        }
        public async Task<bool> DeleteCoinAsync(int id)
        {
            try
            {
                await _coinRepository.DeleteCoinAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> VerifyCoinAsync(string symbol)
        {
            if (!symbol.EndsWith("USDT"))
            {
                return false;
            }
            return await _binanceHelper.VerifyCoinAsync(symbol);
        }
        public async Task<List<Coin>> GetFavoriteCoinsAsync()
        {
            return await _coinRepository.GetFavoriteCoinsAsync();
        }
        public async Task<Coin> ToggleFavoriteCoinAsync(int id)
        {
            return await _coinRepository.ToggleFavoriteCoinAsync(id);
        }
        public async Task<List<KlineDTO>> GetKlinesBySymbolAsync(string symbol)
        {
            return await _binanceHelper.GetKlinesBySymbolAsync(symbol);
        }
        public async Task<List<TradeDTO>> GetRecentTradesAsync(string symbol)
        {
            return await _binanceHelper.GetRecentTradesAsync(symbol);
        }
    }
}
