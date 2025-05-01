using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TransitionPoint : MonoBehaviour
{
    public int levelToTransitionTo;

    //keep track of what is the largest priority camera number, to be able to change the camera position smoothly by only accessing 1 camera (the one to be activated)
    public static int mainCameraFocusLevel = 1;

    public void WarpToLevel()
    {
        mainCameraFocusLevel += 1;
        GameObject.Find("CinemachineCameras/" + levelToTransitionTo).GetComponent<CinemachineVirtualCamera>().m_Priority = mainCameraFocusLevel;
    }
}
