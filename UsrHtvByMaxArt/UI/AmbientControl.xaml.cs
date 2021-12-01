using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MaxArtHomeControl.UI
{
    /// <summary>
    /// Interaction logic for AmbientControl.xaml
    /// </summary>
    public partial class AmbientControl : UserControl
    {
        public decimal Humidity { get; private set; }
        public decimal Temperature { get; private set; }
        public decimal TemperatureMark { get; private set; }
        public AmbientControl()
        {
            InitializeComponent();
            this.UpdateValues(79.9M, 49.9M);
            this.SetTemperatureMArk(22, 24);
            this.humidityGaugeLabel.Content = string.Format("{0}%", "---");
            this.temperatureGaugeLabel.Content = string.Format("{0}°C", "---");
        }

        public void UpdateValues(decimal humidity, decimal temperature)
        {
            this.Humidity = humidity;

            this.humidityGauge.Scales[0].Needles[0].Value = (double)humidity;
            this.humidityGaugeLabel.Content = string.Format("{0}%", this.Humidity.ToString("0.00"));
            this.Temperature = temperature;
            this.temperatureGauge.Scales[0].Needles[0].Value = (double)temperature;
            this.temperatureGaugeLabel.Content = string.Format("{0}°C", this.Temperature.ToString("0.00"));
        }

        public void SetTemperatureMArk(double minValue, double maxValue)
        {
            this.TemperatureMinMarker.Value = minValue;
            this.TemperatureMaxMArker.Value = maxValue;
        }

        public void SetTitle(string value)
        {
            this.titleLabel.Content = value;
        }
    }
}
