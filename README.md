# Markdown.Avalonia.Containers.VLC

This is test movie package or ContainerBlockHandler in Markdown.Avalonia 

## Nuget


## How to use

* Instal `VideoLAN.LibVLC.***`
   Markdown.Avalonia.Containers.VLC use LibVLC. It requires VideoLAN.LibVLC.
   For example, if you want to run on Windows, you should install VideoLAN.LibVLC.Windows.

* Edit Xaml
   ```xml
   <!-- xmlns:md="clr-namespace:Markdown.Avalonia;assembly=Markdown.Avalonia" -->
   <!-- xmlns:vlc="clr-namespace:Markdown.Avalonia.Containers.VLC;assembly=Markdown.Avalonia.Containers.VLC" -->
    <md:MarkdownScrollViewer>
      <md:MarkdownScrollViewer.Engine>
        <md:Markdown>
          <md:Markdown.ContainerBlockHandler>
            <md:ContainerSwitch>
              <vlc:VideoContainerBlockHandler x:Key="movie"/>
            </md:ContainerSwitch>
          </md:Markdown.ContainerBlockHandler>
        </md:Markdown>
      </md:MarkdownScrollViewer.Engine>
    </md:MarkdownScrollViewer>

   or

    <md:MarkdownScrollViewer>
      <md:MarkdownScrollViewer.Engine>
        <md:Markdown>
          <md:Markdown.ContainerBlockHandler>
            <vlc:VideoContainerBlockHandler/>
          </md:Markdown.ContainerBlockHandler>
        </md:Markdown>
      </md:MarkdownScrollViewer.Engine>
    </md:MarkdownScrollViewer>
   ```
* Edit Markdown
  ```md
  **only url** 
  ::: movie
  http://example.com/foo/bar.mp4
  :::

  **url and size** 
  ::: movie
  { 
    Source:"http://example.com/foo/bar.mp4",
    Width:"640",
    Height:"480"
  }
  :::


  ```


## License

LGPL-2.1-or-later
