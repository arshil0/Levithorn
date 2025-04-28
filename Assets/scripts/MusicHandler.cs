using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{


    //on which second exactly should the music loop from?
    //different tracks will have to start at different times while looping (for example the main menu music loops starting at second 4.846 to make it seamless)
    [SerializeField] float loopStartOffset = 0f;

    AudioSource audio;
    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audio.isPlaying)
        {
            //replay the music (I am starting the music before changing the time, as the other way didn't work)
            audio.Play();
            //this is around exactly the time where the music should loop from
            audio.time = loopStartOffset;
        }
    }
}
