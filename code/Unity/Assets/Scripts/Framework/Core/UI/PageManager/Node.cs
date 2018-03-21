using LitJson;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Snaplingo.UI
{
	public abstract class Node : MonoBehaviour
	{
		public Canvas RootCanvas { get; set; }

		public Page ParentPage { get; set; }

		bool _hasInit = false;

		public bool HasInit { get { return _hasInit; } }

		public virtual void Init(params object[] args)
		{
			Show();
			_hasInit = true;
			AudioManager.Instance.InitBtnClick(gameObject);
		}

		public void CheckInit()
		{
			if (!_hasInit) {
				Init();
			}
		}

		public virtual void Open()
		{
			Show();
			CheckInit();
		}

		public static T AddAndOpen<T>(Page page, bool open, params object[] args) where T : Node
		{
			return page.AddNode<T>(open, args) as T;
		}

		public virtual void Refresh()
		{
		}

		public bool IsShown()
		{
			return gameObject.activeSelf;
		}

		void Show()
		{
			gameObject.SetActive(true);
		}

		void Hide()
		{
			gameObject.SetActive(false);
		}

		public virtual void Close(bool destroy = false)
		{
			Hide();

			if (destroy) {
				ParentPage.RemoveNodes(new List<Node>() { this });
				Destroy(this.gameObject);
			}
		}
	}
}
