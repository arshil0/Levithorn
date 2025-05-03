using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TransitionPoint : MonoBehaviour
{
    public int levelToTransitionTo;

    //keep track of what is the largest priority camera number, to be able to change the camera position smoothly by only accessing 1 camera (the one to be activated)
    public static int mainCameraFocusLevel = 1;

    public bool WarpToLevel()
    {

        //get the camera of the level to transition to
        var camera = GameObject.Find("CinemachineCameras/" + levelToTransitionTo).GetComponent<CinemachineVirtualCamera>();

        //check to see if we are actually going to transition to a new level, or if nothing is happening
        bool transitioning = false;
        if (camera.m_Priority != mainCameraFocusLevel)
        {
            transitioning = true;
        }

        mainCameraFocusLevel += 1;

        camera.m_Priority = mainCameraFocusLevel;

        return transitioning;
    }
}
