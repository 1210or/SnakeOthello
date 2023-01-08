using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StageMaker : MonoBehaviour
{
  public GameObject hexagon; //オブジェクトの元になるプレファブを格納
  public GameObject cam; //カメラを格納

  [SerializeField, Range(1, 100)]//スライダ
  public static int stageSizeX = 15; //x方向のステージの大きさ

  [SerializeField, Range(1, 100)] //スライダ
  public static int stageSizeZ = 15; //z方向のステージの大きさ
  
  public static float[,] stagePosition = new float[stageSizeX,stageSizeZ]; //座標のための配列、ステージの大きさを決めて配列のサイズにする
  public static GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];//ゲームオブジェクトが入っている配列
  
  //ステージのx方向の大きさはcos30の2倍
  public  static float hexSizeX = MathF.Cos(MathF.PI/6);
  public  static float hexSizeZ = 0.75f;

  public static int stageProcessFlag = 0;
  
    // Start is called before the first frame update
    void Start()
    {     
      for(int z=0;z<stagePosition.GetLength(0);z++) //zの大きさぶんループ
      {
        for(int x=0;x<stagePosition.GetLength(1);x++)//xの大きさぶんループ
        {
           //オブジェクトをインスタンスコピー
          stageObject[x,z] = Instantiate(hexagon, new Vector3(hexSizeX *  x + z　*　0.5f　* hexSizeX, 0, z * hexSizeZ ) , new Quaternion(1,0,0,-1));
          
          //オブジェクトの名前変更
          stageObject[x,z].name = "hexagon" + x + "_" + z;
          
          //ステージの端を定義
          if(x == 0 || z == 0 || x == stageSizeX - 1 || z == stageSizeZ -1)
          {
            stageObject[x,z].tag = "stageEdge";
          }
        }
      }

      //ステージの中心を探す
      Vector3 stageCenter = (stageObject[(stageSizeX-1)/2,(stageSizeZ-1)/2].transform.position);

      //ステージのサイズに合わせてカメラを動かす
      cam.transform.position = new Vector3(stageCenter.x, (stageSizeX + stageSizeZ) * 0.8f, stageCenter.z );
    }

    // Update is called once per frame
    void Update()
    {
      if(stageProcessFlag == stageSizeX*stageSizeZ ){
        ChangeHexColor.arroundScanFlag = false;//フラグを立てる
        stageProcessFlag = 0;
      }
    }
}

public class StageHexagon{
  //引数に入れたゲームオブジェクト(ステージ)の接するステージを配列にして返す
  public static GameObject[] arroundHexagons (GameObject hexagon)
  {
          //位置座標からインデックスに変換
          int x = (int)(hexagon.transform.position.x/StageMaker.hexSizeX - (0.5f * hexagon.transform.position.z)/StageMaker.hexSizeX);
          int z = (int)(hexagon.transform.position.z/StageMaker.hexSizeZ);

          //配列に周囲のステージを格納
          GameObject[] arroundHexagons　= new GameObject[6];
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
}
