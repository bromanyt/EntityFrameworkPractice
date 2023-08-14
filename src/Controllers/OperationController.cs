using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using WebRetail.Models;
using WebRetail.ViewModels;

namespace WebRetail.Controllers;

public class OperationController : Controller
{
    private readonly Serilog.ILogger _logger;
    private readonly RetailContext? _dbContext;

    public OperationController(RetailContext db, Serilog.ILogger logger)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        _logger = logger;
        _dbContext = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.Information("Open Offers Functions page.");
        return View();
    }

    [HttpGet]
    public IActionResult AverageCheck()
    {
        _logger.Information("Open AverageCheck page.");
        return View(new AverageCheckViewModel());
    }

    [HttpGet]
    public IActionResult FrequencyVisits()
    {
        _logger.Information("Open FrequencyVisits page.");
        return View(new FrequencyVisitsViewModel());
    }

    [HttpGet]
    public IActionResult CrossSelling()
    {
        _logger.Information("Open CrossSelling page.");
        return View(new CrossSellingViewModel());
    }

    [HttpPost]
    [Route("/Operation/AverageCheck/ShowResultAverageCheck")]
    public IActionResult ShowResultAverageCheck(AverageCheckViewModel model)
    {
        string loggerAverageCheck = "Offers formed by the Average Check Growth: ";
        if (ModelState.IsValid)
        {
            try
            {
                var result = _dbContext!.GetOffersByAverageCheck(model.Method, model.FirstDate,
                    model.LastDate, model.TransactionsNumber, model.CoefAverCheckIncrease,
                    model.MaxChurnIndex, model.MaxShareDiscountTransactions, model.MarginShare).ToList();
                if (!result.Any())
                {
                    _logger.Information($"{loggerAverageCheck}result is empty.");
                    model.EmptyResult = "Sorry, No Offers. Use other parameters.";
                    return View("AverageCheck", model);
                }
                model.Table.AddRange(result);
                _logger.Information($"{loggerAverageCheck}result is recieved.");
            }
            catch (Exception ex)
            {
                _logger.Warning(loggerAverageCheck + ex.Message);
                return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
            }
        }
        else
        {
            model.EmptyResult = "No method choosen.";
            _logger.Warning(loggerAverageCheck + model.EmptyResult);
        }
        return View("AverageCheck", model);
    }

    [HttpPost]
    [Route("/Operation/FrequencyVisits/ShowResultFrequencyVisits")]
    public IActionResult ShowResultFrequencyVisits(FrequencyVisitsViewModel model)
    {
        string loggerFrequencyVisits = "Offers formed by the Frequency Visits Increase: ";
        try
        {
            var result = _dbContext!.GetOffersByFrequencyVisits(model.FirstDate,
                model.LastDate, model.AddedTransactionsNumber, model.MaxChurnIndex,
                model.MaxShareDiscountTransactions, model.MarginShare).ToList();
            if (!result.Any())
            {
                _logger.Information($"{loggerFrequencyVisits}result is empty.");
                model.EmptyResult = "Sorry, No Offers. Use other parameters.";
                return View("FrequencyVisits", model);
            }
            model.Table.AddRange(result);
            _logger.Information($"{loggerFrequencyVisits}result is recieved.");
            return View("FrequencyVisits", model);
        }
        catch (Exception ex)
        {
            _logger.Warning(loggerFrequencyVisits + ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpPost]
    [Route("/Operation/CrossSelling/ShowResultCrossSelling")]
    public IActionResult ShowResultCrossSelling(CrossSellingViewModel model)
    {
        string loggerCrossSelling = "Offers formed by the Cross-Selling: ";
        try
        {
            var result = _dbContext!.GetOffersByCrossSelling(model.GroupsNumber,
                model.MaxChurnIndex, model.MaxConsumptionStabilityIndex,
                model.MaxSkuShare, model.MarginShare).ToList();
            if (!result.Any())
            {
                _logger.Information($"{loggerCrossSelling}result is empty.");
                model.EmptyResult = "Sorry, No Offers. Use other parameters.";
                return View("CrossSelling", model);
            }
            model.Table.AddRange(result);
            _logger.Information($"{loggerCrossSelling}result is recieved.");
            return View("CrossSelling", model);
        }
        catch (Exception ex)
        {
            _logger.Warning(loggerCrossSelling + ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpGet]
    [Route("/Operation/ClearInput/{modelName}")]
    public IActionResult ClearInput(string modelName)
    {
        return modelName switch
        {
            "AverageCheckViewModel" => RedirectToAction("AverageCheck", "Operation"),
            "FrequencyVisitsViewModel" => RedirectToAction("FrequencyVisits", "Operation"),
            "CrossSellingViewModel" => RedirectToAction("CrossSelling", "Operation"),
            _ => RedirectToPage("Home", "Error"),
        };
    }
}

