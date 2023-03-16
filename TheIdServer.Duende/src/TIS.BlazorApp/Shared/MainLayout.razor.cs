using System.Runtime.InteropServices;

namespace TIS.BlazorApp.Shared
{
    public partial class MainLayout
    {
        static bool IsClientSide => RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
    }
}
