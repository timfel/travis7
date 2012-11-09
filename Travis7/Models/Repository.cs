using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TravisCI.Models
{
    public class Repository
    {
        public long Id { get; set; }
        public string Slug { get; set; }

        private List<Build> _builds;
        public List<Build> Builds {
            get {
                return _builds;
            }
            set { _builds = value; }
        }

        public void GetBuilds(Action<List<Build>> callback, Action<Exception> error)
        {
            if (_builds == null)
            {
                LoadBuilds(callback, error);
            }
            else
            {
                callback.Invoke(_builds);
            }
        }

        public void LoadBuilds(Action<List<Build>> callback, Action<Exception> error)
        {
            TravisAPI.GetBuilds(this, (obj) =>
            {
                Builds = (List<Build>)obj;
                callback.Invoke(Builds);
            },
            error);
        }
    }
}
