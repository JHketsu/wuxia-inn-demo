using UnityEngine;

public partial class InnDemo
{
    void AttachCounterModels()
    {
        AttachCounterModel(boss, "Characters/Innkeeper/Innkeeper", BossHeight, false);
        AttachCounterModel(sword, "Props/InnkeeperSword/InnkeeperSword", .95f, true);
    }

    void AttachCounterModel(Transform anchor, string path, float size, bool layFlat)
    {
        var prefab = Resources.Load<GameObject>(path);
        if (!prefab) return;

        var model = Instantiate(prefab, anchor.position, Quaternion.identity);
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Destroy(model);
            return;
        }

        var bounds = ModelBounds(renderers);
        if (layFlat)
        {
            // Align the longest mesh axis with the existing sword pivot.
            Vector3 dimensions = bounds.size;
            Vector3 length = dimensions.x >= dimensions.y && dimensions.x >= dimensions.z
                ? Vector3.right : dimensions.y >= dimensions.z ? Vector3.up : Vector3.forward;
            Vector3 thickness = dimensions.x <= dimensions.y && dimensions.x <= dimensions.z
                ? Vector3.right : dimensions.y <= dimensions.z ? Vector3.up : Vector3.forward;
            if (length != thickness)
                model.transform.rotation = Quaternion.Inverse(Quaternion.LookRotation(length, thickness));
            bounds = ModelBounds(renderers);
        }
        else
        {
            model.transform.rotation = Quaternion.Euler(0, 180, 0);
            bounds = ModelBounds(renderers);
        }

        float extent = layFlat ? bounds.size.z : bounds.size.y;
        if (extent < .001f)
        {
            Destroy(model);
            return;
        }

        float scale = size / extent;
        var source = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        var target = anchor.position + Vector3.down * (layFlat ? .04f : BossHeight / 2);
        model.transform.localScale *= scale;
        model.transform.position = target + (model.transform.position - source) * scale;

        var material = Resources.Load<Material>(path + "Material");
        if (material)
        {
            foreach (var renderer in renderers)
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
            }
        }

        // Keep the placeholder's collider and story pivot at their original scale.
        model.transform.SetParent(anchor, true);
        anchor.GetComponent<Renderer>().enabled = false;
    }

    static Bounds ModelBounds(Renderer[] renderers)
    {
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
}
