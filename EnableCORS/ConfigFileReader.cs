using System;
using System.IO;

namespace Net.SByteStream.Utils.EnableCORS
{
    public class ConfigEntry : EventArgs
    {
        public string APIName { get; set; }
        public string Path { get; set; }
        public string AllowedOrigins { get; set; }
        public string AllowedHeaders { get; set; }
        public string AllowedMethods { get; set; }
    }

    public class ConfigException : Exception
    {
        public ConfigException() : base() { }
        public ConfigException(string message) : base(message) { }
    }

    public class ConfigFileReader
    {
        public EventHandler<ConfigEntry> EntryFoundEvent;

        public ConfigFileReader(string file)
        {
            m_fileName = file;
        }

        public void Read()
        {
            using (TextReader tw = File.OpenText(m_fileName))
            {
                ConfigEntry entry = null;
                ConfigEntry lastEntry = null;
                
                string apiName = null;
                string line = tw.ReadLine();

                while (line != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        if (line.StartsWith(PATH_IDENTIFIER) || line.StartsWith(ORIGIN_IDENTIFIER) ||
                            line.StartsWith(HEADERS_IDENTIFIER) || line.StartsWith(METHODS_IDENTIFIER) ||
                            line.StartsWith(API_NAME_IDENTIFIER))
                        {
                            string[] parts = line.Split(SEPARATORS);
                            string identifier = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (identifier == API_NAME_IDENTIFIER)
                            {
                                if (apiName != null)
                                    throw new ConfigException(string.Format("{0} has been specified more than once", API_NAME_IDENTIFIER));
                                apiName = value;
                            }
                            else if (identifier == PATH_IDENTIFIER)
                            {
                                if (entry != lastEntry)
                                {
                                    FireEntryFoundEvent(entry);
                                    lastEntry = entry;
                                }
                                entry = new ConfigEntry
                                {
                                    APIName = apiName,
                                    Path = value
                                };
                            }
                            else if (identifier == ORIGIN_IDENTIFIER)
                            {
                                if (entry != null) entry.AllowedOrigins = value;
                            }
                            else if (identifier == HEADERS_IDENTIFIER)
                            {
                                if (entry != null) entry.AllowedHeaders = value;
                            }
                            else if (identifier == METHODS_IDENTIFIER)
                            {
                                if (entry != null) entry.AllowedMethods = value;
                            }
                        }
                    }
                    
                    line = tw.ReadLine();
                }
                
                if (entry != null)
                {
                    FireEntryFoundEvent(entry);
                }
            }
        }

        private void FireEntryFoundEvent(ConfigEntry entry)
        {
            if (EntryFoundEvent != null) EntryFoundEvent(this, entry);
        }
        
        private string m_fileName;
        private const string API_NAME_IDENTIFIER = "api-name";
        private const string PATH_IDENTIFIER = "path";
        private const string ORIGIN_IDENTIFIER = "allowed-origins";
        private const string HEADERS_IDENTIFIER = "allowed-headers";
        private const string METHODS_IDENTIFIER = "allowed-methods";
        private readonly char[] SEPARATORS = new char[] { '=' };
    }
}
