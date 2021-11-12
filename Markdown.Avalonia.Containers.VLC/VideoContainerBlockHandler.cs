using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Markdown.Avalonia.Utils;
using Newtonsoft.Json.Linq;
using System;

namespace Markdown.Avalonia.Containers.VLC
{
    public class VideoContainerBlockHandler : IContainerBlockHandler
    {
        private static bool HasInitError;
        private static string InitErrorMessage;

        static VideoContainerBlockHandler()
        {
            try
            {
                Core.Initialize();
                HasInitError = false;
                InitErrorMessage = "";
            }
            catch (Exception e)
            {
                HasInitError = true;
                InitErrorMessage = String.Format("{0}: {1}", e.GetType().Name, e.Message);
            }
        }

        public Border ProvideControl(string assetPathRoot, string blockName, string lines)
        {
            if (HasInitError)
            {
                // Failed to load LibVLC.
                var text = new TextBlock()
                {
                    Text = InitErrorMessage,
                    Foreground = new SolidColorBrush(Colors.Red)
                };
                var border = new Border()
                {
                    BorderThickness = new Thickness(5),
                    Padding = new Thickness(5),
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    Child = text,
                };
                return border;
            }
            else
            {
                var svplayer = new SimpleVideoPlayer();
                svplayer.Parameter = ParseParameter(assetPathRoot, lines);

                var border = new Border();
                border.Child = svplayer;
                return border;
            }
        }

        private VideoParameter ParseParameter(string assetPathRoot, string lines)
        {
            var parameter = new VideoParameter();
            string urlTxt = null;
            try
            {
                var paramObj = JObject.Parse(lines.Trim());

                if (paramObj.ContainsKey("Source"))
                    urlTxt = paramObj["Source"].ToString();

                if (paramObj.ContainsKey("Width"))
                    parameter.Width = Double.TryParse(paramObj["Width"].ToString(), out var width) ? width : Double.NaN;

                if (paramObj.ContainsKey("Height"))
                    parameter.Height = Double.TryParse(paramObj["Height"].ToString(), out var height) ? height : Double.NaN;
            }
            catch
            {
                urlTxt = lines.Trim();
            }

            parameter.Source = GetUrlFrom(assetPathRoot, urlTxt);

            return parameter;
        }

        private Uri GetUrlFrom(string assetPathRoot, string urlTxt)
        {
            if (urlTxt is null) return null;

            // check network
            if (Uri.TryCreate(urlTxt, UriKind.Absolute, out var url))
            {
                return url;
            }

            // check filesystem
            if (assetPathRoot != null)
            {
                return new Uri(new Uri(assetPathRoot), urlTxt);
            }

            return null;
        }
    }
}
