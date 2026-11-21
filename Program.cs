using System.Globalization;

namespace EczaneApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Tarih ve para Türkçe görünsün (03.09.2026, 1.701,10)
            var tr = new CultureInfo("tr-TR");
            CultureInfo.DefaultThreadCurrentCulture = tr;
            CultureInfo.CurrentCulture = tr;

            ApplicationConfiguration.Initialize();
            Application.Run(new AnaForm());
        }
    }
}
