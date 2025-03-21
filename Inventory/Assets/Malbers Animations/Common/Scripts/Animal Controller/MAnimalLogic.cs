﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    public partial class MAnimal
    {
        void Awake()
        {
            if (Anim == null) Anim = GetComponentInParent<Animator>();   //Cache the Animator
            if (RB == null) RB = GetComponentInParent<Rigidbody>();      //Cache the Rigid Body  
            if (Aimer == null) Aimer = gameObject.FindComponent<Aim>();                     //Cache the Aim Component 

            DefaultCameraInput = UseCameraInput;

            if (NoParent) transform.parent = null; //IMPORTANT the animal cannot be parent of any game Object (Known Issue)


            if (Rotator != null)
            {
                if (RootBone == null)
                {
                    if (Anim.avatar.isHuman)
                        RootBone = Anim.GetBoneTransform(HumanBodyBones.Hips).parent; //Get the RootBone from
                    else
                        RootBone = Rotator.GetChild(0);           //Find the First Rotator Child  THIS CAUSE ISSUES WITH TIMELINE!!!!!!!!!!!!

                    if (RootBone == null)
                        Debug.LogWarning("Make sure the Root Bone is Set on the Advanced Tab -> Misc -> RootBone. This is the Character's Avatar root bone");
                }

                if (RootBone != null && !RootBone.IsGrandchild(Rotator)) //If the rootbone is not grandchild Parent it
                {
                    if (Rotator.position != RootBone.position)
                    {
                        var offset = new GameObject("Offset");
                        offset.transform.rotation = transform.rotation;
                        offset.transform.position = transform.position;

                        offset.transform.SetParent(Rotator);
                        RootBone.SetParent(offset.transform);

                        offset.transform.localScale = Vector3.one;
                        RootBone.localScale = Vector3.one;
                    }
                    else
                    {
                        RootBone.parent = Rotator;
                    }
                }
            }

            GetHashIDs();

            //Initialize all SpeedModifiers
            foreach (var set in speedSets) set.CurrentIndex = set.StartVerticalIndex;

            RB.useGravity = false;
            RB.constraints = RigidbodyConstraints.FreezeRotation;
            RB.drag = 0;

            //Initialize The Default Stance
            if (defaultStance == null)
            {
                defaultStance = ScriptableObject.CreateInstance<StanceID>();
                defaultStance.name = "Default";
                defaultStance.ID = 0;
            }

            if (currentStance == null) currentStance = defaultStance; //Set the current Stance

            GetAnimalColliders();

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] != null)
                {
                    if (CloneStates)
                    {
                        var instance = ScriptableObject.Instantiate(states[i]);         //Create a clone from the Original Scriptable Objects! IMPORTANT
                        instance.name = instance.name.Replace("(Clone)", "(C)");
                        states[i] = instance;
                    }

                    states[i].AwakeState(this);
                }
            }

            if (!CloneStates) Debug.LogWarning($"[{name}] has [ClonesStates] disabled. If multiple characters use the same states it will cause issues. Use this wisely");
            


            //Awake all modes
            for (int i = 0; i < modes.Count; i++)
            {
                modes[i].Priority = modes.Count - i;
                modes[i].AwakeMode(this);
            }

            SetPivots();
            CalculateHeight();

            currentSpeedSet = defaultSpeedSet;
            //new MSpeedSet() { Speeds = new List<MSpeed>(1) { new MSpeed("Default", 1, 4, 4) } }; //Create a Default Speed at Awake
            AlignUniqueID = UnityEngine.Random.Range(0, 99999);


            if (CanStrafe && !Aimer) Debug.LogWarning("This character can strafe but there's no Aim component. Please add the Aim component");

            //if (Anim.avatar == null)
            //    Debug.LogWarning("There's no Avatar on the Animator");
        }

        public virtual void ResetController()
        {
            if (MainCamera == null) //Find the Camera   
            {
                m_MainCamera.UseConstant = true;
                m_MainCamera.Value = MTools.FindMainCamera().transform;
            }

            if (Anim)
            {
                Anim.Rebind(); //Reset the Animator Controller
                Anim.speed = AnimatorSpeed;                         //Set the Global Animator Speed
                Anim.updateMode = AnimatorUpdateMode.AnimatePhysics;


                var AllModeBehaviours = Anim.GetBehaviours<ModeBehaviour>();

                if (AllModeBehaviours != null)
                {
                    foreach (var ModeB in AllModeBehaviours) ModeB.InitializeBehaviour(this);
                }
                else
                {
                    if (modes != null && modes.Count > 0)
                    {
                        Debug.LogWarning("Please check your Animator Controller. There's no Mode Behaviors Attached to it. Re-import the Animator again");
                    }
                }
            }

            foreach (var state in states)
            {
                state.InitializeState();
                state.InputValue = false;
                state.ResetState();
            }

            if (RB) RB.isKinematic = false; //Make use the Rigibody is not kinematic
            EnableColliders(true); //Make sure to enable all colliders



            CheckIfGrounded(); //Make the first Alignment 
            CalculateHeight();


            activeState = 
                OverrideStartState == null ?        //If we are not overriding
                states[states.Count - 1] :          //Set the First state as the active state (IMPORTANT TO BE THE FIRST THING TO DO)
                State_Get(OverrideStartState);      //Set the OverrideState
             
            
            ActiveStateID = activeState.ID;         //Set the New ActivateID
            activeState.Activate();
            lastState = activeState;                //Do not use the Properties....
          
            
            activeState.IsPending = false;          //Force the active state to start without entering the animation.
            ActiveState.CanExit = true;             //Force that it can exit... so another can activate it
            activeState.General.Modify(this);       //Force the active state to Modify all the Animal Settings

            JustActivateState = false;              //Force this to false

            State_SetFloat(0);

            UsingMoveWithDirection = (UseCameraInput); //IMPORTANT

            Mode_Stop();

            //Set Start with Mode
            if (StartWithMode.Value != 0)
            {
                if (StartWithMode.Value / 1000 == 0)
                {
                    Mode_Activate(StartWithMode.Value);
                }
                else
                {
                    var mode = StartWithMode.Value / 1000;
                    var modeAb = StartWithMode.Value % 1000;
                    if (modeAb == 0) modeAb = -99;
                    Mode_Activate(mode, modeAb);
                }
            }


            LastPos = transform.position; //Store Last Animal Position

            ForwardMultiplier = 1f; //Initialize the Forward Multiplier
            GravityMultiplier = 1f;

            MovementAxis =
            MovementAxisRaw =
            AdditivePosition =
            InertiaPositionSpeed =
            MovementAxisSmoothed = Vector3.zero; //Reset Vector Values

            LockMovementAxis = (new Vector3(LockHorizontalMovement ? 0 : 1, LockUpDownMovement ? 0 : 1, LockForwardMovement ? 0 : 1));

            UseRawInput = true; //Set the Raw Input as default.
            UseAdditiveRot = true;
            UseAdditivePos = true;
            Grounded = true;
            Randomizer = true;
            AlwaysForward = AlwaysForward;         // Execute the code inside Always Forward .... Why??? Don't know ..something to do with the Input stuff
            Strafe = Strafe;                       // Execute the code inside Strafe
            Stance = currentStance;
            GlobalOrientToGround = GlobalOrientToGround; // Execute the code inside Global Orient

            SpeedMultiplier = 1;
            CurrentCycle = Random.Range(0, 99999);
            ResetGravityValues();

            UpdateDamagerSet();

            var TypeHash = TryOptionalParameter(m_Type);
            SetOptionalAnimParameter(TypeHash, animalType); //This is only done once!
        }

        [ContextMenu("Set Pivots")]
        public void SetPivots()
        {
            Pivot_Hip = pivots.Find(item => item.name.ToUpper() == "HIP");
            Pivot_Chest = pivots.Find(item => item.name.ToUpper() == "CHEST");

            Has_Pivot_Hip = Pivot_Hip != null;
            Has_Pivot_Chest = Pivot_Chest != null;
            Starting_PivotChest = Has_Pivot_Chest;

            CalculateHeight();

#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }


        public void OnEnable()
        {
            if (Animals == null) Animals = new List<MAnimal>();
            Animals.Add(this);                                              //Save the the Animal on the current List

            ResetInputSource(); //Connect the Inputs

            if (isPlayer) SetMainPlayer();

            SetBoolParameter += SetAnimParameter;
            SetIntParameter += SetAnimParameter;
            SetFloatParameter += SetAnimParameter;
            SetTriggerParameter += SetAnimParameter;

            if (!alwaysForward.UseConstant && alwaysForward.Variable != null)
                alwaysForward.Variable.OnValueChanged += Always_Forward;

            ResetController();
            Sleep = false;
        }

        public void OnDisable()
        {
            if (Animals != null) Animals.Remove(this);       //Remove all this animal from the Overall AnimalList

            UpdateInputSource(false); //Disconnect the inputs

            DisableMainPlayer();

            MTools.ResetFloatParameters(Anim); //Reset all Anim Floats!!
            RB.velocity = Vector3.zero;

            SetBoolParameter -= SetAnimParameter;
            SetIntParameter -= SetAnimParameter;
            SetFloatParameter -= SetAnimParameter;
            SetTriggerParameter -= SetAnimParameter;


            if (!alwaysForward.UseConstant && alwaysForward.Variable != null)
                alwaysForward.Variable.OnValueChanged -= Always_Forward;

            if (states != null)
            {
                foreach (var st in states)
                    if (st != null) st.ExitState();
            }


            if (ActiveMode != null) ActiveMode.PlayingMode = false;
            Mode_Stop();
        }


        public void CalculateHeight()
        {
            if (Has_Pivot_Hip)
            {
                if (height == 1) height = Pivot_Hip.position.y;
                Center = Pivot_Hip.position; //Set the Center to be the Pivot Hip Position
            }
            else if (Has_Pivot_Chest)
            {
                if (height == 1) height = Pivot_Chest.position.y;
                Center = Pivot_Chest.position;
            }

            if (Has_Pivot_Chest && Has_Pivot_Hip)
            {
                Center = (Pivot_Chest.position + Pivot_Hip.position) / 2;
            }
        }

        /// <summary>Update all the Attack Triggers Inside the Animal... In case there are more or less triggers</summary>
        public void UpdateDamagerSet()
        {
            Attack_Triggers = GetComponentsInChildren<IMDamager>(true).ToList();        //Save all Attack Triggers.
            foreach (var at in Attack_Triggers)
            {
                at.Owner = (gameObject);                 //Tell to avery Damager that this Animal is the Owner
                at.Active = false;
            }
        }

        public void AttackTriggers_Update() => UpdateDamagerSet();

        #region Animator Stuff
        protected virtual void GetHashIDs()
        {
            if (Anim == null) return;


            //Store all the Animator Parameter in a Dictionary
            //animatorParams = new Hashtable();
            animatorHashParams = new List<int>();

            foreach (var parameter in Anim.parameters)
            { 
                animatorHashParams.Add(parameter.nameHash);
            }

            #region Main Animator Parameters
            //Movement
            hash_Vertical = Animator.StringToHash(m_Vertical);
            hash_Horizontal = Animator.StringToHash(m_Horizontal);
            hash_SpeedMultiplier = Animator.StringToHash(m_SpeedMultiplier);

            hash_Movement = Animator.StringToHash(m_Movement);
            hash_Grounded = Animator.StringToHash(m_Grounded);

            //States
            hash_State = Animator.StringToHash(m_State);
            hash_StateEnterStatus = Animator.StringToHash(m_StateStatus);


            hash_LastState = Animator.StringToHash(m_LastState);
            hash_StateFloat = Animator.StringToHash(m_StateFloat);

            //Modes
            hash_Mode = Animator.StringToHash(m_Mode);
        
            hash_ModeStatus = Animator.StringToHash(m_ModeStatus);
            #endregion

            #region Optional Parameters

            //Movement 
            hash_StateExitStatus = TryOptionalParameter(m_StateExitStatus);
            //hash_StateEnterStatus = TryOptionalParameter(m_StateStatus);
            hash_SpeedMultiplier = TryOptionalParameter(m_SpeedMultiplier);

            hash_UpDown = TryOptionalParameter(m_UpDown);
            hash_DeltaUpDown = TryOptionalParameter(m_DeltaUpDown);

            hash_Slope = TryOptionalParameter(m_Slope);


            hash_DeltaAngle = TryOptionalParameter(m_DeltaAngle);
            hash_Sprint = TryOptionalParameter(m_Sprint);

            //States
            hash_StateTime = TryOptionalParameter(m_StateTime);


            hash_Strafe = TryOptionalParameter(m_Strafe);
            hash_StrafeAngle = TryOptionalParameter(m_strafeAngle);

            //Stance
            hash_Stance = TryOptionalParameter(m_Stance);
         
            hash_LastStance = TryOptionalParameter(m_LastStance);

            //Misc
            hash_Random = TryOptionalParameter(m_Random);
            hash_ModePower = TryOptionalParameter(m_ModePower);

            //Triggers
            hash_ModeOn = TryOptionalParameter(m_ModeOn);
            hash_StateOn = TryOptionalParameter(m_StateOn);
           // hash_StanceOn = TryOptionalParameter(m_StanceOn);
            #endregion
        }


        //Send 0 if the Animator does not contain
        private int TryOptionalParameter(string param)
        {
            var AnimHash = Animator.StringToHash(param);

            if (!animatorHashParams.Contains(AnimHash))
                return 0;
            return AnimHash;
        }

        protected virtual void CacheAnimatorState()
        {
            m_PreviousCurrentState = m_CurrentState;
            m_PreviousNextState = m_NextState;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentState = Anim.GetCurrentAnimatorStateInfo(0);
            m_NextState = Anim.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = Anim.IsInTransition(0);

            if (m_IsAnimatorTransitioning)
            {
                if (m_NextState.fullPathHash != 0)
                {
                    AnimStateTag = m_NextState.tagHash;
                    AnimState = m_NextState;
                }
            }
            else
            {
                if (m_CurrentState.fullPathHash != AnimState.fullPathHash)
                {
                    AnimStateTag = m_CurrentState.tagHash;
                }

                AnimState = m_CurrentState;
            }

            var lastStateTime = StateTime;
            // = m_CurrentState.normalizedTime;
            StateTime = Mathf.Repeat(AnimState.normalizedTime, 1);

          //  Debug.Log("StateTime = " + StateTime);

            if (lastStateTime > StateTime) StateCycle?.Invoke(ActiveStateID); //Check if the Animation Started again.
        }

        /// <summary>Link all Parameters to the animator</summary>
        protected virtual void UpdateAnimatorParameters()
        {
            SetFloatParameter(hash_Vertical, VerticalSmooth);
            SetFloatParameter(hash_Horizontal, HorizontalSmooth);

            SetOptionalAnimParameter(hash_UpDown, UpDownSmooth);
            SetOptionalAnimParameter(hash_DeltaUpDown, DeltaUpDown);


            SetOptionalAnimParameter(hash_DeltaAngle, DeltaAngle);
            SetOptionalAnimParameter(hash_Slope, SlopeNormalized);
            SetOptionalAnimParameter(hash_SpeedMultiplier, SpeedMultiplier);
            SetOptionalAnimParameter(hash_StateTime, StateTime);
        }
        #endregion

        #region Inputs 
        internal void InputAxisUpdate()
        {
            if (UseRawInput)
            {
                if (AlwaysForward)
                    RawInputAxis.z = 1;

                var inputAxis = RawInputAxis;

                if (LockMovement || Sleep)
                {
                    MovementAxis = Vector3.zero;
                    return;
                }

                if (MainCamera && UsingMoveWithDirection && !Strafe)
                {
                    var Cam_Forward = Vector3.ProjectOnPlane(MainCamera.forward, UpVector).normalized; //Normalize the Camera Forward Depending the Up Vector IMPORTANT!
                    var Cam_Right = Vector3.ProjectOnPlane(MainCamera.right, UpVector).normalized;

                    Vector3 UpInput;

                    if (!FreeMovement)
                    {
                        UpInput = Vector3.zero;            //Reset the UP Input in case is on the Ground
                    }
                    else
                    {
                        if (UseCameraUp)
                        {
                            UpInput = (inputAxis.y * MainCamera.up);
                            UpInput += Vector3.Project(MainCamera.forward, UpVector) * inputAxis.z;
                        }
                        else
                        {
                            UpInput = (inputAxis.y * UpVector);

                            //if (inputAxis.y != 0 && inputAxis.z == 0)
                            //{
                            //    inputAxis.z = 0.01f; //Hack to add a bit of Forward movement to find which direction is forward
                            //}
                        }
                    }

                    var m_Move = (inputAxis.z * Cam_Forward) + (inputAxis.x * Cam_Right) + UpInput;

                    MoveFromDirection(m_Move);
                }
                else
                {
                    MoveWorld(inputAxis);
                }
            }
            else //Means that is Using a Direction Instead  Update every frame
            {
                MoveFromDirection(RawInputAxis);
            }
        }


        /// <summary>Get the Raw Input Axis from a source</summary>
        public virtual void SetInputAxis(Vector3 inputAxis)
        {
            UseRawInput = true;
            RawInputAxis = inputAxis; // Store the last current use of the Input
            if (UsingUpDownExternal)
                RawInputAxis.y = UpDownAdditive; //Add the UPDown Additive from the Mobile.
        }

         

        public virtual void SetInputAxis(Vector2 inputAxis) => SetInputAxis(new Vector3(inputAxis.x, 0, inputAxis.y));

        public virtual void SetInputAxisXY(Vector2 inputAxis) => SetInputAxis(new Vector3(inputAxis.x, inputAxis.y, 0));

        public virtual void SetInputAxisYZ(Vector2 inputAxis) => SetInputAxis(new Vector3(0, inputAxis.x, inputAxis.y));

        private float UpDownAdditive;
        private bool UsingUpDownExternal;

        /// <summary>Use this for Custom UpDown Movement</summary>
        public virtual void SetUpDownAxis(float upDown)
        {
            UpDownAdditive = upDown;
            UsingUpDownExternal = true;
            SetInputAxis(RawInputAxis); //Call the Raw IMPORTANT
        }

        /// <summary>Gets the movement from the World Coordinates</summary>
        /// <param name="move">World Direction Vector</param>
        protected virtual void MoveWorld(Vector3 move)
        {
            UsingMoveWithDirection = false;

            if (!UseSmoothVertical && move.z > 0) move.z = 1;                    //It will remove slowing Stick push when rotating and going Forward
            Move_Direction = transform.TransformDirection(move).normalized;    //Convert from world to relative IMPORTANT

            SetMovementAxis(move);
        }

        private void SetMovementAxis(Vector3 move)
        {
            MovementAxisRaw = move;
            MovementAxisRaw.z *= ForwardMultiplier;

            MovementAxis = MovementAxisRaw;
            MovementDetected = MovementAxisRaw != Vector3.zero;

            MovementAxis.Scale(LockMovementAxis);
            MovementAxis.Scale(ActiveState.MovementAxisMult);
        }

        /// <summary>Gets the movement values from a Direction</summary>
        /// <param name="move">Direction Vector</param>
        public virtual void MoveFromDirection(Vector3 move)
        {
            if (LockMovement)
            {
                MovementAxis = Vector3.zero;
                return;
            }

            UsingMoveWithDirection = true;
             
            if (move.magnitude > 1f) move.Normalize();
           
            var UpDown = FreeMovement ?  move.y : 0; //Ignore UP Down Axis when the Animal is not on Free movement


            if (!FreeMovement)
                move = Quaternion.FromToRotation(UpVector, SurfaceNormal) * move;    //Rotate with the ground Surface Normal. CORRECT!

            Move_Direction = move;
            move = transform.InverseTransformDirection(move);               //Convert the move Input from world to Local  

            //     Debug.DrawRay(transform.position, move * 5, Color.yellow);

            float turnAmount = Mathf.Atan2(move.x, move.z);                 //Convert it to Radians
            float forwardAmount = move.z < 0 ? 0 : move.z;

            // Find the difference between the current rotation of the player and the desired rotation of the player in radians.
            float angleCurrent = Mathf.Atan2(Forward.x, Forward.z) * Mathf.Rad2Deg;

            float targetAngle = Mathf.Atan2(Move_Direction.x, Move_Direction.z) * Mathf.Rad2Deg;
            var Delta = Mathf.DeltaAngle(angleCurrent, targetAngle);

            DeltaAngle = MovementDetected ? Delta : 0;

            if (Mathf.Approximately(Delta, float.NaN))  DeltaAngle = 0f; //Remove the NAN Bug
         
            if (Mathf.Abs(Vector3.Dot(Move_Direction, UpVector)) == 1)//Remove turn Mount when its goinf UP/Down
            {
                turnAmount = 0; 
                DeltaAngle = 0f;
            }
         

            //It will remove slowing Stick push when rotating and going Forward
            if (!UseSmoothVertical)
            {
                forwardAmount = Mathf.Abs(move.z);
                forwardAmount = forwardAmount > 0 ? 1 : forwardAmount;
            }
            else
            {
                if (Mathf.Abs(DeltaAngle) < TurnLimit)
                    forwardAmount = Mathf.Clamp01(Move_Direction.magnitude);
            }


           // if (Rotate_at_Direction)  forwardAmount = 0;
           

            SetMovementAxis(new Vector3(turnAmount, UpDown, forwardAmount));
        }

        /// <summary>Gets the movement from a Direction but it wont fo forward it will only rotate in place</summary>
        public virtual void RotateAtDirection(Vector3 direction)
        {
            if (IsPlayingMode && !ActiveMode.AllowRotation) return;

            RawInputAxis = direction; // Store the last current use of the Input
            UseRawInput = false;
            Rotate_at_Direction = true;
        }
        #endregion

        #region Additional Speeds (Movement, Turn) 

        public void CalculateTargetSpeed()
        {
            //var lerp = CurrentSpeedModifier.lerpPosition * DeltaTime;

            if ((!UseAdditivePos) ||                            //Do nothing when UseAdditivePos is False
                 (IsPlayingMode && !ActiveMode.AllowMovement))  //Do nothing when the Mode Locks the Movement
            {
                //TargetSpeed = Vector3.Lerp(TargetSpeed, Vector3.zero, lerp);
                TargetSpeed = Vector3.zero;
                return;
            }

            Vector3 TargetDir = ActiveState.Speed_Direction();

            float Speed_Modifier = Strafe ? CurrentSpeedModifier.strafeSpeed.Value : CurrentSpeedModifier.position.Value;

            if (Strafe)
            {
                TargetDir = (Forward * VerticalSmooth) + (Right * HorizontalSmooth);

                if (FreeMovement)
                    TargetDir += (Up * UpDownSmooth);

            }
            else
            {
                if ((VerticalSmooth < 0) && CurrentSpeedSet != null)//Decrease when going backwards and NOT Strafing
                {
                    TargetDir *= -CurrentSpeedSet.BackSpeedMult.Value;
                    Speed_Modifier = CurrentSpeedSet[0].position;
                }
                if (FreeMovement)
                {
                    float SmoothZYInput = Mathf.Clamp01(Mathf.Max(Mathf.Abs(UpDownSmooth), Mathf.Abs(VerticalSmooth))); // Get the Average Multiplier of both Z and Y Inputs
                    TargetDir *= SmoothZYInput;
                }
                else  
                {
                    TargetDir *= VerticalSmooth; //Use Only the Vertical Smooth while grounded
                }

            }

            if (TargetDir.magnitude > 1) TargetDir.Normalize();
            TargetSpeed = TargetDir * Speed_Modifier * DeltaTime * ScaleFactor;   //Calculate these Once per Cycle Extremely important 

            //TargetSpeed = Vector3.Lerp(TargetSpeed, TargetDir * Speed_Modifier * DeltaTime * ScaleFactor, lerp);   //Calculate these Once per Cycle Extremely important 

            HorizontalVelocity = Vector3.ProjectOnPlane(Inertia, UpVector);
            HorizontalSpeed = HorizontalVelocity.magnitude;
        }

        private void MoveRotator()
        {
            if (!FreeMovement && Rotator)
            {
                if (PitchAngle != 0 || Bank != 0)
                {
                    float limit = 0.005f;
                    var lerp = DeltaTime * (CurrentSpeedSet.PitchLerpOff);

                    Rotator.localRotation = Quaternion.Slerp(Rotator.localRotation, Quaternion.identity, lerp);

                    PitchAngle = Mathf.Lerp(PitchAngle, 0, lerp); //Lerp to zero the Pitch Angle when goind Down
                    Bank = Mathf.Lerp(Bank, 0, lerp);

                    if (Mathf.Abs(PitchAngle) < limit && Mathf.Abs(Bank) < limit)
                    {
                        Bank = PitchAngle = 0;
                        Rotator.localRotation = Quaternion.identity;
                    }
                }
            }
            else
            {
                CalculatePitchDirectionVector();
            }
        }

        public virtual void FreeMovementRotator(float Ylimit, float bank)
        {
            CalculatePitch(Ylimit);
            CalculateBank(bank);
            CalculateRotator();
        }

        internal virtual void CalculateRotator()
        {
            Rotator.localEulerAngles = new Vector3(PitchAngle, 0, Bank); //Angle for the Rotator
        }
        internal virtual void CalculateBank(float bank) => Bank = Mathf.Lerp(Bank, -bank * Mathf.Clamp(HorizontalSmooth, -1, 1), DeltaTime * CurrentSpeedSet.BankLerp);
        internal virtual void CalculatePitch(float Ylimit)
        {
            float NewAngle = 0;

            if (PitchDirection.sqrMagnitude > 0.0001)             //Rotation PITCH
            {
                NewAngle = 90 - Vector3.Angle(UpVector, PitchDirection);
                NewAngle = Mathf.Clamp(-NewAngle, -Ylimit, Ylimit);
            }

            var deltatime = DeltaTime * CurrentSpeedSet.PitchLerpOn;

            PitchAngle = Mathf.Lerp(PitchAngle, NewAngle, deltatime);
            DeltaUpDown = Mathf.Lerp(DeltaUpDown, -Mathf.DeltaAngle(PitchAngle, NewAngle), deltatime * 2);

            if (Mathf.Abs(DeltaUpDown) < 0.01f) DeltaUpDown = 0;
        }


        /// <summary>Calculates the Pitch direction to Appy to the Rotator Transform</summary>
        internal virtual void CalculatePitchDirectionVector()
        {
            var dir = Move_Direction != Vector3.zero ? Move_Direction : Forward;
            PitchDirection = Vector3.Lerp(PitchDirection, dir, DeltaTime * CurrentSpeedSet.PitchLerpOn * 2);
        }

        /// <summary> Add more Speed to the current Move animations</summary>  
        protected virtual void AdditionalSpeed(float time)
        {
           // if (IsPlayingMode && !ActiveMode.AllowMovement) return; //Do nothing because the mode does not allow Additional Speed

            var LerpPos = CurrentSpeedModifier.lerpPosition;

            InertiaPositionSpeed = (LerpPos > 0) ? Vector3.Lerp(InertiaPositionSpeed, TargetSpeed, time * LerpPos) : TargetSpeed;

            AdditivePosition += InertiaPositionSpeed;
        }


        /// <summary>Add more Rotations to the current Turn Animations  </summary>
        protected virtual void AdditionalTurn(float time)
        {
            float SpeedRotation = CurrentSpeedModifier.rotation;

            if (VerticalSmooth < 0.01 && !CustomSpeed && CurrentSpeedSet != null)
            {
                SpeedRotation = CurrentSpeedSet[0].rotation;
            }

            if (SpeedRotation < 0) return;          //Do nothing if the rotation is lower than 0

            if (MovementDetected)
            {
                float ModeRotation = (IsPlayingMode && !ActiveMode.AllowRotation) ? 0 : 1;//If the mode does not allow rotation set the multiplier to zero

                if (UsingMoveWithDirection)
                {
                    var TargetLocalRot = Quaternion.Euler(0, DeltaAngle, 0);
                    Quaternion targetRotation = Quaternion.Slerp(Quaternion.identity, TargetLocalRot, (SpeedRotation + 1) / 4 * ((TurnMultiplier + 1) * time * ModeRotation));
                    AdditiveRotation *= targetRotation;
                }
                else
                {
                    float Turn = SpeedRotation * 10;           //Add Extra Multiplier
                    float TurnInput = Mathf.Clamp(HorizontalSmooth, -1, 1) * (MovementAxis.z >= 0 ? 1 : -1);  //Add +Rotation when going Forward and -Rotation when going backwards
                    AdditiveRotation *= Quaternion.Euler(0, Turn * TurnInput * time * ModeRotation, 0);
                    var TargetGlobal = Quaternion.Euler(0, TurnInput * (TurnMultiplier + 1), 0);
                    var AdditiveGlobal = Quaternion.Slerp(Quaternion.identity, TargetGlobal, time * (SpeedRotation + 1) * ModeRotation);
                    AdditiveRotation *= AdditiveGlobal;
                }
            }
        }


        /// <summary> Movement Trot Walk Run (Velocity changes)</summary>
        internal void MovementSystem(float DeltaTime)
        {
            float maxspeedV = CurrentSpeedModifier.Vertical;
            float maxspeedH = 1;

            var LerpUpDown = DeltaTime * CurrentSpeedSet.PitchLerpOn;
            var LerpVertical = DeltaTime * CurrentSpeedModifier.lerpPosAnim;
            var LerpTurn = DeltaTime * CurrentSpeedModifier.lerpRotAnim;
            var LerpAnimator = DeltaTime * CurrentSpeedModifier.lerpAnimator;

            if (Strafe)
            {
                maxspeedH = maxspeedV; //if the animal is strafing
                LerpVertical = LerpTurn = LerpUpDown = DeltaTime * CurrentSpeedModifier.lerpStrafe;
            }

            if (IsPlayingMode && !ActiveMode.AllowMovement) //Active mode and Isplaying Mode is failing!!**************
                MovementAxis = Vector3.zero;

            var Horiz = Mathf.Lerp(HorizontalSmooth, MovementAxis.x * maxspeedH, LerpTurn);

            float v = MovementAxis.z;
           
            
            if (Rotate_at_Direction)
            {
                float r = 0;
                v = 0; //Remove the Forward since its
                Horiz = Mathf.SmoothDamp(HorizontalSmooth, MovementAxis.x * 4, ref r, inPlaceDamp * DeltaTime); //Using properly the smooth  down
            }

            VerticalSmooth = LerpVertical > 0 ?
                Mathf.Lerp(VerticalSmooth, v * maxspeedV, LerpVertical) :
                MovementAxis.z * maxspeedV;           //smoothly transitions bettwen Speeds
         
            
            HorizontalSmooth = LerpTurn > 0 ? Horiz : MovementAxis.x * maxspeedH;               //smoothly transitions bettwen Directions

            UpDownSmooth = LerpVertical > 0 ?
                Mathf.Lerp(UpDownSmooth, MovementAxis.y, LerpUpDown) :
                MovementAxis.y;                                                //smoothly transitions bettwen Directions


            SpeedMultiplier = (LerpAnimator > 0) ?
                Mathf.Lerp(SpeedMultiplier, CurrentSpeedModifier.animator.Value, LerpAnimator) :
                CurrentSpeedModifier.animator.Value;  //Changue the velocity of the animator

            var zero = 0.005f;

            if (Mathf.Abs(VerticalSmooth) < zero) VerticalSmooth = 0;
            if (Mathf.Abs(HorizontalSmooth) < zero) HorizontalSmooth = 0;
            if (Mathf.Abs(UpDownSmooth) < zero) UpDownSmooth = 0;
        }


        #endregion

        #region Platorm movement
        public void SetPlatform(Transform newPlatform)
        {
            platform = newPlatform;
            platform_LastPos = platform.position;
            platform_Rot = platform.rotation;
        }

        public void PlatformMovement()
        {
            if (platform == null) return;
            if (platform.gameObject.isStatic) return; //means it cannot move

            var DeltaPlatformPos = platform.position - platform_LastPos;
            //AdditivePosition.y += DeltaPlatformPos.y;
            //DeltaPlatformPos.y = 0;                                 //the Y is handled by the Fix Position

            transform.position += DeltaPlatformPos;                 //Set it Directly to the Transform.. Additive Position can be reset any time..


            Quaternion Inverse_Rot = Quaternion.Inverse(platform_Rot);
            Quaternion Delta = Inverse_Rot * platform.rotation;

            if (Delta != Quaternion.identity)                                        // no rotation founded.. Skip the code below
            {
                var pos = transform.DeltaPositionFromRotate(platform, Delta);

                // AdditivePosition += pos;
                transform.position += pos;   //Set it Directly to the Transform.. Additive Position can be reset any time..
            }

            //AdditiveRotation *= Delta;
            transform.rotation *= Delta;  //Set it Directly to the Transform.. Additive Position can be reset any time..

            platform_LastPos = platform.position;
            platform_Rot = platform.rotation;
        }
        #endregion


        #region Terrain Alignment
        /// <summary>Raycasting stuff to align and calculate the ground from the animal ****IMPORTANT***</summary>
        internal virtual void AlignRayCasting()
        {
            MainRay = FrontRay = false;
            hit_Chest = new RaycastHit() { normal = Vector3.zero };                               //Clean the Raycasts every time 
            hit_Hip = new RaycastHit();                                 //Clean the Raycasts every time 
            hit_Chest.distance = hit_Hip.distance = Height;            //Reset the Distances to the Heigth of the animal

            //var Direction = Gravity;
            var Direction = -transform.up;

            if (Physics.Raycast(Main_Pivot_Point, Direction, out hit_Chest, Pivot_Multiplier, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                FrontRay = true;

                if (debugGizmos)
                {
                    Debug.DrawRay(hit_Chest.point, hit_Chest.normal * ScaleFactor * 0.2f, Color.green);
                    MTools.DrawWireSphere(Main_Pivot_Point + Direction * (hit_Chest.distance - RayCastRadius), Color.green, RayCastRadius * ScaleFactor);
                }

                MainPivotSlope = Vector3.SignedAngle(hit_Chest.normal, UpVector, Right);


                if (MainPivotSlope > maxAngleSlope)  //Means that the Slope is higher thanthe Max slope so stop the animal from Going forward AND ONLY ON LOCOMOTION 
                {
                    if (MovementAxisRaw.z > 0 && !hit_Chest.transform.gameObject.CompareTag(DebrisTag))
                    {
                        AdditivePosition = Vector3.ProjectOnPlane(AdditivePosition, Forward); //Remove Forward Movement
                        MovementAxis.z = 0;
                    }
                }
                else if (MainPivotSlope < -maxAngleSlope) //Meaning it has touched the ground but the angle too deep
                {
                    FrontRay = false;
                }
                else
                {
                    if (platform != hit_Chest.transform)               //Platforming logic
                        SetPlatform(hit_Chest.transform);

                    hit_Chest.collider.attachedRigidbody?.AddForceAtPosition(Gravity * (RB.mass / 2), hit_Chest.point, ForceMode.Force);
                }
            }
            else
            {
                platform = null;
            }

            if (Has_Pivot_Hip && Has_Pivot_Chest) //Ray From the Hip to the ground
            {
                var hipPoint = Pivot_Hip.World(transform) + DeltaVelocity;

                if (Physics.Raycast(hipPoint, Direction, out hit_Hip, ScaleFactor * Pivot_Hip.multiplier, GroundLayer, QueryTriggerInteraction.Ignore))
                {

                    var MainPivotSlope = Vector3.SignedAngle(hit_Hip.normal, UpVector, Right);

                    if (MainPivotSlope < -maxAngleSlope) //Meaning it has touched the ground but the angle too deep
                    {
                        MainRay = false; //meaning the Slope is too deep
                    }
                    else
                    {
                        MainRay = true;

                        if (debugGizmos)
                        {
                            Debug.DrawRay(hit_Hip.point, hit_Hip.normal * ScaleFactor * 0.2f, Color.green);
                            MTools.DrawWireSphere(hipPoint + Direction * (hit_Hip.distance - RayCastRadius), Color.green, RayCastRadius * ScaleFactor);
                        }

                        if (platform != hit_Hip.transform) SetPlatform(hit_Hip.transform);               //Platforming logic

                        hit_Hip.collider.attachedRigidbody?.AddForceAtPosition(Gravity * (RB.mass / 2), hit_Hip.point, ForceMode.Force);

                        if (!FrontRay/* && hit_Chest.normal == Vector3.zero*/) hit_Chest = hit_Hip; //If there's no Front Ray and it not collide with anything
                    }
                }
                else
                {
                    platform = null;

                    if (FrontRay)
                    {
                        MovementAxis.z = 1; //Force going forward in case there's no Back Ray (HACK)
                        hit_Hip = hit_Chest;  //In case there's no Hip Ray
                    }
                }
            }
            else
            {
                MainRay = FrontRay; //Just in case you dont have HIP RAY IMPORTANT FOR HUMANOID CHARACTERS
                hit_Hip = hit_Chest;  //In case there's no Hip Ray
            }

            if (ground_Changes_Gravity)
                Gravity = -hit_Hip.normal;


            // Debug.Log($"MainRay{MainRay} FrontRay{FrontRay}");

            CalculateSurfaceNormal();
        }

        internal virtual void CalculateSurfaceNormal()
        {
            if (Has_Pivot_Hip)
            {
                Vector3 TerrainNormal;

                if (Has_Pivot_Chest)
                {
                    Vector3 direction = (hit_Chest.point - hit_Hip.point).normalized;
                    Vector3 Side = Vector3.Cross(UpVector, direction).normalized;
                    SurfaceNormal = Vector3.Cross(direction, Side).normalized;

                    TerrainNormal = SurfaceNormal;
                }
                else
                {
                    SurfaceNormal = TerrainNormal = hit_Hip.normal;
                }

                TerrainSlope = Vector3.SignedAngle(TerrainNormal, UpVector, Right);
            }
            else
            {
                TerrainSlope = Vector3.SignedAngle(hit_Hip.normal, UpVector, Right);
                SurfaceNormal = UpVector;
            }
        }

        /// <summary>Align the Animal to Terrain</summary>
        /// <param name="align">True: Aling to Surface Normal, False: Align to Up Vector</param>
        internal virtual void AlignRotation(bool align, float time, float smoothness)
        {
            AlignRotation(align ? SurfaceNormal : UpVector, time, smoothness);
        }

        /// <summary>Align the Animal to a Custom </summary>
        /// <param name="align">True: Aling to UP, False Align to Terrain</param>
        internal virtual void AlignRotation(Vector3 alignNormal, float time, float Smoothness)
        {
            AlignRotLerpDelta = Mathf.Lerp(AlignRotLerpDelta, Smoothness, time * AlignRotDelta * 4);

            Quaternion AlignRot = Quaternion.FromToRotation(transform.up, alignNormal) * transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, time * AlignRotLerpDelta); //Calculate the Delta Align Rotation

            transform.rotation *= Delta;
            //AdditiveRotation *= Delta;
        }

        /// <summary>Snap to Ground with Smoothing</summary>
        internal void AlignPosition(float time)
        {
            if (!MainRay && !FrontRay) return;         //DO NOT ALIGN  IMPORTANT This caused the animals jumping upwards when falling down
            AlignPosition(hit_Hip.distance, time, AlignPosLerp * 2);
        }

        internal void AlignPosition(float distance, float time, float Smoothness)
        {
            //Debug.Log(distance);
            float difference = Height - distance;

            if (!Mathf.Approximately(distance, Height))
            {
                AlignPosLerpDelta = Mathf.Lerp(AlignPosLerpDelta, Smoothness, time * AlignPosDelta);

                var deltaHeight = difference * time * AlignPosLerpDelta;

                Vector3 align = transform.rotation * new Vector3(0, deltaHeight, 0); //Rotates with the Transform to better alignment
                AdditivePosition += align;

                hit_Hip.distance += deltaHeight;
            }
        }

        /// <summary>Snap to Ground with no Smoothing</summary>
        internal virtual void AlignPosition_Distance(float distance)
        {
            float difference = Height - distance;
            AdditivePosition += transform.rotation * new Vector3(0, difference, 0); //Rotates with the Transform to better alignment
        }


        /// <summary>Snap to Ground with no Smoothing</summary>
        internal virtual void AlignPosition()
        {
            float difference = Height - hit_Hip.distance;
            AdditivePosition += transform.rotation * new Vector3(0, difference, 0); //Rotates with the Transform to better alignment
        }
        #endregion

        /// <summary> Try Activate all other states </summary>
        protected virtual void TryActivateState()
        {
            if (ActiveState.IsPersistent) return;        //If the State cannot be interrupted the ignored trying activating any other States
            if (JustActivateState) return;               //Do not try to activate a new state since there already a new one on Activation


            //Wake UP the State that is no longer on QUEUE and it was activated! (PRIORITY FOR THE QUEDED STATES)!
            //if (QueueReleased)
            //{
            //    QueueState.ActivateQueued();
            //    return;
            //}


            foreach (var trySt in states)
            {
                if (trySt.IsActiveState) continue;      //Skip Re-Activating yourself
                if (ActiveState.IgnoreLowerStates && ActiveState.Priority > trySt.Priority) return; //Do not check lower priority states

                if ((trySt.UniqueID + CurrentCycle) % trySt.TryLoop != 0) continue;   //Check the Performance Loop for the  trying state

                if (!ActiveState.IsPending && ActiveState.CanExit)    //Means a new state can be activated
                {
                    if (trySt.Active &&
                        !trySt.OnEnterCoolDown &&
                        !trySt.IsSleep &&
                        !trySt.OnQueue &&
                         trySt.TryActivate())
                    {
                        trySt.Activate();
                        break;
                    }
                }
            }
        }

        /// <summary>Check if the Active State can exit </summary>
        protected virtual void TryExitActiveState()
        {
            if (ActiveState.CanExit)
                ActiveState.TryExitState(DeltaTime);     //if is not in transition and is in the Main Tag try to Exit to lower States
        }


        //private void FixedUpdate() => OnAnimalMove();

        protected virtual void OnAnimatorMove() => OnAnimalMove();


        protected virtual void OnAnimalMove()
        {
            if (Sleep)
            {
                Anim.ApplyBuiltinRootMotion();
                return;
            }

            CacheAnimatorState();
            ResetValues();

            if (ActiveState == null) return;

            DeltaTime = Time.fixedDeltaTime;

            InputAxisUpdate(); //make sure when using Raw Input UPDATE the Calculations ****IMPORTANT****

            ActiveState.SetCanExit(); //Check if the Active State can Exit to a new State (Was not Just Activated or is in transition)

            PreStateMovement(this);                         //Check the Pre State Movement on External Scripts

            ActiveState.OnStatePreMove(DeltaTime);          //Call before the Target is calculated After the Input

            CalculateTargetSpeed();

            MoveRotator();

            if (IsPlayingMode) ActiveMode.OnAnimatorMove(DeltaTime); //Do Charged Mode

            if (UseAdditivePos) AdditionalSpeed(DeltaTime);
            if (UseAdditiveRot) AdditionalTurn(DeltaTime);

            ApplyExternalForce();

            if (Grounded)
            {

                if (AlignLoop.Value <= 1 || (AlignUniqueID + CurrentCycle) % AlignLoop.Value == 0)
                    AlignRayCasting();

                AlignPosition(DeltaTime);

                if (!UseCustomAlign)
                    AlignRotation(UseOrientToGround, DeltaTime, AlignRotLerp);

                PlatformMovement();
            }
            else
            {
                MainRay = FrontRay = false;
                SurfaceNormal = UpVector;
                AlignPosLerpDelta = 0;
                AlignRotLerpDelta = 0;

                if (!UseCustomAlign)
                    AlignRotation(false, DeltaTime, AlignRotLerp); //Align to the Gravity Normal
                TerrainSlope = 0;
            }

            ActiveState.OnStateMove(DeltaTime);                                                     //UPDATE THE STATE BEHAVIOUR

            PostStateMovement(this); // Check the Post State Movement on External Scripts

            TryExitActiveState();
            TryActivateState();

            MovementSystem(DeltaTime);
            GravityLogic();

            LastPos = transform.position;

            if (float.IsNaN(AdditivePosition.x)) return;

            if (!DisablePositionRotation)
            {
                if (RB)
                {
                    if (RB.isKinematic)
                    {
                        transform.position += AdditivePosition;
                    }
                    else
                    {
                        // var OldRBVelocity = RB.velocity;
                        RB.velocity = Vector3.zero;
                        RB.angularVelocity = Vector3.zero;

                        if (DeltaTime > 0) 
                        {
                            DesiredRBVelocity = AdditivePosition / DeltaTime;
                            RB.velocity = DesiredRBVelocity;
                            // RB.MoveRotation(transform.rotation * AdditiveRotation);// This does not work on newe versions of Unity
                        }
                        transform.rotation *= AdditiveRotation;
                    }
                }
                else
                {
                    transform.position += AdditivePosition;
                    transform.rotation *= AdditiveRotation;
                }

                Strafing_Rotation();
            }
            UpdateAnimatorParameters();              //Set all Animator Parameters
        }

        private void Strafing_Rotation()
        {
            if (Strafe && Aimer)
            {
                var RawDirection = Aimer.RawAimDirection;

                Vector3 HorizontalDir = Vector3.ProjectOnPlane(RawDirection, UpVector);
                Vector3 ForwardDir = Vector3.ProjectOnPlane(Forward, UpVector);

                HorizontalAimAngle_Raw = -Vector3.SignedAngle(HorizontalDir, ForwardDir, UpVector);


                StrafeDeltaValue = Mathf.Lerp(StrafeDeltaValue,
                    MovementDetected ? ActiveState.MovementStrafe : ActiveState.IdleStrafe,
                    DeltaTime * m_StrafeLerp);

                SetOptionalAnimParameter(hash_StrafeAngle, HorizontalAimAngle_Raw);

                // RB.MoveRotation(transform.rotation * Quaternion.Euler(0, HorizontalAimAngle_Raw * StrafeDeltaValue, 0));
                transform.rotation *= Quaternion.Euler(0, HorizontalAimAngle_Raw * StrafeDeltaValue, 0);
            }
            else
            {
                HorizontalAimAngle_Raw = 0;
                StrafeDeltaValue = 0;
                SetOptionalAnimParameter(hash_StrafeAngle, HorizontalAimAngle_Raw);
            }
        }

        /// <summary> This is used to add an External force to </summary>
        private void ApplyExternalForce()
        {
            var Acel = ExternalForceAcel > 0 ? (DeltaTime * ExternalForceAcel) : 1; //Use Full for changing

            CurrentExternalForce = Vector3.Lerp(CurrentExternalForce, ExternalForce, Acel);

            if (CurrentExternalForce.sqrMagnitude <= 0.01f) CurrentExternalForce = Vector3.zero; //clean Tiny forces


            if (CurrentExternalForce != Vector3.zero)
                AdditivePosition += CurrentExternalForce * DeltaTime;
        }

        private void GravityLogic()
        {
            if (UseGravity)
            {
                if (Grounded) return;  //Means has found the Ground

                var GTime = DeltaTime * GravityTime;

                GravityStoredVelocity = Gravity * GravityPower * (GTime * GTime / 2);
                AdditivePosition += GravityStoredVelocity * DeltaTime;                                         //Add Gravity if is in use
                GravityTime++;

                if (LimitGravityTime > 0 && LimitGravityTime < GravityTime) GravityTime--; //Make limit
            }
        }
         

        /// <summary> Resets Additive Rotation and Additive Position to their default</summary>
        void ResetValues()
        {
           // MainRay = FrontRay = false;
           // AdditivePosition = Anim.deltaPosition;//? Anim.deltaPosition : Vector3.zero;
            AdditivePosition = RootMotion ? Anim.deltaPosition : Vector3.zero;
            AdditiveRotation = RootMotion ? Anim.deltaRotation : Quaternion.identity;

            DeltaPos = transform.position - LastPos;                    //DeltaPosition from the last frame
            CurrentCycle = (CurrentCycle + 1) % 999999999;

            var DeltaRB = RB.velocity * DeltaTime;
            DeltaVelocity = Grounded ? Vector3.ProjectOnPlane(DeltaRB, UpVector) : DeltaRB; //When is not grounded take the Up Vector this is the one!!!
        }
    }
}