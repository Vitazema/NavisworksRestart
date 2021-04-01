using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NwRestart
{
  public class BaseClass : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    internal void OnPropertyChanged([CallerMemberName] string nameProp = "") =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameProp));
  }
}