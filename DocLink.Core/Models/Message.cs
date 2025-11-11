namespace DocLink.Core.Models;

public class Message
{
    public Guid Id { get; set; }

    public Guid IdSender { get; set; }
    public Account? Sender { get; set; }

    public Guid IdReceiver { get; set; }
    public Account? Receiver { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime DateSent { get; set; } = DateTime.UtcNow;
}