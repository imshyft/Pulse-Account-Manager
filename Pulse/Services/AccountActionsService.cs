using Studio.Contracts.Services;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Wpf.Ui.Input;

namespace Studio.Services
{
    public class AccountActionsService(BattleNetService battleNetService, IProfileFetchingService profileFetchingService, CustomSnackbarService snackbarService)
    {
        public async Task TryLaunchAccount(ProfileV2 profile, bool launchGame = false)
        {
            if (profile == null)
                return;

            if (profile.Email == null)
            {
                snackbarService.Show(true, s =>
                {
                    s.Appearance = ControlAppearance.Danger;
                    s.Title = "Couldn't Launch Account";
                    s.Content = "No email is associated with this account";
                    s.Icon = new SymbolIcon(SymbolRegular.ErrorCircle12);
                });
                return;
            }

            snackbarService.Show(true, s =>
            {
                s.Appearance = ControlAppearance.Success;
                s.Title = "Switching Account!";
                s.Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35);
            });

            battleNetService.OpenBattleNetWithAccount(profile.Email);
            if (!launchGame)
                return;

            bool result = await battleNetService.WaitForMainWindow();
            if (result)
            {
                snackbarService.Show(true, s =>
                {
                    s.Appearance = ControlAppearance.Success;
                    s.Title = "Launching Game!";
                    s.Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35);
                });

                battleNetService.OpenBattleNet(true);
            }
            else
            {
                snackbarService.Show(true, s =>
                {
                    s.Appearance = ControlAppearance.Danger;
                    s.Title = "Couldn't Launch Overwatch";
                    s.Content = "Timed out waiting for Battle.net to load";
                    s.Icon = new SymbolIcon(SymbolRegular.ErrorCircle12);
                });
            }
        }

        public async Task<ProfileV2> TrySyncAccount(ProfileV2 profile)
        {
            snackbarService.Show(true, s =>
            {
                s.Appearance = ControlAppearance.Info;
                s.Title = "Syncing Account";
                s.Content = "Please wait a moment.";
                s.Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16);
            });

            var result = await profileFetchingService.UpdateProfileAsync(profile);


            if (result.Outcome == ProfileFetchOutcome.Success)
            {
                snackbarService.Show(true, s =>
                {
                    s.Appearance = ControlAppearance.Success;
                    s.Title = "Synced Account";
                    s.Content = "Profile successfully synced";
                    s.Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16);
                });

                return result.Profile;
            }
            else
            {
                snackbarService.Show(true, s =>
                {
                    s.Appearance = ControlAppearance.Danger;
                    s.Title = "Could not fetch account";
                    s.Content = "Please try again later, and make sure it is a public profile";
                    s.Icon = new SymbolIcon(SymbolRegular.ErrorCircle24);
                });

                return null;
            }
        }
    }
}
