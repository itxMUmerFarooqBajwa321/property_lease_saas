using System.ComponentModel.DataAnnotations;

namespace property_lease_saas.Models.Entities;
public class Property
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; }
    public string Description {set;get;}
    public string LandlordId { get; set; }

    [Required]
    public string Address { get; set; }
    public decimal Rent { get; set; }

    public bool IsPublished { get; set; }
    public bool IsTaken{get;set;} = false;
    public DateTime CreatedAt { get; set; }

    public List<PropertyImage> Images { get; set; }
    public List<PropertyDocument> Documents { get; set; }
}