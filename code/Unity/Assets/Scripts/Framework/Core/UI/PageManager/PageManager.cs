using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using StructUtils;

namespace Snaplingo.UI
{
	[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
	public class PageManager : MonoBehaviour
	{
		static PageManager _instance = null;

		public static PageManager Instance {
			get {
				if (_instance == null) {
					GameObject go = new GameObject();
					_instance = go.AddComponent<PageManager>();
					go.name = _instance.GetType().ToString();
					_instance.Init(go);
				}

				return _instance;
			}
		}

		// 初始化Page必须的Canvas、CanvasScaler组件和EventSystem
		void Init(GameObject go)
		{
			DontDestroyOnLoad(go);
			var canvas = go.GetComponent<Canvas>();
			var canvasScaler = go.GetComponent<CanvasScaler>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.referenceResolution = ConfigurationController.Instance._CanvasResolution;
			canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
			StartCoroutine(CheckCreateEventSystem());
		}

		IEnumerator CheckCreateEventSystem()
		{
			yield return null;

			while (true) {
				if (EventSystem.current == null) {
					GameObject evt = new GameObject("EventSystem");
					evt.AddComponent<EventSystem>();
					evt.AddComponent<StandaloneInputModule>();
				}

				yield return new WaitForSeconds(2f);
			}
		}

		void Awake()
		{
			// 处理Hierarchy中已存在的PageManager
			if (_instance == null) {
				_instance = this;
				Init(gameObject);

				var presetPage = GetComponentsInChildren<Page>(true);
				if (presetPage != null && presetPage.Length > 0) {
					for (int i = 1; i < presetPage.Length; ++i) {
						DestroyPage(presetPage[i]);
					}
					if (State == CommandState.Common)
						OpenPage(presetPage[0].Name);
					else
						DestroyPage(presetPage[0]);
				}
			} else {
				LogManager.LogWarning("Warning! Destroy an instance of PageManager: " + gameObject.name);
				Destroy(gameObject);
				return;
			}
		}

		public enum CommandState
		{
			Common,
			WaitingForCommand,
		}

		static CommandState _state = CommandState.Common;

		public static CommandState State {
			get { return _state; }
			set { _state = value; }
		}

		// 当前Page
		public Page CurrentPage { get; set; }

		// Page历史, 用于返回时检索. Key: page name; Value: scene name and page.
		static OrderedDictionary<string, Tuple<string, Page>> _pageHistory = new OrderedDictionary<string, Tuple<string, Page>>();

		// 打开上一个Page
		public void OpenLastPage()
		{
			if (_pageHistory.Count > 1) {
				var pair = _pageHistory.PairAt(_pageHistory.Count - 2);
				var pageName = pair._item1;
				OpenPage(pageName);
			}
		}

		string GetCurrentSceneName()
		{
			var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			if (sceneName == LoadSceneManager.LoadingScene)
				sceneName = LoadSceneManager.Instance.GetNextScene();
			return sceneName;
		}

		void RefreshHistory(string pageName, Page page)
		{
			int index = _pageHistory.IndexOf(pageName);
			if (index >= 0) {
				// 复用之前的Page，并删除该Page之后的所有Page
				var count = _pageHistory.Count - index - 1;
				if (count > 0) {
					for (int i = index + 1; i < _pageHistory.Count; ++i) {
						var p = _pageHistory.PairAt(i)._item2._item2;
						if (p != null)
							DestroyPage(p);
					}
				}
				_pageHistory.RemoveRange(index, count + 1);
			}
			var current = GetCurrentSceneName();
			_pageHistory.Add(pageName, new Tuple<string, Page>(current, page));

			// PrintHistory();
		}

		void PrintHistory()
		{
			Debug.Log(MiscUtils.AssembleObjectsAsString<string>(_pageHistory.Keys, ","));
			var values = new List<string>();
			for (int i = 0; i < _pageHistory.Values.Count; ++i) {
				values.Add(_pageHistory.Values[i]._item1);
			}
			Debug.Log(MiscUtils.AssembleObjectsAsString<string>(values, ","));
		}

		// 打开特定类型的Page
		public void OpenPage(string pageName, bool dontDestroy = false, Action finishCallback = null, bool isCurrentCommand = false)
		{
			if (CurrentPage != null && CurrentPage.Name == pageName)
				return;

			if (_pageHistory.ContainsKey(pageName) && _pageHistory[pageName]._item1 != GetCurrentSceneName()) {
				LoadSceneManager.Instance.LoadSceneAsyncAndOpenPage(_pageHistory[pageName]._item1, pageName, dontDestroy, finishCallback);
			} else {
				OpenPageSub(pageName, dontDestroy, finishCallback, isCurrentCommand);
			}
		}

		public void OpenPageSub(string pageName, bool dontDestroy = false, Action finishCallback = null, bool isCurrentCommand = false)
		{
			if (!isCurrentCommand && State == CommandState.WaitingForCommand) {
				return;
			}

			if (isCurrentCommand)
				State = CommandState.Common;
			// 从内存中获取Page，如果没有则实例化一个新的
			if (!string.IsNullOrEmpty(pageName)) {
				var page = GetPage(pageName);
				if (page == null) {
					// 获取page类型和附加node的信息
					var s = pageName.Split('|');
					var pageType = s[s.Length - 1];
					var nodes = new List<string>();
					for (int i = 0; i < s.Length - 1; ++i)
						nodes.Add(s[i]);
					page = CreatePage(pageType, nodes);
					page.gameObject.name = page.Name = pageName;
					page.SetCanvas(GetComponent<Canvas>());
				}
				if (page != null) {
					if (!dontDestroy) {
						page.InitNodes(CurrentPage);
						DestroyCurrentPages(page);
					} else {
						page.InitNodes(null);
					}
					// 刷新历史
					RefreshHistory(pageName, page);
					CurrentPage = page;
					// 等一帧再执行调整宽高的操作
					StartCoroutine(OpenPageCoroutine(page, finishCallback));
				}
			}
		}

		IEnumerator OpenPageCoroutine(Page page, Action finishCallback)
		{
			yield return null;

			(page.transform as RectTransform).sizeDelta = (transform as RectTransform).sizeDelta;
			page.Open();

			if (finishCallback != null)
				finishCallback();
		}

		// 根据Page子类类型打开特定的Page
		public void OpenPage<T>(bool dontDestroy = false, Action finishCallback = null, bool isCurrentCommand = false) where T : Page
		{
			OpenPage(GetPageType<T>(), dontDestroy, finishCallback, isCurrentCommand);
		}

		string EmptyPage = "EmptyPage";

		public void OpenPageWithNodes(Type[] nodeTypes, bool dontDestroy = false, string pageType = "", Action finishCallback = null, bool isCurrentCommand = false)
		{
			if (nodeTypes == null || nodeTypes.Length == 0)
				return;

			pageType = string.IsNullOrEmpty(pageType) ? EmptyPage : pageType;
			List<string> names = new List<string>();
			for (int i = 0; i < nodeTypes.Length; ++i) {
				names.Add(nodeTypes[i].ToString());
			}
			names.Add(pageType);
			var pageName = MiscUtils.AssembleObjectsAsString<string>(names, "|");
			OpenPage(pageName, dontDestroy, finishCallback, isCurrentCommand);
		}

		public void DestroyCurrentPages(Page exceptionPage = null)
		{
			for (int i = 0; i < _pageHistory.Count; ++i) {
				var page = _pageHistory.PairAt(i)._item2._item2;
				if (page != null && page != exceptionPage) {
					DestroyPage(page);
				}
			}
		}

		// 销毁Page
		void DestroyPage(Page page)
		{
			page.Close();
			Destroy(page.gameObject);
		}

		// 创建一个新的特定类型的Page
		Page GetPage(string pageName)
		{
			var pages = GetComponentsInChildren<Page>(true);
			Page page = null;
			if (pages != null) {
				for (int i = 0; i < pages.Length; ++i) {
					if (pages[i].Name == pageName) {
						page = pages[i];
						break;
					}
				}
			}

			return page;
		}

		Page CreatePage(string pageType, List<string> nodes)
		{
			Page page = null;
			GameObject go = InstantiatePage(pageType);

			if (go) {
				UIUtils.AttachAndReset(go, transform);
				page = go.GetComponent<Page>();

				for (int i = 0; i < nodes.Count; ++i)
					page.AddNode(Type.GetType(nodes[i]), true);
			} else {
				Debug.LogError("Error! Cannot find page: " + pageType);
			}
			return page;
		}

		GameObject InstantiatePage(string pageType)
		{
			var path = "UI/Pages/" + pageType;
			var prefab = ResourceLoadUtils.Load<GameObject>(path, true);
			if (prefab != null)
				return Instantiate(prefab);

			return null;
		}

		string GetPageType<T>() where T : Page
		{
			return typeof(T).ToString();
		}

	}
}
