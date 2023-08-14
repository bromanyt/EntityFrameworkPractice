namespace WebRetail.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public string Information { get; set; } = "No information";

    public ErrorViewModel() { }

    public ErrorViewModel(Exception? exception)
    {
        if (exception is not null)
        {
            Information = exception.Message;
        }
    }

    public ErrorViewModel(string message) => Information = message;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
