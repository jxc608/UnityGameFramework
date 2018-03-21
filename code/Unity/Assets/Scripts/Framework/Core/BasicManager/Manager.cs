using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class Manager : MonoBehaviour
{
	static GameObject _gameManager = null;
	static GameObject _gameManagerRemain = null;

	protected void AddToGameManager(bool dontDestroy = false)
	{
		GameObject gm = null;
		if (dontDestroy) {
			gm = _gameManagerRemain;
		} else
			gm = _gameManager;
		if (gm == null) {
			gm = new GameObject();
			if (dontDestroy) {
				gm.name = "GameManager_Remain";
				_gameManagerRemain = gm;
				DontDestroyOnLoad(gm);
			} else {
				gm.name = "GameManager";
				_gameManager = gm;
			}
		}

		transform.parent = gm.transform;
	}

	static List<Manager> _managers = new List<Manager>();

	public static T GetManager<T>() where T : Manager
	{
		return (T)GetManager(typeof(T));
	}

	public static Manager GetManager(Type type)
	{
		for (int i = 0; i < _managers.Count; ++i) {
			if (_managers[i].GetType() == type) {
				return _managers[i];
			}
		}

		GameObject go = new GameObject();
		Manager inst = go.AddComponent(type) as Manager;
		go.name = inst.GetType().ToString();
		inst.AddToGameManager(true);
		inst.Init();
		_managers.Add(inst);
		return inst;
	}

	public static void RemoveManager(Type type)
	{
		for (int i = 0; i < _managers.Count; ++i) {
			if (_managers[i].GetType() == type) {
				_managers.RemoveAt(i);
				break;
			}
		}
	}

	protected virtual void Init()
	{
	}

	protected virtual void OnDestory()
	{
		RemoveManager(this.GetType());
	}

}