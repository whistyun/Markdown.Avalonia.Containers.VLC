using Avalonia;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Markdown.Avalonia.Containers.VLC
{
    class VideoParameter
    {
        private string[] AssetAssemblyNames;
        private string AssetPathRoot { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public string SourcePath { get; set; }

        public VideoParameter(string assetPathRoot, string[] assetAssemblyNames)
        {
            AssetPathRoot = assetPathRoot;
            AssetAssemblyNames = assetAssemblyNames;

            SourcePath = null;
            Width = Double.NaN;
            Height = Double.NaN;
        }

        public bool TryOpenSource(LibVLC vlc, out Media media, out string errMsg)
        {
            if (SourcePath is null)
            {
                media = null;
                errMsg = "no media source input";
                return false;
            }

            var url = CreatePath();

            switch (url.Scheme)
            {
                default:
                case "http":
                case "https":
                    try
                    {
                        using var wc = new WebClient();
                        // try open url. 
                        // StreamMediaInput did not work via WebClient?
                        wc.OpenRead(url).Close();

                        media = new Media(vlc, url);
                        errMsg = null;
                        return true;
                    }
                    catch
                    {
                        media = null;
                        errMsg = $"'{SourcePath}' is not available.";
                        return false;
                    }

                case "file":
                    if (File.Exists(url.LocalPath))
                    {
                        media = new Media(vlc, url);
                        errMsg = null;
                        return true;
                    }
                    else
                    {
                        media = null;
                        errMsg = $"the filepath '{SourcePath}' is not exists.";
                        return false;
                    }

                case "avares":
                    var loader = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    if (loader.Exists(url))
                    {
                        media = new Media(vlc, new StreamMediaInput(loader.Open(url)));
                        errMsg = null;
                        return true;
                    }
                    else
                    {
                        media = null;
                        errMsg = $"the resource '{SourcePath}' is not exists.";
                        return false;
                    }
            }
        }

        private Uri CreatePath()
        {
            if (Uri.IsWellFormedUriString(SourcePath, UriKind.Absolute))
            {
                return new Uri(SourcePath);
            }
            if (Path.IsPathRooted(SourcePath))
            {
                return new Uri("file://" + SourcePath);
            }

            // check resources
            var loader = AvaloniaLocator.Current.GetService<IAssetLoader>();
            foreach (var asmNm in AssetAssemblyNames)
            {
                var assetUrl = new Uri($"avares://{asmNm}/{SourcePath}");
                if (loader.Exists(assetUrl))
                    return assetUrl;
            }

            // check filesystem
            if (Uri.IsWellFormedUriString(AssetPathRoot, UriKind.Absolute))
            {
                return new Uri(new Uri(AssetPathRoot), SourcePath);
            }
            else
            {
                return new Uri("file://" + Path.Combine(AssetPathRoot, SourcePath));
            }
        }

    }
}
