using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {

    //Public fields
    public StringAudioClipDictionary loopingClipDictionary;
    public StringAudioClipDictionary soundClipDictionary;

	//Private fields
	Dictionary<string, AudioSource> loopingLayerDictionary;
	Dictionary<string, Coroutine> loopingCoroutineDictionary;
    AudioSource soundSource;

	// Use this for initialization
	void Awake() {
        //Instantiate dicts
        loopingLayerDictionary = new Dictionary<string, AudioSource>();
        loopingCoroutineDictionary = new Dictionary<string, Coroutine>();

        //Create gameobjects for each looping layer
        foreach (string str in loopingClipDictionary.Keys)
        {
            AudioSource layer = new GameObject(str, typeof(AudioSource)).GetComponent<AudioSource>();
            layer.transform.parent = transform;
            layer.clip = loopingClipDictionary[str];
            layer.playOnAwake = false;
			layer.loop = true;
			layer.volume = 0;
            layer.Stop();
            loopingLayerDictionary.Add(str, layer);
			loopingCoroutineDictionary.Add(str, null);
        }

        //Create soundeffect layer
        soundSource = new GameObject("SoundEffects", typeof(AudioSource)).GetComponent<AudioSource>();
        soundSource.transform.parent = transform;
    }

	/// <summary>
	/// Get an audio clip from the sound effect dictionary
	/// </summary>
	/// <param name="clip">The clip's key</param>
	/// <returns>The returned clip</returns>
	public AudioClip GetSoundEffect(string clip)
	{
		if(!soundClipDictionary.ContainsKey(clip))
		{
			Debug.LogError("Audio clip does not exist in sound effect dictionary");
			return null;
		}

		return soundClipDictionary[clip];
	}

    /// <summary>
    /// Check if a layer is playing
    /// </summary>
    /// <param name="layerName">Name of the layer</param>
    public bool IsLayerPlaying(string layerName)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio layer does not exist in dictionary");
            return false;
        }

        return loopingLayerDictionary[layerName].isPlaying;
    }

    /// <summary>
    /// Play a one shot sound effect
    /// </summary>
    /// <param name="name">The name of the audio clip to play</param>
    /// <param name="volumeScale">The scale of the volume (0-1)</param>
    public void PlayOneShot(string name, float volumeScale)
    {
        //Error check
        if (!soundClipDictionary.ContainsKey(name))
        {
            Debug.LogError("Sound clip does not exist in dictionary");
            return;
        }

        soundSource.PlayOneShot(soundClipDictionary[name], volumeScale);
    }

    /// <summary>
    /// Play a layer
    /// </summary>
    /// <param name="layerName">The name of the layer</param>
    public void PlayLayer(string layerName)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio layer does not exist in dictionary");
            return;
        }

        if (!IsLayerPlaying(layerName))
        {
            loopingLayerDictionary[layerName].Play();
        }
    }

	/// <summary>
	/// Plays all layers with "m_" in front, denoting music
	/// </summary>
	public void PlayAllMusicLayers()
	{
		foreach(string key in loopingLayerDictionary.Keys)
		{
			//Check for "m_"
			if(key.Length > 1 && key[0] == 'm' && key[1] == '_')
			{
				loopingLayerDictionary[key].volume = 0;
				loopingLayerDictionary[key].Play();
			}
		}
	}

    /// <summary>
    /// Stop a layer
    /// </summary>
    /// <param name="layerName">The name of the layer</param>
    public void StopLayer(string layerName)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio layer does not exist in dictionary");
            return;
        }

        if (IsLayerPlaying(layerName))
        {
            loopingLayerDictionary[layerName].Stop();
        }
    }

    /// <summary>
    /// Set the volume of a layer
    /// </summary>
    /// <param name="layerName">The name of the layer</param>
    /// <param name="volume">The volume to set it to</param>
    public void SetLayerVolume(string layerName, float volume)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio layer does not exist in dictionary");
            return;
        }

        loopingLayerDictionary[layerName].volume = volume;
    }

    /// <summary>
    /// Fade an audio layer based on a given goal
    /// </summary>
    /// <param name="layerName">Layer to fade</param>
    /// <param name="goalVolume">The goal volume</param>
    /// <param name="fadeLength">The length of the fade</param>
    public void FadeLayer(string layerName, float goalVolume, float fadeLength, bool stop = false)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio layer does not exist in dictionary");
            return;
        }

        if (loopingLayerDictionary[layerName].volume > goalVolume)
        {
            FadeOutLayer(layerName, goalVolume, fadeLength, stop);
        }
        else FadeInLayer(layerName, goalVolume, fadeLength);
    }

    /// <summary>
    /// Fade in an audio layer
    /// </summary>
    /// <param name="layerName">Layer to fade in</param>
    /// <param name="goalVolume">The goal volume</param>
    /// <param name="fadeLength">The length of the fade</param>
    public void FadeInLayer(string layerName, float goalVolume, float fadeLength)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio " + layerName + " layer does not exist in dictionary");
            return;
        }

        if (loopingCoroutineDictionary[layerName] != null)
            StopCoroutine(loopingCoroutineDictionary[layerName]);

		if (!IsLayerPlaying(layerName))
			PlayLayer(layerName);

        loopingCoroutineDictionary[layerName] = StartCoroutine(SourceFadeIn(
						loopingLayerDictionary[layerName], goalVolume, 
						fadeLength, layerName));
    }

	/// <summary>
	/// Fade out an audio layer
	/// </summary>
	/// <param name="layerName">Layer to fade out</param>
	/// <param name="goalVolume">The goal volume</param>
	/// <param name="fadeLength">The length of the fade</param>
	/// <param name="stop">Stop the audio source at the end</param>
	public void FadeOutLayer(string layerName, float goalVolume, float fadeLength, bool stop = false)
    {
        //Error check
        if (!loopingLayerDictionary.ContainsKey(layerName))
        {
            Debug.LogError("Audio layer does not exist in dictionary");
            return;
        }

        if (loopingCoroutineDictionary[layerName] != null)
            StopCoroutine(loopingCoroutineDictionary[layerName]);

		loopingCoroutineDictionary[layerName] = StartCoroutine(SourceFadeOut(
						loopingLayerDictionary[layerName], goalVolume,
						fadeLength, layerName, stop));
	}

    /// <summary>
    /// Coroutine to fade in an audio layer
    /// </summary>
    /// <param name="source">Audio source to fade in</param>
    /// <param name="goalVol">Goal volume</param>
    /// <param name="fadeLength">The length of the fade</param>
    /// <returns></returns>
    IEnumerator SourceFadeIn(AudioSource source, float goalVol, float fadeLength, string layerName)
    {
        float vol = source.volume;
        //Fade the vol
        while (vol < goalVol)
        {
            vol += Time.deltaTime / fadeLength;
            source.volume = vol;
            yield return null;
        }
        source.volume = goalVol;
		loopingCoroutineDictionary[layerName] = null;
	}

	/// <summary>
	/// Coroutine to fade in an audio layer
	/// </summary>
	/// <param name="source">Audio source to fade in</param>
	/// <param name="goalVol">Goal volume</param>
	/// <param name="fadeLength">The length of the fade</param>
	/// <param name="stop">Stop the audio source at the end</param>
	/// <returns></returns>
	IEnumerator SourceFadeOut(AudioSource source, float goalVol, float fadeLength, string layerName, bool stop = false)
    {
        float vol = source.volume;
        //Fade the vol
        while (vol > goalVol)
        {
            vol -= Time.deltaTime / fadeLength;
            source.volume = vol;
            yield return null;
        }
        source.volume = goalVol;
		loopingCoroutineDictionary[layerName] = null;

		if (stop)
			loopingLayerDictionary[layerName].Stop();
	}
}
