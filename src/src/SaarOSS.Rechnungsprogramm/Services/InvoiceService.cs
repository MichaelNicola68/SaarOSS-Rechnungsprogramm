using System.IO;
using System.Text.Json;
using SaarOSS.Rechnungsprogramm.Models;
namespace SaarOSS.Rechnungsprogramm.Services;
public sealed class InvoiceService
{
 private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
 private readonly string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaarOSS", "Rechnungsprogramm");
 private string InvoicePath => Path.Combine(_folder, "invoice-drafts.json");
 private string InvoiceBackupPath => Path.Combine(_folder, "invoice-drafts.backup.json");
 private string CustomerPath => Path.Combine(_folder, "customers.json");
 private string CompanyPath => Path.Combine(_folder, "company-settings.json");
 public InvoiceStore LoadInvoices(){if(!File.Exists(InvoicePath))return new();try{return JsonSerializer.Deserialize<InvoiceStore>(File.ReadAllText(InvoicePath),Options)??new();}catch(Exception ex){throw new InvalidDataException("Die Rechnungsdatei konnte nicht gelesen werden. Die Datei wurde nicht überschrieben.",ex);}}
 public List<CustomerChoice> LoadCustomers()
 {
  if(!File.Exists(CustomerPath))return new();using var doc=JsonDocument.Parse(File.ReadAllText(CustomerPath));if(!doc.RootElement.TryGetProperty("Customers",out var customers))return new();var result=new List<CustomerChoice>();
  foreach(var x in customers.EnumerateArray()){var archived=x.TryGetProperty("IsArchived",out var a)&&a.GetBoolean();var type=x.TryGetProperty("CustomerType",out var t)?t.GetString():"Unternehmen";var company=ReadString(x,"CompanyName");var first=ReadString(x,"FirstName");var last=ReadString(x,"LastName");var display=type=="Privatperson"?$"{first} {last}".Trim():company;var address=string.Join(Environment.NewLine,new[]{display,ReadString(x,"Street"),$"{ReadString(x,"PostalCode")} {ReadString(x,"City")}".Trim(),ReadString(x,"Country")}.Where(v=>!string.IsNullOrWhiteSpace(v)));result.Add(new CustomerChoice{Id=x.GetProperty("Id").GetGuid(),CustomerNumber=ReadCustomerNumber(x),DisplayName=display,AddressSnapshot=address,PaymentTermDays=ReadNullableInt(x,"PaymentTermDays"),IsArchived=archived});}
  return result.Where(x=>!x.IsArchived).OrderBy(x=>x.DisplayName).ToList();
 }
 public string LoadCompanySnapshot(){if(!File.Exists(CompanyPath))return "";using var doc=JsonDocument.Parse(File.ReadAllText(CompanyPath));var x=doc.RootElement;return string.Join(Environment.NewLine,new[]{ReadString(x,"CompanyName"),ReadString(x,"OwnerName"),ReadString(x,"Street"),$"{ReadString(x,"PostalCode")} {ReadString(x,"City")}".Trim(),ReadString(x,"TaxNumber"),ReadString(x,"VatId"),ReadString(x,"Iban")}.Where(v=>!string.IsNullOrWhiteSpace(v)));}
 private static string ReadString(JsonElement x,string name)=>x.TryGetProperty(name,out var p)?p.GetString()??"":"";
 private static int? ReadNullableInt(JsonElement x,string name)=>x.TryGetProperty(name,out var p)&&p.ValueKind==JsonValueKind.Number?p.GetInt32():null;
 private static string ReadCustomerNumber(JsonElement x){var prefix=x.TryGetProperty("NumberPrefix",out var p)?p.GetString()??"KD-":"KD-";var digits=x.TryGetProperty("NumberDigits",out var d)?d.GetInt32():5;var n=x.TryGetProperty("SequenceNumber",out var s)?s.GetInt32():0;return prefix+n.ToString().PadLeft(digits,'0');}
 public void SaveDraft(InvoiceStore store,InvoiceDraft draft){if(draft.Status!="Entwurf")throw new InvalidOperationException("Abgeschlossene oder stornierte Rechnungen können nicht gewöhnlich geändert werden.");if(draft.DraftNumber==0)draft.DraftNumber=store.NextDraftNumber++;draft.UpdatedAt=DateTimeOffset.Now;Upsert(store,draft);SaveStore(store);}
 public string PreviewInvoiceNumber(InvoiceStore store,DateTime date)=>$"{store.InvoicePrefix}{(store.IncludeYear?date.Year+"-":"")}{store.NextInvoiceNumber.ToString().PadLeft(store.InvoiceDigits,'0')}";
 public void SaveNumbering(InvoiceStore store,string prefix,bool includeYear,int digits,int next){prefix=prefix.Trim();if(string.IsNullOrWhiteSpace(prefix))throw new ArgumentException("Das Präfix darf nicht leer sein.");if(digits<3||digits>9)throw new ArgumentOutOfRangeException(nameof(digits));if(next<1)throw new ArgumentOutOfRangeException(nameof(next));var candidate=$"{prefix}{(includeYear?DateTime.Today.Year+"-":"")}{next.ToString().PadLeft(digits,'0')}";if(store.Drafts.Any(x=>x.InvoiceNumber.Equals(candidate,StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Diese Rechnungsnummer ist bereits vergeben.");store.InvoicePrefix=prefix;store.IncludeYear=includeYear;store.InvoiceDigits=digits;store.NextInvoiceNumber=next;SaveStore(store);}
 public void FinalizeInvoice(InvoiceStore store,InvoiceDraft draft,string companySnapshot,string customerSnapshot)
 {
  if(draft.Status!="Entwurf")throw new InvalidOperationException("Nur Entwürfe können abgeschlossen werden.");
  while(true){var number=PreviewInvoiceNumber(store,draft.InvoiceDate);if(!store.Drafts.Any(x=>x.InvoiceNumber.Equals(number,StringComparison.OrdinalIgnoreCase))){draft.InvoiceNumber=number;break;}store.NextInvoiceNumber++;}
  if(draft.IsSmallBusinessSnapshot){foreach(var line in draft.Lines)line.VatRate=0m;if(string.IsNullOrWhiteSpace(draft.SmallBusinessNoticeSnapshot))throw new InvalidOperationException("Der Kleinunternehmer-Pflichttext fehlt.");}draft.CompanySnapshot=companySnapshot;draft.CustomerAddressSnapshot=customerSnapshot;draft.Status="Abgeschlossen";draft.FinalizedAt=DateTimeOffset.Now;draft.UpdatedAt=DateTimeOffset.Now;store.NextInvoiceNumber++;Upsert(store,draft);SaveStore(store);
 }
 public void CancelInvoice(InvoiceStore store,InvoiceDraft draft,string reason){if(draft.Status!="Abgeschlossen")throw new InvalidOperationException("Nur abgeschlossene Rechnungen können storniert werden.");if(string.IsNullOrWhiteSpace(reason))throw new ArgumentException("Bitte geben Sie einen Stornierungsgrund ein.");draft.Status="Storniert";draft.CancellationReason=reason.Trim();draft.CancelledAt=DateTimeOffset.Now;draft.UpdatedAt=DateTimeOffset.Now;Upsert(store,draft);SaveStore(store);}
 private static void Upsert(InvoiceStore store,InvoiceDraft draft){var i=store.Drafts.FindIndex(x=>x.Id==draft.Id);if(i>=0)store.Drafts[i]=draft;else store.Drafts.Add(draft);}
 private void SaveStore(InvoiceStore store){Directory.CreateDirectory(_folder);var temp=InvoicePath+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(store,Options));if(File.Exists(InvoicePath))File.Copy(InvoicePath,InvoiceBackupPath,true);File.Move(temp,InvoicePath,true);}
}
