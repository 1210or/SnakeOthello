using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Diagnostics; 
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{    
    //プレイヤーの属性(チーム)のための変数
    public int playerPowerValue = 0;

    //チームカラー
    public Color teamColor;

    //プレイヤーカラー
    public Color playerColor;

    //塗る色
    public Color paintColor;

    //初期位置
    public Vector3 firstPosition;

    //直前にいたマスを保存するための変数
    public GameObject solverGameObject = null;

    //現在のいるマスを取得
    public GameObject currentPlayerHexa = null;

    //新しいマスに入った瞬間のフラグ
    public bool newHexaFlag = false;

    public static Player instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public int playerID;
    // Start is called before the first frame update
    void Start()
    {
        playerID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
         
        this.transform.Find("player").gameObject.GetComponent<Renderer>().material.color = playerColor;//プレイヤーの子供オブジェクトの色を変える

        StartCoroutine(ButtonAddListener());
    }

        //インスペクタ状には出てこない
    IEnumerator ButtonAddListener()
    {       
        yield return new WaitForSeconds(1);

        //インスペクタ状には出てこない
        Button button = GameObject.Find("DebugButton").GetComponent<Button>(); //buttonコンポーネントを取得
        GameObject temp = this.gameObject;
        button.onClick.AddListener(temp.GetComponent<Player>().OnClickDebugButton);
    }

    // Update is called once per frame
    void Update()
    {
        
        //自分の下のマスのステージパワー値を変更、現在自分がいるマスも取得→currentPlayerHexa
        CheckMyFeet();
        
        
        //現在のマスの周囲をforeach
        foreach(var currentAround in currentPlayerHexa.GetComponent<Stage>().arroundHexagons)
        {
            ScanFrom(currentAround);
        }
        
        //ステージから落ちた時の処理
        DropPlayer(-2); //引数はy座標、それ以下になったら落ちた判定
    }

    public void OnClickDebugButton()
    {
        print("Debug!!");
        print(this.name + " CurrentHexa: " + currentPlayerHexa);
        
        //現在のマスの周囲をforeach
        foreach(var currentAround in this.currentPlayerHexa.GetComponent<Stage>().arroundHexagons)
        {
            print("currentAround: " + currentAround.name);
            ScanFrom(currentAround);
        }
    }

    //走査
    private void ScanFrom(GameObject stageHexa)
    {
        GameObject currentScanStage = stageHexa;
        GameObject nextScanStage = stageHexa;

        int playerStagePowerValue = this.playerPowerValue; //赤1,青-1
        int stageScanIndex = this.playerID;

        //全てのマスのisScanedをfalseに戻す
        ResetIsScaned();
    
        //スキャンしてきた順番でリストに入れる
        List<GameObject> scanStageList = new List<GameObject>();
        List<GameObject> fillStageList = new List<GameObject>();

        scanStageList.Add(stageHexa);
        fillStageList.Add(stageHexa);

        print("0");
        //開始するマスが赤じゃない&&走査前である(プレイヤーが赤の場合)
        if(stageHexa.GetComponent<Stage>().stagePowerValue != playerStagePowerValue && stageHexa.GetComponent<Stage>().isScaned[playerID] == false)
        { 
            print("1");
            // new によりStopwatch のインスタンスを生成する方法
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < 5000)//無限ループ防止
            { 
                currentScanStage = nextScanStage;
      
                //走査マスが一番外側かつスタートマスが赤じゃない(赤の場合)
                if(currentScanStage.GetComponent<Stage>().distanceFromEdge == 0 && stageHexa.GetComponent<Stage>().stagePowerValue != playerStagePowerValue)
                {//端にたどり着いた時の処理内容 
                    print("2");                         
                    break;
                }
                
                //次を探す-------------------------------------
                //周囲6回
                for(int i = 0; i < currentScanStage.GetComponent<Stage>().arroundHexagons.Count; i++ )
                {
                    //次のマスを探す作業
                        
                    //周囲を調べるときに長いから変数に入れとく
                    GameObject OneOfArroundHexa = currentScanStage.GetComponent<Stage>().arroundHexagons[i];
                    bool tempIsScaned = OneOfArroundHexa.GetComponent<Stage>().isScaned[playerID];  
                    int tempPowerValue = OneOfArroundHexa.GetComponent<Stage>().stagePowerValue;
                        
                    //そのマスの走査前であり  //そのマスが自分の色じゃない場合
                    if((tempIsScaned == false)&&(tempPowerValue != playerStagePowerValue))
                    {    
                        print("3");                         
                        //次の処理マスを返す     
                        nextScanStage = OneOfArroundHexa;

                        //リストに追加
                        scanStageList.Add(nextScanStage);
                        fillStageList.Add(nextScanStage); //スキャンは戻る時に消すので一緒にするとうまく塗れない

                        //現在のマスのisScanedを変更
                        currentScanStage.GetComponent<Stage>().isScaned[playerID] = true;

                        print("Next to " + nextScanStage);
                        break;

                    }else
                    {//次のマスが見つかったらbreakしてるので入らない
                    print("4");                     
                        //もし6個全部から進む先がなかったら
                        if(i == currentScanStage.GetComponent<Stage>().arroundHexagons.Count-1)
                        {
                            print("5");
                            if((scanStageList.Count-2) >= 0)//リストが負の数にならないようにする
                            {
                                print("6");
                                scanStageList.RemoveAt(scanStageList.Count-1);  //一番後ろを削除、上に持ってってもいいのでは？

                                nextScanStage = scanStageList[scanStageList.Count-1]; //スキャンリストの一番後ろの一つ手前を次のマスにする。一番後ろは行き止まりだったため。                                                        
                            }else //リストが負の数になったら最初に戻ってる   
                            {
                                print("7");
                                nextScanStage = stageHexa;                          
                            }
                            print("Back to " + nextScanStage);  

                            //現在のマスのisScanedを変更
                            currentScanStage.GetComponent<Stage>().isScaned[playerID] = true; 
                        }
                    }  
                } 

                if(nextScanStage == stageHexa)//最初のマスまで戻ってきた時
                {
                    
                    bool isSrounded =false;
                    
                    print("8");
                    //周囲6回
                    for(int i = 0; i < nextScanStage.GetComponent<Stage>().arroundHexagons.Count; i++ )
                    {
                        //次のマスを探す作業
                            
                        //周囲を調べるときに長いから変数に入れとく
                        GameObject OneOfArroundHexa = nextScanStage.GetComponent<Stage>().arroundHexagons[i];
                        bool tempIsScaned = OneOfArroundHexa.GetComponent<Stage>().isScaned[playerID];  
                        int tempPowerValue = OneOfArroundHexa.GetComponent<Stage>().stagePowerValue;                        
                            
                        //そのマスの走査前であり  //そのマスが自分の色じゃない場合
                        if((tempIsScaned == false)&&(tempPowerValue != playerStagePowerValue))
                        {    
                            print("9");                         
                            //次の処理マスを返す     
                            nextScanStage = OneOfArroundHexa;

                            //リストに追加
                            scanStageList.Add(nextScanStage);                            

                            //現在のマスのisScanedを変更
                            currentScanStage.GetComponent<Stage>().isScaned[playerID] = true;

                            print("Next to " + nextScanStage);
                            break;

                        }else
                        {
                            print("10");
                            //もし6個全部から進む先がなかったら
                            if(i == currentScanStage.GetComponent<Stage>().arroundHexagons.Count-1)
                            {
                                print("11");
                                isSrounded = true;
                            }
                        }
                    }    

                    if(isSrounded == true)
                    {
                        print("12");
                        foreach (var content in fillStageList)
                        {                                
                            print(content.name);
                            content.GetComponent<Stage>().stagePowerValue = playerStagePowerValue; //着色
                        }
                        break;
                    }
                    
                }

                //次を探す、終了-------------------------------------
            }
        }
    }

    //ステージ走査値を初期値にする
    private void ResetIsScaned()
    {
        for(int i=0; i<StageManager.instance.stageObjectFromEdge.Count; i++)
        {          
          StageManager.instance.stageObjectFromEdge[i].GetComponent<Stage>().isScaned[playerID] = false;                                                  
        }  
    }

    //自分の下のマスのステージパワー値を変更、現在自分がいるマスも取得
    private void CheckMyFeet()
    {        
        Ray ray = new Ray(transform.position + new Vector3(0,0.1f,0),Vector3.down);// 現在の位置から下(0,-1,0)に向かってRayをセット
        RaycastHit hit;// Rayが当たった相手を保存する変数
        
        if(Physics.Raycast(ray, out hit, 10.0f)) // Rayを10.0fの距離まで発射。何かに当たればhitで受け取る
        {
            if(hit.collider.tag == "stage") //もし当たった相手のタグがstageなら下の足場の色を変える
            {
                currentPlayerHexa = hit.collider.gameObject;//現在のマスを格納
                
                if(PhotonNetwork.PlayerList.Length == 2 || GameManager.instance.isDebug == true)//プレイヤーが2人ならステージパワー値を変更
                {
                    if(hit.collider.gameObject.GetComponent<Stage>().stagePowerValue != playerPowerValue)//もし足元のステージのパワー値と自分のチームパワー値が違うならば
                    {                    
                        hit.collider.gameObject.GetComponent<Stage>().stagePowerValue = playerPowerValue;//ステージパワーを変える
                    }
                }   
            }else
            {
                currentPlayerHexa = null;
            }
        }
    }


    //落下したら戻す
    private void DropPlayer(float yPosition)
    {
        if(this.transform.root.gameObject.transform.position.y < yPosition){ //この子ゲームオブジェクトのy座標がyPosition以下だったら
            //初期マスにプレイヤーをとばす
            this.transform.root.gameObject.transform.position = firstPosition + new Vector3(0, 2, 0);       
        }
    }

