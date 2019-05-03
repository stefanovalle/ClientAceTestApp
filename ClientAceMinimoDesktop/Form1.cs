using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kepware.ClientAce.OpcDaClient;

namespace ClientAceMinimoDesktop
{
    public partial class Form1 : Form
    {
        DaServerMgt daServerMgt = new Kepware.ClientAce.OpcDaClient.DaServerMgt();
        ConnectInfo connectInfo = new Kepware.ClientAce.OpcDaClient.ConnectInfo();

        public Form1()
        {
            InitializeComponent();

            // Event handler
            daServerMgt.DataChanged += DaServerMgt_DataChanged;
        }

        private void DaServerMgt_DataChanged(int clientSubscription, bool allQualitiesGood, bool noErrors, ItemValueCallback[] itemValues)
        {
            try
            {
                foreach (ItemValueCallback itemValue in itemValues)
                {
                    if (itemValue.ResultID.Succeeded)
                    {
                        
                        realtime.Text += itemValue.TimeStamp + ": " + itemValue.ClientHandle + " - " + itemValue.Value + Environment.NewLine;
                        realtime.SelectionStart = realtime.TextLength;
                        realtime.ScrollToCaret();
                    }
                    else
                    {
                        debugBox.Text += "Errore";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataChanged exception. Reason: {0}", ex);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectInfo.LocalId = "en";
            connectInfo.KeepAliveTime = 5000;
            connectInfo.RetryAfterConnectionError = true;
            connectInfo.RetryInitialConnection = true;
            connectInfo.ClientName = "Demo ClientAceC-Sharp DesktopApplication";
            bool connectFailed;
            string url = "opcda://127.0.0.1/Kepware.KEPServerEX.V6/{7BC0CC8E-482C-47CA-ABDC-0FE7F9C6E729}";
            int clientHandle = 1;

            // Connessione al server
            try
            {
                daServerMgt.Connect(url, clientHandle, ref connectInfo, out connectFailed);
                if (connectFailed)
                {
                    debugBox.Text  = "Connect failed" + Environment.NewLine;
                }
                else
                {
                    debugBox.Text = "Connected to Server "+ url + " Succeeded." + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                debugBox.Text = ex.ToString();
            }

            AggiornaDati();

            // Tag a cui mi voglio sottoscrivere
            ItemIdentifier[] items = new ItemIdentifier[2];
            items[0] = new ItemIdentifier
            {
                ItemName = "its-iot-device.Device1.PlantStatus",
                ClientHandle = "PlantStatus"
            };
            items[1] = new ItemIdentifier
            {
                ItemName = "Simulation Examples.Functions.Ramp1",
                ClientHandle = "Ramp1"
            };

            int serverSubscription;
            ReturnCode returnCode;
            
            // Parametri di sottoscrizione agli eventi
            int clientSubscription = 1;
            bool active = true;
            int updateRate = 1000;
            int revisedUpdateRate;
            float deadband = 0;

            try
            {
                // Sottoscrizione agli eventi change data
                returnCode = daServerMgt.Subscribe(clientSubscription,
                active,
                updateRate,
                out revisedUpdateRate,
                deadband,
                ref items,
                out serverSubscription);
            }
            catch (Exception ex)
            {
                debugBox.Text = ex.ToString();
            }
            
        }

        private void AggiornaDati()
        {
            // Aggiorno a mano i valori di due tag

            int maxAge = 0;
            Kepware.ClientAce.OpcDaClient.ItemIdentifier[] OPCItems = new Kepware.ClientAce.OpcDaClient.ItemIdentifier[2];
            Kepware.ClientAce.OpcDaClient.ItemValue[] OPCItemValues = null;

            OPCItems[0] = new Kepware.ClientAce.OpcDaClient.ItemIdentifier();
            OPCItems[0].ItemName = "its-iot-device.Device1.PlantStatus";
            OPCItems[0].ClientHandle = 1;

            OPCItems[1] = new Kepware.ClientAce.OpcDaClient.ItemIdentifier();
            OPCItems[1].ItemName = "Simulation Examples.Functions.Ramp1";
            OPCItems[1].ClientHandle = 2;

            label3.Text = OPCItems[0].ItemName;
            label6.Text = OPCItems[1].ItemName;

            try
            {
                daServerMgt.Read(maxAge, ref OPCItems, out OPCItemValues);

                if (OPCItems[0].ResultID.Succeeded & OPCItemValues[0].Quality.IsGood)
                {
                    label4.Text = OPCItemValues[0].Value.ToString();
                }
                else
                {
                    debugBox.Text += OPCItems[0].ResultID.Description;
                }

                if (OPCItems[1].ResultID.Succeeded & OPCItemValues[1].Quality.IsGood)
                {
                    label5.Text = OPCItemValues[1].Value.ToString();
                }
                else
                {
                    debugBox.Text += OPCItems[1].ResultID.Description;
                }
            }
            catch (Exception ex)
            {
                debugBox.Text += ex.ToString();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            AggiornaDati();
        }
    }
}
