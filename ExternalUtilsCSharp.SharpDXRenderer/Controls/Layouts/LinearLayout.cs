
namespace SharpDXRenderer.Controls.Layouts
{
    public class LinearLayout : Layout
    {
        #region SINGLETON
        private static LinearLayout instance = new LinearLayout();
        public static Layout Instance { get { return instance; } }
        private LinearLayout() : base()
        { }
        #endregion

        public override void ApplyLayout(SharpDXControl parent)
        {
            for (int i = 0; i < parent.ChildControls.Count; i++)
            {
                var control = parent.ChildControls[i];
                if (!control.Visible)
                    continue;

                if (control.FillParent)
                    control.Width = parent.Width - parent.MarginLeft - parent.MarginRight - control.MarginLeft - control.MarginRight;

                if (i == 0)
                {
                    control.X = control.MarginLeft + parent.MarginLeft;
                    control.Y = control.MarginTop;
                }
                else
                {
                    var lastControl = parent.ChildControls[i - 1];
                    control.X = lastControl.X;
                    control.Y = lastControl.Y + lastControl.Height + lastControl.MarginBottom + control.MarginTop;
                }
            }
        }
    }
}
