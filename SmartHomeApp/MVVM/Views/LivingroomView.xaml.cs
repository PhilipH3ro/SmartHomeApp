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

namespace SmartHomeApp.MVVM.Views
{
    public partial class LivingroomView : UserControl
    {
        public LivingroomView()
        {
            InitializeComponent();
        }
        private void btn_Open_CreateDeviceView(object sender, RoutedEventArgs e)
        {
            CreateDeviceView infoWindow = new CreateDeviceView();

            infoWindow.Show();
        }
    }
}
