namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class DocumentDesignProfile
{
    public string LogoFileName { get; set; } = "";
    public bool ShowLogo { get; set; } = true;
    public string HeaderLine1 { get; set; } = "";
    public string HeaderLine2 { get; set; } = "";
    public string HeaderLine3 { get; set; } = "";
    public string HeaderLine4 { get; set; } = "";
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
