using System.IO;
using UnityEngine;
using System;

public class WriteLog : Manager
{ 
	public static WriteLog Instance { get { return GetManager<WriteLog>(); } }
	private string m_LogName;

	public void fileName (string logName)
	{
		m_LogName = logName;
	}
	public void writelog (string strLog)
	{
        #if UNITY_EDITOR
        string sFilePath = Application.persistentDataPath;
		string sFileName = m_LogName + DateTime.Now.ToString ("dd") + ".log";
		sFileName = sFilePath + "\\" + sFileName;
		if (!Directory.Exists (sFilePath)) {
			Directory.CreateDirectory (sFilePath);
		}

		FileStream fs;
		StreamWriter sw;
		if (File.Exists (sFileName))
		{
			fs = new FileStream (sFileName, FileMode.Append, FileAccess.Write);
		}
		else
		{
			fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
		}
		sw = new StreamWriter(fs);
		sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + " --- " + strLog );
		sw.Close();
		fs.Close();
        #endif
    }
}