﻿using System.IO;
using UnityEngine;

namespace YooAsset
{
	public class DefaultLocationServices : ILocationServices
	{
		private readonly string _resourceRoot;

		public DefaultLocationServices(string resourceRoot)
		{
			if (string.IsNullOrEmpty(resourceRoot) == false)
				_resourceRoot = PathHelper.GetRegularPath(resourceRoot);
		}
		public string ConvertLocationToAssetPath(YooAssets.EPlayMode playMode, string location)
		{
#if UNITY_EDITOR
			CheckLocation(location);
#endif

			if (playMode == YooAssets.EPlayMode.EditorPlayMode)
			{
				string filePath = CombineAssetPath(_resourceRoot, location);
				return FindDatabaseAssetPath(filePath);
			}
			else
			{
				return CombineAssetPath(_resourceRoot, location);
			}
		}

		/// <summary>
		/// 合并资源路径
		/// </summary>
		private static string CombineAssetPath(string root, string location)
		{
			if (string.IsNullOrEmpty(root))
				return location;
			else
				return $"{root}/{location}";
		}

		/// <summary>
		/// 获取AssetDatabase的加载路径
		/// </summary>
		private static string FindDatabaseAssetPath(string filePath)
		{
#if UNITY_EDITOR
			if (File.Exists(filePath))
				return filePath;

			// AssetDatabase加载资源需要提供文件后缀格式，然而资源定位地址并没有文件格式信息。
			// 所以我们通过查找该文件所在文件夹内同名的首个文件来确定AssetDatabase的加载路径。
			// 注意：AssetDatabase.FindAssets() 返回文件内包括递归文件夹内所有资源的GUID
			string fileName = Path.GetFileName(filePath);
			string directory = PathHelper.GetDirectory(filePath);
			string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Empty, new[] { directory });
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);

				if (UnityEditor.AssetDatabase.IsValidFolder(assetPath))
					continue;

				string assetDirectory = PathHelper.GetDirectory(assetPath);
				if (assetDirectory != directory)
					continue;

				string assetName = Path.GetFileNameWithoutExtension(assetPath);
				if (assetName == fileName)
					return assetPath;
			}

			// 没有找到同名的资源文件
			YooLogger.Warning($"Not found asset : {filePath}");
			return filePath;
#else
			throw new System.NotImplementedException();
#endif
		}

#if UNITY_EDITOR
		private void CheckLocation(string location)
		{
			if (string.IsNullOrEmpty(location))
			{
				Debug.LogError("location param is null or empty!");
			}
			else
			{
				// 检查路径末尾是否有空格
				int index = location.LastIndexOf(" ");
				if (index != -1)
				{
					if (location.Length == index + 1)
						Debug.LogWarning($"Found blank character in location : \"{location}\"");
				}

				if (location.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
					Debug.LogWarning($"Found illegal character in location : \"{location}\"");
			}
		}
#endif
	}
}