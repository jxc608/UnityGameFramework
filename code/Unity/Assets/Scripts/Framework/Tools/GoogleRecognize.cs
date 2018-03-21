using System;
using UnityEngine;
using LitJson;
using System.Collections;
using System.Collections.Generic;

public class GoogleRecognize : Manager
{
    public static GoogleRecognize Instance { get { return GetManager<GoogleRecognize>(); } }

    private string m_URL = "";
    public void GetTextFromServer(AudioClip clip, Action<string> callback)
    {
        StartCoroutine(PostGoo(callback));
    }

    IEnumerator PostGoo(Action<string> callback)
	{
        WWWForm form = new WWWForm();

        WWW post = new WWW(m_URL, form);
        yield return post;

        if (!string.IsNullOrEmpty(post.error))
        {
            print("Error downloading: " + post.error);
        }
        else
        {
            callback(post.text);
        }
    }
}

