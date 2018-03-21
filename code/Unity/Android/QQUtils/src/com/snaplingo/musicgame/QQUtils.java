package com.snaplingo.musicgame;

import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;

import com.tencent.tauth.*;
import com.tencent.connect.share.*;
import com.unity3d.player.UnityPlayer;

public class QQUtils {

	public static Tencent mTencent = null;
    public static IUiListener mListener = null;
	private static File mTempFile;
    
    enum Result {
    	ERROR,
    	COMPLETED,
    	CANCEL,
    }
	
	static void RegisterToQQ(Context context, String appId) {
		mTencent = Tencent.createInstance(appId, context);
        //显示分享的状态接口
        mListener = new IUiListener() {
			@Override
			public void onCancel() {
				UnityPlayer.UnitySendMessage("ShareManager", "QQCallback", "" + Result.CANCEL.ordinal());
				ClearTempFile();
			}
			
			@Override
			public void onError(UiError arg0) {
				UnityPlayer.UnitySendMessage("ShareManager", "QQCallback", "" + Result.ERROR.ordinal());
				ClearTempFile();
			}
			
			@Override
			public void onComplete(Object arg0) {
				UnityPlayer.UnitySendMessage("ShareManager", "QQCallback", "" + Result.COMPLETED.ordinal());
				ClearTempFile();
			}
		};
	}
	
	static void ClearTempFile() {
		if (mTempFile != null && mTempFile.exists()) {
			Log.i("Unity", "clear temp file");
			mTempFile.delete();
			mTempFile = null;
		}
	}
	
	static boolean IsQQInstalled() {
		return true;
	}
	
	static boolean IsQQAppSupportAPI() {
		return true;
	}
	
	static void ShareImageQQ(Activity activity, byte[] imgData) {
	    Bundle params = new Bundle();
		CreateTempFileByData(imgData);
	    params.putString(QQShare.SHARE_TO_QQ_IMAGE_LOCAL_URL, mTempFile.getAbsolutePath());
	    params.putString(QQShare.SHARE_TO_QQ_APP_NAME, GetAppName(activity));
	    params.putInt(QQShare.SHARE_TO_QQ_KEY_TYPE, QQShare.SHARE_TO_QQ_TYPE_IMAGE);
	    params.putInt(QQShare.SHARE_TO_QQ_EXT_INT, QQShare.SHARE_TO_QQ_FLAG_QZONE_ITEM_HIDE);
	    mTencent.shareToQQ(activity, params, mListener);
	}
	
	static void ShareImageQzone(Activity activity, byte[] imgData, String summary) {
	    Bundle params = new Bundle();
	    params.putInt(QzoneShare.SHARE_TO_QZONE_KEY_TYPE, QzonePublish.PUBLISH_TO_QZONE_TYPE_PUBLISHMOOD);
	    params.putString(QzoneShare.SHARE_TO_QQ_SUMMARY, summary);
	    ArrayList<String> imgList = new ArrayList<String>();
		CreateTempFileByData(imgData);
	    imgList.add(mTempFile.getAbsolutePath());
	    params.putStringArrayList(QzoneShare.SHARE_TO_QQ_IMAGE_URL, imgList);
	    mTencent.publishToQzone(activity, params, mListener);
	}
	
    static void CreateTempFileByData(byte[] bytes) {
        /**
         * 创建File对象，其中包含文件所在的目录以及文件的命名
         */
    	mTempFile = new File(Environment.getExternalStorageDirectory(), "tempImage.jpg");
        // 创建FileOutputStream对象
        FileOutputStream outputStream = null;
        // 创建BufferedOutputStream对象
        BufferedOutputStream bufferedOutputStream = null;
        try {
            // 如果文件存在则删除
            if (mTempFile.exists()) {
            	mTempFile.delete();
            }
            // 在文件系统中根据路径创建一个新的空文件
            mTempFile.createNewFile();
            // 获取FileOutputStream对象
            outputStream = new FileOutputStream(mTempFile);
            // 获取BufferedOutputStream对象
            bufferedOutputStream = new BufferedOutputStream(outputStream);
            // 往文件所在的缓冲输出流中写byte数据
            bufferedOutputStream.write(bytes);
            // 刷出缓冲输出流，该步很关键，要是不执行flush()方法，那么文件的内容是空的。
            bufferedOutputStream.flush();
        } catch (Exception e) {
            // 打印异常信息
            e.printStackTrace();
        } finally {
            // 关闭创建的流对象
            if (outputStream != null) {
                try {
                    outputStream.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
            if (bufferedOutputStream != null) {
                try {
                    bufferedOutputStream.close();
                } catch (Exception e2) {
                    e2.printStackTrace();
                }
            }
        }
    }
    
    public static String GetAppName(Context context) {
        try {
            PackageManager packageManager = context.getPackageManager();  
            PackageInfo packageInfo = packageManager.getPackageInfo(context.getPackageName(), 0);
            int labelRes = packageInfo.applicationInfo.labelRes;
            return context.getResources().getString(labelRes);
        } catch (NameNotFoundException e) {
            e.printStackTrace();  
        }
        return null;
    }
}
