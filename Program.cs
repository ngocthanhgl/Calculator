using System;
using System.Reflection;
using System.Windows.Forms;

[assembly: AssemblyTitle("Calculator")]
[assembly: AssemblyDescription("Fluent Design Calculator")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Calculator")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new FluentCalculator());
    }
}
