using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HeadsTailsPortableCode
{
    public interface IHeadsTailsUserDetailsModel
    {
        ObservableCollection<Leaderboard> groupLeaderboardResults { get; set; }
        string CurrentLoginStatus { get; set; }
        int highScore { get; set; }
        string groupName { get; set; }
        bool CurrentlyLoggedIn { get; set; }

        Task ChangeGroupChoice(string newGroupName);
        Task UpdateLeaderBoard();
        Task UpdateHighScoreOnRemoteService(int CurrentMaxInARow);
        int GetHighScore();
    }
}
