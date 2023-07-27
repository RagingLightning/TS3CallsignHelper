using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

  private readonly IHost _host;

  public App() {
    _host = Host.CreateDefaultBuilder()
      .ConfigureServices((hostcontext, services) => {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton(s => new MainWindow() {
          DataContext = s.GetRequiredService<MainViewModel>()
        });
      })
      .Build();
  }

  protected override void OnStartup(StartupEventArgs e) {
    _host.Start();

    var mainv = _host.Services.GetRequiredService<MainWindow>();
    var mainvm = _host.Services.GetRequiredService<MainViewModel>();
    mainv.Show();
    mainvm.AddView(new CanvasContainerViewModel(mainvm, new CallsignInformationViewModel()));
    

    base.OnStartup(e);
  }

  protected override void OnExit(ExitEventArgs e) {
    _host.Dispose();

    base.OnExit(e);
  }
}
