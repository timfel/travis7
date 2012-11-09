using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using TravisCI.Models;

namespace TravisCI
{
    public class BuildViewModel : AbstractViewModel
    {
        private Build build;

        public BuildViewModel(Build b)
        {
            Jobs = new ObservableCollection<JobViewModel>();
            UpdateFrom(b);
        }

        private string _number;
        public string Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (value != _number)
                {
                    _number = value;
                    NotifyPropertyChanged("Number");
                }
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

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }

        private string _shortMessage;
        public string ShortMessage
        {
            get
            {
                return _shortMessage;
            }
            set
            {
                if (value != _shortMessage)
                {
                    _shortMessage = value;
                    NotifyPropertyChanged("ShortMessage");
                }
            }
        }

        private string _eventType;
        public string EventType
        {
            get
            {
                return _eventType;
            }
            set
            {
                if (value != _eventType)
                {
                    _eventType = value;
                    NotifyPropertyChanged("EventType");
                }
            }
        }

        private Color _resultColor;
        public Color ResultColor
        {
            get
            {
                return _resultColor;
            }
            set
            {
                if (value != _resultColor)
                {
                    _resultColor = value;
                    NotifyPropertyChanged("ResultColor");
                }
            }
        }

        private Color GetResultColor(string result)
        {
            if (result.Equals(Models.Build.SuccessResult))
            {
                return Colors.Green;
            }
            else if (result.Equals(Models.Build.ErrorResult))
            {
                return  Colors.Red;
            }
            else
            {
                return  Colors.Yellow;
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                if (value != _author)
                {
                    _author = value;
                    NotifyPropertyChanged("Author");
                }
            }
        }

        private string _commit;
        public string Commit
        {
            get
            {
                return _commit;
            }
            set
            {
                if (value != _commit)
                {
                    _commit = value;
                    NotifyPropertyChanged("Commit");
                }
            }
        }

        private TimeSpan _buildDuration;
        public TimeSpan BuildDuration
        {
            get
            {
                return _buildDuration;
            }
            set
            {
                if (value != _buildDuration)
                {
                    _buildDuration = value;
                    NotifyPropertyChanged("BuildDuration");
                }
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                if (value != _date)
                {
                    _date = value;
                    NotifyPropertyChanged("Date");
                }
            }
        }

        private string _url;
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                if (value != _url)
                {
                    _url = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _branch;
        public string Branch
        {
            get
            {
                return _branch;
            }
            set
            {
                if (value != _branch)
                {
                    _branch = value;
                    NotifyPropertyChanged("Branch");
                }
            }
        }

        public ObservableCollection<JobViewModel> Jobs { get; private set; }

        internal void UpdateFrom(Build b)
        {
            this.build = b;
            this.Message = b.Message;
            this.ShortMessage = b.Message.Split('\n')[0];
            this.EventType = b.EventType;
            this.ResultColor = GetResultColor(b.Result);
            if (b.Repository != null)
            {
                this.Slug = b.Repository.Slug;
            }
            this.BuildId = b.BuildId;
            this.Number = b.Number;

            if (b.Committer != null && b.Author != null)
            {
                if (b.Committer.Name == b.Author.Name && b.Committer.EMail == b.Author.EMail)
                {
                    this.Author = String.Format("{0} ({1})", b.Committer.Name, b.Committer.EMail);
                }
                else
                {
                    this.Author = String.Format("{0} ({1}) (Authored by: {2} {3})", b.Committer.Name, b.Committer.EMail, b.Author.Name, b.Author.EMail);
                }
            }
            if (b.StartedAt != null && b.FinishedAt != null)
            {
                this.BuildDuration = b.StartedAt - b.FinishedAt;
            }
            this.Date = b.CommittedAt;
            this.Commit = b.Commit;
            this.Url = b.CompareUrl;
            b.LoadJobs(
                (job) => Dispatch(() => Jobs.Add(new JobViewModel(job))),
                (ex) => Dispatch(() => MessageBox.Show("There was an error getting the job matrix:\n" + ex.Message))
            );
        }

        public long BuildId { get; set; }
    }
}