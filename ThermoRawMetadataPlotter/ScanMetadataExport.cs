using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using ThermoRawMetadataReader;

namespace ThermoRawMetadataPlotter
{
    public class ScanMetadataExport
    {
        public static void WriteScanMetadata(IEnumerable<ScanMetadata> data, string filePath)
        {
            using (var writer = new CsvWriter(new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))))
            {
                var config = writer.Configuration;
                config.HasHeaderRecord = true;
                config.Delimiter = filePath.ToLower().EndsWith("csv") ? "," : "\t";
                config.RegisterClassMap<ScanMetadataMap>();

                writer.WriteRecords(data);
            }
        }
    }

    public class ScanMetadataMap : ClassMap<ScanMetadata>
    {
        public ScanMetadataMap()
        {
            var index = 0;
            Map(x => x.ScanNumber).Name("Scan Number").Index(index++);
            Map(x => x.MSLevel).Name("MS Level").Index(index++);
            Map(x => x.RetentionTime).Name("Retention Time").Index(index++);
            Map(x => x.IonInjectionTime).Name("Ion Injection Time (ms)").Index(index++);
            Map(x => x.BPI).Name("BPI").Index(index++);
            Map(x => x.TIC).Name("TIC").Index(index++);
        }
    }
}
