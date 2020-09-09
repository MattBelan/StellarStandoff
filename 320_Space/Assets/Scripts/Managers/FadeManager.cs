using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour {

	//Public fields
	public Image fadeSprite;
	public float fadeLengthOnLoad;

	//Private fields
	float fadeVal;
	Coroutine fadeCor;

	//Called before start
	private void Awake()
	{
		fadeCor = null;
	}

	// Use this for initialization
	void Start () {
		if (fadeLengthOnLoad > 0)
		{
			fadeVal = 1;
			Fade(fadeLengthOnLoad, 0);
		}
		else fadeVal = 0;

		fadeSprite.color = new Color(0, 0, 0, fadeVal);
	}

	/// <summary>
	/// Fade the background in or out
	/// </summary>
	/// <param name="fadeLength">How long the fade lasts</param>
	/// <param name="fadeGoal">The target alpha value [0-1]</param>
	/// <param name="sceneToLoad">OPTIONAL: scene to load at the end of the fade</param>
	public void Fade(float fadeLength, float fadeGoal, string sceneToLoad = null)
	{
		//Stop the previous coroutine
		if(fadeCor != null)
		{
			StopCoroutine(fadeCor);
		}

		//Run the correct coroutine
		if(fadeGoal < fadeVal)
			fadeCor = StartCoroutine(FadeIn(fadeLength, fadeGoal, sceneToLoad));
		else fadeCor = StartCoroutine(FadeOut(fadeLength, fadeGoal, sceneToLoad));
	}

	/// <summary>
	/// Fade the background in
	/// </summary>
	/// <param name="fadeLength">How long the fade lasts</param>
	/// <param name="fadeGoal">The target alpha value [0-1]</param>
	/// <param name="sceneToLoad">OPTIONAL: scene to load at the end of the fade</param>
	IEnumerator FadeIn(float fadeLength, float fadeGoal, string sceneToLoad = null)
	{
		//Fade the bg
		while (fadeVal >= fadeGoal)
		{
			fadeVal -= Time.deltaTime / fadeLength;
			fadeSprite.color = new Color(0, 0, 0, fadeVal);
			yield return null;
		}
		//Finish up
		fadeCor = null;
		fadeSprite.color = new Color(0, 0, 0, fadeGoal);
		if (sceneToLoad != null)
			SceneManager.LoadScene(sceneToLoad);
	}

	/// <summary>
	/// Fade the background out
	/// </summary>
	/// <param name="fadeLength">How long the fade lasts</param>
	/// <param name="fadeGoal">The target alpha value [0-1]</param>
	/// <param name="sceneToLoad">OPTIONAL: scene to load at the end of the fade</param>
	IEnumerator FadeOut(float fadeLength, float fadeGoal, string sceneToLoad = null)
	{
		//Fade the bg
		while(fadeVal <= fadeGoal)
		{
			fadeVal += Time.deltaTime / fadeLength;
			fadeSprite.color = new Color(0, 0, 0, fadeVal);
			yield return null;
		}
		//Finish up
		fadeCor = null;
		fadeSprite.color = new Color(0, 0, 0, fadeGoal);
		if (sceneToLoad != null)
			SceneManager.LoadScene(sceneToLoad);
	}
}
