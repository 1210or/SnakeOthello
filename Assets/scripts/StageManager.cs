using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics; 
using Photon.Pun;
using Photon.Realtime;



public class StageManager : MonoBehaviour
{
  public GameObject hexagon; //オブジェクトの元になるプレファブを格納
  public GameObject cam; //カメラを格納

  [SerializeField, Range(1, 100)]//スライダ
  public static int stageSizeX = 10; //x方向のステージの大きさ

  public static int stageSizeZ = stageSizeX; //z方向のステージの大きさ
  
  public float[,] stagePosition = new float[stageSizeX,stageSizeZ]; //座標のための配列、ステージの大きさを決めて配列のサイズにする
  public GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];//ゲームオブジェクトが入っている配列
  
  //ステージのx方向の大きさはcos30の2倍
  public  static float hexSizeX = MathF.Cos(MathF.PI/6);
  public  static float hexSizeZ = 0.75f;

  public static int stageProcessFlag = 0;
  
  public GameObject stageParent;

    //ステージの周の数
   public int ringsCount;

   //ステージ端から近い順にオブジェクト
   public List<GameObject> stageObjectFromEdge;

    public static StageManager instance;
    
    // Start is called before the first frame update
    void Awake()
    {    
      if (PhotonNetwork.IsMasterClient) {
      //プレファブを並べる 
      for(int z=0;z<stageSizeZ;z++) //zの大きさぶんループ
      {
        for(int x=0;x<stageSizeX;x++)//xの大きさぶんループ
        {      
           // "RoomObject"プレハブからルームオブジェクトを生成する
          stageObject[x,z] = PhotonNetwork.InstantiateRoomObject("stageHexa", new Vector3(hexSizeX *  x + z * 0.5f * hexSizeX, 0, z * hexSizeZ ) , new Quaternion(1,0,0,-1));

          //インデックス情報を付与
          stageObject[x,z].GetComponent<Stage>().stageIndexX = x;
          stageObject[x,z].GetComponent<Stage>().stageIndexZ = z;          
        }
        
      } 

        //周の数
        ringsCount = (int)((StageManager.stageSizeX+1)/2);      
      }

      //インスタンス化
      if(instance == null)
      {
          instance = this;
      }
    }

    // Update is called once per frame
    void Update()
    {
      //プレイヤーの人数行う GameManager.instance.players.Length
      for(int p=0; p<2; p++){
        SrroundAndFill(p);
      }      
    }

    //リセットボタンから呼び出し
    public void ResetAllPowerValue()
    {
      for(int i=0; i<stageObjectFromEdge.Count; i++){        
        stageObjectFromEdge[i].GetComponent<Stage>().stagePowerValue = 0;
      } 
    }



