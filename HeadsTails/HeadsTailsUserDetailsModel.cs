using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HeadsTailsPortableCode;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
#if (WINDOWS_PHONE)
using HeadsTails_WP8;
using Microsoft.Live.Controls;
#else
#endif

namespace HeadsTails
{
    public class HeadsTailsUserDetailsModel : BindableBase, IHeadsTailsUserDetailsModel
    {
        private MobileServiceUser mobileServiceLoggedInUser { get; set; }
        private LiveConnectClient liveConnectClient;
        private LiveOperationResult microsoftAccountLoggedInUser;
        public ObservableCollection<Leaderboard> groupLeaderboardResults { get; set; }
        private Leaderboard appUser;

        private string _userName = "";
        public string userName
        {
            get { return this._userName; }
            set { this.SetProperty(ref this._userName, value); }
        }

        private string _groupName = "";
        public string groupName
        {
            get { return this._groupName; }
            set { this.SetProperty(ref this._groupName, value); }
        }

        private int _highScore = 0;
        public int highScore
        {
            get { return this._highScore; }
            set { this.SetProperty(ref this._highScore, value); }
        }

        private string _currentLoginStatus = "";
        public string CurrentLoginStatus
        {
            get { return this._currentLoginStatus; }
            set { this.SetProperty(ref this._currentLoginStatus, value); }
        }

        private bool _currentlyLoggedIn = false;
        public bool CurrentlyLoggedIn
        {
            get { return this._currentlyLoggedIn; }
            set { this.SetProperty(ref this._currentlyLoggedIn, value); }
        }

        public HeadsTailsUserDetailsModel()
        {
            groupLeaderboardResults = new ObservableCollection<Leaderboard>();
        }

        public int GetHighScore()
        {
            return highScore;
        }

#if (WINDOWS_PHONE)
        public async Task Authenticate(LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                CurrentLoginStatus = "Connected to Microsoft Services";
                liveConnectClient = new LiveConnectClient(e.Session);
                microsoftAccountLoggedInUser = await liveConnectClient.GetAsync("me");

                CurrentLoginStatus = "Getting Microsoft Account Details";
                mobileServiceLoggedInUser =
                    await App.MobileService.LoginWithMicrosoftAccountAsync(e.Session.AuthenticationToken);

                CurrentLoginStatus = string.Format("Welcome {0}!", microsoftAccountLoggedInUser.Result["name"]);
                userName = microsoftAccountLoggedInUser.Result["name"].ToString();

                await Task.Delay(2000); // non-blocking Thread.Sleep for 2000ms (2 seconds)

                CurrentlyLoggedIn = true;
                CurrentLoginStatus = string.Empty;
                // this will then flip the bit that shows the different UI for logged in

                await UpdateUserOnRemoteService(); // and update on the remote service
                await UpdateLeaderBoard(); // do first pass on leaderboard update
            }
            else
            {
                CurrentLoginStatus = "Y U NO LOGIN?";
                CurrentlyLoggedIn = false;
            }

        }
#else
        public async Task Authenticate()
        {
            // For Microsoft ID to work in Windows 8, you need to associate the Windows 8 app
            // with a Windows 8 Store app (ie: create an app in the Store/reserve a name)
            // you do not need to publish the app to test

            var liveIdClient = new LiveAuthClient("https://headstails.azure-mobile.net/");

            while (App.liveConnectSession == null)
            {
                // Force a logout to make it easier to test with multiple Microsoft Accounts
                if (liveIdClient.CanLogout)
                    liveIdClient.Logout();

                CurrentLoginStatus = "Connecting to Microsoft Services";

                // as per: http://stackoverflow.com/questions/17938828/obtaining-the-users-email-address-from-the-live-sdk

                var result = await liveIdClient.LoginAsync(new[] { "wl.emails" });
                if (result.Status == LiveConnectSessionStatus.Connected)
                {
                    CurrentLoginStatus = "Connected to Microsoft Services";
                    App.liveConnectSession = result.Session;
                    var client = new LiveConnectClient(result.Session);
                    microsoftAccountLoggedInUser = await client.GetAsync("me");

                    CurrentLoginStatus = "Getting Microsoft Account Details";
                    mobileServiceLoggedInUser = await App.MobileService.LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);
                    
                    var remoteLeaderBoard = App.MobileService.GetTable<Leaderboard>();
                    remoteLeaderBoard = App.MobileService.GetTable<Leaderboard>();

                    CurrentLoginStatus = string.Format("Welcome {0}!", microsoftAccountLoggedInUser.Result["name"]);
                    userName = microsoftAccountLoggedInUser.Result["name"].ToString();

                    await Task.Delay(2000); // non-blocking Thread.Sleep for 2000ms (2 seconds)

                    CurrentlyLoggedIn = true; // this will then flip the bit that shows the different UI for logged in
                }
                else
                {
                    CurrentLoginStatus = "Y U NO LOGIN?";
                    CurrentlyLoggedIn = false;
                }
            }
        }
