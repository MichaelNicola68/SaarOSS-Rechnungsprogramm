using System.IO;using System.Text.Json;using SaarOSS.Rechnungsprogramm.Models;
namespace SaarOSS.Rechnungsprogramm.Services;
public sealed class TaxProfileService
{
 static readonly JsonSerializerOptions Options=new(){WriteIndented=true};readonly string _folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SaarOSS","Rechnungsprogramm");string FileName=>Path.Combine(_folder,"tax-profile.json");string Backup=>Path.Combine(_folder,"tax-profile.backup.json");
 public TaxProfile Load(){if(!File.Exists(FileName))return new();try{return JsonSerializer.Deserialize<TaxProfile>(File.ReadAllText(FileName),Options)??new();}catch(Exception ex){throw new InvalidDataException("Das Steuerprofil konnte nicht gelesen werden und wurde nicht überschrieben.",ex);}}
 public void Save(TaxProfile profile){if(profile.TaxMode is not("Regelbesteuerung" or "Kleinunternehmer"))throw new ArgumentException("Unbekannte Besteuerungsart.");if(profile.IsSmallBusiness&&string.IsNullOrWhiteSpace(profile.SmallBusinessNotice))throw new ArgumentException("Der Pflichttext für Kleinunternehmer darf nicht leer sein.");Directory.CreateDirectory(_folder);var temp=FileName+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(profile,Options));if(File.Exists(FileName))File.Copy(FileName,Backup,true);File.Move(temp,FileName,true);}
}
