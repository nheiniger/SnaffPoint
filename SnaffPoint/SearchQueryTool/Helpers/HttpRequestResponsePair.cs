using System;
using System.Net;

namespace SearchQueryTool.Helpers
{
    public class HttpRequestResponsePair : Tuple<HttpWebRequest, HttpWebResponse, string>
    {
        public HttpRequestResponsePair(HttpWebRequest request, HttpWebResponse response)
            : this(request, response, null)
        { }

        public HttpRequestResponsePair(HttpWebRequest request, HttpWebResponse response, string requestContent)
            : base(request, response, requestContent)
        { }
    }
}
