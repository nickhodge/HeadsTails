using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HeadsTailsPortableCode
{
    public class FlipResult
    {
        public FlipResult(Boolean thisFlip)
        {
            this.When = DateTime.Now;
            this.Win = thisFlip;
        }

        public Boolean Win { get; set; }
        public DateTime When { get; set; }

        public String WhenTime { get { return When.ToString(); } }

        public override string ToString()
        {
            return Win ? "Win" : "Lose";
        }
    }

    public class HeadsTailsModel : BindableBase
    {
        private ObservableCollection<FlipResult> _userWinningTable { get; set; }
        public List<FlipResult> userWinningTableSorted
        {
            get
            {
                var r = from f in _userWinningTable
                        orderby f.When descending
                        select f;
                return r.ToList();
            }
        }

        private int _currentInARow = 0;

        private int _currentInMaxARow = 0;
        public int CurrentMaxInARow
        {
            get { return this._currentInMaxARow; }
            set { this.SetProperty(ref this._currentInMaxARow, value); }
        }

        private string _latestResult = "";
        public string LatestResult
        {
            get { return this._latestResult; }
            set { this.SetProperty(ref this._latestResult, value); }
        }

        private readonly IHeadsTailsUserDetailsModel headsTailsUserDetailsModel;

        public HeadsTailsModel(IHeadsTailsUserDetailsModel passedInheadsTailsUserDetailsModel)
        {
            headsTailsUserDetailsModel = passedInheadsTailsUserDetailsModel;
            _userWinningTable = new ObservableCollection<FlipResult>();
            // following thanks to http://stackoverflow.com/questions/12028660/windows-8-collectionviewsource-observablecollection-binding-not-updating
            _userWinningTable.CollectionChanged += delegate
            {
                base.OnPropertyChanged("userWinningTableSorted");
            };
        }

        public async void AddFlipResult(Boolean choice, Boolean isAWinner)
        {
            _userWinningTable.Add(new FlipResult(isAWinner));
            if (isAWinner)
            {
                _currentInARow++;
                if (_currentInARow > CurrentMaxInARow)
                    CurrentMaxInARow = _currentInARow;

                if (CurrentMaxInARow > headsTailsUserDetailsModel.GetHighScore())
                {
                    await headsTailsUserDetailsModel.UpdateHighScoreOnRemoteService(CurrentMaxInARow);
                    await headsTailsUserDetailsModel.UpdateLeaderBoard();
                }
                LatestResult = choice ? "Heads, You Win" : "Tails, You Win";
            }
            else
            {
                _currentInARow = 0;
                LatestResult = choice ? "Heads, You Lose" : "Tails, You Lose";
            }
        }

        public async Task<bool> RefreshLeaderboard()
        {
            while (true) // do forever in a forever loop
            {
                await headsTailsUserDetailsModel.UpdateLeaderBoard();
                await Task.Delay(1 * 1000 * 60); // await 1 minute before refreshing again
            }
            return true;
        } //this function will rarely return
    }

}
