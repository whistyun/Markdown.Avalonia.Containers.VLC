using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Markdown.Avalonia.Containers.VLC.Demo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public MainWindowViewModel()
        {
            Text =
            "# Movie\r\n" +
            "::: movie\r\n" +
            "{\r\n" +
            "Source: \"http://example.com/foo/bar.mp4\",\r\n"+
            "Width: 640,\r\n" +
            "Height: 480,\r\n" +
            "}\r\n"+
            ":::\r\n";
        }
    }
}
