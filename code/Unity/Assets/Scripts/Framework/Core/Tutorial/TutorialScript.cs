using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

namespace Snaplingo.Tutorial
{
	[JsonClassAttribute]
	public abstract class TutorialAction
	{
		[JsonAttribute("delay", "", "延迟")]
		public float _delay = 0f;

		protected TutorialManager _tutorialManager;

		bool _hasInit = false;
		Coroutine _startCoroutine = null;

		public void Start(TutorialManager manager)
		{
			_tutorialManager = manager;
			_startCoroutine = _tutorialManager.StartCoroutine(Start());
		}

		IEnumerator Start()
		{
			if (_delay > 0) {
				yield return new WaitForSeconds(_delay);
			}

			Init();
			_hasInit = true;
		}

		public void Stop()
		{
			if (_hasInit)
				Cancel();
			else {
				if (_startCoroutine != null) {
					_tutorialManager.StopCoroutine(_startCoroutine);
					_startCoroutine = null;
				}
			}
		}

		protected abstract void Init();

		protected abstract void Cancel();
	}

	[JsonClassAttribute]
	public abstract class TutorialEvent
	{
		public string _nextTag = "";
		[JsonAttribute("nTag", "", "func: GetTagLabels")]
		public List<string> _switchTags = new List<string>();

		public virtual string[] GetTagLabels()
		{
			return new string[]{ "跳转标签" };
		}

		protected TutorialManager _tutorialManager;

		protected Action<TutorialEvent> _shootEvent;

		public virtual void Init(TutorialManager manager, Action<TutorialEvent> shootEvent)
		{
			_tutorialManager = manager;
			_shootEvent = shootEvent;
		}

		public abstract void Cancel();

		public void Shoot()
		{
			Shoot(0);
		}

		public void Shoot(int index)
		{
			if (_switchTags.Count > index)
				_nextTag = _switchTags[index];
			if (_shootEvent != null) {
				_shootEvent(this);
				_shootEvent = null;
			}
		}
	}

	public class TutorialStep
	{
		[JsonAttribute("tag")]
		public string _tag;
		[JsonAttribute("actions")]
		public List<TutorialAction> _actions = new List<TutorialAction>();
		[JsonAttribute("events")]
		public List<TutorialEvent> _events = new List<TutorialEvent>();

		Action<string> _shootEvent = null;

		public void Init(TutorialManager manager, Action<string> shootEvent)
		{
			for (int i = 0; i < _actions.Count; ++i) {
				_actions[i].Start(manager);
			}

			for (int i = 0; i < _events.Count; ++i) {
				_events[i].Init(manager, ShootEventsCallback);
			}

			_shootEvent = shootEvent;
		}

		void ShootEventsCallback(TutorialEvent evt)
		{
			Clear();

			int idx = _events.IndexOf(evt);
			if (idx >= 0) {
				if (_shootEvent != null) {
					_shootEvent(evt._nextTag);
					if (evt._nextTag != _tag)
						_shootEvent = null;
				}
			}
		}

		public void Clear()
		{
			for (int i = 0; i < _actions.Count; ++i) {
				_actions[i].Stop();
			}

			for (int i = 0; i < _events.Count; ++i) {
				_events[i].Cancel();
			}
		}
	}

	public class TutorialScript
	{
		[JsonAttribute("sTag")]
		public string _startTag = "";
		[JsonAttribute("step")]
		public List<TutorialStep> _steps = new List<TutorialStep>();
		[JsonAttribute("events")]
		public List<TutorialEvent> _events = new List<TutorialEvent>();
		[JsonAttribute("actions")]
		public List<TutorialAction> _actions = new List<TutorialAction>();

		TutorialManager _tutorialManager;
		TutorialStep _currentStep = null;
		Action<string> _shootEvent = null;

		public bool Execute(TutorialManager manager, string startTag = "")
		{
			for (int i = 0; i < _actions.Count; ++i) {
				_actions[i].Start(manager);
			}

			for (int i = 0; i < _events.Count; ++i) {
				_events[i].Init(manager, ShootEventsCallback);
			}

			if (_steps.Count == 0) {
				LogManager.LogWarning("Warning! Not any step in script");
				return false;
			}

			_tutorialManager = manager;
			if (string.IsNullOrEmpty(startTag))
				startTag = _startTag;
			if (!PerformStep(startTag))
				PerformStep(_steps[0]._tag);

			return true;
		}

		void ShootEventsCallback(TutorialEvent evt)
		{
			Clear();

			int idx = _events.IndexOf(evt);
			if (idx >= 0) {
				if (_shootEvent != null) {
					_shootEvent(evt._nextTag);
					if (evt._nextTag != _startTag)
						_shootEvent = null;
				}
			}
		}

		public void Clear()
		{
			for (int i = 0; i < _actions.Count; ++i) {
				_actions[i].Stop();
			}

			for (int i = 0; i < _events.Count; ++i) {
				_events[i].Cancel();
			}
		}

		public TutorialStep GetCurrentStep()
		{
			return _currentStep;
		}

		TutorialStep GetStep(string tag)
		{
			for (int i = 0; i < _steps.Count; ++i) {
				if (_steps[i]._tag == tag) {
					return _steps[i];
				}
			}

			return null;
		}

		public bool HasStep(string tag)
		{
			return GetStep(tag) != null;
		}

		bool PerformStep(string tag)
		{
			var step = GetStep(tag);
			if (step != null) {
				_currentStep = step;
				step.Init(_tutorialManager, CheckPerformStep);
				return true;
			} else
				return false;
		}

		void CheckPerformStep(string tag)
		{
			if (!PerformStep(tag))
				End();
		}

		public void Stop()
		{
			if (_currentStep != null) {
				_currentStep.Clear();
			}
		}

		void End()
		{
			_tutorialManager.End();
		}

		public static TutorialScript LoadFromJson(string json)
		{
			object script = null;
			JsonData data = JsonMapper.ToObject(json);
			JsonUtils.ReadFromJson(ref script, typeof(TutorialScript), data);
			return script == null ? new TutorialScript() : (TutorialScript)script;
		}

		#if UNITY_EDITOR
		public string SaveAsJson()
		{
			return ((JsonData)JsonUtils.GetJsonData(this)).ToJson();
		}
		#endif
	}
}
