namespace DocLink.Core.Models;

public class Log
{
    public Guid Id{ get; set; }

    public Guid IdAccount { get; set; }
    public Account? Account { get; set; }

    public string Action { get; set; } = string.Empty;
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}