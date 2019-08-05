#if UNITY_EDITOR
using System;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace NpmPackageLoader
{
    public class FtpData
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public string UnityPackageName { get; set; }

        public string LocalFile { get; set; }
    }

    public static class FtpUtils
    {
        private static WebClient _request;

        public static void Upload(FtpData data, Action success, Action fail)
        {
            const string progressHeader = "Upload";

            try
            {
                var remoteServerUrl = data.Url;
                var remotePackageDir = $"{remoteServerUrl}/{data.PackageName}";
                var remoteVersionDir = $"{remotePackageDir}/{data.PackageVersion}";
                var remoteFile = new Uri($"{remoteVersionDir}/{data.UnityPackageName}.unitypackage");

                var localFile = data.LocalFile;

                EditorUtility.DisplayProgressBar(progressHeader, remoteVersionDir, 0f);

                CreateDirectoryIfNotExists(remotePackageDir, data);
                CreateDirectoryIfNotExists(remoteVersionDir, data);

                _request = new WebClient();

                _request.UploadProgressChanged += (sender, args) =>
                {
                    var progress = 1f * args.BytesSent / args.TotalBytesToSend;
                    EditorUtility.DisplayProgressBar(progressHeader, remoteVersionDir, progress);
                };

                _request.UploadFileCompleted += (sender, args) =>
                {
                    EditorUtility.ClearProgressBar();

                    // ReSharper disable once AccessToDisposedClosure
                    _request.Dispose();

                    if (args.Error != null)
                    {
                        Debug.LogException(args.Error);
                        fail();
                    }
                    else
                    {
                        success();
                    }
                };

                _request.Credentials = new NetworkCredential(data.User, data.Password);
                _request.UploadFileAsync(remoteFile, localFile);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _request?.Dispose();

                fail();
            }
        }

        public static void Download(FtpData data, Action success, Action fail)
        {
            const string progressHeader = "Download";

            try
            {
                var localFile = data.LocalFile;
                var remoteVersionDir = $"{data.Url}/{data.PackageName}/{data.PackageVersion}/";
                var remoteFile = new Uri($"{remoteVersionDir}/{data.UnityPackageName}.unitypackage");

                _request = new WebClient();

                _request.DownloadProgressChanged += (sender, args) =>
                {
                    var progress = 1f * args.BytesReceived / args.TotalBytesToReceive;
                    EditorUtility.DisplayProgressBar(progressHeader, remoteVersionDir, progress);
                };

                _request.DownloadFileCompleted += (sender, args) =>
                {
                    EditorUtility.ClearProgressBar();

                    // ReSharper disable once AccessToDisposedClosure
                    _request.Dispose();

                    if (args.Error != null)
                    {
                        Debug.LogException(args.Error);
                        fail();
                    }
                    else
                    {
                        success();
                    }
                };

                _request.Credentials = new NetworkCredential(data.User, data.Password);
                _request.DownloadFileAsync(remoteFile, localFile);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _request?.Dispose();
                fail();
            }
        }


        private static void CreateDirectoryIfNotExists(string url, FtpData data)
        {
            try
            {
                var r = (FtpWebRequest) WebRequest.Create(url);
                r.Credentials = new NetworkCredential(data.User, data.Password);
                r.Method = WebRequestMethods.Ftp.MakeDirectory;

                using (var resp = (FtpWebResponse) r.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
            }
            catch (WebException)
            {
                // already exist
            }
        }
    }
}

#endif