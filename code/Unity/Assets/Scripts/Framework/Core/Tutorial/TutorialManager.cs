using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Snaplingo.Tutorial
{
	public class TutorialManager : Manager
	{
		// 注意，请不要在Awake中使用Instance，否则会出现死循环
		public static TutorialManager Instance { get { return GetManager<TutorialManager>(); } }

		public static string TutorialScriptPath = "Tutorial/Scripts/";

		#region script manage

		TutorialScript _currentScript = null;
		Action _callback;

		public bool StartTutorial(string name, Action callback = null)
		{
			if (_currentScript != null) {
				LogManager.LogWarning("Warning! There's already a tutorial running.");
				return false;
			}

			var asset = ResourceLoadUtils.Load<TextAsset>(Path.Combine(TutorialScriptPath, name), true);
			if (asset != null) {
				_callback = callback;
				LogManager.Log("Start Tutorial: " + name);

				_currentScript = TutorialScript.LoadFromJson(asset.text);
				_currentScript.Execute(this);
				return true;
			}

			return false;
		}

		public TutorialScript GetCurrentScript()
		{
			return _currentScript;
		}

		public void StopTutorial()
		{
			LogManager.Log("End Tutorial");
			if (_currentScript != null)
				_currentScript.Stop();
			_currentScript = null;
			_callback = null;
		}

		public void End()
		{
			_currentScript = null;
			if (_callback != null) {
				var callback = _callback;
				_callback = null;
				callback();
			}
		}

		#endregion
	}
}