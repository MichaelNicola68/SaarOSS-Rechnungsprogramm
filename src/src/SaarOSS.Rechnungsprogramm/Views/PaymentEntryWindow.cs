using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SaarOSS.Rechnungsprogramm.Models;
using SaarOSS.Rechnungsprogramm.Services;

namespace SaarOSS.Rechnungsprogramm.Views;

public sealed class PaymentEntryWindow : Window
{
    private readonly InvoiceDraft _invoice;
    private readonly PaymentService _service = new();
    private readonly PaymentStore _store;
    private readonly DatePicker _date = new() { SelectedDate = DateTime.Today };
    private readonly TextBox _amount = new();
    private readonly ComboBox _method = new();
    private readonly TextBox _reference = new();
    private readonly TextBox _note = new() { AcceptsReturn = true, Height = 72, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _message = new() { Foreground = Brushes.DarkRed, TextWrapping = TextWrapping.Wrap };
    private readonly decimal _openAmount;
    private readonly Button _saveButton = MakeButton("Zahlung speichern", true);

    public PaymentEntryWindow(InvoiceDraft invoice)
    {
        _invoice = invoice;
        _store = _service.Load();
        var summary = _service.CalculateSummary(_store, invoice.Id, invoice.GrossTotal, invoice.InvoiceDate.AddDays(invoice.PaymentTermDays), invoice.Status == "Storniert");
        _openAmount = summary.OpenAmount;
        Title = "Zahlung erfassen"; Width = 620; Height = 680; MinWidth = 540; MinHeight = 620;
        WindowStartupLocation = WindowStartupLocation.CenterOwner; Background = new SolidColorBrush(Color.FromRgb(247,249,248));
        _method.Items.Add(PaymentMethods.BankTransfer); _method.Items.Add(PaymentMethods.Cash); _method.SelectedIndex = 0;
        _amount.Text = _openAmount.ToString("N2", CultureInfo.CurrentCulture);
        Content = Build(summary);
    }

    private UIElement Build(PaymentSummary summary)
    {
        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); root.RowDefinitions.Add(new RowDefinition()); root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var title = new StackPanel();
        title.Children.Add(new TextBlock { Text = "Zahlung erfassen", FontSize = 26, FontWeight = FontWeights.Bold });
        title.Children.Add(new TextBlock { Text = $"Rechnung {_invoice.DisplayNumber}", Margin = new Thickness(0,5,0,18), Foreground = new SolidColorBrush(Color.FromRgb(70,85,77)) });
        root.Children.Add(title);
        var card = new Border { Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(220,226,222)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(20) };
        Grid.SetRow(card,1); root.Children.Add(card);
        var form = new StackPanel(); card.Child = form;
        var sums = new Grid { Margin = new Thickness(0,0,0,18) }; sums.ColumnDefinitions.Add(new ColumnDefinition()); sums.ColumnDefinitions.Add(new ColumnDefinition()); sums.ColumnDefinitions.Add(new ColumnDefinition());
        AddSum(sums,0,"Rechnungsbetrag",summary.InvoiceTotal); AddSum(sums,1,"Bereits bezahlt",summary.PaidAmount); AddSum(sums,2,"Offener Betrag",summary.OpenAmount); form.Children.Add(sums);
        AddField(form,"Zahlungsdatum",_date); AddField(form,"Betrag",_amount); AddField(form,"Zahlungsart",_method); AddField(form,"Referenz / Verwendungszweck (optional)",_reference); AddField(form,"Interne Notiz (optional)",_note); form.Children.Add(_message);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,18,0,0) };
        var cancel = MakeButton("Abbrechen",false); cancel.Click += (_,_) => Close();
        var save = _saveButton; save.IsEnabled = _openAmount > 0m; save.Click += Save;
        actions.Children.Add(cancel); actions.Children.Add(save); Grid.SetRow(actions,2); root.Children.Add(actions);
        return root;
    }
    private static void AddSum(Grid grid,int col,string label,decimal value){var p=new StackPanel{Margin=new Thickness(col==0?0:8,0,0,0)};p.Children.Add(new TextBlock{Text=label,Foreground=Brushes.DimGray});p.Children.Add(new TextBlock{Text=$"{value:N2} €",FontSize=19,FontWeight=FontWeights.SemiBold});Grid.SetColumn(p,col);grid.Children.Add(p);}
    private static void AddField(Panel panel,string label,Control control){panel.Children.Add(new TextBlock{Text=label,Margin=new Thickness(0,8,0,5),FontWeight=FontWeights.SemiBold});control.MinHeight=36;control.Padding=new Thickness(8,5,8,5);panel.Children.Add(control);}
    private static Button MakeButton(string text,bool primary){return new Button{Content=text,Padding=new Thickness(18,10,18,10),Margin=new Thickness(8,0,0,0),Background=primary?new SolidColorBrush(Color.FromRgb(12,91,54)):Brushes.White,Foreground=primary?Brushes.White:Brushes.Black,BorderBrush=new SolidColorBrush(Color.FromRgb(190,201,195))};}
    private void Save(object sender,RoutedEventArgs e)
    {
        _message.Text="";
        if(!_date.SelectedDate.HasValue){_message.Text="Bitte wählen Sie ein Zahlungsdatum.";return;}
        if(!decimal.TryParse(_amount.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out var amount)||amount<=0){_message.Text="Bitte geben Sie einen gültigen Betrag größer als 0,00 € ein.";return;}
        if(amount>_openAmount){_message.Text="Der Betrag darf den offenen Rechnungsbetrag nicht überschreiten.";return;}
        var method=_method.SelectedItem as string;
        if(!PaymentMethods.IsAllowed(method)){_message.Text="Bitte wählen Sie Überweisung oder Barzahlung.";return;}
        try
        {
            _service.AddPayment(_store,new PaymentEntry{InvoiceId=_invoice.Id,PaymentDate=_date.SelectedDate.Value,Amount=amount,PaymentMethod=method!,Reference=_reference.Text.Trim(),InternalNote=_note.Text.Trim()});
            MessageBox.Show(this,"Die Zahlung wurde sicher gespeichert.","Zahlung gespeichert",MessageBoxButton.OK,MessageBoxImage.Information);DialogResult=true;
        }
        catch(Exception ex){_message.Text="Die Zahlung konnte nicht gespeichert werden: "+ex.Message;}
    }
}
