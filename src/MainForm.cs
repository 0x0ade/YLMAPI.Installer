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

namespace YLMAPI.Installer {
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

        public bool DrawFPS = true;

        public Size BackgroundSize;
        public float BackgroundSizeFactor;

        private Stopwatch _Stopwatch;

        private PrivateFontCollection _Fonts;
        private Font _Font;

        public MainForm() {
            InitializeComponent();

            SuspendLayout();
            MinimumSize = Size = new Size(460, 600);
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
            Controls.ForEachDeep(c => c.Font.SystemFontName == "DefaultFont" || c.Font.FontFamily.Name == _Font.FontFamily.Name, c => c.Font = _Font);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                AnimationManager.IsMono) {
                Application.ApplicationExit += (s, e) => {
                    // At least hides the "mono.exe crashed" window.
                    throw new Exception("Thanks, Windows Mono!");
                };
            }

            if (RNG.NextDouble() < 0.5D)
                BackgroundImage = Properties.Resources.background_2;

            BackgroundSize = new Size(
                (int) (BackgroundImage.Width * 4f),
                (int) (BackgroundImage.Height * 4f)
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

            HeaderPanel.SlideIn(1f, delay: 0.05f);

            MainPanel.SlideIn(1f, delay: 0.1f);

        }

        private Pen _ResizeCornerPen = new Pen(Color.FromArgb(127, 255, 255, 255));
        private SolidBrush _BackgroundBrush = new SolidBrush(Color.FromArgb(127, 0, 0, 0));
        private SolidBrush _FPSBrush = new SolidBrush(Color.FromArgb(127, 255, 255, 255));
        private long _FrameStart;
        private long _FrameStartPrev;
        private float _CurrentFrameTime;
        protected override void OnPaintBackground(PaintEventArgs e) {
            _FrameStartPrev = _FrameStart;
            _FrameStart = _Stopwatch.ElapsedMilliseconds;
            _CurrentFrameTime = (_FrameStart - _FrameStartPrev) * 0.001f;

            Graphics g = e.Graphics;

            // This should obviously give a perf boost.
            g.CompositingQuality = CompositingQuality.HighSpeed;
            // ... it still renders blurred?
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            // And text's still fine?!?!
            g.SmoothingMode = SmoothingMode.HighSpeed;
            // Free perf boost!

            Point cursor = PointToClient(Cursor.Position);
            g.DrawBackgroundImage(
                BackgroundImage,
                Width,
                Height,
                (int) (BackgroundSize.Width * BackgroundSizeFactor),
                (int) (BackgroundSize.Height * BackgroundSizeFactor),
                ImageLayout.Center,
                -cursor.X * 0.1f,
                -cursor.Y * 0.1f
            );

            foreach (Panel panel in Controls)
                if (panel.Visible)
                    g.FillRectangle(_BackgroundBrush, panel.Left, panel.Top, panel.Width, panel.Height);

            for (int i = 0; i < 4; i++) {
                g.DrawLine(_ResizeCornerPen,
                    Width - 8,
                    Height - 1,
                    Width - 1,
                    Height - 8
                );
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
                if (cursor.X >= ClientSize.Width - 32 && cursor.Y >= ClientSize.Height - 32) {
                    m.Result = HTBOTTOMRIGHT;
                    return;
                }
            }

            base.WndProc(ref m);
        }
    }
}
