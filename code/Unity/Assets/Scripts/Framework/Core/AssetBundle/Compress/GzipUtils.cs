using System.IO;
using System.IO.Compression;
using System;
using System.Text;

public static class GZipUtil
{
	public static byte[] Zip(string plain)
	{
		return Zip(Encoding.UTF8.GetBytes(plain));
	}

	public static byte[] Zip(byte[] byteArray)
	{
		MemoryStream ms = new MemoryStream();
		GZipStream sw = new GZipStream(ms, CompressionMode.Compress);
		try {
			sw.Write(byteArray, 0, byteArray.Length);
			sw.Close();
			byteArray = ms.ToArray();
			return byteArray;
		} catch (Exception e) {
			UnityEngine.Debug.LogError(e.Message);
			UnityEngine.Debug.LogError(e.StackTrace);
			return null;
		} finally {
			sw.Close();
			ms.Close();
			sw.Dispose();
			ms.Dispose();
		}
	}

	public static byte[] UnZip(byte[] byteArray)
	{
		MemoryStream ms = new MemoryStream(byteArray);
		GZipStream sr = new GZipStream(ms, CompressionMode.Decompress);
		MemoryStream outBuffer = new MemoryStream();
		try {
			byteArray = new byte[1024];
			while (true) {
				int rByte = sr.Read(byteArray, 0, byteArray.Length);
				if (rByte <= 0)
					break;
				else
					outBuffer.Write(byteArray, 0, rByte);
			}
			return outBuffer.ToArray();
		} catch (Exception e) {
			UnityEngine.Debug.LogError(e.Message);
			UnityEngine.Debug.LogError(e.StackTrace);
			return null;
		} finally {
			sr.Close();
			ms.Close();
			sr.Dispose();
			ms.Dispose();
			outBuffer.Close();
			outBuffer.Dispose();
		}
	}

	public static string UnZipToString(byte[] byteArray)
	{
		byteArray = UnZip(byteArray);
		StringBuilder sb = new StringBuilder(byteArray.Length);
		for (int i = 0; i < byteArray.Length; i++) {
			sb.Append((char)byteArray[i]);
		}
		return sb.ToString();
	}
}