using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Containers.VLC
{
    class VideoParameter
    {
        public Uri Source { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public VideoParameter()
        {
            Source = null;
            Width = Double.NaN;
            Height = Double.NaN;
        }
    }
}
