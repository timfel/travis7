
namespace TravisCI.Models
{
    public class Job
    {
        public long Id { get; set; }
        public string Result { get; set; }
        public string Number { get; set; }
        public string Env { get; set; }
    }
}
