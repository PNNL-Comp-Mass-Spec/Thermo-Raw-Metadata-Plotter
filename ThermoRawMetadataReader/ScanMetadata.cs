using System;
using System.ComponentModel;
using System.Reflection;

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

        /// <summary>
        /// Takes a PropertyInfo object and returns a Func that retrieves the value for the given property.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static Func<ScanMetadata, double> GetValueRetrieverFunction(PropertyInfo prop)
        {
            if (prop.Name.Equals(nameof(ScanNumber)))
            {
                return x => x.ScanNumber;
            }
            if (prop.Name.Equals(nameof(MSLevel)))
            {
                return x => x.MSLevel;
            }
            if (prop.Name.Equals(nameof(RetentionTime)))
            {
                return x => x.RetentionTime;
            }
            if (prop.Name.Equals(nameof(IonInjectionTime)))
            {
                return x => x.IonInjectionTime;
            }
            if (prop.Name.Equals(nameof(BPI)))
            {
                return x => x.BPI;
            }
            if (prop.Name.Equals(nameof(TIC)))
            {
                return x => x.TIC;
            }
            return x => Convert.ToDouble(prop.GetValue(x));
        }
    }
}
