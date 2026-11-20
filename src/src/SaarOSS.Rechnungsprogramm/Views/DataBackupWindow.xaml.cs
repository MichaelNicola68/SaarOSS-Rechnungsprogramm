using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace SaarOSS.Rechnungsprogramm.Views;

public partial class DataBackupWindow : Window
{
    private string _target;

    public DataBackupWindow()
    {
        InitializeComponent();
        _target = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SaarOSS-Sicherungen");
        TargetBox.Text = _target;
    }

    private void ChooseFolder(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Sicherungsordner auswählen",
            InitialDirectory = Directory.Exists(_target)
                ? _target
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) == true)
        {
            _target = dialog.FolderName;
            TargetBox.Text = _target;
        }
    }

    private void CreateBackup(object sender, RoutedEventArgs e)
    {
        BackupButton.IsEnabled = false;

        try
        {
            var source = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SaarOSS",
                "Rechnungsprogramm");

            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException(
                    "Der lokale Programmdatenordner wurde nicht gefunden: " + source);

            var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories)
                .Where(file => !file.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (files.Count == 0)
                throw new InvalidOperationException(
                    "Im Programmdatenordner wurden keine Dateien gefunden.");

            Directory.CreateDirectory(_target);

            var stamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            var rootName = $"SaarOSS-Rechnungsprogramm-Alpha8-Datensicherung-{stamp}";
            var zipPath = Path.Combine(_target, rootName + ".zip");
            var reportPath = Path.Combine(
                _target,
                $"SaarOSS-Rechnungsprogramm-Alpha8-Datensicherungsbericht-{stamp}.txt");

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var relative = Path.GetRelativePath(source, file).Replace('\\', '/');
                    archive.CreateEntryFromFile(
                        file,
                        rootName + "/Daten/" + relative,
                        CompressionLevel.Optimal);
                }
            }

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                if (archive.Entries.Count != files.Count)
                    throw new InvalidDataException(
                        "Die Anzahl der ZIP-Einträge stimmt nicht mit der Quelldateianzahl überein.");

                if (archive.Entries.Any(entry =>
                    !entry.FullName.StartsWith(rootName + "/", StringComparison.Ordinal)))
                    throw new InvalidDataException(
                        "Die ZIP-Struktur enthält einen ungültigen Eintrag.");

                foreach (var entry in archive.Entries)
                {
                    using var stream = entry.Open();
                    var buffer = new byte[81920];
                    while (stream.Read(buffer, 0, buffer.Length) > 0) { }
                }
            }

            var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(zipPath)));
            var report = new[]
            {
                "ERGEBNIS: ERFOLGREICH",
                $"Erstellt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Quelle: {source}",
                $"Dateien: {files.Count}",
                $"ZIP: {zipPath}",
                $"SHA-256: {hash}",
                "ZIP-Struktur: genau ein Hauptordner",
                "Lesetest aller ZIP-Einträge: erfolgreich"
            };

            File.WriteAllLines(reportPath, report, new UTF8Encoding(false));
            ResultText.Text = string.Join(Environment.NewLine, report);

            MessageBox.Show(
                this,
                "Die Datensicherung wurde erfolgreich erstellt und geprüft.",
                "Datensicherung",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ResultText.Text = "FEHLER: " + ex.Message;
            MessageBox.Show(
                this,
                ex.Message,
                "Datensicherung fehlgeschlagen",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            BackupButton.IsEnabled = true;
        }
    }

    private void OpenFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(_target);
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = _target,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Ordner öffnen",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseWindow(object sender, RoutedEventArgs e) => Close();
}
