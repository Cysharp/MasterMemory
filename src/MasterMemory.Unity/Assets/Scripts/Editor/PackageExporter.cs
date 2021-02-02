﻿using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PackageExporter
{
    [MenuItem("Tools/Export Unitypackage")]
    public static void Export()
    {
        var version = Environment.GetEnvironmentVariable("UNITY_PACKAGE_VERSION");

        // configure
        var root = "Scripts/MasterMemory";
        var fileName = string.IsNullOrEmpty(version) ? "MasterMemory.Unity.unitypackage" : $"MasterMemory.Unity.{version}.unitypackage";
        var exportPath = "./" + fileName;

        var path = Path.Combine(Application.dataPath, root);
        var assets = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Where(x => Path.GetExtension(x) == ".cs" || Path.GetExtension(x) == ".asmdef" || Path.GetExtension(x) == ".json")
            .Where(x => Path.GetFileNameWithoutExtension(x) != "_InternalVisibleTo")
            .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace(@"\", "/"))
            .ToArray();

        UnityEngine.Debug.Log("Export below files" + Environment.NewLine + string.Join(Environment.NewLine, assets));

        AssetDatabase.ExportPackage(
            assets,
            exportPath,
            ExportPackageOptions.Default);

        UnityEngine.Debug.Log("Export complete: " + Path.GetFullPath(exportPath));
    }
}