using System.Collections;
using System.ComponentModel;
using System.IO;

using Microsoft.Extensions.Options;

using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Storage;

// TODO : split this into multiple services (one for userprofiles, favourites, general settings etc)
public class PersistAndRestoreService
{
    private readonly FileService _fileService;
    private readonly AppConfig _appConfig;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public PersistAndRestoreService(FileService fileService, IOptions<AppConfig> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
    }

    public void PersistData()
    {
        if (App.Current.Properties != null)
        {
            var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationFolderName);
            var fileName = _appConfig.ConfigFileName;
            _fileService.Save(folderPath, fileName, App.Current.Properties);
        }
        
    }

    public void RestoreData()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.ConfigurationFolderName);
        var fileName = _appConfig.ConfigFileName;
        var properties = _fileService.Read<IDictionary>(folderPath, fileName);
        if (properties != null)
        {
            foreach (DictionaryEntry property in properties)
            {
                App.Current.Properties.Add(property.Key, property.Value);
            }
        }
    }

    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (App.Current.Properties.Contains(key))
        {
            return (T)App.Current.Properties[key];
        }

        return defaultValue;
    }
    
    public void SetValue<TKey, TValue>(TKey key, TValue value)
    {
        if (App.Current.Properties.Contains(key))
        {
            App.Current.Properties[key] = value;
        }
        else
        {
            App.Current.Properties.Add(key, value);
        }
    }
}
