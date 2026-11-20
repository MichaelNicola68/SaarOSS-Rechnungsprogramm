using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using SaarOSS.Rechnungsprogramm.Models;
namespace SaarOSS.Rechnungsprogramm.Services;
public sealed class DocumentDesignService
{
    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    readonly string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaarOSS", "Rechnungsprogramm", "Dokumentdesign");
    string ProfileFile => Path.Combine(_folder, "document-design.json");
    string BackupFile => Path.Combine(_folder, "document-design.backup.json");
    public string? LogoPath(DocumentDesignProfile p) => string.IsNullOrWhiteSpace(p.LogoFileName) ? null : Path.Combine(_folder, p.LogoFileName);
    public DocumentDesignProfile Load(){if(!File.Exists(ProfileFile))return new();try{return JsonSerializer.Deserialize<DocumentDesignProfile>(File.ReadAllText(ProfileFile),Options)??new();}catch(Exception ex){throw new InvalidDataException("Das Dokumentdesign konnte nicht gelesen werden und wurde nicht überschrieben.",ex);}}
    public void Save(DocumentDesignProfile p){Directory.CreateDirectory(_folder);p.UpdatedAt=DateTime.Now;var temp=ProfileFile+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(p,Options));if(File.Exists(ProfileFile))File.Copy(ProfileFile,BackupFile,true);File.Move(temp,ProfileFile,true);}
    public string ImportLogo(string source){if(!File.Exists(source))throw new FileNotFoundException("Die Logodatei wurde nicht gefunden.");var info=new FileInfo(source);if(info.Length>5*1024*1024)throw new InvalidDataException("Das Logo darf höchstens 5 MB groß sein.");BitmapDecoder decoder;using(var stream=File.OpenRead(source)){decoder=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);}if(decoder.Frames.Count!=1)throw new InvalidDataException("Animierte oder mehrseitige Bilddateien werden nicht unterstützt.");var frame=decoder.Frames[0];if(frame.PixelWidth<32||frame.PixelHeight<32||frame.PixelWidth>6000||frame.PixelHeight>6000)throw new InvalidDataException("Das Logo muss zwischen 32 und 6000 Pixel groß sein.");Directory.CreateDirectory(_folder);var output=Path.Combine(_folder,"unternehmen-logo.png");var temp=output+".tmp";var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(frame));using(var target=File.Create(temp)){encoder.Save(target);}File.Move(temp,output,true);return Path.GetFileName(output);}
    public (bool ShowLogo,string LogoPngBase64,string HeaderLine1,string HeaderLine2,string HeaderLine3,string HeaderLine4) CreateSnapshot(){var p=Load();var path=LogoPath(p);var logo=p.ShowLogo&&path is not null&&File.Exists(path)?Convert.ToBase64String(File.ReadAllBytes(path)):"";return(p.ShowLogo&&!string.IsNullOrWhiteSpace(logo),logo,p.HeaderLine1??"",p.HeaderLine2??"",p.HeaderLine3??"",p.HeaderLine4??"");}
    public void RemoveLogo(DocumentDesignProfile p){var path=LogoPath(p);if(path is not null&&File.Exists(path))File.Delete(path);p.LogoFileName="";Save(p);}
}
