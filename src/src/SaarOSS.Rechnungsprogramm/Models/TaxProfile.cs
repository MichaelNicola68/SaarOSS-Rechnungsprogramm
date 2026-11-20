namespace SaarOSS.Rechnungsprogramm.Models;
public sealed class TaxProfile
{
 public string TaxMode{get;set;}="Regelbesteuerung";
 public string SmallBusinessNotice{get;set;}="Steuerbefreiung für Kleinunternehmer gemäß § 19 UStG. Es wird keine Umsatzsteuer ausgewiesen.";
 public decimal PreviousYearInformationLimit{get;set;}=25000m;
 public decimal CurrentYearInformationLimit{get;set;}=100000m;
 public string LegalReference{get;set;}="§ 19 UStG";
 public DateTime LegalStatusCheckedAt{get;set;}=new DateTime(2026,11,17);
 public bool IsSmallBusiness=>TaxMode=="Kleinunternehmer";
}
