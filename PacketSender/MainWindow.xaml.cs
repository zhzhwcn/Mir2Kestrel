using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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
using Newtonsoft.Json;

namespace PacketSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<ClientPacketIds, Packet> _clientPackets = new();

        public MainWindow()
        {
            InitializeComponent();
            var values = Enum.GetValues<ClientPacketIds>().ToDictionary(x => (short)x, x => $"{(short)x} - {x}");
            foreach (var value in values)
            {
                Type.Items.Add(value);
            }

            var types = typeof(Packet).Assembly.GetTypes().Where(t => t.IsClass && t.Namespace == "ClientPackets");
            foreach (var type in types)
            {
                var p = (Packet?)Activator.CreateInstance(type);
                if (p == null)
                {
                    continue;
                }
                _clientPackets[(ClientPacketIds)p.Index] = p;
            }

            Network.OnConnected += OnConnected;
            Network.OnDisconnected += OnDisconnected;

            Task.Run(async () =>
            {
                while (Network.Connected)
                {
                    Network.Process();
                    await Task.Delay(5);
                }
            });
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Connect.Visibility = Visibility.Visible;
                Disconnect.Visibility = Visibility.Collapsed;
            }));
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Connect.Visibility = Visibility.Collapsed;
                Disconnect.Visibility = Visibility.Visible;
            }));
        }

        private void Type_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.IsDefined(typeof(ClientPacketIds), Type.SelectedValue))
            {
                var packId = (ClientPacketIds) Type.SelectedValue;
                if (!_clientPackets.ContainsKey(packId))
                {
                    MessageBox.Show("PacketType Not Found");
                    return;
                }

                var json = JsonConvert.SerializeObject(_clientPackets[packId], Formatting.Indented);
                Content.Text = json;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Settings.IpAddress = Ip.Text;
            Settings.Port = int.Parse(Port.Text);
            Network.Connect();
        }
    }
}
