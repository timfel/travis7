using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Windows.Controls;

namespace TravisCI
{
    public partial class MainPage : TravisPhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((TravisCI.App)App.Current).CurrentView = this;
        }

        public override void Refresh_Click(object sender, EventArgs e)
        {
            App.ViewModel.LoadData();
        }

        public override void Add_Click(object sender, EventArgs e)
        {
            Guide.BeginShowKeyboardInput(Microsoft.Xna.Framework.PlayerIndex.One,
                "Please enter a project slug",
                "", "travis-ci/travis-ci", (res) =>
                {
                    string result = Guide.EndShowKeyboardInput(res);
                    if (result != null && !result.Equals(""))
                    {
                        App.ViewModel.AddRepository(result);
                    }
                }, null);
        }

        public override void Remove_Click(object sender, EventArgs e)
        {
            var item = Pivot.SelectedItem;
            if (item != null)
            {
                App.ViewModel.RemoveRepository(((RepositoryViewModel)item).Slug);
            }
        }

        public void SelectBuild(object sender, EventArgs e)
        {
            var addedItems = ((SelectionChangedEventArgs)e).AddedItems;
            if (addedItems != null && addedItems.Count > 0)
            {
                var item = (BuildViewModel)addedItems[0];
                NavigationService.Navigate(new Uri(String.Format("/BuildPage.xaml?slug={0}&id={1}", item.Slug, item.BuildId), UriKind.Relative));
            }
        }
    }
}