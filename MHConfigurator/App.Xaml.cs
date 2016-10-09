using System.Windows;
using MugenMvvmToolkit;
using MugenMvvmToolkit.WPF.Infrastructure;

namespace MHConfigurator
{
    public partial class App : Application
    {
        public App()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Bootstrapper<Starter>(this, new AutofacContainer());
        }
    }
}