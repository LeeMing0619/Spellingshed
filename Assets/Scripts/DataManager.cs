using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

using Mono.Data.SqliteClient;
using System.IO;
using System.Data;

public class DataManager : MonoBehaviour {

    string dataAsJson;
    public string filePath = "";
    public string dbname = "Data.db";
    public string WordtableName = "Words";
    public string ScorestableName = "Scores";
    public static DataManager Instance { get; private set; }

    //[HideInInspector]
    public List<string> words = new List<string>();
    public List<MyScores> myscores = new List<MyScores>();
    
    void Awake()
    {
        //Unity singleton 
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadData();
    }
    private void Start()
    {
        
        
    }

    public void LoadData()
    {
        words.Clear();
        myscores.Clear();
        #region
        filePath = string.Empty;
        if (Application.platform == RuntimePlatform.Android)
        {
            filePath = Application.persistentDataPath + "/" + dbname;
            if (!File.Exists(filePath))
            {
                print("File \"" + filePath + "\" does not exist. Attempting to create from \"" +
                    Application.dataPath + "!/assets/" + dbname);
                WWW LoadDB = new WWW("jar:file://" + Application.dataPath + "!/assets/" + dbname);
                LoadDB.bytesDownloaded.ToString();
                while (!LoadDB.isDone) { }
                File.WriteAllBytes(filePath, LoadDB.bytes);
            }
        }
        else
        {
            filePath = Application.dataPath + "/" + dbname;
            if (!File.Exists(filePath))
            {
                File.Copy(Application.streamingAssetsPath + "/" + dbname, filePath);
            }
        }

        string connectionString = "URI=file:" + filePath;
        #endregion
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery1 = string.Format("SELECT * FROM {0}", WordtableName);
                string sqlQuery2 = string.Format("SELECT * FROM {0}", ScorestableName);

                dbCmd.CommandText = sqlQuery1;
                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string word = reader.GetString(1);
                        //print(word);
                        words.Add(word);
                    }
                    reader.Close();
                }

                dbCmd.CommandText = sqlQuery2;
                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        myscores.Add(new MyScores(reader.GetString(1), int.Parse(reader.GetString(2)), int.Parse(reader.GetString(3))));

                    }
                    reader.Close();
                }
            }
            dbConnection.Close();
        }

        foreach (MyScores item in myscores)
        {
            print(item.Date + " " + item.Times + " " + item.Score);
        }
    }

}