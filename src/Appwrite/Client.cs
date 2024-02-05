using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Appwrite.Converters;
using Appwrite.Extensions;
using Appwrite.Models;

namespace Appwrite
{
    public class Client
    {
        public string Endpoint => _endpoint;
        public Dictionary<string, string> Config => _config;

        private HttpClient _http;
        private readonly Dictionary<string, string> _headers;
        private readonly Dictionary<string, string> _config;
        private string _endpoint;

        private static readonly int ChunkSize = 5 * 1024 * 1024;

        public static JsonSerializerSettings DeserializerSettings { get; set; } = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy()),
                new ValueClassConverter()
            }
        };

        public static JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy()),
                new ValueClassConverter()
            }
        };

        public Client(
            string endpoint = "https://HOSTNAME/v1",
            bool selfSigned = false,
            HttpClient? http = null)
        {
            _endpoint = endpoint;
            _http = http ?? new HttpClient();
            _headers = new Dictionary<string, string>()
            {
                { "content-type", "application/json" },
                { "user-agent" , "AppwriteDotNetSDK/0.8.0-rc.1 (${Environment.OSVersion.Platform}; ${Environment.OSVersion.VersionString})"},
                { "x-sdk-name", ".NET" },
                { "x-sdk-platform", "server" },
                { "x-sdk-language", "dotnet" },
                { "x-sdk-version", "0.8.0-rc.1"},                { "X-Appwrite-Response-Format", "1.4.0" }
            };

            _config = new Dictionary<string, string>();

            if (selfSigned)
            {
                SetSelfSigned(true);
            }

            JsonConvert.DefaultSettings = () => DeserializerSettings;
        }

        public Client SetSelfSigned(bool selfSigned)
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true
            };

            _http = selfSigned
                ? new HttpClient(handler)
                : new HttpClient();

            return this;
        }

        public Client SetEndpoint(string endpoint)
        {
            _endpoint = endpoint;

            return this;
        }

        /// <summary>Your project ID</summary>
        public Client SetProject(string value) {
            _config.Add("project", value);
            AddHeader("X-Appwrite-Project", value);

            return this;
        }

        /// <summary>Your secret API key</summary>
        public Client SetKey(string value) {
            _config.Add("key", value);
            AddHeader("X-Appwrite-Key", value);

            return this;
        }

        /// <summary>Your secret JSON Web Token</summary>
        public Client SetJWT(string value) {
            _config.Add("jWT", value);
            AddHeader("X-Appwrite-JWT", value);

            return this;
        }

        public Client SetLocale(string value) {
            _config.Add("locale", value);
            AddHeader("X-Appwrite-Locale", value);

            return this;
        }

        /// <summary>The user session to authenticate with</summary>
        public Client SetSession(string value) {
            _config.Add("session", value);
            AddHeader("X-Appwrite-Session", value);

            return this;
        }

        /// <summary>The IP address of the client that made the request</summary>
        public Client SetForwardedFor(string value) {
            _config.Add("forwardedFor", value);
            AddHeader("X-Forwarded-For", value);

            return this;
        }

        /// <summary>The user agent string of the client that made the request</summary>
        public Client SetForwardedUserAgent(string value) {
            _config.Add("forwardedUserAgent", value);
            AddHeader("X-Forwarded-User-Agent", value);

            return this;
        }

        public Client AddHeader(string key, string value)
        {
            _headers.Add(key, value);

            return this;
        }

        public Task<Dictionary<string, object?>> Call(
            string method,
            string path,
            Dictionary<string, string> headers,
            Dictionary<string, object?> parameters)
        {
            return Call<Dictionary<string, object?>>(method, path, headers, parameters);
        }

        public async Task<T> Call<T>(
            string method,
            string path,
            Dictionary<string, string> headers,
            Dictionary<string, object?> parameters,
            Func<Dictionary<string, object>, T>? convert = null) where T : class
        {
            var methodGet = "GET".Equals(method, StringComparison.OrdinalIgnoreCase);

            var queryString = methodGet ?
                "?" + parameters.ToQueryString() :
                string.Empty;

            var request = new HttpRequestMessage(
                new HttpMethod(method),
                _endpoint + path + queryString);

            if ("multipart/form-data".Equals(
                headers["content-type"],
                StringComparison.OrdinalIgnoreCase))
            {
                var form = new MultipartFormDataContent();

                foreach (var parameter in parameters)
                {
                    if (parameter.Key == "file")
                    {
                        form.Add(((MultipartFormDataContent)parameters["file"]).First()!);
                    }
                    else if (parameter.Value is IEnumerable<object> enumerable)
                    {
                        var list = new List<object>(enumerable);
                        for (int index = 0; index < list.Count; index++)
                        {
                            form.Add(new StringContent(list[index].ToString()!), $"{parameter.Key}[{index}]");
                        }
                    }
                    else
                    {
                        form.Add(new StringContent(parameter.Value.ToString()!), parameter.Key);
                    }
                }
                request.Content = form;

            }
            else if (!methodGet)
            {
                string body = parameters.ToJson();

                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            foreach (var header in _headers)
            {
                if (header.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                {
                    _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header.Value));
                }
                else
                {
                    if (_http.DefaultRequestHeaders.Contains(header.Key)) {
                        _http.DefaultRequestHeaders.Remove(header.Key);
                    }
                    _http.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            foreach (var header in headers)
            {
                if (header.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(header.Value));
                }
                else if (header.Key.Equals("content-range", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                else
                {
                    if (request.Headers.Contains(header.Key)) {
                        request.Headers.Remove(header.Key);
                    }
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _http.SendAsync(request);
            var code = (int)response.StatusCode;
            var contentType = response.Content.Headers
                .GetValues("Content-Type")
                .FirstOrDefault() ?? string.Empty;

            var isJson = contentType.Contains("application/json");
            var isBytes = contentType.Contains("application/octet-stream");

            if (code >= 400) {
                var message = await response.Content.ReadAsStringAsync();

                if (isJson) {
                    message = JObject.Parse(message)["message"]!.ToString();
                }

                throw new AppwriteException(message, code);
            }

            if (isJson)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    responseString,
                    DeserializerSettings);

                if (convert != null)
                {
                    return convert(dict!);
                }

                return (dict as T)!;
            }
            else if (isBytes)
            {
                return ((await response.Content.ReadAsByteArrayAsync()) as T)!;
            }
            else
            {
                return default!;
            }
        }

        public async Task<T> ChunkedUpload<T>(
            string path,
            Dictionary<string, string> headers,
            Dictionary<string, object?> parameters,
            Func<Dictionary<string, object>, T> converter,
            string paramName,
            string? idParamName = null,
            Action<UploadProgress>? onProgress = null) where T : class
        {
            var input = parameters[paramName] as InputFile;
            var size = 0L;
            switch(input.SourceType)
            {
                case "path":
                    var info = new FileInfo(input.Path);
                    input.Data = info.OpenRead();
                    size = info.Length;
                    break;
                case "stream":
                    size = (input.Data as Stream).Length;
                    break;
                case "bytes":
                    size = ((byte[])input.Data).Length;
                    break;
            };

            var offset = 0L;
            var buffer = new byte[Math.Min(size, ChunkSize)];
            var result = new Dictionary<string, object?>();

            if (size < ChunkSize)
            {
                switch(input.SourceType)
                {
                    case "path":
                    case "stream":
                        await (input.Data as Stream).ReadAsync(buffer, 0, (int)size);
                        break;
                    case "bytes":
                        buffer = (byte[])input.Data;
                        break;
                }

                var content = new MultipartFormDataContent {
                    { new ByteArrayContent(buffer), paramName, input.Filename }
                };

                parameters[paramName] = content;

                return await Call(
                    method: "POST",
                    path,
                    headers,
                    parameters,
                    converter
                );
            }

            if (!string.IsNullOrEmpty(idParamName) && (string)parameters[idParamName] != "unique()")
            {
                // Make a request to check if a file already exists
                var current = await Call<Dictionary<string, object?>>(
                    method: "GET",
                    path: "$path/${params[idParamName]}",
                    headers,
                    parameters = new Dictionary<string, object?>()
                );
                var chunksUploaded = (long)current["chunksUploaded"];
                offset = chunksUploaded * ChunkSize;
            }

            while (offset < size)
            {
                switch(input.SourceType)
                {
                    case "path":
                    case "stream":
                        var stream = input.Data as Stream;
                        stream.Seek(offset, SeekOrigin.Begin);
                        await stream.ReadAsync(buffer, 0, ChunkSize);
                        break;
                    case "bytes":
                        buffer = ((byte[])input.Data)
                            .Skip((int)offset)
                            .Take((int)Math.Min(size - offset, ChunkSize - 1))
                            .ToArray();
                        break;
                }

                var content = new MultipartFormDataContent {
                    { new ByteArrayContent(buffer), paramName, input.Filename }
                };

                parameters[paramName] = content;

                headers["Content-Range"] =
                    $"bytes {offset}-{Math.Min(offset + ChunkSize - 1, size - 1)}/{size}";

                result = await Call<Dictionary<string, object?>>(
                    method: "POST",
                    path,
                    headers,
                    parameters
                );

                offset += ChunkSize;

                var id = result.ContainsKey("$id")
                    ? result["$id"]?.ToString() ?? string.Empty
                    : string.Empty;
                var chunksTotal = result.ContainsKey("chunksTotal")
                    ? (long)result["chunksTotal"]
                    : 0;
                var chunksUploaded = result.ContainsKey("chunksUploaded")
                    ? (long)result["chunksUploaded"]
                    : 0;

                headers["x-appwrite-id"] = id;

                onProgress?.Invoke(
                    new UploadProgress(
                        id: id,
                        progress: Math.Min(offset, size) / size * 100,
                        sizeUploaded: Math.Min(offset, size),
                        chunksTotal: chunksTotal,
                        chunksUploaded: chunksUploaded));
            }

            return converter(result);
        }
    }
}
