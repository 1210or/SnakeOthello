using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : MonoBehaviour
{
    public static Speed instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public string heroName = "Speed";
    
    public void PassiveSkill()
    {
        print("test, this is Speed's passive skill");
    }

    public void ActiveSlill()
    {
        print("test, this is Speed's active skill");
    }
}
