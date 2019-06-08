using System;
using System.Web.Http;
using System.Linq;
using Newtonsoft.Json.Linq;
using Owin;
using Apache.Geode.Client;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace cloudcache_ownin_example
{
    class Program
    {
        static void Main(string[] args)
        {
            string port = Environment.GetEnvironmentVariable("PORT");
            if (port == null)
            {
                port = "9000";
            }
            using (Microsoft.Owin.Hosting.WebApp.Start<Startup>("http://*:" + port))
            {
                Console.WriteLine("Press [enter] to quit...");
                Console.ReadLine();
            }
        }
    }
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}"
            );
            appBuilder.UseWebApi(config);
        }
    }
    static class Constants
    {
        public const string jsonPathLocators = "$.p-cloudcache[0].credentials.locators";
        public const string jsonPathPassword = "$.p-cloudcache[0].credentials.users[?(@.roles[*] == 'developer')].password";
        public const string jsonPathUsername = "$.p-cloudcache[0].credentials.users[?(@.roles[*] == 'developer')].username";
    }
    public class BookController : ApiController
    {
        //Made this class varibles since the controller gets instanciated on each request and I wanted to resuse the connection pool  
        //
        private static IRegion<string, Book> region = null;
        private static Object monitor = new Object();

        public BookController()
        {
            if (region == null)
            {
                Monitor.Enter(monitor);
                try
                {
                    if (region == null)
                    {
                        ConnectToCloudCache();
                    }
                }
                finally
                {
                    Monitor.Exit(monitor);
                }
            }
        }
        private void ConnectToCloudCache()
        {
            JObject vcapJson = JObject.Parse(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            Cache cache = new CacheFactory()
                .SetAuthInitialize(
                new UsernamePassword(
                    (string)vcapJson.SelectToken(Constants.jsonPathUsername),
                    (string)vcapJson.SelectToken(Constants.jsonPathPassword)))
                .Create();
            cache.TypeRegistry.PdxSerializer = new ReflectionBasedAutoSerializer();

            PoolFactory pool = cache.GetPoolFactory();
            foreach (string locator in vcapJson.SelectToken(Constants.jsonPathLocators).Select(s => (string)s).ToArray())
            {
                string[] hostPort = locator.Split('[', ']');
                pool.AddLocator(hostPort[0], Int32.Parse(hostPort[1]));
            }
            pool.Create("pool");

            region = cache.CreateRegionFactory(RegionShortcut.PROXY)
                .SetPoolName("pool")
                .Create<string, Book>("owinexample");
        }
        public string[] GetAll()
        {
            string[] keys =  region.Keys.ToArray();
            return keys;
        }
        public Book Get(string isbn)
        {
            return region[isbn];
        }
        public HttpResponseMessage Post([FromBody]Book[] books)
        {
            if (books != null && books.Length > 0)
            {
                Dictionary<string, Book> bulk = new Dictionary<string, Book>();
                foreach (Book currBook in books)
                {
                    region[currBook.ISBN] = currBook;
                }
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        public HttpResponseMessage Put([FromBody]Book value)
        {
            region[value.ISBN] = value;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        public HttpResponseMessage Delete(string isbn)
        {
            region.Remove(isbn);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        public HttpResponseMessage DeleteAll()
        {
            foreach(String key in region.Keys)
            {
                region.Remove(key);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
