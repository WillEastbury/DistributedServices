namespace AppFrame.Interfaces;

public interface ILocalisation
{
    ITable<LocaleEntry> table { get; set; }
    Task<string> GetLocalisedStringAsync(string key, string languageCode);
    Task<IEnumerable<string>> GetSupportedLanguagesAsync();
}

public class LocaleEntry
{
    public string Key { get; set; }
    public string languageCode { get; set; }
    public string translation { get ; set; }

}