//名前空間、継承、コンポーネントアタッチ必須、順番通りに送受信されるので順番が大切
//同期
    void IPunObservable.OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo i_info )
    {
        if(stream.IsWriting )
        {
            //データの送信
            //プレイヤーカラー
            
            stream.SendNext(this.GetComponent<Player>().playerColor.r);
            stream.SendNext(this.GetComponent<Player>().playerColor.g);
            stream.SendNext(this.GetComponent<Player>().playerColor.b);
            //stream.SendNext(this.GetComponent<Player>().playerColor.a);

            //初期位置
            stream.SendNext(this.GetComponent<Player>().firstPosition);
            
            //パワー値
            stream.SendNext(this.GetComponent<Player>().playerPowerValue);

            
            //チームカラー
            stream.SendNext(this.GetComponent<Player>().teamColor.r);
            stream.SendNext(this.GetComponent<Player>().teamColor.g);
            stream.SendNext(this.GetComponent<Player>().teamColor.b);

            //プレイヤーカラー
            stream.SendNext(this.GetComponent<Player>().playerColor.r);
            stream.SendNext(this.GetComponent<Player>().playerColor.g);
            stream.SendNext(this.GetComponent<Player>().playerColor.b);

            //塗る色
            stream.SendNext(this.GetComponent<Player>().paintColor.r);
            stream.SendNext(this.GetComponent<Player>().paintColor.g);
            stream.SendNext(this.GetComponent<Player>().paintColor.b);
        
            //stream.SendNext(this.GetComponent<Player>().solverGameObject);

        }
        else
        {            
            //データの受信
            //プレイヤーカラー            
            float r = (float)stream.ReceiveNext();
            float g = (float)stream.ReceiveNext();
            float b = (float)stream.ReceiveNext(); 

            //初期位置
            Vector3 firstPosition = (Vector3)stream.ReceiveNext();

            //パワー値        
            int playerPowerValue = (int)stream.ReceiveNext();

            float teamColorR = (float)stream.ReceiveNext();
            float teamColorG = (float)stream.ReceiveNext();
            float teamColorB = (float)stream.ReceiveNext();

            float playerColorR = (float)stream.ReceiveNext();
            float playerColorG = (float)stream.ReceiveNext();
            float playerColorB = (float)stream.ReceiveNext();

            float paintColorR = (float)stream.ReceiveNext();
            float paintColorG = (float)stream.ReceiveNext();
            float paintColorB = (float)stream.ReceiveNext();

            //this.GetComponent<Player>().solverGameObject = (GameObject)stream.ReceiveNext();
            
            

            this.transform.Find("player").gameObject.GetComponent<Renderer>().material.color = new Color(r, g, b, 1);
            
            this.GetComponent<Player>().playerPowerValue = playerPowerValue;
            
            this.GetComponent<Player>().firstPosition = firstPosition;
            


            this.GetComponent<Player>().teamColor = new Color(teamColorR, teamColorG, teamColorB, 1);

            this.GetComponent<Player>().playerColor = new Color(playerColorR, playerColorG, playerColorB, 1);

            this.GetComponent<Player>().paintColor = new Color(paintColorR, paintColorG, paintColorB, 1);

            

        }
    }
}

