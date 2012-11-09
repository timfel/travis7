using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TravisCI.Models;

namespace TravisCI
{
    public class JobViewModel : AbstractViewModel
    {
        private Job buildelement;

        public JobViewModel(Job job)
        {
            buildelement = job;
            ResultColor = GetResultColor(job.Result);
            Id = job.Number;
            Env = job.Env;
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
                return Colors.Red;
            }
            else
            {
                return Colors.Yellow;
            }
        }

        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _env;
        public string Env
        {
            get
            {
                return _env;
            }
            set
            {
                if (value != _env)
                {
                    _env = value;
                    NotifyPropertyChanged("Env");
                }
            }
        }
    }
}