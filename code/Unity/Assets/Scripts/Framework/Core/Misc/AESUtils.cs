using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public class AESUtils
{
	public static byte[] AESEncrypt(string plainText)
	{
		byte[] bKey = Encoding.UTF8.GetBytes(ConfigurationController.Instance._AesKey);
		byte[] byteArray = Encoding.UTF8.GetBytes(plainText);

		byte[] encrypt = null;
		Rijndael aes = Rijndael.Create();
		aes.GenerateIV();
		aes.Mode = CipherMode.CFB;
		try {
			using (MemoryStream mStream = new MemoryStream()) {
				using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, aes.IV), CryptoStreamMode.Write)) {
					cStream.Write(aes.IV, 0, aes.IV.Length);
					cStream.Write(byteArray, 0, byteArray.Length);
					cStream.FlushFinalBlock();
					encrypt = mStream.ToArray();
				}
			}
		} catch (Exception e) {
			UnityEngine.Debug.Log(e);
			return null;
		} finally {
			aes.Clear();
		}

		return encrypt;
	}

	public static string AESDecrypt(byte[] encryptedArray)
	{
		byte[] bKey = Encoding.UTF8.GetBytes(ConfigurationController.Instance._AesKey);
		var bIV = new byte[16];
		Array.Copy(encryptedArray, 0, bIV, 0, bIV.Length);

		string decrypt = null;
		Rijndael aes = Rijndael.Create();
		aes.Mode = CipherMode.CFB;
		aes.Padding = PaddingMode.None;
		try {
			using (MemoryStream mStream = new MemoryStream()) {
				using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write)) {
					cStream.Write(encryptedArray, bIV.Length, encryptedArray.Length - bIV.Length);
					cStream.FlushFinalBlock();
					decrypt = Encoding.UTF8.GetString(mStream.ToArray());
				}
			}
		} catch (Exception e) {
			UnityEngine.Debug.Log(e);
			return null;
		} finally {
			aes.Clear();
		}
		return decrypt.Substring(0, decrypt.Length - (int)decrypt[decrypt.Length - 1]);
	}

	//	static void PrintByteArray (byte[] array)
	//	{
	//		var s = "";
	//		for (int i = 0; i < array.Length; i++) {
	//			s += "-" + array [i];
	//		}
	//		LogManager.Log (s);
	//	}
}
