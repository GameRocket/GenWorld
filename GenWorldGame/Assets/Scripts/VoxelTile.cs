using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    //  Variable defining voxel size
    public float VoxelSize = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        GetVoxelColor(verticalLayer: 0, horizontalOffset: 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Voxel color retrieval function
    /// </summary>
    /// <param name="verticalLayer">The vertical layer on which we want to get the voxel color</param>
    /// <param name="horizontalOffset">The horizontal coordinate of voxel</param>
    private void GetVoxelColor(int verticalLayer, int horizontalOffset)
    {
        //  First, we grab the voxels from the front of the tile

        //  In this variable we save the MeshCollider component
        var meshCollider = GetComponentInChildren<MeshCollider>();

        //  Here we start a ray, setting from which coordinate it will go 
        Vector3 rayStart = meshCollider.bounds.min;
        rayStart.x += 0.5f * VoxelSize;
        rayStart.y += 0.5f * VoxelSize;
        rayStart.z -= 0.5f * VoxelSize;

        Debug.DrawRay(rayStart, dir: Vector3.forward, Color.blue, duration: 2);

        //  We launch a ray that hits the collider and saves this point color
        if (Physics.Raycast(new Ray(), out RaycastHit hit))
        {
            //  Variable in which we save our meshCollider
            Mesh mesh = meshCollider.sharedMesh;

            //  Save the index of the triangle that the ray hit 
            int hitTriangleVertex = meshCollider.sharedMesh.triangles[hit.triangleIndex*3+0];

            //  Get coordinates in texture
            Debug.Log(mesh.uv[hitTriangleVertex]);
        }
    }
}
