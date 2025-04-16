/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using UnityEngine.UI;

public class TrackingOriginSample : MonoBehaviour
{
    public GameObject Axes;

    public OVRCameraRig CameraRig;

    public float CycleTime = 3f;
    public OVRInput.RawButton TogglePassthrough;
    public OVRInput.RawButton ToggleBoundary;

    OVRManager ovrManager;
    OVRPassthroughLayer ovrPassthroughLayer;

    Transform _eyeOrigin;
    Transform _floorOrigin;
    Transform _stageOrigin;

    OVRManager.TrackingOrigin _currentTrackingOrigin;

    float _lastCycle;

    void Start()
    {
        ovrManager = CameraRig.GetComponent<OVRManager>();
        ovrPassthroughLayer = CameraRig.GetComponent<OVRPassthroughLayer>();

        _currentTrackingOrigin = ovrManager.trackingOriginType;

        _eyeOrigin = SpawnOrigin(OVRManager.TrackingOrigin.EyeLevel);
        _floorOrigin = SpawnOrigin(OVRManager.TrackingOrigin.FloorLevel);
        _stageOrigin = SpawnOrigin(OVRManager.TrackingOrigin.Stage);

        Axes.SetActive(false);
    }

    void Update()
    {
        if (Time.time - _lastCycle > CycleTime)
        {
            _lastCycle = Time.time;
            NextOrigin();
        }

        if (OVRInput.GetDown(TogglePassthrough))
        {
            var newState = !ovrPassthroughLayer.enabled;
            ovrPassthroughLayer.enabled = newState;
            ovrManager.isInsightPassthroughEnabled = newState;
        }

        if (OVRInput.GetDown(ToggleBoundary))
        {
            ovrManager.shouldBoundaryVisibilityBeSuppressed =
                !ovrManager.shouldBoundaryVisibilityBeSuppressed;
        }

        UpdateAllSpaces();
    }

    void NextOrigin()
    {
        switch (_currentTrackingOrigin)
        {
            case OVRManager.TrackingOrigin.EyeLevel:
                _currentTrackingOrigin = OVRManager.TrackingOrigin.FloorLevel;
                break;
            case OVRManager.TrackingOrigin.FloorLevel:
                _currentTrackingOrigin = OVRManager.TrackingOrigin.Stage;
                break;
            case OVRManager.TrackingOrigin.Stage:
                _currentTrackingOrigin = OVRManager.TrackingOrigin.EyeLevel;
                break;
            default:
                _currentTrackingOrigin = OVRManager.TrackingOrigin.EyeLevel;
                break;
        }

        Debug.Log($"Setting OVRManager tracking origin to {_currentTrackingOrigin}");
        ovrManager.trackingOriginType = _currentTrackingOrigin;

        UpdateOriginUIs();
    }

    Transform SpawnOrigin(OVRManager.TrackingOrigin trackingOrigin)
    {
        var origin = Instantiate(Axes).transform;
        origin.GetChild(3).GetChild(1).GetComponent<Text>().text = trackingOrigin.ToString().ToUpper();
        origin.GetChild(3).GetChild(2).gameObject.SetActive(ovrManager.trackingOriginType == trackingOrigin);
        return origin;
    }

    void UpdateOriginUIs()
    {
        _eyeOrigin.GetChild(3).GetChild(2).gameObject
            .SetActive(ovrManager.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel);
        _floorOrigin.GetChild(3).GetChild(2).gameObject
            .SetActive(ovrManager.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel);
        _stageOrigin.GetChild(3).GetChild(2).gameObject
            .SetActive(ovrManager.trackingOriginType == OVRManager.TrackingOrigin.Stage);
    }

    void UpdateAllSpaces()
    {
        var eyePose = OVRPlugin.GetTrackingTransformRelativePose(OVRPlugin.TrackingOrigin.EyeLevel).ToOVRPose();
        _eyeOrigin.SetPositionAndRotation(eyePose.position, eyePose.orientation);

        var floorPose = OVRPlugin.GetTrackingTransformRelativePose(OVRPlugin.TrackingOrigin.FloorLevel).ToOVRPose();
        _floorOrigin.SetPositionAndRotation(floorPose.position, floorPose.orientation);

        var stagePose = OVRPlugin.GetTrackingTransformRelativePose(OVRPlugin.TrackingOrigin.Stage).ToOVRPose();
        _stageOrigin.SetPositionAndRotation(stagePose.position, stagePose.orientation);

    }
}
