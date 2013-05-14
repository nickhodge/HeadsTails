using System;
using System.Threading.Tasks;

namespace HeadsTailsPortableCode
{
    public class HeadsTailsViewModel : BindableBase
    {
        public HeadsTailsModel headsTailsModel { get; set; }
        public IHeadsTailsUserDetailsModel headsTailsUserDetailsModel { get; set; }

        private bool _editingGroup = false;
        public Boolean EditingGroup
        {
            get { return this._editingGroup; }
            set { this.SetProperty(ref this._editingGroup, value); }
        }

        // set to static with seed from ms datetime now
        private static readonly Random rand = new Random(DateTime.Now.Millisecond);

        public HeadsTailsViewModel(HeadsTailsModel passedInModel, IHeadsTailsUserDetailsModel passedInUserModel)
        {
            headsTailsModel = passedInModel;
            headsTailsUserDetailsModel = passedInUserModel;
        }

        public void PickWinner(Boolean clickChoice)
        {
            headsTailsModel.AddFlipResult(clickChoice, clickChoice == HeadsOrTails());
        }

        // will return true for Heads
        private Boolean HeadsOrTails()
        {
            var r = rand.Next(0, 2);
            return r == 1;
        }

        public void ToggleEditGroupChoice()
        {
            EditingGroup = !EditingGroup;
        }

        public async Task ChangeGroupChoice(string newGroupName)
        {
            EditingGroup = false;
            if (newGroupName != string.Empty)
            {
                await headsTailsUserDetailsModel.ChangeGroupChoice(newGroupName);
                await headsTailsUserDetailsModel.UpdateLeaderBoard();
            }
        }

    }
}
