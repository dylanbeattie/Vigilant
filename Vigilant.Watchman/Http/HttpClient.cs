using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Vigilant.Watchman.Http {
    public class HttpClient : IHttpClient {

        private static Stream OpenStream(Socket socket, bool useSsl) {
            if (useSsl) {
                var stream = new SslStream(new NetworkStream(socket), false, ValidateCertificate);
                stream.AuthenticateAsClient(String.Empty);
                return (stream);
            }
            return (new NetworkStream(socket));
        }

        public string Retrieve(string ipAddress, int port, string httpRequest, bool useSsl) {
            var endpoint = CreateEndpoint(ipAddress, port);
            using (var socket = ConnectSocket(endpoint)) {
                using (var stream = OpenStream(socket, useSsl)) {
                    return (ReadStream(httpRequest, stream));
                }
            }
        }

        private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return (true);
        }

        public void Dispose() {
            // Cleanup code goes here.
        }

        public const int CR = 13;
        public const int LF = 10;
        protected static readonly Encoding DefaultEncoding = Encoding.ASCII;

        protected static string ReadLine(Stream stream) {
            var lineBuffer = new List<byte>();
            while (true) {
                var b = stream.ReadByte();
                if (b == -1) return null;
                if (b == LF) break;
                if (b != CR) lineBuffer.Add((byte)b);
            }
            return DefaultEncoding.GetString(lineBuffer.ToArray());
        }

        protected static IPEndPoint CreateEndpoint(string ipAddress, int port) {
            var address = ipAddress.Split('.').Select(Byte.Parse).ToArray();
            return (new IPEndPoint(new IPAddress(address), port));
        }

        protected static Socket ConnectSocket(IPEndPoint endpoint) {
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) {SendTimeout = 5000, ReceiveTimeout = 5000};
            socket.Connect(endpoint);
            return (socket);
        }

        protected Dictionary<string, List<string>> ReadHttpHeaders(Stream stream) {
            var headers = new Dictionary<string, List<string>>();
            while (true) {
                var line = ReadLine(stream);
                if (line.Length == 0) break;
                var index = line.IndexOf(':');
                var headerName = line.Substring(0, index);
                var headerValue = line.Substring(index + 2);
                if (!headers.ContainsKey(headerName)) headers.Add(headerName, new List<string>());
                headers[headerName].Add(headerValue);
            }
            return (headers);
        }

        protected string ReadEncodedStream(String encoding, Stream stream) {
            switch (encoding) {
                case "gzip": stream = new GZipStream(stream, CompressionMode.Decompress); break;
                case "deflate": stream = new DeflateStream(stream, CompressionMode.Decompress); break;
            }
            return (ReadNetworkStream(stream));
        }

        protected static string ReadNetworkStream(Stream stream, int contentLength) {
            var bytesToRead = contentLength;
            var buffer = new byte[bytesToRead];
            var offset = 0;
            while (bytesToRead > 0) {
                var bytesReadThisTime = stream.Read(buffer, offset, bytesToRead);
                offset += bytesReadThisTime;
                bytesToRead -= bytesReadThisTime;
            }
            return (DefaultEncoding.GetString(buffer));
        }

        protected static string ReadNetworkStream(Stream stream) {
            using (var memoryStream = new MemoryStream()) {
                var buffer = new byte[4096];
                try {
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0) memoryStream.Write(buffer, 0, bytesRead);
                } finally {
                    stream.Close();
                }
                return (DefaultEncoding.GetString(memoryStream.ToArray()));
            }
        }

        protected static string ReadChunkedStream(Stream stream) {
            var sb = new StringBuilder();
            while (true) {
                // Chunked encoding consists of a line specifying the number of bytes, then a \r\n, then the bytes, then another \r\n
                var line = ReadLine(stream);
                int bytesToRead;
                if (!Int32.TryParse(line, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytesToRead)) break;
                if (bytesToRead <= 0) break;
                sb.Append(ReadNetworkStream(stream, bytesToRead));
                if (stream.ReadByte() != '\r' || stream.ReadByte() != '\n') throw (new Exception("Invalid chunk terminator"));
            }
            return (sb.ToString());
        }

        protected string ReadStream(string httpRequest, Stream stream) {
            while (!httpRequest.EndsWith("\r\n\r\n")) httpRequest += "\r\n";
            var data = DefaultEncoding.GetBytes(httpRequest);
            stream.Write(data, 0, data.Length);
            var status = ReadLine(stream);
            var headers = ReadHttpHeaders(stream);

            string contentEncoding = null;
            string transferEncoding = null;
            var contentLength = 0;
            if (headers.ContainsKey("Content-Encoding")) contentEncoding = headers["Content-Encoding"][0];
            if (headers.ContainsKey("Transfer-Encoding")) transferEncoding = headers["Transfer-Encoding"][0];
            if (headers.ContainsKey("Content-Length")) Int32.TryParse(headers["Content-Length"][0], out contentLength);

            string responseBody;
            if (!String.IsNullOrWhiteSpace(contentEncoding)) {
                responseBody = ReadEncodedStream(contentEncoding, stream);
            } else if (transferEncoding == "chunked") {
                responseBody = ReadChunkedStream(stream);
            } else if (contentLength > 0) {
                responseBody = ReadNetworkStream(stream, contentLength);
            } else {
                responseBody = ReadNetworkStream(stream);
            }
            var sb = new StringBuilder();
            sb.AppendLine(status);
            foreach (var header in headers) {
                foreach (var value in headers[header.Key]) sb.AppendLine(String.Format("{0}: {1}", header.Key, value));
            }
            sb.AppendLine();
            sb.AppendLine(responseBody);
            return (sb.ToString());
        }
    }
}