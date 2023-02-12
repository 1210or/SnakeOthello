using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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

    }

    // Update is called once per frame
    void Update()
    {      
      if(GameManager.instance.isPlaying == true || GameManager.instance.isDebug == true){        
        if(this.stagePowerValue != 0){ 
          //ステージのパワー値が変わったら色を変更する
          this.GetComponent<Renderer>().material.color = GameManager.instance.playerArray[(int)(-0.5f*(this.stagePowerValue-1))].GetComponent<Player>().paintColor;
        }else
        {
          this.GetComponent<Renderer>().material.color = defaultStageColor;
        }     
      }            
    }

      //引数に入れたゲームオブジェクト(ステージ)の接するステージを配列にして返す
    public static List<GameObject> arroundHexagons (GameObject hexagon, int x, int z)
    {
      //リストに周囲のステージを格納
      List<GameObject> arroundHexagons = new List<GameObject>();      
      
      //左下から反時計回り
      try{arroundHexagons.Add(GameManager.instance.stageObject[x,z-1]   );}  catch (System.Exception){/*何もしない*/} //左下
      try{arroundHexagons.Add(GameManager.instance.stageObject[x+1,z-1] );}  catch (System.Exception){/*何もしない*/} //下
      try{arroundHexagons.Add(GameManager.instance.stageObject[x+1,z]   );}  catch (System.Exception){/*何もしない*/} //右下
      try{arroundHexagons.Add(GameManager.instance.stageObject[x,z+1]   );}  catch (System.Exception){/*何もしない*/} //右上
      try{arroundHexagons.Add(GameManager.instance.stageObject[x-1,z+1] );}  catch (System.Exception){/*何もしない*/} //上
      try{arroundHexagons.Add(GameManager.instance.stageObject[x-1,z]   );}  catch (System.Exception){/*何もしない*/} //左上
      
      return arroundHexagons;
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
    IEnumerator EnterStageObjectArray()
    {  
      //ゲームマネージャーでマスターでインデックス付与されたあとphotonViewで同期されるのでそれ待ち
      yield return new WaitForSeconds (1.0f);
      //yield return new WaitUntil(() => this.GetComponent<Stage>().stageIndexX != -1); //なぜかこれだとダメ
      
      int x = this.GetComponent<Stage>().stageIndexX;
      int z = this.GetComponent<Stage>().stageIndexZ;

      GameManager.instance.stageObject[x, z] = this.gameObject;
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
