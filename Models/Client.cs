using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Multiverse.Web.Models;

public class Client
{
    public int Id { get; set; }

    // ---------- Identity (existing) ----------
    [Required, MaxLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(15)]  public string Phone { get; set; } = string.Empty;
    [MaxLength(12)] public string? AadhaarNumber { get; set; }
    [MaxLength(10)] public string? PanNumber { get; set; }
    [Required, MaxLength(200)] public string AddressLine { get; set; } = string.Empty;
    [Required, MaxLength(60)]  public string City { get; set; } = string.Empty;
    [Required] public IndianState State { get; set; }
    [Required, MaxLength(6)] public string PinCode { get; set; } = string.Empty;
    public ClientStatus Status { get; set; } = ClientStatus.Active;

    // ---------- Profile (new) ----------
    public ClientType ClientType { get; set; } = ClientType.Buyer;

    [MaxLength(80)]
    public string? Occupation { get; set; }         // "Salaried – IT", "Business", "Doctor", …

    // ---------- Preferences (new — mostly relevant for Buyer/Tenant/Investor) ----------
    public InterestKind? InterestedIn { get; set; }         // Buy / Rent / Invest
    public PropertyKind? PreferredPropertyType { get; set; } // Flat / Plot / Villa / Commercial
    [MaxLength(120)] public string? PreferredLocation { get; set; }

    [Column(TypeName = "decimal(14,2)")]
    public decimal? MinBudget { get; set; }         // in INR

    [Column(TypeName = "decimal(14,2)")]
    public decimal? MaxBudget { get; set; }

    public BhkPreference? BHK { get; set; }         // 1BHK..5BHK, N/A
    [MaxLength(60)] public string? PreferredArea { get; set; }  // free text: "1000–1500 sq.ft."
    public PossessionPreference? Possession { get; set; }       // Ready / UnderConstruction
    public LoanPreference? LoanRequired { get; set; }           // Yes / No / NotSure

    // ---------- Lead tracking (new) ----------
    public LeadSource? LeadSource { get; set; }     // Website / Referral / FacebookAds / WalkIn / Broker
    public int? AssignedEmployeeId { get; set; }    // FK → User (any internal role)
    public User? AssignedEmployee { get; set; }

    public LeadStatus LeadStatus { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Warm;

    public DateTime? NextFollowUpAt { get; set; }
    [MaxLength(2000)] public string? InitialNotes { get; set; }

    // ---------- Audit (existing) ----------
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int CreatedByUserId { get; set; }

    public ICollection<ClientDocument> Documents { get; set; } = new List<ClientDocument>();
}