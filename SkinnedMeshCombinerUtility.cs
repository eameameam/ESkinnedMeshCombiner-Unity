using System.Collections.Generic;
using UnityEngine;

public static class SkinnedMeshCombinerUtility
{
    public static SkinnedMeshRenderer CombineMeshes(SkinnedMeshRenderer[] skinnedMeshRenderers, Material material, Transform[] bones, GameObject rootObject, Vector3[] boneInitPos, Quaternion[] boneInitRot, Vector3[] boneInitScales)
    {
        if (skinnedMeshRenderers.Length == 0)
        {
            Debug.LogError("No skinned mesh renderers provided.");
            return null;
        }

        InitializeBones(bones, boneInitPos, boneInitRot, boneInitScales);
        
        var boneNameToIndex = BuildBoneNameToIndexMap(skinnedMeshRenderers, out int totalVertexCount);

        var (vertices, normals, uvs, colors, tangents, boneWeights) = InitializeMeshArrays(totalVertexCount);
        
        var (bindPoses, bonesArray) = CreateBonesAndBindPoses(bones, rootObject, boneNameToIndex);

        var subMeshIndices = new List<int>();
        PopulateMeshData(skinnedMeshRenderers, boneNameToIndex, vertices, normals, uvs, colors, tangents, boneWeights, subMeshIndices);

        Mesh combinedMesh = CreateCombinedMesh(vertices, normals, uvs, colors, tangents, boneWeights, bindPoses, subMeshIndices);
        
        return CreateCombinedSkinnedMeshRenderer(rootObject, material, bonesArray, combinedMesh, skinnedMeshRenderers);
    }

    private static void InitializeBones(Transform[] bones, Vector3[] positions, Quaternion[] rotations, Vector3[] scales)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].localPosition = positions[i];
            bones[i].localRotation = rotations[i];
            bones[i].localScale = scales[i];
        }
    }

    private static Dictionary<string, int> BuildBoneNameToIndexMap(SkinnedMeshRenderer[] skinnedMeshRenderers, out int totalVertexCount)
    {
        var boneNameToIndex = new Dictionary<string, int>();
        totalVertexCount = 0;

        foreach (var smr in skinnedMeshRenderers)
        {
            totalVertexCount += smr.sharedMesh.vertexCount;
            foreach (var bone in smr.bones)
            {
                if (!boneNameToIndex.ContainsKey(bone.name))
                {
                    boneNameToIndex[bone.name] = boneNameToIndex.Count;
                }
            }
        }

        return boneNameToIndex;
    }

    private static (Vector3[], Vector3[], Vector2[], Color[], Vector4[], BoneWeight[]) InitializeMeshArrays(int totalVertexCount)
    {
        return (
            new Vector3[totalVertexCount],
            new Vector3[totalVertexCount],
            new Vector2[totalVertexCount],
            new Color[totalVertexCount],
            new Vector4[totalVertexCount],
            new BoneWeight[totalVertexCount]
        );
    }

    private static (Matrix4x4[], Transform[]) CreateBonesAndBindPoses(Transform[] bones, GameObject rootObject, Dictionary<string, int> boneNameToIndex)
    {
        var bindPoses = new Matrix4x4[boneNameToIndex.Count];
        var bonesArray = new Transform[boneNameToIndex.Count];

        foreach (var bone in bones)
        {
            if (boneNameToIndex.TryGetValue(bone.name, out var index))
            {
                bonesArray[index] = bone;
                bindPoses[index] = bone.worldToLocalMatrix * rootObject.transform.localToWorldMatrix;
            }
        }

        return (bindPoses, bonesArray);
    }

    private static void PopulateMeshData(SkinnedMeshRenderer[] skinnedMeshRenderers, Dictionary<string, int> boneNameToIndex, Vector3[] vertices, Vector3[] normals, Vector2[] uvs, Color[] colors, Vector4[] tangents, BoneWeight[] boneWeights, List<int> subMeshIndices)
    {
        int currentVertex = 0;
        int currentBoneWeight = 0;

        foreach (var smr in skinnedMeshRenderers)
        {
            var mesh = smr.sharedMesh;
            if (mesh == null) continue;

            int vertexOffset = currentVertex;

            System.Array.Copy(mesh.vertices, 0, vertices, currentVertex, mesh.vertexCount);
            System.Array.Copy(mesh.normals, 0, normals, currentVertex, mesh.vertexCount);
            System.Array.Copy(mesh.uv, 0, uvs, currentVertex, mesh.vertexCount);
            System.Array.Copy(mesh.colors.Length > 0 ? mesh.colors : new Color[mesh.vertexCount], 0, colors, currentVertex, mesh.vertexCount);
            System.Array.Copy(mesh.tangents.Length > 0 ? mesh.tangents : new Vector4[mesh.vertexCount], 0, tangents, currentVertex, mesh.vertexCount);

            foreach (var bw in mesh.boneWeights)
            {
                boneWeights[currentBoneWeight++] = new BoneWeight
                {
                    boneIndex0 = boneNameToIndex[smr.bones[bw.boneIndex0].name],
                    boneIndex1 = boneNameToIndex[smr.bones[bw.boneIndex1].name],
                    boneIndex2 = boneNameToIndex[smr.bones[bw.boneIndex2].name],
                    boneIndex3 = boneNameToIndex[smr.bones[bw.boneIndex3].name],
                    weight0 = bw.weight0,
                    weight1 = bw.weight1,
                    weight2 = bw.weight2,
                    weight3 = bw.weight3
                };
            }

            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                var indices = mesh.GetTriangles(subMeshIndex);
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] += vertexOffset;
                }
                subMeshIndices.AddRange(indices);
            }

            currentVertex += mesh.vertexCount;
        }
    }

    private static Mesh CreateCombinedMesh(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, Color[] colors, Vector4[] tangents, BoneWeight[] boneWeights, Matrix4x4[] bindPoses, List<int> subMeshIndices)
    {
        var combinedMesh = new Mesh
        {
            vertices = vertices,
            normals = normals,
            uv = uvs,
            colors = colors,
            tangents = tangents,
            boneWeights = boneWeights,
            bindposes = bindPoses,
            subMeshCount = 1
        };
        combinedMesh.SetTriangles(subMeshIndices.ToArray(), 0);

        return combinedMesh;
    }

    private static SkinnedMeshRenderer CreateCombinedSkinnedMeshRenderer(GameObject rootObject, Material material, Transform[] bonesArray, Mesh combinedMesh, SkinnedMeshRenderer[] skinnedMeshRenderers)
    {
        var combinedRenderer = rootObject.AddComponent<SkinnedMeshRenderer>();
        combinedRenderer.sharedMesh = combinedMesh;
        combinedRenderer.bones = bonesArray;
        combinedRenderer.sharedMaterial = material;

        foreach (var smr in skinnedMeshRenderers)
        {
            smr.enabled = false;
        }

        return combinedRenderer;
    }
}