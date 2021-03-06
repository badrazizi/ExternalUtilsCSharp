
namespace SharpDXRenderer.Controls.Layouts
{
    /// <summary>
    /// An abstract class which offers methods to apply certain layouts to contents of a container-control
    /// </summary>
    public abstract class Layout
    {
        /// <summary>
        /// Applies a layout to the content of the given container-control
        /// </summary>
        /// <param name="parent"></param>
        public abstract void ApplyLayout(SharpDXControl parent);
    }
}
