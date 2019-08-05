using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NpmPackageLoader.Loaders
{
    public abstract class UnityPackageLoader : Loader
    {
        [SerializeField] private string[] packedObjects = new string[0];

        public string[] PackedObjects => packedObjects;

#if UNITY_EDITOR
        protected void Export(TextAsset packageJsonAsset, string exportPath, Action success, Action fail)
        {
            Try(fail, () =>
            {
                EditorUtility.DisplayProgressBar("Export Loader", name, 1f);

                var packedAssetsPaths = PackedObjects
                    .Where(path => AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                    .ToArray();

                var packageJson = JsonUtility.FromJson<PackageJson>(packageJsonAsset.text);

                var embedPackageJsonPath = $"{Application.dataPath}/Packages/{packageJson.name}/{name}.json";
                var embedPackageJsonDirectory = Path.GetDirectoryName(embedPackageJsonPath);
                if (embedPackageJsonDirectory == null) return;

                if (!Directory.Exists(embedPackageJsonDirectory))
                {
                    Directory.CreateDirectory(embedPackageJsonDirectory);
                }

                File.WriteAllText(embedPackageJsonPath, JsonUtility.ToJson(packageJson));

                AssetDatabase.Refresh();

                packedAssetsPaths = packedAssetsPaths
                    .Append($"Assets/Packages/{packageJson.name}/{name}.json")
                    .ToArray();

                EditorUtility.ClearProgressBar();

                AssetDatabase.ExportPackage(packedAssetsPaths, exportPath, ExportPackageOptions.Recurse);

                success();
            });
        }
#endif
    }
}