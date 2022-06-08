using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/* Part of code taken from https://github.com/KAPastor/XRHandAnimations/blob/main/HandAnim.cs*/
public class XRHandControllerManager : MonoBehaviour
{
    public List<GameObject> controllerPrefabs;
    public GameObject handGameObject;
    public InputDeviceCharacteristics targetDeviceCharacteristics;
    public bool showController;
    public bool useComplexHandAnimation;
    public bool useOculusHandAnimation;

    List<InputDevice> devices;
    InputDevice targetDevice;
    GameObject spawnedController;
    GameObject spawnedHand;
    Animator handAnimator;

    public const string ANIM_LAYER_NAME_POINT = "Point Layer";
    public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
    public const string ANIM_PARAM_NAME_FLEX = "Flex";
    public const string ANIM_PARAM_NAME_POSE = "Pose";
    public const float INPUT_RATE_CHANGE = 20.0f;

    private int animLayerIndexThumb = -1;
    private int animLayerIndexPoint = -1;
    private int animParamIndexFlex = -1;
    private int animParamIndexPose = -1;

    private bool isPointing = false;
    private bool isGivingThumbsUp = false;
    private float pointBlend = 0.0f;
    private float thumbsUpBlend = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        devices = new List<InputDevice>();

        Initialize();
    }

    void Initialize()
    {
        refreshAvailableDevices();

        if (devices.Count > 0)
        {
            targetDevice = devices[0];

            GameObject controllerPrefab = controllerPrefabs.Find(controller => controller.name == targetDevice.name);
            if (!controllerPrefab)
            {
                controllerPrefab = controllerPrefabs[0];
            }

            spawnedController = Instantiate(controllerPrefab, transform);
        }

        if (spawnedHand == null)
        {
            spawnedHand = handGameObject;
            handAnimator = spawnedHand.GetComponentInChildren<Animator>();

            animLayerIndexPoint = handAnimator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
            animLayerIndexThumb = handAnimator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
            animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
            animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);
        }
    }

    void refreshAvailableDevices()
    {
        InputDevices.GetDevicesWithCharacteristics(targetDeviceCharacteristics, devices);
    }

    // Update is called once per frame
    void Update()
    {
        if (targetDevice.isValid)
        {
            if (showController)
            {
                spawnedController.SetActive(true);
                spawnedHand.SetActive(false);
            }
            else
            {
                spawnedController.SetActive(false);
                spawnedHand.SetActive(true);

                if (useComplexHandAnimation)
                {
                    UpdateHandAnimationComplex();
                }
                else if (useOculusHandAnimation)
                {
                    UpdateOculusHandAnimation();
                }
                else
                {
                    UpdateHandAnimation();
                }
            }
        }
        else
        {
            Initialize();
        }
    }

    void UpdateHandAnimation()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0f);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0f);
        }
    }

    void UpdateOculusHandAnimation()
    {
        UpdateCapTouchStates();

        pointBlend = InputValueRateChange(isPointing, pointBlend);
        thumbsUpBlend = InputValueRateChange(isGivingThumbsUp, thumbsUpBlend);

        // Flex
        // blend between open hand and fully closed fist
        targetDevice.TryGetFeatureValue(CommonUsages.grip, out float flex);
        handAnimator.SetFloat(animParamIndexFlex, flex);

        // Point
        handAnimator.SetLayerWeight(animLayerIndexPoint, pointBlend);

        // Thumbs up
        handAnimator.SetLayerWeight(animLayerIndexThumb, thumbsUpBlend);

        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float pinchValue);
        handAnimator.SetFloat("Pinch", pinchValue);
    }

    // Just checking the state of the index and thumb cap touch sensors, but with a little bit of
    // debouncing.
    private void UpdateCapTouchStates()
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        isPointing = !(triggerValue > 0f);

        targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool primary2DAxisTouch);
        targetDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryTouch);
        targetDevice.TryGetFeatureValue(CommonUsages.secondary2DAxisTouch, out bool secondary2DAxisTouch);
        targetDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryTouch);
        isGivingThumbsUp = !(primary2DAxisTouch || primaryTouch || secondary2DAxisTouch || secondaryTouch);
    }

    private float InputValueRateChange(bool isDown, float value)
    {
        float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
        float sign = isDown ? 1.0f : -1.0f;
        return Mathf.Clamp01(value + rateDelta * sign);
    }

    void UpdateHandAnimationComplex()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Middle", gripValue);
            handAnimator.SetFloat("Ring", gripValue);
            handAnimator.SetFloat("Pinky", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Middle", 0f);
            handAnimator.SetFloat("Ring", 0f);
            handAnimator.SetFloat("Pinky", 0f);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Index", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Index", 0f);
        }

        targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool primary2DAxisTouch);
        targetDevice.TryGetFeatureValue(CommonUsages.primaryTouch, out bool primaryTouch);
        targetDevice.TryGetFeatureValue(CommonUsages.secondary2DAxisTouch, out bool secondary2DAxisTouch);
        targetDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out bool secondaryTouch);
        if (primary2DAxisTouch || primaryTouch || secondary2DAxisTouch || secondaryTouch)
        {
            handAnimator.SetFloat("Thumb", 1f);
        }
        else
        {
            handAnimator.SetFloat("Thumb", 0f);
        }
    }

    public void MockHandAnimation(string finger, float value)
    {
        handAnimator.SetFloat(finger, value);
    }

    public void MockThumbAnimation(float value)
    {
        handAnimator.SetFloat("Thumb", value);
    }

    public void MockIndexAnimation(float value)
    {
        handAnimator.SetFloat("Index", value);
    }

    public void MockMiddleAnimation(float value)
    {
        handAnimator.SetFloat("Middle", value);
    }

    public void MockRingAnimation(float value)
    {
        handAnimator.SetFloat("Ring", value);
    }

    public void MockPinkyAnimation(float value)
    {
        handAnimator.SetFloat("Pinky", value);
    }

    public void MockAnimParamIndexFlex(float value)
    {
        handAnimator.SetFloat(animParamIndexFlex, value);
    }

    public void MockPinch(float value)
    {
        handAnimator.SetFloat("Pinch", value);
    }

    public void MockAnimLayerIndexPoint(float value)
    {
        handAnimator.SetLayerWeight(animLayerIndexPoint, 1f - value);
    }

    public void MockAnimLayerIndexThumb(float value)
    {
        handAnimator.SetLayerWeight(animLayerIndexThumb, 1f - value);
    }
}
