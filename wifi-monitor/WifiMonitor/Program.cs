namespace WifiMonitor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
    using MS.WindowsAPICodePack.Internal;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main()
        {
            TryCreateShortcut();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        // In order to display toasts, a desktop application must have a shortcut on the Start menu. 
        // Also, an AppUserModelID must be set on that shortcut. 
        // The shortcut should be created as part of the installer. The following code shows how to create 
        // a shortcut and assign an AppUserModelID using Windows APIs. You must download and include the  
        // Windows API Code Pack for Microsoft .NET Framework for this code to function 
        // 
        // Included in this project is a wxs file that be used with the WiX toolkit 
        // to make an installer that creates the necessary shortcut. One or the other should be used. 
        private static bool TryCreateShortcut()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\WifiMonitor.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        } 

        private static void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable 
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe 
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property 
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(Constants.APP_ID))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk 
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        } 
    }
}
