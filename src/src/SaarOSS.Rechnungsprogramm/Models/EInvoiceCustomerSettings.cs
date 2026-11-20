namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class EInvoiceCustomerSettings
{
 public Guid CustomerId{get;set;} public string CustomerType{get;set;}="Privatkunde"; public string InvoiceFormat{get;set;}="PDF"; public string EInvoiceEmail{get;set;}=""; public string RoutingId{get;set;}=""; public string BuyerReference{get;set;}=""; public string Department{get;set;}=""; public string ContactPerson{get;set;}=""; public DateTimeOffset UpdatedAt{get;set;}=DateTimeOffset.Now;
}
public sealed class EInvoiceCustomerSettingsStore{public int SchemaVersion{get;set;}=1;public List<EInvoiceCustomerSettings> Customers{get;set;}=new();}
