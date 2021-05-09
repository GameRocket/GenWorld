using System;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    //  Variable defining voxel size
    public float VoxelSize = 0.5f;
    public int TileSideVoxels = 40;

    [HideInInspector] public byte[] ColorsRight;
    [HideInInspector] public byte[] ColorsForward;
    [HideInInspector] public byte[] ColorsLeft;
    [HideInInspector] public byte[] ColorsBack;

    // Start is called before the first frame update
    void Start()
    {
        ColorsRight = new byte[TileSideVoxels * TileSideVoxels];
        ColorsForward = new byte[TileSideVoxels * TileSideVoxels];
        ColorsLeft = new byte[TileSideVoxels * TileSideVoxels];
        ColorsBack = new byte[TileSideVoxels * TileSideVoxels];

        for (int y = 0; y < TileSideVoxels; y++)
        {
            for (int i = 0; i < TileSideVoxels; i++)
            {
                ColorsRight[y * TileSideVoxels + i] = GetVoxelColor(verticalLayer: y, horizontalOffset: i, Vector3.right);
                ColorsForward[y * TileSideVoxels + i] = GetVoxelColor(verticalLayer: y, horizontalOffset: i, Vector3.forward);
                ColorsLeft[y * TileSideVoxels + i] = GetVoxelColor(verticalLayer: y, horizontalOffset: i, Vector3.left);
                ColorsBack[y * TileSideVoxels + i] = GetVoxelColor(verticalLayer: y, horizontalOffset: i, Vector3.back);
            }
        }
        Debug.Log(message: string.Join(separator:", ", ColorsRight));
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
    private byte GetVoxelColor(int verticalLayer, int horizontalOffset, Vector3 direction)
    {
        //  First, we grab the voxels from the front of the tile
        //  In this variable we save the MeshCollider component
        var meshCollider = GetComponentInChildren<MeshCollider>();

        //  Variables for saving voxel size 
        float vox = VoxelSize;
        float half = VoxelSize / 2;

        //  Here we start a ray, setting from which coordinate it will go 
        Vector3 rayStart;

        //  Checking the direction of the ray 
        if (direction == Vector3.right)
        {
            rayStart = meshCollider.bounds.min +
                        new Vector3(x: -half, y: 0, z: half + horizontalOffset * vox);
        }
        else if(direction == Vector3.forward)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(x: half + horizontalOffset * vox, y: 0, z: -half);
        }
        else if (direction == Vector3.left)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(x: half, y:0, z:-half - (TileSideVoxels - horizontalOffset - 1) * vox);
        }
        else if (direction == Vector3.back)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(x: -half - (TileSideVoxels - horizontalOffset - 1) * vox, y: 0, z: half);
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Vector3.left/right/back/forward", nameof(direction));
        }

        rayStart.y = meshCollider.bounds.min.y + half + verticalLayer * vox;

        //  Ray positions in XYZ
        //rayStart.x += 0.5f * VoxelSize + horizontalOffset*VoxelSize;
        //rayStart.y += 0.5f * VoxelSize + verticalLayer*VoxelSize;
        //rayStart.z -= 0.5f * VoxelSize;

        //  We trying to debug if script work by drawing a ray
        Debug.DrawRay(rayStart, dir: direction*.1f, Color.blue, duration: 2);

        //  We launch a ray that hits the collider and saves this point color
        if (Physics.Raycast(new Ray(origin: rayStart, direction), out RaycastHit hit, vox))
        {
            //  Variable in which we save our meshCollider
            Mesh mesh = meshCollider.sharedMesh;

            //  Save the index of the triangle that the ray hit 
            int hitTriangleVertex = mesh.triangles[hit.triangleIndex * 3];

            //  Get coordinates in texture
            byte colorIndex = (byte)(mesh.uv[hitTriangleVertex].x * 256);

            //Debug.Log(colorIndex);

            return colorIndex;
        }
        //  If our raycast didn't get anywhere, it returns zero 
        return 0;
    }
}
