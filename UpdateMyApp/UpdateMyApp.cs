using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UpdateMyApp
{
    public class Update
    {
        /// <summary>
        /// If you set to true you must catch all error.
        /// </summary>
        public static bool IsEnableError { get; set; } = false;

        private static Uri UrlToXML = null;
        private static Uri FileUpdateURL = null;
        private static Version CurrentVersion = null;
        private static Version RemoteVersion = null;
        private static readonly HttpClient client = new HttpClient();

        public delegate void DownloadedProgressDelegate(Int64 byteDownloaded, Int64 byteToDownload, double perCentProgress);

        /// <summary>
        /// Subscribe to get byteDownloaded, byteToDownload and perCentProgress.
        /// </summary>
        public static event DownloadedProgressDelegate DownloadedProgress;

        /// <summary>
        /// Set your current application version.
        /// </summary>
        /// <param name="Version"></param>
        /// <returns></returns>
        public static bool SetCurrentVersion(string Version)
        {
            try
            {
                CurrentVersion = new Version(Version);
                return true;
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Set your current application version.
        /// </summary>
        /// <param name="Version"></param>
        /// <returns></returns>
        public static bool SetCurrentVersion(Version Version)
        {
            try
            {
                CurrentVersion = Version;
                return true;
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Set URL to your XML file.
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static bool SetUrlToXml(string Url)
        {
            try
            {
                if (string.IsNullOrEmpty(Url) || string.IsNullOrWhiteSpace(Url))
                {
                    throw new NullReferenceException("Url is empty or null");
                }
                else
                {
                    Uri _temp = new Uri(Url);
                    if (Path.GetExtension(_temp.LocalPath) == ".xml")
                    {
                        UrlToXML = _temp;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Check if new version is ready.
        /// </summary>
        public static async Task<bool> CheckForNewVersionAsync()
        {
            try
            {
                if (UrlToXML == null || CurrentVersion == null)
                {
                    throw new NullReferenceException("Call first: SetUrlToXml and SetCurrentVersion");
                }

                Dictionary<string, string> _dictopnery = await ReadXmlFromURLAsync();

                if (_dictopnery.Count == 0)
                    return false;

                if (_dictopnery.TryGetValue("url", out string outputUrl))
                    Uri.TryCreate(outputUrl, UriKind.RelativeOrAbsolute, out FileUpdateURL);

                if (_dictopnery.TryGetValue("version", out string _version))
                {
                    Version.TryParse(_version, out RemoteVersion);

                    if (CurrentVersion < RemoteVersion)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (IsEnableError)
                        throw new Exception($"XML do not contain 'version' element");
                    else
                    {
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] XML do not contain 'version' element");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// Private.
        /// </summary>
        /// <returns>Task<Dictionary<string, string>></returns>
        private static async Task<Dictionary<string, string>> ReadXmlFromURLAsync()
        {
            if (UrlToXML == null)
            {
                throw new NullReferenceException("XmlURL is empty or null");
            }

            Dictionary<string, string> _dictopnery = new Dictionary<string, string>();

            try
            {
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };

                var getXml = await client.GetAsync(UrlToXML);
                var stream = await getXml.Content.ReadAsStreamAsync();
                var itemXml = XElement.Load(stream);

                foreach (var item in itemXml.Elements())
                {
                    _dictopnery.Add(item.Name.LocalName, item.Value);
                }

                return _dictopnery;
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");
                }
            }
            return _dictopnery;
        }

        /// <summary>
        /// Read all data from you XML.
        /// </summary>
        public static async Task<Dictionary<string, string>> ReadAllValueFromXmlAsync()
        {
            return await ReadXmlFromURLAsync();
        }

        private static async Task<Int64> GetFileSizeAsync()
        {
            try
            {
                var webRequest = HttpWebRequest.Create(FileUpdateURL);
                webRequest.Method = "HEAD";

                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    return Convert.ToInt64(webResponse.Headers.Get("Content-Length"));
                }
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");

                    return 0;
                }
            }
        }


        private static void EventDownloadedProgress(Int64 send, Int64 total)
        {
            double dProgress = ((double)send / total) * 100.0;

            DownloadedProgress?.Invoke(send, total, dProgress);
        }

        /// <summary>
        /// Download file from url. You can subscribe DownloadedProgress event do get progress.
        /// </summary>
        /// <param name="destinationPatch"></param>
        /// <returns></returns>
        public static async Task<bool> DownloadFileAsync(string destinationPatch)
        {
            if (FileUpdateURL == null)
            {
                throw new NullReferenceException("URL to file is not set. Call CheckForNewVersionAsync first.");
            }

            try
            {
                using (HttpResponseMessage response = await client.GetAsync(FileUpdateURL, HttpCompletionOption.ResponseHeadersRead))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (Stream streamToWriteTo = File.Open(destinationPatch, FileMode.Create))
                    {
                        Int64 totalRead = 0L;
                        var buffer = new byte[65536];
                        var isMoreToRead = true;

                        var FileSize = await GetFileSizeAsync();

                        do
                        {
                            var read = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await streamToWriteTo.WriteAsync(buffer, 0, read);

                                totalRead += read;

                                EventDownloadedProgress(totalRead, FileSize);
                            }
                        }
                        while (isMoreToRead);

                        if (totalRead == FileSize)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");

                    return false;
                }
            }
        }

        /// <summary>
        /// Open website instead of downloading file.
        /// </summary>
        public static void OpenURL()
        {
            try
            {
                Process.Start(FileUpdateURL.ToString());
            }
            catch (Exception ex)
            {
                if (IsEnableError)
                    throw;
                else
                {
                    if (ex.InnerException != null)
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.InnerException.Message}");
                    else
                        Debug.WriteLine($"[UpdateMyApp][{DateTime.Now}] {ex.Message}");
                }
            }
        }
    }
}