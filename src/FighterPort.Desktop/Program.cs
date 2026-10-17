using System.Windows;

namespace FighterPort.Desktop;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var app=new Application();app.Resources.MergedDictionaries.Add(new ResourceDictionary{Source=new Uri("/FighterPort.Desktop;component/ClassicTheme.xaml",UriKind.Relative)});if(args.Length>=2&&args[0]=="--sampling-smoke")return SamplingSmoke.Run(args[1],args.Length>2?args[2]:null);var window=new Workbench(args);return app.Run(window);
    }
}
