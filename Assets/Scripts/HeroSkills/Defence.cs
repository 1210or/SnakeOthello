using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defence : MonoBehaviour
{
    public static Defence instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public string heroName = "Defence";
    
    public void PassiveSkill()
    {
        print("test, this is Defence's passive skill");
    }

    public void ActiveSlill()
    {
        print("test, this is Defence's active skill");
    }
}
