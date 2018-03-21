using UnityEngine;
using System;
using Snaplingo.UI;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AssetBundleDownload : MonoBehaviour
{
	void Start()
	{
		PageManager.Instance.OpenPageWithNodes(new Type[] { typeof(AssetBundleDownloadNode) });
	}
}
