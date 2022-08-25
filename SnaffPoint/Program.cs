using SearchQueryTool.Helpers;
using SearchQueryTool.Model;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

namespace SnaffPoint
{
    class Program
    {
        private static string PresetPath = "./presets";
        private static int MaxRows = 50;
        private static string SingleQueryText = null;
        private static SearchPresetList _SearchPresets;
        private static string BearerToken = null;
        private static string SPUrl = null;
        private static bool isFQL = false;
        private static string RefinementFilters = null;

        private static void LoadSearchPresetsFromFolder(string presetFolderPath)
        {
            try
            {
                _SearchPresets = new SearchPresetList(presetFolderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read search presets. Error: " + ex.Message);
            }
        }

        private static SearchQueryResult StartSearchQueryRequest(SearchQueryRequest request)
        {
            SearchQueryResult searchResults = null;
            try
            {
                HttpRequestResponsePair requestResponsePair = HttpRequestRunner.RunWebRequest(request);
                if (requestResponsePair != null)
                {
                    HttpWebResponse response = requestResponsePair.Item2;
                    if (null != response)
                    {
                        if (!response.StatusCode.Equals(HttpStatusCode.OK))
                        {
                            string status = String.Format("HTTP {0} {1}", (int)response.StatusCode, response.StatusDescription);
                            Console.WriteLine("Request returned with following status: " + status);
                        }
                    }
                }
                searchResults = GetResultItem(requestResponsePair);

                // success, return the results
                return searchResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Request failed with exception: " + ex.Message);
            }
            return searchResults;
        }

        private static SearchQueryResult GetResultItem(HttpRequestResponsePair requestResponsePair)
        {
            SearchQueryResult searchResults;
            var request = requestResponsePair.Item1;

            using (var response = requestResponsePair.Item2)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    NameValueCollection requestHeaders = new NameValueCollection();
                    foreach (var header in request.Headers.AllKeys)
                    {
                        requestHeaders.Add(header, request.Headers[header]);
                    }

                    NameValueCollection responseHeaders = new NameValueCollection();
                    foreach (var header in response.Headers.AllKeys)
                    {
                        responseHeaders.Add(header, response.Headers[header]);
                    }

                    string requestContent = "";
                    if (request.Method == "POST")
                    {
                        requestContent = requestResponsePair.Item3;
                    }

                    searchResults = new SearchQueryResult
                    {
                        RequestUri = request.RequestUri,
                        RequestMethod = request.Method,
                        RequestContent = requestContent,
                        ContentType = response.ContentType,
                        ResponseContent = content,
                        RequestHeaders = requestHeaders,
                        ResponseHeaders = responseHeaders,
                        StatusCode = response.StatusCode,
                        StatusDescription = response.StatusDescription,
                        HttpProtocolVersion = response.ProtocolVersion.ToString()
                    };
                    searchResults.Process();
                }
            }
            return searchResults;
        }

        static void QueryAllPresets()
        {
            LoadSearchPresetsFromFolder(PresetPath);

            if (_SearchPresets.Presets.Count > 0)
            {
                foreach (var preset in _SearchPresets.Presets)
                {
                    Console.WriteLine("\n" + preset.Name + "\n" + new String('=', preset.Name.Length) + "\n");
                    preset.Request.Token = BearerToken;
                    preset.Request.SharePointSiteUrl = SPUrl;
                    preset.Request.RowLimit = MaxRows;
                    preset.Request.AcceptType = AcceptType.Json;
                    preset.Request.AuthenticationType = AuthenticationType.SPOManagement; // force to JWT auth method
                    // Console.WriteLine("DEBUG - Request: " + preset.Request.ToString());
                    SearchQueryResult results = StartSearchQueryRequest(preset.Request);
                    DisplayResults(results);
                }
            }
            else
            {
                Console.WriteLine("No presets were found in " + PresetPath);
            }
        }

