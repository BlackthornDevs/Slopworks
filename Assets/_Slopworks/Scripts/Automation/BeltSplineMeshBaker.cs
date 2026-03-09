using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

/// <summary>
/// Generates a belt mesh from spline data using Unity Splines' SplineExtrude,
/// then bakes the result and destroys the temporary components.
/// Call BakeMesh() once at belt placement time. No runtime overhead after baking.
/// </summary>
public static class BeltSplineMeshBaker
{
    private const float BeltWidth = 0.6f;
    private const float BeltThickness = 0.08f;
    private const int SegmentsPerMeter = 4;

    /// <summary>
    /// Generate a belt mesh from Hermite spline data and apply it to the target GameObject.
    /// Uses bake-and-destroy: creates temporary SplineContainer + SplineExtrude,
    /// generates mesh, copies to MeshFilter, destroys temp components.
    /// </summary>
    public static void BakeMesh(GameObject target, BeltSplineData splineData, Material material)
    {
        var bezier = splineData.GetBezierControlPoints();

        var meshFilter = target.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = target.AddComponent<MeshFilter>();

        var meshRenderer = target.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = target.AddComponent<MeshRenderer>();

        if (material != null)
            meshRenderer.sharedMaterial = material;

        var splineContainer = target.AddComponent<SplineContainer>();
        var spline = splineContainer.Spline;
        spline.Clear();

        var worldToLocal = target.transform.worldToLocalMatrix;

        var localP0 = worldToLocal.MultiplyPoint3x4(bezier.p0);
        var localP3 = worldToLocal.MultiplyPoint3x4(bezier.p3);
        var localTangentOut0 = worldToLocal.MultiplyPoint3x4(bezier.p1)
            - worldToLocal.MultiplyPoint3x4(bezier.p0);
        var localTangentIn1 = worldToLocal.MultiplyPoint3x4(bezier.p2)
            - worldToLocal.MultiplyPoint3x4(bezier.p3);

        spline.Add(new BezierKnot(
            (float3)localP0,
            float3.zero,
            (float3)localTangentOut0
        ));

        spline.Add(new BezierKnot(
            (float3)localP3,
            (float3)localTangentIn1,
            float3.zero
        ));

        var extrude = target.AddComponent<SplineExtrude>();
        extrude.RebuildOnSplineChange = false;
        extrude.Rebuild();

        var generatedMesh = meshFilter.sharedMesh;
        if (generatedMesh != null)
        {
            var bakedMesh = Object.Instantiate(generatedMesh);
            bakedMesh.name = "BakedBeltMesh";

            Object.DestroyImmediate(extrude);
            Object.DestroyImmediate(splineContainer);

            meshFilter.sharedMesh = bakedMesh;
        }
        else
        {
            Object.DestroyImmediate(extrude);
            Object.DestroyImmediate(splineContainer);
            Debug.LogWarning("belt: SplineExtrude failed to generate mesh");
        }

        target.isStatic = true;
    }
}
