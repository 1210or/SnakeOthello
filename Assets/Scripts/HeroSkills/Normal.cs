using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Normal : MonoBehaviour
{
    public static Normal instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public string heroName = "Normal";
    
    public void PassiveSkill()
    {
        print("test, this is Normal's passive skill");
    }

    public void ActiveSlill()
    {
        print("test, this is Normal's active skill");
    }
}
