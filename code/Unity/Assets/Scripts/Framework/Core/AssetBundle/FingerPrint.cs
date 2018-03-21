using LitJson;
using System.IO;
using System.Collections;

public class FingerPrint
{
	static JsonData _fingerPrintData;
	static bool _started = false;
	static string _fingerPrintPath = "";

	public static void AddItem(string path, string md5, string folderPath)
	{
		if (_started) {
			string cleanPath = path.Replace(folderPath, "");
			_fingerPrintData[cleanPath] = md5;
		}
	}

	public static void DeleteItem(string path, string folderPath)
	{
		if (_started) {
			string cleanPath = path.Replace(folderPath, "");
			if (_fingerPrintData.Keys.Contains(cleanPath)) {
				((IDictionary)_fingerPrintData).Remove(cleanPath);
			}
		}
	}

	public static void Flush()
	{
		if (_started) {
			File.WriteAllText(_fingerPrintPath, _fingerPrintData.ToJson());

			_fingerPrintData = null;
			_started = false;
			_fingerPrintPath = "";
		}
	}

	public static void StartWriting(string folderPath)
	{
		_fingerPrintPath = folderPath + "/FingerPrint.txt";
		if (File.Exists(_fingerPrintPath))
			_fingerPrintData = JsonMapper.ToObject(File.ReadAllText(_fingerPrintPath));
		else
			_fingerPrintData = new JsonData();
		_started = true;
	}

}
