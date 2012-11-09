using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace TravisCI
{
    public class AbstractViewModel : INotifyPropertyChanged
    {
        private Dispatcher _dispatcher;
        private Dispatcher Dispatcher {
            get
            {
                if (_dispatcher == null)
                {
                    _dispatcher = Deployment.Current.Dispatcher;
                }
                return _dispatcher;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Dispatch(Action a)
        {
            Dispatcher.BeginInvoke(a);
        }

        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
