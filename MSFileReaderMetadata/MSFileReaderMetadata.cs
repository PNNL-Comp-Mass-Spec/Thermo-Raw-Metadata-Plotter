using System;
using System.Collections.Generic;
using System.Linq;
using ThermoRawFileReader;
using ThermoRawMetadataReader;

namespace MSFileReaderMetadata
{
    public class MSFileReaderMetadata : IMetadataReader
    {
        public string RawFilePath { get; }
        private XRawFileIO rawFile = null;

        public MSFileReaderMetadata(string rawFilePath)
        {
            RawFilePath = rawFilePath;

            if (!XRawFileIO.IsMSFileReaderInstalled(out var error))
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new Exception(error);
                }

                throw new Exception("Cannot create an instance of XRawFile. Check the MSFileReader installation.");
            }

            rawFile = new XRawFileIO(rawFilePath);
        }

        public List<ScanMetadata> ReadScanMetadata()
        {
            var data = new List<ScanMetadata>();

            var numScans = rawFile.GetNumScans();

            data.Capacity = numScans + 5;

            for (var i = 1; i <= numScans; i++)
            {
                var infoGood = rawFile.GetScanInfo(i, out clsScanInfo info);
                if (!infoGood)
                {
                    continue;
                }

                var scan = new ScanMetadata();
                scan.ScanNumber = i;
                scan.RetentionTime = info.RetentionTime;
                scan.TIC = info.TotalIonCurrent;
                scan.BPI = info.BasePeakIntensity;
                scan.MSLevel = info.MSLevel;

                scan.IonInjectionTime = double.Parse(info.ScanEvents.FirstOrDefault(x => x.Key.StartsWith("Ion Injection Time", StringComparison.OrdinalIgnoreCase)).Value ?? "0");

                data.Add(scan);
            }

            return data;
        }

        private void CloseReader()
        {
            if (rawFile != null)
            {
                rawFile.Dispose();
                rawFile = null;
            }
        }

        public void Dispose()
        {
            CloseReader();
            GC.SuppressFinalize(this);
        }

        ~MSFileReaderMetadata()
        {
            CloseReader();
        }
    }
}
