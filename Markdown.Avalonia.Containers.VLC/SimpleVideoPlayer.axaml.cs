using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
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

        private double _Ratio = 16d / 9d;
        private bool _ShoudSetHeight = true;
        private int _measuredWidth;
        private int _calcatedHeight;
        private double _seekbarValue;

        private LibVLC _Libvlc;
        private MediaPlayer _MediaPlayer;

        private Button _PlayButton;
        private Button _ResumeButton;
        private Button _PauseButton;
        private Button _StopButton;
        private Button _MuteButton;
        private Button _LoudButton;

        private TextBlock _NowTime;
        private TextBlock _EntireTime;

        private Border _View;
        private VideoView _VideoView;
        private Canvas[] _Canvases;

        private ScrollBar _SeekBar;

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


            var value = _SeekBar.GetObservable(ScrollBar.ValueProperty);
            value.Subscribe(_SeekBar_ValueChanged);
        }

        ~SimpleVideoPlayer()
        {
            Dispose();
        }

        private void Play()
        {
            if (Parameter.Source is null) return;

            _PlayButton.IsVisible = false;
            _ResumeButton.IsVisible = false;
            _PauseButton.IsVisible = true;
            _StopButton.IsVisible = true;

            _View.Child = _VideoView;
            _VideoView.MediaPlayer = _MediaPlayer = new MediaPlayer(_Libvlc);
            using var media = new Media(_Libvlc, Parameter.Source);

            _MediaPlayer.Mute = _IsMute;
            _MediaPlayer.Playing += (s, e) => Dispatcher.UIThread.InvokeAsync(() => _MediaPlayer_Playing());
            _MediaPlayer.EndReached += (s, e) => Dispatcher.UIThread.InvokeAsync(() => Stop());
            _MediaPlayer.PositionChanged += (s, e) => Dispatcher.UIThread.InvokeAsync(() => _MediaPlayer_PositionChanged());
            _MediaPlayer.Play(media);
        }

        private void _MediaPlayer_Playing()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _SeekBar.IsEnabled = _MediaPlayer.IsSeekable;

                var length = _MediaPlayer.Length;
                if (length != -1)
                {
                    _SeekBar.SmallChange = 5000d / length;
                    _SeekBar.LargeChange = 10000d / length;

                    var sec = length / 1000 % 60;
                    var min = length / 1000 / 60 % 60;
                    var hour = length / 1000 / 60 / 60;
                    _EntireTime.Text = hour == 0 ?
                            String.Format("{0:D2}:{1:D2}", min, sec) :
                            String.Format("{0}:{1:D2}:{2:D2}", hour, min, sec);
                }
            });
        }

        private void _MediaPlayer_PositionChanged()
        {
            _SeekBar.Value = _seekbarValue = _MediaPlayer.Position;

            var length = _MediaPlayer.Length;
            if (length != -1)
            {
                var pos = (int)(_MediaPlayer.Position * length);
                var sec = pos / 1000 % 60;
                var min = pos / 1000 / 60 % 60;
                var hour = pos / 1000 / 60 / 60;
                _NowTime.Text = length < 60 * 60 * 1000 ?
                        String.Format("{0:D2}:{1:D2}", min, sec) :
                        String.Format("{0}:{1:D2}:{2:D2}", hour, min, sec);
            }
        }

        private void _SeekBar_ValueChanged(double newValue)
        {
            if (_seekbarValue != newValue)
            {
                _seekbarValue = newValue;
                if (_MediaPlayer != null)
                    _MediaPlayer.Position = (float)newValue;
            }
        }

        private void Resume()
        {
            _PlayButton.IsVisible = false;
            _ResumeButton.IsVisible = false;
            _PauseButton.IsVisible = true;
            _StopButton.IsVisible = true;

            _MediaPlayer.Play();
        }

        private void Pause()
        {
            _PlayButton.IsVisible = false;
            _ResumeButton.IsVisible = true;
            _PauseButton.IsVisible = false;
            _StopButton.IsVisible = true;

            _MediaPlayer.Pause();
        }

        private void Stop()
        {
            _PlayButton.IsVisible = true;
            _ResumeButton.IsVisible = false;
            _PauseButton.IsVisible = false;
            _StopButton.IsVisible = false;
            _SeekBar.Value = _seekbarValue = 0;
            _SeekBar.IsEnabled = false;
            _MediaPlayer?.Stop();
            _NowTime.Text = "--:--";
            _EntireTime.Text = "--:--";
            _View.Child = null;
        }

        private void Mute()
        {
            _MuteButton.IsVisible = false;
            _LoudButton.IsVisible = true;

            _IsMute = true;
            if (_MediaPlayer != null)
                _MediaPlayer.Mute = true;
        }
        private void Unmute()
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
            _ShoudSetHeight = Double.IsNaN(_Parameter.Height)
                || (!Double.IsNaN(_Parameter.Width) && !Double.IsNaN(_Parameter.Height));

            if (!Double.IsNaN(_Parameter.Width))
            {
                MaxWidth = _Parameter.Width;
            }
            if (!Double.IsNaN(_Parameter.Height))
            {
                MaxHeight = _Parameter.Height;
            }

            if (!Double.IsNaN(_Parameter.Width) && !Double.IsNaN(_Parameter.Height))
            {
                _Ratio = _Parameter.Width / _Parameter.Height;
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

            if (_ShoudSetHeight)
            {
                if (_measuredWidth != (int)width)
                {
                    _measuredWidth = (int)width;

                    var newHeight = width / _Ratio;
                    if (_calcatedHeight != (int)newHeight)
                    {
                        _calcatedHeight = (int)newHeight;
                        Height = newHeight;
                    }
                }
            }
            else Height = MaxHeight;

            InvalidateVisual();
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

            _View = this.FindControl<Border>("View");
            _VideoView = new VideoView();// this.FindControl<VideoView>("VideoView");
            _SeekBar = this.FindControl<ScrollBar>("SeekBar");

            _NowTime = this.FindControl<TextBlock>("NowTime");
            _EntireTime = this.FindControl<TextBlock>("EntireTime");

            _Canvases = Enumerable.Range(1, 6)
                                  .Select(idx => this.FindControl<Canvas>("C" + idx))
                                  .ToArray();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            Stop();
        }

    }
}
