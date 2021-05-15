using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelTilePlacer : MonoBehaviour
{
    //  Time interval between attempts to add new voxels
    public float waitingTime = 0.1f;

    //  Array for tile prefabs (Links to tiles already placed on the scene)
    public List<VoxelTile> TilePrefabs;

    //  Variable for the size of our map
    public Vector2Int MapSize = new Vector2Int(x: 10, y: 10);

    //  Two-dimensional array of tiles (We save here already spawned tiles)
    private VoxelTile[,] spawnedTiles;

    // Start is called before the first frame update
    private void Start()
    {
        //  Initialize the array to fit the map 
        spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        foreach(VoxelTile tilePrefab in TilePrefabs)
        {
            tilePrefab.CalculateSidesColors();
        }

        int countBeforeAdding = TilePrefabs.Count;
        for (int i = 0; i < countBeforeAdding; i++)
        {
            VoxelTile clone;
            switch (TilePrefabs[i].Rotation)
            {
                case VoxelTile.RotationType.OnlyRotation:
                    break;

                case VoxelTile.RotationType.TwoRotations:
                    TilePrefabs[i].Weight /= 2;
                    if (TilePrefabs[i].Weight <= 0) TilePrefabs[i].Weight = 1;

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    TilePrefabs.Add(clone);
                    break;

                case VoxelTile.RotationType.FourRotations:
                    TilePrefabs[i].Weight /= 4;
                    if (TilePrefabs[i].Weight <= 0) TilePrefabs[i].Weight = 1;

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right, Quaternion.identity);
                    clone.Rotate90();
                    TilePrefabs.Add(clone);

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right * 2, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    TilePrefabs.Add(clone);

                    clone = Instantiate(TilePrefabs[i], TilePrefabs[i].transform.position + Vector3.right * 3, Quaternion.identity);
                    clone.Rotate90();
                    clone.Rotate90();
                    clone.Rotate90();
                    TilePrefabs.Add(clone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //  Calling the map generation function 
        StartCoroutine(routine: Generate());
    }

    // Update is called once per frame
    private void Update()
    {
        //  Restarting our generation when pressing the d key
        if (Input.GetKeyDown(KeyCode.D))
        {
            StopAllCoroutines();

            foreach(VoxelTile spawnedTile in spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }

            StartCoroutine(routine: Generate());
        }
    }

    //  This function generates the whole map
    public IEnumerator Generate()
    {
        for (int x = 1; x < MapSize.x - 1; x++)
        {
            for (int y = 1; y < MapSize.y - 1; y++)
            {
                yield return new WaitForSeconds(0.02f);

                PlaceTile(x, y);
            }
        }

        yield return new WaitForSeconds(0.8f);
        foreach (VoxelTile spawnedTile in spawnedTiles)
        {
            if (spawnedTile != null) Destroy(spawnedTile.gameObject);
        }

        StartCoroutine(Generate());
    }

    //  Places a tile at a given location
    private void PlaceTile(int x, int y)
    {
        //  We add a list of tiles that are available (which we can put in this place)
        List<VoxelTile> availableTiles = new List<VoxelTile>();

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            if( CanAppendTile(existingTile: spawnedTiles[x-1,y], tileToAppend: tilePrefab, Direction.Left) &&
                CanAppendTile(existingTile: spawnedTiles[x - 1, y], tileToAppend: tilePrefab, Direction.Right) &&
                CanAppendTile(existingTile: spawnedTiles[x - 1, y], tileToAppend: tilePrefab, Direction.Back) &&
                CanAppendTile(existingTile: spawnedTiles[x - 1, y], tileToAppend: tilePrefab, Direction.Forward))
            {
                availableTiles.Add(tilePrefab);
            }
        }

        if (availableTiles.Count == 0) return;

        //  Choose a random tile from those that suit us
        //  Variable for saving a randomly selected tile from the available ones 
        VoxelTile selectedTile = GetRandomTile(availableTiles);
        Vector3 position = selectedTile.VoxelSize * selectedTile.TileSideVoxels * new Vector3(x, y: 0, z: y);
        spawnedTiles[x,y] = Instantiate(selectedTile, position, selectedTile.transform.rotation);
    }

    private VoxelTile GetRandomTile(List<VoxelTile> availableTiles)
    {
        List<float> chances = new List<float>();
        for( int i = 0; i < availableTiles.Count; i++)
        {
            chances.Add(availableTiles[i].Weight);
        }

        float value = Random.Range(0, chances.Sum());
        float sum = 0;

        for(int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if(value < sum)
            {
                return availableTiles[i];
            }
        }

        return availableTiles[availableTiles.Count - 1];
    }

    //  Function to check if we can add a tile
    private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Direction direction)
    {
        if (existingTile == null) return true;

        if (direction == Direction.Right)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft); //  Compare two arrays
        }
        else if (direction == Direction.Left)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight); //  Compare two arrays
        }
        else if (direction == Direction.Forward)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack); //  Compare two arrays
        }
        else if (direction == Direction.Back)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsBack, tileToAppend.ColorsForward); //  Compare two arrays
        }
        else
        {
            throw new ArgumentException(message: "Wrong direction value, should be Vector3.left/right/back/forward",
                nameof(direction));
        }
    }
}
