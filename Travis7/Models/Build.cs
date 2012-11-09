using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TravisCI.Models
{
    public class Build
    {
        public static string SuccessResult = "Success";
        public static string ErrorResult = "Failure";
        public static string PendingResult = "Pending";

        public List<long> Jobs { private get; set; }

        public Build()
        {
            Jobs = new List<long>();
        }

        public long BuildId { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
        public Repository Repository { get; set; }
        public string Number { get; set; }
        public long Duration { get; set; }

        public Developer Committer { get; set; }
        public Developer Author { get; set; }
        public string Commit { get; set; }
        public DateTime CommittedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public string CompareUrl { get; set; }
        public string Branch { get; set; }

        public void LoadDetails(Action<Build> callback, Action<Exception> error)
        {
            TravisAPI.GetBuildDetails(this, (o) => callback.Invoke((Build)o), error);
        }

        public void LoadJobs(Action<Job> callback, Action<Exception> error)
        {
            foreach(var job in Jobs)
            {
                TravisAPI.GetJob(job, (o) => callback.Invoke((Job)o), error);
            }
        }
    }
}
