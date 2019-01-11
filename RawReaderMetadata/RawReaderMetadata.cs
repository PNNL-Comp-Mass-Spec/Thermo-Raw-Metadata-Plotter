using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoRawMetadataReader;

namespace RawReaderMetadata
{
    public class RawReaderMetadata : IMetadataReader
    {
        public string RawFilePath { get; }
        private IRawFileThreadManager rawReaderThreader = null;

        public RawReaderMetadata(string rawFilePath)
        {
            RawFilePath = rawFilePath;
            rawReaderThreader = RawFileReaderFactory.CreateThreadManager(rawFilePath);
        }

        public List<ScanMetadata> ReadScanMetadata()
        {
            var data = new List<ScanMetadata>();

            using (var rawReader = rawReaderThreader.CreateThreadAccessor())
            {
                if (!rawReader.SelectMsData())
                {
                    // dataset has no MS data. Return.
                    return data;
                }

                var header = rawReader.RunHeaderEx;
                var minScan = header.FirstSpectrum;
                var maxScan = header.LastSpectrum;
                var numScans = header.SpectraCount;

                data.Capacity = numScans + 5;

                for (var i = minScan; i <= maxScan; i++)
                {
                    var scan = new ScanMetadata();
                    scan.ScanNumber = i;
                    //scan.RetentionTime = rawReader.RetentionTimeFromScanNumber(i);

                    var scanStats = rawReader.GetScanStatsForScanNumber(i);
                    scan.RetentionTime = scanStats.StartTime;
                    scan.TIC = scanStats.TIC;
                    scan.BPI = scanStats.BasePeakIntensity;
                    //scan.MSLevel = scanStats.ScanEventNumber;

                    var scanEvents = rawReader.GetScanEventForScanNumber(i);
                    scan.MSLevel = (int)scanEvents.MSOrder;

                    var extra = rawReader.GetTrailerExtraInformation(i);
                    var converted = Enumerable.Range(0, extra.Length).Select(x => new KeyValuePair<string, string>(extra.Labels[x], extra.Values[x]))
                        .ToList();

                    scan.IonInjectionTime = double.Parse(converted.FirstOrDefault(x => x.Key.StartsWith("Ion Injection Time", StringComparison.OrdinalIgnoreCase)).Value ?? "0");

                    data.Add(scan);
                }
            }

            return data;
        }

        private void CloseReader()
        {
            if (rawReaderThreader != null)
            {
                rawReaderThreader.Dispose();
                rawReaderThreader = null;
            }
        }

        public void Dispose()
        {
            CloseReader();
            GC.SuppressFinalize(this);
        }

        ~RawReaderMetadata()
        {
            CloseReader();
        }
    }
}
