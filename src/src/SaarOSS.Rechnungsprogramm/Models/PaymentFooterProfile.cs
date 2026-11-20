namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class PaymentFooterProfile
{
 public bool ShowBankDetailsOnInvoices { get; set; } = true;
 public bool ShowBankDetailsOnOffers { get; set; }
 public string AccountHolder { get; set; } = "";
 public string BankName { get; set; } = "";
 public string Iban { get; set; } = "";
 public string Bic { get; set; } = "";
 public string PaymentInstruction { get; set; } = "Bitte überweisen Sie den Rechnungsbetrag innerhalb der angegebenen Zahlungsfrist auf das folgende Konto.";
 public string PurposeTemplate { get; set; } = "Rechnung {Rechnungsnummer} · Kunde {Kundennummer}";
 public string FooterLine1 { get; set; } = "";
 public string FooterLine2 { get; set; } = "";
 public string FooterLine3 { get; set; } = "";
 public string FooterLine4 { get; set; } = "";
 public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
