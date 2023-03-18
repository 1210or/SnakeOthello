using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics; 
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviourPunCallbacks
{
    public bool isDebug = true;

    //タイマーのための変数    
    private float totalTime = 120;
    int seconds;


    [SerializeField, Range(1, 100)]//スライダ
    public static int stageSizeX = 15; //x方向のステージの大きさ
    public static int stageSizeZ = 15; //z方向のステージの大きさ
    public float[,] stagePosition = new float[stageSizeX,stageSizeZ]; //座標のための配列、ステージの大きさを決めて配列のサイズにする

    [SerializeField]
    public GameObject[,] stageObject = new GameObject[stageSizeX,stageSizeZ];//ゲームオブジェクトが入っている配列
  
    //ステージのx方向の大きさはcos30の2倍
    public  static float hexSizeX = MathF.Cos(MathF.PI/6);
    public  static float hexSizeZ = 0.75f;
    
    //ステージの周の数
    public int ringsCount;

    //UI
    public GameObject finishText;
    public GameObject nextButton;
    public GameObject messageText;
    public GameObject createRoomPanel;
    public Text enterRoomName;
    public GameObject resetPowerValueButton;    

    //プレイヤー作成のための変数
    public GameObject player;
    public GameObject[] playerArray = new GameObject[2];
    //public List<GameObject> playersList = null;

    //スコアのための変数
    public int totalPowerCount = 0;
 
    public bool isFinished = false;
    public bool isPlaying = false;
    public bool isStageArrayOK = false;

    public static GameManager instance;

    // Start is called before the first frame update
    void Awake()
    {//ステージ生成
        if (PhotonNetwork.IsMasterClient)
        {//プレファブを並べる 
            for(int z=0;z<stageSizeZ;z++) //zの大きさぶんループ
            {
                for(int x=0;x<stageSizeX;x++)//xの大きさぶんループ
                {      
                    // "RoomObject"プレハブからルームオブジェクトを生成する
                    GameObject tempPhotonStageObject = PhotonNetwork.InstantiateRoomObject("Photon/stageHexa", new Vector3(hexSizeX *  x + z * 0.5f * hexSizeX, 0, z * hexSizeZ ) , new Quaternion(1,0,0,-1));
                    //インデックス情報を付与
                    tempPhotonStageObject.GetComponent<Stage>().stageIndexX = x;
                    tempPhotonStageObject.GetComponent<Stage>().stageIndexZ = z;          
                }       
            } 
        }
      
        //周の数
        ringsCount = (int)((stageSizeX+1)/2);           

        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }   
    }

    void Start()
    {   
        //配列のnullチェックコルーチンに入れてワンテンポ待たないとないと他のスクリプトのstartメソッドが走らない、
        StartCoroutine(WaitNullCheck());
               
        //ステージの配列入るの待ち
        StartCoroutine(JustBeforeGameStart());

        //ゲーム終了動作
        //コルーチン、isFinished==trueまで
        StartCoroutine(GameFinish());
    }
 
    
    // Update is called once per frame
    void Update()
    {   
        //esc終了
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //プレイ中に動作
        if(isPlaying == true && isDebug == false){   

            //時間制限と勝敗
            if(totalTime != 0){

                if(totalTime > 1){//ゲーム中

                    //ゲームタイマー                
                    totalTime -= Time.deltaTime;                
                    seconds = (int)totalTime;
                    messageText.GetComponent<Text>().text = ("Last " + seconds.ToString() + " second");

                }else{//ゲーム終了                
                    isPlaying = false;
                    isFinished = true;          
                }
            }            
        }
    }

    //コルーチンで待つ
    IEnumerator WaitTime(float seconds)
    {
        yield return new WaitForSeconds (seconds);
    }

    IEnumerator WaitNullCheck()
    {
        yield return new WaitForSeconds (1.1f); //EnterStageObjectArrayが1秒待ちなのでそれ以上
        
        var sw = new Stopwatch();
        sw.Start();

        while( sw.ElapsedMilliseconds < 500 && isStageArrayOK == false){ //無限ループ防止用
            //ステージ配列nullcheck
            isStageArrayOK = NullCheckStageArray();     
            Debug.Log("Is Stage NullCheck : " + isStageArrayOK);       
        }
    }

    //配列にnullがあるか調べる
    public bool NullCheckStageArray()
    {
        bool tempNullCheck = true;

        for(int x=0; x<stageSizeX; x++)
        {
            for(int z=0; z<stageSizeZ; z++)
            {
                if(stageObject[x,z]==null)
                {
                    tempNullCheck = false;
                    break;                    
                }
            }
        }
        
        return tempNullCheck;
    }
    
       
    IEnumerator JustBeforeGameStart()
    {  
        int localNum = PhotonNetwork.LocalPlayer.ActorNumber;

        GameObject.Find("LoadingMessage").GetComponent<Text>().text = localNum.ToString();

        //ステージアレイが全て格納されたら
        yield return new WaitUntil(() => isStageArrayOK == true);

        //ステージを回転させてカメラをステージの中心に移動させる
        SetStageAndCamera();

        //エッジからの距離を計算
        CalcDistanceFromEdge();

        //エッジから近い順にリストに追加
        AddStageListFromEdge();

        //プレイヤーを生成し変数を初期化        
        GenarateOnlinePlayer(localNum - 1);
            
        //コルーチン、ゲーム上にプレイヤータグがついた人が2人になったら配列に入れる。同期せずそれぞれで実行
        //isPlayerフラグもこの中でオンになる
        StartCoroutine(AddPlayerArray());
        

        //ステージ回転させないと見栄えが悪いから隠す
        GameObject.Find ("GameLoadingPanel").SetActive(false);
    }

    public void IsDebugTogle()
    {
        if(isDebug == false){isDebug = true;}
        else{isDebug = false;}
    }

    //オンラインプレイヤーの生成と初期化
    void GenarateOnlinePlayer(int i)
    {                        
            //初期位置を決める
            Vector3 firstPosition = GameManager.instance.stageObject[i * (GameManager.stageSizeX - 1),i * (GameManager.stageSizeZ - 1)].transform.position;
            
            //プレイヤーの人スタンスを生成
            GameObject player = PhotonNetwork.Instantiate("Photon/Player", firstPosition + new Vector3(0, 2, 0) , new Quaternion(1,0,0,180)); 
            
            //↓プレイヤーのメンバ変数を初期化
                //初期位置を代入
                player.GetComponent<Player>().firstPosition = firstPosition;

                //プレイヤーの属性(チーム)を決める
                player.GetComponent<Player>().playerPowerValue =  1 - (i*2); //1人目が1, 2人目が-1になる

                //プレイヤーのチーム色を決める
                player.GetComponent<Player>().teamColor = new Color(1-i,0, i); //1人目は赤, 2人目は青

                //プレイヤー自身の色を変える
                player.GetComponent<Player>().playerColor = player.GetComponent<Player>().teamColor * 0.4f + new Color(0.7f, 0.7f, 0.7f);
                    
                //プレイヤーのマスの色を決める
                player.GetComponent<Player>().paintColor = player.GetComponent<Player>().teamColor * 0.3f + new Color(0.1f, 0.1f, 0.1f);
    }

    void SetStageAndCamera()
    {
        //ステージを回転させる
        GameObject.Find ("StageHexagons").transform.rotation = Quaternion.Euler(0, 30, 0);
        
        GameObject stageL = GameManager.instance.stageObject[0,0];
        GameObject stageR = GameManager.instance.stageObject[(GameManager.stageSizeX-1),(GameManager.stageSizeZ-1)];

        //ステージの中心を探す
        Vector3 stageCenter = ((stageL.transform.position + stageR.transform.position)/2);
      
        //ステージのサイズに合わせてカメラを動かす
        GameObject.Find ("camCenter").transform.position = stageCenter;        
    }

    void CalcDistanceFromEdge()
    {
        for(int r=0; r<GameManager.instance.ringsCount; r++){

            for(int z=0; z<GameManager.stageSizeZ-(2*r); z++) //zの大きさぶんループ
            {
                GameManager.instance.stageObject[r, r+z].GetComponent<Stage>().distanceFromEdge = r;                
                GameManager.instance.stageObject[GameManager.stageSizeZ-r-1, r+z].GetComponent<Stage>().distanceFromEdge = r;

                for(int x=0; x<GameManager.stageSizeX-(2*r); x++)//xの大きさぶんループ
                {
                    //ステージの距離を格納
                    GameManager.instance.stageObject[r+x, r].GetComponent<Stage>().distanceFromEdge = r;
                    GameManager.instance.stageObject[r+x, GameManager.stageSizeX-r-1].GetComponent<Stage>().distanceFromEdge = r;
                }
            }
        }
    }
    void AddStageListFromEdge()
    {
        for(int r=0; r<GameManager.instance.ringsCount; r++){
            for(int z=0;z<GameManager.stageSizeZ;z++) //zの大きさぶんループ
            {
                for(int x=0;x<GameManager.stageSizeX;x++)//xの大きさぶんループ
                {
                    if(GameManager.instance.stageObject[x,z].GetComponent<Stage>().distanceFromEdge == r){                        
                        StageManager.instance.stageObjectFromEdge.Add(GameManager.instance.stageObject[x,z]);
                    }
                }
            }
        }
        
    }
    IEnumerator AddPlayerArray()
    {
        Debug.Log("プレイヤー人数: " + GameObject.FindGameObjectsWithTag("Player").Length);
        Debug.Log("isDebug: " + isDebug);
        //2人揃うまで待つ、デバッグモード
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Player").Length == 2 || isDebug == true);
        
        //プレイヤーを配列に入れる
        GameObject[] tempPlayerArray = GameObject.FindGameObjectsWithTag("Player");

            for(int j=0; j<tempPlayerArray.Length; j++)
            {
                if(tempPlayerArray[j].GetComponent<Player>().photonView.OwnerActorNr == 1)//タグ検索で取得したプレイヤーの配列の順番が正しいかを判定する
                {
                    playerArray[0] =  tempPlayerArray[j];                
                }else //正しくなければ順番を入れ替える
                {
                    playerArray[1] =  tempPlayerArray[j];                
                }
            }        
        
        for(int j=0; j<tempPlayerArray.Length; j++)
        {
            tempPlayerArray[j].name = "Player_" + j as string;
        }

        //ゲーム開始
        isPlaying = true;
    }

    IEnumerator GameFinish()
    {
        //ゲームフィニッシュフラグがたつ
        yield return new WaitUntil(() => isFinished == true);
        
        //勝ち負け判定
        int player1Sum = 0;
        int player2Sum = 0;
        
        for(int z=0;z<GameManager.instance.stageObject.GetLength(0);z++) //zの大きさぶんループ
        {
            for(int x=0;x<GameManager.instance.stageObject.GetLength(1);x++)//xの大きさぶんループ
            {
                //スコアカウント
                if(GameManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue > 0){
                    player1Sum += GameManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                }
                if(GameManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue < 0){
                    player2Sum -= GameManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                }

                //勝ち負け判定
                totalPowerCount += GameManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
            }
        }

        finishText.SetActive(true);
        if(totalPowerCount > 0){
            GameObject.Find ("winMessage").GetComponent<Text>().text = "Player1 is winner.";                
        }
        if(totalPowerCount < 0){
            GameObject.Find ("winMessage").GetComponent<Text>().text = "Player2 is winner.";                        
        }            
        if(totalPowerCount == 0){
            GameObject.Find ("winMessage").GetComponent<Text>().text = "Draw.";
            
        }

        messageText.GetComponent<Text>().text = ("Player1: " + player1Sum + "/" + "Player2: " + player2Sum);
                    
        nextButton.SetActive(true);
    }
}
