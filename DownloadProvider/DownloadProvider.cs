using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DownloadProvider
{
	public abstract class DownloadProvider : IDisposable
	{
		protected bool mIsRunning;

		protected bool isDisposed;

		private string mName;

		public string Name => mName;

		public bool IsRunning => mIsRunning;

		public DownloadProvider(string name)
		{
			mIsRunning = false;
			mName = name;
		}

		~DownloadProvider()
		{
			Dispose(disposing: false);
		}

		protected static string SetupCacheFolder(string url)
		{
			Uri uri = new Uri(url);
			string[] segments = uri.Segments;
			string text = Path.GetTempPath() + "NFSW";
			string path = segments[segments.Length - 1];
			string text2 = Path.Combine(text, path);
			try
			{
				Directory.CreateDirectory(text);
				Directory.CreateDirectory(text2);
				string[] directories = Directory.GetDirectories(text);
				foreach (string text3 in directories)
				{
					if (text3 != text2)
					{
						try
						{
							Directory.Delete(text3);
						}
						catch
						{
						}
					}
				}
				return text2;
			}
			catch
			{
				return text2;
			}
		}

		protected static void RunAsAdmin(string application, string args)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.Verb = "runas";
			processStartInfo.FileName = Path.Combine(directoryName, application);
			processStartInfo.Arguments = args;
			processStartInfo.UseShellExecute = true;
			try
			{
				Process process = Process.Start(processStartInfo);
				process.WaitForExit();
			}
			catch (Exception ex)
			{
				throw new Exception($"DownloadProvider : Failed to RunAsAdmin for application {application} with args {args}. Reason : {ex.Message}");
			}
		}

		public string GetName()
		{
			return mName;
		}

		public abstract void Install(bool currentProcessHasAdminRights);

		public abstract void Start(string url);

		public abstract void Stop();

		public abstract DownloadFile Download(string url, string filename);

		public abstract int GetLastErrorCode();

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;
			}
		}

		public abstract long GetAmountDownloaded();
	}
}