#endif

        public async Task UpdateUserOnRemoteService()
        {
            if (_currentlyLoggedIn)
            {
                appUser = new Leaderboard
                          {
                              userName = this.userName,
                              groupName = this.groupName,
                              channelW8notifications = "",
                              
                          };
#if (WINDOWS_PHONE)
                appUser.channelWP8notifications = App.CurrentChannel.ChannelUri.ToString();
#else
                appUser.channelW8notifications = App.CurrentChannel.Uri;
                appUser.channelWP8notifications = "";
#endif
                var remoteLeaderBoard = App.MobileService.GetTable<Leaderboard>();
                await remoteLeaderBoard.InsertAsync(appUser);
                // do insert; server side will ensure no duplicates based on UserId; which is a value on Mobile services generated as we have authenticated to the service.
                // now "appUser" is set to the link to the remote service, with high scores etc
                this.highScore = appUser.highScore;
                this.groupName = appUser.groupName;
            }
        }

        public async Task UpdateHighScoreOnRemoteService(int newHighScore)
        {
            if (_currentlyLoggedIn)
            {
                this.highScore = newHighScore;
                if (newHighScore > appUser.highScore)
                {
                    appUser.highScore = newHighScore;
                    var remoteLeaderBoard = App.MobileService.GetTable<Leaderboard>();
                    await remoteLeaderBoard.UpdateAsync(appUser);
                }
            }
        }

        public async Task ChangeGroupChoice(string newGroupName)
        {
            if (_currentlyLoggedIn && !String.Equals(newGroupName, this.groupName, StringComparison.CurrentCultureIgnoreCase))
            {
                this.groupName = newGroupName.ToLower();
                appUser.groupName = newGroupName.ToLower();
                var remoteLeaderBoard = App.MobileService.GetTable<Leaderboard>();
                await remoteLeaderBoard.UpdateAsync(appUser);
            }
        }

        public async Task UpdateLeaderBoard()
        {
            if (_currentlyLoggedIn)
            {
                var remoteLeaderBoard = App.MobileService.GetTable<Leaderboard>();
                var remoteLb =
                    await
                    remoteLeaderBoard.Where(u => u.groupName == appUser.groupName)
                                     .OrderByDescending(u => u.highScore)
                                     .ToEnumerableAsync();
                var remoteids = new List<int>();
                foreach (var remoteleaderboardentry in remoteLb)
                {
                    remoteids.Add(remoteleaderboardentry.Id);
                    // add
                    if (groupLeaderboardResults.All(l => l.Id != remoteleaderboardentry.Id))
                    {
                        groupLeaderboardResults.Add(remoteleaderboardentry);
                        continue;
                    }
                    // change
                    if (
                        groupLeaderboardResults.Any(
                            l => l.Id == remoteleaderboardentry.Id && l.highScore != remoteleaderboardentry.highScore))
                    {
                        groupLeaderboardResults.Remove(
                            groupLeaderboardResults.First(
                                l =>
                                l.Id == remoteleaderboardentry.Id && l.highScore != remoteleaderboardentry.highScore));
                        groupLeaderboardResults.Add(remoteleaderboardentry);
                    }
                }

                //delete
                foreach (
                    var localleaderboardentry in
                        groupLeaderboardResults.Where(
                            localleaderboardentry => remoteids.All(r => r != localleaderboardentry.Id)))
                {
                    groupLeaderboardResults.Remove(localleaderboardentry);
                }
                OnPropertyChanged("groupLeaderboardResults");
            }
        }
    }
}
