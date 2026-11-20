namespace SaarOSS.Rechnungsprogramm.Models;

public static class PaymentMethods
{
    public const string BankTransfer = "Überweisung";
    public const string Cash = "Barzahlung";
    public static readonly string[] All = [BankTransfer, Cash];
    public static bool IsAllowed(string? value) => All.Contains(value, StringComparer.Ordinal);
}

public static class PaymentStatuses
{
    public const string Open = "Offen";
    public const string PartiallyPaid = "Teilweise bezahlt";
    public const string Paid = "Bezahlt";
    public const string Overdue = "Überfällig";
    public const string Cancelled = "Storniert";
}

public sealed class PaymentEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = PaymentMethods.BankTransfer;
    public string Reference { get; set; } = "";
    public string InternalNote { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}

public sealed class PaymentStore
{
    public int SchemaVersion { get; set; } = 1;
    public List<PaymentEntry> Payments { get; set; } = new();
}

public sealed class PaymentSummary
{
    public decimal InvoiceTotal { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal OpenAmount { get; init; }
    public string Status { get; init; } = PaymentStatuses.Open;
}
