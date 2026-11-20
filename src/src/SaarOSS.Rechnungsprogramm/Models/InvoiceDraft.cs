namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class InvoiceDraft
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public int DraftNumber { get; set; }
 public string DraftId => $"ENT-{DraftNumber:00000}";
 public string InvoiceNumber { get; set; } = "";
 public Guid CustomerId { get; set; }
 public string CustomerNumber { get; set; } = "";
 public string CustomerName { get; set; } = "";
 public string CustomerAddressSnapshot { get; set; } = "";
 public string CompanySnapshot { get; set; } = "";
 public string TaxModeSnapshot { get; set; } = "Regelbesteuerung";
 public string SmallBusinessNoticeSnapshot { get; set; } = "";
 public bool IsSmallBusinessSnapshot => TaxModeSnapshot == "Kleinunternehmer";
 public bool ShowLogoSnapshot { get; set; }
 public string LogoPngBase64Snapshot { get; set; } = "";
 public string HeaderLine1Snapshot { get; set; } = "";
 public string HeaderLine2Snapshot { get; set; } = "";
 public string HeaderLine3Snapshot { get; set; } = "";
 public string HeaderLine4Snapshot { get; set; } = "";
 public bool ShowBankDetailsSnapshot { get; set; }
 public string AccountHolderSnapshot { get; set; } = "";
 public string BankNameSnapshot { get; set; } = "";
 public string IbanSnapshot { get; set; } = "";
 public string BicSnapshot { get; set; } = "";
 public string PaymentInstructionSnapshot { get; set; } = "";
 public string PurposeTemplateSnapshot { get; set; } = "";
 public string FooterLine1Snapshot { get; set; } = "";
 public string FooterLine2Snapshot { get; set; } = "";
 public string FooterLine3Snapshot { get; set; } = "";
 public string FooterLine4Snapshot { get; set; } = "";
 public DateTime InvoiceDate { get; set; } = DateTime.Today;
 public DateTime ServiceDate { get; set; } = DateTime.Today;
 public int PaymentTermDays { get; set; } = 14;
 public string IntroText { get; set; } = "Vielen Dank für Ihren Auftrag. Wir berechnen folgende Leistungen:";
 public string ClosingText { get; set; } = "Bitte überweisen Sie den Rechnungsbetrag innerhalb der angegebenen Zahlungsfrist.";
 public string InternalNote { get; set; } = "";
 public string Status { get; set; } = "Entwurf";
 public string CancellationReason { get; set; } = "";
 public DateTimeOffset? FinalizedAt { get; set; }
 public DateTimeOffset? CancelledAt { get; set; }
 public List<InvoiceLine> Lines { get; set; } = new();
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
 public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
 public decimal NetTotal => Lines.Sum(x => x.NetAmount);
 public decimal VatTotal => Lines.Sum(x => x.VatAmount);
 public decimal GrossTotal => Lines.Sum(x => x.GrossAmount);
 public string DisplayNumber => string.IsNullOrWhiteSpace(InvoiceNumber) ? DraftId : InvoiceNumber;
 public string GrossTotalDisplay => $"{GrossTotal:N2} €";
 public string PaymentStatusDisplay { get; set; } = "–";
 public string OpenAmountDisplay { get; set; } = "–";
}
public sealed class InvoiceLine
{
 public string Description { get; set; } = "";
 public decimal Quantity { get; set; } = 1m;
 public string Unit { get; set; } = "Stück";
 public decimal UnitPrice { get; set; }
 public decimal VatRate { get; set; } = 19m;
 public decimal DiscountPercent { get; set; }
 public decimal NetAmount => Math.Round(Quantity * UnitPrice * (1m - DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
 public decimal VatAmount => Math.Round(NetAmount * VatRate / 100m, 2, MidpointRounding.AwayFromZero);
 public decimal GrossAmount => NetAmount + VatAmount;
}
public sealed class InvoiceStore
{
 public int NextDraftNumber { get; set; } = 1;
 public string InvoicePrefix { get; set; } = "RE-";
 public bool IncludeYear { get; set; } = true;
 public int InvoiceDigits { get; set; } = 5;
 public int NextInvoiceNumber { get; set; } = 300;
 public List<InvoiceDraft> Drafts { get; set; } = new();
}
public sealed class CustomerChoice
{
 public Guid Id { get; set; }
 public string CustomerNumber { get; set; } = "";
 public string DisplayName { get; set; } = "";
 public string AddressSnapshot { get; set; } = "";
 public int? PaymentTermDays { get; set; }
 public bool IsArchived { get; set; }
 public override string ToString() => $"{CustomerNumber} – {DisplayName}";
}
