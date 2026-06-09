using System.Drawing;

namespace Aspose.Email
{
	internal class Form
	{
		internal object StartPosition;

		public object FormBorderStyle { get; internal set; }
		public bool ShowInTaskbar { get; internal set; }
		public Point Location { get; internal set; }
		public Size Size { get; internal set; }
		public object Controls { get; internal set; }
		public Action<object, EventArgs> Load { get; internal set; }
	}
}