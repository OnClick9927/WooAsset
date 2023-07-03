using System.IO;
using System.Net;
using System;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class AssetsServer
        {
            private class GetOperation : AssetOperation
            {
                public override float progress { get { return isDone ? 1 : _progress; } }
                private float _progress = 0;
                public GetOperation(HttpListenerContext context, string folder)
                {
                    HttpListenerRequest request = context.Request;
                    var rawUrl = request.RawUrl;
                    string filePath = Path.Combine(folder, rawUrl.Remove(0, 1));
                    WriteToClient(filePath, context);
                }
                public void WriteToClient(string filePath, HttpListenerContext context)
                {
                    HttpListenerResponse response = context.Response;
                    var OutputStream = response.OutputStream;
                    if (File.Exists(filePath))
                    {
                        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                        int len = 0;
                        _progress = 0;
                        response.StatusCode = (int)HttpStatusCode.OK;
                        byte[] buffer = new byte[1024];

                        void EndWrite(IAsyncResult ar)
                        {
                            var num = fs.EndRead(ar);
                            len += num;
                            _progress = (float)len / fs.Length;
                            OutputStream.Write(buffer, 0, num);
                            if (num < 1)
                            {
                                fs.Close();
                                OutputStream.Close();
                                InvokeComplete();
                            }
                            else
                            {
                                fs.BeginRead(buffer, 0, buffer.Length, EndWrite, null);
                            }
                        }
                        fs.BeginRead(buffer, 0, buffer.Length, EndWrite, null);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        OutputStream.Close();
                        InvokeComplete();
                    }
                }

            }
            static HttpListener httpListener;
            private static string _dir;
            private static int _port;

            public static void Run(int port, string directory)
            {
                _dir = directory;
                _port = port;
                AssetsInternal.Log($"Server Start {_dir}:{_port}");
                httpListener = new HttpListener();
                httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                httpListener.Prefixes.Add(string.Format("http://*:{0}/", port));
                httpListener.Start();
                Receive();
            }
            private static async void Receive()
            {
                var context = await httpListener.GetContextAsync();
                await new GetOperation(context, _dir);
                Receive();
            }
            public static void Stop()
            {
                if (httpListener != null)
                {
                    httpListener?.Close();
                    httpListener = null;
                    AssetsInternal.Log($"Server Stop {_dir}:{_port}");
                }
            }
        }
    }

}
