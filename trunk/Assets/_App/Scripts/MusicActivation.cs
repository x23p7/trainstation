using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicActivation : MonoBehaviour
{
	public AudioSource music;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
		{
			if (music != null)
			{
				music.Play();
				this.enabled = false;
			}
		}
    }
}
