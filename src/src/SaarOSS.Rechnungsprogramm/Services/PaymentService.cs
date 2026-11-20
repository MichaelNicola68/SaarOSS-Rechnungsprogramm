using System.IO;
using System.Text.Json;
using SaarOSS.Rechnungsprogramm.Models;

namespace SaarOSS.Rechnungsprogramm.Services;

public sealed class PaymentService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _folder;
    private string FilePath => Path.Combine(_folder, "payments.json");
    private string BackupPath => Path.Combine(_folder, "payments.backup.json");

    public PaymentService() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaarOSS", "Rechnungsprogramm")) { }
    internal PaymentService(string folder) => _folder = folder;

    public PaymentStore Load()
    {
        Directory.CreateDirectory(_folder);
        if (!File.Exists(FilePath)) return new PaymentStore();
        try
        {
            var store = JsonSerializer.Deserialize<PaymentStore>(File.ReadAllText(FilePath), Options) ?? new PaymentStore();
            ValidateStore(store);
            return store;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or InvalidDataException)
        {
            throw new InvalidDataException("Die Zahlungsdatei konnte nicht gelesen werden. Die Datei wurde nicht überschrieben.", ex);
        }
    }

    public void AddPayment(PaymentStore store, PaymentEntry payment)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(payment);
        ValidatePayment(payment);
        if (store.Payments.Any(x => x.Id == payment.Id)) throw new InvalidOperationException("Dieser Zahlungseintrag ist bereits vorhanden.");
        store.Payments.Add(payment);
        try { Save(store); }
        catch { store.Payments.Remove(payment); throw; }
    }

    public IReadOnlyList<PaymentEntry> GetPayments(PaymentStore store, Guid invoiceId) =>
        store.Payments.Where(x => x.InvoiceId == invoiceId).OrderBy(x => x.PaymentDate).ThenBy(x => x.CreatedAt).ToList();

    public PaymentSummary CalculateSummary(PaymentStore store, Guid invoiceId, decimal invoiceTotal, DateTime dueDate, bool isCancelled)
    {
        if (invoiceTotal < 0) throw new ArgumentOutOfRangeException(nameof(invoiceTotal));
        var paid = store.Payments.Where(x => x.InvoiceId == invoiceId).Sum(x => x.Amount);
        var open = Math.Max(0m, invoiceTotal - paid);
        var status = isCancelled ? PaymentStatuses.Cancelled
            : open == 0m ? PaymentStatuses.Paid
            : paid > 0m ? PaymentStatuses.PartiallyPaid
            : dueDate.Date < DateTime.Today ? PaymentStatuses.Overdue
            : PaymentStatuses.Open;
        return new PaymentSummary { InvoiceTotal = invoiceTotal, PaidAmount = paid, OpenAmount = open, Status = status };
    }

    private void Save(PaymentStore store)
    {
        ValidateStore(store);
        Directory.CreateDirectory(_folder);
        var temp = FilePath + ".tmp";
        var json = JsonSerializer.Serialize(store, Options);
        File.WriteAllText(temp, json);
        try
        {
            if (File.Exists(FilePath)) File.Replace(temp, FilePath, BackupPath, true);
            else File.Move(temp, FilePath);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }

    private static void ValidateStore(PaymentStore store)
    {
        if (store.SchemaVersion != 1) throw new InvalidDataException("Die Version der Zahlungsdatei wird nicht unterstützt.");
        if (store.Payments is null) throw new InvalidDataException("Die Zahlungsliste fehlt.");
        foreach (var payment in store.Payments) ValidatePayment(payment);
        if (store.Payments.Select(x => x.Id).Distinct().Count() != store.Payments.Count) throw new InvalidDataException("Die Zahlungsdatei enthält doppelte IDs.");
    }

    private static void ValidatePayment(PaymentEntry payment)
    {
        if (payment.Id == Guid.Empty) throw new ArgumentException("Die Zahlungs-ID fehlt.");
        if (payment.InvoiceId == Guid.Empty) throw new ArgumentException("Die Rechnungs-ID fehlt.");
        if (payment.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(payment.Amount), "Der Zahlungsbetrag muss größer als 0 sein.");
        if (!PaymentMethods.IsAllowed(payment.PaymentMethod)) throw new ArgumentException("Als Zahlungsart sind nur Überweisung und Barzahlung erlaubt.");
        if (payment.Reference.Length > 200) throw new ArgumentException("Die Referenz ist zu lang.");
        if (payment.InternalNote.Length > 1000) throw new ArgumentException("Die interne Notiz ist zu lang.");
    }
}
