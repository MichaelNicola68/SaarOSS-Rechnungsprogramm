namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class Customer
{
 public Guid Id { get; set; } = Guid.NewGuid(); public int SequenceNumber { get; set; }
 public string NumberPrefix { get; set; } = "KD-"; public int NumberDigits { get; set; } = 5;
 public string CustomerNumber => $"{NumberPrefix}{SequenceNumber.ToString().PadLeft(NumberDigits, '0')}";
 public string CustomerType { get; set; } = "Unternehmen"; public string CompanyName { get; set; } = ""; public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; public string ContactPerson { get; set; } = ""; public string Street { get; set; } = ""; public string AddressExtra { get; set; } = ""; public string PostalCode { get; set; } = ""; public string City { get; set; } = ""; public string Country { get; set; } = "Deutschland"; public string Email { get; set; } = ""; public string Phone { get; set; } = ""; public string Mobile { get; set; } = ""; public string Website { get; set; } = ""; public string VatId { get; set; } = ""; public int? PaymentTermDays { get; set; } public string Notes { get; set; } = ""; public bool IsArchived { get; set; } public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now; public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
 public string DisplayName => CustomerType == "Unternehmen" ? CompanyName : $"{FirstName} {LastName}".Trim();
}
