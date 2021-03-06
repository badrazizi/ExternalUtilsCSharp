using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ExternalUtilsCSharp.InputUtils;

namespace ExternalUtilsCSharp.UI
{
    /// <summary>
    /// An abstract class which implements the basics of an external overlay(-window)
    /// </summary>
    /// <typeparam name="TColor">Color-type</typeparam>
    /// <typeparam name="TVector2">Vector2-type</typeparam>
    /// <typeparam name="TFont">Font-type</typeparam>
    public abstract class Overlay<TRenderer, TColor, TVector2, TFont> : Form where TRenderer : Renderer<TColor, TVector2, TFont>
    {
        #region VARIABLES
        private Updater updLogic, updDraw;
        private long lastTimerTick, lastDrawTick;
        private IntPtr handle;
        #endregion

        #region PROPERTIES
        public TRenderer Renderer { get; protected set; }
        /// <summary>
        /// Whether to perform drawing-operations when the target-window is in foreground only
        /// </summary>
        public bool DrawOnlyWhenInForeground { get; set; }
        /// <summary>
        /// Whether this overlay will automatically move and resize itself to properly cover the target-window
        /// </summary>
        public bool TrackTargetWindow { get; set; }
        public IntPtr hWnd { get; protected set; }
        public List<UI.Control<TRenderer, TColor, TVector2, TFont>> ChildControls { get; set; }
        public System.Drawing.Point CursorPosition { get { return this.PointToClient(System.Windows.Forms.Cursor.Position); } }
        public Updater DrawUpdater { get { return updDraw; } }
        public Updater LogicUpdater { get { return updLogic; } }
        #endregion

        #region EVENTS
        public event EventHandler<DeltaEventArgs> TickEvent;
        public event EventHandler<OverlayEventArgs> BeforeDrawingEvent;
        public event EventHandler<OverlayEventArgs> AfterDrawingEvent;

        public virtual void OnTickEvent(DeltaEventArgs e)
        {
            if (TickEvent != null)
                TickEvent(this, e);
        }
        public class OverlayEventArgs : EventArgs
        {
            public Overlay<TRenderer, TColor, TVector2, TFont> Overlay { get; private set; }
            public OverlayEventArgs(Overlay<TRenderer, TColor, TVector2, TFont> overlay)
                : base()
            {
                this.Overlay = overlay;
            }
        }
        public class DeltaEventArgs : OverlayEventArgs
        {
            public double SecondsElapsed { get; private set; }
            public DeltaEventArgs(double secondsElapsed, Overlay<TRenderer, TColor, TVector2, TFont> overlay)
                : base(overlay)
            {
                this.SecondsElapsed = secondsElapsed;
            }
        }

        public virtual void OnBeforeDrawingEvent(OverlayEventArgs e)
        {
            if (BeforeDrawingEvent != null)
                BeforeDrawingEvent(this, e);
        }

        public virtual void OnAfterDrawingEvent(OverlayEventArgs e)
        {
            if (AfterDrawingEvent != null)
                AfterDrawingEvent(this, e);
        }
        #endregion

        #region CONSTUCTOR
        public Overlay()
        {
            //Setup form-properties
            this.BackColor = System.Drawing.Color.Black;
            this.TransparencyKey = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "";
            this.Name = "";
            this.TopMost = true;
            this.TopLevel = true;

            //Make form transparent and fully topmost
            int initialStyle = WinAPI.GetWindowLong(this.Handle, (int)WinAPI.GetWindowLongFlags.GWL_EXSTYLE);
            WinAPI.SetWindowLong(this.Handle, (int)WinAPI.GetWindowLongFlags.GWL_EXSTYLE, initialStyle | (int)WinAPI.ExtendedWindowStyles.WS_EX_LAYERED | (int)WinAPI.ExtendedWindowStyles.WS_EX_TRANSPARENT);
            WinAPI.SetWindowPos(this.Handle, (IntPtr)WinAPI.SetWindpwPosHWNDFlags.TopMost, 0, 0, 0, 0, (uint)WinAPI.SetWindowPosFlags.NOMOVE | (uint)WinAPI.SetWindowPosFlags.NOSIZE);
            WinAPI.SetLayeredWindowAttributes(this.Handle, 0, 255, (uint)WinAPI.LayeredWindowAttributesFlags.LWA_ALPHA);

            //Controls
            updLogic = new Updater(256);
            updLogic.TickEvent += updLogic_TickEvent;
            updDraw = new Updater(256);
            updDraw.TickEvent += updDraw_TickEvent;
            lastTimerTick = DateTime.Now.Ticks;
            lastDrawTick = DateTime.Now.Ticks;

            //Overlay-properties
            this.DrawOnlyWhenInForeground = true;
            this.TrackTargetWindow = true;
            this.ChildControls = new List<UI.Control<TRenderer, TColor, TVector2, TFont>>();
            this.handle = this.Handle;
        }

