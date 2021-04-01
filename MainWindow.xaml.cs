using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.CompilerServices;
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
        new() {Name="vpp-nw-navis01"},
        new() {Name="vpp-nw-navis02"},
        //new() {Name="vpp-nw-navis03"},
        //new() {Name="vpp-nw-navis04"},
        //new() {Name="vpp-nw-navis05"},
        //new() {Name="vpp-nw-navis06"},
        //new() {Name="vpp-nw-navis07"},
        //new() {Name="vpp-nw-navis08"}
      };

      ServerList.ItemsSource = Servers;
      CheckPingAsync();
    }

    private void ButtonRestart_OnClick (object sender, RoutedEventArgs e)
    {
      foreach (Server server in ServerList.Items)
      {
        if (server.IsChecked)
        {
          SendServiceCommandAsync(server.Name, "stop");
          Task.Delay(1000);
          SendServiceCommandAsync(server.Name, "start");
        }
      }
    }
    private async void ButtonStart_OnClick(object sender, RoutedEventArgs e)
    {
      foreach (Server server in ServerList.Items)
      {
        if (server.IsChecked)
        {
          SendServiceCommandAsync(server.Name, "start");
        }
      }
    }
    private async void ButtonStop_OnClick(object sender, RoutedEventArgs e)
    {
      foreach (Server server in ServerList.Items)
      {
        if (server.IsChecked)
        {
          SendServiceCommandAsync(server.Name, "stop");
        }
      }
    }

    public async void SendServiceCommandAsync(string serverName, string command)
    {
      IsEnableButtons(false);

      ProcessStartInfo psi = new ProcessStartInfo()
      {
        FileName = "sc",
        Arguments = @$"\\{serverName} {command} Revit2NavisService",
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardOutput = true
      };
      var process = Process.Start(psi);
      var output = await process.StandardOutput.ReadToEndAsync();
      ConsoleBox.AppendText($"{serverName}: {output}");
      IsEnableButtons(true);
    }

    public void IsEnableButtons(bool state)
    {
      Restart.IsEnabled = state;
      Stop.IsEnabled = state;
      Start.IsEnabled = state;
    }

    private async Task CheckPingAsync()
    {
      while (true)
      {
        var pingTasks = Servers.Select(server =>
        {
          bool pingable = false;
          using var ping = new Ping();

          return ping.SendPingAsync(server.Name);
        });
        await Task.WhenAll(pingTasks);
        foreach (var pingReply in pingTasks)
        {
          var serverReplyName = pingReply.Result.Address;
          var server = Servers.FirstOrDefault(s => s.Name == Dns.GetHostEntry(serverReplyName).HostName);
          server.IsServiceOn = pingReply.Result.Status == IPStatus.Success;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(10000));
      }
    }
  }

  public static class Pinger
  {
    public static bool GetPingResponse(string machineAdress, int timeout = 3000)
    {
      bool pingable = false;
      Ping pinger = new Ping();
      try
      {
        pinger = new Ping();
        PingReply reply = pinger.Send(machineAdress, timeout);
        if (reply != null) pingable = reply.Status == IPStatus.Success;
      }
      catch (PingException exception)
      {
        // Discard ping exceptions and return false
      }
      finally
      {
        pinger?.Dispose();
      }

      return pingable;
    }
  }

  public class Server
  {
    public string Name { get; set; }
    public bool IsServiceOn { get; set; }
    public bool IsChecked { get; set; }
  }
}
