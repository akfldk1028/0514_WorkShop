using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
	private AudioSource[] _audioSources = new AudioSource[(int)Define.ESound.Max];
	private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
	private GameObject _soundRoot = null;
	private string _currentBgmName = null; // 현재 재생 중인 BGM 이름

	public void Init()
	{
		if (_soundRoot == null)
		{
			_soundRoot = GameObject.Find("@SoundRoot");

			if (_soundRoot == null)
			{
				_soundRoot = new GameObject { name = "@SoundRoot" };
				UnityEngine.Object.DontDestroyOnLoad(_soundRoot);

				string[] soundTypeNames = System.Enum.GetNames(typeof(Define.ESound));
				for (int count = 0; count < soundTypeNames.Length - 1; count++)
				{
					GameObject go = new GameObject { name = soundTypeNames[count] };
					_audioSources[count] = go.AddComponent<AudioSource>();
					go.transform.parent = _soundRoot.transform;
				}

				_audioSources[(int)Define.ESound.Bgm].loop = true;
			}
		}
	}

	public void Clear()
	{
		foreach (AudioSource audioSource in _audioSources)
			audioSource.Stop();

		_audioClips.Clear();
	}

	public void Play(Define.ESound type)
	{
		AudioSource audioSource = _audioSources[(int)type];
		audioSource.Play();
	}

	public void Play(Define.ESound type, string key, float pitch = 1.0f)
	{
		AudioSource audioSource = _audioSources[(int)type];

		if (type == Define.ESound.Bgm)
		{
			// 같은 BGM이 이미 재생 중이면 계속 재생
			if (_currentBgmName == key && audioSource.isPlaying)
			{
				Debug.Log($"<color=cyan>[SoundManager]</color> BGM '{key}'가 이미 재생 중입니다. 계속 재생합니다.");
				return;
			}

			LoadAudioClip(key, (audioClip) =>
			{
				if (audioSource.isPlaying)
					audioSource.Stop();

				audioSource.clip = audioClip;
				audioSource.Play();
				_currentBgmName = key; // 현재 BGM 이름 저장
				Debug.Log($"<color=green>[SoundManager]</color> BGM 재생 시작: {key}");
			});
		}
		else
		{
			LoadAudioClip(key, (audioClip) =>
			{
				audioSource.pitch = pitch;
				audioSource.PlayOneShot(audioClip);
			});
		}
	}

	public void Play(Define.ESound type, AudioClip audioClip, float pitch = 1.0f)
	{
		AudioSource audioSource = _audioSources[(int)type];

		if (type == Define.ESound.Bgm)
		{
			// 같은 BGM이 이미 재생 중이면 계속 재생
			if (_currentBgmName == audioClip.name && audioSource.isPlaying && audioSource.clip == audioClip)
			{
				Debug.Log($"<color=cyan>[SoundManager]</color> BGM '{audioClip.name}'가 이미 재생 중입니다. 계속 재생합니다.");
				return;
			}

			if (audioSource.isPlaying)
				audioSource.Stop();

			audioSource.clip = audioClip;
			audioSource.Play();
			_currentBgmName = audioClip.name; // 현재 BGM 이름 저장
			Debug.Log($"<color=green>[SoundManager]</color> BGM 재생 시작: {audioClip.name}");
		}
		else
		{
			audioSource.pitch = pitch;
			audioSource.PlayOneShot(audioClip);
		}
	}

	public void Stop(Define.ESound type)
	{
		AudioSource audioSource = _audioSources[(int)type];
		audioSource.Stop();
		
		if (type == Define.ESound.Bgm)
		{
			_currentBgmName = null; // BGM 정지 시 현재 BGM 이름 초기화
		}
	}

	private void LoadAudioClip(string key, Action<AudioClip> callback)
	{
		AudioClip audioClip = null;
		if (_audioClips.TryGetValue(key, out audioClip))
		{
			callback?.Invoke(audioClip);
			return;
		}

		audioClip = Managers.Resource.Load<AudioClip>(key);

		if (_audioClips.ContainsKey(key) == false)
			_audioClips.Add(key, audioClip);

		callback?.Invoke(audioClip);
	}
}
