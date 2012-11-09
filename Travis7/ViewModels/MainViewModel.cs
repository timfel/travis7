using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using TravisCI.Models;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;


namespace TravisCI
{
    public class MainViewModel : AbstractViewModel
    {
        private static string RepositoriesKey = "repositories";
        private static string DefaultRepo1 = "travis-ci/travis-ci";
        private static string DefaultRepo2 = "travis-ci/travis-worker";
        private IsolatedStorageSettings settings { get; set; }

        public MainViewModel()
        {
            this.settings = IsolatedStorageSettings.ApplicationSettings;
            this.Items = new ObservableCollection<RepositoryViewModel>();
            this.ProgressBarVisible = Visibility.Collapsed;
        }

        private Visibility _progressBarVisible;
        private int ActivityCount;
        public Visibility ProgressBarVisible {
            get
            {
                return _progressBarVisible;
            }
            private set
            {
                if (value != _progressBarVisible)
                {
                    _progressBarVisible = value;
                    NotifyPropertyChanged("ProgressBarVisible");
                }
            }
        }

        public ObservableCollection<RepositoryViewModel> Items { get; private set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LoadData()
        {
            var repos = new List<string>();
            if (settings.Contains(RepositoriesKey))
            {
                settings.TryGetValue<List<string>>(RepositoriesKey, out repos);
            }
            else
            {
                repos.Add(DefaultRepo1);
                repos.Add(DefaultRepo2);
                settings.Add(RepositoriesKey, repos);
            }

            if (repos.Count > 0)
            {
                foreach (var r in repos)
                {
                    LoadRepo(r);
                }
            }
            this.IsDataLoaded = true;
        }

        private void LoadRepo(string r)
        {
            var repo = GetItemForSlug(r);
            if (repo != null)
            {
                ShowProgressBar();
                repo.Repository.LoadBuilds(
                    (_) => HideProgressBar(),
                    (ex) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Error retrieving repository " + r + ". Disabling.\n" +
                                "The error was: " + ex.Message);
                        });
                        HideProgressBar();
                    });
            }
            else
            {
                ShowProgressBar();
                TravisAPI.GetRepository(r, (rep) =>
                {
                    Dispatch(() =>
                        {
                            Items.Add(new RepositoryViewModel((Repository)rep));
                        });
                    HideProgressBar();
                },
                (ex) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Error retrieving repository " + r + ". Disabling.\n" +
                            "The error was: " + ex.Message);
                    });
                    RemoveRepository(r);
                    HideProgressBar();
                });
            }
        }

        private RepositoryViewModel GetItemForSlug(string slug)
        {
            foreach (var repo in Items)
            {
                if (repo.Slug.Equals(slug))
                {
                    return repo;
                }
            }
            return null;
        }

        public void ShowProgressBar()
        {
            if (++ActivityCount == 1)
            {
                Dispatch(() => ProgressBarVisible = Visibility.Visible);
            }
        }

        public void HideProgressBar()
        {
            if (--ActivityCount == 0)
            {
                Dispatch(() => ProgressBarVisible = Visibility.Collapsed);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddRepository(string slug)
        {
            var repos = new List<string>();
            if (settings.Contains(RepositoriesKey))
            {
                settings.TryGetValue<List<string>>(RepositoriesKey, out repos);
                if (!repos.Contains(slug))
                {
                    repos.Add(slug);
                    settings.Remove(RepositoriesKey);
                    settings.Add(RepositoriesKey, repos);
                    settings.Save();
                    LoadRepo(slug);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveRepository(string slug)
        {
            var repos = new List<string>();
            if (settings.Contains(RepositoriesKey))
            {
                settings.TryGetValue<List<string>>(RepositoriesKey, out repos);
                if (repos.Contains(slug))
                {
                    repos.Remove(slug);
                    settings.Remove(RepositoriesKey);
                    settings.Add(RepositoriesKey, repos);
                    settings.Save();
                }
            }
            var repo = GetItemForSlug(slug);
            if (repo != null)
            {
                Dispatch(() => Items.Remove(repo));
            }
        }
    }
}