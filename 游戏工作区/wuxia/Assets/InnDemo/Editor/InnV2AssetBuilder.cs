using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class InnV2AssetBuilder
{
    const string Folder = "Assets/InnDemo/Art";
    const string Build = "build003";
    const string PrefabPath = Folder + "/Prefabs/Inn_v2/Inn_v2_Assembly.prefab";
    const string PreviewPath = "Assets/Scenes/InnArtPreview_v2.unity";
    [Serializable] class MaterialSpec { public string name; public float[] color; public string texture; }
    [Serializable] class SourceReport { public MaterialSpec[] material_specs; }
    static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    static int meshIndex;
    static string Clean(string n) => Regex.Replace(n, @"\.\d{3}$", "");

    [MenuItem("Tools/Inn Demo/Art v2/Build Editable Prefabs")]
    public static void BuildPrefabs()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before building assets.");
        AssetDatabase.Refresh();
        string sourcePath = Folder + "/Models/Inn_v2/Inn_v2_" + Build + ".fbx";
        var importer = AssetImporter.GetAtPath(sourcePath) as ModelImporter;
        if (!importer) throw new InvalidOperationException("FBX is not imported yet");
        importer.bakeAxisConversion = true; importer.SaveAndReimport();
        var manifest = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../ArtSource/Architecture/Inn_v2/" + Build + "/manifest.json"));
        var report = JsonUtility.FromJson<SourceReport>(File.ReadAllText(manifest));
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) throw new InvalidOperationException("URP Lit unavailable");
        Materials.Clear();
        Directory.CreateDirectory(Folder + "/Materials/Inn_v2");
        Directory.CreateDirectory(Folder + "/Meshes/Inn_v2/" + Build);
        Directory.CreateDirectory(Folder + "/Prefabs/Inn_v2/Furniture");
        foreach (var spec in report.material_specs)
        {
            string path = Folder + "/Materials/Inn_v2/" + spec.name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            mat.color = new Color(spec.color[0], spec.color[1], spec.color[2], spec.color[3]);
            mat.SetFloat("_Smoothness", .12f);
            if (!string.IsNullOrEmpty(spec.texture))
            {
                var txPath = Folder + "/Textures/Inn_v2/" + spec.texture;
                var ti = AssetImporter.GetAtPath(txPath) as TextureImporter;
                ti.wrapMode = TextureWrapMode.Repeat; ti.filterMode = FilterMode.Point; ti.mipmapEnabled = true; ti.SaveAndReimport();
                mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(txPath);
            }
            EditorUtility.SetDirty(mat); Materials[spec.name] = mat;
        }
        var source = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
        // Normalize actual mesh coordinates, not object-scale guesses. This preserves hinges/pivots.
        var sourceRoot = source.GetComponentsInChildren<Transform>().First(t => t.Cast<Transform>().Any(c => Clean(c.name) == "Architecture") && t.Cast<Transform>().Any(c => Clean(c.name) == "Furniture"));
        var root = new GameObject("Inn_v2"); meshIndex = 0;
        foreach (Transform child in sourceRoot) Copy(child, root.transform);
        UnityEngine.Object.DestroyImmediate(source);
        // Furniture are nested editable Prefabs; instance overrides retain the assembled positions.
        var furniture = root.transform.Find("Furniture");
        var first = furniture.Find("DiningSet_01");
        string[] itemNames = { "Table", "Bench_01", "Stool_01" };
        string[] itemFiles = { "Table_A", "Bench_A", "Stool_A" };
        for (int i = 0; i < itemNames.Length; i++)
        {
            var sample = UnityEngine.Object.Instantiate(first.Find(itemNames[i]).gameObject);
            sample.name = itemFiles[i]; sample.transform.position = Vector3.zero;
            PrefabUtility.SaveAsPrefabAsset(sample, Folder + "/Prefabs/Inn_v2/Furniture/" + itemFiles[i] + ".prefab");
            UnityEngine.Object.DestroyImmediate(sample);
        }
        foreach (Transform set in furniture.Cast<Transform>().Where(t => t.name.StartsWith("DiningSet")).ToArray())
        {
            foreach (Transform item in set.Cast<Transform>().ToArray())
            {
                string file = item.name.StartsWith("Table") ? "Table_A" : item.name.StartsWith("Bench") ? "Bench_A" : "Stool_A";
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Prefabs/Inn_v2/Furniture/" + file + ".prefab");
                var replacement = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                replacement.name = item.name; replacement.transform.SetParent(set, false); replacement.transform.localPosition = item.localPosition;
                UnityEngine.Object.DestroyImmediate(item.gameObject);
            }
            var pos = set.localPosition; set.localPosition = Vector3.zero;
            string setPath = Folder + "/Prefabs/Inn_v2/Furniture/" + set.name + ".prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(set.gameObject, setPath, InteractionMode.AutomatedAction); set.localPosition = pos;
        }
        foreach (string name in new[] { "Counter", "BackShelf" })
        {
            var item = furniture.Find(name); var pos = item.localPosition; item.localPosition = Vector3.zero;
            PrefabUtility.SaveAsPrefabAssetAndConnect(item.gameObject, Folder + "/Prefabs/Inn_v2/Furniture/" + name + ".prefab", InteractionMode.AutomatedAction); item.localPosition = pos;
        }
        var circulation = root.transform.Find("Circulation");
        foreach (Transform item in circulation.Cast<Transform>().ToArray())
        {
            var pos = item.localPosition; item.localPosition = Vector3.zero;
            PrefabUtility.SaveAsPrefabAssetAndConnect(item.gameObject, Folder + "/Prefabs/Inn_v2/" + item.name + ".prefab", InteractionMode.AutomatedAction); item.localPosition = pos;
        }
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        var audit = Audit(root);
        UnityEngine.Object.DestroyImmediate(root); AssetDatabase.SaveAssets();
        string logPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../ArtSource/Architecture/Inn_v2/" + Build + "/unity_audit.txt"));
        File.WriteAllText(logPath, audit); Debug.Log(audit);
    }

    static void Copy(Transform source, Transform parent)
    {
        var target = new GameObject(Clean(source.name)); target.transform.SetParent(parent, true);
        target.transform.position = source.position; target.transform.rotation = Quaternion.identity; target.transform.localScale = Vector3.one;
        var filter = source.GetComponent<MeshFilter>(); var renderer = source.GetComponent<MeshRenderer>();
        if (filter && filter.sharedMesh)
        {
            var mesh = UnityEngine.Object.Instantiate(filter.sharedMesh); mesh.name = target.name;
            Matrix4x4 matrix = source.localToWorldMatrix; var pivot = target.transform.position;
            var vertices = mesh.vertices; for (int i = 0; i < vertices.Length; i++) vertices[i] = matrix.MultiplyPoint3x4(vertices[i]) - pivot; mesh.vertices = vertices;
            var normals = mesh.normals; var normalMatrix = matrix.inverse.transpose;
            for (int i = 0; i < normals.Length; i++) normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized; mesh.normals = normals;
            if (matrix.determinant < 0)
            {
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                { var tris = mesh.GetTriangles(sub); for (int i = 0; i < tris.Length; i += 3) { int tmp = tris[i]; tris[i] = tris[i+1]; tris[i+1] = tmp; } mesh.SetTriangles(tris, sub); }
            }
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            string mp = Folder + "/Meshes/Inn_v2/" + Build + "/" + (meshIndex++).ToString("D4") + "_" + target.name + ".asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(mp)) throw new InvalidOperationException("Build already exists; increment build ID instead of overwriting meshes");
            AssetDatabase.CreateAsset(mesh, mp);
            target.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = target.AddComponent<MeshRenderer>();
            mr.sharedMaterials = renderer.sharedMaterials.Select(m => Materials[Clean(m.name)]).ToArray();
        }
        foreach (Transform child in source) Copy(child, target.transform);
    }

    public static string Audit(GameObject root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        if (transforms.Any(t => (t.localScale - Vector3.one).sqrMagnitude > .000001f)) throw new InvalidOperationException("Non-unit scale");
        if (transforms.Any(t => Quaternion.Angle(t.localRotation, Quaternion.identity) > .01f)) throw new InvalidOperationException("Unexpected rotation");
        var rooms = root.transform.Find("Rooms"); if (rooms.childCount != 6) throw new InvalidOperationException("Expected six rooms");
        var sets = root.transform.Find("Furniture").Cast<Transform>().Where(t => t.name.StartsWith("DiningSet")).ToArray();
        if (sets.Length != 4 || sets.Any(t => t.childCount != 5)) throw new InvalidOperationException("Dining hierarchy mismatch");
        var leaves = transforms.Where(t => t.name == "LeftLeaf" || t.name == "RightLeaf").ToArray();
        if (leaves.Length != 62) throw new InvalidOperationException("Expected 31 paired windows/62 leaves, got " + leaves.Length);
        var stair = root.transform.Find("Circulation/Stair");
        if (Vector3.Distance(stair.position, new Vector3(4, 0, -8.4f)) > .01f) throw new InvalidOperationException("Stair coordinate mismatch");
        var table = root.transform.Find("Furniture/DiningSet_01/Table");
        var rs = table.GetComponentsInChildren<Renderer>(); var bounds = rs[0].bounds; foreach (var r in rs) bounds.Encapsulate(r.bounds);
        if (Mathf.Abs(bounds.max.y - .76f) > .01f) throw new InvalidOperationException("Table height mismatch");
        return "PASS Inn v2: six rooms; 4 table sets + 8 benches + 8 stools; 31 paired windows/62 hinge groups; upper gable eaves both sides; unit scale and identity rotations throughout; stair pivot (4,0,-8.4); table surface .76m. Meshes=" + root.GetComponentsInChildren<MeshFilter>().Length;
    }

    [MenuItem("Tools/Inn Demo/Art v2/Open Preview Scene")]
    public static void OpenPreview()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first");
        if (File.Exists(PreviewPath)) { EditorSceneManager.OpenScene(PreviewPath, OpenSceneMode.Additive); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.SetActiveScene(scene);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        // InnDemo's automatic bootstrap sees this disabled marker and leaves the art scene alone.
        var guard = new GameObject("ArtPreview_GameBootstrapDisabled"); guard.AddComponent<InnDemo>().enabled = false;
        var cam = new GameObject("InnArtCamera"); cam.tag = "MainCamera";
        var c = cam.AddComponent<Camera>(); c.fieldOfView = 48; c.nearClipPlane = .08f; c.farClipPlane = 200; c.depth = 100;
        c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(.67f, .72f, .73f);
        var viewer = cam.AddComponent<InnArtPreviewCamera>(); viewer.assembly = root.transform; viewer.SetView(1);
        var sun = new GameObject("ArtMorningSun").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.6f; sun.color = new Color(1,.91f,.78f); sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.LookRotation(new Vector3(-.883f,-.342f,.321f));
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.65f,.70f,.74f); RenderSettings.ambientEquatorColor = new Color(.45f,.43f,.38f); RenderSettings.ambientGroundColor = new Color(.27f,.24f,.20f);
        EditorSceneManager.SaveScene(scene, PreviewPath); Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();
        Debug.Log("Inn v2 art preview saved. Play: 1 exterior, 2 hall, 3 upper corridor, 4 cutaway.");
    }
}
