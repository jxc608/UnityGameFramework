using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitJson;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Snaplingo.UI;

public class LoadSceneManager : Manager
{
	// 注意，请不要在Awake中使用Instance，否则会出现死循环
	public static LoadSceneManager Instance { get { return GetManager<LoadSceneManager>(); } }

	public static bool _isLoading = false;

	string _lastScene = "";

	public string GetLastScene()
	{
		return _lastScene;
	}

	// 开始异步加载场景
	// sceneName: 下一场景名
	// bundleName: 要同时加载的bundle名字
	// loadingSceneCallback: 完成加载Loading场景后的回调
	// nextSceneInitWork: 下一场景预加载函数，会将预加载数据存储到_nextSceneData，函数调用的时候立刻执行
	// nextSceneHandleData: 下一场景加载完成后取回预加载数据的回调函数
	// forceLoad: 跳过Loading场景，直接加载下一场景
	public void LoadSceneAsync(string sceneName, bool dontDestroy = false, 
	                           NextSceneCallback nextSceneHandleData = null)
	{
		if (_isLoading)
			return;

		_isLoading = true;
		
		var currentScene = SceneManager.GetActiveScene().name;
		if (!currentScene.Equals(LoadingScene)) {
			_dontDestroy = dontDestroy;
			_lastScene = currentScene;
			_nextScene = sceneName;
			_bundleName = sceneName;
			if (_currentSceneEndCallback != null) {
				_currentSceneEndCallback();
				_currentSceneEndCallback = null;
			}
			_loadPageComplete = true;
			AddNextSceneHandler(nextSceneHandleData);

			StartLoadLoadingScene();
		}
	}

	public void LoadSceneAsyncAndOpenPage(string sceneName, string pageName, bool dontDestroy = false, Action finishCallback = null, 
	                                      NextSceneCallback nextSceneHandleData = null)
	{
		if (_isLoading)
			return;
		
		nextSceneHandleData += delegate {
			finishCallback += LoadPageCompleteCallback;
			PageManager.Instance.OpenPage(pageName, dontDestroy, finishCallback, true);
		};
		LoadSceneAsync(sceneName, dontDestroy, nextSceneHandleData);
		PageManager.State = PageManager.CommandState.WaitingForCommand;
		_loadPageComplete = false;
	}

	#region 载入加载场景

	//当前场景结束时的回调
	Action _currentSceneEndCallback = null;

	public void AddCurrentSceneEndCallback(Action callback)
	{
		_currentSceneEndCallback += callback;
	}

	public void RemoveCurrentSceneEndCallback(Action callback)
	{
		_currentSceneEndCallback -= callback;
	}

	// 完成加载Loading场景后的回调
	public delegate void LoadingSceneCallback();

	public static string LoadingScene = "LoadingScene";

	LoadingSceneCallback _loadingSceneCallback = null;

	// 开始加载Loading场景
	void StartLoadLoadingScene()
	{
		AsyncOperation aon = SceneManager.LoadSceneAsync(LoadingScene);
		StartCoroutine(LoadLoadingScene(aon));
	}

	bool _dontDestroy = false;
	bool _loadPageComplete;
	// 加载Loading场景
	IEnumerator LoadLoadingScene(AsyncOperation aon)
	{
		yield return new WaitForEndOfFrame();
		if (aon.progress < 1) {
			StartCoroutine(LoadLoadingScene(aon));
		} else {
			while (SceneManager.GetActiveScene().name != LoadingScene) {
				yield return null;
			}

            InitProgress();
            // 完成加载Loading场景
            if (_loadingSceneCallback != null) {
				_loadingSceneCallback();
				_loadingSceneCallback = null;
			}

			yield return new WaitForEndOfFrame();

			if (_preLoadObject != null) {
				Destroy(_preLoadObject);
			}


			yield return null;

			AssetBundleManager.Instance.ClearBundleMap();
			#if !UNITY_ANDROID || UNITY_EDITOR
			AssetBundleManager.Instance.ClearAudioMap();
			#endif

			Resources.UnloadUnusedAssets();
			System.GC.Collect();

			if (!string.IsNullOrEmpty(_bundleName)) {
				var prefab = ResourceLoadUtils.Load<GameObject>("SceneInitObjects/" + _bundleName);
				if (prefab) {
					GameObject obj = Instantiate(prefab);
					obj.name = obj.name.Replace("(Clone)", "");
					DontDestroyOnLoad(obj);
					_preLoadObject = obj;
				}
			}

			StartLoadNextScene();
		}
	}

