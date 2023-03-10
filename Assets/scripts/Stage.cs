using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System;

public class Stage : MonoBehaviourPunCallbacks, IPunObservable
{
  //マスの形状
  public GameObject hexagon;

  public int stageIndexX=-1;
  public int stageIndexZ=-1;

  public bool isEdge =false;
  public int distanceFromEdge = 0;

  public bool isPlayerOn = false;

  public Color defaultStageColor = new Color(0.9f, 0.9f, 0.9f);

  //どちら陣営か判定 +1はプレイヤー1、-1はプレイヤー2
  public int stagePowerValue = 0; 

  public int[] stageScanFlag = new int[]{1,-1};
  public bool[] isScaned = new bool[]{false,false};

  public float minimapAlpha = 0.8f;

  public List<GameObject> arroundHexagons;


    // Start is called before the first frame update
    void Start()
    {        
      int x = this.GetComponent<Stage>().stageIndexX;
      int z = this.GetComponent<Stage>().stageIndexZ;
      
      //オブジェクトの名前変更
      this.name = "hexagon" + x + "_" + z;

      //親設定
      this.transform.parent = GameObject.Find ("StageHexagons").transform;
      
      //ステージの端を定義
      if(x == 0 || z == 0 || x == GameManager.stageSizeX - 1 || z == GameManager.stageSizeZ -1)
      {
        this.GetComponent<Stage>().isEdge = true;            
      }

      this.GetComponent<Renderer>().material.color = defaultStageColor;

      //配列に入るコルーチン
      StartCoroutine(EnterStageObjectArray());
      
      //ステージメンバ変数に周囲のマスを格納(全てのマスが配列に入ったら実行)
      StartCoroutine(SetArround());

    }

    // Update is called once per frame
    void Update()
    {      
      if(GameManager.instance.isPlaying == true || GameManager.instance.isDebug == true){        
        if(this.stagePowerValue != 0){ 
          //ステージのパワー値が変わったら色を変更する
          this.GetComponent<Renderer>().material.color = GameManager.instance.playerArray[(int)(-0.5f*(this.stagePowerValue-1))].GetComponent<Player>().paintColor;
          this.transform.Find("minimap_icon_inside").gameObject.GetComponent<Renderer>().material.color = GameManager.instance.playerArray[(int)(-0.5f * (this.stagePowerValue - 1))].GetComponent<Player>().teamColor - new Color(0, 0, 0, 1 - minimapAlpha);         
        }else
        {
          this.GetComponent<Renderer>().material.color = defaultStageColor; //デフォルトの状態
                this.transform.Find("minimap_icon_inside").gameObject.GetComponent<Renderer>().material.color = defaultStageColor - new Color(0, 0, 0, 1 - minimapAlpha);
        }     
      }            
    }

      //引数に入れたゲームオブジェクト(ステージ)の接するステージを配列にして返す
    public static List<GameObject> ArroundHexagons (GameObject hexagon)
    {
      //リストに周囲のステージを格納
      List<GameObject> arroundHexagons = new List<GameObject>(); 

      int x = hexagon.GetComponent<Stage>().stageIndexX;
      int z = hexagon.GetComponent<Stage>().stageIndexZ;
      
      //左下から反時計回り
      try{arroundHexagons.Add(GameManager.instance.stageObject[x,z-1]   );}  catch (System.Exception){/*何もしない*/} //左下
      try{arroundHexagons.Add(GameManager.instance.stageObject[x+1,z-1] );}  catch (System.Exception){/*何もしない*/} //下
      try{arroundHexagons.Add(GameManager.instance.stageObject[x+1,z]   );}  catch (System.Exception){/*何もしない*/} //右下
      try{arroundHexagons.Add(GameManager.instance.stageObject[x,z+1]   );}  catch (System.Exception){/*何もしない*/} //右上
      try{arroundHexagons.Add(GameManager.instance.stageObject[x-1,z+1] );}  catch (System.Exception){/*何もしない*/} //上
      try{arroundHexagons.Add(GameManager.instance.stageObject[x-1,z]   );}  catch (System.Exception){/*何もしない*/} //左上
      
      return arroundHexagons;
    }

    private void OnMouseDown() {
      print("This is " + this.name);
      this.arroundHexagons.ForEach(name => print("Around is : " + name));
    }

    
    public void colorBomb(GameObject stageHexa, Color color, int stagePowerValue){
      //自分の色を変える
      stageHexa.GetComponent<Stage>().stagePowerValue = stagePowerValue;
      stageHexa.GetComponent<Renderer>().material.color = color;
      
      //周囲の色を変える
      for(int i = 0; i < ArroundHexagons(stageHexa.gameObject).Count; i++ ){

        //ArroundHexagons関数を使ってthis.gameObjectの隣接するヘキサゴンを取得
        ArroundHexagons(stageHexa.gameObject)[i].GetComponent<Stage>().stagePowerValue = stagePowerValue;
        ArroundHexagons(stageHexa.gameObject)[i].GetComponent<Renderer>().material.color = color;
      }
    }
    IEnumerator EnterStageObjectArray()
    {  
      //ゲームマネージャーでマスターでインデックス付与されたあとphotonViewで同期されるのでそれ待ち
      yield return new WaitForSeconds (1.0f);//この1秒がWaitNullCheckに影響している
      //yield return new WaitUntil(() => this.GetComponent<Stage>().stageIndexX != -1); //なぜかこれだとダメ
      
      int x = this.GetComponent<Stage>().stageIndexX;
      int z = this.GetComponent<Stage>().stageIndexZ;

      GameManager.instance.stageObject[x, z] = this.gameObject;
    }

    IEnumerator SetArround()
    {
      //ステージアレイが全て格納されたら
      yield return new WaitUntil(() => GameManager.instance.isStageArrayOK == true);

      //周囲のマスを保持しておく
      arroundHexagons = ArroundHexagons(this.gameObject);
    }

//名前空間、継承、コンポーネントアタッチ必須
    void IPunObservable.OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo i_info )
    {
        if(stream.IsWriting )
        {
            //データの送信
            //ステージパワー値
            stream.SendNext((int)this.GetComponent<Stage>().stagePowerValue);  

            //インデックス
            stream.SendNext((int)this.GetComponent<Stage>().stageIndexX);
            stream.SendNext((int)this.GetComponent<Stage>().stageIndexZ);  

            stream.SendNext((int)this.GetComponent<Stage>().distanceFromEdge);
        }
        else
        {            
            //データの受信
            this.GetComponent<Stage>().stagePowerValue = (int)stream.ReceiveNext();  
            
            this.GetComponent<Stage>().stageIndexX = (int)stream.ReceiveNext();  
            this.GetComponent<Stage>().stageIndexZ = (int)stream.ReceiveNext();

            this.GetComponent<Stage>().distanceFromEdge = (int)stream.ReceiveNext();
        }
    }
}
