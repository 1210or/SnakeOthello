using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stageMaker : MonoBehaviour
{
  public GameObject hexagon; //オブジェクトの元になるプレファブを格納
  public GameObject cam; //カメラを格納

  [SerializeField, Range(1, 100)]//スライダ
  private int stageSizeX = 15; //x方向のステージの大きさ

  [SerializeField, Range(1, 100)] //スライダ
  private int stageSizeZ = 15; //z方向のステージの大きさ

  //private float stageDistance = 1f;
    // Start is called before the first frame update
    void Start()
    {
      float[,] stagePosition = new float[stageSizeX,stageSizeZ]; //座標のための配列、ステージの大きさを決めて配列のサイズにする
      GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];//ゲームオブジェクトが入っている配列

      for(int z=0;z<stagePosition.GetLength(0);z++) //zの大きさぶんループ
      {
        for(int x=0;x<stagePosition.GetLength(1);x++)//xの大きさぶんループ
        {
          //オブジェクトをインスタンスコピー
          stageObject[x,z] = Instantiate(hexagon, new Vector3(x + z*0.5f, 0, z) , Quaternion.identity);
          
          //オブジェクトの名前変更
          stageObject[x,z].name = "hexagon" + x + "_" + z;
        }
      }

      //ステージのサイズに合わせてカメラを動かす
      cam.transform.position = new Vector3(((stageSizeX - 1) + ((stageSizeZ - 1)/2f))/2f, stageSizeX + stageSizeZ, (stageSizeZ - 1)/2 );

    }

    // Update is called once per frame
    void Update()
    {

    }
}
