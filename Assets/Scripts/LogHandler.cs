using System;
using System.IO;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    private StreamWriter _writer;

    void Awake()
    {
        //_writer = File.AppendText("log.txt");
        DontDestroyOnLoad(gameObject);
        HandleLog("App started", "", LogType.Log);
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        _writer = File.AppendText("log.txt"); // queue to write

        var logEntry = string.Format("[{0}] LogLevel - \"{1}\" \n[Message] {2}\n {3}\n", DateTime.Now, type, condition, stackTrace);
        _writer.Write(logEntry);

        _writer.Close();
    }

    void OnDestroy()
    {
        //_writer.Close();
    }
}
