using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Gestures;

public class GestureRecognitionHandler : MonoBehaviour
{
    public void UpdateSelectedHandGesture(ScriptableObject handPoseOrShape)
    {
        var handShape = handPoseOrShape as XRHandShape;
        var handPose = handPoseOrShape as XRHandPose;

        var gesture = "No gesture.";
        if (handShape)
            gesture = handShape.name;
        else if (handPose)
            gesture = handPose.name;

    }

}