        void updDraw_TickEvent(object sender, Updater.DeltaEventArgs e)
        {
            TimeSpan deltaDraw = new TimeSpan(DateTime.Now.Ticks - lastDrawTick);
            lastDrawTick = DateTime.Now.Ticks;
            this.Invoke((MethodInvoker)(() => { this.OnDraw(deltaDraw.TotalSeconds); }));
        }

        void updLogic_TickEvent(object sender, Updater.DeltaEventArgs e)
        {
            TimeSpan deltaTimer = new TimeSpan(DateTime.Now.Ticks - lastTimerTick);
            lastTimerTick = DateTime.Now.Ticks;
            this.Invoke((MethodInvoker)(() => { this.OnTick(deltaTimer.TotalSeconds); }));
        }
        #endregion

        #region METHODS
        /// <summary>
        /// Perform your drawing-operations here
        /// </summary>
        /// <param name="seconds">Time in seconds since the last Draw-call</param>
        protected virtual void OnDraw(double seconds)
        {
            if (this.DrawOnlyWhenInForeground)
            {
                if (WinAPI.GetForegroundWindow() != this.hWnd)
                    return;
            }

            WinAPI.MARGINS margins = new WinAPI.MARGINS();
            margins.topHeight = 0; //this.Top;
            margins.bottomHeight = 0; // this.Bottom;
            margins.leftWidth = this.Left;
            margins.rightWidth = this.Right;
            this.Invoke((MethodInvoker)(() => { WinAPI.DwmExtendFrameIntoClientArea(this.Handle, ref margins); }));            

            this.Renderer.BeginDraw();
            this.Renderer.Clear(this.Renderer.GetRendererBackColor());
            this.OnBeforeDrawingEvent(new OverlayEventArgs(this));

            foreach (UI.Control<TRenderer, TColor, TVector2, TFont> control in ChildControls)
                if(control.Visible)
                    control.Draw(this.Renderer);

            this.OnAfterDrawingEvent(new OverlayEventArgs(this));
            this.Renderer.EndDraw();
        }
        /// <summary>
        /// Perform your logic-operations here
        /// </summary>
        /// <param name="seconds">Time in seconds since the last Tick-call</param>
        protected virtual void OnTick(double seconds)
        {
            if (this.TrackTargetWindow)
            {
                WinAPI.WINDOWINFO info = new WinAPI.WINDOWINFO();
                if(WinAPI.GetWindowInfo(this.hWnd, ref info))
                {
                    int width = info.rcWindow.Right - info.rcWindow.Left;
                    int height = info.rcWindow.Bottom - info.rcWindow.Top;
                    if(this.Location.X != info.rcClient.Left ||
                        this.Location.Y != info.rcClient.Top)
                    {
                        this.Location = new System.Drawing.Point(info.rcClient.Left, info.rcClient.Top);
                    }
                    if(this.Width != info.rcClient.Right - info.rcClient.Left ||
                        this.Height != info.rcClient.Bottom - info.rcClient.Top)
                    {
                        this.Size = new System.Drawing.Size(info.rcClient.Right - info.rcClient.Left, info.rcClient.Bottom - info.rcClient.Top);
                            this.OnResize();
                    }
                    WinAPI.SetWindowPos(this.hWnd, this.handle, info.rcWindow.Left, info.rcWindow.Top, width, height, 0);
                }
            }
            
            OnTickEvent(new DeltaEventArgs(seconds, this));
        }
        /// <summary>
        /// Attach to the given window-handle (and initialize your device here)
        /// </summary>
        /// <param name="hWnd">Handle to the window to attach to</param>
        public virtual void Attach(IntPtr hWnd)
        {
            updDraw.StartUpdater();
            updLogic.StartUpdater();
        }
        /// <summary>
        /// Detach from the window-handle which was earler attached to (and destroy your device here)
        /// </summary>
        public virtual void Detach()
        {
            updDraw.StopUpdater();
            updLogic.StopUpdater();
        }
        /// <summary>
        /// Is called whenever this form is resized, resize your renderer here
        /// </summary>
        /// <param name="size"></param>
        public abstract void OnResize();
        /// <summary>
        /// Call this method to update all controls on this form
        /// </summary>
        /// <param name="secondsElapsed"></param>
        /// <param name="keys"></param>
        public abstract void UpdateControls(double secondsElapsed, InputUtilities keys);
        public virtual void Kill()
        {
            updDraw.StopUpdater();
            updLogic.StopUpdater();
            Application.Exit();
        }

        public void ShowInactiveTopmost()
        {
            WinAPI.ShowWindow(this.Handle, (int)WinAPI.WindowShowStyle.ShowNoActivate);
            WinAPI.SetWindowPos(this.Handle, (IntPtr)WinAPI.SetWindpwPosHWNDFlags.TopMost,
            this.Left, this.Top, this.Width, this.Height,
            (uint)WinAPI.SetWindowPosFlags.NOACTIVATE);
        }
        public void ResetTopmost()
        {
            WinAPI.BringWindowToTop(this.Handle);
        }
        #endregion
    }
}
