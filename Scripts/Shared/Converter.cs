using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GLTFast;
using GLTFast.Export;

namespace Secur3D.SDK
{
    static class Converter
    {
        public static async Task<bool> ConvertGameObjectToGlbStream(GameObject gameObject, MemoryStream stream)
        {
            ExportSettings exportSettings = new ExportSettings
            {
                Format = GltfFormat.Binary,
                FileConflictResolution = FileConflictResolution.Overwrite,
                ComponentMask = ~(ComponentType.Camera | ComponentType.Animation),
                LightIntensityFactor = 1f,

            };

            GameObjectExportSettings gameObjectExportSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = false,
                DisabledComponents = false,
                LayerMask = LayerMask.GetMask("Default"),
            };
            
            GameObjectExport exporter = new GameObjectExport(exportSettings, gameObjectExportSettings, MaterialExport.GetDefaultMaterialExport());

            exporter.AddScene(new GameObject[] { gameObject });

            return await exporter.SaveToStreamAndDispose(stream);                
        }

        public static async Task<bool> ConvertGameObjectToGlbFile(GameObject gameObject, string path)
        {
            var exportSettings = new ExportSettings
            {
                Format = GltfFormat.Binary,
                FileConflictResolution = FileConflictResolution.Overwrite,
                ComponentMask = ~(ComponentType.Camera | ComponentType.Animation),
                LightIntensityFactor = 1f,
            };
            var gameObjectExportSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = false,
                DisabledComponents = false,
                LayerMask = LayerMask.GetMask("Default"),
            };

            GameObjectExport exporter = new GameObjectExport(exportSettings, gameObjectExportSettings);

            GameObject[] scene = { gameObject };
            exporter.AddScene(scene);
            
            return await exporter.SaveToFileAndDispose(path);

        }
    }
}