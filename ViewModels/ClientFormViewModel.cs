using System.ComponentModel.DataAnnotations;
using Multiverse.Web.Models;

namespace Multiverse.Web.ViewModels;

public class ClientFormViewModel
{
    public int Id { get; set; }

    // ---------- Identity (existing) ----------
    [Required(ErrorMessage = "Full name is required."), MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required."), EmailAddress, MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Use format +91XXXXXXXXXX.")]
    public string Phone { get; set; } = string.Empty;

    [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar must be 12 digits.")]
    public string? AadhaarNumber { get; set; }

    [RegularExpression(@"^[A-Z]{5}\d{4}[A-Z]$", ErrorMessage = "PAN format must be ABCDE1234F.")]
    public string? PanNumber { get; set; }

    [Required(ErrorMessage = "Address is required."), MaxLength(200)]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required."), MaxLength(60)]
    public string City { get; set; } = string.Empty;

    [Required]
    public IndianState State { get; set; } = IndianState.Maharashtra;

    [Required(ErrorMessage = "PIN code is required.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN code must be 6 digits.")]
    public string PinCode { get; set; } = string.Empty;

    public ClientStatus Status { get; set; } = ClientStatus.Active;

    // ---------- Profile ----------
    public ClientType ClientType { get; set; } = ClientType.Buyer;

    [MaxLength(80)]
    public string? Occupation { get; set; }

    // ---------- Preferences (only meaningful for Buyer/Tenant/Investor) ----------
    public InterestKind? InterestedIn { get; set; }
    public PropertyKind? PreferredPropertyType { get; set; }

    [MaxLength(120)]
    public string? PreferredLocation { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Minimum budget must be non-negative.")]
    public decimal? MinBudget { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Maximum budget must be non-negative.")]
    public decimal? MaxBudget { get; set; }

    public BhkPreference? BHK { get; set; }

    [MaxLength(60)]
    public string? PreferredArea { get; set; }

    public PossessionPreference? Possession { get; set; }
    public LoanPreference? LoanRequired { get; set; }

    // ---------- Lead tracking ----------
    public LeadSource? LeadSource { get; set; }
    public int? AssignedEmployeeId { get; set; }

    public LeadStatus LeadStatus { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Warm;

    public DateTime? NextFollowUpAt { get; set; }

    [MaxLength(2000)]
    public string? InitialNotes { get; set; }

    /// <summary>Populated by the controller — employees available for assignment.</summary>
    public IReadOnlyList<EmployeeOption> AssignableEmployees { get; set; } = Array.Empty<EmployeeOption>();
}

public record EmployeeOption(int Id, string FullName, string RoleLabel);