using System;
using System.IO;
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
            try
            {
                new Bootstrapper<Starter>(this, new AutofacContainer());
            }
            catch (Exception e)
            {
                File.WriteAllText(@"D:\1.txt", e.Message);
                File.WriteAllText(@"D:\1.txt", e.StackTrace);
            }
        }
    }
}