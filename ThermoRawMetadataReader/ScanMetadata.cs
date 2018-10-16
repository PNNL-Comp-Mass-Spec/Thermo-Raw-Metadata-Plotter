using System.ComponentModel;

namespace ThermoRawMetadataReader
{
    public class ScanMetadata
    {
        [Description("Scan Number")]
        public int ScanNumber { get; set; }

        [Description("MS Level")]
        public int MSLevel { get; set; }

        [Description("Retention Time")]
        public double RetentionTime { get; set; }

        [Description("Ion Injection Time (ms)")]
        public double IonInjectionTime { get; set; }

        [Description("Base Peak Intensity")]
        public double BPI { get; set; }

        [Description("Total Ion Current")]
        public double TIC { get; set; }
    }
}
