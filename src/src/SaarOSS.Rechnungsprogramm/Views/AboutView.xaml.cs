using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class AboutView : UserControl
{
 private const string SupportUrl = "https://saaross.de";
 public AboutView()=>InitializeComponent();
 private void OpenSupport(object sender,RoutedEventArgs e)
 {
  try { Process.Start(new ProcessStartInfo(SupportUrl) { UseShellExecute = true }); }
  catch (Exception ex) { MessageBox.Show("Die Supportseite konnte nicht geöffnet werden.\n\n"+ex.Message,"Support",MessageBoxButton.OK,MessageBoxImage.Error); }
 }
}
