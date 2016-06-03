
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

            if (threadMax <= 1) //Sequencial
            {
                foreach (var line in iislogs)
                {
                    RelayAction(line, server, headers, cookies, matchRequest, modifyPattern, replacement);
                }
            }
            else //Parallelized
            {
                Parallel.ForEach<string[]>(iislogs, new ParallelOptions { MaxDegreeOfParallelism = threadMax }, (line) =>
                {
                    RelayAction(line, server, headers, cookies, matchRequest, modifyPattern, replacement);
                });
            }
        }

        private void RelayAction(string[] line, string server, string headers = null, string cookies = null, string matchRequest = null, string modifyPattern = null, string replacement = null)
        {
            string verb = line[_verbNb];
            if (verb == "GET")
            {
                string path = line[_pathNb];
                string queryString = line[_queryStringNb];

                if (MatchRequest(path, queryString, matchRequest))
                {
                    ChangeUri(ref path, ref queryString, modifyPattern, replacement);

                    string userAgent = _userAgentNb != -1 ? line[_userAgentNb] : null;

                    WebClientHelper.Get(server, path, queryString, userAgent, headers, cookies);
                }
            }
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
