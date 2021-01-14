using System;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;

namespace HC05CT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        //Richtextbox
        readonly FlowDocument mcFlowDoc = new FlowDocument();
        readonly Paragraph para = new Paragraph();

        //Serial 
        readonly SerialPort serial = new SerialPort();
        string recieved_data;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponent();
            //overwite to ensure state
            cmbPorts.ItemsSource = SerialPort.GetPortNames();
            Connect_btn.Content = "Connect";
        }

        private void Connect_Comms(object sender, RoutedEventArgs e)
        {
            switch (Connect_btn.Content)
            {
                case "Connect":
                    try
                    {
                        Analytics.TrackEvent("Connect");
                        //Sets up serial port
                        serial.PortName = cmbPorts.Text;
                        serial.BaudRate = Convert.ToInt32(38400);
                        serial.Handshake = Handshake.None;
                        serial.Parity = Parity.None;
                        serial.DataBits = 8;
                        serial.StopBits = StopBits.One;
                        serial.ReadTimeout = 200;
                        serial.WriteTimeout = 50;
                        serial.Open();

                        //Sets button State and Creates function call on data recieved
                        Connect_btn.Content = "Disconnect";
                        serial.DataReceived += new SerialDataReceivedEventHandler(Recieve);
                    }
                    catch (Exception exception)
                    {
                        Crashes.TrackError(exception);
                    }
                    break;
                default:
                    try // just in case serial port is not open could also be acheved using if(serial.IsOpen)
                    {
                        serial.Close();
                        Connect_btn.Content = "Connect";
                        Analytics.TrackEvent("Disconnect");
                    }
                    catch (Exception exception)
                    {
                        Crashes.TrackError(exception);
                    }
                    break;
            }
        }

        #region Recieving

        private delegate void UpdateUiTextDelegate(string text);
        private void Recieve(object sender, SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            recieved_data = serial.ReadExisting();
            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), recieved_data);
            Analytics.TrackEvent("Recieve");
        }
        private void WriteData(string text)
        {
            // Assign the value of the recieved_data to the RichTextBox.
            para.Inlines.Add(text);
            mcFlowDoc.Blocks.Add(para);
            Commdata.Document = mcFlowDoc;
        }

        #endregion

        #region Sending        

        private void Send_Data(object sender, RoutedEventArgs e)
        {
            SerialCmdSend("AT+UART=9600,0,0\r\n");
            Analytics.TrackEvent("Send Data");
        }
        private void Verify_Data(object sender, RoutedEventArgs e)
        {
            SerialCmdSend("AT+UART?\r\n");
            Analytics.TrackEvent("Verify Data");
        }
        public void SerialCmdSend(string data)
        {
            if (serial.IsOpen)
            {
                try
                {
                    // Send the binary data out the port
                    byte[] hexstring = Encoding.ASCII.GetBytes(data);
                    //There is a intermitant problem that I came across
                    //If I write more than one byte in succesion without a 
                    //delay the PIC i'm communicating with will Crash
                    //I expect this id due to PC timing issues ad they are
                    //not directley connected to the COM port the solution
                    //Is a ver small 1 millisecound delay between chracters
                    foreach (byte hexval in hexstring)
                    {
                        byte[] _hexval = new byte[] { hexval }; // need to convert byte to byte[] to write
                        serial.Write(_hexval, 0, 1);
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    para.Inlines.Add("Failed to SEND" + data + "\n" + ex + "\n");
                    mcFlowDoc.Blocks.Add(para);
                    Commdata.Document = mcFlowDoc;
                    Crashes.TrackError(ex);
                }
            }
        }
        #endregion
    }
}
