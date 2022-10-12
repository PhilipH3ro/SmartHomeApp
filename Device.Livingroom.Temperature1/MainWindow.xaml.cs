using Microsoft.Azure.Devices.Client;
using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Azure.Devices.Shared;
using Device.Livingroom.Temperature1.Models;
using Newtonsoft.Json;
using System.Text;

namespace Device.Livingroom.Temperature1
{
    public partial class MainWindow : Window
    {
        private readonly string _connect_url = "http://localhost:7289/api/devices/connect";
        private readonly string _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\PhilipHero\\source\\repos\\SmartHomeApp\\Device.Livingroom.Temperature1\\Data\\DB_Livingroom_Temperature1.mdf;Integrated Security=True;Connect Timeout=30";
        private DeviceClient _deviceClient;
        private DeviceInfo deviceInfo;
        private string _deviceId = "";
        private bool _connected = false;
        private int _interval = 1000;
        private bool _lightState = true;
        private bool _lightPrevState = false;

        public MainWindow()
        {
            InitializeComponent();
            SetupAsync().ConfigureAwait(false);
            SendMessageAsync().ConfigureAwait(false);
        }

        private async Task SetupAsync()
        {
            tbStateMessage.Text = "Initializing Device. Please wait...";

            using IDbConnection conn = new SqlConnection(_connectionString);
            _deviceId = await conn.QueryFirstOrDefaultAsync<string>("SELECT DeviceId FROM DeviceInfo");
            if (string.IsNullOrEmpty(_deviceId))
            {
                tbStateMessage.Text = "Generating new DeviceID";
                _deviceId = Guid.NewGuid().ToString();
                await conn.ExecuteAsync("INSERT INTO DeviceInfo (DeviceId,DeviceName,DeviceType,Location) VALUES (@DeviceId, @DeviceName, @DeviceType, @Location)", new { DeviceId = _deviceId, DeviceName = "Temperature", DeviceType = "temperature", Location = "livingroom" });
            }

            var device_ConnectionString = await conn.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM DeviceInfo WHERE DeviceId = @DeviceId", new { DeviceId = _deviceId });
            if (string.IsNullOrEmpty(device_ConnectionString))
            {
                tbStateMessage.Text = "Initializing ConnectionString. Please wait...";
                using var http = new HttpClient();
                var result = await http.PostAsJsonAsync(_connect_url, new { deviceId = _deviceId });
                device_ConnectionString = await result.Content.ReadAsStringAsync();
                await conn.ExecuteAsync("UPDATE DeviceInfo SET ConnectionString = @ConnectionString WHERE DeviceId = @DeviceId", new { DeviceId = _deviceId, ConnectionString = device_ConnectionString });
            }

            _deviceClient = DeviceClient.CreateFromConnectionString(device_ConnectionString);

            tbStateMessage.Text = "Updating Twin Properties. Please wait...";

            deviceInfo = await conn.QueryFirstOrDefaultAsync<DeviceInfo>("SELECT * FROM DeviceInfo WHERE DeviceId = @DeviceId", new { DeviceId = _deviceId });

            var twinCollection = new TwinCollection();
            twinCollection["deviceName"] = deviceInfo.DeviceName;
            twinCollection["deviceType"] = deviceInfo.DeviceType;
            twinCollection["location"] = deviceInfo.Location;

            await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

            _connected = true;

            tbStateMessage.Text = $"{deviceInfo.DeviceId}\nName: {deviceInfo.DeviceName}\nType: {deviceInfo.DeviceType}\nLocation: {deviceInfo.Location}\nDevice Connected.";
        }

        private async Task SendMessageAsync()
        {
            while (true)
            {
                if (_connected)
                {
                    if (_lightState != _lightPrevState)
                    {
                        _lightPrevState = _lightState;

                        // d2c
                        var json = JsonConvert.SerializeObject(new { lightState = _lightState });
                        var message = new Message(Encoding.UTF8.GetBytes(json));
                        message.Properties.Add("deviceName", deviceInfo.DeviceName);
                        message.Properties.Add("deviceType", deviceInfo.DeviceType);
                        message.Properties.Add("location", deviceInfo.Location);

                        await _deviceClient.SendEventAsync(message);
                        tbStateMessage.Text = $"{deviceInfo.DeviceId}\nName: {deviceInfo.DeviceName}\nType: {deviceInfo.DeviceType}\nLocation: {deviceInfo.Location}\n\nMessage sent at {DateTime.Now}.";

                        // device twin (reported)
                        var twinCollection = new TwinCollection();
                        twinCollection["lightState"] = _lightState;
                        await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);
                    }
                }
                await Task.Delay(_interval);
            }
        }

        private void btnOnOff_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Working Yet");
        }
    }
}
