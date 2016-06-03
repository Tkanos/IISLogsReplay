using IISLogsReplay;
using System;
using System.Linq;

namespace IISLogsReplayConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Check Parameters
            if (!CheckParameters(args))
            {
                Help();
                return;
            }
            #endregion

            #region Get Values
            string path, server, fileType, headers, cookies, matchRequest, modifyPattern, replacement;
            char delimiter;
            int pathNb, queryStringNb, verbNb, beginLine, threadMax;
            int? userAgentNb;

            if(!GetValues(args, out path, out delimiter, out pathNb, out queryStringNb, out verbNb, out server, out fileType, out beginLine, out userAgentNb, out headers, out cookies, out matchRequest, out modifyPattern, out replacement, out threadMax))
            {
                Help();
                return;
            }
            #endregion

            var replayer = new Replayer(path, delimiter, pathNb, queryStringNb, verbNb, fileType, beginLine, userAgentNb.HasValue ? userAgentNb.Value : -1);
            replayer.Replay(server, headers, cookies, matchRequest, modifyPattern, replacement, threadMax);


        }

        static bool CheckParameters(string[] args)
        {
            if (args.Length < 6)
            {
                return false;
            }

            if (!args.Contains("-p") || !args.Contains("-d") || !args.Contains("-pn") || !args.Contains("-qsn") || !args.Contains("-vn") || !args.Contains("-s"))
            {
                return false;
            }

            return true;
        }

        static bool GetValues(string[] args, out string path, out char delimiter, out int pathNb, out int queryStringNb, out int verbNb, out string server, 
            out string fileType, out int beginLine, out int? userAgentNb, out string headers, out string cookies, out string matchRequest, out string modifyPattern, out string replacement, out int threadMax)
        {
            path = GetArgsValue(args, "-p");
            string delimiterTemp = GetArgsValue(args, "-d");
            delimiter = ' ';
            server = GetArgsValue(args, "-s");

            bool pn = int.TryParse(GetArgsValue(args, "-pn"), out pathNb);
            bool qsn = int.TryParse(GetArgsValue(args, "-qsn"), out queryStringNb);
            bool vn = int.TryParse(GetArgsValue(args, "-vn"), out verbNb);

            fileType = GetArgsValue(args, "-ft");
            headers = GetArgsValue(args, "-H");
            cookies = GetArgsValue(args, "-C");
            matchRequest = GetArgsValue(args, "-mr");
            modifyPattern = GetArgsValue(args, "-mp");
            replacement = GetArgsValue(args, "-r");

            int.TryParse(GetArgsValue(args, "-bl"), out beginLine);

            userAgentNb = null;
            int userAgentNbTemp = 0;
            if(int.TryParse(GetArgsValue(args, "-uan"), out userAgentNbTemp))
                userAgentNb = userAgentNbTemp;

            threadMax = 1;
            int threadMaxTemp = 0;
            if (int.TryParse(GetArgsValue(args, "-tm"), out threadMaxTemp))
                threadMax = threadMaxTemp > 1 ? threadMaxTemp : 1;

            if (string.IsNullOrEmpty(path) || delimiterTemp.Length != 1 || string.IsNullOrEmpty(server) || !pn || !qsn || !vn)
                return false;

            delimiter = delimiterTemp[0];

            return true;

        }
        static string GetArgsValue(string[] args, string pattern)
        {
            if(args.Contains(pattern))
            {
                int nb = Array.IndexOf(args, pattern);
                if (args.Length > nb + 1)
                    return args[nb + 1];
            }

            return null;
        }

        static void Help()
        {
            Console.WriteLine("IISLogsReplayConsole.exe -p path -d delimiter -pn pathNb -qsn queryStringNb -vn verbNr -s server [-ft fileType] [-bl beginLine] [-uan userAgentNb] [-H headers] [-C cookies] [-mr matchRequest] [-mp modifyPattern] [-r replacement] ");
            Console.WriteLine("\n\n");
            Console.WriteLine("Mandatory Parameters");
            Console.WriteLine("\n");
            Console.WriteLine("-p  \t : path where is located the iislogs file/directory");
            Console.WriteLine("-d  \t : delimiter");
            Console.WriteLine("-pn \t : int that locate on each line of iislog the uri-stem");
            Console.WriteLine("-qsn\t : int that locate on each line of iislog the uri-query");
            Console.WriteLine("-vn \t : int that locate on each line of iislog the method (verb)");
            Console.WriteLine("-s  \t : base uri address");

            Console.WriteLine("\n\n");
            Console.WriteLine("Optional Parameters");
            Console.WriteLine("\n");
            Console.WriteLine("-ft \t : if in -p you inform the directory path, ft is needed to inform the filetype (example csv)");
            Console.WriteLine("-bl \t : int that tell to the program in which line we begin");
            Console.WriteLine("-uan\t : int that locate on each line of iislog the user-agent");
            Console.WriteLine("-H  \t : to specify others (http)Headers");
            Console.WriteLine("-C  \t : to specify (http)Cookies");
            Console.WriteLine("-mr \t : to specify a regexp to execute only the request that match -mr");
            Console.WriteLine("-mp \t : to specify a regexp pattern for Replace somthing on path or queryString");
            Console.WriteLine("-r  \t : if you have specified -mp, -r is to specify by what you want to replace your -mp");
            Console.WriteLine("-tm \t : int representing the Thread max (in parallelization) you want to use, by default it's sequencial (1)");

            Console.WriteLine("\n\n");
            Console.WriteLine("Example");
            Console.WriteLine("\n");
            Console.WriteLine("IISLogsReplayConsole.exe -p \"D:\\IssLogs\\myIIslog.log\" -d ' ' -pn 3 -qsn 4 -vn 9 -s http://mybetaserver.com");
            Console.WriteLine("\n");
            Console.WriteLine("IISLogsReplayConsole.exe -p \"D:\\IssLogs\" -d ' ' -pn 3 -qsn 4 -vn 9 -s http://mybetaserver.com -ft .log -bl 5");
            Console.WriteLine("\n");
            Console.WriteLine("IISLogsReplayConsole.exe -p \"D:\\IssLogs\" -d ' ' -pn 3 -qsn 4 -vn 9 -s http://mybetaserver.com -ft .log -bl 5 -uan 2 -H \"HeaderName: HeaderValue\" -C \"cookieName1: CookieValue1; CookieName2:CookieValue2\" -mr \"v1\" -mp \"v1\" -r \"v2\" ");

        }
    }
}
