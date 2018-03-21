package com.snaplingo.musicgame.wxapi;

import com.tencent.mm.opensdk.constants.ConstantsAPI;
import com.tencent.mm.opensdk.modelbase.BaseReq;
import com.tencent.mm.opensdk.modelbase.BaseResp;
import com.tencent.mm.opensdk.openapi.IWXAPIEventHandler;
import com.unity3d.player.UnityPlayer;
import com.snaplingo.musicgame.WechatUtils;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

public class WXEntryActivity extends Activity implements IWXAPIEventHandler {
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        try {
        	WechatUtils.api.handleIntent(getIntent(), this);
        } catch (Exception e) {
        	e.printStackTrace();
        }
    }

	@Override
	protected void onNewIntent(Intent intent) {
		super.onNewIntent(intent);
		
		setIntent(intent);
		WechatUtils.api.handleIntent(intent, this);
	}

	@Override
	public void onReq(BaseReq req) {
		Log.i("Unity", "basereq.getType = " + req.getType());
		switch (req.getType()) {
		case ConstantsAPI.COMMAND_GETMESSAGE_FROM_WX:
			break;
		case ConstantsAPI.COMMAND_SHOWMESSAGE_FROM_WX:
			break;
		default:
			break;
		}
		finish();
	}
	
	@Override
	public void onResp(BaseResp resp) {
        UnityPlayer.UnitySendMessage("ShareManager", "WechatCallBack", "" + resp.errCode);
		switch (resp.errCode) {
		case BaseResp.ErrCode.ERR_OK:
			break;
		case BaseResp.ErrCode.ERR_USER_CANCEL:
			break;
		case BaseResp.ErrCode.ERR_AUTH_DENIED:
			break;
		case BaseResp.ErrCode.ERR_UNSUPPORT:
			break;
		default:
			break;
		}
		finish();
	}
}