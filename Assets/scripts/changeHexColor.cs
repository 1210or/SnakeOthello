using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeHexColor : MonoBehaviour
{
 
  
  public static bool arroundScanFlag = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      //周囲の色を判定するフラグ
      int arroundHexColorFlag = 1;
      
      //プレイヤーが新しいマスに入ったタイミングで行う
      if(arroundScanFlag == true){
        
        for(int i = 0; i < StageHexagon.arroundHexagons(this.gameObject).Length; i++ ){
          if(StageHexagon.arroundHexagons(this.gameObject)[i].GetComponent<Renderer>().material.color != Player.instance.playerColor)
          {
            //周囲6個が一つでも塗られていなかったら0にする
            arroundHexColorFlag = arroundHexColorFlag * 0; 
          }
        }

        //6個全部がその色だったら真ん中も塗る
        if(arroundHexColorFlag == 1){
          this.GetComponent<Renderer>().material.color = Player.instance.playerColor;
        }

        //全部のマスの処理完了をカウント
        StageMaker.stageProcessFlag = StageMaker.stageProcessFlag + 1;
      }
    }

    void OnMouseDown() //クリックされると起動
  {
      //print("click " + this.name); //オブジェクトの名前をプリント

      //ランダムにマテリアルカラーを変える
      this.GetComponent<Renderer>().material.color = Player.instance.playerColor;
      //print();
      //周囲の色を変える(ボム)
      for(int i = 0; i < StageHexagon.arroundHexagons(this.gameObject).Length; i++ ){

        //arroundHexagons関数を使ってthis.gameObjectの隣接するヘキサゴンを取得
        //StageHexagon.arroundHexagons(this.gameObject)[i].GetComponent<Renderer>().material.color = Player.playerColor;
      
 
  }
  


}}
