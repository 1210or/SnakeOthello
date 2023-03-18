using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TitleUiManager : MonoBehaviourPunCallbacks
{
    //UI
    public GameObject messageText;
    public GameObject createRoomPanel;
    public Text enterRoomName;
    public GameObject lobbyButtons;
    public string levelToPlay;
    public GameObject roomPanel;
    public Text roomName;

    public GameObject roomListPanel;

    public Text playerNameText;

    private List<Text> allPlayerNames = new List<Text>();

    public GameObject playerNameContent;

    public GameObject nameInputPanel;

    public Text placeholderText;

    public InputField nameInput;

    private bool setName;

    public GameObject startButton;

    public GameObject selectHeroPanel;

    public GameObject openSelectHeroPanel;



    //ルームボタン格納
    public Room originalRoomButton;
    //ルームボタンの親オブジェクト
    public GameObject roomButtonContent;
    //ルームの情報を扱う辞書
    Dictionary<string, RoomInfo> roomsList = new Dictionary<string, RoomInfo> ();

 

    //ルームボタンを扱うリスト
    private List<Room> allRoomButtons = new List<Room>();
    
    public static TitleUiManager instance;

    public ToggleGroup heroToggleGroup;
    public string selectedHero;

    // Start is called before the first frame update
    void Awake()
    {   // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings(); 

        selectedHero = "Normal";

        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }   
     }

     void Update()
     {
        //esc終了
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
     }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        //ロビー接続
        PhotonNetwork.JoinLobby();
        messageText.GetComponent<Text>().text = "Connecting...";

        //辞書の初期化
        roomsList.Clear();

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();//ユーザーネームをとりあえず適当に決める

        ConfirmationName();//名前が入力されていればその名前を入力テキストに反映させる

        //MasterClientと同じレベルをロード
        PhotonNetwork.AutomaticallySyncScene = true;
        
    }

    //ロビー接続時に呼ばれるコールバック
    public override void OnJoinedLobby(){
        messageText.GetComponent<Text>().text = "Joined lobby";
    }
    
    //ボタンから呼び出される
    public void OpenCreateRoomPanel()
    {
        createRoomPanel.SetActive(true);
    }
    
    //入力テキストから部屋を作る
    public void CreateRoomButton()
    {
        //ルームのオプションをインスタンス化して変数に入れる 
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 2;// プレイヤーの最大参加人数の設定（無料版は20まで。1秒間にやり取りできるメッセージ数に限りがあるので10以上は難易度上がる）

        if(!string.IsNullOrEmpty(enterRoomName.text)) ///入力文字がエンプティじゃなければ
        {
            
            //ルームを作る(ルーム名：部屋の設定)
            PhotonNetwork.CreateRoom(enterRoomName.text, options);

        }else
        {
            // ユニークなルーム名を自動生成してルームを作成する
            PhotonNetwork.CreateRoom(null, options);

            messageText.GetComponent<Text>().text = "Creating a new room...";
        }
    }
    
    //部屋に入った時のコールバック
    public override void OnJoinedRoom()
    {
        roomPanel.SetActive(true);

        //ルームの名前を反映する
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        CloseUI();
        roomPanel.SetActive(true);

        //ルーム名を表示   
        messageText.GetComponent<Text>().text = "Joined a room: " + PhotonNetwork.CurrentRoom.Name;

        //ルームにいるプレイヤー情報を取得する
        GetAllPlayer();

        //マスターか判定
        CheckRoomMaster();            
        
    }

    //ルーム退出関数
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();

        //UI
        CloseUI();
    }

    public void LeaveSelectRoom()
    {
        //UI
        CloseUI();
        //ロビーのUI表示
        lobbyButtons.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        //ロビーのUI表示
        lobbyButtons.SetActive(true);
    }

    public void FindRoom()
    {        
        CloseUI();
        roomListPanel.SetActive(true);
    }

    //ルームリストに更新があったときに呼ばれる関数
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {        
        RoomUIinitialization();//ルームUIの初期化
        //辞書に登録
        UpdateRoomList(roomList);
    }
        //ルームボタンUI初期化
    void RoomUIinitialization()
    {
        foreach (Room rm in allRoomButtons)// ルームオブジェクトの数分ループ
        {
            Destroy(rm.gameObject);// ボタンオブジェクトを削除
        }

        allRoomButtons.Clear();//リスト要素削除

    }

    //ルームの情報を辞書に
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)//ルームの数分ループ
        {
            RoomInfo info = roomList[i];//ルーム情報を変数に格納

            if (info.RemovedFromList)//ロビーで使用され、リストされなくなった部屋をマークします（満室、閉鎖、または非表示）
            {
                roomsList.Remove(info.Name);//辞書から削除
            }
            else
            {
                roomsList[info.Name] = info;//ルーム名をキーにして、辞書に追加
            }
        }

        RoomListDisplay(roomsList);//辞書にあるすべてのルームを表示
    }

    //ルームボタン作成して表示
    void RoomListDisplay(Dictionary<string,RoomInfo> cachedRoomList)
    {
        foreach(var roomInfo in cachedRoomList)
        {
            //ルームボタン作成            
            Room newButton = Instantiate(originalRoomButton);                        
            
            //生成したボタンにルームの情報を設定
            newButton.RegisterRoomDetails(roomInfo.Value);
            
            //生成したボタンに親の設定
            newButton.transform.SetParent(roomButtonContent.transform);
            
            //リストに追加
            allRoomButtons.Add(newButton);
        }
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        //ルームに参加
        PhotonNetwork.JoinRoom(roomInfo.Name);

        //UI
        CloseUI();
    }

    public void GetAllPlayer()
    {
        //初期化
        InitializePlayerList();

        //プレイヤー表示
        PlayerDisplay();
    }


    //プレイヤー一覧初期化
    void InitializePlayerList()
    {
        //リストで管理している数分ループ
        foreach (var rm in allPlayerNames)
        {
            //text削除
            Destroy(rm.gameObject);
        }

        //リスト初期化
        allPlayerNames.Clear();

    }

    //ルームにいるプレイヤーを表示する
    void PlayerDisplay()
    {
        //ルームに接続しているプレイヤーの数分ループ
        foreach (var players in PhotonNetwork.PlayerList)
        {
            //テキストの生成
            PlayerTextGeneration(players);
        }
    }

    //プレイヤーテキスト生成
    void PlayerTextGeneration(Photon.Realtime.Player players)
    {
        Text newPlayerText = Instantiate(playerNameText);//用意してあるテキストをベースにプレイヤーテキストを生成
        newPlayerText.text = players.NickName;//テキストに名前を反映
        newPlayerText.transform.SetParent(playerNameContent.transform);//親の設定

        allPlayerNames.Add(newPlayerText);//リストに追加
    }

     //名前の判定
    private void ConfirmationName()
    {
        if (!setName)//名前を入力していない場合
        {
            //UI
            CloseUI();            

            nameInputPanel.SetActive(true);


            if (PlayerPrefs.HasKey("playerName"))//キーが保存されているか確認
            {                
                placeholderText.text = PlayerPrefs.GetString("playerName");
                
                nameInput.text = PlayerPrefs.GetString("playerName");//インプットフィールドに名前を表示しておく
            }

        }
        else//一度入力したことがある場合は自動的に名前をセットする（ルーム入って戻った時とかいちいち表示されないように）
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    //名前登録関数
    public void SetName()
    {
        if (!string.IsNullOrEmpty(nameInput.text))//入力されている場合
        {
            
            PhotonNetwork.NickName = nameInput.text;//ユーザー名に入力された名前を反映

            PlayerPrefs.SetString("playerName", nameInput.text);//名前を保存する            

            CloseUI();
            lobbyButtons.SetActive(true);
            //LobbyMenuDisplay();//ロビーに戻る

            setName = true;//名前入力済み判定
        }
    }

    //プレイヤーがルームに入ったときに呼び出されるコールバック
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        PlayerTextGeneration(newPlayer);
    }

    //プレイヤーがルームを離れるか、非アクティブになったとき
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        GetAllPlayer();
    }

    public void CheckRoomMaster()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }else
        {
            CloseUI();
        }
    }

    //マスターが切り替わったときに呼ばれる関数
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }

    //遷移関数、ボタンから呼び出される
    public void PlayGame()
    {   
        //mainシーンへ遷移
        PhotonNetwork.LoadLevel(levelToPlay);
    }

    public void OpenHeroSelectPanel()
    {
        selectHeroPanel.SetActive(true);
        //Find関数がアクティブなオブジェクトにしか作用しないため、ヒーローパネルが開かれたタイミングで探す
        heroToggleGroup = GameObject.Find ("HeroNameContent").GetComponent<ToggleGroup>();
    }
    
    public void CloseHeroSelectPanel()
    {
        //ヒーロー選択パネルを閉じたタイミングで選択しているヒーローを取得
        selectedHero = heroToggleGroup.ActiveToggles().FirstOrDefault().name;
        selectHeroPanel.SetActive(false);
    }
    

    public void CloseUI()
    {
        createRoomPanel.SetActive(false);
        lobbyButtons.SetActive(false);
        roomPanel.SetActive(false);
        roomListPanel.SetActive(false);
        nameInputPanel.SetActive(false);
        startButton.SetActive(false);
        selectHeroPanel.SetActive(false);
    }

}
