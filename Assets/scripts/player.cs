using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Color playerColor;
    //public static Color playerColor = Color.red * 0.5f;
    
    [SerializeField]
    private int[] firstPosition;

    //直前にいたマスを保存するための変数
    private GameObject solverGameObject;

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

    // Start is called before the first frame update
    void Start()
    {
        playerColor = playerColor * 0.5f;
        this.GetComponent<Renderer>().material.color = playerColor * 2;
    
        //初期マスにプレイヤーをとばす
        this.transform.root.gameObject.transform.position = StageMaker.stageObject[firstPosition[0],firstPosition[1]].transform.position + new Vector3(0, 2, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        //print(this.gameObject.name + ": " + newHexaFlag);

         // 現在の位置から下(0,-1,0)に向かってRayをセット
        Ray ray = new Ray(transform.position + new Vector3(0,0.1f,0),Vector3.down);
        // Rayが当たった相手を保存する変数
        RaycastHit hit;

        // Rayを10.0fの距離まで発射。何かに当たればhitで受け取る
        if(Physics.Raycast(ray, out hit, 10.0f)) {
            // もし当たった相手のタグがstageなら下の足場の色を変える
            if(hit.collider.tag == "stage") {

                //もし直前のステージと足元のステージが違うならば(新しいマスに入ったら)
                if(solverGameObject != hit.collider.gameObject){
                    
                    //足元を塗りつぶす
                    hit.collider.gameObject.GetComponent<Renderer>().material.color = playerColor;

                    //マスの変数を変更する
                    hit.collider.gameObject.GetComponent<Stage>().isPlayerOn = true;
                    try{solverGameObject.GetComponent<Stage>().isPlayerOn =  false;}catch(System.Exception){/*何もしない*/}

                    //RGBの赤の値と青の値*-1の輪を求める。
                    hit.collider.gameObject.GetComponent<Stage>().stagePowerValue = (int)((2 * hit.collider.gameObject.GetComponent<Renderer>().material.color.r) + (-2 * hit.collider.gameObject.GetComponent<Renderer>().material.color.b));                   

                    newHexaFlag = true;//フラグを立てる
                }else
                {
                    newHexaFlag = false;//フラグを立てる
                }
                solverGameObject = hit.collider.gameObject;   
            }
        }

        //print(this.transform.position.y);

        if(this.transform.root.gameObject.transform.position.y < -2){
            
            //初期マスにプレイヤーをとばす
            this.transform.root.gameObject.transform.position = StageMaker.stageObject[firstPosition[0],firstPosition[1]].transform.position + new Vector3(0, 2, 0);
        
        }
    }
}
