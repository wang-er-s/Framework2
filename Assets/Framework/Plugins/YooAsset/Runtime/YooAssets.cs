using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	public static partial class YooAssets
	{
		public static bool IsInitialize { get; private set; }
		private static GameObject _driver = null;
		private static readonly List<AssetsPackage> _packages = new List<AssetsPackage>();

		/// <summary>
		/// 初始化资源系统
		/// </summary>
		public static void Initialize()
		{
			if (IsInitialize)
				throw new Exception($"{nameof(YooAssets)} is initialized !");

			if (IsInitialize == false)
			{
				// 创建驱动器
				IsInitialize = true;
				_driver = new UnityEngine.GameObject($"[{nameof(YooAssets)}]");
				_driver.AddComponent<YooAssetsDriver>();
				UnityEngine.Object.DontDestroyOnLoad(_driver);
				YooLogger.Log($"{nameof(YooAssets)} initialize !");

#if DEBUG
				// 添加远程调试脚本
				_driver.AddComponent<RemoteDebuggerInRuntime>();
#endif

				// 初始化异步系统
				OperationSystem.Initialize();
			}
		}

		/// <summary>
		/// 销毁资源系统
		/// </summary>
		public static void Destroy()
		{
			if (IsInitialize)
			{
				OperationSystem.DestroyAll();
				DownloadSystem.DestroyAll();
				CacheSystem.ClearAll();

				foreach (var package in _packages)
				{
					package.DestroyPackage();
				}
				_packages.Clear();

				IsInitialize = false;
				if (_driver != null)
					GameObject.Destroy(_driver);
				YooLogger.Log($"{nameof(YooAssets)} destroy all !");
			}
		}

		/// <summary>
		/// 更新资源系统
		/// </summary>
		internal static void Update()
		{
			if (IsInitialize)
			{
				OperationSystem.Update();
				DownloadSystem.Update();

				for (int i = 0; i < _packages.Count; i++)
				{
					_packages[i].UpdatePackage();
				}
			}
		}


		/// <summary>
		/// 创建资源包
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static AssetsPackage CreateAssetsPackage(string packageName)
		{
			if (IsInitialize == false)
				throw new Exception($"{nameof(YooAssets)} not initialize !");

			if (string.IsNullOrEmpty(packageName))
				throw new Exception("Package name is null or empty !");

			if (HasAssetsPackage(packageName))
				throw new Exception($"Package {packageName} already existed !");

			AssetsPackage assetsPackage = new AssetsPackage(packageName);
			_packages.Add(assetsPackage);
			return assetsPackage;
		}

		/// <summary>
		/// 获取资源包
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static AssetsPackage GetAssetsPackage(string packageName)
		{
			var package = TryGetAssetsPackage(packageName);
			if (package == null)
				YooLogger.Warning($"Not found assets package : {packageName}");
			return package;
		}

		/// <summary>
		/// 尝试获取资源包
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static AssetsPackage TryGetAssetsPackage(string packageName)
		{
			if (IsInitialize == false)
				throw new Exception($"{nameof(YooAssets)} not initialize !");

			if (string.IsNullOrEmpty(packageName))
				throw new Exception("Package name is null or empty !");

			foreach (var package in _packages)
			{
				if (package.PackageName == packageName)
					return package;
			}
			return null;
		}

		/// <summary>
		/// 检测资源包是否存在
		/// </summary>
		/// <param name="packageName">资源包名称</param>
		public static bool HasAssetsPackage(string packageName)
		{
			if (IsInitialize == false)
				throw new Exception($"{nameof(YooAssets)} not initialize !");

			foreach (var package in _packages)
			{
				if (package.PackageName == packageName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 开启一个异步操作
		/// </summary>
		/// <param name="operation">异步操作对象</param>
		public static void StartOperation(GameAsyncOperation operation)
		{
			OperationSystem.StartOperation(operation);
		}

		#region 系统参数
		/// <summary>
		/// 设置下载系统参数，启用断点续传功能文件的最小字节数
		/// </summary>
		public static void SetDownloadSystemBreakpointResumeFileSize(int fileBytes)
		{
			DownloadSystem.BreakpointResumeFileSize = fileBytes;
		}

		/// <summary>
		/// 设置下载系统参数，下载失败后清理文件的HTTP错误码
		/// </summary>
		public static void SetDownloadSystemClearFileResponseCode(List<long> codes)
		{
			DownloadSystem.ClearFileResponseCodes = codes;
		}

		/// <summary>
		/// 设置下载系统参数，自定义的证书认证实例
		/// </summary>
		public static void SetDownloadSystemCertificateHandler(UnityEngine.Networking.CertificateHandler instance)
		{
			DownloadSystem.CertificateHandlerInstance = instance;
		}

		/// <summary>
		/// 设置下载系统参数，自定义下载请求
		/// </summary>
		public static void SetDownloadSystemUnityWebRequest(DownloadRequestDelegate requestDelegate)
		{
			DownloadSystem.RequestDelegate = requestDelegate;
		}

		/// <summary>
		/// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
		/// </summary>
		public static void SetOperationSystemMaxTimeSlice(long milliseconds)
		{
			if (milliseconds < 30)
			{
				milliseconds = 30;
				YooLogger.Warning($"MaxTimeSlice minimum value is 30 milliseconds.");
			}
			OperationSystem.MaxTimeSlice = milliseconds;
		}

		/// <summary>
		/// 设置缓存系统参数，已经缓存文件的校验等级
		/// </summary>
		public static void SetCacheSystemCachedFileVerifyLevel(EVerifyLevel verifyLevel)
		{
			CacheSystem.InitVerifyLevel = verifyLevel;
		}
		#endregion

		#region 沙盒相关
		/// <summary>
		/// 获取内置文件夹名称
		/// </summary>
		public static string GetStreamingAssetBuildinFolderName()
		{
			return YooAssetSettings.StreamingAssetsBuildinFolder;
		}

		/// <summary>
		/// 获取沙盒的根路径
		/// </summary>
		public static string GetSandboxRoot()
		{
			return PathHelper.GetPersistentRootPath();
		}

		/// <summary>
		/// 清空沙盒目录
		/// </summary>
		public static void ClearSandbox()
		{
			PersistentHelper.DeleteSandbox();
		}
		#endregion

		#region 调试信息
		internal static DebugReport GetDebugReport()
		{
			DebugReport report = new DebugReport();
			report.FrameCount = Time.frameCount;

			foreach (var package in _packages)
			{
				var packageData = package.GetDebugPackageData();
				report.PackageDatas.Add(packageData);
			}
			return report;
		}
		#endregion
	}
}