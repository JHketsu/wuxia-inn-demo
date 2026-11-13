using UnityEngine;
using UnityEngine.UI;

// Project a bent sheet onto the Canvas, one strip per segment.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ScaleLeaf : MaskableGraphic
{
    public Texture2D PaperTexture { get; set; }
    public bool BackFace { get; set; }
    public override Texture mainTexture => PaperTexture ? PaperTexture : base.mainTexture;

    public float Progress { get; private set; }
    public int Direction { get; private set; } = 1;

    public void SetTurn(float progress, int direction)
    {
        Progress = Mathf.Clamp01(progress);
        Direction = direction;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        const int strips = 64;
        Rect paper = rectTransform.rect;
        // Roll the free edge first, then sweep the rolled sheet across the hinge.
        float sweep = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.22f, .94f, Progress));
        float angle = sweep * Mathf.PI;
        float curlWidth = Mathf.Lerp(.18f, .66f, Mathf.Sin(sweep * Mathf.PI * .5f));
        float curl = 4.5f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .24f, Progress))
            * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.68f, 1, Progress)));
        float x = 0, depth = 0;
        for (int i = 0; i <= strips; i++)
        {
            float u = (float)i / strips;
            float curledEdge = Mathf.Clamp01((u - (1 - curlWidth)) / curlWidth);
            float tangent = angle + curl * curledEdge;
            if (i > 0)
            {
                x += Mathf.Cos(tangent) * paper.width / strips;
                depth += Mathf.Sin(tangent) * paper.width / strips;
            }
            float perspective = 1 + depth / 1600f;
            float facing = Mathf.Cos(tangent);
            float shade = facing >= 0 ? .74f + .26f * facing : .52f - .19f * facing;
            Color tint = new(color.r * shade, color.g * shade, color.b * shade, color.a);
            float hinge = Direction > 0 ? paper.xMin : paper.xMax;
            float projectedX = hinge + Direction * x * perspective;
            mesh.AddVert(new Vector3(projectedX, paper.center.y - paper.height * perspective / 2), tint, new Vector2(BackFace ? 1-u : u,0));
            mesh.AddVert(new Vector3(projectedX, paper.center.y + paper.height * perspective / 2), tint, new Vector2(BackFace ? 1-u : u,1));
            if (i == 0 || (facing < 0) != BackFace) continue;
            int edge = i * 2;
            mesh.AddTriangle(edge-2, edge-1, edge);
            mesh.AddTriangle(edge, edge-1, edge+1);
        }
    }
}
