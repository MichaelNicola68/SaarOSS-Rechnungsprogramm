using System.Windows;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class LocalDataResetWindow : Window
{
 public LocalDataResetWindow(){InitializeComponent();}
 private void ConfirmationChanged(object sender,System.Windows.Controls.TextChangedEventArgs e)=>ResetButton.IsEnabled=ConfirmBox.Text=="RESET"&&(TransactionsBox.IsChecked==true||SettingsBox.IsChecked==true||NumberingBox.IsChecked==true);
 private void Cancel(object sender,RoutedEventArgs e)=>Close();
 private void Reset(object sender,RoutedEventArgs e)
 {
  if(ConfirmBox.Text!="RESET")return;
  if(MessageBox.Show(this,"Die ausgew\u00E4hlten lokalen Daten werden nach einer automatischen Sicherung dauerhaft zur\u00FCckgesetzt. Fortfahren?","Letzte Best\u00E4tigung",MessageBoxButton.YesNo,MessageBoxImage.Warning)!=MessageBoxResult.Yes)return;
  ResetButton.IsEnabled=false;
  try{var result=new Services.LocalDataResetService().Reset(TransactionsBox.IsChecked==true,SettingsBox.IsChecked==true,NumberingBox.IsChecked==true);StatusText.Text=$"\u2713 Zur\u00FCcksetzen erfolgreich.\nSicherung: {result.BackupPath}\nSHA-256: {result.Sha256}";MessageBox.Show(this,"Die ausgew\u00E4hlten Daten wurden erfolgreich gesichert, zur\u00FCckgesetzt und gepr\u00FCft. Bitte das Programm neu starten.","Zur\u00FCcksetzen abgeschlossen",MessageBoxButton.OK,MessageBoxImage.Information);}
  catch(Exception ex){StatusText.Text="FEHLER: "+ex.Message;MessageBox.Show(this,ex.Message,"Zur\u00FCcksetzen fehlgeschlagen",MessageBoxButton.OK,MessageBoxImage.Error);ResetButton.IsEnabled=true;}
 }
}