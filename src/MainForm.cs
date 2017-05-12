using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

                if (!AnimationManager.SupportsFast) {
                    cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                }

                return cp;
            }
        }

        public static Random RNG = new Random();

        public Size BackgroundSize;
        public float BackgroundSizeFactor;

        private PrivateFontCollection _Fonts;
        private Font _Font;

        public MainForm() {
            InitializeComponent();

            SuspendLayout();
            Size = new Size(460, 600);
            ResumeLayout(false);
            MinimumSize = Size;
        }


        private void MainForm_Load(object _s, EventArgs _e) {
            SuspendLayout();

            if (AnimationManager.SupportsFast) {
                // Nop. Everything works perfectly fine... somehow!
            } else {
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                DoubleBuffered = true;
            }

            ResizeRedraw = true;

            ResizeBegin += (s, e) => AnimationManager.IsThrottled = true;
            ResizeEnd += (s, e) => AnimationManager.IsThrottled = false;

            SizeChanged += (s, e) => {
                HeaderPanel.Width = Width;
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

            ResumeLayout(true);

            AnimationManager.AnimationRoot = this;
            Invalidated += (s, e) => AnimationManager.InvalidatedRoot++;

            this.Animate((a, t) => Opacity = t, dur: 0.4f, smooth: true);
            this.Animate((a, t) => BackgroundSizeFactor = 1f + 8f * (1f - t), dur: 2f, easing: Easings.QuinticEaseOut, smooth: true);

            HeaderPanel.SlideIn(1f, 461, 1, delay: 0.05f);

            MainPanel.SlideIn(1f, 461, 1, delay: 0.1f);

            MainPanel.Animate(new AnimationSequence() {
                Sequence = {
                    MainPanel.AnimationDelay(4f, run: false),
                    MainPanel.SlideOut(1f, run: false),
                    MainPanel.SlideIn(1f, run: false),
                    MainPanel.SlideOut(1f, run: false),
                    MainPanel.SlideIn(1f, run: false),
                    MainPanel.SlideOut(1f, run: false),
                    MainPanel.SlideIn(1f, run: false),
                }
            });

            ProgressPanel.Animate(new AnimationSequence() {
                Sequence = {
                    ProgressPanel.AnimationDelay(4f, run: false),
                    ProgressPanel.SlideIn(1f, run: false),
                    ProgressPanel.SlideOut(1f, run: false),
                    ProgressPanel.SlideIn(1f, run: false),
                    ProgressPanel.SlideOut(1f, run: false),
                    ProgressPanel.SlideIn(1f, run: false),
                    ProgressPanel.SlideOut(1f, run: false),
                }
            });

        }

        private Pen _ResizeCornerPen = new Pen(Color.FromArgb(127, 255, 255, 255));
        private SolidBrush _BackgroundBrush = new SolidBrush(Color.FromArgb(127, 0, 0, 0));
        protected override void OnPaintBackground(PaintEventArgs e) {
            Graphics g = e.Graphics;

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

            foreach (Panel panel in Controls) {
                g.FillRectangle(_BackgroundBrush, panel.Left, panel.Top, panel.Width, panel.Height);
            }

            for (int i = 0; i < 4; i++) {
                g.DrawLine(_ResizeCornerPen,
                    Width - 8,
                    Height - 1,
                    Width - 1,
                    Height - 8
                );
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
