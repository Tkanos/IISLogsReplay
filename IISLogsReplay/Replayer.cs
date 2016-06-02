
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            _pathNb = pathNb;
            _queryStringNb = queryStringNb;
            _verbNb = verbNb;

            //Optional
            _fileType = fileType;
            _beginLine = beginLine;
            _userAgentNb = userAgentNb;
        }
        #endregion


        public void Replay(string server, string headers = null, string cookies = null, string matchRequest = null, string modifyPattern = null, string replacement = null)
        {
            //Parse Files
            List<string[]> iislogs = FileParser.Parse(_path, _delimiter, _fileType, _beginLine);
            foreach(var line in iislogs )
            {
                string verb = line[_verbNb];
                if (verb == "GET")
                {
                    string path = line[_pathNb];
                    string queryString = line[_queryStringNb];

                    if (MatchRequest(path, queryString, matchRequest))
                    {
                        ChangeUri(path, queryString, modifyPattern, replacement);

                        string userAgent = _userAgentNb != -1 ? line[_userAgentNb] : null;

                        WebClientHelper.Get(server, path, queryString, userAgent, headers, cookies);
                    }
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

        private void ChangeUri(string path, string queryString, string modifyPattern = null, string replacement = null)
        {
            if (modifyPattern == null || replacement == null)
                return;

            var reg = new Regex(modifyPattern);

            path = reg.Replace(path, replacement);
            queryString = reg.Replace(queryString, replacement);
        }
    }
}
