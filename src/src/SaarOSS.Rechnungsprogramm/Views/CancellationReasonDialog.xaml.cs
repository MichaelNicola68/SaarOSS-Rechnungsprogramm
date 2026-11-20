using System.Windows;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class CancellationReasonDialog : Window
{
 public string Reason => ReasonBox.Text.Trim();
 public CancellationReasonDialog(){InitializeComponent();Loaded += (_,_) => ReasonBox.Focus();}
 private void CancelClick(object sender,RoutedEventArgs e){DialogResult=false;}
 private void ContinueClick(object sender,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(Reason)){MessageBox.Show("Bitte geben Sie einen Stornierungsgrund ein.","Stornierung",MessageBoxButton.OK,MessageBoxImage.Warning);ReasonBox.Focus();return;}DialogResult=true;}
}
