using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Snaplingo.SaveData;
using LitJson;
using System;
using System.Text;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : Manager, ISaveData
{
	// 注意，请不要在Awake中使用Instance，否则会出现死循环
	public static AudioManager Instance { get { return GetManager<AudioManager>(); } }

    private bool _mute;
	protected override void Init()
	{
		base.Init();

		var obj = new GameObject();
		obj.name = "BackgroundSound";
		obj.transform.SetParent(transform);

		_bgmSource = obj.AddComponent<AudioSource>();
		_bgmSource.playOnAwake = false;
		_sfxSource = GetComponent<AudioSource>();
		_sfxSource.playOnAwake = false;
        _mute = false;
        InitBtnClick();

        SaveDataUtils.LoadTo(this);
	}

	string _folder = "Audio/";

	AudioSource _sfxSource;
	AudioSource _bgmSource;

	public bool IsSfxPlaying {
		get {
			return _sfxSource.isPlaying;
		}
	}
    public bool IsBGMPlaying {
        get {
            return _bgmSource.isPlaying;
        }
    }

	Queue<AudioSource> _coverSfxs = new Queue<AudioSource>();

	AudioSource GetIdleSfx()
	{
		var count = _coverSfxs.Count;
		while (count > 0) {
			count--;
			var sfx = _coverSfxs.Dequeue();
			_coverSfxs.Enqueue(sfx);
			if (!sfx.isPlaying)
				return sfx;
		}
		return CreateSfx();
	}

	AudioSource CreateSfx()
	{
		var audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		_coverSfxs.Enqueue(audioSource);
		audioSource.enabled = _sfxSource.enabled;
		audioSource.volume = _sfxSource.volume;
		return audioSource;
	}

	void BasePlaySfx(AudioSource audioSource, object type, bool isLoop = false)
	{
		if (type.GetType() == typeof(int))
			audioSource.clip = ResourceLoadUtils.Load<AudioClip>(_folder + AudioConfig.Instance.GetNameById((int)type), true);
		else if (type.GetType() == typeof(string))
			audioSource.clip = ResourceLoadUtils.Load<AudioClip>(_folder + AudioConfig.Instance.GetNameByKey((string)type), true);
		audioSource.loop = isLoop;
		audioSource.Play();
	}

	public void PlaySfx(int musicId, bool isLoop = false, bool cover = true)
	{
        if (_mute) return;

        if (!cover) {
			var sfx = GetIdleSfx();
			BasePlaySfx(sfx, musicId, isLoop);
		} else
			BasePlaySfx(_sfxSource, musicId, isLoop);

	}

	public void PlaySfx(string musicType, bool isLoop = false, bool cover = true)
	{
        if (_mute) return;

        if (!cover) {
			var sfx = GetIdleSfx();
			BasePlaySfx(sfx, musicType, isLoop);
		} else
			BasePlaySfx(_sfxSource, musicType, isLoop);
	}

	public void StopSfx()
	{
		_sfxSource.Stop();
		while (_coverSfxs.Count > 0) {
			var sfx = _coverSfxs.Dequeue();
			Destroy(sfx);
		}
	}

	public void PlayMusic(int musicId)
	{
        if (_mute) return;

        _bgmSource.clip = ResourceLoadUtils.Load<AudioClip>(_folder + AudioConfig.Instance.GetNameById(musicId), true);
		_bgmSource.loop = true;
		_bgmSource.Play();
	}

	public float GetBgmTime()
	{
		return _bgmSource.time;
	}

	public void PlayMusic(string musicType)
	{//播放系统背景音乐
        if (_mute) return;

        _bgmSource.clip = ResourceLoadUtils.Load<AudioClip>(_folder + AudioConfig.Instance.GetNameByKey(musicType), true);
		_bgmSource.loop = true;
		_bgmSource.Play();
	}

    private Dictionary<string, AudioClip> _wordCache = new Dictionary<string, AudioClip>();
    public void ClearWordCache()
    {
        _wordCache.Clear();
    }

    private Dictionary<string, AudioClip> _songCache = new Dictionary<string, AudioClip>();
    public void LoadSong(string songName)
    {
        if (!_songCache.ContainsKey(songName))
        {
            AudioClip clip = ResourceLoadUtils.Load<AudioClip>(_folder + songName.ToLower(), true);
            _songCache.Add(songName, clip);
        }
    }

    public void PlaySong(string songName)
    {//播放游戏歌曲
        if (_mute) return;

        _bgmSource.clip = _songCache[songName];
        _bgmSource.loop = false;
        _bgmSource.Play();
    }

    public void PlayWord(string word)
    {
        if (_mute) return;

        try
        {
            var sfx = GetIdleSfx();
            if (_wordCache.ContainsKey(word))
            {
                sfx.clip = _wordCache[word];
            }
            else
            {
                sfx.clip = ResourceLoadUtils.Load<AudioClip>(_folder + word.ToLower(), true);
                _wordCache.Add(word, sfx.clip);
            }
            //float[] data = new float[sfx.clip.samples * sfx.clip.channels];
            //sfx.clip.GetData(data, 0);
            //FileStream fs = new FileStream(Application.dataPath + "/test.txt", FileMode.Create, FileAccess.ReadWrite);
            //StreamWriter sw = new StreamWriter(fs);
            //for (int i = 0; i < data.Length; i++)
            //{
            //    sw.WriteLine(data[i]);
            //}
            sfx.loop = false;
            sfx.Play();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }

	public void StopMusic()
	{
		_bgmSource.Stop();
	}

	public void PauseSound()
	{
		_bgmSource.Pause();
		_sfxSource.Pause();
	}

	public void ResumeSound()
	{
		_bgmSource.UnPause();
		_sfxSource.UnPause();
	}

	public void StopSound()
	{
		_bgmSource.Stop();
		_sfxSource.Stop();
	}

    private float _bgmClipPauseTime;
    public void Mute()
    {
        _mute = true;
        if (_bgmSource.isPlaying)
        {
            _bgmClipPauseTime = _bgmSource.time;
        }
        else
        {
            _bgmClipPauseTime = -1;
        }

        foreach (AudioSource audioSource in _coverSfxs)
        {
            audioSource.Stop();
        }

        _muteTimeLength = 0;
        _bgmSource.Pause();
        _sfxSource.Stop();
    }

    public void ResumeFromMute()
    {
        _mute = false;
        if (_bgmClipPauseTime > 0)
        {
            _bgmSource.time = _bgmClipPauseTime + _muteTimeLength;
        }

        _bgmSource.UnPause();
    }

    private float _muteTimeLength;
    void Update()
    {
        if (_mute)
        {
            _muteTimeLength += Time.deltaTime;
        }
    }

	public void InitBtnClick(GameObject pageObj = null)
	{
		Button[] btnAry;
		if (pageObj != null)
			btnAry = pageObj.GetComponentsInChildren<Button>(true);
		else
			btnAry = FindObjectsOfType<Button>();
		foreach (var btn in btnAry) {
			if (string.IsNullOrEmpty(CustomAudioBtn.Get(btn.gameObject).m_AudioType)) {
				CustomAudioBtn.Get(btn.gameObject).m_AudioType = "BtnClick";
			}
		}
	}

	#region ISaveData
	System.Action _musicOnCallback = null;
	public void AddMusicOnCallback(System.Action action)
	{
		_musicOnCallback += action;
	}

	public void RemoveMusicOnCallback(System.Action action)
	{
		_musicOnCallback -= action;
	}

	bool _musicEnable = true;
	bool _sfxEnable = true;

	public bool Music {
		get {
			return _musicEnable;
		}
		set {
			var oldMusicEnable = _musicEnable;
			_musicEnable = value;
			_bgmSource.enabled = _musicEnable;
			_bgmSource.volume = _musicEnable ? 1 : 0;
			SaveDataUtils.SaveFrom(this);
			if (_musicEnable && !oldMusicEnable) {
				if (_musicOnCallback != null)
					_musicOnCallback();
			}
		}
	}

	public bool Sfx {
		get {
			return _sfxEnable;
		}
		set {
			_sfxEnable = value;
			_sfxSource.enabled = _sfxEnable;
			_sfxSource.volume = _sfxEnable ? 1 : 0;
			foreach (var source in _coverSfxs) {
				source.enabled = _sfxEnable;
				source.volume = _sfxEnable ? 1 : 0;
			}
			SaveDataUtils.SaveFrom(this);
		}
	}

	public void SetSound(bool enabled)
	{
		Music = enabled;
		Sfx = enabled;
		SaveDataUtils.SaveFrom(this);
	}

	public void SetMusicVolumn(float vol)
	{
		if (Music)
			_bgmSource.volume = vol;
	}

	public void SetSfxVolumn(float vol)
	{
		if (Sfx)
			_sfxSource.volume = vol;
	}

	public string SaveTag()
	{
		return "AudioInfo";
	}

	public string SaveAsJson()
	{
		JsonData data = new JsonData();
		data["music"] = Music;
		data["sfx"] = Sfx;
		return data.ToJson();
	}

	public void LoadFromJson(string json)
	{
		JsonData data = JsonMapper.ToObject(json);
		Music = bool.Parse(data.TryGetString("music"));
		Sfx = bool.Parse(data.TryGetString("sfx"));
	}

	#endregion

}
