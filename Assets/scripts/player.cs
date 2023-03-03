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

    public GameObject playerCamera;

    public float walkSpeed = 3;
    public float speedBuff = 1; //今のところ未使用
    private float constSpeedBuff = 2;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public int playerID;
    public int enemyPlayerID;
    // Start is called before the first frame update
    void Start()
    {
        playerID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        enemyPlayerID = 1 - PhotonNetwork.LocalPlayer.ActorNumber;

        StartCoroutine(WaitIsStart());


        if(photonView.IsMine)
        {
            playerCamera = GameObject.Find ("MainCamera");
        }
    }

    public List<GameObject> fillStageList;
    // Update is called once per frame
    void Update()
    {
        MoveControler();

        //自分の下のマスのステージパワー値を変更、現在自分がいるマスも取得→currentPlayerHexa
        CheckMyFeet();        

        ScanAndFill();
        
        //ステージから落ちた時の処理
        DropPlayer(-2); //引数はy座標、それ以下になったら落ちた判定
    }

    public void MoveControler()
    {
        if(photonView.IsMine){
            
            int numOfArroundPlayerColor = CountArroundColor(currentPlayerHexa, playerPowerValue);
            if(numOfArroundPlayerColor == 6)
            {
                speedBuff = constSpeedBuff;
            }else
            {
                speedBuff = 1;
            }
            //x軸方向の入力を保存
            Vector3 inputKey;  
            
            // WASD入力から、XZ平面(水平な地面)を移動する方向(velocity)を得ます
            inputKey = Vector3.zero;
            
            if(Input.GetKey(KeyCode.W))
                inputKey.z += 1;
            if(Input.GetKey(KeyCode.A))
                inputKey.x -= 1;
            if(Input.GetKey(KeyCode.S))
                inputKey.z -= 1;
            if(Input.GetKey(KeyCode.D))
                inputKey.x += 1;

            //移動の向きなど座標関連はVector3で扱う
            Vector3 velocity = new Vector3(inputKey.x, 0, inputKey.z).normalized;

            //45度を30度に変換
            float convertRot = ((1-Mathf.Tan(Mathf.PI/6))/2) * (Mathf.Cos(4 * (Mathf.Atan2(velocity.z, velocity.x)))+1) + Mathf.Tan(Mathf.PI/6);

            //ベクトルの向きを取得 //zに角度変換をかける
            Vector3 direction = new Vector3(inputKey.x, 0, inputKey.z * convertRot).normalized;
           
            //移動距離を計算
            float distance = walkSpeed * Time.deltaTime * speedBuff;
            //移動先を計算
            Vector3 destination = transform.position + direction * distance;
            
            //移動先に向けて回転
            transform.LookAt(destination);
            
            //プレイ中なら  移動先の座標を設定
            if(GameManager.instance.isPlaying == true || GameManager.instance.isDebug == true)
            {
                transform.position = destination;
                
                //カメラ移動
                playerCamera.transform.position = new Vector3(destination.x, 0, destination.z);
            }                        
        }  
    }

    IEnumerator WaitIsStart()
    {
        yield return new WaitUntil(() => GameManager.instance.isPlaying == true || GameManager.instance.isDebug == true);

        this.transform.Find("player").gameObject.GetComponent<Renderer>().material.color = playerColor;//プレイヤーの子供オブジェクトの色を変える
        this.transform.Find("MiniMapIcon_inside").gameObject.GetComponent<Renderer>().material.color = teamColor;//プレイヤーの子供オブジェクトの色を変える
        this.transform.Find("MiniMapIcon_outside").gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1);//プレイヤーの子供オブジェクトの色を変える
    }

    public void ScanAndFill()
    {
        fillStageList = new List<GameObject>();

        //現在のマスの周囲をforeach
        foreach(var currentAround in currentPlayerHexa.GetComponent<Stage>().arroundHexagons)
        {
            ScanFrom(currentAround);
        }
        //走査と塗りは分けるべき、すべて走査し終わった後に塗らないと条件が変わっちゃう

        foreach (var content in fillStageList)
        {                                                                                                  
            content.GetComponent<Stage>().stagePowerValue = this.playerPowerValue; //着色
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
        List<GameObject> localFillStageList = new List<GameObject>();

        scanStageList.Add(stageHexa);
        localFillStageList.Add(stageHexa);

        bool isScanStart = false;

        //走査を走らせる条件
        int numOfArroundEnemyColor = CountArroundColor(this.currentPlayerHexa, (-1*this.playerPowerValue));
        int numOfArroundColorBlocks = CountArroundColorBlocks(this.currentPlayerHexa, (this.playerPowerValue));

        if (numOfArroundColorBlocks > 1 || numOfArroundEnemyColor == 0)
        {
            isScanStart = true;
        }
        //走査を走らせる条件


        //開始するマスが赤じゃない&&走査前である(プレイヤーが赤の場合)
        if(isScanStart == true && stageHexa.GetComponent<Stage>().stagePowerValue != playerStagePowerValue && stageHexa.GetComponent<Stage>().isScaned[playerID] == false)
        { 
            // new によりStopwatch のインスタンスを生成する方法
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < 5000)//無限ループ防止
            { 
                currentScanStage = nextScanStage;
      
                //走査マスが一番外側かつスタートマスが赤じゃない(赤の場合)
                if(currentScanStage.GetComponent<Stage>().distanceFromEdge == 0 && stageHexa.GetComponent<Stage>().stagePowerValue != playerStagePowerValue)
                {//端にたどり着いた時の処理内容                        
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
                        //次の処理マスを返す     
                        nextScanStage = OneOfArroundHexa;

                        //リストに追加
                        scanStageList.Add(nextScanStage);
                        localFillStageList.Add(nextScanStage); //スキャンは戻る時に消すので一緒にするとうまく塗れない

                        //現在のマスのisScanedを変更
                        currentScanStage.GetComponent<Stage>().isScaned[playerID] = true;

                        break;

                    }else
                    {//次のマスが見つかったらbreakしてるので入らない                 
                        //もし6個全部から進む先がなかったら
                        if(i == currentScanStage.GetComponent<Stage>().arroundHexagons.Count-1)
                        {
                            if((scanStageList.Count-2) >= 0)//リストが負の数にならないようにする
                            {
                                scanStageList.RemoveAt(scanStageList.Count-1);  //一番後ろを削除、上に持ってってもいいのでは？

                                nextScanStage = scanStageList[scanStageList.Count-1]; //スキャンリストの一番後ろの一つ手前を次のマスにする。一番後ろは行き止まりだったため。                                                        
                            }else //リストが負の数になったら最初に戻ってる   
                            {
                                nextScanStage = stageHexa;                          
                            }  

                            //現在のマスのisScanedを変更
                            currentScanStage.GetComponent<Stage>().isScaned[playerID] = true; 
                        }
                    }  
                } 

                if(nextScanStage == stageHexa)//最初のマスまで戻ってきた時
                {
                    
                    bool isSrounded =false;
                    
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
                            //次の処理マスを返す     
                            nextScanStage = OneOfArroundHexa;

                            //リストに追加
                            scanStageList.Add(nextScanStage);                            

                            //現在のマスのisScanedを変更
                            currentScanStage.GetComponent<Stage>().isScaned[playerID] = true;

                            break;

                        }else
                        {
                            print("10");
                            //もし6個全部から進む先がなかったら
                            if(i == currentScanStage.GetComponent<Stage>().arroundHexagons.Count-1)
                            {
                                isSrounded = true;
                            }
                        }
                    }    
                    
                    //着色--切り分けたい
                    if(isSrounded == true)
                    {
                        foreach (var content in localFillStageList)
                        {                                
                            fillStageList.Add(content);                            
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

    //周囲のマスの色を数える
    private int CountArroundColor(GameObject stageHexa, int playerPowerValue)
    {
        int numOfArroundColor = 0;

        if(stageHexa == null)
        {
            return -1;
        }

        for (int i = 0; i < stageHexa.GetComponent<Stage>().arroundHexagons.Count; i++)
        {
            GameObject currentHexa = stageHexa.GetComponent<Stage>().arroundHexagons[i];
            if(currentHexa.GetComponent<Stage>().stagePowerValue == playerPowerValue)
            {
                numOfArroundColor += 1;
            }
        }


        return numOfArroundColor;
    }

    private int CountArroundColorBlocks(GameObject stageHexa, int separatePowerValue)
    {
        int numOfArroundColorBlocks = 0;
        for (int i = 0; i < stageHexa.GetComponent<Stage>().arroundHexagons.Count; i++)
        {
            GameObject currentHexa = stageHexa.GetComponent<Stage>().arroundHexagons[i];
            GameObject lastHexa;

            //周囲を走査で一つ前のマスを入れる
            try
            {
                lastHexa = stageHexa.GetComponent<Stage>().arroundHexagons[i-1];
            }catch 
            { 
                lastHexa = null;
            }


            if (currentHexa.GetComponent<Stage>().stagePowerValue != separatePowerValue && (lastHexa == null || lastHexa.GetComponent<Stage>().stagePowerValue == separatePowerValue))
            {
                numOfArroundColorBlocks += 1;
            }
        }
        print(numOfArroundColorBlocks);
        return numOfArroundColorBlocks;
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

