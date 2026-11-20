using System.Collections.ObjectModel;using System.Windows;using SaarOSS.Rechnungsprogramm.Models;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class InvoiceFinalizationCheckWindow:Window
{
 public sealed class CheckItem{public string Symbol{get;set;}="";public string Color{get;set;}="";public string Title{get;set;}="";public string Detail{get;set;}="";public bool Blocks{get;set;}}
 public InvoiceFinalizationCheckWindow(InvoiceDraft invoice,string customerAddress,string plannedNumber)
 {
  InitializeComponent();NumberText.Text=plannedNumber;TotalText.Text=$"{invoice.GrossTotal:N2} €";var items=Build(invoice,customerAddress,plannedNumber);CheckList.ItemsSource=new ObservableCollection<CheckItem>(items);var errors=items.Count(x=>x.Blocks);SummaryText.Text=errors==0?"Alle verbindlichen Prüfungen wurden bestanden.":$"{errors} Fehler verhindern den Abschluss.";ConfirmButton.IsEnabled=errors==0;BlockingText.Text=errors==0?"":"Bitte die rot markierten Punkte korrigieren.";
 }
 static List<CheckItem> Build(InvoiceDraft x,string address,string number)
 {
  var a=new List<CheckItem>();void Add(bool ok,string title,string good,string bad,bool block=true)=>a.Add(new(){Symbol=ok?"✓":"✕",Color=ok?"#14532D":"#A11B1B",Title=title,Detail=ok?good:bad,Blocks=!ok&&block});
  Add(x.CustomerId!=Guid.Empty&&!string.IsNullOrWhiteSpace(x.CustomerNumber),"Kunde","Kunde und Kundennummer sind zugeordnet.","Bitte einen Kunden auswählen.");
  Add(!string.IsNullOrWhiteSpace(address)&&address.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).Length>=3,"Rechnungsanschrift","Eine Rechnungsanschrift ist vorhanden.","Die Kundenanschrift ist unvollständig.");
  Add(x.Lines.Count>0,"Positionen","Mindestens eine Rechnungsposition ist vorhanden.","Es ist keine Rechnungsposition vorhanden.");
  Add(x.Lines.All(v=>!string.IsNullOrWhiteSpace(v.Description)&&v.Quantity>0&&v.UnitPrice>=0&&v.DiscountPercent>=0&&v.DiscountPercent<=100&&v.VatRate>=0&&v.VatRate<=100),"Positionswerte","Bezeichnungen, Mengen, Preise, Rabatte und Steuersätze sind plausibel.","Mindestens eine Position enthält ungültige Werte.");
  Add(x.ServiceDate<=DateTime.Today.AddYears(10),"Leistungsdatum","Das Leistungsdatum ist plausibel.","Das Leistungsdatum liegt ungewöhnlich weit in der Zukunft.");
  Add(x.PaymentTermDays>=0&&x.PaymentTermDays<=365,"Zahlungsziel","Das Zahlungsziel liegt zwischen 0 und 365 Tagen.","Das Zahlungsziel ist ungültig.");
  Add(!string.IsNullOrWhiteSpace(x.TaxModeSnapshot),"Besteuerung","Der Besteuerungsstand ist im Entwurf gespeichert.","Der Besteuerungsstand fehlt.");
  Add(!x.IsSmallBusinessSnapshot||!string.IsNullOrWhiteSpace(x.SmallBusinessNoticeSnapshot),"Kleinunternehmerhinweis",x.IsSmallBusinessSnapshot?"Der Pflichttext ist vorhanden.":"Nicht erforderlich.","Der Kleinunternehmer-Pflichttext fehlt.");
  Add(!x.ShowBankDetailsSnapshot||(!string.IsNullOrWhiteSpace(x.AccountHolderSnapshot)&&!string.IsNullOrWhiteSpace(x.IbanSnapshot)),"Zahlungsdaten",x.ShowBankDetailsSnapshot?"Kontoinhaber und IBAN sind im Snapshot vorhanden.":"Zahlungsbereich ist für diese Rechnung deaktiviert.","Bei aktiviertem Zahlungsbereich fehlen Kontoinhaber oder IBAN.");
  Add(x.GrossTotal>=0,"Gesamtbetrag","Der Gesamtbetrag wurde berechnet.","Der Gesamtbetrag ist ungültig.");
  Add(!string.IsNullOrWhiteSpace(number),"Endgültige Rechnungsnummer","Die vorgesehene Nummer lautet "+number+".","Es konnte keine Rechnungsnummer ermittelt werden.");return a;
 }
 void Confirm(object sender,RoutedEventArgs e){DialogResult=true;Close();}void Cancel(object sender,RoutedEventArgs e){DialogResult=false;Close();}
}
