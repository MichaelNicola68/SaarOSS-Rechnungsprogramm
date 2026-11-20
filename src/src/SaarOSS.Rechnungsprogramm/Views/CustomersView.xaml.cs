using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using SaarOSS.Rechnungsprogramm.Models;
using SaarOSS.Rechnungsprogramm.Services;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class CustomersView : UserControl
{
 private readonly CustomerService _service = new();
 private CustomerStore _store = new();
 private Customer _current = NewModel();
 private bool _loading;
 public CustomersView(){InitializeComponent();PersonGrid.Visibility=Visibility.Collapsed;LoadStore();RefreshList();LoadForm(_current);}
 private static Customer NewModel()=>new(){Country="Deutschland"};
 private void LoadStore(){try{_store=_service.Load();}catch(Exception ex){MessageBox.Show(ex.Message,"Kundendaten",MessageBoxButton.OK,MessageBoxImage.Error);_store=new();}}
 private string TypeValue()=>((ComboBoxItem)TypeBox.SelectedItem).Content?.ToString()??"Unternehmen";
 private void RefreshList()
 {
  var q=SearchBox.Text.Trim(); var mode=FilterBox.SelectedIndex;
  var data=_store.Customers.Where(x=>mode==1 || (mode==0&&!x.IsArchived) || (mode==2&&x.IsArchived))
   .Where(x=>string.IsNullOrWhiteSpace(q) || ($"{x.CustomerNumber} {x.CompanyName} {x.FirstName} {x.LastName} {x.ContactPerson} {x.City} {x.Email}").Contains(q,StringComparison.CurrentCultureIgnoreCase))
   .OrderBy(x=>x.DisplayName).ToList();
  CustomerList.ItemsSource=data; CountText.Text=$"{data.Count} Treffer";
 }
 private void LoadForm(Customer x)
 {
  _loading=true;_current=x;TypeBox.SelectedIndex=x.CustomerType=="Privatperson"?1:0;CompanyBox.Text=x.CompanyName;FirstNameBox.Text=x.FirstName;LastNameBox.Text=x.LastName;ContactBox.Text=x.ContactPerson;StreetBox.Text=x.Street;ExtraBox.Text=x.AddressExtra;PostalBox.Text=x.PostalCode;CityBox.Text=x.City;CountryBox.Text=x.Country;EmailBox.Text=x.Email;PhoneBox.Text=x.Phone;MobileBox.Text=x.Mobile;VatBox.Text=x.VatId;PaymentBox.Text=x.PaymentTermDays?.ToString()??"";NotesBox.Text=x.Notes;NumberText.Text=x.SequenceNumber==0?"Nummer wird beim Speichern vergeben":x.CustomerNumber;FormTitle.Text=x.SequenceNumber==0?"Neuer Kunde":x.DisplayName;ArchiveButton.Visibility=x.SequenceNumber==0?Visibility.Collapsed:Visibility.Visible;ArchiveButton.Content=x.IsArchived?"Kunde reaktivieren":"Kunde archivieren";StatusText.Text="";UpdateTypeVisibility();_loading=false;
 }
 private void UpdateTypeVisibility(){var company=TypeValue()=="Unternehmen";CompanyLabel.Visibility=company?Visibility.Visible:Visibility.Collapsed;CompanyBox.Visibility=company?Visibility.Visible:Visibility.Collapsed;PersonGrid.Visibility=company?Visibility.Collapsed:Visibility.Visible;ContactBox.IsEnabled=company;}
 private void TypeChanged(object sender,SelectionChangedEventArgs e){if(!_loading&&IsLoaded)UpdateTypeVisibility();}
 private void SearchChanged(object sender,TextChangedEventArgs e){if(IsLoaded)RefreshList();}
 private void FilterChanged(object sender,SelectionChangedEventArgs e){if(IsLoaded)RefreshList();}
 private void NewCustomer(object sender,RoutedEventArgs e){CustomerList.SelectedItem=null;LoadForm(NewModel());}
 private void CustomerSelected(object sender,SelectionChangedEventArgs e){if(CustomerList.SelectedItem is Customer c)LoadForm(Clone(c));}
 private static Customer Clone(Customer x)=>new(){Id=x.Id,SequenceNumber=x.SequenceNumber,NumberPrefix=x.NumberPrefix,NumberDigits=x.NumberDigits,CustomerType=x.CustomerType,CompanyName=x.CompanyName,FirstName=x.FirstName,LastName=x.LastName,ContactPerson=x.ContactPerson,Street=x.Street,AddressExtra=x.AddressExtra,PostalCode=x.PostalCode,City=x.City,Country=x.Country,Email=x.Email,Phone=x.Phone,Mobile=x.Mobile,Website=x.Website,VatId=x.VatId,PaymentTermDays=x.PaymentTermDays,Notes=x.Notes,IsArchived=x.IsArchived,CreatedAt=x.CreatedAt,UpdatedAt=x.UpdatedAt};
 private bool ValidateInput(out int? payment)
 {
  payment=null;var type=TypeValue();
  if(type=="Unternehmen"&&string.IsNullOrWhiteSpace(CompanyBox.Text)){Warn("Bitte geben Sie den Firmennamen ein.",CompanyBox);return false;}
  if(type=="Privatperson"&&(string.IsNullOrWhiteSpace(FirstNameBox.Text)||string.IsNullOrWhiteSpace(LastNameBox.Text))){Warn("Bitte geben Sie Vorname und Nachname ein.",FirstNameBox);return false;}
  if(string.IsNullOrWhiteSpace(StreetBox.Text)||string.IsNullOrWhiteSpace(PostalBox.Text)||string.IsNullOrWhiteSpace(CityBox.Text)){Warn("Bitte füllen Sie Straße, PLZ und Ort aus.",StreetBox);return false;}
  if(!string.IsNullOrWhiteSpace(EmailBox.Text)){try{_ = new MailAddress(EmailBox.Text.Trim());}catch{Warn("Bitte geben Sie eine gültige E-Mail-Adresse ein.",EmailBox);return false;}}
  if(!string.IsNullOrWhiteSpace(PaymentBox.Text)){if(!int.TryParse(PaymentBox.Text,out var days)||days<0||days>365){Warn("Das Zahlungsziel muss zwischen 0 und 365 Tagen liegen.",PaymentBox);return false;}payment=days;}
  return true;
 }
 private static void Warn(string text,Control focus){MessageBox.Show(text,"Eingabe prüfen",MessageBoxButton.OK,MessageBoxImage.Warning);focus.Focus();}
 private void SaveCustomer(object sender,RoutedEventArgs e)
 {
  if(!ValidateInput(out var payment))return;SaveButton.IsEnabled=false;
  try{_current.CustomerType=TypeValue();_current.CompanyName=CompanyBox.Text.Trim();_current.FirstName=FirstNameBox.Text.Trim();_current.LastName=LastNameBox.Text.Trim();_current.ContactPerson=ContactBox.Text.Trim();_current.Street=StreetBox.Text.Trim();_current.AddressExtra=ExtraBox.Text.Trim();_current.PostalCode=PostalBox.Text.Trim();_current.City=CityBox.Text.Trim();_current.Country=string.IsNullOrWhiteSpace(CountryBox.Text)?"Deutschland":CountryBox.Text.Trim();_current.Email=EmailBox.Text.Trim();_current.Phone=PhoneBox.Text.Trim();_current.Mobile=MobileBox.Text.Trim();_current.VatId=VatBox.Text.Trim();_current.PaymentTermDays=payment;_current.Notes=NotesBox.Text.Trim();_service.SaveCustomer(_store,_current);StatusText.Text="✓ Kunde sicher gespeichert";RefreshList();LoadForm(Clone(_current));StatusText.Text="✓ Kunde sicher gespeichert";}
  catch(Exception ex){MessageBox.Show("Der Kunde konnte nicht gespeichert werden.\n\n"+ex.Message,"Speicherfehler",MessageBoxButton.OK,MessageBoxImage.Error);}finally{SaveButton.IsEnabled=true;}
 }
 private void DiscardChanges(object sender,RoutedEventArgs e){if(_current.SequenceNumber==0)LoadForm(NewModel());else{var saved=_store.Customers.FirstOrDefault(x=>x.Id==_current.Id);if(saved!=null)LoadForm(Clone(saved));}}
 private void ToggleArchive(object sender,RoutedEventArgs e){if(_current.SequenceNumber==0)return;var action=_current.IsArchived?"reaktivieren":"archivieren";if(MessageBox.Show($"Möchten Sie diesen Kunden wirklich {action}?","Kundenstatus",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;_current.IsArchived=!_current.IsArchived;try{_service.SaveCustomer(_store,_current);RefreshList();LoadForm(Clone(_current));StatusText.Text=_current.IsArchived?"✓ Kunde archiviert":"✓ Kunde reaktiviert";}catch(Exception ex){MessageBox.Show(ex.Message,"Speicherfehler",MessageBoxButton.OK,MessageBoxImage.Error);}}
 private void OpenEInvoiceData(object s,RoutedEventArgs e){new EInvoiceCustomerWindow(){Owner=Window.GetWindow(this)}.ShowDialog();}
}
