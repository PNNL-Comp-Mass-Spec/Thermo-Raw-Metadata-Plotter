using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using ThermoRawMetadataReader;

namespace ThermoRawMetadataPlotter
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string rawFilePath;
        private PropertyInfo yAxisProperty;
        private PropertyInfo xAxisProperty;
        private PlotModel dataPlot;
        private List<ScanMetadata> scanMetadata = new List<ScanMetadata>();
        private LinearAxis yAxis;
        private LinearAxis xAxis;
        private LinearColorAxis colorAxis;
        private ScatterSeries dataSeries;
        private readonly DescriptionConverter descConverter = new DescriptionConverter();
        private string status;
        private readonly Timer statusResetTimer;
        private readonly IReadOnlyReactiveList<MsLevelOptions> allMsLevelOptions;
        private IReadOnlyReactiveList<MsLevelOptions> msLevelOptionsList;
        private MsLevelOptions selectedMsLevel;

        public string RawFilePath
        {
            get => rawFilePath;
            set => this.RaiseAndSetIfChanged(ref rawFilePath, value);
        }

        public PropertyInfo YAxisProperty
        {
            get => yAxisProperty;
            set => this.RaiseAndSetIfChanged(ref yAxisProperty, value);
        }

        public PropertyInfo XAxisProperty
        {
            get => xAxisProperty;
            set => this.RaiseAndSetIfChanged(ref xAxisProperty, value);
        }

        public PlotModel DataPlot
        {
            get => dataPlot;
            private set => this.RaiseAndSetIfChanged(ref dataPlot, value);
        }

        public string Status
        {
            get => status;
            set
            {
                this.RaiseAndSetIfChanged(ref status, value);
                statusResetTimer.Change(TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
            }
        }

        public MsLevelOptions SelectedMSLevel
        {
            get => selectedMsLevel;
            set => this.RaiseAndSetIfChanged(ref selectedMsLevel, value);
        }

        public IReadOnlyReactiveList<MsLevelOptions> MsLevelOptionsList
        {
            get => msLevelOptionsList;
            set => this.RaiseAndSetIfChanged(ref msLevelOptionsList, value);
        }

        public List<PropertyInfo> ScanMetadataProperties { get; }

        public ReactiveCommand<Unit, Unit> OpenRawFileCommand { get; }
        public ReactiveCommand<Unit, Unit> BrowseForRawFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SwapAxisCommand { get; }
        public ReactiveCommand<Unit, Unit> ZoomFullCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportDataCommand { get; }

        public MainWindowViewModel() : this("")
        {
        }

        public MainWindowViewModel(string rawFilePath)
        {
            RawFilePath = rawFilePath;
            allMsLevelOptions = new ReactiveList<MsLevelOptions>(Enum.GetValues(typeof(MsLevelOptions)).Cast<MsLevelOptions>().OrderBy(x => (int)x));
            MsLevelOptionsList = allMsLevelOptions;

            OpenRawFileCommand = ReactiveCommand.Create(OpenRawFile);
            BrowseForRawFileCommand = ReactiveCommand.Create(BrowseForRawFile);
            SwapAxisCommand = ReactiveCommand.Create(SwapAxis);
            ZoomFullCommand = ReactiveCommand.Create(ZoomFull);
            ExportDataCommand = ReactiveCommand.Create(ExportData);

            ScanMetadataProperties = new List<PropertyInfo>(GetProperties());
            XAxisProperty = ScanMetadataProperties.First(x => x.Name.Equals(nameof(ScanMetadata.ScanNumber)));
            YAxisProperty = ScanMetadataProperties.First(x => x.Name.Equals(nameof(ScanMetadata.IonInjectionTime)));

            SelectedMSLevel = MsLevelOptions.All;

            SetupPlot();

            statusResetTimer = new Timer(StatusReset, this, Timeout.Infinite, Timeout.Infinite);

            if (!string.IsNullOrWhiteSpace(RawFilePath))
            {
                OpenRawFile();
            }

            this.WhenAnyValue(x => x.XAxisProperty, x => x.YAxisProperty, x => x.SelectedMSLevel).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe(x => ChangePlot());
        }

        private void ExportData()
        {
            if (scanMetadata.Count == 0)
            {
                return;
            }

            // get file save path, default to same directory...
            var dialog = new CommonSaveFileDialog()
            {
                Filters = { new CommonFileDialogFilter("Tab-separated", ".tsv,.txt"), new CommonFileDialogFilter("Comma-separated", ".csv"), },
                AlwaysAppendDefaultExtension = true,
                DefaultExtension = ".tsv"
            };

            if (!string.IsNullOrWhiteSpace(RawFilePath))
            {
                var dir = Path.GetDirectoryName(RawFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                {
                    dialog.InitialDirectory = dir;
                }

                dialog.DefaultFileName = Path.GetFileNameWithoutExtension(RawFilePath) + ".tsv";
            }

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var savePath = dialog.FileName;

                if (!string.IsNullOrWhiteSpace(savePath))
                {
                    try
                    {
                        ScanMetadataExport.WriteScanMetadata(scanMetadata, savePath);
                    }
                    catch (Exception ex)
                    {
                        Status = $"Error exporting data: {ex.Message}";
                    }
                }
            }
        }

        private void StatusReset(object sender)
        {
            RxApp.MainThreadScheduler.Schedule(() => Status = "");
        }

        private void SwapAxis()
        {
            var temp = xAxisProperty;
            xAxisProperty = yAxisProperty;
            yAxisProperty = temp;

            this.RaisePropertyChanged(nameof(XAxisProperty));
            this.RaisePropertyChanged(nameof(YAxisProperty));
        }

        private void OpenRawFile()
        {
            if (string.IsNullOrWhiteSpace(RawFilePath) || !File.Exists(RawFilePath))
            {
                Status = $"Error: file \"{RawFilePath}\" does not exist.";
                return;
            }

            try
            {
                Status = $"Loading data...";
                scanMetadata = DataLoader.GetMetadata(RawFilePath);
                Status = $"Data loaded.";
            }
            catch (Exception e)
            {
                Status = $"Error: failed to load file. {e.Message}";
                return;
            }

            var msLevels = scanMetadata.Select(x => x.MSLevel).Distinct().ToList();
            MsLevelOptionsList = allMsLevelOptions.CreateDerivedCollection(x => x,
                x => msLevels.Contains((int) x) ||
                     (x == MsLevelOptions.All && msLevels.Any(y => y == 1) && msLevels.Any(y => y > 1)) ||
                     (x == MsLevelOptions.MSn && msLevels.Any(y => y > 1)));

            SelectedMSLevel = msLevelOptionsList.Min();

            SetColorAxis();
            ChangePlot();
        }

        private void BrowseForRawFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                Filters = { new CommonFileDialogFilter("Thermo .Raw file", "*.raw") },
            };

            if (!string.IsNullOrWhiteSpace(RawFilePath))
            {
                var dir = Path.GetDirectoryName(RawFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                {
                    dialog.InitialDirectory = dir;
                }
            }

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                RawFilePath = dialog.FileName;

                if (!string.IsNullOrWhiteSpace(RawFilePath))
                {
                    OpenRawFile();
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetProperties()
        {
            return typeof(ScanMetadata).GetProperties();
        }

        private void ChangePlot()
        {
            xAxis.Title = descConverter.Convert(xAxisProperty);
            yAxis.Title = descConverter.Convert(yAxisProperty);
            dataSeries.ItemsSource = scanMetadata.Where(x => SelectedMSLevel == MsLevelOptions.All || SelectedMSLevel == MsLevelOptions.MSn && x.MSLevel > 1 || x.MSLevel == (int)SelectedMSLevel);

            var data = scanMetadata.Where(x =>
                    SelectedMSLevel == MsLevelOptions.All || SelectedMSLevel == MsLevelOptions.MSn && x.MSLevel > 1 ||
                    x.MSLevel == (int) SelectedMSLevel)
                .ToList();

            GetMinMax(data, x => Convert.ToDouble(xAxisProperty.GetValue(x)), out var xMin, out var xMax);
            GetMinMax(data, x => Convert.ToDouble(yAxisProperty.GetValue(x)), out var yMin, out var yMax);

            xAxis.AbsoluteMinimum = xMin;
            xAxis.AbsoluteMaximum = xMax;
            yAxis.AbsoluteMinimum = yMin;
            yAxis.AbsoluteMaximum = yMax;

            dataSeries.Mapping = new Func<object, ScatterPoint>(x =>
            {
                return new ScatterPoint(Convert.ToDouble(xAxisProperty.GetValue(x)), Convert.ToDouble(yAxisProperty.GetValue(x)), value: ((ScanMetadata)x).ScanNumber);
            });

            DataPlot.ResetAllAxes();
            DataPlot.InvalidatePlot(true);
        }

        private void ZoomFull()
        {
            DataPlot.ResetAllAxes();
            DataPlot.InvalidatePlot(false);
        }

        private void SetColorAxis()
        {
            var max = 100;
            if (scanMetadata.Count > 0)
            {
                max = scanMetadata.Max(x => x.ScanNumber);
            }

            colorAxis.Minimum = 1;
            colorAxis.Maximum = max;

            colorAxis.Palette.Colors.Clear();
            ////for (int i = 0; i < 256; i++)
            //for (int i = 120; i < 136; i++)
            //{
            //    colorAxis.Palette.Colors.Add(OxyColor.FromAColor((byte)i, OxyColors.DodgerBlue));
            //}
            for (var i = 0; i < max + 1; i++)
            {
                colorAxis.Palette.Colors.Add(OxyColor.FromAColor((byte)(i % 60 + 98), OxyColors.DodgerBlue));
            }
        }

        private void GetMinMax(List<ScanMetadata> data, Func<ScanMetadata, double> selector, out double min, out double max)
        {
            if (data.Count == 0)
            {
                min = 0;
                max = 100;
                return;
            }

            var minVal = data.Min(selector);
            var maxVal = data.Max(selector);

            if (minVal.Equals(maxVal))
            {
                min = minVal - 0.1;
                max = maxVal + 0.1;
            }
            else
            {
                var diff = Math.Abs(maxVal - minVal);
                var adj = 0.05 * diff;
                min = minVal - adj;
                max = maxVal + adj;
            }
        }

        private void SetupPlot()
        {
            DataPlot = new PlotModel();

            yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Ion Injection Time (ms)"
            };

            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Scan Number"
            };

            colorAxis = new LinearColorAxis
            {
                Key = "ColorAxis",
                Position = AxisPosition.None
            };

            colorAxis.Palette.Colors.Clear();
            //for (int i = 0; i < 256; i++)
            for (int i = 120; i < 136; i++)
            {
                colorAxis.Palette.Colors.Add(OxyColor.FromAColor((byte)i, OxyColors.DodgerBlue));
            }

            DataPlot.Axes.Add(yAxis);
            DataPlot.Axes.Add(xAxis);
            DataPlot.Axes.Add(colorAxis);

            var color = Colors.DodgerBlue;
            //var color3 = OxyColor.
            var trackerFormatString = $"Scan: {{{nameof(ScanMetadata.ScanNumber)}}}\nStart time: {{{nameof(ScanMetadata.RetentionTime)}}}\nIon Injection Time (ms): {{{nameof(ScanMetadata.IonInjectionTime)}}}\nBPI: {{{nameof(ScanMetadata.BPI)}}}\nTIC: {{{nameof(ScanMetadata.TIC)}}}\nMS Level: {{{nameof(ScanMetadata.MSLevel)}}}";
            var pointMapper = new Func<object, ScatterPoint>(x => new ScatterPoint(((ScanMetadata) x).ScanNumber, ((ScanMetadata) x).IonInjectionTime, value: ((ScanMetadata)x).ScanNumber));

            dataSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 8,
                MarkerFill = OxyColor.FromAColor(64, OxyColors.DodgerBlue),
                ColorAxisKey = "ColorAxis",
                TrackerFormatString = trackerFormatString,
                Mapping = pointMapper
            };

            DataPlot.Series.Add(dataSeries);
        }
    }
}
