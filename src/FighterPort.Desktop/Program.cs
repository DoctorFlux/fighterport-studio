using System.Windows;

namespace FighterPort.Desktop;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var app=new Application();var window=new Workbench(args);return app.Run(window);
    }
}
