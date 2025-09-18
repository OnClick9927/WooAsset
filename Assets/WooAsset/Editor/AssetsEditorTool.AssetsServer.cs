using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        class AssetsServer
        {
            private static void ServerLog(string msg)
            {
                UnityEngine.Debug.Log($"<color=#0FA>AssetsServer: </color>{msg}");
            }
            private class GetOperation : Operation
            {
                public override float progress { get { return isDone ? 1 : 0; } }
                public GetOperation(HttpListenerContext context, string folder)
                {
                    HttpListenerRequest request = context.Request;
                    var rawUrl = request.RawUrl.Remove(0, 1);

                    string filePath = AssetsEditorTool.CombinePath(folder, rawUrl);
                    ServerLog($"Client DownLoad {rawUrl}");
                    WriteToClient(filePath, context);
                }
                public async void WriteToClient(string filePath, HttpListenerContext context)
                {
                    HttpListenerResponse response = context.Response;
                    var OutputStream = response.OutputStream;
                    if (AssetsEditorTool.ExistsFile(filePath))
                    {
                        response.StatusCode = (int)HttpStatusCode.OK;
                        using (FileStream fs = File.OpenRead(filePath))
                        {
                            byte[] buffer = new byte[fs.Length];
                            await fs.ReadAsync(buffer, 0, buffer.Length);


                            int gap = _speed;
                            var start = 0;
                            while (start < buffer.Length)
                            {
                                var len = Mathf.Min(gap, buffer.Length - start);
                                OutputStream.Write(buffer, start, len);
                                start += len;
                                if (start < buffer.Length)
                                {
                                    await Task.Delay(1000);
                                }
                            }
                            //await OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    OutputStream.Close();
                    InvokeComplete();
                }

            }
            static HttpListener httpListener;
            private static string _dir;
            private static int _port;
            private static int _speed;
            public static void Run(int port, string directory,int speed)
            {
                _speed = speed;
                _dir = directory;
                _port = port;
                ServerLog($"Server Start {_dir}:{_port}");
                httpListener = new HttpListener();

                httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                httpListener.Prefixes.Add(string.Format("http://*:{0}/", port));
                httpListener.Start();
                Receive();
            }
            private static async void Receive()
            {
                try
                {
                    var context = await httpListener.GetContextAsync();
                    await new GetOperation(context, _dir);
                    Receive();
                }
                catch (System.Exception)
                {

                }

            }
            public static void Stop()
            {
                if (httpListener != null)
                {
                    httpListener?.Close();
                    httpListener = null;
                    ServerLog($"Server Stop {_dir}:{_port}");
                }
            }
        }
    }

}
