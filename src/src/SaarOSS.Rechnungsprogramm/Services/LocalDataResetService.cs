using System.IO;using System.IO.Compression;using System.Security.Cryptography;using System.Text;using System.Text.Json;
namespace SaarOSS.Rechnungsprogramm.Services;
public sealed class LocalDataResetService
{
 public sealed record ResetResult(string BackupPath,string Sha256);
 readonly string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SaarOSS","Rechnungsprogramm");
 public ResetResult Reset(bool transactions,bool settings,bool numbering)
 {
  if(!transactions&&!settings&&!numbering)throw new InvalidOperationException("Es wurde kein Bereich ausgewählt.");
  Directory.CreateDirectory(folder);var backupRoot=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"SaarOSS-Sicherungen");Directory.CreateDirectory(backupRoot);var stamp=DateTime.Now.ToString("yyyy-MM-dd-HHmmss");var rootName="SaarOSS-vor-Datenreset-"+stamp;var zip=Path.Combine(backupRoot,rootName+".zip");
  var files=Directory.GetFiles(folder,"*",SearchOption.AllDirectories).Where(x=>!x.EndsWith(".tmp",StringComparison.OrdinalIgnoreCase)).ToList();
  using(var a=ZipFile.Open(zip,ZipArchiveMode.Create))foreach(var f in files)a.CreateEntryFromFile(f,rootName+"/Daten/"+Path.GetRelativePath(folder,f).Replace('\\','/'),CompressionLevel.Optimal);
  using(var a=ZipFile.OpenRead(zip)){foreach(var e in a.Entries){using var s=e.Open();s.CopyTo(Stream.Null);}}
  var sha=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zip)));
  if(transactions)ResetTransactions(numbering); if(settings)ResetSettings(); else if(numbering&&!transactions)ResetNumberingOnly(); Verify(transactions,settings);
  return new(zip,sha);
 }
 void ResetTransactions(bool numbering)
 {
  Write("e-invoice-customers.json",new{SchemaVersion=1,Customers=Array.Empty<object>()});Write("payments.json",new{SchemaVersion=1,Payments=Array.Empty<object>()});
  if(numbering){Write("customers.json",new{NumberPrefix="KD-",NumberDigits=5,NextSequenceNumber=211,NumberingMigratedTo211=true,Customers=Array.Empty<object>()});Write("invoice-drafts.json",new{NextDraftNumber=1,InvoicePrefix="RE-",IncludeYear=true,InvoiceDigits=5,NextInvoiceNumber=300,Drafts=Array.Empty<object>()});Write("offers.json",new{NextDraftNumber=1,Prefix="AG-",IncludeYear=true,Digits=5,NextNumber=300,Offers=Array.Empty<object>()});}
  else{ClearArrayPreserve("customers.json","Customers");ClearArrayPreserve("invoice-drafts.json","Drafts");ClearArrayPreserve("offers.json","Offers");}
 }
 void ResetSettings(){Write("company-settings.json",new{CompanyName="",OwnerName="",Street="",PostalCode="",City="",Email="",Phone="",TaxNumber="",VatId="",Iban="",PaymentTermDays=14,IsSmallBusiness=false});DeleteIfExists("document-design.json");DeleteIfExists(Path.Combine("Zahlungsbereich","payment-footer.json"));}
 void ResetNumberingOnly(){UpdateNumbers("customers.json",new Dictionary<string,object>{{"NumberPrefix","KD-"},{"NumberDigits",5},{"NextSequenceNumber",211},{"NumberingMigratedTo211",true}});UpdateNumbers("invoice-drafts.json",new Dictionary<string,object>{{"InvoicePrefix","RE-"},{"IncludeYear",true},{"InvoiceDigits",5},{"NextInvoiceNumber",300},{"NextDraftNumber",1}});UpdateNumbers("offers.json",new Dictionary<string,object>{{"Prefix","AG-"},{"IncludeYear",true},{"Digits",5},{"NextNumber",300},{"NextDraftNumber",1}});}
 void ClearArrayPreserve(string name,string property){var p=Path.Combine(folder,name);if(!File.Exists(p)){return;}var d=JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(File.ReadAllText(p))??new();var o=d.ToDictionary(x=>x.Key,x=>(object)x.Value);o[property]=Array.Empty<object>();Write(name,o);}
 void UpdateNumbers(string name,Dictionary<string,object> values){var p=Path.Combine(folder,name);var o=File.Exists(p)?(JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(File.ReadAllText(p))??new()).ToDictionary(x=>x.Key,x=>(object)x.Value):new Dictionary<string,object>();foreach(var x in values)o[x.Key]=x.Value;Write(name,o);}
 void Verify(bool t,bool s){if(t){CheckEmpty("customers.json","Customers");CheckEmpty("invoice-drafts.json","Drafts");CheckEmpty("offers.json","Offers");CheckEmpty("payments.json","Payments");CheckEmpty("e-invoice-customers.json","Customers");}if(s){using var d=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"company-settings.json")));if(d.RootElement.GetProperty("CompanyName").GetString()!="")throw new InvalidDataException("Unternehmensdatenprüfung fehlgeschlagen.");}}
 void CheckEmpty(string name,string prop){using var d=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,name)));if(d.RootElement.GetProperty(prop).GetArrayLength()!=0)throw new InvalidDataException(name+" wurde nicht vollständig geleert.");}
 void Write(string name,object value){var p=Path.Combine(folder,name);Directory.CreateDirectory(Path.GetDirectoryName(p)!);var tmp=p+".tmp";File.WriteAllText(tmp,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));File.Move(tmp,p,true);}void DeleteIfExists(string name){var p=Path.Combine(folder,name);if(File.Exists(p))File.Delete(p);}
}
