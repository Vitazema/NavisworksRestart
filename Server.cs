using System.Net;

namespace NwRestart
{
  public class Server: BaseClass
  {
    public Server(string serverAddress)
    {
      Name = serverAddress;
    }
    private bool _isServiceOn;
    public string Name { get; set; }

    public IPAddress IpAddress { get; set; }

    public bool IsServiceOn
    {
      get => _isServiceOn;
      set
      {
        _isServiceOn = value;
        OnPropertyChanged();
      }
    }

    public bool IsChecked { get; set; }
  }
}