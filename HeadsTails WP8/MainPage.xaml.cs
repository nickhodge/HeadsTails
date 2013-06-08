using System.Windows;
using System.Windows.Navigation;
using HeadsTails;
using HeadsTailsPortableCode;
using Microsoft.Phone.Controls;
using Microsoft.Live.Controls;


namespace HeadsTails_WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        private HeadsTailsViewModel _headsTailsViewModel = null;
        private HeadsTailsUserDetailsModel _headsTailsUserDetailsModel = null;
        private HeadsTailsModel _headsTailsModel = null;

        /// <summary>
        /// A static HeadsTailsModel used by the views to store data.
        /// </summary>
        /// <returns>The HeadsTailsModel object.</returns>
        public HeadsTailsUserDetailsModel headsTailsUserDetails
        {
            get
            {
                // Delay creation of the view model until necessary
                return _headsTailsUserDetailsModel ?? (_headsTailsUserDetailsModel = new HeadsTailsUserDetailsModel());
            }
        }

        /// <summary>
        /// A static HeadsTailsModel used by the views to store data.
        /// </summary>
        /// <returns>The HeadsTailsModel object.</returns>
        public HeadsTailsModel headsTailsModel
        {
            get
            {
                // Delay creation of the view model until necessary
                return _headsTailsModel ?? (_headsTailsModel = new HeadsTailsModel(headsTailsUserDetails));
            }
        }

        /// <summary>
        /// A static ViewModel used by the views to bind against
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public HeadsTailsViewModel headsTailsViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (_headsTailsViewModel == null)
                {
                    _headsTailsViewModel = new HeadsTailsViewModel(headsTailsModel, headsTailsUserDetails);
                }

                return _headsTailsViewModel;
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            DataContext = headsTailsViewModel;
        }

        // Load data for the ViewModel Items
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await headsTailsModel.RefreshLeaderboard();
        }
 
        private async void PickHeads(object sender, RoutedEventArgs e)
        {
            (DataContext as HeadsTailsViewModel).PickWinner(true); // heads is true
        }

        private async void PickTails(object sender, RoutedEventArgs e)
        {
            (DataContext as HeadsTailsViewModel).PickWinner(false); // tails is false
        }

        private async void btnSignin_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            await headsTailsUserDetails.Authenticate(e);
        }

    }
}