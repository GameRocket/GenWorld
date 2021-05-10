using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelTilePlacer : MonoBehaviour
{
    //  Array for tile prefabs (Links to tiles already placed on the scene)
    public VoxelTile[] TilePrefabs;

    //  Variable for the size of our map
    public Vector2Int MapSize = new Vector2Int(x: 10, y: 10);

    //  Two-dimensional array of tiles (We save here already spawned tiles)
    private VoxelTile[,] spawnedTiles;

    // Start is called before the first frame update
    void Start()
    {
        //  Initialize the array to fit the map 
        spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        //  Calling the map generation function 
        StartCoroutine(routine: Generate());
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    //  This function generates the whole map
    public IEnumerator Generate()
    {
        //  Add the first tile to the center of the map
        PlaceTile(x: MapSize.x / 2, y: MapSize.y / 2);

        //  Cycle for adding subsequent tiles
        while (true)
        {
            Vector2Int pos; //  Variable for tile position
            while (true)
            {
                //  We will generate a random position for the tile 
                pos = new Vector2Int(x: Random.Range(1, MapSize.x - 1), y: Random.Range(1, MapSize.y - 1));

                //  Checking if the generated position suits us
                if (spawnedTiles[pos.x, pos.y] == null &&
                    (spawnedTiles[pos.x+1, pos.y] != null ||
                    spawnedTiles[pos.x-1, pos.y] != null ||
                    spawnedTiles[pos.x, pos.y+1] != null ||
                    spawnedTiles[pos.x, pos.y-1] != null))
                {
                    break;
                }
            }
            //  We will wait 200 milliseconds before adding a new tile 
            yield return new WaitForSeconds(0.2f);

            PlaceTile(pos.x, pos.y);    //  Add the next tile to the map
        }
    }

    //  Places a tile at a given location
    private void PlaceTile(int x, int y)
    {
        //  We add a list of tiles that are available (which we can put in this place)
        List<VoxelTile> availableTiles = new List<VoxelTile>();

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            if( CanAppendTile(existingTile: spawnedTiles[x-1,y], tileToAppend: tilePrefab, Vector3.left) &&
                CanAppendTile(existingTile: spawnedTiles[x - 1, y], tileToAppend: tilePrefab, Vector3.right) &&
                CanAppendTile(existingTile: spawnedTiles[x - 1, y], tileToAppend: tilePrefab, Vector3.back) &&
                CanAppendTile(existingTile: spawnedTiles[x - 1, y], tileToAppend: tilePrefab, Vector3.forward))
            {
                availableTiles.Add(tilePrefab);
            }
        }

        if (availableTiles.Count == 0) return;

        //  Choose a random tile from those that suit us
        //  Variable for saving a randomly selected tile from the available ones 
        VoxelTile selectedTile = availableTiles[Random.Range(0, availableTiles.Count)];

        spawnedTiles[x,y] = Instantiate(selectedTile, position: new Vector3(x, y: 0, z: y)*0.8f, Quaternion.identity);
    }

    //  Function to check if we can add a tile
    private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Vector3 direction)
    {
        if (existingTile == null) return true;

        if (direction == Vector3.left)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft); //  Compare two arrays
        }
        else if (direction == Vector3.right)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight); //  Compare two arrays
        }
        else if (direction == Vector3.right)
        {
            return Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack); //  Compare two arrays
        }
        else if (direction == Vector3.right)
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
