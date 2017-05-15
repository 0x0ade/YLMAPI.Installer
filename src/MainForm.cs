using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MonoMod.Installer {
    public partial class MainForm : Form {

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                // cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                // cp.ExStyle |= 0x00080000; // WS_EX_LAYERED

                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED

                return cp;
            }
        }

        public readonly static Random RNG = new Random();

        public readonly GameModInfo Info;

        public bool DrawFPS = true;

        public Size BackgroundSize;
        public float BackgroundSizeFactor;

        private Stopwatch _Stopwatch;

        private PrivateFontCollection _Fonts;
        private Font _Font;

        private Animation _IntroSlideAnimation;

        private OpenFileDialog _BrowseDialog;

        public MainForm(GameModInfo info) {
            Info = new CachedInfo(info);

            InitializeComponent();

            SuspendLayout();

            MinimumSize = MaximumSize = Size = new Size((int) (460 * AutoScaleFactor.Width), (int) (600 * AutoScaleFactor.Height));

            Text = Info.ModInstallerName;
            MainStep1Label.Text = string.Format(MainStep1Label.Text, Info.ExecutableName);
            BackgroundImage = Info.BackgroundImage;
            HeaderPicture.Image = Info.HeaderImage;

            ResumeLayout(false);
        }


        private void MainForm_Load(object _s, EventArgs _e) {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Reset();
            _Stopwatch.Start();

            SuspendLayout();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;

            ResizeRedraw = true;

            ResizeBegin += (s, e) => AnimationManager.IsThrottled = true;
            ResizeEnd += (s, e) => AnimationManager.IsThrottled = false;

            SizeChanged += (s, e) => {
                foreach (Panel panel in Controls)
                    panel.Width = Width;
            };

            _Fonts = new PrivateFontCollection();
            unsafe
            {
                byte[] data = Properties.Resources.selawksl;
                fixed (byte* dataptr = &data[0])
                    _Fonts.AddMemoryFont(new IntPtr(dataptr), data.Length);
            }
            _Font = new Font(_Fonts.Families[0], 12f);
            Controls.ForEachDeep(c => c.Font = _Font);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                AnimationManager.IsMono) {
                Application.ApplicationExit += (s, e) => {
                    // At least hides the "mono.exe crashed" window.
                    throw new Exception("Thanks, Windows Mono!");
                };
            }

            BackgroundSize = new Size(
                2048,
                1228
            );

            foreach (Panel panel in Controls) {
                if (panel != HeaderPanel && panel != MainPanel)
                    panel.Visible = false;
            }

            ResumeLayout(false);
            PerformLayout();

            AnimationManager.AnimationRoot = this;
            Invalidated += (s, e) => AnimationManager.InvalidatedRoot++;

            this.Animate((a, t) => Opacity = t, dur: 0.4f, smooth: true);
            this.Animate((a, t) => BackgroundSizeFactor = 1f + 8f * (1f - t), dur: 2f, easing: Easings.QuinticEaseOut, smooth: true);

            HeaderPanel.SlideIn(delay: 0.05f);

            _IntroSlideAnimation = MainPanel.SlideIn(delay: 0.1f);

            this.Animate(UpdateBackground, loop: true, smooth: false);

            Info.OnChangeCurrentExecutablePath += (info, path) => {
                MainPathBox.Text = path ?? "";
                bool valid = !string.IsNullOrEmpty(path);
                MainInstallButton.Enabled = valid;
                MainUninstallButton.Enabled = valid;
            };
            Info.CurrentExecutablePath = GameFinderManager.Find(Info);

            _BrowseDialog = new OpenFileDialog() {
                Title = $"Select {Info.ExecutableName}",
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                Multiselect = false,
                ShowReadOnly = false,
                Filter = $"{Info.ExecutableName}|{Info.ExecutableName}",
                FilterIndex = 0
            };
            _BrowseDialog.FileOk += (object s, CancelEventArgs e) => {
                Info.CurrentExecutablePath = _BrowseDialog.FileNames[0];
            };
        }


        private PointF _BackgroundOffs;
        private SolidBrush _BackgroundBrush = new SolidBrush(Color.FromArgb(127, 45, 45, 45));
        private SolidBrush _FPSBrush = new SolidBrush(Color.FromArgb(127, 255, 255, 255));
        private SolidBrush _ProgressShapeBrush = new SolidBrush(Color.FromArgb(127, 255, 255, 255));
        private long _FrameStart;
        private long _FrameStartPrev;
        private float _CurrentFrameTime;

        private int[] _ProgressShapeSizes = { 7, 4, 10, 3, 16, 7, 5, 7 };
        private PointF[][] _ProgressShapes;
        private PointF[] _ProgressShapeCurrent;
        private bool _ProgressShapesInit;

        public void UpdateBackground(Animation a, float t) {
            Point cursor = PointToClient(Cursor.Position);
            _BackgroundOffs = new PointF(
                cursor.X - Width / 2f,
                cursor.Y - Height / 2f
            );

            if (!_ProgressShapesInit) {
                _ProgressShapes = new PointF[_ProgressShapeSizes.Length][];
                for (int si = _ProgressShapeSizes.Length - 1; si > -1; --si) {
                    PointF[] current = _ProgressShapes[si] = new PointF[_ProgressShapeSizes[si]];
                    for (int i = current.Length - 1; i > -1; --i) {
                        float f = (i / (float) current.Length) * (float) Math.PI * 2f;
                        current[i] = new PointF(
                            (float) (128f * Math.Cos(f)),
                            (float) (128f * Math.Sin(f))
                        );
                    }
                }
                _ProgressShapeCurrent = _ProgressShapes[0];
                _ProgressShapesInit = true;

            } else {
                float pulse = Easings.CubicEaseIn(Math.Max(0f, 1f - (AnimationManager.Time % 3f) / 3f));
                int currentIndex = (int) (pulse * _ProgressShapes.Length);
                PointF[] current = _ProgressShapes[currentIndex];
                for (int i = current.Length - 1; i > -1; --i) {
                    float f = (i / (float) current.Length) * (float) Math.PI * 2f;
                    f += AnimationManager.Time * 0.1f;
                    PointF from = current[i];
                    PointF to = new PointF(
                        (float) (128f * Math.Cos(f)),
                        (float) (128f * Math.Sin(f))
                    );
                    PointF offs = new PointF(
                        32f * (float) (RNG.NextDouble() - 0.5f),
                        32f * (float) (RNG.NextDouble() - 0.5f)
                    );
                    current[i] = new PointF(
                        from.X + (to.X - from.X) * 0.1f + pulse * offs.X,
                        from.Y + (to.Y - from.Y) * 0.1f + pulse * offs.Y
                    );
                }
                _ProgressShapeCurrent = current;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if (e.ClipRectangle.Size == Size) {
                _FrameStartPrev = _FrameStart;
                _FrameStart = _Stopwatch.ElapsedMilliseconds;
                _CurrentFrameTime = (_FrameStart - _FrameStartPrev) * 0.001f;
            }

            Graphics g = e.Graphics;

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            if (e.ClipRectangle.Size != Size || _IntroSlideAnimation.Status != Animation.EStatus.Finished)
                g.InterpolationMode = InterpolationMode.Bilinear;
            g.DrawBackgroundImage(
                BackgroundImage,
                Width,
                Height,
                (int) (BackgroundSize.Width * BackgroundSizeFactor),
                (int) (BackgroundSize.Height * BackgroundSizeFactor),
                ImageLayout.Center,
                _BackgroundOffs.X * -0.1f,
                _BackgroundOffs.Y * -0.1f
            );
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            // g.FillRectangle(_BackgroundBrush, 1, 1, Width - 2, Height - 2);

            foreach (Panel panel in Controls)
                if (panel.Visible)
                    g.FillRectangle(_BackgroundBrush, panel.Left, panel.Top, panel.Width, panel.Height);

            if (_ProgressShapesInit && ProgressPanel.Visible) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Matrix transformPrev = g.Transform;

                g.TranslateTransform(
                    ProgressPanel.Left + Width * 0.5f + _BackgroundOffs.X * -0.03f,
                    Height * 0.5f + _BackgroundOffs.Y * -0.03f
                );
                g.FillPolygon(_ProgressShapeBrush, _ProgressShapeCurrent);

                g.Transform = transformPrev;
                g.SmoothingMode = SmoothingMode.HighSpeed;
            }

            if (DrawFPS) {
                g.DrawString((1f / AnimationManager.CurrentFrameTime).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), _Font, _FPSBrush, 0, 0);
                g.DrawString((1f / _CurrentFrameTime).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), _Font, _FPSBrush, 0, 14 * (AutoScaleFactor.Height));
            }
        }

        protected virtual void OnDispose(bool disposing) {
            if (disposing) {
                _Font?.Dispose();
                _Font = null;
                _Fonts?.Dispose();
                _Fonts = null;
            }
        }

        private const int WM_NCHITTEST = 0x0084;
        private readonly static IntPtr HTCAPTION = new IntPtr(0x02);
        private readonly static IntPtr HTBOTTOMRIGHT = new IntPtr(0x11);

        private const int WM_ERASEBKGND = 0x0014;
        private readonly static IntPtr FALSE = new IntPtr(0);
        private readonly static IntPtr TRUE = new IntPtr(1);

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_NCHITTEST) {
                Point cursor = new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16);
                cursor = PointToClient(cursor);
                if (cursor.Y < 130) {
                    m.Result = HTCAPTION;
                    return;
                }
                /*
                if (cursor.X >= ClientSize.Width - 32 && cursor.Y >= ClientSize.Height - 32) {
                    m.Result = HTBOTTOMRIGHT;
                    return;
                }
                */
            }

            base.WndProc(ref m);
        }

        private void MainBrowseButton_Click(object sender, EventArgs e) {
            _BrowseDialog.ShowDialog(this);
        }

        private void InstallButton_Click(object sender, EventArgs e) {
            MainPanel.SlideOut();
            ProgressPanel.SlideIn();
        }

        private void MainUninstallButton_Click(object sender, EventArgs e) {

        }
    }
}
