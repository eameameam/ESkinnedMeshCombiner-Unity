# SkinnedMeshCombinerUtility

`SkinnedMeshCombinerUtility` is a runtime utility for combining multiple skinned mesh renderers into a single skinned mesh renderer. This is useful for optimizing and simplifying the management of animations and meshes in your game.

## Usage

### Why You Need It

This utility allows you to combine multiple skinned mesh renderers into one at any time during runtime. This can help reduce the number of draw calls and optimize performance in scenes with a large number of animated objects.

### How to Use

To use the `SkinnedMeshCombinerUtility`, simply call the `CombineMeshes` method, passing the required data. Here is an example:

```csharp
using UnityEngine;
using Utils.Scripts;

public class SkinnedMeshCombinerExample : MonoBehaviour
{
    public SkinnedMeshRenderer[] skinnedMeshRenderers; // Array of skinned mesh renderers to combine
    public Material combinedMaterial; // Material for the combined mesh
    public Transform[] bones; // Array of bones
    public GameObject rootObject; // Root object for the combined skinned mesh renderer
    public Vector3[] boneInitPos; // Initial positions of bones
    public Quaternion[] boneInitRot; // Initial rotations of bones
    public Vector3[] boneInitScales; // Initial scales of bones

    void Start()
    {
        SkinnedMeshRenderer combinedRenderer = SkinnedMeshCombinerUtility.CombineMeshes(
            skinnedMeshRenderers,
            combinedMaterial,
            bones,
            rootObject,
            boneInitPos,
            boneInitRot,
            boneInitScales
        );

        if (combinedRenderer != null)
        {
            Debug.Log("Successfully combined skinned mesh renderers.");
        }
        else
        {
            Debug.LogError("Failed to combine skinned mesh renderers.");
        }
    }
}
```

### Method Signature

```csharp
public static SkinnedMeshRenderer CombineMeshes(
    SkinnedMeshRenderer[] skinnedMeshRenderers,
    Material material,
    Transform[] bones,
    GameObject rootObject,
    Vector3[] boneInitPos,
    Quaternion[] boneInitRot,
    Vector3[] boneInitScales
);
```

### Parameters

- `skinnedMeshRenderers`: Array of `SkinnedMeshRenderer` components to be combined.
- `material`: Material to be used for the combined mesh.
- `bones`: Array of bone transforms.
- `rootObject`: Root GameObject for the combined skinned mesh renderer.
- `boneInitPos`: Initial positions of the bones.
- `boneInitRot`: Initial rotations of the bones.
- `boneInitScales`: Initial scales of the bones.

### Example Scene Setup

1. Create a new GameObject and attach the `SkinnedMeshCombinerExample` script.
2. Assign the skinned mesh renderers, material, bones, and root object in the inspector.
3. Configure the initial positions, rotations, and scales of the bones.
4. Run the scene to see the combined skinned mesh renderer.

This utility helps streamline the process of combining skinned meshes at runtime, enhancing performance and simplifying mesh management.

---
