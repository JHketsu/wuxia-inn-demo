using UnityEngine;

public partial class InnDemo
{
    // Visual-only shell. Original walls remain collision/camera proxies.
    void AttachExteriorPreview()
    {
        var prefab = Resources.Load<GameObject>("InnDemo/InnExterior_v001");
        if (!prefab) return;
        var shell = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        shell.name = "客栈外皮预览_v001";
        foreach (var name in new[] { "Back wall", "West wall", "East wall", "Front left", "Front right", "Lintel" })
        {
            var wall = GameObject.Find(name);
            if (wall && wall.TryGetComponent<Renderer>(out var renderer)) renderer.enabled = false;
        }
        // Match the established east-southeast morning direction.
        var sunObject = GameObject.Find("Warm daylight");
        if (sunObject)
        {
            sunObject.transform.rotation = Quaternion.LookRotation(new Vector3(-.883f, -.342f, .321f));
            var sun = sunObject.GetComponent<Light>();
            sun.color = new Color(1f, .91f, .78f);
        }
    }
}
