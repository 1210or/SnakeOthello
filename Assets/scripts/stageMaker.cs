using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StageMaker : MonoBehaviour
{
  public GameObject hexagon; //オブジェクトの元になるプレファブを格納
  public GameObject cam; //カメラを格納

  [SerializeField, Range(1, 100)]//スライダ
  public static int stageSizeX = 8; //x方向のステージの大きさ

  public static int stageSizeZ = stageSizeX; //z方向のステージの大きさ
  
  public float[,] stagePosition = new float[stageSizeX,stageSizeZ]; //座標のための配列、ステージの大きさを決めて配列のサイズにする
  public static GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];//ゲームオブジェクトが入っている配列
  
  //ステージのx方向の大きさはcos30の2倍
  public  static float hexSizeX = MathF.Cos(MathF.PI/6);
  public  static float hexSizeZ = 0.75f;

  public static int stageProcessFlag = 0;
  
  public GameObject stageParent;

    //ステージの周の数
   public static int ringsCount;

  
    // Start is called before the first frame update
    void Awake()
    {    
      
      //プレファブを並べる 
      for(int z=0;z<stagePosition.GetLength(0);z++) //zの大きさぶんループ
      {
        for(int x=0;x<stagePosition.GetLength(1);x++)//xの大きさぶんループ
        {
      
           //オブジェクトをインスタンスコピー
          stageObject[x,z] = Instantiate(hexagon, new Vector3(hexSizeX *  x + z * 0.5f * hexSizeX, 0, z * hexSizeZ ) , new Quaternion(1,0,0,-1));
          
          //オブジェクトの名前変更
          stageObject[x,z].name = "hexagon" + x + "_" + z;
          stageObject[x,z].transform.parent = stageParent.transform;

          stageObject[x,z].GetComponent<Stage>().stageIndexX = x;
          stageObject[x,z].GetComponent<Stage>().stageIndexZ = z;

          //ステージの端を定義
          if(x == 0 || z == 0 || x == stageSizeX - 1 || z == stageSizeZ -1)
          {
            stageObject[x,z].GetComponent<Stage>().isEdge = true;            
          }
          
        }
        
      }

      //ステージを回転させる
      stageParent.transform.rotation = Quaternion.Euler(0, 30, 0);

      //ステージの中心を探す
      Vector3 stageCenter = (stageObject[0,0].transform.position + stageObject[(stageSizeX-1),(stageSizeZ-1)].transform.position)/2;
      
      //ステージのサイズに合わせてカメラを動かす
      cam.transform.position = stageCenter + new Vector3(0, 0, 1.8f);

      //プレファブを非アクティブにする
      hexagon.transform.root.gameObject.SetActive (false);

      //周の数
      ringsCount = (int)((stageSizeX+1)/2);

      //エッジからの距離を計算
      for(int r=0; r<ringsCount; r++){

        for(int z=0; z<stageSizeZ-(2*r); z++) //zの大きさぶんループ
        {
          stageObject[r, r+z].GetComponent<Stage>().distanceFromEdge = r;
          stageObject[stageSizeZ-r-1, r+z].GetComponent<Stage>().distanceFromEdge = r;

          for(int x=0; x<stageSizeX-(2*r); x++)//xの大きさぶんループ
          {
            stageObject[r+x, r].GetComponent<Stage>().distanceFromEdge = r;
            stageObject[r+x, stageSizeX-r-1].GetComponent<Stage>().distanceFromEdge = r;
          }
        }
      }
    }

    // Update is called once per frame
    void Update()
    {
    }
}

