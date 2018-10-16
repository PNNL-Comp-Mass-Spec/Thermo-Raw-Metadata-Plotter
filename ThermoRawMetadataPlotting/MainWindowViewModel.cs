using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveUI;
using ThermoRawMetadataReader;

namespace ThermoRawMetadataPlotting
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string rawFilePath;
        private PropertyInfo yAxisProperty;
        private PropertyInfo xAxisProperty;
        private PlotModel dataPlot;
        private List<ScanMetadata> scanMetadata = new List<ScanMetadata>();
        private Axis yAxis;
        private Axis xAxis;
        private ScatterSeries dataSeries;

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

        public List<PropertyInfo> ScanMetadataProperties { get; }

        public ReactiveCommand<Unit, Unit> OpenRawFileCommand { get; }
        public ReactiveCommand<Unit, Unit> BrowseForRawFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SwapAxisCommand { get; }

        public MainWindowViewModel() : this("")
        {
        }

        public MainWindowViewModel(string rawFilePath)
        {
            RawFilePath = rawFilePath;

            OpenRawFileCommand = ReactiveCommand.Create(OpenRawFile);
            BrowseForRawFileCommand = ReactiveCommand.Create(BrowseForRawFile);
            SwapAxisCommand = ReactiveCommand.Create(SwapAxis);

            ScanMetadataProperties = new List<PropertyInfo>(GetProperties());
            XAxisProperty = ScanMetadataProperties.First(x => x.Name.Equals(nameof(ScanMetadata.ScanNumber)));
            YAxisProperty = ScanMetadataProperties.First(x => x.Name.Equals(nameof(ScanMetadata.IonInjectionTime)));

            SetupPlot();

            if (!string.IsNullOrWhiteSpace(RawFilePath))
            {
                OpenRawFile();
            }

            this.WhenAnyValue(x => x.XAxisProperty, x => x.YAxisProperty).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe(x => ChangePlot());
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
                //statusUpdate($"Error: file \"{RawFilePath}\" does not exist.");
                return;
            }

            try
            {
                scanMetadata = DataLoader.GetMetadata(RawFilePath);
            }
            catch (Exception e)
            {
                //statusUpdate($"Error: failed to load file. {e.Message}");
                return;
            }

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

        private DescriptionConverter descConverter = new DescriptionConverter();

        private void ChangePlot()
        {
            xAxis.Title = descConverter.Convert(xAxisProperty);
            yAxis.Title = descConverter.Convert(yAxisProperty);
            dataSeries.ItemsSource = scanMetadata;

            dataSeries.Mapping = new Func<object, ScatterPoint>(x =>
            {
                return new ScatterPoint(Convert.ToDouble(xAxisProperty.GetValue(x)), Convert.ToDouble(yAxisProperty.GetValue(x)));
            });

            DataPlot.InvalidatePlot(true);
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

            DataPlot.Axes.Add(yAxis);
            DataPlot.Axes.Add(xAxis);

            var color = Colors.DodgerBlue;
            var trackerFormatString = $"Scan: {{{nameof(ScanMetadata.ScanNumber)}}}\nStart time: {{{nameof(ScanMetadata.RetentionTime)}}}\nIon Injection Time (ms): {{{nameof(ScanMetadata.IonInjectionTime)}}}\nBPI: {{{nameof(ScanMetadata.BPI)}}}\nTIC: {{{nameof(ScanMetadata.TIC)}}}\nMS Level: {{{nameof(ScanMetadata.MSLevel)}}}";
            var pointMapper = new Func<object, ScatterPoint>(x => new ScatterPoint(((ScanMetadata) x).ScanNumber, ((ScanMetadata) x).IonInjectionTime));

            dataSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromAColor(128, OxyColors.DodgerBlue),
                TrackerFormatString = trackerFormatString,
                Mapping = pointMapper
            };

            DataPlot.Series.Add(dataSeries);
        }
    }
}
