using System.Collections;
using System.ComponentModel;
using System.IO;

using Microsoft.Extensions.Options;

using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Storage;

public class PersistAndRestoreService
{
    private readonly FileService _fileService;
    private readonly AppConfig _appConfig;
    private readonly IAppPaths _appPaths;

    public PersistAndRestoreService(FileService fileService, IAppPaths appPaths, IOptions<AppConfig> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
        _appPaths = appPaths;
    }

    public void PersistData()
    {
        if (App.Current.Properties != null)
        {
            var fileName = _appConfig.ConfigFileName;
            _fileService.Save(_appPaths.Root, fileName, App.Current.Properties);
        }
        
    }

    public void RestoreData()
    {
        var fileName = _appConfig.ConfigFileName;

        //if (!Directory.Exists(folderPath))
        //{
        //    Directory.CreateDirectory(folderPath);
        //}

        var properties = _fileService.Read<IDictionary>(_appPaths.Root, fileName);
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
