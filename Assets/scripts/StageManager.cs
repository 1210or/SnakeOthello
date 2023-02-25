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

  public static int stageProcessFlag = 0;
  
  public GameObject stageParent;

  //ステージ端から近い順にオブジェクト
  public List<GameObject> stageObjectFromEdge;
  
  public static StageManager instance;  
  void Awake()
  {//インスタンス化
    if(instance == null)
    {
      instance = this;
    }
  }
  void Update()
  {
     
  }

  //リセットボタンから呼び出し
  public void ResetAllPowerValue()
  {
    for(int i=0; i<stageObjectFromEdge.Count; i++)
    {
      stageObjectFromEdge[i].GetComponent<Stage>().stagePowerValue = 0;
    } 
  }

}


