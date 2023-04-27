using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    public class TCPServer
    {
        //マニュアルリセットイベントのインスタンスを生成
        public ManualResetEvent allDone = new ManualResetEvent(false);

        StateObject? state = null;

        public async void Start(int port)
        {
            await StartListening(port);
        }

        //TCP/IPの接続開始処理
        public async Task<bool> StartListening(int port)
        {
            // IPアドレスとポート番号を指定して、ローカルエンドポイントを設定
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            // TCP/IPのソケットを作成
            Socket TcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                TcpServer.Bind(localEndPoint);  // TCP/IPのソケットをローカルエンドポイントにバインド
                TcpServer.Listen(10);           // 待ち受け開始

                await Task.Run(()=>
                {
                    while (true)
                    {
                        // シグナルの状態をリセット
                        allDone.Reset();

                        // 非同期ソケットを開始して、接続をリッスンする
                        Debug.WriteLine("接続待機中...");
                        TcpServer.BeginAccept(new AsyncCallback(AcceptCallback), TcpServer);

                        // シグナル状態になるまで待機
                        allDone.WaitOne();
                    }
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return false;
        }
        public Action<object> WriteLog { get; set; }

        public void AcceptCallback(IAsyncResult ar)
        {
            // シグナル状態にし、メインスレッドの処理を続行する
            allDone.Set();
            WriteLog("TCP Cilent 接続");
            // クライアント要求を処理するソケットを取得
            Socket TcpServer = (Socket)ar.AsyncState;
            Socket TcpClient = TcpServer.EndAccept(ar);

            // 端末からデータ受信を待ち受ける
            state = new StateObject();
            state.workSocket = TcpClient;
            TcpClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        
        }

        public void SendMessage(byte[] message)
        {
            if (state != null && state.workSocket != null)
            {
                Socket TcpClient = state.workSocket;

                // クライアントへデータの送信を開始
                TcpClient.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendCallback), TcpClient);

            }
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            var content = string.Empty;

            try
            {
                // 非同期オブジェクトからソケット情報を取得
                StateObject state = (StateObject)ar.AsyncState;
                Socket TcpClient = state.workSocket;

                // クライアントソケットからデータを読み取り
                int bytesRead = TcpClient.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // 受信したデータを蓄積
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // 蓄積データの終端タグを確認
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // 終了タグ<EOF>があれば、読み取り完了
                        Debug.WriteLine(string.Format("クライアントから「{0}」を受信", content));

                        // ASCIIコードをバイトデータに変換
                        byte[] byteData = Encoding.ASCII.GetBytes("OK");

                        // クライアントへデータの送信を開始
                        TcpClient.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), TcpClient);
                    }
                    else
                    {
                        // 取得していないデータがあるので、受信再開
                        TcpClient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[" + DateTime.Now + "] " + ex.Message);

            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // 非同期オブジェクトからソケット情報を取得
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                Socket TcpClient = (Socket)ar.AsyncState;
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。

                // クライアントへデータ送信完了
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                int bytesSent = TcpClient.EndSend(ar);
#pragma warning restore CS8602 // null 参照の可能性があるものの逆参照です。
                
                Console.WriteLine("[" + DateTime.Now + "] " + "Messageをクライアントへ送信");
                
                //ソケット通信を終了
                Debug.WriteLine("接続終了");
                //TcpClient.Shutdown(SocketShutdown.Both);
                //TcpClient.Close();

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        
    }
    // 非同期処理でソケット情報を保持する為のオブジェクト
    public class StateObject
    {
        // 受信バッファサイズ
        public const int BufferSize = 1024;

        // 受信バッファ
        public byte[] buffer = new byte[BufferSize];

        // 受信データ
        public StringBuilder sb = new StringBuilder();

        // ソケット
        public Socket? workSocket = null;
    }
}
