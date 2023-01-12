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
public int distanceFromEdge = 0;

public bool isPlayerOn = false;

//どちら陣営か判定 +1はプレイヤー1、-1はプレイヤー2
public int stagePowerValue = 0; 

public int[] stageScanFlag = new int[]{1,-1};


    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Renderer>().material.color = new Color ((float)distanceFromEdge/(float)StageMaker.ringsCount, (float)distanceFromEdge/(float)StageMaker.ringsCount, (float)distanceFromEdge/(float)StageMaker.ringsCount);
    }

    // Update is called once per frame
    void Update()
    {
      //ステージのパワー値が変わったら色を変更する
      valueToColor(this.stagePowerValue, GameManager.instance.players[0].GetComponent<Player>().paintColor, GameManager.instance.players[1].GetComponent<Player>().paintColor);

      

      //周囲6マスに合わせて色を塗る
      fillColor1Hexa();

      fillColor(this.gameObject, 1);
    }

    void OnMouseDown() //クリックされると起動
    {
      colorBomb(this.gameObject, new Color(0, 0, 0), 0);
    }

      //引数に入れたゲームオブジェクト(ステージ)の接するステージを配列にして返す
    public static List<GameObject> arroundHexagons (GameObject hexagon, int x, int z)
    {
            //リストに周囲のステージを格納
            List<GameObject> arroundHexagons = new List<GameObject>();
            
            //左下から反時計回り
            try{arroundHexagons.Add(StageMaker.stageObject[x,z-1]);}catch (System.Exception){/*何もしない*/} //左下
            try{arroundHexagons.Add(StageMaker.stageObject[x+1,z-1]);}catch (System.Exception){/*何もしない*/} //下
            try{arroundHexagons.Add(StageMaker.stageObject[x+1,z]);}catch (System.Exception){/*何もしない*/} //右下
            try{arroundHexagons.Add(StageMaker.stageObject[x,z+1]);}catch (System.Exception){/*何もしない*/} //右上
            try{arroundHexagons.Add(StageMaker.stageObject[x-1,z+1]);}catch (System.Exception){/*何もしない*/} //上
            try{arroundHexagons.Add(StageMaker.stageObject[x-1,z]);}catch (System.Exception){/*何もしない*/} //左上

          return arroundHexagons;
    }

    //ステージパワー値によって色を変える
    public void valueToColor(int stagePowerValue, Color color1, Color color2){
      switch (stagePowerValue)
      {
        case 1:
          this.GetComponent<Renderer>().material.color = color1;
          break;

        case -1:
          this.GetComponent<Renderer>().material.color = color2;
          break;

        default:
          //this.GetComponent<Renderer>().material.color = color3;
          break;

      }
    }

    //囲んで塗る
    public static void fillColor(GameObject stageHexa, int stagePowerValue_){ //stagePowerValue_ = 1

      stageHexa.GetComponent<Stage>().stageScanFlag[0] = stagePowerValue_;
      
      //自分が赤じゃない
      if(stageHexa.GetComponent<Stage>().stagePowerValue != stagePowerValue_){
      
        //エッジである
        if(stageHexa.GetComponent<Stage>().isEdge == true)
        {
          stageHexa.GetComponent<Stage>().stageScanFlag[0] = 0;           
        }
      
        //エッジと接している
        for(int i = 0; i < arroundHexagons(stageHexa.gameObject, stageHexa.GetComponent<Stage>().stageIndexX, stageHexa.GetComponent<Stage>().stageIndexZ).Count; i++ ){//周囲マスの数-1回ループ
            //エッジまたはエッジと触れてるマスと触れていたら
            if((arroundHexagons(stageHexa, stageHexa.GetComponent<Stage>().stageIndexX, stageHexa.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Stage>().isEdge == true) || (arroundHexagons(stageHexa, stageHexa.GetComponent<Stage>().stageIndexX, stageHexa.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Stage>().stageScanFlag[0] == 0)){            
              stageHexa.GetComponent<Stage>().stageScanFlag[0] = 0; 
              //stageHexa.GetComponent<Renderer>().material.color = new Color(0, 0, 0);

              //順番に周りから処理していかないとうまく動かない、感染の概念を考える必要がある

              break;
            }else{ //エッジと触れてない
              stageHexa.GetComponent<Stage>().stageScanFlag[0] = stagePowerValue_;
              //stageHexa.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            }                  
        }
      }else{ //自分が赤である
        stageHexa.GetComponent<Stage>().stageScanFlag[0] = stagePowerValue_;
      }

    }
      
    

    //一マス囲んで塗る
    public void fillColor1Hexa(){
      //自分の周りのマスが全て同じカラーだったら自分の色を変更する
        if(arroundHexagons(this.gameObject, stageIndexX, stageIndexZ).Count == 6){

          for(int i = 0; i < arroundHexagons(this.gameObject, stageIndexX, stageIndexZ).Count - 1; i++ ){//周囲マスの数-1回ループ

            //自分と周囲マス左下が違う色かつ、周囲マスが反時計回りに一つ隣と同じ色である
            if(
                (this.stagePowerValue != (arroundHexagons(this.gameObject, stageIndexX, stageIndexZ)[0].GetComponent<Stage>().stagePowerValue))               
                & (arroundHexagons(this.gameObject, stageIndexX, stageIndexZ)[i].GetComponent<Stage>().stagePowerValue) == (arroundHexagons(this.gameObject, stageIndexX, stageIndexZ)[i+1].GetComponent<Stage>().stagePowerValue)
              ){            
                if(i == (arroundHexagons(this.gameObject, stageIndexX, stageIndexZ).Count - 2)){ //ループの一番最後の処理

                  //ステージパワー値を変更
                  this.stagePowerValue = arroundHexagons(this.gameObject, stageIndexX, stageIndexZ)[0].GetComponent<Stage>().stagePowerValue;
                  
                }
              }else{
                break;
              }
          }
        }

    }

    
    public void colorBomb(GameObject stageHexa, Color color, int stagePowerValue){
      //自分の色を変える
      stageHexa.GetComponent<Stage>().stagePowerValue = stagePowerValue;
      stageHexa.GetComponent<Renderer>().material.color = color;
      
      //周囲の色を変える
      for(int i = 0; i < arroundHexagons(stageHexa.gameObject, stageHexa.GetComponent<Stage>().stageIndexX, stageHexa.GetComponent<Stage>().stageIndexZ).Count; i++ ){

        //arroundHexagons関数を使ってthis.gameObjectの隣接するヘキサゴンを取得
        arroundHexagons(stageHexa.gameObject, stageHexa.GetComponent<Stage>().stageIndexX, stageHexa.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Stage>().stagePowerValue = stagePowerValue;
        arroundHexagons(stageHexa.gameObject, stageHexa.GetComponent<Stage>().stageIndexX, stageHexa.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Renderer>().material.color = color;

      }

    }
}
