namespace DocLink.Core.Models;

public class Location
{
    public Guid Id { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public int Number { get; set; }
    public int Post { get; set; }
    public ICollection<SpecialistLocation> Specialists { get; set; } = new List<SpecialistLocation>();
}