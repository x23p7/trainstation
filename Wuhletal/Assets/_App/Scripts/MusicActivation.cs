using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicActivation : MonoBehaviour
{
	public AudioSource music;
	public GameObject myUI;
	private void Start()
	{
		myUI.SetActive(music.isPlaying);
	}
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.M))
		{
			if (music != null)
			{
				if (!music.isPlaying)
				{
					myUI.SetActive(true);
					music.Play();
				}
				else
				{
					myUI.SetActive(false);
					music.Stop();
				}
			}
		}
	}
}
