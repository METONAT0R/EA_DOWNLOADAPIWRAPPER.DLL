using System;
using System.Runtime.InteropServices;

namespace DownloadProvider
{
	public class Akamai : DownloadProvider
	{
		private class AkamaiFile : DownloadFile
		{
			private Akamai mAkamai;

			private AkamaiAPI.AKFileAttributes mAttribs;

			public AkamaiFile(string url, string filename, Akamai akamai)
				: base(url, filename)
			{
				mAkamai = akamai;
			}

			public unsafe override void Cancel()
			{
				if (mAkamai.IsRunning && !string.IsNullOrEmpty(base.Url) && AkamaiAPI.ak_deleteFile(mAkamai.Context, 0, base.Url, IntPtr.Zero) != 0)
				{
					throw new Exception($"AkamaiFile : Failed to cancel file {base.Url}. Reason : {GetLastErrorString()}");
				}
			}

			public override Status GetStatus()
			{
				switch (GetAKStatus())
				{
				case AkamaiAPI.AKFileStatusCode.AK_FILE_UNKNOWN:
					return Status.Cancelled;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_SEARCHING:
					return Status.Downloading;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_WAITING:
					return Status.Downloading;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_DOWNLOADING:
					return Status.Downloading;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_PAUSED:
					return Status.Paused;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_COMPLETE:
					return Status.Downloaded;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_INSUFFICIENTDISKSPACE:
					return Status.Cancelled;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_DISCONNECTED:
					return Status.Cancelled;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_NOTFOUND:
					return Status.Cancelled;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_MOVING:
					return Status.Downloading;
				case AkamaiAPI.AKFileStatusCode.AK_FILE_FORBIDDEN:
					return Status.Cancelled;
				default:
					return Status.Cancelled;
				}
			}

			private unsafe AkamaiAPI.AKFileStatusCode GetAKStatus()
			{
				if (!mAkamai.IsRunning || string.IsNullOrEmpty(base.Url))
				{
					return AkamaiAPI.AKFileStatusCode.AK_FILE_UNKNOWN;
				}
				if (AkamaiAPI.ak_getFileAttributes(mAkamai.Context, 0, base.Url, ref mAttribs, IntPtr.Zero) != 0)
				{
					throw new Exception($"AkamaiFile : Failed to get status for file {base.Url}. Reason : {GetLastErrorString()}");
				}
				return mAttribs.status;
			}

			public unsafe override string GetCacheFileName()
			{
				if (!mAkamai.IsRunning || string.IsNullOrEmpty(base.Url))
				{
					return string.Empty;
				}
				if (AkamaiAPI.ak_getFileAttributes(mAkamai.Context, 0, base.Url, ref mAttribs, IntPtr.Zero) != 0)
				{
					throw new Exception($"AkamaiFile : Failed to get cache file name for file {base.Url}. Reason : {GetLastErrorString()}");
				}
				return Marshal.PtrToStringAnsi(mAttribs.path);
			}

			public override int GetLastErrorCode()
			{
				return AkamaiAPI.ak_getLastError();
			}
		}

		private const string ApplicationName = "NFSW Game Launcher";

		private unsafe AkamaiAPI._Akamai* mContext;

		private long AmountDownloaded;

		public unsafe AkamaiAPI._Akamai* Context => mContext;

		public Akamai()
			: base("akamai")
		{
			AmountDownloaded = 0L;
		}

		public override void Install(bool currentProcessHasAdminRights)
		{
			if (currentProcessHasAdminRights)
			{
				if (AkamaiAPI.ak_install("NFSW Game Launcher", 0, "NFSW_EUALA_v1.0", IntPtr.Zero) != 0)
				{
					throw new Exception(string.Format("Akamai : Failed to install akamai services for application {0}. Reason : {1}", "NFSW Game Launcher", GetLastErrorString()));
				}
			}
			else
			{
				DownloadProvider.RunAsAdmin("AkamaiInstall.exe", string.Format("\"{0}\"", "NFSW Game Launcher"));
			}
		}

		public unsafe override void Start(string url)
		{
			AmountDownloaded = 0L;
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
			try
			{
				if (AkamaiAPI.ak_initialize("NFSW Game Launcher", intPtr, IntPtr.Zero) != 0)
				{
					throw new Exception(string.Format("Akamai : Failed to get initialize application {0}. Reason : {1}", "NFSW Game Launcher", GetLastErrorString()));
				}
				mContext = (AkamaiAPI._Akamai*)((IntPtr)Marshal.PtrToStructure(intPtr, typeof(IntPtr))).ToPointer();
				mIsRunning = true;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		public unsafe override void Stop()
		{
			if (mIsRunning)
			{
				mIsRunning = false;
				if (AkamaiAPI.ak_uninitialize(mContext, IntPtr.Zero) != 0)
				{
					throw new Exception(string.Format("Akamai : Failed to get uninitialize application {0}. Reason : {1}", "NFSW Game Launcher", GetLastErrorString()));
				}
			}
		}

		public static string GetLastErrorString()
		{
			AkamaiAPI.AKErrorCode code = (AkamaiAPI.AKErrorCode)AkamaiAPI.ak_getLastError();
			return AkamaiAPI.ak_errorToString(code);
		}

		public unsafe override DownloadFile Download(string url, string filename)
		{
			if (base.IsRunning)
			{
				string text = url + filename;
				text = text.Replace("/static.cdn", "/csdstatic.cdn");
				AkamaiFile result = new AkamaiFile(text, filename, this);
				AkamaiAPI.AKExtraParameters extra = default(AkamaiAPI.AKExtraParameters);
				if (AkamaiAPI.ak_downloadFile(mContext, 0, text, ref extra) != 0)
				{
					throw new Exception($"AkamaiFile : Failed to request download for file {text}. Reason : {GetLastErrorString()}");
				}
				return result;
			}
			return null;
		}

		public override long GetAmountDownloaded()
		{
			return AmountDownloaded;
		}

		public override int GetLastErrorCode()
		{
			return AkamaiAPI.ak_getLastError();
		}
	}
}
