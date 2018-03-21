using LitJson;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Snaplingo.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class Page : MonoBehaviour
	{
		public Canvas RootCanvas { get; set; }

		private string _name;

		public string Name {
			get {
				if (string.IsNullOrEmpty(_name)) {
					_name = gameObject.name.Replace("(Clone)", "");
				}

				return _name;
			}
			set {
				_name = value;
			}
		}

		public List<GameObject> _nodePrefabs = new List<GameObject>();
		List<Node> _nodes = new List<Node>();
		bool _hasInitNodes = false;

		public void SetCanvas(Canvas canvas)
		{
			RootCanvas = canvas;
		}

		public void InitNodes(Page lastPage)
		{
			if (_hasInitNodes)
				return;
			_hasInitNodes = true;
			
			_nodes.Clear();
			// Hierarchy中已存在的节点
			for (int i = 0; i < transform.childCount; ++i) {
				var trans = transform.GetChild(i);
				Node node = trans.GetComponent<Node>();
				if (node != null) {
					AddNodeToList(node, false);
				}
			}

			// 从Prefab中创建的节点
			for (int i = 0; i < _nodePrefabs.Count; ++i) {
				Node prefabNode = _nodePrefabs[i].GetComponent<Node>();
				if (GetNode(prefabNode.GetType()) != null)
					continue;

				var go = Instantiate(_nodePrefabs[i]) as GameObject;
				Node node = go.GetComponent<Node>();
				if (node != null) {
					AddNodeToList(node, true, _nodePrefabs[i]);
				}
			}

			gameObject.SetActive(false);
		}

		void AddNodeToList(Node node, bool attach, GameObject prefab = null)
		{
			if (attach)
				UIUtils.AttachAndReset(node.gameObject, transform, prefab);
			if (!_nodes.Contains(node)) {
				node.RootCanvas = RootCanvas;
				node.ParentPage = this;
				_nodes.Add(node);
			}
		}

		public virtual void Open()
		{
			Show();
			for (int i = 0; i < _nodes.Count; ++i) {
				if (_nodes[i].gameObject.activeSelf)
					_nodes[i].Open();
			}
		}

		// 强制初始化所有Node，慎重使用
		public void InitAllNodes()
		{
			for (int i = 0; i < _nodes.Count; ++i) {
				_nodes[i].CheckInit();
			}
		}

		public Node AddNode(Type type, bool open, params object[] args)
		{
			Node node = GetNode(type);
			if (node == null) {
				var prefab = GetNodePrefab(type.ToString());
				GameObject go = null;
				if (prefab != null)
					go = Instantiate(prefab);
				if (go) {
					node = go.GetComponent(type) as Node;
					AddNodeToList(node, true, prefab);
				} else {
					Debug.LogError("Error! Cannot find node: " + type.ToString());
					return null;
				}
			}

			if (!node.HasInit)
				node.Init(args);
			if (open)
				node.Open();
			else
				node.Close();
			return node;
		}

		GameObject GetNodePrefab(string type)
		{
			var path = "UI/Nodes/" + type;
			return ResourceLoadUtils.Load<GameObject>(path, true);
		}

		public Node AddNode<T>(bool open, params object[] args) where T : Node
		{
			return AddNode(typeof(T), open, args);
		}

		public Node GetNode(Type type)
		{
			for (int i = 0; i < _nodes.Count; ++i) {
				if (_nodes[i].GetType() == type) {
					return _nodes[i];
				}
			}

			return null;
		}

		public T GetNode<T>() where T : Node
		{
			return (T)GetNode(typeof(T));
		}

		public void RemoveNodes(List<Node> nodes)
		{
			if (nodes != null) {
				for (int i = 0; i < nodes.Count; ++i) {
					_nodes.Remove(nodes[i]);
				}
			}
		}

		public virtual void Refresh()
		{
			for (int i = 0; i < _nodes.Count; ++i) {
				_nodes[i].Refresh();
			}
		}

		public virtual void Close()
		{
			Hide();
			for (int i = 0; i < _nodes.Count; ++i) {
				if (_nodes[i].IsShown())
					_nodes[i].Close();
			}
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}
