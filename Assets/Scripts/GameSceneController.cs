using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Mono.Data.SqliteClient;
using System.IO;
using System.Data;
using Random = UnityEngine.Random;

public class GameSceneController : MonoBehaviour
{
    public GameObject ReadyPanel;
    public GameObject GamePanel;
    public GameObject CorrectPanel;
    public GameObject characterPrefab;
    public GameObject MessagePanel;

    public UILabel readyWord;
    public UILabel timer;
    public UILabel timeCounter;
    public UILabel Score;
    public UILabel levels;
    public UILabel warningMessage;

    public UIGrid Characters;

    public Transform Letters;

    public int timelimit = 5;
    public int timeCount = 20;

    private string studyword;
    private List<bool> checking = new List<bool>();
    private List<string> rightCharacters = new List<string>();
    private int wrongCount = 0;
    readonly string[] Alpabet = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "x", "y", "z" };
    private void Awake()
    {
        ReadyPanel.SetActive(true);
        GamePanel.SetActive(false);
        CorrectPanel.SetActive(false);
        timer.text = timelimit.ToString();
    }
    void Start ()
    {
        DataManager.Instance.LoadData();
        Score.text = MainGameManager.Instance.score.ToString();

        if (MainGameManager.Instance.isTryAgain)
        {
            studyword = MainGameManager.Instance.current_studayword;
        }
        else if (MainGameManager.Instance.isNext)
        {
            studyword = MainGameManager.Instance.current_studayword  = DataManager.Instance.words[MainGameManager.Instance.studayword_index];
        }
        else
        {
            studyword =  DataManager.Instance.words[0];
            MainGameManager.Instance.current_studayword = studyword;
            MainGameManager.Instance.studayword_index = 0;
        }
        CreateCharacters();
        readyWord.text = studyword;
        StartCoroutine("TimerCoroutine");
        StartCoroutine("Updating");
    }

    // ready word timer counting
    IEnumerator TimerCoroutine()
    {
        while (!timelimit.Equals(0))
        {
            timelimit--;
            timer.text = timelimit.ToString();
            if (timelimit.Equals(0))
            {
                GamePanel.SetActive(true);
                StartCoroutine("TimeCounting");
                ReadyPanel.SetActive(false);
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    // Create Characters
    public void CreateCharacters()
    {
        char[] characters = studyword.ToCharArray();

        //foreach (char item in characters)
        //{
        //    Debug.Log(item);
        //}

        for (int j = 0; j < characters.Length; j++)
        {
            GameObject character = Instantiate(characterPrefab, Characters.transform);
            rightCharacters.Add(characters[j].ToString());
        }

        Characters.Reposition();

        int count = Characters.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            checking.Add(false);
        }
        RandomLetterValue();
    }

    // When a letter is clicked, check spelling
    public void OnClick_Letter(UILabel letterLbl)
    {
        string letterStr = letterLbl.text;
        print(letterStr);
        int matchId = 0;
        for (int i = 0; i < checking.Count; i++)
        {
            if (!checking[i])
            {
                matchId = i;
                break;
            }
        }
        if (rightCharacters[matchId].Equals(letterStr)) // correct letter
        {
            Characters.GetChild(matchId).GetComponent<UILabel>().text = letterStr;
            letterLbl.transform.parent.Find("block").gameObject.SetActive(true);
            letterLbl.transform.parent.GetComponent<BoxCollider>().enabled = false;
            checking[matchId] = true;
        }
        else // wrong letter
        {
            wrongCount++;
            
            Characters.GetChild(matchId).GetComponent<UILabel>().text = letterStr;
            letterLbl.transform.parent.Find("red").gameObject.SetActive(true);
            checking[matchId] = false;
            //StartCoroutine(DeleteLetter(letterLbl));
            switch (wrongCount)
            {
                case 1:
                    warningMessage.gameObject.SetActive(true);
                    warningMessage.text = "You have two more tries";
                    break;
                case 2:
                    warningMessage.text = "You have one more try...";
                    break;
                case 3:
                    warningMessage.text = "Last try...";
                    break;
                case 4:
                    warningMessage.text = "";
                    warningMessage.gameObject.SetActive(false);
                    MessagePanel.SetActive(true);
                    StopAllCoroutines();
                    break;

            }
        }
    }
    IEnumerator DeleteLetter(UILabel letterLbl)
    {
        yield return new WaitForSeconds(0.7f);
        letterLbl.color = Color.white;
    }

    // Input string value in letter
    private void RandomLetterValue()
    {
        //string[] realletters = rightCharacters.Distinct().ToArray();
        int lettersCount = Letters.childCount;
        int[] arr = GetRandomInt(rightCharacters.Count, 0, lettersCount);

        //foreach (int item in arr)
        //{
        //    print(item);
        //}
        
        for (int i = 0; i < rightCharacters.Count; i++)
        {
            Letters.GetChild(arr[i]).GetChild(0).GetComponent<UILabel>().text = rightCharacters[i];
        }
        
        int diff = lettersCount - rightCharacters.Count;
        print("lettersCount: " + lettersCount + " rightCharacterCount: " + rightCharacters.Count);
        print(diff);
        List<string> remainLetters = new List<string>();
        List<string> compareArray = new List<string>();
        List<int> remainIndexLetter = new List<int>();

        for (int num = 0; num < rightCharacters.Count; num++)
        {
            compareArray.Add(rightCharacters[num]);
        }
        
        for (int index = 0; index < diff; index++)
        {
            bool result = false;
            bool isSame = true;
            while (!result)
            {
                int random = UnityEngine.Random.Range(0, 24);
                string val = Alpabet[random];
                for (int i = 0; i < compareArray.Count; i++)
                {
                    if (val.Equals(compareArray[i]))
                    {
                        isSame = true;
                        break;
                    }
                    else
                        isSame = false;
                }
                if (!isSame)
                {
                    //print("## " + val);
                    compareArray.Add(val);
                    remainLetters.Add(val);
                    result = true;
                }
            }
        }
        
        for (int i = 0; i < lettersCount; i++)
        {
            string tex = Letters.GetChild(i).GetChild(0).GetComponent<UILabel>().text;
            if (string.IsNullOrEmpty(tex))
            {
                remainIndexLetter.Add(i);
            }
        }

        for (int j = 0; j < remainLetters.Count; j++)
        {
            Letters.GetChild(remainIndexLetter[j]).GetChild(0).GetComponent<UILabel>().text = remainLetters[j];
        }

        for (int k = 0; k < lettersCount; k++)
        {
            UILabel lbl = Letters.GetChild(k).GetChild(0).GetComponent<UILabel>();
            AddOnClickEvent(this, Letters.GetChild(k).GetComponent<UIButton>(), "OnClick_Letter", lbl, typeof(UILabel));
        }
        
    }

    // top timer counting
    IEnumerator TimeCounting()
    {
        while (!timeCount.Equals(0))
        {
            timeCount--;
            MainGameManager.Instance.totalTime++;
            timeCounter.text = timeCount.ToString();
            if (timeCount.Equals(0))
            {
                print("Game End");
                CorrectPanel.SetActive(true);
                if (!checking[checking.Count - 1])
                {
                    CorrectPanel.transform.Find("Label").GetComponent<UILabel>().text = "Mistake";
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
    // dynamic event
    public void AddOnClickEvent(MonoBehaviour target, UIButton btn, string method, object value, Type type)
    {
        EventDelegate onClickEvent = new EventDelegate(target, method);
        EventDelegate.Parameter param = new EventDelegate.Parameter();
        param.value = value;
        param.expectedType = type;
        onClickEvent.parameters[0] = param;
        EventDelegate.Add(btn.onClick, onClickEvent);
    }

    // random array, not duplicated
    public int[] GetRandomInt(int length, int min, int max)
    {
        int[] randArray = new int[length];
        bool isSame;
        for (int i = 0; i < length; i++)
        {
            while (true)
            {
                randArray[i] = Random.Range(min, max);
                isSame = false;
                for (int j = 0; j < i; j++)
                {
                    if (randArray[j] == randArray[i])
                    {
                        isSame = true;
                        break;
                    }
                }
                if (!isSame)
                    break;
            }
        }
        return randArray;
    }
    IEnumerator Updating()
    {
        while (!CorrectPanel.activeSelf)
        {
            if (checking[checking.Count - 1])
            {
                // calculate stars percent
                int correctCount = rightCharacters.Count - wrongCount;
                MainGameManager.Instance.score += correctCount;
                Score.text = MainGameManager.Instance.score.ToString();
                float percent = Mathf.Round(correctCount * 100 / rightCharacters.Count);
                int starCount = (int)Mathf.Round(percent / 20);
                for (int i = 0; i < starCount; i++)
                {
                    CorrectPanel.transform.Find("Stars").GetChild(i).GetComponent<UISprite>().spriteName = "star full";
                }
                StopAllCoroutines();
                CorrectPanel.SetActive(true);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void OnClickReturn()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClick_TryAgain()
    {
        MainGameManager.Instance.isTryAgain = true;
        SceneManager.LoadScene("GameScene");
    }
    public void OnClick_NextWord()
    {
        MainGameManager.Instance.isNext = true;
        MainGameManager.Instance.studayword_index += 1;
        if (MainGameManager.Instance.studayword_index > DataManager.Instance.words.Count - 1)
        {
            print("Game Over");
            InsertScore();
            SceneManager.LoadScene("MenuScene");
        }
        else
            SceneManager.LoadScene("GameScene");
    }

    public void InsertScore()
    {
        string newTime = System.DateTime.Now.ToLongDateString() + " / " + System.DateTime.Now.ToLongTimeString();
        print(newTime + "  " + MainGameManager.Instance.totalTime.ToString());
        string connectionString = "URI=file:" + DataManager.Instance.filePath;
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = string.Format("INSERT INTO {0}('Date', 'Time', 'Score')VALUES('{1}', {2}, {3})", DataManager.Instance.ScorestableName, newTime, MainGameManager.Instance.totalTime, MainGameManager.Instance.score);
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
    }
}
