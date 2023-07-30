using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TS3CallsignHelper.Wpf.Commands;

namespace TS3CallsignHelper.Wpf.Models;
public class InterfaceLanguageModel {
  public static readonly List<string> SupportedLanguages = new List<string> { "en-US", "de-DE" };
  public string Name { get; }
  public string Tooltip => $"Header_Language_{Name}";
  public CommandBase Selector { get; }
  public ImageBrush Flag => new ImageBrush {
    ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Resources/Flags/{Name}.png"))
  };

  public InterfaceLanguageModel(string name, CommandBase selector) {
    Name = name;
    Selector = selector;
  }

}
