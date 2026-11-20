using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SaarOSS.Rechnungsprogramm.Models;

namespace SaarOSS.Rechnungsprogramm.Views;

public partial class InvoicesView
{
    private void OpenPaymentEntry(object sender,RoutedEventArgs e)
    {
        if (DraftList.SelectedItem is not InvoiceDraft invoice) { MessageBox.Show("Bitte wählen Sie zuerst eine Rechnung aus.","Zahlung erfassen",MessageBoxButton.OK,MessageBoxImage.Information); return; }
        if (invoice.Status == "Entwurf" || string.IsNullOrWhiteSpace(invoice.InvoiceNumber)) { MessageBox.Show("Zahlungen können erst für eine abgeschlossene Rechnung erfasst werden.","Zahlung erfassen",MessageBoxButton.OK,MessageBoxImage.Information); return; }
        if (invoice.Status == "Storniert") { MessageBox.Show("Für eine stornierte Rechnung kann keine Zahlung erfasst werden.","Zahlung erfassen",MessageBoxButton.OK,MessageBoxImage.Information); return; }
        var paymentService = new SaarOSS.Rechnungsprogramm.Services.PaymentService();
        var paymentStore = paymentService.Load();
        var paymentSummary = paymentService.CalculateSummary(paymentStore, invoice.Id, invoice.GrossTotal, invoice.InvoiceDate.AddDays(invoice.PaymentTermDays), false);
        if (paymentSummary.OpenAmount <= 0m)
        {
            MessageBox.Show("Diese Rechnung ist bereits vollständig bezahlt.", "Zahlung erfassen", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }        var window = new PaymentEntryWindow(invoice) { Owner = Window.GetWindow(this) }; window.ShowDialog(); LoadData();
    }
}
