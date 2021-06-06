using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ClearSceneFromVoxels();
    }

    //  Clears all voxel clones from the scene 
    public void ClearSceneFromVoxels()
    {
        GameObject[] GameObjects = GameObject.FindGameObjectsWithTag("Voxel");

        for (int i = 0; i < GameObjects.Length; i++)
        {
                Destroy(GameObjects[i]);
        }

    }

    //  Quit from game scene
    public void QuitGameScene()
    {
        Application.Quit();
    }
}
