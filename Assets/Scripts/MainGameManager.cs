using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : Singleton<MainGameManager>
{
    public int score = 0;
    public string current_studayword = "";
    public bool isTryAgain = false;
    public bool isNext = false;
    public int studayword_index = 0;
    public int totalTime = 0;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    protected MainGameManager() { }

}
