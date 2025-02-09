using Syncfusion.Licensing;
using System.Configuration;
using System.Data;
using System.Windows;

namespace kiosk_snapprint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Register Syncfusion License
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH1ecXRRQ2hcVkBzXUY=");
        }
    }
}
