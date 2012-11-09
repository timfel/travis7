using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TravisCI.Models;

namespace TravisCI
{
    public partial class BuildPage : TravisPhoneApplicationPage
    {
        private Build build;
        public BuildPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string slug = "";
            string buildid = "";
            NavigationContext.QueryString.TryGetValue("slug", out slug);
            NavigationContext.QueryString.TryGetValue("id", out buildid);
            long id = Int64.Parse(buildid);

            TravisAPI.GetBuildDetails(slug, id, (build) =>
            {
                this.build = (Build)build;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                   this.DataContext = new BuildViewModel((Build)build);
                });
            },
            (ex) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Error retrieving build " + buildid + " in " + slug + "\n" +
                                    "The error was: " + ex.Message);
                });
                NavigationService.GoBack();
            });
        }

        public override void Refresh_Click(object sender, EventArgs e)
        {
            build.LoadDetails((_) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ((BuildViewModel)this.DataContext).UpdateFrom(build);
                    });
                },
                (ex) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("There was an error refreshing the build data\n" +
                                        "The error was: " + ex.Message);
                    });
                });
        }
    }
}