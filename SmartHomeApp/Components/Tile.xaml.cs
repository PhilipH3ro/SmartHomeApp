using Microsoft.Azure.Devices;
using SmartHomeApp.MVVM.Models;
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

namespace SmartHomeApp.Components
{
    /// <summary>
    /// Interaction logic for TileComponent.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        private readonly RegistryManager registryManager = RegistryManager.CreateFromConnectionString("HostName=kyh-iot.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SbMYzmgd/E1FhiLrw/CVm2LrLPss1Ma4/kTF9Sy3X0Y=");

        public Tile()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DeviceNameProperty = DependencyProperty.Register("DeviceName", typeof(string), typeof(Tile));
        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty = DependencyProperty.Register("DeviceType", typeof(string), typeof(Tile));
        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(string), typeof(Tile));
        public string Location
        {
            get { return (string)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(Tile));
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IconActiveProperty = DependencyProperty.Register("IconActive", typeof(string), typeof(Tile));

        public string IconActive
        {
            get { return (string)GetValue(IconActiveProperty); }
            set { SetValue(IconActiveProperty, value); }
        }

        public static readonly DependencyProperty IconInActiveProperty = DependencyProperty.Register("IconInActive", typeof(string), typeof(Tile));

        public string IconInActive
        {
            get { return (string)GetValue(IconInActiveProperty); }
            set { SetValue(IconInActiveProperty, value); }
        }

        public static readonly DependencyProperty StateActiveProperty = DependencyProperty.Register("StateActive", typeof(string), typeof(Tile));

        public string StateActive
        {
            get { return (string)GetValue(StateActiveProperty); }
            set { SetValue(StateActiveProperty, value); }
        }

        public static readonly DependencyProperty StateInActiveProperty = DependencyProperty.Register("StateInActive", typeof(string), typeof(Tile));

        public string StateInActive
        {
            get { return (string)GetValue(StateInActiveProperty); }
            set { SetValue(StateInActiveProperty, value); }
        }

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var deviceItem = (DeviceItem)button.DataContext;
            await registryManager.RemoveDeviceAsync(deviceItem.DeviceId);

            MessageBox.Show($"{deviceItem.DeviceId} was removed.");
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var deviceItem = (DeviceItem)button.DataContext;

            MessageBox.Show($"{deviceItem.DeviceId}\nName: {deviceItem.DeviceName}\nType: {deviceItem.DeviceType}\nLocation: {deviceItem.Location}\nCan't be edited yet.");

        }
    }
}