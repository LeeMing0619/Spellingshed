using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

using Mono.Data.SqliteClient;
using System.IO;
using System.Data;

public class MenuSceneController : MonoBehaviour {

    public GameObject MylistPanel;
    public GameObject MyscorePanel;
    public UIGrid mylistGrid;
    public UIGrid myscoreGrid;
    public GameObject wordItemPrefab;
    public GameObject scoreItemPrefab;
    public List<string> words = new List<string>();
    
    private void Awake()
    {
        MylistPanel.SetActive(false);
        MyscorePanel.SetActive(false);
    }
    void Start()
    {
        DataManager.Instance.LoadData();
        words = DataManager.Instance.words;
        for (int i = 0; i < words.Count; i++)
        {
            GameObject go = Instantiate(wordItemPrefab, mylistGrid.transform);
            go.transform.GetChild(1).GetComponent<UIInput>().value = words[i];
            AddOnClickEvent(this, go.transform.GetChild(1).GetComponent<UIInput>(), "OnClickSave", go.transform.GetChild(1).GetComponent<UIInput>(), typeof(UIInput), i, typeof(int));
        }
        ShowHistory();
    }

    public void OnClick_SpellCheckButton()
    {
        MainGameManager.Instance.score = 0;
        MainGameManager.Instance.current_studayword = "";
        MainGameManager.Instance.isTryAgain = false;
        MainGameManager.Instance.isNext = false;
        MainGameManager.Instance.studayword_index = 0;
        MainGameManager.Instance.totalTime = 0;
        SceneManager.LoadScene("GameScene");
    }
    public void OnClick_MyListButton()
    {
        MyscorePanel.SetActive(false);
        MylistPanel.SetActive(true);
    }
    public void OnClick_MyScoreButton()
    {
        MyscorePanel.SetActive(true);
        MylistPanel.SetActive(false);
    }

    public void OnClickBack()
    {
        MylistPanel.SetActive(false);
        MyscorePanel.SetActive(false);
    }

    #region MyList
    public void OnClickSave(UIInput uinput, int index)
    {
        print(DataManager.Instance.WordtableName + DataManager.Instance.filePath);
        string connectionString = "URI=file:" + DataManager.Instance.filePath;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery_time2 = string.Format("UPDATE {0} SET word='{1}' WHERE Id={2}", DataManager.Instance.WordtableName, uinput.value, index+1);
                dbCmd.CommandText = sqlQuery_time2;
                dbCmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }

    }
    public void AddItem()
    {
        GameObject go = Instantiate(wordItemPrefab, mylistGrid.transform);
        AddOnClickEvent(this, go.transform.GetChild(1).GetComponent<UIInput>(), "OnClickSave", go.transform.GetChild(1).GetComponent<UIInput>(), typeof(UIInput), mylistGrid.transform.childCount-1, typeof(int));
        mylistGrid.Reposition();

        string connectionString = "URI=file:" + DataManager.Instance.filePath;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery5 = string.Format("INSERT INTO {0}('word')VALUES('{1}')", DataManager.Instance.WordtableName," ");
                dbCmd.CommandText = sqlQuery5;
                dbCmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }

    }
    #endregion

    #region MyScores
    public void ShowHistory()
    {
        List<MyScores> scores = new List<MyScores>();
        scores = DataManager.Instance.myscores;
        for (int i = 0; i < scores.Count; i++)
        {
            GameObject go = Instantiate(scoreItemPrefab, myscoreGrid.transform);
            go.transform.GetChild(0).GetComponent<UILabel>().text = scores[i].Date;
            go.transform.GetChild(1).GetComponent<UILabel>().text = scores[i].Times.ToString();
            go.transform.GetChild(2).GetComponent<UILabel>().text = scores[i].Score.ToString();
        }
        myscoreGrid.Reposition();
    }

    #endregion
    public void AddOnClickEvent(MonoBehaviour target, UIInput btn, string method, object value1, Type type1, object value2, Type type2)
    {
        EventDelegate onClickEvent = new EventDelegate(target, method);
        EventDelegate.Parameter param1 = new EventDelegate.Parameter();
        EventDelegate.Parameter param2 = new EventDelegate.Parameter();
        
        param1.value = value1;
        param1.expectedType = type1;
        param2.value = value2;
        param2.expectedType = type2;
        onClickEvent.parameters[0] = param1;
        onClickEvent.parameters[1] = param2;

        EventDelegate.Add(btn.onChange, onClickEvent);
    }
}
