namespace NwRestart
{
  public class Server: BaseClass
  {
    private bool _isServiceOn;
    public string Name { get; set; }

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