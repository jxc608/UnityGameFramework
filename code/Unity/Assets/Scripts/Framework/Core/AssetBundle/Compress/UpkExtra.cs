using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

public class UPKExtra
{
	private static Dictionary<int,OneFileInfo> m_allFileInfoDic = new Dictionary<int,OneFileInfo>();

	private static UTF8Encoding m_UTF8Encoding = new UTF8Encoding();

	public static bool ExtraUPK(string upkfilepath, string outputpath, IProgress progress, bool saveToFile, Action<string, byte[]> callback)
	{
		var pathList = new List<string>();
		return ExtraUPK(upkfilepath, outputpath, ref pathList, progress, saveToFile, callback);
	}

	public static bool ExtraUPK(string upkfilepath, string outputpath, ref List<string> pathList)
	{
		return ExtraUPK(upkfilepath, outputpath, ref pathList, null, true, null);
	}

	static bool ExtraUPK(string upkfilepath, string outputpath, ref List<string> pathList, IProgress progress, bool saveToFile, Action<string, byte[]> callback)
	{
		m_allFileInfoDic.Clear();

		if (!outputpath.EndsWith("/"))
			outputpath = outputpath + "/";
		
		int totalsize = 0;

		bool success = true;
		pathList = new List<string>();
		FileStream upkFilestream = new FileStream(upkfilepath, FileMode.Open);
		try {
			upkFilestream.Seek(0, SeekOrigin.Begin);
		
			int offset = 0;
		
			//读取文件数量;
			byte[] totaliddata = new byte[4];
			upkFilestream.Read(totaliddata, 0, 4);
			int filecount = BitConverter.ToInt32(totaliddata, 0);
			offset += 4;
			Debug.Log("filecount=" + filecount);
		
			//读取所有文件信息;
			for (int index = 0; index < filecount; index++) {
				//读取id;
				byte[] iddata = new byte[4];
				upkFilestream.Seek(offset, SeekOrigin.Begin);
				upkFilestream.Read(iddata, 0, 4);
				int id = BitConverter.ToInt32(iddata, 0);
				offset += 4;
			
				//读取StartPos;
				byte[] startposdata = new byte[4];
				upkFilestream.Seek(offset, SeekOrigin.Begin);
				upkFilestream.Read(startposdata, 0, 4);
				int startpos = BitConverter.ToInt32(startposdata, 0);
				offset += 4;
			
				//读取size;
				byte[] sizedata = new byte[4];
				upkFilestream.Seek(offset, SeekOrigin.Begin);
				upkFilestream.Read(sizedata, 0, 4);
				int size = BitConverter.ToInt32(sizedata, 0);
				offset += 4;
			
				//读取pathLength;
				byte[] pathLengthdata = new byte[4];
				upkFilestream.Seek(offset, SeekOrigin.Begin);
				upkFilestream.Read(pathLengthdata, 0, 4);
				int pathLength = BitConverter.ToInt32(pathLengthdata, 0);
				offset += 4;
			
				//读取path;
				byte[] pathdata = new byte[pathLength];
				upkFilestream.Seek(offset, SeekOrigin.Begin);
				upkFilestream.Read(pathdata, 0, pathLength);
				string path = m_UTF8Encoding.GetString(pathdata);
				offset += pathLength;

				//添加到Dic;
				OneFileInfo info = new OneFileInfo();
				info.m_id = id;
				info.m_Size = size;
				info.m_PathLength = pathLength;
				info.m_Path = path;
				info.m_StartPos = startpos;
				m_allFileInfoDic.Add(id, info);
				pathList.Add(path);
			
				totalsize += size;
			
//				Debug.Log ("id=" + id + " startPos=" + startpos + " size=" + size + " pathLength=" + pathLength + " path=" + path);
			}

			//释放文件;
			int totalprocesssize = 0;
			foreach (var infopair in m_allFileInfoDic) {
				OneFileInfo info = infopair.Value;
			
				int startPos = info.m_StartPos;
				int size = info.m_Size;
				string path = info.m_Path;
			
				//创建文件
				string dirpath = outputpath + (path.LastIndexOf('/') < 0 ? "" : path.Substring(0, path.LastIndexOf('/')));
				string filepath = outputpath + path;
				if (Directory.Exists(dirpath) == false) {
					Directory.CreateDirectory(dirpath);
				}
				if (File.Exists(filepath)) {
					File.Delete(filepath);
				}

				FileStream fileStream = null;
				if (saveToFile)
					fileStream = new FileStream(filepath, FileMode.Create);
				MemoryStream memoryStream = new MemoryStream();

				try {
					byte[] tmpfiledata;
					int processSize = 0;
					while (processSize < size) {
						if (size - processSize < 1024) {
							tmpfiledata = new byte[size - processSize];
						} else {
							tmpfiledata = new byte[1024];
						}

						//读取;
						upkFilestream.Seek(startPos + processSize, SeekOrigin.Begin);
						upkFilestream.Read(tmpfiledata, 0, tmpfiledata.Length);

						//写入;
						if (saveToFile)
							fileStream.Write(tmpfiledata, 0, tmpfiledata.Length);
						memoryStream.Write(tmpfiledata, 0, tmpfiledata.Length);

						processSize += tmpfiledata.Length;
						totalprocesssize += tmpfiledata.Length;

						if (progress != null)
							progress.SetProgress((long)totalprocesssize, (long)totalsize);
					}
					if (saveToFile)
						fileStream.Flush();
					if (callback != null)
						callback(filepath, memoryStream.ToArray());
				} catch (Exception e) {
					Debug.LogError(e.Message);
					Debug.LogError(e.StackTrace);
					success = false;
				} finally {
					if (saveToFile)
						fileStream.Close();
					memoryStream.Close();
				}
			}
		} catch (Exception e) {
			Debug.LogError(e.Message);
			Debug.LogError(e.StackTrace);
			success = false;
		} finally {
			upkFilestream.Close();
		}

		return success;
	}
}
