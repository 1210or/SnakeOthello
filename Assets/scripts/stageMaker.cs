using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stageMaker : MonoBehaviour
{
  public GameObject hexagon; //オブジェクトの元になるプレファブを格納
  public GameObject cam; //カメラを格納

  [SerializeField, Range(1, 100)]//スライダ
  public static int stageSizeX = 15; //x方向のステージの大きさ

  [SerializeField, Range(1, 100)] //スライダ
  public static int stageSizeZ = 15; //z方向のステージの大きさ
  
  public static float[,] stagePosition = new float[stageSizeX,stageSizeZ]; //座標のための配列、ステージの大きさを決めて配列のサイズにする
  public static GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];//ゲームオブジェクトが入っている配列
  
    // Start is called before the first frame update
    void Start()
    {
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
public class StageHexagon{
  //引数に入れたゲームオブジェクト(ステージ)の接するステージを配列にして返す
  public static GameObject[] arroundHexagons (GameObject hexagon)
  {
          //位置座標からインデックスに変換
          int x = (int)(hexagon.transform.position.x - (0.5f * hexagon.transform.position.z));
          int z = (int)(hexagon.transform.position.z);

          //配列に周囲のステージを格納
          GameObject[] arroundHexagons　= new GameObject[6];
          try{arroundHexagons[0] = stageMaker.stageObject[x,z-1];}catch (System.Exception){arroundHexagons[0] = hexagon;}
          try{arroundHexagons[1] = stageMaker.stageObject[x+1,z-1];}catch (System.Exception){arroundHexagons[1] = hexagon;}
          try{arroundHexagons[2] = stageMaker.stageObject[x-1,z];}catch (System.Exception){arroundHexagons[2] = hexagon;}
          try{arroundHexagons[3] = stageMaker.stageObject[x+1,z];}catch (System.Exception){arroundHexagons[3] = hexagon;}
          try{arroundHexagons[4] = stageMaker.stageObject[x-1,z+1];}catch (System.Exception){arroundHexagons[4] = hexagon;}
          try{arroundHexagons[5] = stageMaker.stageObject[x,z+1];}catch (System.Exception){arroundHexagons[5] = hexagon;}
          //↑冗長、端で何もない場合は中心のhexagonを入れてるけれど、本当は配列自体を消したい。

          return arroundHexagons;
  }
}
