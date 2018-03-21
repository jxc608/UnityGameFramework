using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

#pragma warning disable 0618

public class EditorCustomMenu : EditorWindow
{
	[MenuItem("自定义/工具/导出脚本对象/一般配置", false, 2)]
	static void CreateConfigrationAsset()
	{
		EditorUtils.CreateAsset<ConfigurationController>(ConfigurationController.ConfigurationControllerPath);
	}

	[MenuItem("自定义/工具/导出脚本对象/联网配置", false, 2)]
	public static void CreateServerConfigAsset()
	{
		EditorUtils.CreateAsset<DebugConfigController>(DebugConfigController.DebugConfigControllerPath);
	}

	[MenuItem("自定义/工具/清除本地存档", false, 3)]
	static void ClearSaveData()
	{
		PlayerPrefs.DeleteAll();
		Snaplingo.SaveData.SaveDataUtils.ClearAll();
	}

	[MenuItem("自定义/表格/客户端/导出表格", false, 2)]
	static void ExportConfigsClient()
	{
		ExecuteExportConfigsClient();
		AssetDatabase.Refresh();
	}

	static void ExecuteExportConfigsClient()
	{
		EditorUtils.ProcessCommand(Application.dataPath + "/Tools/Framework/BuildConfigs/build_configs", 
		                            new string[] { Application.dataPath }, true);
	}

	[MenuItem("自定义/烘焙/Login场景")]
	static void Bake()
	{
		var oldScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
        var loginSceneName = "Assets/Scenes/FrameworkEx/Login/Login";
		var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(loginSceneName + ".unity");
		if (!scene.IsValid())
			return;

		var oldQualityLevel = QualitySettings.GetQualityLevel();
		QualitySettings.SetQualityLevel(5);
		var oldShadowDistance = QualitySettings.shadowDistance;
		QualitySettings.shadowDistance = ConfigurationController.Instance._ShadowDistance;

		var oldIntensity = 0f;
		var go = GameObject.Find("Login/Directional Light");
		Light light = null;
		if (go != null) {
			light = go.GetComponent<Light>();
			if (light != null) {
				oldIntensity = light.intensity;
				light.intensity = ConfigurationController.Instance._BakeIntensity;
			}
		}

		LightmapEditorSettings.maxAtlasWidth = 1024;
		LightmapEditorSettings.maxAtlasHeight = 1024;
		Lightmapping.Clear();
		Lightmapping.Bake();

		if (oldIntensity > 0) {
			light.intensity = oldIntensity;
		}
		string[] filePaths = Directory.GetFiles(loginSceneName, "*.*", SearchOption.AllDirectories);
		foreach (var filePath in filePaths) {
			var fileName = Path.GetFileName(filePath);
			if (fileName.EndsWith(".png") || fileName.EndsWith(".exr")) {
				AssetImporter ai = AssetImporter.GetAtPath(filePath);
				if (ai is TextureImporter) {
					if (fileName.StartsWith("Lightmap")) {
						var ti = ((TextureImporter)ai);
						ti.mipmapEnabled = false;
						// iOS
						ti.SetPlatformTextureSettings("iPhone", 1024, TextureImporterFormat.PVRTC_RGB4);
						// Android
						ti.SetPlatformTextureSettings("Android", 1024, TextureImporterFormat.ETC_RGB4);
						ti.SaveAndReimport();
					}
				} else {
					Debug.LogError("Error! " + fileName + " is not an image: " + ai);
				}
			}
		}

		UnityEditor.SceneManagement.EditorSceneManager.OpenScene(oldScene);
		QualitySettings.shadowDistance = oldShadowDistance;
		QualitySettings.SetQualityLevel(oldQualityLevel);
	}

	[MenuItem("自定义/切图")]
	static void CutTexture ()
	{
		/*
		string filePath = "Assets/Artworks/ArtAlphabet/Small/xiaoxie.png";
		string savePath = "Assets/Artworks/ArtAlphabet/Small/";
		string filename = "abcdefghijklmnopqrstuvwxyz";
		cutPicture( filePath, savePath, filename );
		filePath = "Assets/Artworks/ArtAlphabet/Big/daxie.png";
		savePath = "Assets/Artworks/ArtAlphabet/Big/";
		filename = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		cutPicture( filePath, savePath, filename );
		*/
		string filePath = "Assets/Artworks/test/大写.png";
		string savePath = "Assets/Artworks/test/";
		string filename = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		cutPicture( filePath, savePath, filename );
	}

	static void cutPicture (string filepath, string savepath, string filename)
	{
		string path = filepath;
		Debug.Log (path);
		Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D> (path);
		Color[] pixels = tex.GetPixels ();
		float[] alpth = new float[tex.width];
		for (int i = 0; i < tex.width; i++) {
			float temp = 0f;
			for (int j = 0; j < tex.height; j++) {
				temp += pixels [i + j * tex.width].a;
			}
			alpth [i] = temp;
		}

		float alpt = 2f;
		bool Begin = false;
		int begin = 0;
		string alphabet=filename;
		int num = 0;

		for (int i = 0; i < tex.width; i++)
		{
			if (alpth [i] > alpt && !Begin)
			{
				begin = i;
				Begin = true;
			}
			if (alpth [i] <= alpt && Begin)
			{
				Texture2D newTex = new Texture2D (i - begin, tex.height);

				for (int j = 0; j < tex.height; j++)
				{
					for (int k = 0; k < i - begin; k++)
					{
						Color temp = pixels [begin + k + j * tex.width];
						newTex.SetPixel (k, j, temp);
						newTex.Apply ();
					}
				}
				string tempPath = savepath + alphabet[num] + ".png";
				FileStream sf = new FileStream (tempPath, FileMode.Create, FileAccess.ReadWrite);

				byte[] bytes = newTex.EncodeToPNG ();
				sf.Write (bytes, 0, bytes.Length);
				Begin = false;
				num++;
			}
		}
	}
	[MenuItem("自定义/图像重命名")]
	static void ChangeName()
	{
		string filePath = "Assets/Resources/ArtAlphabet/Chinese/";
		string savePath = "Assets/Resources/ArtAlphabet/Chinese/";
		string filename = "一们兄和奶妈妹姐家小开弟我抱拥是汽爷爸画能自行跑跳车辆骑阳星太亮月见看么什在你上天";
		string dataPath = "";
		string dataResultPath = "";
		string a = "";
		for (int i = 0; i < filename.Length; i++)
		{
			Debug.Log(filename[i]);
			dataPath = filePath + filename[i] + ".png";
			dataResultPath = savePath + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(filename[i].ToString())) + ".png";
			/*
			Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dataPath);
			FileStream sf = new FileStream(dataResultPath, FileMode.Create, FileAccess.ReadWrite);

			byte[] bytes = tex.EncodeToPNG();
			sf.Write(bytes, 0, bytes.Length);
			*/
			a = a + "mv "+dataPath + " " + dataResultPath + "/n";
		}
		Debug.Log(a);
	}
}
