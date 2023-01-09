using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    //マスの形状
  public GameObject hexagon;

  public int stageIndexX = 0;
  public int stageIndexZ = 0;

  public bool isEdge =false;
  public bool isPlayerOn = false;

  public int stagePowerValue = 0; //どちら陣営か判定

  //引数に入れたゲームオブジェクト(ステージ)の接するステージを配列にして返す
  public static GameObject[] arroundHexagons (GameObject hexagon, int x, int z)
  {

          //配列に周囲のステージを格納
          GameObject[] arroundHexagons = new GameObject[6];
          try{arroundHexagons[0] = StageMaker.stageObject[x,z-1];}catch (System.Exception){arroundHexagons[0] = hexagon;}
          try{arroundHexagons[1] = StageMaker.stageObject[x+1,z-1];}catch (System.Exception){arroundHexagons[1] = hexagon;}
          try{arroundHexagons[2] = StageMaker.stageObject[x-1,z];}catch (System.Exception){arroundHexagons[2] = hexagon;}
          try{arroundHexagons[3] = StageMaker.stageObject[x+1,z];}catch (System.Exception){arroundHexagons[3] = hexagon;}
          try{arroundHexagons[4] = StageMaker.stageObject[x-1,z+1];}catch (System.Exception){arroundHexagons[4] = hexagon;}
          try{arroundHexagons[5] = StageMaker.stageObject[x,z+1];}catch (System.Exception){arroundHexagons[5] = hexagon;}
          //↑冗長、端で何もない場合は中心のhexagonを入れてるけれど、本当は配列要素自体を消したい。Listで作るべき？
        
        //arroundHexagons = arroundHexagons.Where(value => value != hexagon).ToArray();
        //print(arroundHexagons.Length);
        return arroundHexagons;
  }

  public static void fillColor(GameObject[,] stageObject, Color color){

      for(int z=0;z<stageObject.GetLength(0);z++) //zの大きさぶんループ
      {
        for(int x=0;x<stageObject.GetLength(1);x++)//xの大きさぶんループ
        {
          if(stageObject[x,z].tag == "stageEdge")
          {
            //ステージの走査値を0にする、オブジェクトに変数を持ちたい
          }
        }
      }
  }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
