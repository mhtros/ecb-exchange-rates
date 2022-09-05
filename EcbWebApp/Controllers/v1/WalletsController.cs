using EcbWebApp.Entities;
using EcbWebApp.Exceptions;
using EcbWebApp.Models;
using EcbWebApp.Repositories;
using EcbWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcbWebApp.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletAdjustmentService _walletAdjustmentService;
    private readonly IWalletRepository _walletRepository;

    public WalletsController(IWalletRepository walletRepository, IWalletAdjustmentService walletAdjustmentService)
    {
        _walletRepository = walletRepository;
        _walletAdjustmentService = walletAdjustmentService;
    }

    /// <summary>
    ///     Creates a new wallet entity
    /// </summary>
    /// <param name="model">
    ///     <see cref="CreateWalletModel" />
    /// </param>
    [HttpPost]
    public async Task<IActionResult> CreateWallet(CreateWalletModel model)
    {
        var entity = new WalletEntity(model.CurrencyCode, model.Balance, DateTime.Now, DateTime.Now);
        await _walletRepository.CreateWalletAsync(entity);

        var saved = await _walletRepository.SaveAsync();
        if (!saved) return UnprocessableEntity("Entity not Saved");

        return Ok();
    }

    /// <summary>
    ///     Adjust balance (add or subtract money) for a wallet
    /// </summary>
    /// <param name="exchangeRateStrategy">Strategy type used for exchange rate calculation</param>
    /// <param name="exchangeRateDate"> Search date to find exchange rates</param>
    /// <param name="payload">
    ///     <see cref="AdjustBalancePayload" />
    /// </param>
    /// <response code="200">Ιf there adjust balance calculation succeeds</response>
    /// <response code="400">Ιf the payload or query params are invalid</response>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<decimal>> AdjustBalance(
        [FromQuery] string exchangeRateStrategy,
        [FromQuery] DateTime exchangeRateDate,
        [FromBody] AdjustBalancePayload payload)
    {
        var walletExist = await _walletRepository.WalletExistsAsync(payload.WalletId);
        if (walletExist == false) return BadRequest();

        var validStrategy = Constants.ExchangeRateStrategies.StrategiesList.Exists(x => x == exchangeRateStrategy);
        if (validStrategy == false) return BadRequest($"\"{exchangeRateStrategy}\": Not a valid strategy");

        try
        {
            var newBalance = await _walletAdjustmentService.AdjustBalanceAsync(exchangeRateStrategy, exchangeRateDate,
                payload.WalletId, payload.CurrencyCode, payload.Amount);

            return Ok(newBalance);
        }
        catch (ExchangeRateDateNotFoundException e)
        {
            var badRequestResponse = new
            {
                Message = "Exchange rate date not found",
                e.CurrencyCode,
                e.Date
            };

            return BadRequest(badRequestResponse);
        }
        catch (NoSufficientBalanceException e)
        {
            var badRequestResponse = new
            {
                Message = "No sufficient balance",
                e.WalletId,
                e.RequestedAmount,
                e.AvailableBalance
            };

            return BadRequest(badRequestResponse);
        }
    }
}