using System.Windows.Controls; namespace SaarOSS.Rechnungsprogramm.Views; public partial class DashboardView:UserControl { public DashboardView()=>InitializeComponent();  private void RefreshLiveDashboard(object sender,System.Windows.RoutedEventArgs e)
{
 try
 {
  var customerStore=new SaarOSS.Rechnungsprogramm.Services.CustomerService().Load();
  LiveCustomerCount.Text=customerStore.Customers.Count(x=>!x.IsArchived).ToString();
  var invoiceStore=new SaarOSS.Rechnungsprogramm.Services.InvoiceService().LoadInvoices();
  var paymentService=new SaarOSS.Rechnungsprogramm.Services.PaymentService();
  var paymentStore=paymentService.Load();
  var openCount=0;
  var overdueCount=0;
  var openAmount=0m;
  foreach(var invoice in invoiceStore.Drafts)
  {
   if(invoice.Status=="Entwurf"||string.IsNullOrWhiteSpace(invoice.InvoiceNumber))continue;
   var summary=paymentService.CalculateSummary(paymentStore,invoice.Id,invoice.GrossTotal,invoice.InvoiceDate.AddDays(invoice.PaymentTermDays),invoice.Status=="Storniert");
   if(summary.OpenAmount<=0m||summary.Status=="Storniert")continue;
   openCount++;
   openAmount+=summary.OpenAmount;
   if(summary.Status=="\u00DCberf\u00E4llig")overdueCount++;
  }
  LiveOpenInvoiceCount.Text=openCount.ToString();
  LiveOpenAmount.Text=$"{openAmount:N2} \u20AC offen";
  LiveOverdueCount.Text=overdueCount.ToString();
  LiveOverdueHint.Text=overdueCount==0?"Kein Handlungsbedarf":overdueCount==1?"1 Rechnung ist \u00FCberf\u00E4llig":$"{overdueCount} Rechnungen sind \u00FCberf\u00E4llig";
 }
 catch(System.Exception ex)
 {
  LiveOpenAmount.Text="Dashboarddaten konnten nicht geladen werden";
  System.Diagnostics.Debug.WriteLine(ex);
 }
}
 private static System.Collections.Generic.List<System.Text.Json.JsonElement> ReadArray(string file,string property)
 {
  var result=new System.Collections.Generic.List<System.Text.Json.JsonElement>();if(!System.IO.File.Exists(file))return result;
  using var doc=System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(file));var root=doc.RootElement;
  if(root.ValueKind==System.Text.Json.JsonValueKind.Array){foreach(var x in root.EnumerateArray())result.Add(x.Clone());return result;}
  if(root.ValueKind==System.Text.Json.JsonValueKind.Object&&root.TryGetProperty(property,out var array)&&array.ValueKind==System.Text.Json.JsonValueKind.Array)foreach(var x in array.EnumerateArray())result.Add(x.Clone());
  return result;
 }
 private static string ReadString(System.Text.Json.JsonElement item,string name)=>item.TryGetProperty(name,out var value)&&value.ValueKind==System.Text.Json.JsonValueKind.String?(value.GetString()??""):"";
 private static System.DateTime? ReadDate(System.Text.Json.JsonElement item,string name)=>item.TryGetProperty(name,out var value)&&value.ValueKind==System.Text.Json.JsonValueKind.String&&System.DateTime.TryParse(value.GetString(),out var date)?date:null; private void QuickNewInvoice(object sender,System.Windows.RoutedEventArgs e)=>InvokeMainWindow(sender,e,"ShowInvoices","ShowInvoicesView","OpenInvoices");
 private void QuickNewCustomer(object sender,System.Windows.RoutedEventArgs e)=>InvokeMainWindow(sender,e,"ShowCustomers","ShowCustomersView","OpenCustomers");
 private void QuickBackup(object sender,System.Windows.RoutedEventArgs e){new DataBackupWindow(){Owner=System.Windows.Window.GetWindow(this)}.ShowDialog();}
 private void QuickUpdateCenter(object sender,System.Windows.RoutedEventArgs e)=>InvokeMainWindow(sender,e,"OpenUpdateCenter","ShowUpdateCenter");
 private void InvokeMainWindow(object sender,System.Windows.RoutedEventArgs e,params string[] methodNames)
 {
  var host=System.Windows.Window.GetWindow(this);if(host is null){System.Windows.MessageBox.Show("Das Hauptfenster wurde nicht gefunden.");return;}
  var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
  foreach(var name in methodNames)
  {
   var method=host.GetType().GetMethod(name,flags);if(method is null)continue;
   var parameters=method.GetParameters();
   if(parameters.Length==0)method.Invoke(host,null);
   else if(parameters.Length==2)method.Invoke(host,new object[]{sender,e});
   else continue;
   return;
  }
  System.Windows.MessageBox.Show("Die gewünschte Schnellaktion ist in dieser Programmversion nicht verfügbar.","Schnellzugriff",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Information);
 }}
