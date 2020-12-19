using System;
using System.Runtime.InteropServices;

public static class AkamaiAPI
{
	public delegate int onInstallProgress(int percent, IntPtr extra);

	public enum AKFileStatusCode
	{
		AK_FILE_UNKNOWN,
		AK_FILE_SEARCHING,
		AK_FILE_WAITING,
		AK_FILE_DOWNLOADING,
		AK_FILE_PAUSED,
		AK_FILE_COMPLETE,
		AK_FILE_INSUFFICIENTDISKSPACE,
		AK_FILE_DISCONNECTED,
		AK_FILE_NOTFOUND,
		AK_FILE_MOVING,
		AK_FILE_FORBIDDEN
	}

	public enum AKErrorCode
	{
		AK_OK,
		AK_ERR_INVALIDARGUMENT,
		AK_ERR_NOTINSTALLED,
		AK_ERR_INTERNAL,
		AK_ERR_INSUFFICIENTMEMORY,
		AK_ERR_NETWORK,
		AK_ERR_INI,
		AK_ERR_DISK,
		AK_ERR_PLATFORM,
		AK_ERR_START,
		AK_ERR_STOP,
		AK_ERR_JSON,
		AK_ERR_FILEINUSE,
		AK_ERR_FILENOTFOUND,
		AK_ERR_CLIENT_NOTRESPONDING,
		AK_ERR_UNABLE_TO_DOWNLOAD,
		AK_ERR_DISK_WRITE,
		AK_ERR_INI_PATH_NOT_FOUND,
		AK_ERR_INI_PARSE_ERROR,
		AK_ERR_DISK_PATH_OPEN,
		AK_ERR_FILE_ACCESS,
		AK_ERR_VERIFICATION_FAILURE,
		AK_ERR_FILE_DOWNLOADED,
		AK_ERR_RUN_TASKS_THROTTLED,
		AK_ERR_CHALLENGE_RESPONSE
	}

	public struct AKExtraParameters
	{
		public string downloadPath;

		public onInstallProgress onP;

		public string cookiestring;

		public string queryauth;

		public string basicauth_username;

		public string basicauth_password;

		public string errorInfo;

		public string originUrl;

		public string resumeUrl;

		public string resumeFileName;

		public int size
		{
			get;
			set;
		}

		public int flags
		{
			get;
			set;
		}

		public int targetSpeed
		{
			get;
			set;
		}

		public int delayHttp
		{
			get;
			set;
		}

		public int keepPhysicalFileOnDelete
		{
			get;
			set;
		}

		public int urlSetId
		{
			get;
			set;
		}

		public int autoPause
		{
			get;
			set;
		}
	}

	public struct AKFileAttributes
	{
		public int size
		{
			get;
			set;
		}

		public int flags
		{
			get;
			set;
		}

		public int cpcode
		{
			get;
			set;
		}

		public IntPtr url
		{
			get;
			set;
		}

		public AKFileStatusCode status
		{
			get;
			set;
		}

		public IntPtr path
		{
			get;
			set;
		}

		public long totalSize
		{
			get;
			set;
		}

		public long uniqueRecv
		{
			get;
			set;
		}

		public long progress
		{
			get;
			set;
		}

		public long downloadRate
		{
			get;
			set;
		}

		public IntPtr errorString
		{
			get;
			set;
		}

		public long errorCode
		{
			get;
			set;
		}

		public long subErrorCode
		{
			get;
			set;
		}

		public IntPtr verifiedContentMd5
		{
			get;
			set;
		}

		public long totalRecv
		{
			get;
			set;
		}

		public int resetDownload
		{
			get;
			set;
		}
	}

	public struct AKAttributes
	{
		public int size
		{
			get;
			set;
		}

		public int flags
		{
			get;
			set;
		}

		public int isOnline
		{
			get;
			set;
		}

		public long shareRate
		{
			get;
			set;
		}

		public long downloadRate
		{
			get;
			set;
		}

		public int version
		{
			get;
			set;
		}

		public int acceptedEULA
		{
			get;
			set;
		}

		public IntPtr executableVersion
		{
			get;
			set;
		}

		public IntPtr configurationVersion
		{
			get;
			set;
		}

		public IntPtr guid
		{
			get;
			set;
		}

		public int uninstallMsgSent
		{
			get;
			set;
		}

		public long uploadsPausedUntilSecs
		{
			get;
			set;
		}

		public IntPtr runningExecutable
		{
			get;
			set;
		}

		public IntPtr runningClientBinId
		{
			get;
			set;
		}
	}

	public struct _Akamai
	{
		public IntPtr applicationName
		{
			get;
			set;
		}

		public int openSocket
		{
			get;
			set;
		}
	}

	[DllImport("akamaiDLL.dll")]
	public static extern int ak_install([MarshalAs(UnmanagedType.LPStr)] string applicationName, int cpcode, [MarshalAs(UnmanagedType.LPStr)] string eulaString, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public static extern int ak_uninstall([MarshalAs(UnmanagedType.LPStr)] string applicationName, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public static extern int ak_initialize([MarshalAs(UnmanagedType.LPStr)] string applicationName, IntPtr akamai, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public unsafe static extern int ak_uninitialize(_Akamai* akamai, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public static extern int ak_acceptEULA(int cpcode, [MarshalAs(UnmanagedType.LPStr)] string versionName, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public unsafe static extern int ak_downloadFile(_Akamai* akamai, int cpcode, [MarshalAs(UnmanagedType.LPStr)] string url, ref AKExtraParameters extra);

	[DllImport("akamaiDLL.dll")]
	public unsafe static extern int ak_pauseFile(_Akamai* akamai, int cpcode, [MarshalAs(UnmanagedType.LPStr)] string url, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public unsafe static extern int ak_deleteFile(_Akamai* akamai, int cpcode, [MarshalAs(UnmanagedType.LPStr)] string url, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public unsafe static extern int ak_getFileAttributes(_Akamai* akamai, int cpcode, [MarshalAs(UnmanagedType.LPStr)] string url, ref AKFileAttributes fileAttributes, IntPtr extra);

	[DllImport("akamaiDLL.dll")]
	public static extern int ak_getLastError();

	[DllImport("akamaiDLL.dll")]
	public static extern string ak_errorToString(AKErrorCode code);

	[DllImport("akamaiDLL.dll")]
	public unsafe static extern int ak_getAttributes(_Akamai* akamai, int cpcode, ref AKAttributes attributes, IntPtr extra);
}
