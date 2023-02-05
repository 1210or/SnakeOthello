using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class GameManager : MonoBehaviourPunCallbacks
{
    //タイマーのための変数
    [SerializeField, Range(0, 100)]
    public float totalTime = 60;
    int seconds;
    
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

    public static GameManager instance;



    // Start is called before the first frame update
    void Awake()
    {          
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }   
    }

    void Start() {                                 
        
        //3秒後に開始
        StartCoroutine(JustBeforeGameStart());
        
        //if(isDebug==true)
        //resetPowerValueButton.SetActive(true);

        //ゲーム終了動作
        //コルーチン、isFinished==trueまで
        StartCoroutine(GameFinish());
    }
    
    public bool isFinished = false;
    public bool isPlaying = false;
    public bool isDebug = true;
    
    // Update is called once per frame
    void Update()
    {   
        //esc終了
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //プレイ中に動作
        if(GameManager.instance.isPlaying == true && isDebug == false){   

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
       
    IEnumerator JustBeforeGameStart()
    {  
        //本当は配列の中にnullがなくなるまでのコルーチンを作りたかったが二重配列の中を調べるのが複雑だった。
        yield return new WaitForSeconds (3.0f);

        //ステージを回転させてカメラをステージの中心に移動させる
        SetStageAndCamera();

        //エッジからの距離を計算
        CalcDistanceFromEdge();

        //エッジから近い順にリストに追加
        AddStageListFromEdge();

        //プレイヤーを生成し変数を初期化
        GenarateOnlinePlayer(PhotonNetwork.LocalPlayer.ActorNumber - 1);
            
        //コルーチン、ゲーム上にプレイヤータグがついた人が2人になったら配列に入れる。同期せずそれぞれで実行
        StartCoroutine(AddPlayerArray());
  
        //ステージ回転させないと見栄えが悪いから隠す
        GameObject.Find ("GameLoadingPanel").SetActive(false);
    }

    //オンラインプレイヤーの生成と初期化
    void GenarateOnlinePlayer(int i)
    {                        
            //初期位置を決める
            Vector3 firstPosition = StageManager.instance.stageObject[i * (StageManager.stageSizeX - 1),i * (StageManager.stageSizeZ - 1)].transform.position;
            
            //プレイヤーの人スタンスを生成
            GameObject player = PhotonNetwork.Instantiate("Player", firstPosition + new Vector3(0, 2, 0) , new Quaternion(1,0,0,180)); 
            
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
        
        //ステージの中心を探す
        Vector3 stageCenter = (StageManager.instance.stageObject[0,0].transform.position + StageManager.instance.stageObject[(StageManager.stageSizeX-1),(StageManager.stageSizeZ-1)].transform.position)/2;
      
        //ステージのサイズに合わせてカメラを動かす
        GameObject.Find ("camCenter").transform.position = stageCenter + new Vector3(0, 0, 1.8f);
    }

    void CalcDistanceFromEdge()
    {
        for(int r=0; r<StageManager.instance.ringsCount; r++){

            for(int z=0; z<StageManager.stageSizeZ-(2*r); z++) //zの大きさぶんループ
            {
            StageManager.instance.stageObject[r, r+z].GetComponent<Stage>().distanceFromEdge = r;
            StageManager.instance.stageObject[StageManager.stageSizeZ-r-1, r+z].GetComponent<Stage>().distanceFromEdge = r;

                for(int x=0; x<StageManager.stageSizeX-(2*r); x++)//xの大きさぶんループ
                {
                    //ステージの距離を格納
                    StageManager.instance.stageObject[r+x, r].GetComponent<Stage>().distanceFromEdge = r;
                    StageManager.instance.stageObject[r+x, StageManager.stageSizeX-r-1].GetComponent<Stage>().distanceFromEdge = r;
                }
            }
        }
    }
    void AddStageListFromEdge()
    {
        for(int r=0; r<StageManager.instance.ringsCount; r++){
        for(int z=0;z<StageManager.stageSizeZ;z++) //zの大きさぶんループ
        {
          for(int x=0;x<StageManager.stageSizeX;x++)//xの大きさぶんループ
          {
            if(StageManager.instance.stageObject[x,z].GetComponent<Stage>().distanceFromEdge == r){
              StageManager.instance.stageObjectFromEdge.Add(StageManager.instance.stageObject[x,z]);
            }
          }
        }
      }
    }
    IEnumerator AddPlayerArray()
    {
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

        for(int j=0; j<playerArray.Length; j++)
        {
            playerArray[j].name = "Player_" + j as string;
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
        
        for(int z=0;z<StageManager.instance.stageObject.GetLength(0);z++) //zの大きさぶんループ
        {
            for(int x=0;x<StageManager.instance.stageObject.GetLength(1);x++)//xの大きさぶんループ
            {
                //スコアカウント
                if(StageManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue > 0){
                    player1Sum += StageManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                }
                if(StageManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue < 0){
                    player2Sum -= StageManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                }

                //勝ち負け判定
                totalPowerCount += StageManager.instance.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
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
