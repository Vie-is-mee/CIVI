using UnityEditor;

namespace Civi.EditorTools
{
    // Everything under Models comes from tools/blender; materials are assigned by CiviSceneBuilder, so nothing is imported from FBX but geometry.
    sealed class CiviModelImport : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.Contains("/_Civi/Models/")) return;
            var m = (ModelImporter)assetImporter;
            bool character = assetPath.Contains("/Characters/");
            m.materialImportMode = ModelImporterMaterialImportMode.None;
            m.importAnimation = false;
            m.animationType = character ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
            m.importCameras = false;
            m.importLights = false;
            m.importBlendShapes = false;
            m.importNormals = ModelImporterNormals.Import;
            m.importTangents = ModelImporterTangents.None;
            m.meshCompression = ModelImporterMeshCompression.Off;
            m.addCollider = false;
            m.useFileScale = true;
            m.globalScale = 1;
            m.bakeAxisConversion = !character;
            m.isReadable = false;
        }
    }

    // Audio is authored at 48 kHz WAV; Vorbis keeps the build small and long ambience streams instead of sitting in RAM.
    sealed class CiviAudioImport : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (!assetPath.Contains("/Resources/Audio/")) return;
            var a = (AudioImporter)assetImporter;
            bool ambience = assetPath.Contains("/Amb/");
            var s = a.defaultSampleSettings;
            s.compressionFormat = UnityEngine.AudioCompressionFormat.Vorbis;
            s.quality = ambience ? .45f : .6f;
            s.loadType = ambience ? UnityEngine.AudioClipLoadType.Streaming : UnityEngine.AudioClipLoadType.DecompressOnLoad;
            s.preloadAudioData = !ambience;
            a.defaultSampleSettings = s;
            a.forceToMono = false;
        }
    }
}
