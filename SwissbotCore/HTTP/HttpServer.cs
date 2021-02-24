using Newtonsoft.Json;
using SwissbotCore.HTTP.Websocket;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace SwissbotCore.HTTP
{
    public class HttpServer
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private HttpHandler _handler;
        private event EventHandler<HttpListenerContext> requestEvent;
        public HttpServer(int port)
        {
            _listener = new HttpListener();
#if DEBUG
            _listener.Prefixes.Add($"http://localhost:{port}/apprentice/v1/");
#else
                    _listener.Prefixes.Add($"http://*:{port}/apprentice/v1/");
#endif

            _handler = new HttpHandler(_listener);

            requestEvent += HttpServer_requestEvent;

            _listenerThread = new Thread(ListenerLoop);

            _listener.Start();

            _listenerThread.Start();

            Global.ConsoleLog("Running init thread");
        }

        private async void HttpServer_requestEvent(object sender, HttpListenerContext context)
        {
            if (context.Request.IsWebSocketRequest)
            {
                // Handle sepratly
                if (context.Request.RawUrl != "/apprentice/v1/socket")
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }

                Global.ConsoleLog("Upgrading a socket");
                var sock = await context.AcceptWebSocketAsync(null);
                WebSocketServer.AcceptSocket(sock);
                return;
            }
            else
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "*");

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Global.ConsoleLog($"Got new request on {context.Request.RawUrl}");
                    var sw = Stopwatch.StartNew();

                    using (var response = context.Response)
                    {
                        _handler.ExecuteAsync(context).GetAwaiter().GetResult();

                        sw.Stop();

                        var u = context.GetUsername();
                        Global.ConsoleLog($"Executed an http request to {context.Request.RawUrl} in {sw.ElapsedMilliseconds}ms {(u != null ? $"for {u}" : "")}");
                    }
                });
            }
        }
        public async void ListenerLoop()
        {
            while (_listener.IsListening)
            {
                requestEvent?.Invoke(this, await _listener.GetContextAsync());
            }
        }
    }
}
