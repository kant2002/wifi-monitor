namespace WifiMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using WifiMonitor.Properties;
    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;

    public partial class MainForm : Form
    {
        private bool found = false;

        private WlanClient client = new WlanClient();

        private WlanClient.WlanInterface[] interfaces;

        private Dictionary<WlanClient.WlanInterface, bool> scanMap = new Dictionary<WlanClient.WlanInterface, bool>();

        public MainForm()
        {
            this.interfaces = client.Interfaces;
            foreach (var wlanInterface in this.interfaces)
            {
                wlanInterface.WlanNotification += (Wlan.WlanNotificationData notifyData) =>
                {
                    if (notifyData.notificationSource != Wlan.WlanNotificationSource.ACM)
                    {
                        switch ((Wlan.WlanNotificationCodeAcm)notifyData.NotificationCode)
                        {
                            case Wlan.WlanNotificationCodeAcm.ScanFail:
                            case Wlan.WlanNotificationCodeAcm.ScanComplete:
                                scanMap[wlanInterface] = false;
                                break;
                        }
                    }
                };
                scanMap[wlanInterface] = false;
            }

            this.found = this.ScanForNetworks();
            this.InitializeComponent();
        }

        void StartScan(WlanClient.WlanInterface wlanInterface)
        {
            if (scanMap[wlanInterface])
            {
                return;
            }

            scanMap[wlanInterface] = true;
            wlanInterface.Scan();
        }

        private void checkTimer_Tick(object sender, System.EventArgs e)
        {
            var newFound = this.ScanForNetworks();
            if (found != newFound)
            {
                this.found = newFound;
                string message;
                if (newFound)
                {
                    message = "Пришел";
                }
                else
                {
                    message = "Ушел";
                }

                SendNotification(message);
            }
        }

        private bool ScanForNetworks()
        {
            foreach (var wlanInterface in this.interfaces)
            {
                StartScan(wlanInterface);
                if (CheckInterface(wlanInterface))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckInterface(WlanClient.WlanInterface wlanInterface)
        {
            var networks = wlanInterface.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles);
            foreach (var net in networks)
            {
                Wlan.Dot11Ssid ssid = net.dot11Ssid;
                string networkName = Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
                if (networkName == Settings.Default.TargetNetworkName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SendNotification(string message)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(message));
            ToastNotification toast = new ToastNotification(toastXml);
            var notifier = ToastNotificationManager.CreateToastNotifier(Constants.APP_ID);
            notifier.Show(toast);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.textBoxTargetNetwork.Text = Settings.Default.TargetNetworkName;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Settings.Default.TargetNetworkName = this.textBoxTargetNetwork.Text;
            Settings.Default.Save();
        }
    }
}
