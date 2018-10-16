using System;
using System.Collections.Generic;

namespace ThermoRawMetadataReader
{
    public interface IMetadataReader : IDisposable
    {
        string RawFilePath { get; }

        List<ScanMetadata> ReadScanMetadata();
    }
}
