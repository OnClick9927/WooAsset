using System.Net;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class AssetsServer
        {
            private class GetOperation : AssetOperation
            {
                public override float progress { get { return isDone ? 1 : 0; } }
                public GetOperation(HttpListenerContext context, string folder)
                {
                    HttpListenerRequest request = context.Request;
                    var rawUrl = request.RawUrl;
                    string filePath = AssetsHelper.CombinePath(folder, rawUrl.Remove(0, 1));
                    WriteToClient(filePath, context);
                }
                public async void WriteToClient(string filePath, HttpListenerContext context)
                {
                    HttpListenerResponse response = context.Response;
                    var OutputStream = response.OutputStream;
                    if (AssetsHelper.ExistsFile(filePath))
                    {
                        response.StatusCode = (int)HttpStatusCode.OK;
                        await AssetsHelper.WriteStream(filePath, OutputStream);
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

            public static void Run(int port, string directory)
            {
                _dir = directory;
                _port = port;
                AssetsHelper.Log($"Server Start {_dir}:{_port}");
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
                    AssetsHelper.Log($"Server Stop {_dir}:{_port}");
                }
            }
        }
    }

}
