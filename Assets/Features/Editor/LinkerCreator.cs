﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MagicSwords.Features.Editor
{
    internal sealed class LinkerGenerator : IPreprocessBuildWithReport
    {
        private const string SourcesFolder = "/";// + nameof(DI) + "/";

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            Generate();
        }

        [MenuItem("Tools/Generate link.xml")]
        public static void Generate()
        {
            var assetsDir = Application.dataPath;

            var linkXmlFilePath = Path.Combine(assetsDir, Application.dataPath + "/Settings/", "link.xml");

            Directory.CreateDirectory
            (
                Path.GetDirectoryName(linkXmlFilePath)
                ?? throw new InvalidOperationException($"No directory in file name {linkXmlFilePath}")
            );

            var assembliesToPreserve = Enumerable.Empty<string>()
                .Concat(GetDllAssemblyNames(assetsDir + SourcesFolder))
                .Distinct()
                .OrderBy(s => s);

            var content = Enumerable.Empty<string>()
                .Concat("<linker>")
                .Concat(string.Empty)
                .Concat(assembliesToPreserve.Select(assemblyName =>
                    $"    <assembly fullname=\"{assemblyName}\" preserve=\"all\" />"))
                .Concat(string.Empty)
                .Concat("</linker>")
                .Aggregate(new StringBuilder(), (builder, line) => builder.AppendLine(line));

            using var fileStream = File.Open(linkXmlFilePath, FileMode.Create);
            using var streamWriter = new StreamWriter(fileStream);

            streamWriter.Write(content);
        }

        private static IEnumerable<string> GetDllAssemblyNames(string assetsDir)
        {
            return Directory.EnumerateFiles(assetsDir, "*.asmdef", SearchOption.AllDirectories)
                .Distinct()
                .Select(Path.GetFileNameWithoutExtension);
        }
    }

    internal static class LinkerGeneratorExtensions
    {
        public static IEnumerable<TItem> Concat<TItem>(this IEnumerable<TItem> enumerable, TItem item)
        {
            // enumerable.Append()
            
            return enumerable.Concat(new[] { item });

            // return enumerable.Append(item);
        }
    }
}