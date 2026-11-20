using System.IO;
using System.Windows;
using SaarOSS.Rechnungsprogramm.Models;
using SaarOSS.Rechnungsprogramm.Services;
namespace SaarOSS.Rechnungsprogramm.Views;
public partial class InvoicePreviewWindow : Window
{
 private readonly InvoiceDraft _invoice;
 public InvoicePreviewWindow(InvoiceDraft invoice){InitializeComponent();_invoice=invoice;TitleText.Text=$"Vorschau {_invoice.DisplayNumber}";Reader.Document=InvoiceDocumentService.BuildDocument(_invoice);}
 private void SavePdf(object sender,RoutedEventArgs e){var dialog=new Microsoft.Win32.SaveFileDialog{Title="Rechnung als PDF speichern",Filter="PDF-Datei (*.pdf)|*.pdf",FileName=$"{SafeName(_invoice.DisplayNumber)}.pdf",AddExtension=true,DefaultExt=".pdf"};if(dialog.ShowDialog(this)!=true)return;try{InvoiceDocumentService.SavePdf(_invoice,dialog.FileName);MessageBox.Show("Die PDF-Datei wurde erfolgreich gespeichert.","PDF gespeichert",MessageBoxButton.OK,MessageBoxImage.Information);}catch(Exception ex){MessageBox.Show("Die PDF-Datei konnte nicht erstellt werden.\n\n"+ex.Message,"PDF-Fehler",MessageBoxButton.OK,MessageBoxImage.Error);}}
 private void Print(object sender,RoutedEventArgs e){try{var dialog=new System.Windows.Controls.PrintDialog();if(dialog.ShowDialog()!=true)return;var document=InvoiceDocumentService.BuildDocument(_invoice);var paginator=((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator;paginator.PageSize=new Size(dialog.PrintableAreaWidth,dialog.PrintableAreaHeight);dialog.PrintDocument(paginator,$"Rechnung {_invoice.DisplayNumber}");}catch(Exception ex){MessageBox.Show("Die Rechnung konnte nicht gedruckt werden.\n\n"+ex.Message,"Druckfehler",MessageBoxButton.OK,MessageBoxImage.Error);}}
 private void CloseWindow(object sender,RoutedEventArgs e)=>Close();
 private static string SafeName(string value)=>string.Concat(value.Select(ch=>Path.GetInvalidFileNameChars().Contains(ch)?'_':ch));
 private void ZoomChanged(object sender,System.Windows.RoutedPropertyChangedEventArgs<double> e){if(Reader is null||ZoomText is null)return;Reader.Zoom=e.NewValue;ZoomText.Text=$"{e.NewValue:0} %";}
 private void ZoomIn(object sender,RoutedEventArgs e){ZoomSlider.Value=Math.Min(ZoomSlider.Maximum,ZoomSlider.Value+10);}
 private void ZoomOut(object sender,RoutedEventArgs e){ZoomSlider.Value=Math.Max(ZoomSlider.Minimum,ZoomSlider.Value-10);}
 private void ZoomReset(object sender,RoutedEventArgs e){ZoomSlider.Value=100;} private void FitPage(object sender,RoutedEventArgs e)
 {
  Dispatcher.BeginInvoke(new Action(()=>
  {
   // Der bewährte Ganzseitenwert zeigt eine vollständige A4-Seite ohne horizontales Abschneiden.
   ZoomSlider.Value=90;
  }),System.Windows.Threading.DispatcherPriority.Loaded);
 }}
