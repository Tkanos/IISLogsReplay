using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Net.Http;

namespace IISLogsReplay
{
    public class Replayer
    {
        #region Properties
        private string _path;
        private char _delimiter;
        private string _fileType;
        private int _beginLine;

        private int _pathNb;
        private int _queryStringNb;
        private int _userAgentNb;
        private int _verbNb;
        #endregion

        #region Constructor
        public Replayer(string path, char delimiter, int pathNb, int queryStringNb, int verbNb, string fileType = null, int beginLine = 0, int userAgentNb = -1)
        {
            _path = path;
            _delimiter = delimiter;
            _pathNb = pathNb - 1;
            _queryStringNb = queryStringNb - 1;
            _verbNb = verbNb - 1;

            //Optional
            _fileType = fileType;
            _beginLine = beginLine;
            _userAgentNb = userAgentNb - 1;
        }
        #endregion

        public void Replay(string server, string headers = null, string cookies = null, string matchRequest = null, string modifyPattern = null, string replacement = null, int threadMax = 1)
        {
            //Parse Files
            List<string[]> iislogs = FileParser.Parse(_path, _delimiter, _fileType, _beginLine);

            iislogs = PrepareRequest(iislogs, matchRequest, modifyPattern, replacement);

            Console.WriteLine("{0} requets found", iislogs.Count);

            List<Tuple<HttpStatusCode, TimeSpan>> webResponses = new List<Tuple<HttpStatusCode, TimeSpan>>();

            var watch = new Stopwatch();
            watch.Start();
            int counter = 0;
            if (threadMax <= 1) //Sequencial
            {
                threadMax = 1;
                foreach (var line in iislogs)
                {
                    webResponses.Add(ReplayAction(line, server, headers, cookies));

                    if (counter % 100 == 0)
                        Console.WriteLine("{0} requests completed", counter);

                    counter++;
                }
            }
            else //Parallelized
            {
                object sync = new object();
                Parallel.ForEach<string[]>(iislogs, new ParallelOptions { MaxDegreeOfParallelism = threadMax }, (line) =>
                {
                    webResponses.Add(ReplayAction(line, server, headers, cookies));
                       
                    if (counter % 100 == 0)
                    {
                        lock (sync)
                        {
                            if (counter % 100 == 0)
                                Console.WriteLine("{0} requests completed", counter);
                        }
                    }

                    counter++;
                });
            }

            watch.Stop();

            var statusesCount = (from p in webResponses
                                 where p != null
                                 group p.Item1 by p.Item1 into g
                                 select new { Status = (int)g.Key, Count = g.ToList().Count }).ToList();

            double min = double.MaxValue;
            double med = 0;
            double max = 0;
            int i = 0;
            foreach(var webresponse in webResponses)
            {
                if(webresponse != null)
                {
                    if (min > webresponse.Item2.TotalMilliseconds)
                        min = webresponse.Item2.TotalMilliseconds;

                    if (max < webresponse.Item2.TotalMilliseconds)
                        max = webresponse.Item2.TotalMilliseconds;

                    med = med + webresponse.Item2.TotalMilliseconds;
                    i++;
                }
            }
            med = med / i;

            Console.WriteLine("\nReport :");
            Console.WriteLine("Total Time taken : {0:0.00} ms ({1:N0} ticks)", watch.ElapsedMilliseconds, watch.ElapsedTicks);
            Console.WriteLine("Completed Requets : {0:N0} iterations", iislogs.Count);
            Console.WriteLine("Requests per seconds : {0:N0}", (double)iislogs.Count/ watch.Elapsed.TotalSeconds);
            Console.WriteLine("");
            Console.WriteLine("Time per Request : {0:N0} ms", ((double)(watch.ElapsedMilliseconds / iislogs.Count)* threadMax));
            Console.WriteLine("Time per Request : {0:N0} ms", (double)(watch.ElapsedMilliseconds / iislogs.Count));
            Console.WriteLine("Min : {0:N0} ms", min);
            Console.WriteLine("Median : {0:N0} ms",med);
            Console.WriteLine("Max : {0:N0} ms", max);
            Console.WriteLine("");
            Console.WriteLine("Status : ");
            foreach (var status in statusesCount)
            {
                Console.WriteLine("\t{0} : {1}", status.Status, status.Count);
            }
        }

        private Tuple<HttpStatusCode, TimeSpan> ReplayAction(string[] line, string server, string headers = null, string cookies = null)
        {
            string verb = line[_verbNb];
            if (verb == "GET")
            {
                string path = line[_pathNb];
                string queryString = line[_queryStringNb];
                string userAgent = _userAgentNb != -1 ? line[_userAgentNb] : null;

                return WebClientHelper.Get(server, path, queryString, userAgent, headers, cookies);
            }

            return null;
        }

        private List<string[]> PrepareRequest(List<string[]> allLogs, string matchRequest = null, string modifyPattern = null, string replacement = null)
        {
            List<string[]> result = new List<string[]>();

            foreach(var line in allLogs)
            {
                string path = line[_pathNb];
                string queryString = line[_queryStringNb];

                if (MatchRequest(path, queryString, matchRequest))
                {
                    ChangeUri(ref path, ref queryString, modifyPattern, replacement);

                    line[_pathNb] = path;
                    line[_queryStringNb] = queryString;

                    result.Add(line);
                }
            }

            return result;
        }

        private bool MatchRequest(string path, string queryString, string matchRequest = null)
        {
            if (matchRequest == null)
                return true;

            var reg = new Regex(matchRequest);

            if(reg.IsMatch(path) || reg.IsMatch(queryString))
            {
                return true;
            }

            return false;
        }

        private void ChangeUri(ref string path, ref string queryString, string modifyPattern = null, string replacement = null)
        {
            if (modifyPattern == null || replacement == null)
                return;

            var reg = new Regex(modifyPattern);

            path = reg.Replace(path, replacement);
            queryString = reg.Replace(queryString, replacement);
        }
    }
}
