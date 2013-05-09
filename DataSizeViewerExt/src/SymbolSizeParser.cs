using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

    class SymbolSizeParser
    {
        private ObservableCollection<ItemSize> mSymbolSizes = new ObservableCollection<ItemSize>();

        private static readonly Regex symbolParserRegex = new Regex(
                @"^" + //                   Start of line
                @"(?<Size>[^\W]*)" + //     Match/capture symbol size
                @"\W+" + //                 Whitespace seperator
                @"(?<Storage>[^\W])" + //   Match/capture symbol storage
                @"\W+" + //                 Whitespace seperator
                @"(?<Name>[^\W]*)" + //     Match/capture symbol name
                @"\W+" + //                 Whitespace seperator
                @"(?<Location>.*)" + //     Match/capture symbol location
                @"$", //                    End of line
                RegexOptions.Compiled);
        
        public ObservableCollection<ItemSize> symbolSizes
        {
            get { return mSymbolSizes; }
        }

        public void ClearSymbols()
        {
            mSymbolSizes.Clear();
        }

        public void ReloadSymbols(string elfPath, string toolchainNMPath)
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
            
            foreach (string s in symbolOutput)
            {
                if (s == null)
                    continue;

                string[] itemData = symbolParserRegex.Split(s);

                if (itemData.Length == 1)
                    continue;

                mSymbolSizes.Add(new ItemSize()
                {
                    Size = UInt32.Parse(itemData[1]),
                    Storage = StorageLocation.Instance.GetStorageDescription(itemData[2]),
                    Name = itemData[3],
                    Location = itemData[4]
                });
            }
        }
    }
}
