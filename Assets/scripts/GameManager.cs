using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
    
    

    //プレイヤー作成のための変数
    public GameObject player;
    public GameObject[] playerArray = new GameObject[2];
    //public List<GameObject> playersList = null;

    //スコアのための変数
    public int totalPowerCount = 0;

    public static GameManager instance;

    // Start is called before the first frame update
    void Awake()
    {   // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();        

        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }   
    }

    void Start() {                                 
            int i = PhotonNetwork.LocalPlayer.ActorNumber - 1;            
            
            //初期位置を格納
            Vector3 firstPosition = StageManager.stageObject[i * (StageManager.stageSizeX - 1),i * (StageManager.stageSizeZ - 1)].transform.position;
            
            //プレイヤーの人スタンスを生成
            GameObject player = PhotonNetwork.Instantiate("Player", firstPosition + new Vector3(0, 2, 0) , new Quaternion(1,0,0,180)); 

            //初期位置を決める
            player.GetComponent<Player>().firstPosition = firstPosition;

            //これが同期されていない   
            //プレイヤーの名前を変える
            player.name = "Player" + (i + 1) as string; 

            //プレイヤーの属性(チーム)を決める
            player.GetComponent<Player>().playerPowerValue =  1 - (i*2); //1人目が1, 2人めが-1になる

            //初期位置のステージにパワー値を設定
            //StageManager.stageObject[i * (StageManager.stageSizeX - 1),i * (StageManager.stageSizeZ - 1)].GetComponent<Stage>().stagePowerValue = player.GetComponent<Player>().playerPowerValue;
 
            //プレイヤーのチーム色を決める
            player.GetComponent<Player>().teamColor = new Color(1-i,0, i); //1人目は赤, 2人目は青

            //プレイヤー自身の色を変える
            player.GetComponent<Player>().playerColor = player.GetComponent<Player>().teamColor * 0.4f + new Color(0.7f, 0.7f, 0.7f);
                
            //プレイヤーのマスの色を決める
            player.GetComponent<Player>().paintColor = player.GetComponent<Player>().teamColor * 0.3f + new Color(0.1f, 0.1f, 0.1f);

            //コルーチン、ゲーム上にプレイヤータグがついた人が2人になったら配列に入れる。同期せずそれぞれで実行
            StartCoroutine(addPlayerArray());

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

        //プレイヤーが2人揃ったら始動
        if(PhotonNetwork.PlayerList.Length == 2 && isDebug == false){   

        //時間制限と勝敗
        if(totalTime != 0){

            if(totalTime > 1){//ゲーム中

                //ゲームタイマー                
                totalTime -= Time.deltaTime;                
                seconds = (int)totalTime;
                messageText.GetComponent<Text>().text = ("Last " + seconds.ToString() + " second");
                //print(seconds.ToString());  
            }else{//ゲーム終了

                //一回だけ処理
                if(isFinished != true){
                    isPlaying = false;

                    //勝ち負け判定
                    int player1Sum = 0;
                    int player2Sum = 0;
                    for(int z=0;z<StageManager.stageObject.GetLength(0);z++) //zの大きさぶんループ
                    {
                        for(int x=0;x<StageManager.stageObject.GetLength(1);x++)//xの大きさぶんループ
                        {
                            //スコアカウント
                            if(StageManager.stageObject[x,z].GetComponent<Stage>().stagePowerValue > 0){
                                player1Sum += StageManager.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                            }
                            if(StageManager.stageObject[x,z].GetComponent<Stage>().stagePowerValue < 0){
                                player2Sum -= StageManager.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                            }

                            //勝ち負け判定
                            totalPowerCount += StageManager.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                        }
                    }

                    finishText.SetActive(true);
                    if(totalPowerCount > 0){
                        GameObject.Find ("winMessage").GetComponent<Text>().text = "Player1 is winner.";
                        //print("赤の勝ち");
                    }
                    if(totalPowerCount < 0){
                        GameObject.Find ("winMessage").GetComponent<Text>().text = "Player2 is winner.";
                        //print("青の勝ち");
                    }            
                    if(totalPowerCount == 0){
                        GameObject.Find ("winMessage").GetComponent<Text>().text = "Draw.";
                        //print("引き分け");
                    }

                    messageText.GetComponent<Text>().text = ("Player1: " + player1Sum + "/" + "Player2: " + player2Sum);
                    
                    nextButton.SetActive(true);

                    isFinished = true;
                }

                
            }
        }
    }


        /*
        for(int i = 0; i < playersList.Count; i++) //プレイヤーの人数分まわす
        {
            if(playersList[i].GetComponent<Player>().newHexaFlag == true)
            {
                //print("どちらかがマスを移動した");
                break;
            }
        }*/
    }

    public void Close()
    {

    }

    //ボタンから呼び出される
    public void Retry()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator addPlayerArray(){

        //2人揃うまで待つ
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Player").Length == 2);
        
        //プレイヤーを配列に入れる
        GameObject[] tempPlayerArray = GameObject.FindGameObjectsWithTag("Player");

        for(int j=0; j<tempPlayerArray.Length; j++)
        {
            if(tempPlayerArray[j].GetComponent<Player>().playerPowerValue == 1 - (j*2))//タグ検索で取得したプレイヤーの配列の順番が正しいかを判定する
            {
                playerArray[j] =  tempPlayerArray[j];                
            }else //正しくなければ順番を入れ替える
            {
                playerArray[j] =  tempPlayerArray[1-j];                
            }
        }

        for(int j=0; j<playerArray.Length; j++)
        {
            playerArray[j].name = "Player_" + j as string;
        }

        //ゲーム開始
        isPlaying = true;
    }
}
