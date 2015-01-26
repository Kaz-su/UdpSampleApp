using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UdpSampleApp
{
    public partial class Form1 : Form
    {
        UDPClientManager udpMgr = null;

        /// <summary>
        /// TextBoxにUDPのEventを追加するdelegate
        /// </summary>
        delegate void AppendUdpEventDelegate(UDPDataReceivedEventArgs e);

        public Form1()
        {
            InitializeComponent(); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void openButton_Click(object sender, EventArgs e)
        {
            //既にUDPManagerクラスが生成されていたら何もしない
            if (udpMgr != null)
            {
                return;
            }
            udpMgr = new UDPClientManager(Int32.Parse(portTextBox.Text));
            udpMgr.ReceiveEvent += new UDPClientManager.UDPDataReceivedEventHandler(OnUDPDataReceived);
            udpMgr.Open();
            logTextBox.AppendText("Opened." + Environment.NewLine);
            sendButton.Enabled = true;
        }

        private void OnUDPDataReceived(object sender, UDPDataReceivedEventArgs e)
        {
            Invoke(new AppendUdpEventDelegate(AppendUdpEvent), e);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            //既に破棄されていたら何もしない
            if (udpMgr == null)
            {
                return;
            }
            udpMgr.Close();
            udpMgr = null;  //破棄
            logTextBox.AppendText("Closed." + Environment.NewLine);
            sendButton.Enabled = false;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            string msg = sendTextBox.Text;
            //msgが空、もしくはUDPクラスが生成されていなかったなら何もしない
            if (msg == "" || udpMgr == null)
            {
                return;
            }
            string[] sendToStr = sendtoTextBox.Text.Split(':');
            udpMgr.Write(msg, sendToStr[0], Int32.Parse(sendToStr[1]));
            logTextBox.AppendText("send : " + msg + Environment.NewLine);
            sendTextBox.Text = "";
        }

        /// <summary>
        /// logTextBoxにUDPのEvent内容を追加する
        /// </summary>
        /// <param name="e"></param>
        void AppendUdpEvent(UDPDataReceivedEventArgs e)
        {
            logTextBox.AppendText(e.Address + " : " + e.Message + Environment.NewLine);
        }
    }
}
