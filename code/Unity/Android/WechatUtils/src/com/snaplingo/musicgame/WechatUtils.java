package com.snaplingo.musicgame;

import android.content.Context;

import com.tencent.mm.opensdk.modelmsg.SendMessageToWX;
import com.tencent.mm.opensdk.modelmsg.WXImageObject;
import com.tencent.mm.opensdk.modelmsg.WXMediaMessage;
import com.tencent.mm.opensdk.openapi.IWXAPI;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;

public class WechatUtils {

	public static IWXAPI api = null;
	
	static void RegisterToWechat (Context context, String appId) {
		api = WXAPIFactory.createWXAPI(context, appId, true);
		api.registerApp(appId);
	}
	
	static boolean IsWechatInstalled () {
		return api.isWXAppInstalled();
	}
	
	static boolean IsWechatAppSupportAPI() {
		return api.isWXAppSupportAPI();
	}
	
	static void ShareImageWechat(int scene, byte[] imgData, byte[] thumbData) {
		WXImageObject imgObj = new WXImageObject(imgData);
		WXMediaMessage msg = new WXMediaMessage();
		msg.mediaObject = imgObj;
		msg.thumbData = thumbData;

		SendMessageToWX.Req req = new SendMessageToWX.Req();
		req.transaction = BuildTransaction("img");
		req.message = msg;
		req.scene = scene;
		api.sendReq(req);
	}

	static String BuildTransaction(final String type) {
		return (type == null) ? String.valueOf(System.currentTimeMillis()) : type + System.currentTimeMillis();
	}
}
