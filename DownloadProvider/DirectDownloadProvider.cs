using log4net;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace DownloadProvider
{
	public class DirectDownloadProvider : DownloadProvider
	{
		private class DirectDownloadFile : DownloadFile
		{
			private const int BufferSize = 1048576;

			private static readonly ILog smLogger = LogManager.GetLogger("DirectDownloadFile");

			private HttpWebRequest mRequest;

			private HttpWebResponse mResponse;

			private string mCacheFilename;

			public Status mStatus;

			private DirectDownloadProvider mParent;

			public DirectDownloadFile(string url, string filename, DirectDownloadProvider parent, HttpWebRequest request, string cacheFilename)
				: base(url, filename)
			{
				if (File.Exists(cacheFilename))
				{
					mRequest = null;
					mStatus = Status.Downloaded;
				}
				else
				{
					mRequest = request;
					mRequest.Method = "GET";
					mRequest.BeginGetResponse(ReadCallback, this);
					mStatus = Status.Downloading;
				}
				mCacheFilename = cacheFilename;
				mParent = parent;
			}

			public override void Cancel()
			{
				smLogger.DebugFormat("DDF::Cancel {0}", mCacheFilename);
				if (mRequest != null)
				{
					mRequest.Abort();
				}
			}

			public override Status GetStatus()
			{
				return mStatus;
			}

			public override string GetCacheFileName()
			{
				return mCacheFilename;
			}

			public override int GetLastErrorCode()
			{
				return (int)mResponse.StatusCode;
			}

			private static void ReadCallback(IAsyncResult asyncResult)
			{
				DirectDownloadFile directDownloadFile = (DirectDownloadFile)asyncResult.AsyncState;
				try
				{
					directDownloadFile.mResponse = (HttpWebResponse)directDownloadFile.mRequest.EndGetResponse(asyncResult);
					using (Stream stream = directDownloadFile.mResponse.GetResponseStream())
					{
						if (!asyncResult.IsCompleted || directDownloadFile.mResponse.StatusCode != HttpStatusCode.OK)
						{
							return;
						}
						Directory.CreateDirectory(Path.GetDirectoryName(directDownloadFile.mCacheFilename));
						using (FileStream fileStream = new FileStream(directDownloadFile.mCacheFilename, FileMode.Create))
						{
							long num = directDownloadFile.mResponse.ContentLength;
							byte[] buffer = new byte[1048576];
							while (num > 0)
							{
								int i;
								int num2;
								for (i = 0; num > i && i < 1048576; i += num2)
								{
									num2 = stream.Read(buffer, i, 1048576 - i);
								}
								fileStream.Write(buffer, 0, i);
								Interlocked.Add(ref directDownloadFile.mParent.AmountDownloaded, i);
								num -= i;
							}
						}
						directDownloadFile.mStatus = Status.Downloaded;
					}
				}
				catch (Exception)
				{
					directDownloadFile.mStatus = Status.Cancelled;
				}
			}
		}

		private readonly ILog mLogger = LogManager.GetLogger("DirectDownloadProvider");

		private long AmountDownloaded;

		private string mCacheFolder;

		public DirectDownloadProvider()
			: base("direct")
		{
			mCacheFolder = null;
			AmountDownloaded = 0L;
		}

		public override void Start(string url)
		{
			mLogger.Info("Start");
			AmountDownloaded = 0L;
			mCacheFolder = DownloadProvider.SetupCacheFolder(url);
		}

		public override void Stop()
		{
			mLogger.Info("Stop");
			try
			{
				Directory.Delete(mCacheFolder, recursive: true);
			}
			catch
			{
				mLogger.InfoFormat("Failed to delete the cache directory {0}", mCacheFolder);
			}
		}

		public override void Install(bool currentProcessHasAdminRights)
		{
		}

		public override DownloadFile Download(string url, string filename)
		{
			string requestUriString = url + filename;
			mLogger.DebugFormat("Download {0}", filename);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
			if (httpWebRequest != null)
			{
				string cacheFilename = mCacheFolder + filename;
				return new DirectDownloadFile(url, filename, this, httpWebRequest, cacheFilename);
			}
			return null;
		}

		public override long GetAmountDownloaded()
		{
			return AmountDownloaded;
		}

		public override int GetLastErrorCode()
		{
			return 200;
		}
	}
}
