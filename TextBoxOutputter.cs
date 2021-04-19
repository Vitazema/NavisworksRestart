using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace NwRestart
{
  public class TextBoxOutputter : TextWriter
  {
    private readonly TextBox _textBox;
 
    public TextBoxOutputter(TextBox output)
    {
      _textBox = output;
    }
 
    public override void Write(char value)
    {
      base.Write(value);
      _textBox.Dispatcher.BeginInvoke(new Action(() =>
      {
        _textBox.AppendText(value.ToString());
      }));
    }
 
    public override Encoding Encoding => Encoding.GetEncoding(855);
  }
}