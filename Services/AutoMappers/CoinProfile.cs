using AutoMapper;
using Repositories.DTOs;
using Repositories.Entities;

namespace Services.Automappers
{
    public class CoinProfile : Profile
    {
        public CoinProfile()
        {
            CreateMap<Coin, CoinCreateDTO>();
            CreateMap<CoinCreateDTO, Coin>();
        }
    }
}
