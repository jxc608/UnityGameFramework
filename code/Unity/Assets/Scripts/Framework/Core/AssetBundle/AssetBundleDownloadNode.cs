using UnityEngine;
using System;
using Snaplingo.UI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleDownloadNode : Node, IProgress
{
	private Slider m_ProgressBar;
	private Text m_ProgressLabel;

	public override void Init(params object[] args)
	{
		base.Init(args);

		m_ProgressBar = transform.Find("Progress").GetComponent<Slider>();
		m_ProgressLabel = transform.Find("Progress/ProgressLabel").GetComponent<Text>();

		#if UNITY_EDITOR
		if (DebugConfigController.Instance.TestBundleInEditor)
			AssetBundleManager.Instance.CheckIfTesting(this, LoadModule, ShouldCheckBundles);
		else
			LoadModule();
		#else
		AssetBundleManager.Instance.CheckIfTesting (this, LoadModule, ShouldCheckBundles);
		#endif
	}

	static readonly float MinProgress = .05f;

	void Update()
	{
		UpdateShouldCheckUpk();
		
		m_ProgressLabel.text = _currentText;
		m_ProgressBar.value = MinProgress + (1f - MinProgress) * ((float)_currentInSize / (float)_currentOutSize);
	}

	long _currentInSize = 0;
	long _currentOutSize = 0;
	GetProgressText _currentFunc = null;
	GetProgressText2 _currentFunc2 = null;
	string _currentText = "";

	public void ResetProgress()
	{
		SetProgress(0, 0, "");
	}

	public void SetTextFunc(GetProgressText func)
	{
		_currentFunc = func;
		_currentFunc2 = null;
	}

	public void SetTextFunc2(GetProgressText2 func)
	{
		_currentFunc2 = func;
		_currentFunc = null;
	}

	public void SetProgress(long inSize, long outSize, string extra = "")
	{
		_currentInSize = inSize;
		_currentOutSize = outSize;
		if (_currentFunc != null)
			_currentText = _currentFunc(inSize, outSize);
		if (_currentFunc2 != null)
			_currentText = _currentFunc2(inSize, outSize, extra);
	}

	void LoadModule()
	{
		LoadSceneManager.Instance.LoadSceneAsyncAndOpenPage(ConfigurationController.Instance._FirstScene, 
		                                                     ConfigurationController.Instance._FirstScenePage);
	}

	bool _shouldCheckBundles = false;
	bool _available = true;

	void ShouldCheckBundles()
	{
		if (ConfigurationController.Instance.BuildAssetBundle || ConfigurationController.Instance.BuildSceneAsBundle)
			_shouldCheckBundles = true;
		else
			LoadModule();
	}

	void UpdateShouldCheckUpk()
	{
		if (_available) {
			if (_shouldCheckBundles) {
				LogManager.Log("Start Checking Upk...");
				_shouldCheckBundles = false;
				CheckUpk();
				_available = false;
				StartCoroutine(ResetAvailable(2f));
			}
		}
	}

	IEnumerator ResetAvailable(float time)
	{
		yield return new WaitForSeconds(time);
		_available = true;
	}

	void CheckUpk()
	{
		UpkManager.Instance.CheckUpk(this, delegate {
			UpdateBundleManager.Instance.StartCheckLocalFiles(this, LoadModule);
		}, delegate(UpkManager.Status status) {
			LogManager.Log("Check Upk fail, status: " + status.ToString());
			ShouldCheckBundles();
		});
	}
}
