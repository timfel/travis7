using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;


namespace TravisCI.Models
{
    class TravisAPI
    {
        private static string buildsUrl = "http://api.travis-ci.org/repos/{0}/{1}/builds";
        private static string repositoryUrl = "http://api.travis-ci.org/repos/{0}/{1}";
        private static string buildUrl = "http://api.travis-ci.org/repos/{0}/{1}/builds/{2}";
        private static string jobsUrl = "http://api.travis-ci.org/jobs/{0}";

        public static void GetBuildDetails(string slug, long id, Action<object> success, Action<Exception> error)
        {
            GetBuildDetails(new Build { BuildId = id }, slug, success, error);
        }

        public static void GetBuildDetails(Build build, Action<object> success, Action<Exception> error)
        {
            GetBuildDetails(build, build.Repository.Slug, success, error);
        }

        public static void GetBuildDetails(Build build, string slug, Action<object> success, Action<Exception> error)
        {
            ApiRequest(String.Format(buildUrl, "{0}", "{1}", build.BuildId.ToString()), slug, success, error, (string json) =>
            {
                var result = JsonConvert.DeserializeObject(json);
                Dictionary<string, object> d;
                Dictionary<string, object> commit;
                // FIXME: UUUUUUGUGUGHGGH
                if (((Newtonsoft.Json.Linq.JObject)result).ToObject<Dictionary<string, object>>().ContainsKey("build"))
                {
                    d = Get<Dictionary<string, object>>(result, "build");
                    commit = Get<Dictionary<string, object>>(result, "commit");
                }
                else
                {
                    d = ((Newtonsoft.Json.Linq.JObject)result).ToObject<Dictionary<string, object>>();
                    commit = d;
                }

                if (build.EventType == null || build.Message == null)
                {
                    BasicInitializeBuildFrom(build, d, commit);
                }
                build.StartedAt = Get<DateTime>(d, "started_at", null);
                build.FinishedAt = Get<DateTime>(d, "finished_at", null);
                if (d != commit)
                {
                    build.Jobs = Get<Newtonsoft.Json.Linq.JArray>(d, "job_ids").ToObject<List<long>>();
                }
                else
                {
                    var matrix = Get<Newtonsoft.Json.Linq.JArray>(d, "matrix").ToObject<List<Dictionary<string, object>>>();
                    var jobs = new List<long>();
                    foreach (var job in matrix)
                    {
                        jobs.Add((long)job["id"]);
                    }
                    build.Jobs = jobs;
                }
                return build;
            });
        }

        private static void BasicInitializeBuildFrom(Build build, Dictionary<string, object> d, Dictionary<string, object> commit)
        {
            build.BuildId = Get<long>(d, "id");
            build.Number = Get<string>(d, "number");
            build.Duration = Get<long>(d, "duration", (long)-1);
            build.EventType = Get<string>(d, "event_type", ""); // FIXME: OLD STYLE
            if (build.EventType.Equals(""))
            {
                build.EventType = (bool)Get<bool>(d, "pull_request", false) ? "Pull Request" : "Push";
            }
            build.Message = Get<string>(commit, "message", "No Message");
            build.Result = ((Get<string>(d, "state", "pending")).Equals("finished")
                              ? Get<long>(d, "result", (long)-1) == 0 ? Build.SuccessResult : Build.ErrorResult
                              : Build.PendingResult);
            build.Commit = Get<string>(commit, "sha", "");
            if (build.Commit.Equals(""))
            {
                // FIXME: Jeez, Travis
                build.Commit = Get<string>(commit, "commit", "");
            }
            build.CommittedAt = Get<DateTime>(commit, "committed_at", null);
            build.Branch = Get<string>(commit, "branch");
            build.CompareUrl = Get<string>(commit, "compare_url", "");
            build.Committer = new Developer
            {
                EMail = Get<string>(commit, "committer_email", ""),
                Name = Get<string>(commit, "committer_name", "")
            };
            build.Author = new Developer
            {
                EMail = Get<string>(commit, "author_email", ""),
                Name = Get<string>(commit, "author_name", "")
            };
        }

        public static void GetRepository(string slug, Action<object> success, Action<Exception> error)
        {
            ApiRequest(repositoryUrl, slug, success, error, (string json) =>
            {
                var d = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                Dictionary<string, object> repo;
                // FIXME: UUUUUUGUGUGHGGH
                if (d.ContainsKey("repo"))
                {
                    repo = Get<Newtonsoft.Json.Linq.JObject>(d, "repo").ToObject<Dictionary<string, object>>();
                }
                else if (d.ContainsKey("slug") && d.ContainsKey("id"))
                {
                    repo = d;
                }
                else
                {
                    throw new Exception("Invalid JSON returned for " + slug);
                }
                return new Repository { Id = Get<long>(repo, "id"), Slug = Get<string>(repo, "slug") };
            });
        }

