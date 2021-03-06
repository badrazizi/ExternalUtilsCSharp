using ExternalUtilsCSharp.InputUtils;

namespace SharpDXRenderer.Controls
{
    /// <summary>
    /// Serves as a spacer between controls in a container (like a panel)
    /// </summary>
    public class SharpDXSpacer : SharpDXControl
    {
        public SharpDXSpacer() : base()
        {
            this.Height = 2f;
        }
        public override void Draw(SharpDXRenderer renderer)
        {
            renderer.FillRectangle(
                this.ForeColor,
                this.GetAbsoluteLocation(),
                this.GetSize());
            base.Draw(renderer);
        }

        public override void Update(double secondsElapsed, InputUtilities keyUtils, SharpDX.Vector2 cursorPoint, bool checkMouse = false)
        {
            if (this.Parent != null)
                this.Width = Parent.Width - this.MarginLeft - this.MarginRight - Parent.MarginLeft - Parent.MarginRight;
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
        }
    }
}