//関数

    //まとめた
    public void SrroundAndFill(int p)
    {
      //ステージ走査値を初期値にする
      ResetStageScanValue(p);  
        
      //外側から順番に処理
      for(int i=0; i<stageObjectFromEdge.Count; i++){
        if(stageObjectFromEdge[i].GetComponent<Stage>().isEdge == true){
          //走査値を設定する
          FillColorScan(stageObjectFromEdge[i], 1-(2*p), p);
        }
      } 

      //既に塗られているところの走査値を0にする//今のところなくてもいいがこの操作によって新規で囲まれた部分だけをぬれるための案
      RemoveScanDeplication(p);

      //プレイヤーのいるマスの走査値を0にする
      ResetOnPlayerHexa(p);

      //塗り潰す
      SetScanToPowerValue(p);

    }
    public void ResetStageScanValue(int p)
    {
      //ステージ走査値を初期値にする
        for(int i=0; i<stageObjectFromEdge.Count; i++){          
            stageObjectFromEdge[i].GetComponent<Stage>().stageScanFlag[p]=1-(2*p); //                                                  
        }  
    }

    public void RemoveScanDeplication(int p)
    {
      for(int i=0; i<stageObjectFromEdge.Count; i++){    
        if(stageObjectFromEdge[i].GetComponent<Stage>().stagePowerValue == 1-(2*p)){      
          stageObjectFromEdge[i].GetComponent<Stage>().stageScanFlag[p] = 0;                                               
        }  
      }
    }

    //囲んで塗る
    public void FillColorScan(GameObject stageHexa, int stagePowerValue_, int stageScanFlagIndex){


      GameObject currentScanStage = stageHexa;
      GameObject nextScanStage = stageHexa;

      //スキャンしてきた順番でリストに入れる
      List<GameObject> scanStageList = new List<GameObject>();
      scanStageList.Add(stageHexa);

      //自分が赤じゃない(赤の場合)         //走査値が1である(赤の場合)
      if(stageHexa.GetComponent<Stage>().stagePowerValue != stagePowerValue_ && stageHexa.GetComponent<Stage>().stageScanFlag[stageScanFlagIndex] == stagePowerValue_){ 
        //print("Start Hexa: " + stageHexa.name);        

        // new によりStopwatch のインスタンスを生成する方法
        var sw = new Stopwatch();
        sw.Start();

        //無限ループ防止
        while (sw.ElapsedMilliseconds < 1000)
        {
          //print(sw.ElapsedMilliseconds + "ms");
          
          currentScanStage = nextScanStage;
          //print(currentScanStage.name + "に移動した");
          
          
          //走査値を0にする
          //一番外側であれば //自分が赤じゃない(赤の場合)
          if(currentScanStage.GetComponent<Stage>().distanceFromEdge == 0 && stageHexa.GetComponent<Stage>().stagePowerValue != stagePowerValue_){
            //ステージ走査値を0にする                  
            //print(currentScanStage.name + "の走査値を0にした");
            
            currentScanStage.GetComponent<Stage>().stageScanFlag[stageScanFlagIndex] = 0; 

          }else{//外側でないなら周囲に0があれば0にする

            //周囲6回
            for(int i = 0; i < Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ).Count; i++ ){

              //そのマスの走査値が0であれば
              if(Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Stage>().stageScanFlag[stageScanFlagIndex] == 0){
                      
                //print(currentScanStage.name + "の走査値を0にした");
                //ステージ走査値を0にする
                currentScanStage.GetComponent<Stage>().stageScanFlag[stageScanFlagIndex] = 0;
                break; //なくてもいい、無駄な処理を繰り返さないため

              }
            }
          }
            
          //print("この後" + currentScanStage.name + "の周囲" + Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ).Count + "マスを調べて進みます。進めなかったら戻ります。");                   

          //周囲6回
          for(int i = 0; i < Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ).Count; i++ ){
            //print("point1");
            //次のマスを探す作業
                
            //周囲を調べるときに長いから変数に入れとく
            GameObject tempAroundStage = Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ)[i];
            int tempScanFlag = Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Stage>().stageScanFlag[stageScanFlagIndex];  
            int tempPowerValue =Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ)[i].GetComponent<Stage>().stagePowerValue;
                  
            //print(i + "番目" + tempAroundStage.name + ", 走査値: " + tempScanFlag + "==" + stagePowerValue_ + ", パワー値: " + tempPowerValue + "!=" + stagePowerValue_); 
            //そのマスの走査値が1であり  //そのマスが自分の色じゃない場合
            if((tempScanFlag == stagePowerValue_)&&(tempPowerValue != stagePowerValue_)){            
                      
              //print(tempAroundStage.name + "に進む");
              //次の処理マスを返す     
              nextScanStage = tempAroundStage;

              //リストに追加
              scanStageList.Add(nextScanStage);
              //print("break1");
              break;

              }else{                      
              
              //次のマスが見つかったらbreakしてるので入らない
                      
              //もし6個全部から進む先がなかったら
              if(i == Stage.arroundHexagons(currentScanStage, currentScanStage.GetComponent<Stage>().stageIndexX, currentScanStage.GetComponent<Stage>().stageIndexZ).Count-1){
              //print("進めなかった");
                
                //リストが負の数にならないようにする
                if((scanStageList.Count-2) >= 0){
                  nextScanStage = scanStageList[scanStageList.Count-2];
                }else{
                  nextScanStage = stageHexa;
                }            
                //print(scanStageList[scanStageList.Count-1].name + "をリストから削除");
                
                //リストが負の数にならないようにする
                if((scanStageList.Count-1) >= 0){
                  scanStageList.RemoveAt(scanStageList.Count-1); 
                }

                //print(nextScanStage.name + "に戻る");
                    
                }
              }
              //print("point2");                  
            } 

            if(nextScanStage == stageHexa){
              //print("先頭まで戻った!");
              break;
            }
            //print("今のリストの長さは" + scanStageList.Count);

        }
      }
    }
    //フィル関数終わり

    //プレイヤーがいるマスの走査値を0に戻す
    public void ResetOnPlayerHexa(int p)
    {
      GameObject[] tempPlayerList = GameObject.FindGameObjectsWithTag("Player");

        for(int i=0; i<tempPlayerList.Length; i++){
          // 現在の位置から下(0,-1,0)に向かってRayをセット
          Ray ray = new Ray(tempPlayerList[i].transform.position + new Vector3(0,0.1f,0),Vector3.down);
          // Rayが当たった相手を保存する変数
          RaycastHit hit;
          // Rayを10.0fの距離まで発射。何かに当たればhitで受け取る
          if(Physics.Raycast(ray, out hit, 10.0f)) {
            // もし当たった相手のタグがstageなら下の足場の色を変える
            if(hit.collider.tag == "stage") {
              hit.collider.gameObject.GetComponent<Stage>().stageScanFlag[p] = 0;
            }
          }
        }
    }

    //スキャン値からパワー値を設定
    public void SetScanToPowerValue(int p)
    {
      for(int i=0; i<stageObjectFromEdge.Count; i++){
        if(stageObjectFromEdge[i].GetComponent<Stage>().stageScanFlag[p] == 1-(2*p)){ //走査値が初期値のままだったら             
          stageObjectFromEdge[i].GetComponent<Stage>().stagePowerValue = 1-(2*p);
        }         
      }
    }
}


