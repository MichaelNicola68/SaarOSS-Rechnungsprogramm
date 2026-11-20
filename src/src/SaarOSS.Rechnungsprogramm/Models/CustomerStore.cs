namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class CustomerStore { public string NumberPrefix { get; set; } = "KD-"; public int NumberDigits { get; set; } = 5; public int NextSequenceNumber { get; set; } = 211; public bool NumberingMigratedTo211 { get; set; } public List<Customer> Customers { get; set; } = new(); }
