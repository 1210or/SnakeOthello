using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HeroParamater : ScriptableObject 
{
  public string heroName = "";   
  public float HP = 100;
  public float walkSpeed = 3.5f;    
  public float heroSpeedBuff = 2;
  public float atack = 1;
  public float diffence = 1;
  public float respornTime = 1;
}