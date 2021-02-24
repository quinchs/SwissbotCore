using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP
{
    public class Route : Attribute
    {
        internal string _Route { get; private set; }
        internal string _Method { get; private set; }
        internal bool _Regex { get; private set; } = false;

        /// <summary>
        /// Creates a new Route
        /// </summary>
        /// <param name="Route">The Route to this api function.</param>
        public Route(string Route, string Method)
        {
            _Route = Route;
            _Method = Method;
        }
        public Route(string Route, string Method, bool Regex)
        {
            _Route = Route;
            _Method = Method;
            _Regex = Regex;
        }
    }

    class HttpHandler
    {
        private class HttpRoute
        {
            public string Route { get; set; }
            public string HttpMethod { get; set; }
            public bool isRegex { get; set; }
            public MethodInfo Method { get; set; }
        }
        private HashSet<HttpRoute> Routes { get; set; } = new HashSet<HttpRoute>();
        private HttpListener listener;
        public HttpHandler(HttpListener l)
        {
            listener = l;

            // Get our routes
            var types = Assembly.GetEntryAssembly().GetTypes();

            var methods = types.SelectMany(x => 
                x.GetMethods().Where(y => 
                    y.GetCustomAttributes(typeof(Route), false).Length > 0)).ToArray();

            foreach (var method in methods)
            {
                var att = method.GetCustomAttribute<Route>();

                var r = new HttpRoute()
                {
                    Route = att._Route,
                    Method = method,
                    HttpMethod = att._Method,
                    isRegex = att._Regex
                };

                Routes.Add(r);
            }
        }

        public async Task ExecuteAsync(HttpListenerContext context)
        {
            // Check our routes
            string Path = context.Request.RawUrl.Replace("/apprentice/v1", "");

            var route = Routes.FirstOrDefault(x => 
                x.isRegex ? Regex.IsMatch(Path, x.Route) && x.HttpMethod == context.Request.HttpMethod 
                : x.Route == Path && x.HttpMethod == context.Request.HttpMethod);
            
            if (route != null)
            {
                Task task;
                if (route.isRegex)
                {
                    MatchCollection mtch = Regex.Matches(Path, route.Route);
                    task = (Task)route.Method.Invoke(null, new object[] { context, mtch});
                }
                else
                    task = (Task)route.Method.Invoke(null, new object[] { context });

                await task;

                if(task.Exception != null)
                {
                    Global.ConsoleLog($"Failed route: {task.Exception}");
                };
            }
        }
    }
}
