using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Leaf
{
    public Matrix4x4 transform;
    public float size;

    public Leaf(Matrix4x4 transform, float size)
    {
        this.transform = transform;
        this.size = size;
    }
};

public static class LeafMeshBuilder
{
    public static CombineInstance CreateLeaf(Leaf leafData)
    {
        Matrix4x4 t1 = Matrix4x4.Rotate(Quaternion.Euler(0, Random.Range(0, 360), 0));
        Matrix4x4 t2 = t1 * Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));

        CombineInstance[] leafInstances = new CombineInstance[]
        {
            CreateQuad(t1),
            CreateQuad(t2)
        };

        Mesh leavesMesh = new Mesh();
        leavesMesh.CombineMeshes(leafInstances);

        // Apply leaf size
        Matrix4x4 leafTransform = leafData.transform;
        leafTransform *= Matrix4x4.Scale(Vector3.one * leafData.size);

        CombineInstance result = new CombineInstance();
        result.mesh = leavesMesh;
        result.transform = leafTransform;
        return result;
    }

    private static CombineInstance CreateQuad(Matrix4x4 transform)
    {
        Vector3[] verts = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };

        Vector2[] uvs = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        Vector3[] normals = new Vector3[] {
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1)
        };

        int[] tris = new int[] {
            0, 2, 1,
            1, 2, 3
        };

        Mesh quadMesh = new Mesh();
        quadMesh.SetVertices(verts);
        quadMesh.SetNormals(normals);
        quadMesh.SetUVs(0, uvs);
        quadMesh.SetTriangles(tris, 0);

        // Move x-axis origin to center
        Matrix4x4 t = transform;
        t = t * Matrix4x4.Translate(Vector3.left * 0.5f);

        CombineInstance quadInstance = new CombineInstance();
        quadInstance.mesh = quadMesh;
        quadInstance.transform = t;

        return quadInstance;
    }

    public static void ProjectNormals(Mesh mesh, Vector3 origin)
    {
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            normals[i] = (mesh.vertices[i] - origin).normalized;
        }

        mesh.SetNormals(normals);
    }
}