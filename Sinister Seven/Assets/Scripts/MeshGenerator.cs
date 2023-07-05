using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    MeshCollider meshCollider;

    int chunkSize = 10;
    int numChunksX;
    int numChunksZ;
    List<Vector3[]> chunkVertices;
    List<int[]> chunkTriangles;
    Dictionary<Vector2Int, GameObject> chunkObjects;

    public int xSize = 200;
    public int zSize = 200;
    public float noiseScale = 0.05f;
    public int octaves = 4;
    public float persistence = 0.5f;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        numChunksX = Mathf.CeilToInt((float)xSize / chunkSize);
        numChunksZ = Mathf.CeilToInt((float)zSize / chunkSize);

        chunkVertices = new List<Vector3[]>();
        chunkTriangles = new List<int[]>();
        chunkObjects = new Dictionary<Vector2Int, GameObject>();

        for (int cz = 0; cz < numChunksZ; cz++)
        {
            for (int cx = 0; cx < numChunksX; cx++)
            {
                Vector2Int chunkKey = new Vector2Int(cx, cz);
                GameObject chunkObject = new GameObject("Chunk_" + chunkKey);
                chunkObject.transform.parent = transform;
                chunkObjects.Add(chunkKey, chunkObject);

                int chunkStartX = cx * chunkSize;
                int chunkStartZ = cz * chunkSize;
                int chunkEndX = Mathf.Min(chunkStartX + chunkSize, xSize + 1);
                int chunkEndZ = Mathf.Min(chunkStartZ + chunkSize, zSize + 1);
                int chunkWidth = chunkEndX - chunkStartX;
                int chunkHeight = chunkEndZ - chunkStartZ;

                Vector3[] chunkVerts = new Vector3[(chunkWidth + 1) * (chunkHeight + 1)];
                int[] chunkTris = new int[chunkWidth * chunkHeight * 6];

                int vertIndex = 0;
                int triIndex = 0;

                for (int z = chunkStartZ; z <= chunkEndZ; z++)
                {
                    for (int x = chunkStartX; x <= chunkEndX; x++)
                    {
                        float y = CalculateHeight(x, z);

                        chunkVerts[vertIndex] = new Vector3(x, y, z);

                        if (x < chunkEndX && z < chunkEndZ)
                        {
                            chunkTris[triIndex + 0] = vertIndex;
                            chunkTris[triIndex + 1] = vertIndex + chunkWidth + 1;
                            chunkTris[triIndex + 2] = vertIndex + 1;
                            chunkTris[triIndex + 3] = vertIndex + 1;
                            chunkTris[triIndex + 4] = vertIndex + chunkWidth + 1;
                            chunkTris[triIndex + 5] = vertIndex + chunkWidth + 2;

                            triIndex += 6;
                        }

                        vertIndex++;
                    }
                }

                chunkVertices.Add(chunkVerts);
                chunkTriangles.Add(chunkTris);

                Mesh chunkMesh = new Mesh();
                chunkMesh.vertices = chunkVerts;
                chunkMesh.triangles = chunkTris;
                chunkMesh.RecalculateNormals();

                MeshFilter chunkMeshFilter = chunkObject.AddComponent<MeshFilter>();
                MeshRenderer chunkMeshRenderer = chunkObject.AddComponent<MeshRenderer>();
                chunkMeshFilter.mesh = chunkMesh;
                chunkMeshRenderer.material = GetComponent<MeshRenderer>().material;

                MeshCollider chunkMeshCollider = chunkObject.AddComponent<MeshCollider>();
                chunkMeshCollider.sharedMesh = chunkMesh;
            }
        }
    }

    float CalculateHeight(int x, int z)
    {
        float y = 0f;

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
        y *= 10f;

        return y;
    }

    private void OnDrawGizmos()
    {
        if (chunkVertices == null)
            return;

        foreach (Vector3[] chunk in chunkVertices)
        {
            for (int i = 0; i < chunk.Length; i++)
            {
                Gizmos.DrawSphere(chunk[i], .1f);
            }
        }
    }

    void Update()
    {
        // Add logic to determine which chunks are visible and activate/deactivate/render only those chunks
    }
}
