using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices;
using SmartHomeApp.MVVM.Models;
using SmartHomeApp.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using SmartHomeApp.Services;

namespace SmartHomeApp.MVVM.ViewModels
{
    internal class LivingroomViewModel : ViewHelper
    {
        private DispatcherTimer timer;
        private DispatcherTimer timerTemperature;
        private ObservableCollection<DeviceItem> _deviceItems;
        private List<DeviceItem> _tempList;
        private readonly RegistryManager registryManager = RegistryManager.CreateFromConnectionString("HostName=kyh-iot.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SbMYzmgd/E1FhiLrw/CVm2LrLPss1Ma4/kTF9Sy3X0Y=");
        private readonly IWeatherService _weatherService;
        public string Title { get; set; } = "Livingroom";

        public LivingroomViewModel()
        {
            _tempList = new List<DeviceItem>();
            _deviceItems = new ObservableCollection<DeviceItem>();
            StartDevicesAsync().ConfigureAwait(false);

            _weatherService = new WeatherService();

            SetInterval(TimeSpan.FromSeconds(1));
            SetIntervalTemperature(TimeSpan.FromSeconds(5));
        }

        public IEnumerable<DeviceItem> DeviceItems => _deviceItems;

        private void SetInterval(TimeSpan interval)
        {
            timer = new DispatcherTimer()
            {
                Interval = interval
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void SetIntervalTemperature(TimeSpan interval)
        {
            timerTemperature = new DispatcherTimer()
            {
                Interval = interval
            };
            timerTemperature.Tick += new EventHandler(Timer_Tick_Temperature);
            timerTemperature.Start();
        }

        private string? _currentWeatherCondition;
        public string CurrentWeatherCondition
        {
            get => _currentWeatherCondition!;
            set
            {
                _currentWeatherCondition = value;
                OnPropertyChanged();
            }
        }

        private string? _currentTemperature;
        public string CurrentTemperature
        {
            get => _currentTemperature!;
            set
            {
                _currentTemperature = value;
                OnPropertyChanged();
            }
        }

        private string? _name;
        public string Name
        {
            get => _name!;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string? _currentTime;
        public string CurrentTime
        {
            get => _currentTime!;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        private string? _currentDate;
        public string CurrentDate
        {
            get => _currentDate!;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.ToString("dd MMMM yyyy");
        }

        private async void Timer_Tick_Temperature(object sender, EventArgs e)
        {
            await StartDevicesAsync();
            await UpdateDevicesAsync();
            await SetWeatherAsync();
        }

        private async Task SetWeatherAsync()
        {
            var weather = await _weatherService.GetWeatherDataAsync();
            CurrentTemperature = weather.Temperature.ToString() + "°C";
            CurrentWeatherCondition = weather.WeatherCondition ?? "";
            Name = weather.Name;
        }

        private async Task UpdateDevicesAsync()
        {
            _tempList.Clear();

            foreach (var item in _deviceItems)
            {
                var device = await registryManager.GetDeviceAsync(item.DeviceId);
                if (device == null)
                    _tempList.Add(item);
            }

            foreach (var item in _tempList)
            {
                _deviceItems.Remove(item);
            }
        }

        private async Task StartDevicesAsync()
        {
            //var result = registryManager.CreateQuery("select * from devices where location = 'kitchen'");
            var result = registryManager.CreateQuery("select * from devices where properties.reported.location = 'livingroom' or properties.reported.location = 'Livingroom'");

            if (result.HasMoreResults)
            {
                foreach (Twin twin in await result.GetNextAsTwinAsync())
                {
                    var device = _deviceItems.FirstOrDefault(x => x.DeviceId == twin.DeviceId);

                    if (device == null)
                    {
                        device = new DeviceItem
                        {
                            DeviceId = twin.DeviceId,
                        };

                        try { device.DeviceName = twin.Properties.Reported["deviceName"]; }
                        catch { device.DeviceName = device.DeviceId; }
                        try { device.DeviceType = twin.Properties.Reported["deviceType"]; }
                        catch { }

                        switch (device.DeviceType.ToLower())
                        {
                            case "temperature":
                                device.IconActive = "\uf76a";
                                device.IconInActive = "\uf768";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            case "light":
                                device.IconActive = "\uf0eb";
                                device.IconInActive = "\uf673";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            case "tv":
                                device.IconActive = "\ue163";
                                device.IconInActive = "\ue2fa";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            case "computer":
                                device.IconActive = "\uf109";
                                device.IconInActive = "\ue1c7";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            default:
                                device.IconActive = "UNKNOWN";
                                device.IconInActive = "UNKNOWN";
                                device.StateActive = "ENABLE";
                                device.StateInActive = "DISABLE";
                                break;
                        }
                        _deviceItems.Add(device);
                    }
                    else { }
                }
            }
            else
            {
                _deviceItems.Clear();
            }
        }
    }
}