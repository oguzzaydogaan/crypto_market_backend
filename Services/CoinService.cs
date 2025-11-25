using AutoMapper;
using Microsoft.AspNetCore.SignalR;
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
        public KlineHelper _klineHelper;
        private readonly IHubContext<CryptoHub> _hubContext;
        public IMapper _mapper;
        public CoinService(CoinRepository coinRepository, IMapper mapper, KlineHelper klineHelper, IHubContext<CryptoHub> hubContext)
        {
            _coinRepository = coinRepository;
            _mapper = mapper;
            _klineHelper = klineHelper;
            _hubContext = hubContext;
        }

        public async Task<List<Coin>> GetAllCoinsAsync()
        {
            return await _coinRepository.GetAllCoinsAsync();
        }
        public async Task<Coin?> GetCoinByIdAsync(int id)
        {
            return await _coinRepository.GetCoinByIdAsync(id);
        }
        public async Task<Coin> AddCoinAsync(CoinCreateDTO dto)
        {
            var coin = _mapper.Map<CoinCreateDTO, Coin>(dto);
            var added = await _coinRepository.AddCoinAsync(coin);
            return added;
        }

        public async Task<List<KlineDTO>> GetKlinesBySymbolAsync(string symbol)
        {
            return await _klineHelper.GetKlinesBySymbolAsync(symbol);
        }
    }
}
