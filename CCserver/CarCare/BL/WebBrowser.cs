namespace Aspose.Email
{
	internal class WebBrowser
	{
		public bool AllowNavigation { get; internal set; }
		public Action<object, WebBrowserDocumentCompletedEventArgs> DocumentCompleted { get; internal set; }

		internal void Navigate(string v)
		{
			throw new NotImplementedException();
		}
	}
}