using System;using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Documents;
using Windows.System;
using OverwatchAccountLauncher.Classes;
using Studio.Core.Contracts.Services;
using Studio.Core.Models;
using System.Diagnostics;
using System.Xml.Linq;

namespace Studio.Core.Services
{
    public class AccountHandlerService
    {
        public string FilePath { get; set; }
        public string BattleNetConfigPath { get; set; }
        public string OverwatchLauncherPath { get; set; }
        public string OverwatchInstallDir { get; set; }

        public void LoadUserProfiles()
        {
            string[] accounts = Directory.GetFiles($"{FilePath}\\accounts\\user");
            foreach (string account in accounts)    
            {

            }
        }

        public void LoadFavouriteProfiles()
        {
            string[] accounts = Directory.GetFiles($"{FilePath}\\accounts\\favourite");
            foreach (string account in accounts)
            {

            }
        }

        private string FilePathFromProfileTag(string name, string tag)
        {
            return $"{FilePath}\\accounts\\{name}-{tag}";
        }

        private UserData GetAccountFromDataFile(string path)
        {
            string jsonString = JsonHandler.LoadJsonFromFile(path);
            UserData profile = JsonHandler.DeserializeUserDataJson(jsonString);
            return profile;
        }

        private void CreateUserProfileData(string username, string tag, string email)
        {
            UserData data = JsonHandler.CreateUserData(username, tag, email);
            string dataFilePath = Path.Join(FilePath, "Accounts", $"{username}-{tag}.json");


            Debug.WriteLine($"Saving data to {dataFilePath} !");
            JsonHandler.WriteUserDataToFile(data, dataFilePath);
        }

        private void CreateFavouritedProfileData(string battletag)
        {

        }

        //public IEnumerable<ProfileData> GetFavouriteProfiles()
        //{
            
        //}

        //public IEnumerable<ProfileData> GetUserProfiles()
        //{
        //    List<UserData> users = new();
        //    string[] accounts = Directory.GetFiles($"{FilePath}\\accounts\\user");
        //    foreach (string account in accounts)
        //    {
        //        users.Add(GetAccountFromDataFile(account));
        //    }

        //    return users;
        //}
    }
}
