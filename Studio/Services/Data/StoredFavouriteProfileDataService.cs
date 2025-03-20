using System.IO;
using Microsoft.Extensions.Options;
using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Data;

public class StoredFavouriteProfileDataService : FavouriteProfileDataService
{
    private readonly FileService _fileService;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public StoredFavouriteProfileDataService(FileService fileService, IOptions<AppConfig> appConfig)
    {
        ProfileDirectory = Path.Combine(_localAppData, appConfig.Value.ConfigurationFolderName, appConfig.Value.FavouritesPath);

        _fileService = fileService;

        if (!Directory.Exists(ProfileDirectory))
            Directory.CreateDirectory(ProfileDirectory);
    }

    public override void SaveProfile(UserData profile)
    {
        string fileName = $"{profile.Battletag}.json";
        _fileService.Save(ProfileDirectory, fileName, profile);

        base.SaveProfile(profile);
    }

    public override UserData ReadProfile(Battletag battletag)
    {
        string fileName = $"{battletag}.json";
        var data = _fileService.Read<UserData>(ProfileDirectory, fileName);
        return data;
    }

    public override void DeleteProfile(UserData profile)
    {
        string fileName = $"{profile.Battletag}.json";

        _fileService.Delete(ProfileDirectory, fileName);

        base.DeleteProfile(profile);
    }

    public override void LoadProfilesFromDisk()
    {
        foreach (var file in Directory.GetFiles(ProfileDirectory))
        {
            Profiles.Add(_fileService.Read<UserData>(file));
        }
    }

}
