namespace DownloadProvider
{
	public abstract class DownloadFile
	{
		public enum Status
		{
			Queued,
			Downloading,
			Downloaded,
			Paused,
			Cancelled,
			Timeout
		}

		private string mUrl;

		private string mFilename;

		public string Url => mUrl;

		public string Filename => mFilename;

		protected DownloadFile(string url, string filename)
		{
			mUrl = url;
			mFilename = filename;
		}

		public abstract void Cancel();

		public abstract Status GetStatus();

		public abstract string GetCacheFileName();

		public abstract int GetLastErrorCode();
	}
}
