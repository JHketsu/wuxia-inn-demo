using UnityEngine;

public partial class InnDemo
{
    void AttachHeroModel()
    {
        const string resource = "Characters/YunGenggeng/YunGenggeng";
        var prefab = Resources.Load<GameObject>(resource);
        if (!prefab) return;

        // Keep the existing turning and sword pivot at torso height.
        var placeholder = body.GetComponent<Renderer>();
        body.localScale = Vector3.one;
        var model = Instantiate(prefab, body, false);
        model.name = "云耿耿";
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Destroy(model);
            body.localScale = new Vector3(.54f, HeroHeight/2, .54f);
            return;
        }

        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if (bounds.size.y < .001f)
        {
            Destroy(model);
            body.localScale = new Vector3(.54f, HeroHeight/2, .54f);
            return;
        }

        float scale = HeroHeight / bounds.size.y;
        Vector3 origin = model.transform.position;
        Vector3 feet = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        model.transform.localScale *= scale;
        model.transform.position = player.transform.position - (feet-origin)*scale;
        var material = Resources.Load<Material>("Characters/YunGenggeng/HeroMaterial");
        if (material)
        {
            foreach (var renderer in renderers)
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
            }
        }
        placeholder.enabled = false;
        heroRenderers = renderers;
        heroAnimator = SetupActorAnimator(model);
    }
}
