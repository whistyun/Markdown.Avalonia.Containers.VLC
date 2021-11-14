using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Markdown.Avalonia.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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

        private string[] AssetAssemblyNames;

        public VideoContainerBlockHandler()
        {
            var stack = new StackTrace();
            this.AssetAssemblyNames = stack.GetFrames()
                            .Select(frm => frm.GetMethod().DeclaringType.Assembly)
                            .Where(asm => !asm.GetName().Name.Equals("Markdown.Avalonia"))
                            .Where(asm => !asm.GetName().Name.Equals("Markdown.Avalonia.Containers.VLC"))
                            .Select(asm => asm.GetName().Name)
                            .Distinct()
                            .ToArray();
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
            var parameter = new VideoParameter(assetPathRoot, AssetAssemblyNames);
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

            parameter.SourcePath = urlTxt;

            return parameter;
        }

    }
}
