using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ChromeRegistryMonitor;

public class RegistryWatcher
{
    private const string GOOGLE_KEY_PATH = @"SOFTWARE\Policies\Google";
    private const string CHROME_SUB_KEY = @"Chrome";
    private const int KEY_QUERY_VALUE = 0x0001;
    private const int KEY_NOTIFY = 0x0010;
    private const int STANDARD_RIGHTS_READ = 0x00020000;
    private const int ERROR_SUCCESS = 0;
    private const RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute | RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Security;

    #region P/Invoke

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegOpenKeyEx(
        IntPtr hKey,
        string lpSubKey,
        int ulOptions,
        int samDesired,
        out IntPtr phkResult);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(
        IntPtr hKey,
        bool bWatchSubtree,
        RegChangeNotifyFilter dwNotifyFilter,
        IntPtr hEvent,
        bool fAsynchronous);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegCloseKey(IntPtr hKey);

    #endregion

    public static void WatchRegistry()
    {
        IntPtr hKey = IntPtr.Zero;
        ManualResetEvent eventTerminate = new(false);
        RegistryKey? googleKey = null;

        try
        {
            Console.WriteLine($"Watching for changes in the {GOOGLE_KEY_PATH} registry");
            googleKey = Registry.LocalMachine.OpenSubKey(GOOGLE_KEY_PATH, true);
            if (googleKey != null)
            {
                if (googleKey.OpenSubKey(CHROME_SUB_KEY) != null)
                {
                    googleKey.DeleteSubKeyTree(CHROME_SUB_KEY);
                    Console.WriteLine("Chrome subkey tree deleted successfully");
                }
            }

            while (true)
            {
                int result = RegOpenKeyEx(
                    new IntPtr((int)RegistryHive.LocalMachine),
                    GOOGLE_KEY_PATH,
                    0,
                    STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY,
                    out hKey);

                if (result != ERROR_SUCCESS)
                {
                    throw new Exception("Failed to open registry key. Error code: " + result);
                }

                AutoResetEvent eventNotify = new(false);
                WaitHandle[] waitHandles = new WaitHandle[] { eventNotify, eventTerminate };

                result = RegNotifyChangeKeyValue(
                    hKey,
                    true,
                    _regFilter,
                    eventNotify.SafeWaitHandle.DangerousGetHandle(),
                    true);

                if (result != ERROR_SUCCESS)
                {
                    throw new Exception("Failed to watch registry key. Error code: " + result);
                }

                if (WaitHandle.WaitAny(waitHandles) == 0)
                {
                    Thread.Sleep(5000);
                    googleKey = Registry.LocalMachine.OpenSubKey(GOOGLE_KEY_PATH, true);
                    if (googleKey != null)
                    {
                        if (googleKey.OpenSubKey(CHROME_SUB_KEY) != null)
                        {
                            googleKey.DeleteSubKeyTree(CHROME_SUB_KEY);
                            Console.WriteLine("Chrome subkey tree deleted successfully");
                        }
                    }

                    if (hKey != IntPtr.Zero)
                    {
                        RegCloseKey(hKey);
                    }
                    googleKey?.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
        finally
        {
            if (hKey != IntPtr.Zero)
            {
                RegCloseKey(hKey);
            }
            googleKey?.Close();
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [Flags]
    public enum RegChangeNotifyFilter
    {
        Key = 1,
        Attribute = 2,
        Value = 4,
        Security = 8,
    }
}

class Program
{
    static void Main(string[] args)
    {
        RegistryWatcher.WatchRegistry();
    }
}
