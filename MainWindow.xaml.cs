using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Text;
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

namespace NwRestart
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public ObservableCollection<Server> Servers { get; set; }
    public List<Server> SelectedServers;
    public MainWindow()
    {
      InitializeComponent();
      SelectedServers = new List<Server>();

      Servers = new ObservableCollection<Server>() {
        new("vpp-nw-navis01"),
        new("vpp-nw-navis02"),
        new("vpp-nw-navis03"),
        new("vpp-nw-navis04"),
        new("vpp-nw-navis05"),
        new("vpp-nw-navis06"),
        new("vpp-nw-navis07"),
        new("vpp-nw-navis08"),
      };

      ServerList.ItemsSource = Servers;
      StartCheckPingAsync();
    }

    private void ButtonRestart_OnClick(object sender, RoutedEventArgs e)
    {
      foreach (Server server in ServerList.Items)
      {
        if (server.IsChecked)
        {
          SendServiceCommandAsync(server.IpAddress, "stop");
          Task.Delay(1000);
          SendServiceCommandAsync(server.IpAddress, "start");
        }
      }
    }
    private async void ButtonStart_OnClick(object sender, RoutedEventArgs e)
    {
      foreach (Server server in ServerList.Items)
      {
        if (server.IsChecked)
        {
          SendServiceCommandAsync(server.IpAddress, "start");
        }
      }
    }
    private async void ButtonStop_OnClick(object sender, RoutedEventArgs e)
    {
      foreach (Server server in ServerList.Items)
      {
        if (server.IsChecked && server.IsServiceOn)
        {
          SendServiceCommandAsync(server.IpAddress, "stop");
        }
      }
    }

    private async void ButtonServerRestart_OnClick(object sender, RoutedEventArgs e)
    {
    }

    public async void SendServiceCommandAsync(IPAddress server, string command)
    {
      IsEnableButtons(false);

      ProcessStartInfo psi = new ProcessStartInfo()
      {
        FileName = "sc",
        Arguments = @$"\\{server} {command} Revit2NavisService",
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true
      };
      var process = Process.Start(psi);
      ConsoleBox.AppendText($"{server} is {command}ing...\n");

      var output = await process.StandardOutput.ReadToEndAsync();
      ConsoleBox.AppendText($"{server}: {output}");
      IsEnableButtons(true);
    }

    public async void SendRestartCommandAsync(IPAddress server)
    {
      IsEnableButtons(false);
      var psi = new ProcessStartInfo()
      {
        FileName = "shutdown",
        Arguments = $@"/r /m \\{server} /t 0",
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true
      };
      var process = Process.Start(psi);
      ConsoleBox.AppendText($"{server} is restarting...\n");

      var output = await process.StandardOutput.ReadToEndAsync();
      ConsoleBox.AppendText($"{server}: {output}");
      IsEnableButtons(true);
    }

    public void IsEnableButtons(bool state)
    {
      Restart.IsEnabled = state;
      Stop.IsEnabled = state;
      Start.IsEnabled = state;
    }

    private async Task StartCheckPingAsync()
    {
      while (true)
      {
        var pingTasks = Servers.ToDictionary(server => server, server =>
         {
           using var ping = new Ping();

           server.IpAddress = Dns.GetHostAddresses(server.Name)[0];
           return ping.SendPingAsync(server.IpAddress);

         });

        await Task.WhenAll(pingTasks.Values);

        foreach (var pingTask in pingTasks)
        {
          pingTask.Key.IsServiceOn = pingTask.Value.Result.Status == IPStatus.Success;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(10000));
      }
    }
  }
}
