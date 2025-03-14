﻿using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Mode/Directional Dodge")]
    public class ModifierForce : ModeModifier
    {
        [HelpBox]
        public string Desc ="Applies a Force to the Animal when the Mode starts. Remove the force when the mode ends";
        
        [Tooltip("Direction of the Force")]
        public Vector3Reference Direction = new Vector3Reference(Vector3.forward);
       
        [Tooltip("Amount of force to apply to the Animal")]
        public FloatReference Force = new FloatReference(2);
        [Tooltip("Time the Force will be applied to the Animal. if is set to Zero then it will be applied during the whole Animation")]
        public FloatReference m_Time = new FloatReference(0);
        [Tooltip("Start Acceleration of the force")]
        public FloatReference EnterAceleration = new FloatReference(5);
        [Tooltip("Exit Acceleration of the force")]
        public FloatReference ExitAceleration = new FloatReference(5);
        [Tooltip("When the Force is applied the Gravity will be Reseted")]
        public BoolReference ResetGravity = new BoolReference(true);

        public override void OnModeEnter(Mode mode)
        {
            mode.Animal.Force_Add(mode.Animal.transform.TransformDirection(Direction), Force, EnterAceleration, ResetGravity);
        }

        public override void OnModeMove(Mode mode, AnimatorStateInfo stateinfo, Animator anim, int Layer)
        {
            if (m_Time > 0 && 
                m_Time < Time.time - mode.ActivationTime &&
                mode.Animal.ExternalForce != Vector3.zero)
            {
                mode.Animal.Force_Remove(ExitAceleration);
            }
        }

        public override void OnModeExit(Mode mode) => mode.Animal.Force_Remove(ExitAceleration);
    }
}