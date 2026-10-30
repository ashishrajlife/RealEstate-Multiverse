namespace Multiverse.Web.Models;

public enum ClientStatus
{
    Active = 1,
    Inactive = 2,
    Blacklisted = 3
}

public enum ClientType
{
    Buyer = 1,
    Seller = 2,
    Tenant = 3,
    Landlord = 4,
    Investor = 5
}

public enum InterestKind
{
    Buy = 1,
    Rent = 2,
    Invest = 3
}

public enum PropertyKind
{
    Flat = 1,
    Plot = 2,
    Villa = 3,
    Commercial = 4,
    IndependentHouse = 5
}

public enum BhkPreference
{
    Any = 0,
    OneBHK = 1,
    TwoBHK = 2,
    ThreeBHK = 3,
    FourBHK = 4,
    FivePlusBHK = 5,
    NotApplicable = 9
}

public enum PossessionPreference
{
    ReadyToMove = 1,
    UnderConstruction = 2,
    Either = 3
}

public enum LoanPreference
{
    Yes = 1,
    No = 2,
    NotSure = 3
}

public enum LeadSource
{
    Website = 1,
    Referral = 2,
    FacebookAds = 3,
    WalkIn = 4,
    Broker = 5,
    Other = 9
}

public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Converted = 4,
    Lost = 5
}

public enum LeadPriority
{
    Hot = 1,
    Warm = 2,
    Cold = 3
}

public enum DocumentType
{
    Aadhaar = 1,
    Pan = 2,
    LandProof = 3,
    SaleAgreement = 4,
    Other = 5
}