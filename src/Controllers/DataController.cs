using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using WebRetail.Models;
using WebRetail.ViewModels;

namespace WebRetail.Controllers;

public class DataController : Controller
{
    private readonly Serilog.ILogger _logger;
    private readonly RetailContext _dbContext;

    public DataController(RetailContext db, Serilog.ILogger logger)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        _logger = logger;
        _dbContext = db;
    }

    [HttpGet]
    [Route("/Data")]
    [Route("/Data/Index")]
    public IActionResult Index()
    {
        _logger.Information("Open page Data.");
        return View();
    }

    [HttpGet]
    [Route("/Data/ShowTable/{tableName}")]
    public IActionResult ShowTable(string tableName)
    {
        try
        {
            _logger.Information($"Try to open table {tableName}");
            List<IRetailTable>? list = _dbContext.GetTable(tableName);
            _logger.Information($"Send {tableName} table to view.");
            ViewData["TableName"] = tableName;
            return View("Table", list ?? null);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }


    [HttpGet]
    public IActionResult Edit(string[] items)
    {
        try
        {
            _logger.Information("Try to find object");
            var obj = _dbContext.GetTableElement(items);
            _logger.Information("Object found");
            return View(obj);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpPost]
    public IActionResult EditObject(IRetailTable obj)
    {
        try
        {
            _logger.Information("Try to edit object");
            string? result = _dbContext.ChangeState(obj, EntityState.Modified);
            _logger.Information("Object modified");
            return RedirectToAction("ShowTable", new { tableName = result });
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpGet]
    public IActionResult Delete(string[] items)
    {
        try
        {
            _logger.Information("Try to delete object");
            string? result = _dbContext.DeleteObject(items);
            _logger.Information("Object deleted");
            return RedirectToAction("ShowTable", new { tableName = result });
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpGet]
    [Route("/Data/Add/{tableName}")]
    public IActionResult Add(string tableName)
    {
        try
        {
            _logger.Information("Try to create object");
            Type? type = Type.GetType($"WebRetail.Models.{tableName}");
            IRetailTable? obj = null;
            if (type != null)
            {
                obj = Activator.CreateInstance(type) as IRetailTable;
                _logger.Information("Object created");
            }
            return View(obj);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpPost]
    public IActionResult AddObject(IRetailTable obj)
    {
        try
        {
            _logger.Information("Try to add object");
            string? result = _dbContext.ChangeState(obj, EntityState.Added);
            _logger.Information("Object has been added");
            return RedirectToAction("ShowTable", new { tableName = result });
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel((ex is RetailException) ? ex : null));
        }
    }

    [HttpPost]
    [Route("/Data/Export/{tableName}/{filename}/{delimiter}")]
    public IActionResult Export(string tableName, string filename, string delimiter)
    {
        try
        {
            _logger.Information($"Try to export the table {tableName} with the delimiter" +
                $" {delimiter} to the file {filename}.");
            List<IRetailTable> dbTable = _dbContext.GetTable(tableName)!;
            ExportViewModel viewModel = new() { Table = dbTable, Delimiter = delimiter };
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(viewModel.FormExport()));
            _logger.Information($"The Table {tableName} with the delimiter {delimiter} " +
                $"is saved to the file {filename}.");
            return new FileStreamResult(stream, "text/plain")
            {
                FileDownloadName = String.IsNullOrWhiteSpace(filename) ?
                $"{dbTable[0].GetTableName()}_exp.csv" : $"{filename}.csv"
            };
        }
        catch (Exception ex)
        {
            _logger.Warning(ex.Message);
            return View("Error", new ErrorViewModel(ex));
        }
    }

    [HttpPost]
    [Route("/Data/Import/{tableName}/{fromFile}/{delimiter}")]
    public IActionResult Import(string tableName, IFormFile fromFile, string delimiter)
    {
        try
        {
            _logger.Information($"Try to import the table {tableName} with the delimiter " +
                $"{delimiter} from the file {fromFile.FileName}.");

            // This stroke for deploy! Don't delete!
            //string filePath = Path.Combine(_configuration["SharedDir"]!, fromFile.FileName);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fromFile.FileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                fromFile.CopyToAsync(stream);
            }
            Type? type = Type.GetType($"WebRetail.Models.{tableName}");
            var entityTypeOfTable = _dbContext.Model.GetEntityTypes().First(t => t.ClrType == type);
            var tableNameDB = entityTypeOfTable.GetAnnotation("Relational:TableName").Value!.ToString();
            _dbContext!.Database.ExecuteSql($"CALL import_data({filePath},{tableNameDB},{delimiter})");
            _logger.Information($"The table {tableName} with the delimiter {delimiter} " +
                $"is imported from file {fromFile.FileName}.");
            return RedirectToAction("ShowTable", new { tableName });
        }
        catch (Exception ex)
        {
            _logger.Warning($"{ex.Message}");
            return View("Error", new ErrorViewModel(ex));
        }
    }
}