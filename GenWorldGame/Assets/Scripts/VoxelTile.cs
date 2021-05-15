using System;
using UnityEngine;

public class VoxelTile : MonoBehaviour
{
    //  Variable defining voxel size
    public float VoxelSize = 0.1f;
    public int TileSideVoxels = 40;

    //  Weight variable responsible for the chance of a specific pixel to spawn
    [Range(1, 100)]         //  We indicate the boundaries in which the frequency can change 
    public int Weight = 50;

    public RotationType Rotation;

    public enum RotationType
    {
        OnlyRotation,
        TwoRotations,
        FourRotations
    }

    [HideInInspector] public byte[] ColorsRight;
    [HideInInspector] public byte[] ColorsForward;
    [HideInInspector] public byte[] ColorsLeft;
    [HideInInspector] public byte[] ColorsBack;

    // Start is called before the first frame update
    public void CalculateSidesColors()
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
        //Debug.Log(message: string.Join(separator:", ", ColorsRight));
    }

    public void Rotate90()
    {
        transform.Rotate(0, 90, 0);

        byte[] colorsRightNew = new byte[TileSideVoxels * TileSideVoxels];
        byte[] colorsForwardNew = new byte[TileSideVoxels * TileSideVoxels];
        byte[] colorsLeftNew = new byte[TileSideVoxels * TileSideVoxels];
        byte[] colorsBackNew = new byte[TileSideVoxels * TileSideVoxels];

        for (int layer = 0; layer < TileSideVoxels; layer++)
        {
            for (int offset = 0; offset < TileSideVoxels; offset++)
            {
                colorsRightNew[layer * TileSideVoxels + offset] = ColorsForward[layer * TileSideVoxels + TileSideVoxels - offset - 1];
                colorsForwardNew[layer * TileSideVoxels + offset] = ColorsLeft[layer * TileSideVoxels + offset];
                colorsLeftNew[layer * TileSideVoxels + offset] = ColorsBack[layer * TileSideVoxels + TileSideVoxels - offset - 1];
                colorsBackNew[layer * TileSideVoxels + offset] = ColorsRight[layer * TileSideVoxels + offset];
            }
        }

        ColorsRight = colorsRightNew;
        ColorsForward = colorsForwardNew;
        ColorsLeft = colorsLeftNew;
        ColorsBack = colorsBackNew;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

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
        Vector3 rayDir;
        //  Checking the direction of the ray 
        if (direction == Vector3.right)
        {
            rayStart = meshCollider.bounds.min +
                        new Vector3(x: -half, y: 0, z: half + horizontalOffset * vox);
            rayDir = Vector3.right;
        }
        else if(direction == Vector3.forward)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(x: half + horizontalOffset * vox, y: 0, z: -half);
            rayDir = Vector3.forward;
        }
        else if (direction == Vector3.left)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(x: half, y:0, z:-half - (TileSideVoxels - horizontalOffset - 1) * vox);
            rayDir = Vector3.left;
        }
        else if (direction == Vector3.back)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(x: -half - (TileSideVoxels - horizontalOffset - 1) * vox, y: 0, z: half);
            rayDir = Vector3.back;
        }
        else
        {
            throw new ArgumentException("Wrong direction value, should be Direction.left/right/back/forward",
                nameof(direction));
        }

        rayStart.y = meshCollider.bounds.min.y + half + verticalLayer * vox;

        //  Ray positions in XYZ
        //rayStart.x += 0.5f * VoxelSize + horizontalOffset*VoxelSize;
        //rayStart.y += 0.5f * VoxelSize + verticalLayer*VoxelSize;
        //rayStart.z -= 0.5f * VoxelSize;

        int debugDuration = 100;    //  How many seconds will we see debug information 

        //  We trying to debug if script work by drawing a ray
        //Debug.DrawRay(rayStart, dir: direction*.1f, Color.blue, duration: debugDuration);

        //  We launch a ray that hits the collider and saves this point color
        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit hit, vox))
        {
            //  Variable in which we save our meshCollider
            //Mesh mesh = meshCollider.sharedMesh;

            //  Save the index of the triangle that the ray hit 
            //int hitTriangleVertex = mesh.triangles[hit.triangleIndex * 3];

            //  Get coordinates in texture
            //byte colorIndex = (byte)(mesh.uv[hitTriangleVertex].x * 256);

            byte colorIndex = (byte)(hit.textureCoord.x * 256);

            if (colorIndex == 0) Debug.LogWarning("Found color 0 in mesh palette, this can cause conflicts");

            //Debug.Log(colorIndex);

            return colorIndex;
        }
        //  If our raycast didn't get anywhere, it returns zero 
        return 0;
    }
}
