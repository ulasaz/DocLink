namespace DocLink.Core.Models;

public class Offer
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public decimal Price { get; set; }
    
    public Specialist? Specialist { get; set; }
}