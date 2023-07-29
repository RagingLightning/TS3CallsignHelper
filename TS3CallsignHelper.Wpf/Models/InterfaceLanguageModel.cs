using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TS3CallsignHelper.Wpf.Commands;

namespace TS3CallsignHelper.Wpf.Models;
internal class InterfaceLanguageModel {
  public string Name { get; }
  public CommandBase Selector { get; }
  public ImageBrush Flag => new ImageBrush {
    ImageSource = new BitmapImage(new Uri($"pack://application:,,,/{GetType().Assembly.FullName};component/Resources/Flags/{Name}.png"))
  };

  public InterfaceLanguageModel(string name, CommandBase selector) {
    Name = name;
    Selector = selector;
  }

}
