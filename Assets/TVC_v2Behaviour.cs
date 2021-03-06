using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jundroo.SimplePlanes.ModTools;
using UnityEngine;

public class TVC_v2Behaviour : Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour
{
    ServiceProvider _service;
    IPlayerAircraft _player;
    TVC_v2 _modifier;
    Rigidbody _rb;
    Transform nozzle;
    Transform smoke;

    Transform centerOfThrust;
    float prevDir_Vtol = 0f;
    float prevAngle_Vtol = 0f;
    float prevAngle_Yaw = 0f;
    float prevDir_Yaw = 0f;
    Quaternion originalThrustAngle;

    bool tested; // FOR TESTING

    void Start()
    {
        _service = ServiceProvider.Instance;
        _player = _service.PlayerAircraft;
        _rb = this.GetComponentInParent<Rigidbody>();
        _modifier = (TVC_v2)this.PartModifier;

        //Get Engine Model
        Transform engine = getChildTransform(transform, "TVC_Engine");
        nozzle = getChildTransform(engine, "Nozzle");

        centerOfThrust = getChildTransform(engine, "CenterOfThrust");
        smoke = getChildTransform(transform, "EngineSmokeSystem");

        originalThrustAngle = centerOfThrust.rotation;

        //TESTING
        tested = false;
    }

    void FixedUpdate()
    {
        if (!_service.GameState.IsInDesigner)
        {
            VectorNozzles();
        }
    }

    private void VectorNozzles()
    {
        try
        {
            float angle_Vtol = _player.Controls.Vtol * _modifier.maxVectorAngle;
            float dir_Vtol = Math.Sign(-angle_Vtol - prevAngle_Vtol);

            float angle_Yaw = -_player.Controls.Yaw * _modifier.maxVectorAngle;
            float dir_Yaw = Math.Sign(angle_Yaw - prevAngle_Yaw);

            // Controls the Up/Down (pitch) angle of nozzle
            if (Math.Sign(angle_Vtol) <= _modifier.maxVectorAngle || Math.Sign(angle_Vtol) == _modifier.maxVectorAngle && dir_Vtol != prevDir_Vtol && dir_Vtol != 0)
            {
                centerOfThrust.Rotate(angle_Vtol - prevAngle_Vtol, 0, 0, Space.Self);
                smoke.Rotate(angle_Vtol - prevAngle_Vtol, 0, 0, Space.Self);
                nozzle.Rotate(angle_Vtol - prevAngle_Vtol, 0, 0, Space.Self);
            }

            // Controls the Left/Right (yaw) angle of nozzle
            if (Math.Sign(angle_Yaw) <= _modifier.maxVectorAngle || Math.Sign(angle_Yaw) == _modifier.maxVectorAngle && dir_Yaw != prevDir_Yaw && dir_Yaw != 0)
            {
                centerOfThrust.Rotate(0, (angle_Yaw - prevAngle_Yaw), 0, Space.Self);
                smoke.Rotate(0, 0, angle_Yaw - prevAngle_Yaw, Space.Self);
                nozzle.Rotate(0, 0, -(angle_Yaw - prevAngle_Yaw), Space.Self);
            }

            // Resets Left/Right (yaw) angle to 180f when yaw angle is zero. Without this, the center of thrust sometimes does not re-center correctly.
            if (_player.Controls.Yaw == 0 && centerOfThrust.localEulerAngles.y < 182f && centerOfThrust.localEulerAngles.y > 178f)
            {
                centerOfThrust.localEulerAngles = new Vector3(centerOfThrust.localEulerAngles.x, 180f, centerOfThrust.localEulerAngles.z);
            }

            prevDir_Vtol = dir_Vtol;
            prevAngle_Vtol = angle_Vtol;

            prevDir_Yaw = dir_Yaw;
            prevAngle_Yaw = angle_Yaw;
        }
        catch (Exception ex)
        {
            _service.GameWorld.ShowStatusMessage("VectorNozzles Method Error: " + ex.Message);
        }
    }

    private Transform getChildTransform(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.ToString().Contains(childName))
            {
                return child;
            }
        }

        return null;
    }
}