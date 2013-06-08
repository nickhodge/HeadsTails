using HeadsTails.Common;
using HeadsTailsPortableCode;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HeadsTails
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        public HeadsTailsModel headsTailsModel;
        public HeadsTailsUserDetailsModel headsTailsUserDetails;
        public HeadsTailsViewModel headsTailsViewModel;

        public MainPage()
        {
            this.InitializeComponent();
            headsTailsUserDetails = new HeadsTailsUserDetailsModel();
            headsTailsModel = new HeadsTailsModel(headsTailsUserDetails);
            headsTailsViewModel = new HeadsTailsViewModel(headsTailsModel, headsTailsUserDetails);
            DataContext = headsTailsViewModel;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Start the "loop"
            await headsTailsUserDetails.Authenticate(); // this will loop until the user logs in. Maybe not pretty
            await headsTailsUserDetails.UpdateUserOnRemoteService(); // and update on the remote service
            await headsTailsModel.RefreshLeaderboard(); // and start the "leaderboard pump"
        }

        private void ToggleEditGroup(object sender, RoutedEventArgs e)
        {
            (DataContext as HeadsTailsViewModel).ToggleEditGroupChoice();
        }

        private void ChangeGroup(object sender, RoutedEventArgs e)
        {
            (DataContext as HeadsTailsViewModel).ChangeGroupChoice((sender as TextBox).Text);
        }

        private async void PickHeads(object sender, RoutedEventArgs e)
        {
            (DataContext as HeadsTailsViewModel).PickWinner(true); // heads is true
        }

        private async void PickTails(object sender, RoutedEventArgs e)
        {
            (DataContext as HeadsTailsViewModel).PickWinner(false); // tails is false
        }

    }
}
