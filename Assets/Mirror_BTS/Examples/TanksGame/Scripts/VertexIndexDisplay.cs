using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class VertexIndexDisplay : MonoBehaviour
{
    public Vector3[] _vertices;
    private BoneWeight[] _boneWeights; // Store bone weights to check for unassigned vertices
    public bool useBakedMesh = true; // Toggle between baked (deformed) and base mesh vertices
    public bool applyRotationFix = true; // Toggle to apply -90 degree rotation fix
    public bool showAssignedVertices = true; // Toggle to show assigned vertices in blue

    public SkinnedMeshRenderer _skinnedMeshRenderer;
    public Mesh _bakedMesh; // Temporary mesh for baking

    void Start()
    {
        // Seed the random number generator with current time
        int seed = (int)System.DateTime.Now.Ticks;
        Random.InitState(seed);

        // Get the SkinnedMeshRenderer component
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (_skinnedMeshRenderer != null && _skinnedMeshRenderer.sharedMesh != null)
        {
            // Initialize baked mesh if needed
            if (useBakedMesh)
            {
                _bakedMesh = new Mesh();
                _skinnedMeshRenderer.BakeMesh(_bakedMesh);
                _vertices = _bakedMesh.vertices;
            }
            else
            {
                _vertices = _skinnedMeshRenderer.sharedMesh.vertices;
            }

            // Get bone weights from the shared mesh
            _boneWeights = _skinnedMeshRenderer.sharedMesh.boneWeights;
        }
    }

    void Update()
    {
        // Update vertices if using baked mesh (to account for animations)
        if (useBakedMesh && _skinnedMeshRenderer != null && _bakedMesh != null)
        {
            _skinnedMeshRenderer.BakeMesh(_bakedMesh);
            _vertices = _bakedMesh.vertices;
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_vertices == null || _vertices.Length == 0 || _boneWeights == null || _boneWeights.Length == 0) return;

        Matrix4x4 ltw = transform.localToWorldMatrix;

        // Define rotation fix: -90 degrees around X-axis, applied around the GameObject's position
        Quaternion rotationFix = applyRotationFix ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotationFix);

        // For baked mesh, we need the GameObject's position to apply the fix correctly
        Vector3 objectPosition = transform.position;
        Matrix4x4 objectToWorld = Matrix4x4.TRS(objectPosition, Quaternion.identity, Vector3.one);

        // Draw all vertices
        for (int i = 0; i < _vertices.Length; i++)
        {
            // Check if vertex has no valid bone weights or is assigned to bone #0 with weight 1
            bool isUnassigned = IsVertexUnassigned(i);

            // Set color: red for unassigned vertices, blue for assigned (if enabled)
            Gizmos.color = isUnassigned ? Color.red : (showAssignedVertices ? Color.blue : Color.clear);

            // Skip drawing if assigned vertex and showAssignedVertices is false
            if (!isUnassigned && !showAssignedVertices) continue;

            Vector3 vertexPos = _vertices[i];
            Vector3 worldPos;

            if (useBakedMesh)
            {
                // Baked vertices are in world space, apply rotation fix around the object's pivot
                if (applyRotationFix)
                {
                    Vector3 localPos = vertexPos - objectPosition; // Move to local space
                    localPos = rotationMatrix.MultiplyPoint3x4(localPos); // Apply rotation
                    worldPos = objectToWorld.MultiplyPoint3x4(localPos); // Back to world space
                }
                else
                {
                    worldPos = vertexPos; // Use baked vertex directly
                }
            }
            else
            {
                // Base mesh vertices are in local space, apply localToWorldMatrix and rotation fix
                worldPos = ltw.MultiplyPoint3x4(vertexPos);
                if (applyRotationFix)
                {
                    Vector3 localPos = worldPos - objectPosition;
                    localPos = rotationMatrix.MultiplyPoint3x4(localPos);
                    worldPos = objectToWorld.MultiplyPoint3x4(localPos);
                }
            }

            Gizmos.DrawSphere(worldPos, isUnassigned ? 0.001f : 0.02f); // Smaller for unassigned (as per your script)
            if (isUnassigned)
            {
                Gizmos.DrawWireSphere(worldPos, 0.03f); // Wireframe for unassigned vertices
            }
            Handles.Label(worldPos, i.ToString());
        }
    }

    // Check if a vertex has no valid bone weights or is assigned to bone #0 with weight 1
    private bool IsVertexUnassigned(int vertexIndex)
    {
        if (_boneWeights == null || vertexIndex < 0 || vertexIndex >= _boneWeights.Length)
            return false;

        BoneWeight bw = _boneWeights[vertexIndex];
        // A vertex is unassigned if all weights are zero or if it is assigned to bone #0 with weight 1
        if (bw.weight0 == 0f && bw.weight1 == 0f && bw.weight2 == 0f && bw.weight3 == 0f)
        {
            return true;
        }
        // Check for Unity's default assignment to bone #0 with weight 1
        if (bw.boneIndex0 == 0 && bw.weight0 == 1f && bw.weight1 == 0f && bw.weight2 == 0f && bw.weight3 == 0f)
        {
            return true;
        }
        return false;
    }
    #endif

    void OnDestroy()
    {
        // Clean up baked mesh to avoid memory leaks
        if (_bakedMesh != null)
        {
            Destroy(_bakedMesh);
        }
    }
}