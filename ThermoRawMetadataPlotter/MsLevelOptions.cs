using System.ComponentModel;

namespace ThermoRawMetadataPlotter
{
    public enum MsLevelOptions
    {
        [Description("All MS Levels")]
        All = -1,

        [Description("All MSn")]
        MSn = 0,

        // ReSharper disable UnusedMember.Global

        [Description("MS1")]
        MS1 = 1,

        [Description("MS2")]
        MS2 = 2,

        [Description("MS3")]
        MS3 = 3,

        [Description("MS4")]
        MS4 = 4,

        [Description("MS5")]
        MS5 = 5,

        [Description("MS6")]
        MS6 = 6,

        [Description("MS7")]
        MS7 = 7,

        [Description("MS8")]
        MS8 = 8,

        [Description("MS9")]
        MS9 = 9,

        [Description("MS10")]
        MS10 = 10,

        // ReSharper restore UnusedMember.Global
    }
}
