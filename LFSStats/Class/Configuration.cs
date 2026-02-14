using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LFSStatistics
{
    class Configuration
    {
        internal enum ExportOptions { no, yes, ask }
        internal enum GraphOptions { no, dll, exe }
        private const bool ignoreCase = true;

        private const string _host = "host";
        private static readonly IPAddress defIPAddr = IPAddress.Loopback;
        private const string _port = "port";
        private const int defPort = 29999;
        public IPEndPoint IpEndPoint { get; private set; }
        public IPHostEntry HostInfo { get; private set; }

        private const string _adminPassword = "adminPassword";
        private const string defAdminPassword = "";
        private string adminPassword;
        public string AdminPassword { get { return adminPassword; } private set { adminPassword = value; } }

        private const string _tcpMode = "tcpMode";
        private const bool defTCPmode = true;
        private bool tcpMode;
        public bool TcpMode { get { return tcpMode; } private set { tcpMode = value; } }

        private const string _isLocal = "isLocal";
        private const bool defIsLocal = true;
        private bool isLocal;
        public bool IsLocal { get { return isLocal; } private set { isLocal = value; } }

        private const string _pracDir = "pracDir";
        private const string defPracDir = "results";
        private string pracDir;
        public string PracDir { get { return pracDir; } private set { pracDir = value; } }
        private const string _qualDir = "qualDir";
        private const string defQualDir = "results";
        private string qualDir;
        public string QualDir { get { return qualDir; } private set { qualDir = value; } }
        private const string _raceDir = "raceDir";
        private const string defRaceDir = "results";
        private string raceDir;
        public string RaceDir { get { return raceDir; } private set { raceDir = value; } }

        private const string _exportOnRaceSTart = "exportOnRaceSTart";
        private const ExportOptions defExportOnRaceSTart = ExportOptions.yes;
        private ExportOptions exportOnRaceSTart;
        public ExportOptions ExportOnRaceSTart { get { return exportOnRaceSTart; } private set { exportOnRaceSTart = value; } }

        private const string _askForFileNameOnRST = "askForFileNameOnRST";
        private const bool defAskForFileNameOnRST = false;
        private bool askForFileNameOnRST;
        public bool AskForFileNameOnRST { get { return askForFileNameOnRST; } private set { askForFileNameOnRST = value; } }

        private const string _exportOnSTAte = "exportOnSTAte";
        private const ExportOptions defExportOnSTAte = ExportOptions.no;
        private ExportOptions exportOnSTAte;
        public ExportOptions ExportOnSTAte { get { return exportOnSTAte; } private set { exportOnSTAte = value; } }

        private const string _askForFileNameOnSTA = "askForFileNameOnSTA";
        private const bool defAskForFileNameOnSTA = false;
        private bool askForFileNameOnSTA;
        public bool AskForFileNameOnSTA { get { return askForFileNameOnSTA; } private set { askForFileNameOnSTA = value; } }

        private const string _pubStatIDkey = "pubStatIDkey";
        private const string defPubStatIDkey = "";
        private string pubStatIDkey;
        public string PubStatIDkey { get { return pubStatIDkey; } private set { pubStatIDkey = value; } }

        public Configuration(string configFilePath)
        {
            // Even if LoadFromFile returns empty dictionary after an error, constructor should set all parameters to default.
            Dictionary<string, string> config = LoadFromFile(configFilePath);
            string tmp;
            int port;
            IPAddress ipAddr;

            // host
            if (!config.TryGetValue(_host, out tmp) || !IPAddress.TryParse(tmp, out ipAddr))
            {       // for the sake of uniformity TryParse is kept here despite
                try // GetHostEntry actually working with both host name and IP
                {
                    HostInfo = Dns.GetHostEntry(tmp);
                }
                catch { }
                if (HostInfo != null && HostInfo.AddressList.Length > 0)
                    ipAddr = HostInfo.AddressList[0];
                else
                {
                    ReportError(_host, defIPAddr, tmp);
                    ipAddr = defIPAddr;
                }
            }
            // port
            if (!config.TryGetValue(_port, out tmp) || !int.TryParse(tmp, out port))
            {
                ReportError(_port, defPort, tmp);
                port = defPort;
            }
            // creates IP endpoint and host info
            try
            {
                IpEndPoint = new IPEndPoint(ipAddr, port);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                LFSStats.ErrorWriteException("IP address " + ipAddr + " or port" + port + " in configuration file \""
                    + configFilePath + "\" is out of range.", ex);
                ReportError(_host, defIPAddr, ipAddr);
                ReportError(_port, defPort, port);

                IpEndPoint = new IPEndPoint(defIPAddr, defPort);
            }
            finally
            {
                HostInfo = Dns.GetHostEntry(IpEndPoint.Address);
            }
            // administrator password
            if (!config.TryGetValue(_adminPassword, out adminPassword)) adminPassword = defAdminPassword;
            // TCP mode
            if (!config.TryGetValue(_tcpMode, out tmp) || !bool.TryParse(tmp, out tcpMode))
            {
                ReportError(_tcpMode, defTCPmode, tmp);
                tcpMode = defTCPmode;
            }
            // isLocal
            if (!config.TryGetValue(_isLocal, out tmp) || !bool.TryParse(tmp, out isLocal))
            {
                ReportError(_isLocal, defIsLocal, tmp);
                isLocal = defIsLocal;
            }
            // practice directory
            if (!config.TryGetValue(_pracDir, out pracDir) || pracDir.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                ReportError(_pracDir, defPracDir, pracDir);
                pracDir = defPracDir;
            }
            // qualification directory
            if (!config.TryGetValue(_qualDir, out qualDir) || qualDir.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                ReportError(_qualDir, defQualDir, qualDir);
                qualDir = defQualDir;
            }
            // race directory
            if (!config.TryGetValue(_raceDir, out raceDir) || raceDir.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                ReportError(_raceDir, defRaceDir, raceDir);
                raceDir = defRaceDir;
            }
            // export on race start
            if (!config.TryGetValue(_exportOnRaceSTart, out tmp) || !Enum.TryParse(tmp, ignoreCase, out exportOnRaceSTart))
            {
                ReportError(_exportOnRaceSTart, defExportOnRaceSTart, tmp);
                exportOnRaceSTart = defExportOnRaceSTart;
            }
            // ask for file name on race start
            if (!config.TryGetValue(_askForFileNameOnRST, out tmp) || !bool.TryParse(tmp, out askForFileNameOnRST))
            {
                ReportError(_askForFileNameOnRST, defAskForFileNameOnRST, tmp);
                askForFileNameOnRST = defAskForFileNameOnRST;
            }
            // export on state
            if (!config.TryGetValue(_exportOnSTAte, out tmp) || !Enum.TryParse(tmp, ignoreCase, out exportOnSTAte))
            {
                ReportError(_exportOnSTAte, defExportOnSTAte, tmp);
                exportOnSTAte = defExportOnSTAte;
            }
            // ask for file name on state
            if (!config.TryGetValue(_askForFileNameOnSTA, out tmp) || !bool.TryParse(tmp, out askForFileNameOnSTA))
            {
                ReportError(_askForFileNameOnSTA, defAskForFileNameOnSTA, tmp);
                askForFileNameOnSTA = defAskForFileNameOnSTA;
            }
            // PubStat ID key
            if (!config.TryGetValue(_pubStatIDkey, out pubStatIDkey)) pubStatIDkey = defPubStatIDkey;
        }

        /// <summary>
        /// Loads configuration from file
        /// </summary>
        /// <param name="configFilePath">Path and name of config file.</param>
        /// <returns>Dictionary with configuration key value pairs</returns>
        private static Dictionary<string, string> LoadFromFile(string configFilePath)
        {
            Dictionary<string, string> config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (StreamReader sr = new StreamReader(configFilePath))
                {
                    int index, linecounter = 0;
                    string line;

                    while (true)
                    {
                        line = sr.ReadLine();
                        if (line == null) break;
                        linecounter++;

                        index = line.IndexOf('#');      // removes comments "a=2#comment" "#comment"
                        if (index >= 0) line = line.Remove(index);

                        line = line.Trim();
                        if (line.Length == 0) continue; // ignores empty lines

                        index = line.IndexOf('=');      // look for first assignment
                        if (index < 0)
                        {
                            LFSStats.ErrorWriteLine(string.Format("Corrupted line #{0} \"{1}\" in config file \"{2}\", can't find '='.",
                                linecounter, line, configFilePath));
                            continue;                   // ignores invalid lines
                        }
                        // trims the spaces when config contains "key = val" ==> "key " " val" ==> trim ==> "key" "val"
                        config[line.Substring(0, index).Trim()] = line.Substring(index + 1).Trim();
#if DEBUG
                        // writes key and value to debug listener  
                        Debug.WriteLine(string.Format("{0} = {1}", line.Substring(0, index).Trim(), line.Substring(index + 1).Trim()));
#endif
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                LFSStats.ErrorWriteException("Configuration file not found.", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                LFSStats.ErrorWriteException("Configuration file directory not found.", ex);
            }
            catch (IOException ex)
            {
                LFSStats.ErrorWriteException("Configuration file IO exception.", ex);
            }
            catch (Exception ex)
            {
                LFSStats.ErrorWriteException("Configuration file exception.", ex);
            }
            return config;
        }

        private static void ReportError(string parameter, object defValue, object value)
        {
            LFSStats.WarningWriteLines("Config param invalid: " + parameter + " = " + value,
                                       "Using default value:  " + parameter + " = " + defValue);
        }
    }
}