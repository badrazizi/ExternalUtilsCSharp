using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExternalUtilsCSharp.InputUtils;

namespace SharpDXRenderer.Controls
{
    public class SharpDXButtonKey : SharpDXButton
    {
        #region VARIABLES
        private int skip;
        private bool listen;
        private ExternalUtilsCSharp.WinAPI.VirtualKeyShort key;
        #endregion

        #region PROPERTIES
        public ExternalUtilsCSharp.WinAPI.VirtualKeyShort Key 
        {
            get { return this.key; }
            set
            {
                if(this.key != value)
                {
                    this.key = value;
                    OnKeyChangedEvent(new EventArgs());
                }
            }
        }
        #endregion


        #region EVENTS
        public event EventHandler KeyChangedEvent;
        protected virtual void OnKeyChangedEvent(EventArgs e)
        {
            if (KeyChangedEvent != null)
                KeyChangedEvent(this, e);
        }
        #endregion

        #region CONSTRUCTOR
        public SharpDXButtonKey() : base()
        {
            listen = false;
            this.key = ExternalUtilsCSharp.WinAPI.VirtualKeyShort.XBUTTON1;
            this.MouseClickEventUp += SharpDXButtonKey_MouseClickEventUp;
        }
        #endregion

        #region METHODS
        void SharpDXButtonKey_MouseClickEventUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            listen = true;
            skip = 10;
        }

        public override void Draw(SharpDXRenderer renderer)
        {
            string text = null;
            string orig = this.Text;
            if (listen)
                text = string.Format("{0} <press key>", this.Text);
            else
                text = string.Format("{0} {1}", this.Text, this.Key);
            this.Text = text;
            base.Draw(renderer);
            this.Text = orig;
        }

        public override void Update(double secondsElapsed, InputUtilities keyUtils, SharpDX.Vector2 cursorPoint, bool checkMouse = false)
        {
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
            if (listen)
            {
                if(skip > 0)
                {
                    skip--;
                    return;
                }
                ExternalUtilsCSharp.WinAPI.VirtualKeyShort[] buttons = keyUtils.Keys.KeysThatWentUp();
                if (buttons.Length > 0)
                {
                    Key = buttons[0];
                    listen = false;
                }
            }
        }

        public override void ApplySettings(ExternalUtilsCSharp.ConfigUtils config)
        {
            if (this.Tag != null)
                if (config.HasKey(this.Tag.ToString()))
                    this.Key = config.GetValue<ExternalUtilsCSharp.WinAPI.VirtualKeyShort>(this.Tag.ToString());
        }
        #endregion
    }
}
