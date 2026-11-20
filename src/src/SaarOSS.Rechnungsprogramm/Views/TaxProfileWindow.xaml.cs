using System.Windows;using SaarOSS.Rechnungsprogramm.Models;using SaarOSS.Rechnungsprogramm.Services;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class TaxProfileWindow:Window
{
 readonly TaxProfileService _service=new();TaxProfile _profile=new();public TaxProfileWindow(){InitializeComponent();try{_profile=_service.Load();RegularBox.IsChecked=!_profile.IsSmallBusiness;SmallBox.IsChecked=_profile.IsSmallBusiness;NoticeBox.Text=_profile.SmallBusinessNotice;ApplyMode();}catch(Exception ex){MessageBox.Show(this,ex.Message,"Besteuerung",MessageBoxButton.OK,MessageBoxImage.Error);}}
 void ModeChanged(object s,RoutedEventArgs e)=>ApplyMode();void ApplyMode(){if(NoticeBox is not null)NoticeBox.IsEnabled=SmallBox?.IsChecked==true;}
 void Save(object s,RoutedEventArgs e){var mode=SmallBox.IsChecked==true?"Kleinunternehmer":"Regelbesteuerung";if(mode!=_profile.TaxMode&&MessageBox.Show(this,"Die Besteuerungsart wird geändert. Bereits abgeschlossene Dokumente bleiben unverändert. Fortfahren?","Besteuerungsart ändern",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes)return;_profile.TaxMode=mode;_profile.SmallBusinessNotice=NoticeBox.Text.Trim();try{_service.Save(_profile);MessageBox.Show(this,"Die Besteuerungsart wurde gespeichert.","Besteuerung",MessageBoxButton.OK,MessageBoxImage.Information);DialogResult=true;}catch(Exception ex){MessageBox.Show(this,ex.Message,"Speichern fehlgeschlagen",MessageBoxButton.OK,MessageBoxImage.Error);}}
 void Cancel(object s,RoutedEventArgs e)=>Close();
}