	private void LoadPageCompleteCallback()
	{
		_loadPageComplete = true;
	}

	#endregion

	#region 载入目标场景

	// 下一场景加载完成后的回调
	public delegate void NextSceneCallback();

	NextSceneCallback _nextSceneHandleData;

	public void AddNextSceneHandler(NextSceneCallback handler)
	{
		_nextSceneHandleData += handler;
	}

	public void RemoveNextSceneHandler(NextSceneCallback handler)
	{
		_nextSceneHandleData -= handler;
	}

	string _nextScene = "";
	string _bundleName = "";

	public string GetNextScene()
	{
		return _nextScene;
	}

	// 开始加载下一场景
	void StartLoadNextScene()
	{
		UnloadLastSceneAssetBundle();

		#if !UNITY_EDITOR
		if (ConfigurationController.Instance.BuildSceneAsBundle)
			StartCoroutine (StartLoadNextSceneSub ());
		else
			LoadNextScene ();
		#else
		LoadNextScene();
		#endif
	}

	void LoadNextScene()
	{
		AsyncOperation ao = SceneManager.LoadSceneAsync(_nextScene);
		ao.allowSceneActivation = false;
		StartCoroutine(LoadNextScene(ao, Time.timeSinceLevelLoad));
	}

	AssetBundle _sceneAssetBundle = null;

	IEnumerator StartLoadNextSceneSub()
	{
		string path = "file://" + Application.persistentDataPath + "/AssetBundle/" + MiscUtils.GetCurrentPlatform() + "/scenes/" + _nextScene + ".unity3d";
		WWW www = WWW.LoadFromCacheOrDownload(path, 0);
		yield return www;

		if (string.IsNullOrEmpty(www.error)) {
			_sceneAssetBundle = www.assetBundle;
		} else {
			Debug.LogWarning(www.error);
		}
		LoadNextScene();
	}

	void UnloadLastSceneAssetBundle()
	{
		if (_sceneAssetBundle != null)
			_sceneAssetBundle.Unload(true);
	}

	// 加载下一场景
	private static GameObject _preLoadObject;

	public void DeletePreLoadObject()
	{
		if (_preLoadObject) {
			Destroy(_preLoadObject);
		}
	}

	IEnumerator LoadNextScene(AsyncOperation ao, float startTime)
	{
		while (true) {
			if (_progress < 1f) {
				// ao.progress到0.9就卡住了，所以大约在0.8之前读取ao.progress，之后模拟进度直到100%
				float threshold = .8f;
				if (_progress < threshold) {
					RefreshProgress(ao.progress);
				} else {
					RefreshProgress(1f);
				}
				yield return null;
			} else {

				if (!_dontDestroy)
					PageManager.Instance.DestroyCurrentPages();

				yield return null;
				
				if (_nextSceneHandleData != null) {
					_nextSceneHandleData();
					_nextSceneHandleData = null;
				}

				while (!_loadPageComplete) {
					yield return null;
				}

				ao.allowSceneActivation = true;

				while (SceneManager.GetActiveScene().name == LoadingScene) {
					yield return null;
				}

				Resources.UnloadUnusedAssets();
				System.GC.Collect();

				GeneralUIManager.RefreshSortingOrder();

				_isLoading = false;
				break;
			}
		}
	}

	#endregion

	#region Progress

	float _progress = 0;
    private Image _progressSlider = null;
    private float _maxProgressBarWidth;
    // 初始化进度条相关参数
    void InitProgress()
	{
		_progress = 0;
        if (_progressSlider == null)
        {
            _progressSlider = GameObject.Find("Canvas/LoadingProgress").GetComponent<Image>();
            _maxProgressBarWidth = _progressSlider.rectTransform.sizeDelta.x;
        }
			
	}

	// 刷新进度条
	void RefreshProgress(float progress)
	{
		var delta = Mathf.Max(progress - _progress, 0.0001f);
		#if UNITY_EDITOR
		if (DebugConfigController.Instance._Debug)
			_progress += delta;
		else {
		#endif
			var maxDelta = Mathf.Max(Time.deltaTime * .5f, delta * .1f);
			_progress += Mathf.Min(delta, maxDelta);
		#if UNITY_EDITOR
		}
#endif
        if (_progressSlider)
            UpdateProgress(_progress);
	}

    private void UpdateProgress(float progress)
    {
        _progressSlider.rectTransform.sizeDelta = new Vector2(progress * _maxProgressBarWidth, _progressSlider.rectTransform.sizeDelta.y);
    }
    #endregion

}
