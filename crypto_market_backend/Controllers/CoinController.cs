using Microsoft.AspNetCore.Mvc;
using Repositories.DTOs;
using Services;

namespace crypto_market_backend.Controllers
{
    [Route("api/coins")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        public ILogger<CoinController> _logger;
        public CoinService _coinService;
        public CoinController(CoinService coinService, ILogger<CoinController> logger)
        {
            _coinService = coinService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCoins()
        {
            try
            {
                var coins = await _coinService.GetAllCoinsAsync();
                return Ok(coins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all coins.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCoinById(int id)
        {
            try
            {
                var coin = await _coinService.GetCoinByIdAsync(id);
                if (coin == null)
                {
                    return NotFound();
                }
                return Ok(coin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting coin with ID {id}.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddCoin([FromBody] CoinCreateDTO dto)
        {
            try
            {
                var coin = await _coinService.AddCoinAsync(dto);
                return Ok(coin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new coin.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{symbol}/klines")]
        public async Task<IActionResult> GetKlinesBySymbol(string symbol)
        {
            try
            {
                var klines = await _coinService.GetKlinesBySymbolAsync(symbol);
                return Ok(klines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting klines for symbol {symbol}.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
