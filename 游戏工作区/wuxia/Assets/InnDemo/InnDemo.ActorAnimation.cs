using UnityEngine;

public partial class InnDemo
{
    static readonly int ActorSpeed = Animator.StringToHash("Speed");
    Animator heroAnimator, companionAnimator;
    Transform companionVisual;
    SkinnedMeshRenderer companionSkin;
    Vector3 companionPoseOffset;
    bool companionPoseAwake;

    Animator SetupActorAnimator(GameObject model)
    {
        var animator = model.GetComponent<Animator>();
        var controller = Resources.Load<RuntimeAnimatorController>("Characters/Locomotion/InnLocomotion");
        if (!animator || !animator.avatar || !animator.avatar.isValid || !controller) return null;
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        return animator;
    }

    void AttachCompanionModel()
    {
        const string path = "Characters/XiaoLuoyin/";
        var prefab = Resources.Load<GameObject>(path + "XiaoLuoyin");
        if (!prefab) return;
        var model = Instantiate(prefab, transform);
        model.name = "萧罗音";
        companionSkin = model.GetComponentInChildren<SkinnedMeshRenderer>();
        companionAnimator = SetupActorAnimator(model);
        if (!companionSkin || !companionAnimator || companionSkin.bounds.size.y < .001f)
        {
            Destroy(model);
            return;
        }

        companionVisual = model.transform;
        companionVisual.localScale *= CompanionHeight / companionSkin.bounds.size.y;
        var material = Resources.Load<Material>(path + "CompanionMaterial");
        if (material) companionSkin.sharedMaterial = material;
        companion.GetComponent<Renderer>().enabled = false;
        SetCompanionPose();
    }

    void SetCompanionPose()
    {
        companionPoseAwake = story.awake;
        companionVisual.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        companionAnimator.SetFloat(ActorSpeed, 0);
        companionAnimator.Play(companionPoseAwake ? "Locomotion" : "Laying", 0, 0);
        companionAnimator.Update(0);

        // Ground the sampled pose using its actual deformed mesh.
        var mesh = new Mesh();
        companionSkin.BakeMesh(mesh);
        var vertices = mesh.vertices;
        if (vertices.Length > 0)
        {
            var bounds = new Bounds(companionSkin.transform.TransformPoint(vertices[0]), Vector3.zero);
            foreach (var vertex in vertices) bounds.Encapsulate(companionSkin.transform.TransformPoint(vertex));
            companionPoseOffset = -new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }
        Destroy(mesh);
    }

    void UpdateActorModels()
    {
        if (!player || !companion) return;
        bool canMove = dialogTitle == null && !journal && !hudMenuOpen && !won;
        if (heroAnimator) heroAnimator.SetFloat(ActorSpeed, canMove ? HorizontalSpeed(player) : 0, .12f, Time.deltaTime);
        if (!companionVisual) return;
        if (companionPoseAwake != story.awake) SetCompanionPose();
        if (companionPoseAwake)
            companionAnimator.SetFloat(ActorSpeed, canMove && follower ? HorizontalSpeed(follower) : 0, .12f, Time.deltaTime);
        var rotation = companionPoseAwake ? companion.rotation : Quaternion.identity;
        float centerHeight = companionPoseAwake ? CompanionCenterY : .29f;
        companionVisual.SetPositionAndRotation(
            companion.position - Vector3.up * centerHeight + rotation * companionPoseOffset, rotation);
    }

    static float HorizontalSpeed(CharacterController controller)
    {
        var velocity = controller.velocity;
        return new Vector2(velocity.x, velocity.z).magnitude;
    }
}
