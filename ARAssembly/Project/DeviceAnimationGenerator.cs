using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceAnimationGenerator : AnimationGenerator
{

    public float duration_time = 1f; // 移动的持续时间
    public float static_time = 1f; // 静止的持续时间
    public float max_z = 0.8f; // 组件下落起点
    public float max_x = 0.8f;
    public float max_y = 0.8f;

    public bool isYAnimation = false;
    public bool isXAnimation = false;
    public bool isZAnimation = true;

    private DeviceController deviceController;
    private BoxCollider deviceBoxCollider;
    private GameObject meshCenter;

    private void Awake()
    {
        deviceController = GameObject.Find("Controller").GetComponent<DeviceController>();
        
    }

    public void InitialBindingInfo()
    {
        meshCenter = transform.Find("MeshCenter").gameObject;

    }


    public void GenerateAnimation()
    {
        GameObject thisDevice = this.gameObject;
        Animation animation = thisDevice.AddComponent<Animation>();

        AnimationClip myclip = new AnimationClip();
        myclip.name = thisDevice.name;
        myclip.legacy = true;
        myclip.wrapMode = WrapMode.Once;

        AnimationCurve curve = new AnimationCurve();
        if (isYAnimation)
        {
            curve.AddKey(new Keyframe(0, max_y));
        }
        else
        {
            curve.AddKey(new Keyframe(0, thisDevice.transform.localPosition.y));
        }
        curve.AddKey(new Keyframe(0 + duration_time, thisDevice.transform.localPosition.y, inTangent: 0f, outTangent: 0f));
        curve.AddKey(new Keyframe(0 + duration_time + static_time, thisDevice.transform.localPosition.y, inTangent: 0f, outTangent: 0f));
        myclip.SetCurve("", typeof(Transform), "localPosition.y", curve);

        curve = new AnimationCurve();
        if (isXAnimation)
        {
            curve.AddKey(new Keyframe(0, max_x));
        }
        else
        {
            curve.AddKey(new Keyframe(0, thisDevice.transform.localPosition.x));
        }
        curve.AddKey(new Keyframe(0 + duration_time, thisDevice.transform.localPosition.x, inTangent: 0f, outTangent: 0f));
        curve.AddKey(new Keyframe(0 + duration_time + static_time, thisDevice.transform.localPosition.x, inTangent: 0f, outTangent: 0f));
        myclip.SetCurve("", typeof(Transform), "localPosition.x", curve);

        curve = new AnimationCurve();
        if (isZAnimation)
        {
            curve.AddKey(new Keyframe(0, max_z));
        }
        else
        {
            curve.AddKey(new Keyframe(0, thisDevice.transform.localPosition.z));
        }
        curve.AddKey(new Keyframe(0 + duration_time, thisDevice.transform.localPosition.z, inTangent: 0f, outTangent: 0f));
        curve.AddKey(new Keyframe(0 + duration_time + static_time, thisDevice.transform.localPosition.z, inTangent: 0f, outTangent: 0f));
        myclip.SetCurve("", typeof(Transform), "localPosition.z", curve);

        animation.AddClip(myclip, myclip.name);
        animation.clip = myclip;

        generatedAnimList.Add(animation);

    }

    public override Vector3 GetArrowPos()
    {
        if (meshCenter == null) { return new Vector3(); }
        return meshCenter.transform.position;
    }

    public override void PlayAnimation()
    {
        foreach(Animation animation in generatedAnimList)
        {
            //animation[animation.clip.name].wrapMode = WrapMode.Loop;
            // animation.Play(animation.transform.gameObject.name);
            animation.Play();
        }
        return;
    }

    public override void StopAnimation()
    {
        foreach (Animation animation in generatedAnimList)
        {
            // animation[animation.clip.name].wrapMode = WrapMode.Once;
            animation.Stop();
        }
        return;
    }


}
