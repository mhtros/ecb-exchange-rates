using System.Text.Json;
using EcbWebApp.Entities;
using EcbWebApp.Models;
using EcbWebApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace EcbWebApp.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RatesController : ControllerBase
{
    private readonly IDiagnosticContext _diagnosticContext;
    private readonly ICurrencyRatesRepository _repository;

    public RatesController(ICurrencyRatesRepository repository, IDiagnosticContext diagnosticContext)
    {
        _repository = repository;
        _diagnosticContext = diagnosticContext;
    }

    /// <summary>
    ///     Returns a list with all the CurrencyRate objects for the specific day
    /// </summary>
    /// <param name="date">Search date</param>
    /// <response code="200">Ιf there is at least one CurrencyRate for a particular day</response>
    /// <response code="404">If there is no CurrencyRate for a particular day</response>
    /// <response code="400">Ιf the search date is not initialized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyRateModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CurrencyRateEntity>>> GetRates([FromQuery] DateTime date)
    {
        _diagnosticContext.Set("QueryString", HttpContext.Request.QueryString);

        var currencyRates = (await _repository.GetRatesAsync(date)).ToArray();
        if (!currencyRates.Any()) return NotFound();

        _diagnosticContext.Set("ResponseBody", JsonSerializer.Serialize(currencyRates));

        return Ok(currencyRates);
    }
}