using System.Text.Json.Serialization;

namespace DocLink.Core.Models;

public class Offer
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public decimal Price { get; set; }
    
    public ICollection<OfferSpecialist> Specialists { get; set; } = new List<OfferSpecialist>();
}