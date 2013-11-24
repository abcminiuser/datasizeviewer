using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FourWalledCubicle.DataSizeViewerExt
{
    internal class StorageLocation
    {
        private static StorageLocation mInstance;
        public static StorageLocation Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new StorageLocation();

                return mInstance;
            }
        }

        private readonly Dictionary<string, string> mStorageMap;

        private StorageLocation()
        {
            mStorageMap = new Dictionary<string, string>();

            mStorageMap.Add("t", "Text Segment");
            mStorageMap.Add("T", "Text Segment");
            mStorageMap.Add("W", "Text Segment");
            mStorageMap.Add("w", "Text Segment");
            mStorageMap.Add("d", "Data Segment");
            mStorageMap.Add("D", "Data Segment");
            mStorageMap.Add("b", "Data Segment");
            mStorageMap.Add("B", "Data Segment");
            mStorageMap.Add("r", "Data Segment");
            mStorageMap.Add("R", "Data Segment");
            mStorageMap.Add("g", "Data Segment");
            mStorageMap.Add("G", "Data Segment");
        }

        public string GetStorageDescription(string nmStorageID)
        {
            try
            {
                return mStorageMap[nmStorageID];
            }
            catch (KeyNotFoundException)
            {
                return String.Format(@"Unknown Location ""{0}""", nmStorageID);
            }
        }
    }

    static internal class StringUtil
    {
        public static string StringValueOrDefault(this string s1, string s2)
        {
            return string.IsNullOrWhiteSpace(s1) ? s2 : s1;
        }
    }

    public class SymbolSizeParser
    {
        private ObservableCollection<ItemSize> mSymbolSizes = new ObservableCollection<ItemSize>();

        private static readonly Regex symbolParserRegex = new Regex(
                @"^" + //                   Start of line
                @"(?<Size>[^\s]*)" + //     Match/capture symbol size
                @"\s*" + //                 Whitespace seperator
                @"(?<Storage>[^\s])" + //   Match/capture symbol storage
                @"\s*" + //                 Whitespace seperator
                @"(?<Name>[^\t]*)" + //     Match/capture symbol name
                @"\t" + //                  Tab seperator
                @"(?<Location>.*)" + //     Match/capture symbol location
                @"$", //                    End of line
                RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        
        public ObservableCollection<ItemSize> SymbolSizes
        {
            get { return mSymbolSizes; }
        }

        public void ClearSymbols()
        {
            mSymbolSizes.Clear();
        }

        public void ReloadSymbols(string elfPath, string toolchainNMPath, bool verifyLocations)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            Process p = new Process();

            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = String.Format(@"--line-numbers --size-sort --demangle --radix=d ""{0}""", elfPath);
            startInfo.FileName = toolchainNMPath;

            List<String> symbolOutput = new List<String>();

            p.StartInfo = startInfo;
            p.EnableRaisingEvents = true;
            p.OutputDataReceived += (sender, args) => symbolOutput.Add(args.Data);
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();

            mSymbolSizes.Clear();

            Dictionary<string, bool> symbolLocationDict = new Dictionary<string,bool>();
            
            foreach (string s in symbolOutput)
            {
                if (s == null)
                    continue;

                Match itemData = symbolParserRegex.Match(s);

                if (!itemData.Groups["Name"].Success || !itemData.Groups["Size"].Success || !itemData.Groups["Storage"].Success)
                    continue;

                string locationPath = itemData.Groups["Location"].Value.Substring(0, itemData.Groups["Location"].Value.LastIndexOf(':'));
                bool locationExists = true;

                if (verifyLocations)
                {
                    if (symbolLocationDict.ContainsKey(locationPath) == false)
                        symbolLocationDict.Add(locationPath, File.Exists(locationPath));

                    locationExists = symbolLocationDict[locationPath];
                }

                mSymbolSizes.Add(new ItemSize()
                {
                    Size = UInt32.Parse(itemData.Groups["Size"].Value),
                    Storage = StorageLocation.Instance.GetStorageDescription(itemData.Groups["Storage"].Value),
                    Name = itemData.Groups["Name"].Value,
                    Location = itemData.Groups["Location"].Value.StringValueOrDefault("Symbol Location Unspecified"),
                    LocationExists = locationExists
                });
            }
        }

        public Tuple<String, int?> GetSymbolLocationAndLine(ItemSize symbol)
        {
            if ((symbol.Location == null) || (symbol.Location.Contains(":") == false))
                return new Tuple<string,int?>("<Unknown>", null);

            string fileName = Path.GetFullPath(symbol.Location.Substring(0, symbol.Location.LastIndexOf(':')));

            int fileLine = -1;
            bool fileLineValid = int.TryParse(symbol.Location.Substring(symbol.Location.LastIndexOf(':') + 1), out fileLine);

            if (fileLineValid)
                return new Tuple<String, int?>(fileName, fileLine);
            else
                return new Tuple<String, int?>(fileName, null);
        }
    }
}
