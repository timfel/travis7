using System.Collections.ObjectModel;
using System.Windows;
using TravisCI.Models;

namespace TravisCI
{
    public class RepositoryViewModel : AbstractViewModel
    {
        private Repository repository;
        public Repository Repository
        {
            get
            {
                return repository;
            }
            private set
            {
                repository = value;
            }
        }

        public RepositoryViewModel(Repository repository)
        {
            this.repository = repository;
            this.Slug = repository.Slug;
            this.Builds = new ObservableCollection<BuildViewModel>();
            App.ViewModel.ShowProgressBar();
            repository.GetBuilds((builds) =>
                {
                    foreach (var b in builds)
                    {
                        this.Builds.Add(new BuildViewModel(b));
                    }
                    App.ViewModel.HideProgressBar();
                },
                (ex) =>
                {
                    Dispatch(() =>
                    {
                        MessageBox.Show("There was an error getting the build data for " + repository.Slug + "\n" +
                                        "The error was: " + ex.Message);
                    });
                    App.ViewModel.HideProgressBar();
                });
        }

        public override string ToString()
        {
            if (Slug == null)
            {
                return base.ToString();
            }
            else
            {
                return Slug;
            }
        }

        private string _slug;
        public string Slug
        {
            get
            {
                return _slug;
            }
            set
            {
                if (value != _slug)
                {
                    _slug = value;
                    NotifyPropertyChanged("Slug");
                }
            }
        }

        public ObservableCollection<BuildViewModel> Builds { get; private set; }
    }
}