using MaxArtHomeControl.Control;
using MaxArtHomeControl.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace MaxArtHomeControl.UI
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private SynchronizationContext currentContext;
        private CancellationTokenSource tokenSource;
        private Task workingTask;

        private string ip;
        private SecureString user;
        private SecureString password;

        public MainView()
        {
            InitializeComponent();
            this.currentContext = SynchronizationContext.Current;
            this.Loaded += MainView_Loaded;
            this.Switch.Checked += Switch_Checked;
            this.Switch.Unchecked += Switch_Unchecked;
            this.Switch.EditValue = true;
        }

        private void Switch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.tokenSource.Cancel();
            this.ipTextBox.IsEnabled = true;
            if (this.workingTask != null)
            {
                try
                {
                    workingTask.Wait();
                }
                catch (TaskCanceledException)
                {
                    //ignore it
                }
            }

            this.workingTask = null;
        }

        private void Switch_Checked(object sender, RoutedEventArgs e)
        {
            this.ipTextBox.IsEnabled = false;

            this.ip = this.ipTextBox.Text;
            this.user = new SecureString();
            this.password = new SecureString();
            foreach (var cr in "admin") //default user & password are the same
            {
                this.user.AppendChar(cr);
                this.password.AppendChar(cr);
            }

            this.tokenSource = new CancellationTokenSource();
            this.StartDetect();
        }


        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void StartDetect()
        {
            var token = tokenSource.Token;
            this.workingTask = Task.Factory.StartNew(() =>
             {
                 if (token.IsCancellationRequested)
                     return null;

                 var detectedSensor = SensorWorker.DetectSensor(this.ip);
                 var sensorControl = SensorControl.FromBinary(detectedSensor);
                 return sensorControl;
             }, token).ContinueWith(r =>
             {
                 if (r.Result == null) return;
                 this.currentContext.Post((o) =>
                 {
                     var sensorControl = o as SensorControl;
                     this.Room1.SetTitle(sensorControl.Name);
                     this.Room1.Tag = sensorControl;
                     this.StartReadData();
                 }, r.Result);
             }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void StartReadData()
        {
            var token = tokenSource.Token;
            var sensorControl = this.Room1.Tag as SensorControl;
            this.workingTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) return;

                    var result = SensorWorker.ReadAmbientSensor(this.ip, sensorControl.Id, this.user, this.password);
                    if (result != null)
                    {
                        this.currentContext.Post((o) =>
                        {
                            var ambientValue = o as AmbientValue;
                            this.Room1.UpdateValues(ambientValue.Humidity, ambientValue.Temperature);
                        }, result);
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        if (token.IsCancellationRequested) return;
                        Thread.Sleep(1000);
                    }

                }
            }, token).ContinueWith((r) =>
            {
                this.currentContext.Post((o) =>
                {
                    this.statusLabel.Content = o as string;
                }, r.Exception.Message);

                this.StartReadData();
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
