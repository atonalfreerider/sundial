/*
 * Copyright (c) 2015-2018 Beebyte Limited. All rights reserved.
 */
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Beebyte.Obfuscator.Assembly;

namespace Beebyte.Obfuscator
{
	public class Postbuild
	{
		private static Options _options;
		private static readonly IDictionary<string, string> DllsToRevert = new Dictionary<string, string>();

		private static bool _obfuscatedAfterScene;
		private static bool _noCSharpScripts;
		private static bool _monoBehaviourAssetsNeedReverting;
		private static bool _dllsNeedRestoring;
		private static bool _hasError;

		[InitializeOnLoad]
		public static class PostbuildStatic
		{
			/*
			 * Often Unity's EditorApplication.update delegate is reset. Because it's so important to restore
			 * renamed MonoBehaviour assets we assign here where it will be called after scripts are compiled.
			 */ 
			static PostbuildStatic()
			{
				EditorApplication.update += RestoreAssets;
				EditorApplication.update += RestoreOriginalDlls;
				_hasError = false;
			}
		}

		[PostProcessBuild(1)]
		private static void PostBuildHook(BuildTarget buildTarget, string pathToBuildProject)
		{
			if (!_options || (_options.enabled && _obfuscatedAfterScene == false))
			{
				if (_noCSharpScripts) Debug.LogWarning("No obfuscation required because no C# scripts were found");
				else Debug.LogError("Failed to obfuscate");
			}
			else
			{
				if (_monoBehaviourAssetsNeedReverting) RestoreAssets();
				if (_dllsNeedRestoring) RestoreOriginalDlls();
			}
			Clear();
		}

		private static void Clear()
		{
			_obfuscatedAfterScene = false;
			_noCSharpScripts = false;
			_hasError = false;

			if (_options != null && _options.obfuscateMonoBehaviourClassNames == false) Obfuscator.Clear();
		}

		/**
		 * When multiple DLLs are obfuscated, usually the extra DLLs need to be reverted back to their original state.
		 * This method backs up the DLLs to be reverted after obfuscation is complete (or failed).
		 */
		private static void BackupDlls(ICollection<string> locations)
		{
			if (locations.Count > 0)
			{
				EditorApplication.update += RestoreOriginalDlls;
				_dllsNeedRestoring = true;
			}

			foreach (string location in locations)
			{
				string backupLocation = location + ".pre";

				//This throws an exception if the backup already exists - we want this to happen
				File.Copy(location, backupLocation);

				DllsToRevert.Add(backupLocation, location);
			}
		}

		[PostProcessScene(1)]
		public static void Obfuscate()
		{
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			if (!EditorApplication.isPlayingOrWillChangePlaymode && !_obfuscatedAfterScene && _hasError == false)
#else
			if (!EditorApplication.isPlayingOrWillChangePlaymode && !_obfuscatedAfterScene && _hasError == false && BuildPipeline.isBuildingPlayer)
#endif
			{
				try
				{
					EditorApplication.LockReloadAssemblies();
					ObfuscateWhileLocked();
				}
				catch (Exception e)
				{
					Debug.LogError("Obfuscation Failed: " + e);
					_hasError = true;
				}
				finally
				{
					EditorApplication.UnlockReloadAssemblies();
				}
			}
		}

		private static void ObfuscateWhileLocked()
		{
			if (_options == null) _options = OptionsManager.LoadOptions();
			Obfuscator.FixHexBug(_options);

			if (_options.enabled == false) return;
			
			AssemblySelector selector = new AssemblySelector(_options);

			ICollection<string> compiledDlls = selector.GetCompiledAssemblyPaths();
			BackupDlls(compiledDlls);
			ICollection<string> dlls = selector.GetAssemblyPaths();

			if (dlls.Count == 0 && compiledDlls.Count == 0) _noCSharpScripts = true;
			else
			{
				HashSet<string> extraAssemblyReferenceDirectories = new HashSet<string>(_options.extraAssemblyDirectories);
				
#if UNITY_2017_3_OR_NEWER
				extraAssemblyReferenceDirectories.UnionWith(AssemblyReferenceLocator.GetAssemblyReferenceDirectories());
#endif
				
				Obfuscator.SetExtraAssemblyDirectories(extraAssemblyReferenceDirectories.ToArray());
				Obfuscator.Obfuscate(dlls, compiledDlls, _options, EditorUserBuildSettings.activeBuildTarget);

				if (_options.obfuscateMonoBehaviourClassNames)
				{
					/*
					 * RestoreAssets must be called via the update delegate because [PostProcessBuild] is not guaranteed to be called
					 */
					EditorApplication.update += RestoreAssets;
					_monoBehaviourAssetsNeedReverting = true;
				}
				_obfuscatedAfterScene = true;
			}
		}

		/**
		 * This method restores obfuscated MonoBehaviour cs files to their original names.
		 */
		private static void RestoreAssets()
		{
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#else
			if (BuildPipeline.isBuildingPlayer == false)
			{
#endif
				try
				{
					EditorApplication.LockReloadAssemblies();
					Obfuscator.RevertAssetObfuscation();
					_monoBehaviourAssetsNeedReverting = false;
					EditorApplication.update -= RestoreAssets;
				}
				finally
				{
					EditorApplication.UnlockReloadAssemblies();
				}
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#else
			}
#endif
			Obfuscator.Clear();
		}

		private static void DeleteObfuscatedDll(string target)
		{
			int attempts = 60;
			while (attempts > 0)
			{
				try
				{
					AttemptToDeleteObfuscatedDll(target);
					if (attempts < 60) Debug.LogWarning("Successfully accessed " + target);
					return;
				}
				catch (Exception)
				{
					Debug.LogWarning("Failed to access " + target + " - Retrying...");
					Thread.Sleep(500);
					if (--attempts <= 0) throw;
				}
			}
		}

		private static void AttemptToDeleteObfuscatedDll(string target)
		{
			if (File.Exists(target)) File.Delete(target);
		}
		/**
		 * This method restores original Dlls back into the project.
		 * DLLs declared within permanentDLLs will be restored from this method.
		 */
		private static void RestoreOriginalDlls()
		{
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#else
			if (BuildPipeline.isBuildingPlayer) return;
#endif
			foreach (string location in DllsToRevert.Keys)
			{
				try
				{
					if (!File.Exists(location)) continue;
						
					string target = DllsToRevert[location];

					DeleteObfuscatedDll(target);

					File.Move(location, DllsToRevert[location]);
				}
				catch (Exception e)
				{
					Debug.LogError("Could not restore original DLL to " + DllsToRevert[location] + "\n" + e);
				}
			}
			DllsToRevert.Clear();
			EditorApplication.update -= RestoreOriginalDlls;
			_dllsNeedRestoring = false;
		}
	}
}
