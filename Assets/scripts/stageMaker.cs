using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stageMaker : MonoBehaviour
{
  public GameObject hexagon;
  public GameObject cam;

  [SerializeField, Range(1, 100)]
  private int stageSizeX = 15;

  [SerializeField, Range(1, 100)]
  private int stageSizeZ = 15;

  //private float stageDistance = 1f;
    // Start is called before the first frame update
    void Start()
    {
      float[,] stagePosition = new float[stageSizeX,stageSizeZ];
      GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];

      for(int z=0;z<stagePosition.GetLength(0);z++)
      {
        for(int x=0;x<stagePosition.GetLength(1);x++)
        {
          stageObject[x,z] = Instantiate(hexagon, new Vector3(x + z*0.5f, 0, z) , Quaternion.identity);
          stageObject[x,z].name = "hexagon" + x + "_" + z;
        }
      }

      cam.transform.position = new Vector3(((stageSizeX - 1) + ((stageSizeZ - 1)/2f))/2f, stageSizeX + stageSizeZ, (stageSizeZ - 1)/2 );

    }

    // Update is called once per frame
    void Update()
    {

    }
}
