using System.Windows;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.WPF.Infrastructure;

namespace Configurator
{
    public partial class App : Application
    {
        public App()
        {
            // ReSharper disable once ObjectCreationAsStatement
            //new Bootstrapper<Core.App>(this, new IIocContainer());
        }
    }
}