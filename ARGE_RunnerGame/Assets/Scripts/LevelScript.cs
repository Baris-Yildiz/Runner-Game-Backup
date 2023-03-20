using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    public Transform obstacleOrigin;
    public GameObject jellyOrigin;

    public GameObject jellyPool;
    public GameObject player;

    public float jellyPositionZ = 10;

    public float obstaclePositionZ;
    public float obstaclePositionX;

    public void CreateNewLevel()
    {
        AddJellies();
    }

    public LevelScript()
    {
        
    }


    public void AddJellies()  //object pooling yap, x değerleri rastgele local pos a göre, z değerleri ise 10 arttırılarak gidecek.
    {
        int totalJelly = jellyOrigin.transform.childCount;
        
        for(int i = 0; i < totalJelly; i++)
        {
            GameObject jelly = jellyOrigin.transform.GetChild(i).gameObject;
            jelly.transform.localPosition = new Vector3(Random.Range(-3f,4f), 0.5f, jellyPositionZ);
            jellyPositionZ += 10;
        }
        jellyPositionZ = 10;
    }

    public void AddObstacle()
    {

    }

    public void DetermineX(float posValue)
    {
        
    }
    
    
}
