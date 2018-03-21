package com.snaplingo.musicgame;

import java.net.InetAddress;
import java.net.UnknownHostException;

import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.net.Uri;

public class Utils
{
    public static String getVersionName(Context context) {
	   PackageManager packageManager = context.getPackageManager();
	   PackageInfo packInfo;
	   try {
		   packInfo = packageManager.getPackageInfo(context.getPackageName(), 0);
		   String version = packInfo.versionName;
		   return version;
	   } catch (NameNotFoundException e) {
	   }
	   
	   return "";
    }
    
    public static String getInetAddress(String host) {
        String IPAddress = "";
        InetAddress ReturnStr1 = null;
        try {
            ReturnStr1 = java.net.InetAddress.getByName(host);
            IPAddress = ReturnStr1.getHostAddress();
        } catch (UnknownHostException e) {
            e.printStackTrace();
            return IPAddress;
        }
        return IPAddress;
    }
    
    public static void refreshMediaFile(Context context, String filePath) {
        context.sendBroadcast(new Intent(Intent.ACTION_MEDIA_SCANNER_SCAN_FILE, Uri.parse("file://" + filePath)));
    }
    
}
