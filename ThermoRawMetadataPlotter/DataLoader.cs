using System;
using System.Collections.Generic;
using ThermoRawMetadataReader;

namespace ThermoRawMetadataPlotting
{
    public class DataLoader
    {
        private static readonly IInstanceCreator ReaderCreator;
        static DataLoader()
        {
            if (Environment.Is64BitProcess)
            {
                ReaderCreator = new RawReaderMetadata.InstanceCreator();
            }
            else
            {
                ReaderCreator = new MSFileReaderMetadata.InstanceCreator();
            }
        }

        public static IMetadataReader GetReader(string rawFilePath)
        {
            return ReaderCreator.CreateInstance(rawFilePath);
        }

        public static List<ScanMetadata> GetMetadata(string rawFilePath)
        {
            using (var reader = GetReader(rawFilePath))
            {
                return reader.ReadScanMetadata();
            }
        }
    }
}
