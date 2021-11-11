using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Markdown.Avalonia.Containers.VLC
{
    partial class SimpleVideoPlayer : UserControl, IDisposable
    {
        static SimpleVideoPlayer()
        {
            ForegroundProperty.Changed
                .AddClassHandler<SimpleVideoPlayer>((x, _) => x.UpdateForeground());

            BoundsProperty.Changed
                .AddClassHandler<SimpleVideoPlayer>((x, _) => x.UpdateBounds());
        }

        private const double Ratio = 16d / 9d;
        private bool _ShoudSetHeight = true;
        private int _measuredWidth;
        private int _calcatedHeight;


        private LibVLC _Libvlc;
        private MediaPlayer _MediaPlayer;

        private Button _PlayButton;
        private Button _ResumeButton;
        private Button _PauseButton;
        private Button _StopButton;
        private Button _MuteButton;
        private Button _LoudButton;

        private VideoView _VideoView;
        private Canvas[] _Canvases;

        private VideoParameter _Parameter;
        internal VideoParameter Parameter
        {
            set
            {
                _Parameter = value;
                UpdateParameter();
            }
            get => _Parameter;
        }

        private bool _IsMute = false;

        public SimpleVideoPlayer()
        {
            InitializeComponent();
            UpdateForeground();

            _Libvlc = new LibVLC();

            _PlayButton.Click += (s, e) => Play();
            _ResumeButton.Click += (s, e) => Resume();
            _PauseButton.Click += (s, e) => Pause();
            _StopButton.Click += (s, e) => Stop();
            _MuteButton.Click += (s, e) => Mute();
            _LoudButton.Click += (s, e) => Unmute();
        }

        public void Play()
        {
            if (Parameter.Source is null) return;

            _PlayButton.IsVisible = false;
            _ResumeButton.IsVisible = false;
            _PauseButton.IsVisible = true;
            _StopButton.IsVisible = true;

            _VideoView.MediaPlayer = _MediaPlayer = new MediaPlayer(_Libvlc);
            using var media = new Media(_Libvlc, Parameter.Source);

            _MediaPlayer.Mute = _IsMute;
            _MediaPlayer.Play(media);
        }

        public void Resume()
        {
            _PlayButton.IsVisible = false;
            _ResumeButton.IsVisible = false;
            _PauseButton.IsVisible = true;
            _StopButton.IsVisible = true;

            _MediaPlayer.Play();
        }

        public void Pause()
        {
            _PlayButton.IsVisible = false;
            _ResumeButton.IsVisible = true;
            _PauseButton.IsVisible = false;
            _StopButton.IsVisible = true;

            _MediaPlayer.Pause();
        }

        public void Stop()
        {
            _PlayButton.IsVisible = true;
            _ResumeButton.IsVisible = false;
            _PauseButton.IsVisible = false;
            _StopButton.IsVisible = false;

            _MediaPlayer.Stop();
        }

        public void Mute()
        {
            _MuteButton.IsVisible = false;
            _LoudButton.IsVisible = true;

            _IsMute = true;
            if (_MediaPlayer != null)
                _MediaPlayer.Mute = true;
        }
        public void Unmute()
        {
            _MuteButton.IsVisible = true;
            _LoudButton.IsVisible = false;

            _IsMute = false;
            if (_MediaPlayer != null)
                _MediaPlayer.Mute = false;
        }

        public void Dispose()
        {
            _MediaPlayer?.Dispose();
            _Libvlc?.Dispose();
        }

        private void UpdateParameter()
        {
            _ShoudSetHeight = Double.IsNaN(_Parameter.Width) || Double.IsNaN(_Parameter.Height);

            if (!Double.IsNaN(_Parameter.Width))
            {
                MaxWidth = _Parameter.Width;
            }
            if (!Double.IsNaN(_Parameter.Height))
            {
                MaxHeight = _Parameter.Height;
            }
        }

        private void UpdateForeground()
        {
            foreach (var canvas in _Canvases)
            {
                foreach (var shape in canvas.Children.OfType<Shape>())
                {
                    shape.Fill = Foreground;
                }
            }
        }

        private void UpdateBounds()
        {
            var width = Bounds.Width;

            if (_ShoudSetHeight && _measuredWidth != (int)width)
            {
                _measuredWidth = (int)width;

                var newHeight = width / Ratio;
                if (_calcatedHeight != (int)newHeight)
                {
                    _calcatedHeight = (int)newHeight;
                    Height = newHeight;
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _PlayButton = this.FindControl<Button>("PlayButton");
            _ResumeButton = this.FindControl<Button>("ResumeButton");
            _PauseButton = this.FindControl<Button>("PauseButton");
            _StopButton = this.FindControl<Button>("StopButton");
            _MuteButton = this.FindControl<Button>("MuteButton");
            _LoudButton = this.FindControl<Button>("LoudButton");
            _VideoView = this.FindControl<VideoView>("VideoView");

            _Canvases = Enumerable.Range(1, 6)
                                  .Select(idx => this.FindControl<Canvas>("C" + idx))
                                  .ToArray();
        }

    }
}
