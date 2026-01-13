namespace DocLink.Core.Models;

public class OfferSpecialist
{
    public Guid Id { get; set; }

    public Guid OfferId { get; set; }
    public Offer Offer { get; set; }

    public Guid SpecialistId { get; set; }
    public Specialist Specialist { get; set; }
}