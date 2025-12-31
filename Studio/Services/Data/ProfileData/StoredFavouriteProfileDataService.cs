using Microsoft.Extensions.Options;
using Studio.Contracts.Services;
using Studio.Models;
using Studio.Models.Legacy;
using Studio.Services.Files;
using System.IO;

namespace Studio.Services.Data;

public class StoredFavouriteProfileDataService : FavouriteProfileDataService
{
    private readonly FileService _fileService;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public StoredFavouriteProfileDataService(FileService fileService, IAppPaths appPaths)
    {
        ProfileDirectory = appPaths.Favourites;

        _fileService = fileService;

        if (!Directory.Exists(ProfileDirectory))
            Directory.CreateDirectory(ProfileDirectory);
    }

    public override void SaveProfile(ProfileV2 profile)
    {
        string fileName = $"{profile.Battletag}.json";
        _fileService.Save(ProfileDirectory, fileName, profile);

        base.SaveProfile(profile);
    }

    public override ProfileV2 ReadProfile(BattleTagV2 battletag)
    {
        string fileName = $"{battletag}.json";
        var data = _fileService.Read<ProfileV2>(ProfileDirectory, fileName);
        return data;
    }

    public override void DeleteProfile(ProfileV2 profile)
    {
        string fileName = $"{profile.Battletag}.json";

        _fileService.Delete(ProfileDirectory, fileName);

        base.DeleteProfile(profile);
    }

    public override void LoadProfilesFromDisk()
    {
        foreach (string file in Directory.GetFiles(ProfileDirectory))
        {
            try
            {
                var profile = _fileService.Read<ProfileV2>(file);
                if (profile.CustomName == null) // TODO: Better detection for profile file schema
                {
                    var profileV1 = (ProfileV2)_fileService.Read<ProfileV1>(file);
                    SaveProfile(profileV1);
                    Profiles.Add(profileV1);
                }
                else
                {
                    Profiles.Add(profile);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
