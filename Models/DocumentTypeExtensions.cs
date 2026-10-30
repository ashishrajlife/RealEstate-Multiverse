namespace Multiverse.Web.Models;

public static class DocumentTypeExtensions
{
    public static string FolderName(this DocumentType t) => t switch
    {
        DocumentType.Aadhaar       => "Aadhaar",
        DocumentType.Pan           => "PAN",
        DocumentType.LandProof     => "LandProof",
        DocumentType.SaleAgreement => "SaleAgreement",
        _                          => "Other"
    };

    public static string Label(this DocumentType t) => t switch
    {
        DocumentType.Aadhaar       => "Aadhaar",
        DocumentType.Pan           => "PAN card",
        DocumentType.LandProof     => "Land proof / title deed",
        DocumentType.SaleAgreement => "Sale agreement / signed letter",
        _                          => "Other"
    };
}