        private static void DisplayResults(SearchQueryResult results)
        {
            if (results != null)
            {
                if (results.PrimaryQueryResult != null)
                {
                    Console.WriteLine("Found " + results.PrimaryQueryResult.TotalRows + " results");
                    if (results.PrimaryQueryResult.TotalRows > MaxRows)
                    {
                        Console.WriteLine("Only showing " + MaxRows + " results, though!");
                    }        
                    if (results.PrimaryQueryResult.TotalRows > 0)
                    {
                        foreach (ResultItem item in results.PrimaryQueryResult.RelevantResults)
                        {
                            Console.WriteLine("---");
                            Console.WriteLine(item.Title);
                            Console.WriteLine(item.Path);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Found no results... maybe the request failed?");
                }
            }
            else
            {
                Console.WriteLine("Result are null ! What happened there?");
            }
        }

        private static void DoSingleQuery()
        {
            // preparing the request for you
            SearchQueryRequest request = new SearchQueryRequest
            {
                SharePointSiteUrl = SPUrl,
                AcceptType = AcceptType.Json,
                Token = BearerToken,
                AuthenticationType = AuthenticationType.SPOManagement,
                QueryText = SingleQueryText,
                HttpMethodType = HttpMethodType.Get,
                EnableFql = isFQL,
                RowLimit = MaxRows
            };
            if (RefinementFilters != null)
            {
                request.RefinementFilters = RefinementFilters;
            }
            // DO IT, DO IT, DO IT !
            SearchQueryResult results = StartSearchQueryRequest(request);
            DisplayResults(results);
        }

        static void PrintHelp()
        {
            Console.WriteLine(
@"
  .dBBBBP   dBBBBb dBBBBBb     dBBBBP dBBBBP dBBBBBb  dBBBBP dBP dBBBBb dBBBBBBP
  BP           dBP      BB                       dB' dBP.BP         dBP         
  `BBBBb  dBP dBP   dBP BB   dBBBP  dBBBP    dBBBP' dBP.BP dBP dBP dBP   dBP    
     dBP dBP dBP   dBP  BB  dBP    dBP      dBP    dBP.BP dBP dBP dBP   dBP     
dBBBBP' dBP dBP   dBBBBBBB dBP    dBP      dBP    dBBBBP dBP dBP dBP   dBP      

                           https://github.com/nheiniger/snaffpoint
                                               
SnaffPoint, candy finder for SharePoint

Usage: SnaffPoint.exe -u URL -t JWT [OPTIONS]

-h, --help              This is me :)

Mandatory:
-u, --url               SharePoint online URL where you want to search
-t, --token             Bearer token that grants access to said SharePoint

Common options:
-m, --max-rows          Max. number of rows to return per search query (default is 50)

Presets mode (default):
-p, --preset            Path to a folder containing XML search presets (default is ./presets)

Single query mode:
-q, --query             Query search string
-l, --fql               Enables FQL (default is KQL)
-r, --refinement-filter Adds a refinement filter");
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            foreach (var entry in args.Select((value, index) => new { index, value }))
            {
                switch (entry.value)
                {
                    // do you want FQL powaa?
                    case "-l":
                    case "--fql":
                        isFQL = true;
                        break;
                    // no need for hundreds of results
                    case "-m":
                    case "--max-rows":
                        if (args[entry.index + 1].StartsWith("-"))
                        {
                            PrintHelp();
                        }
                        if (! int.TryParse(args[entry.index + 1], out MaxRows))
                        {
                            PrintHelp();
                        }
                        break;
                    // preset path, load presets
                    case "-p":
                    case "--preset":
                        if (args[entry.index + 1].StartsWith("-"))
                        {
                            PrintHelp();
                        }
                        PresetPath = args[entry.index + 1];
                        break;
                    // single query
                    case "-q":
                    case "--query":
                        if (args[entry.index + 1].StartsWith("-"))
                        {
                            PrintHelp();
                        }
                        SingleQueryText = args[entry.index + 1];
                        break;
                    // fine control is good :)
                    case "-r":
                    case "--refinement-filter":
                        if (args[entry.index + 1].StartsWith("-"))
                        {
                            PrintHelp();
                        }
                        RefinementFilters = args[entry.index + 1];
                        break;
                    // Bearer token (JWT)
                    case "-t":
                    case "--token":
                        if (args[entry.index + 1].StartsWith("-"))
                        {
                            PrintHelp();
                        }
                        BearerToken = "Bearer " + args[entry.index + 1];
                        break;
                    // SharePoint online URL
                    case "-u":
                    case "--url":
                        if (args[entry.index + 1].StartsWith("-"))
                        {
                            PrintHelp();
                        }
                        SPUrl = args[entry.index + 1];
                        break;
                    // send help
                    case "-h":
                    case "--help":
                        PrintHelp();
                        break;
                }
            }

            // did you read the doc?
            if (SPUrl == null || BearerToken == null)
            {
                PrintHelp();
            }

            // if you specify a query I assume you want an answer, otherwise I have some defaults
            if (SingleQueryText != null)
            {
                DoSingleQuery();
            }
            else
            {
                QueryAllPresets();
            }
        }
    }
}
