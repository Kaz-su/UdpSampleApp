using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UdpSampleApp
{
    /// <summary>
    /// UDP/IPクライアント管理クラス
    /// </summary>
    public class UDPClientManager : IDisposable
    {
        /// <summary>
        /// The _disposed
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// ポートが開かれている場合true
        /// </summary>
        public bool IsOpen = false;

        /// <summary>
        /// UDP/IPクライアント
        /// </summary>
        private UdpState udpState = null;

        /// <summary>
        /// 送受信用エンコード
        /// </summary>
        private Encoding encoding = Encoding.ASCII;

        /// <summary>
        /// ローカルポート番号
        /// </summary>
        public readonly int LocalPort;

        /// <summary>
        /// リモートホスト名
        /// </summary>
        public readonly string RemoteHost;

        //デリゲートの宣言
        public delegate void UDPDataReceivedEventHandler(object sender, UDPDataReceivedEventArgs e);
        public event UDPDataReceivedEventHandler ReceiveEvent;

        /// <summary>
        /// データ受信イベント
        /// </summary>
        protected virtual void OnUDPDataReceived(UDPDataReceivedEventArgs e)
        {
            if (ReceiveEvent != null)
            {
                ReceiveEvent(this, e);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UDPClientManager(int localPort)
        {
            udpState = new UdpState();

            LocalPort = localPort;
        }

        /// <summary>
        /// ポートをオープンします。
        /// </summary>
        public void Open()
        {
            //既にOpenされていたら何もしない
            if (IsOpen)
            {
                return;
            }

            // オープン処理
            try
            {
                //udpState.client.Connect(RemoteHost, LocalPort);
                udpState.endPoint = new IPEndPoint(IPAddress.Any, LocalPort);
                udpState.client = new UdpClient(LocalPort);
                IsOpen = true;
                udpState.client.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// ソケット受信時Callback
        /// </summary>
        /// <param name="AR"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            //IAsysncResultからUDPClientとIPEndPointを受け取る
            UdpClient uc = ((UdpState)(ar.AsyncState)).client;
            IPEndPoint iep = ((UdpState)(ar.AsyncState)).endPoint;

            try
            {
                Byte[] receivedBytes = uc.EndReceive(ar, ref iep);
                string receivedStr = Encoding.ASCII.GetString(receivedBytes);
                UDPDataReceivedEventArgs args = new UDPDataReceivedEventArgs();
                args.Address = iep.Address;
                args.PortNumber = iep.Port;
                args.Message = receivedStr;
                OnUDPDataReceived(args);

                //受信待ち再開
                udpState.client.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// ポートを閉じます。
        /// </summary>
        public void Close()
        {
            // 切断処理
            try
            {
                udpState.client.Close();
                IsOpen = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// データを送信します。
        /// </summary>
        public void Write(string msg, string remoteHost, int remotePort)
        {
            try
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(msg);
                udpState.client.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// アンマネージ リソースの解放およびリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Dispose() によるリソースの解放後のため、GCでの解放が必要が無いことを通知
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UDPClientManager"/> class.
        /// </summary>
        ~UDPClientManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Dispose がまだ実行されていないときだけ実行
            if (!_disposed)
            {
                // disposing が true の場合(Dispose() が実行された場合)は
                // マネージリソースも解放します。
                if (disposing)
                {
                    // マネージリソース解放処理
                }

                // アンマネージリソース解放処理

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// 受信データ
    /// </summary>
    public class UDPDataReceivedEventArgs : EventArgs
    {
        public IPAddress Address;
        public int PortNumber;
        public string Message;
    }

    /// <summary>
    /// UDPクライアント拡張クラス
    /// </summary>
    public class UdpState
    {
        public IPEndPoint endPoint;
        public UdpClient client;
    }
}
