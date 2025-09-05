using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public static class VertexIndexDisplayEditor
{
    private static bool useBakedMesh = true; // Toggle between baked (deformed) and base mesh vertices
    private static bool applyRotationFix = true; // Toggle to apply -90 degree rotation fix
    private static bool showAssignedVertices = true; // Toggle to show assigned vertices in blue
    private static bool showVertexLabels = true; // Toggle to show vertex index labels
    private static Mesh bakedMesh; // Temporary mesh for baking
    private static readonly List<int> unassignedVertices = new List<int>(); // Cache unassigned vertex indices
    private static Rect windowRect = new Rect(10, 10, 200, 120); // Floating window position and size
    private static Vector2 lastMousePosition; // For dragging the window

    static VertexIndexDisplayEditor()
    {
        // Subscribe to SceneView.duringSceneGui to draw gizmos
        SceneView.duringSceneGui += OnSceneGUI;
        // Seed the random number generator with current time
        int seed = (int)System.DateTime.Now.Ticks;
        Random.InitState(seed);
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // Get the selected GameObject
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null) return;

        // Get the SkinnedMeshRenderer component
        SkinnedMeshRenderer skinnedMeshRenderer = selectedObject.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) return;

        // Get vertices and bone weights
        Vector3[] vertices;
        BoneWeight[] boneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

        if (useBakedMesh)
        {
            if (bakedMesh == null) bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            vertices = bakedMesh.vertices;
        }
        else
        {
            vertices = skinnedMeshRenderer.sharedMesh.vertices;
        }

        if (vertices == null || vertices.Length == 0 || boneWeights == null || boneWeights.Length == 0) return;

        // Cache unassigned vertices to avoid recalculating
        unassignedVertices.Clear();
        for (int i = 0; i < boneWeights.Length; i++)
        {
            if (IsVertexUnassigned(i, boneWeights))
            {
                unassignedVertices.Add(i);
            }
        }

        // Set up transformation matrices
        Matrix4x4 ltw = skinnedMeshRenderer.transform.localToWorldMatrix;
        Quaternion rotationFix = applyRotationFix ? Quaternion.Euler(-90f, 0f, 0f) : Quaternion.identity;
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotationFix);
        Vector3 objectPosition = skinnedMeshRenderer.transform.position;
        Matrix4x4 objectToWorld = Matrix4x4.TRS(objectPosition, Quaternion.identity, Vector3.one);

        // Draw vertices
        foreach (int i in unassignedVertices)
        {
            Vector3 vertexPos = vertices[i];
            Vector3 worldPos = GetWorldPosition(vertexPos, useBakedMesh, ltw, applyRotationFix, rotationMatrix, objectPosition, objectToWorld);

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, worldPos, Quaternion.identity, 0.001f, EventType.Repaint); // Solid sphere
            Handles.DrawWireDisc(worldPos, Vector3.up, 0.03f); // Wireframe disc
            if (showVertexLabels) Handles.Label(worldPos, i.ToString());
        }

        // Draw assigned vertices if enabled
        if (showAssignedVertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                if (unassignedVertices.Contains(i)) continue; // Skip unassigned vertices

                Vector3 vertexPos = vertices[i];
                Vector3 worldPos = GetWorldPosition(vertexPos, useBakedMesh, ltw, applyRotationFix, rotationMatrix, objectPosition, objectToWorld);

                Handles.color = Color.blue;
                Handles.SphereHandleCap(0, worldPos, Quaternion.identity, 0.02f, EventType.Repaint);
                if (showVertexLabels) Handles.Label(worldPos, i.ToString());
            }
        }

        // Draw floating settings window
        Handles.BeginGUI();
        windowRect = GUI.Window(0, windowRect, DrawSettingsWindow, "Vertex Display Settings");
        Handles.EndGUI();
    }

    private static void DrawSettingsWindow(int windowID)
    {
        // Add a drag handle
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));

        // Draw settings
        GUILayout.BeginVertical();
        useBakedMesh = EditorGUILayout.Toggle("Use Baked Mesh", useBakedMesh);
        applyRotationFix = EditorGUILayout.Toggle("Apply Rotation Fix", applyRotationFix);
        showAssignedVertices = EditorGUILayout.Toggle("Show Assigned Vertices", showAssignedVertices);
        showVertexLabels = EditorGUILayout.Toggle("Show Vertex Labels", showVertexLabels);
        GUILayout.EndVertical();

        // Handle dragging
        if (Event.current.type == EventType.MouseDrag && windowRect.Contains(Event.current.mousePosition))
        {
            windowRect.position += Event.current.mousePosition - lastMousePosition;
            lastMousePosition = Event.current.mousePosition;
            SceneView.RepaintAll();
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            lastMousePosition = Event.current.mousePosition;
        }
    }

    private static Vector3 GetWorldPosition(Vector3 vertexPos, bool useBakedMesh, Matrix4x4 ltw, bool applyRotationFix, Matrix4x4 rotationMatrix, Vector3 objectPosition, Matrix4x4 objectToWorld)
    {
        Vector3 worldPos;
        if (useBakedMesh)
        {
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
            worldPos = ltw.MultiplyPoint3x4(vertexPos);
            if (applyRotationFix)
            {
                Vector3 localPos = worldPos - objectPosition;
                localPos = rotationMatrix.MultiplyPoint3x4(localPos);
                worldPos = objectToWorld.MultiplyPoint3x4(localPos);
            }
        }
        return worldPos;
    }

    private static bool IsVertexUnassigned(int vertexIndex, BoneWeight[] boneWeights)
    {
        if (boneWeights == null || vertexIndex < 0 || vertexIndex >= boneWeights.Length)
            return false;

        BoneWeight bw = boneWeights[vertexIndex];
        // A vertex is unassigned if all weights are zero or if it is assigned to bone #0 with weight 1
        if (bw.weight0 == 0f && bw.weight1 == 0f && bw.weight2 == 0f && bw.weight3 == 0f)
        {
            return true;
        }
        if (bw.boneIndex0 == 0 && bw.weight0 == 1f && bw.weight1 == 0f && bw.weight2 == 0f && bw.weight3 == 0f)
        {
            return true;
        }
        return false;
    }

    [MenuItem("Tools/Clean Up Baked Mesh")]
    private static void CleanUp()
    {
        if (bakedMesh != null)
        {
            Object.DestroyImmediate(bakedMesh);
            bakedMesh = null;
        }
    }
}