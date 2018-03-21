package com.snaplingo.musicgame;

import android.content.Intent;
import android.util.Log;

import com.unity3d.player.UnityPlayerActivity;
import com.tencent.tauth.*;

public class UnityCustomActivity extends UnityPlayerActivity {
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		Log.i("Unity", "onActivityResult: requestCode: " + requestCode + ", resultCode: " + resultCode);
		Tencent.onActivityResultData(requestCode, resultCode, data, QQUtils.mListener);
	}

}
