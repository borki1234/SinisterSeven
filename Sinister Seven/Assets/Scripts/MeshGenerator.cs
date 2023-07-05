using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    MeshCollider meshCollider;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 200;
    public int zSize = 200;
    public float noiseScale = 0.05f; // Adjust the noise scale for finer details
    public int octaves = 4; // Increase the number of octaves for more detail
    public float persistence = 0.5f; // Adjust the persistence for more variation

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = 0f;

                // Apply multiple octaves of Perlin noise for smoothing
                float amplitude = 10f;
                float frequency = 0.2f;
                float maxNoiseHeight = 0f;

                for (int oct = 0; oct < octaves; oct++)
                {
                    float perlinX = (x * noiseScale * frequency);
                    float perlinZ = (z * noiseScale * frequency);
                    float noiseHeight = Mathf.PerlinNoise(perlinX, perlinZ) * 2f - 1f;
                    y += noiseHeight * amplitude;

                    maxNoiseHeight += amplitude;
                    amplitude *= persistence;
                    frequency *= 2f;
                }

                y /= maxNoiseHeight;
                y *= 10f; // Adjust the scale of the terrain

                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Update the collider mesh
        meshCollider.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }

    void Update()
    {

    }
}