        public static void GetBuilds(Repository repo, Action<object> success, Action<Exception> error)
        {
            ApiRequest(buildsUrl, repo.Slug, success, error, (string json) =>
            {
                var builds = new List<Build>();
                var result = JsonConvert.DeserializeObject(json);
                List<Dictionary<string, object>> commits;
                List<Dictionary<string, object>> buildsArray;
                // FIXME: UUUUUUGUGUGHGGH
                if (typeof(Newtonsoft.Json.Linq.JArray) == result.GetType())
                {
                    commits = ((Newtonsoft.Json.Linq.JArray)result).ToObject<List<Dictionary<string, object>>>();
                    buildsArray = ((Newtonsoft.Json.Linq.JArray)result).ToObject<List<Dictionary<string, object>>>();
                }
                else
                {
                    commits = Get<List<Dictionary<string, object>>>(result, "commits");
                    buildsArray = Get<List<Dictionary<string, object>>>(result, "builds");
                }
                foreach(var b in buildsArray)
                {
                    var build = new Build { Repository = repo };
                    BasicInitializeBuildFrom(build, b, commits[buildsArray.IndexOf(b)]);
                    builds.Add(build);
                };
                return repo.Builds = builds;
            });
        }

        public static void GetJob(long jobId, Action<object> success, Action<Exception> error)
        {
            ApiRequest(String.Format(jobsUrl, jobId.ToString()), success, error, (string json) =>
            {
                var result = JsonConvert.DeserializeObject(json);
                Dictionary<string, object> job;
                if (((Newtonsoft.Json.Linq.JObject)result).ToObject<Dictionary<string, object>>().ContainsKey("job"))
                {
                    job = Get<Dictionary<string, object>>(result, "job");
                }
                else
                {
                    job = ((Newtonsoft.Json.Linq.JObject)result).ToObject<Dictionary<string, object>>();
                }
                var config = Get<Newtonsoft.Json.Linq.JObject>(job, "config").ToObject<Dictionary<string, object>>();
                return new Job
                        {
                            Id = Get<long>(job, "id"),
                            Result = (Get<string>(job, "state").Equals("finished")
                                      ? Get<long>(job, "result", (long)-1) == 0 ? Build.SuccessResult : Build.ErrorResult
                                      : Build.PendingResult),
                            Number = Get<string>(job, "number"),
                            Env = Get<string>(config, "env", "")
                        };
            });
        }

        private static void ApiRequest(string url, Action<object> success, Action<Exception> error, Func<string, object> converter)
        {
            WebClient wc = new WebClient();
            wc.UploadStringCompleted += (sender, e) =>
            {
                try
                {
                    var r = converter.Invoke(e.Result);
                    success.Invoke(r);
                }
                catch (Exception ex)
                {
                    error.Invoke(ex);
                }
            };
            wc.UploadStringAsync(new Uri(url), "");
        }

        private static void ApiRequest(string baseUrl, string slug, Action<object> success, Action<Exception> error, Func<string, object> converter)
        {
            var splitSlug = slug.Split('/');
            var owner = splitSlug[0];
            var name = splitSlug[1];
            var url = String.Format(baseUrl, owner, name);
            ApiRequest(url, success, error, converter);
        }

        private static T Get<T>(object d, string key)
        {
            T o = Get<T>(d, key, null);
            if (default(T) == null && o == null || default(T) != null && default(T).Equals(o))
            {
                throw new Exception("Cannot find required key " + key);
            }
            return o;
        }

        private static T Get<T>(object d, string key, object alternative)
        {
            if (typeof(Dictionary<string, object>) == d.GetType())
            {
                object o;
                ((Dictionary<string, object>)d).TryGetValue(key, out o);
                if (o == null)
                {
                    if (alternative != null)
                    {
                        return (T)alternative;
                    }
                    else
                    {
                        return default(T);
                    }
                }
                else
                {
                    return (T)o;
                }
            }
            else if (typeof(Newtonsoft.Json.Linq.JObject) == d.GetType())
            {
                Newtonsoft.Json.Linq.JToken o;
                ((Newtonsoft.Json.Linq.JObject)d).TryGetValue(key, out o);
                if (o == null)
                {
                    return (T)alternative;
                }
                else
                {
                    return o.ToObject<T>();
                }
            }
            else
            {
                throw new Exception("Unable to get " + key + " from " + d.GetType().ToString());
            }
        }
    }
}
