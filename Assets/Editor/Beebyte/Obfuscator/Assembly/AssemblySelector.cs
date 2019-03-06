/*
 * Copyright (c) 2018 Beebyte Limited. All rights reserved.
 */
using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_2017_3_OR_NEWER
using UnityEditor.Compilation;
#endif
using UnityEngine;

namespace Beebyte.Obfuscator.Assembly
{
	public class AssemblySelector
	{
		private readonly HashSet<string> _compiledAssemblyPaths = new HashSet<string>();
		private readonly HashSet<string> _assemblyPaths = new HashSet<string>();
		
		public AssemblySelector(Options options)
		{
			if (options == null) throw new ArgumentException("options must not be null", "options");
			if (options.compiledAssemblies == null) throw new ArgumentException(
				"options.compiledAssemblies must not be null", "options");
			if (options.assemblies == null) throw new ArgumentException(
				"options.assemblies must not be null", "options");
			
			foreach (string assemblyName in options.compiledAssemblies)
			{
				_compiledAssemblyPaths.Add(FindDllLocation(assemblyName));
			}
			foreach (string assemblyName in options.assemblies)
			{
				_assemblyPaths.Add(FindDllLocation(assemblyName));
			}

#if UNITY_2017_3_OR_NEWER
			if (!options.includeCompilationPipelineAssemblies) return;
			
			foreach (UnityEditor.Compilation.Assembly assembly in CompilationPipeline.GetAssemblies())
			{
				if ((assembly.flags & AssemblyFlags.EditorAssembly) != 0 ||
					assembly.name.Contains("-firstpass") ||
					assembly.name.Contains("Unity.TextMeshPro"))
				{
					continue;
				}

				string dllLocation = FindDllLocation(assembly.name + ".dll");
				// If the assembly is for a different build target platform, oddly it will still be in the compilation
				// pipeline, however the file won't actually exist.
				if (File.Exists(dllLocation))
				{
					_assemblyPaths.Add(FindDllLocation(assembly.name + ".dll"));
				}
			}
#endif
		}

		public ICollection<string> GetCompiledAssemblyPaths()
		{
			return _compiledAssemblyPaths;
		}
		
		public ICollection<string> GetAssemblyPaths()
		{
			return _assemblyPaths;
		}

		private static string FindDllLocation(string suffix)
		{
			if (string.IsNullOrEmpty(suffix))
			{
				throw new ArgumentException(
					"Empty or null DLL names are forbidden (check Obfuscator Options assemblies / compiled assemblies list)");
			}
			
			foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					if (assembly.Location.Equals(string.Empty))
					{
						DisplayFailedAssemblyParseWarning(assembly);
					}
					else if (assembly.Location.EndsWith(suffix))
					{
						return assembly.Location;
					}
				}
				catch (NotSupportedException)
				{
					DisplayFailedAssemblyParseWarning(assembly);
				}
			}

			throw new ArgumentException(
				suffix + " was not found (check Obfuscator Options assemblies / compiled assemblies list)");
		}

		private static void DisplayFailedAssemblyParseWarning(System.Reflection.Assembly assembly)
		{
			Debug.LogWarning("Could not parse dynamically created assembly (string.Empty location) " +
							 assembly.FullName +
							 ". If you extend classes from within this assembly that in turn extend from " +
							 "MonoBehaviour you will need to manually annotate these classes with [Skip]");
		}
	}
}
