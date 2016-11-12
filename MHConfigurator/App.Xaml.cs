using System;
using System.IO;
using System.Windows;
using MugenMvvmToolkit;
using MugenMvvmToolkit.WPF.Infrastructure;

using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace MHConfigurator
{
    public partial class App : Application
    {
        public App()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (path != null)
            {
                path = Path.Combine(path, IntPtr.Size == 8 ? "x64" : "x86");
                bool ok = SetDllDirectory(path);
                if (!ok) throw new System.ComponentModel.Win32Exception();
            }
            else throw new System.ComponentModel.Win32Exception();

            new Bootstrapper<Starter>(this, new AutofacContainer());
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);
    }
}