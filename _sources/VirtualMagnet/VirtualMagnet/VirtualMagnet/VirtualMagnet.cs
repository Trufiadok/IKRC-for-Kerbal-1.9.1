using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.IO;

namespace WireSnareLock
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class WireSnareLock : PartModule
    {
        #region module variable of part.cfg
        [KSPField(isPersistant = false)]
        public float activeDistance = 0.37f;

        [KSPField(isPersistant = false)]
        public string activeDirection = "Up";

        [KSPField(isPersistant = false)]
        public bool debugTransforms = false;

        [KSPField(isPersistant = false)]
        public string nameID = "none";
        #endregion module variable of part.cfg

        public Vector3 activeDirectionVector = new Vector3();

        private Part Base;
        private uint baseFlightID;

        private bool IsActiveMagnetFixSuccess = false;
        private bool VirtualMagnetIsActive = false;
        private FixedJoint jointVirtualMagnet;
        private bool partsOnly = true;
        private float raycastRange = 0.2f;
        private GameObject rayCastObject;

        private bool endWait = false;
        private bool waitProcess = false;

        private float originalCaptureRange;
        private float originalAcquireRange;
        private float originalAcquireForce;
        private float originalAcquireTorque;
        private float originalMinDistanceToReEngage;
        private float baseOriginalMinDistanceToReEngage;

        private ModuleDockingNode dockingNode;
        private ModuleDockingNode baseDockingNode;

        private string state = "";
        private string baseState = "";

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                //if (VirtualMagnetIsActive)
                //{
                //    Events["TurnOnMagnetEvent"].active = false;
                //    Events["TurnOffMagnetEvent"].active = true;
                //}
                //else
                //{

                switch (activeDirection)
                {
                    case "Up":
                    case "up":
                    case "UP":
                        activeDirectionVector = part.transform.up;
                        activeDirection = "up";
                        break;

                    case "Down":
                    case "down":
                    case "DOWN":
                        activeDirectionVector = part.transform.up * -1f;
                        activeDirection = "down";
                        break;

                    case "Forward":
                    case "forward":
                    case "FORWARD":
                        activeDirectionVector = part.transform.forward;
                        activeDirection = "forward";
                        break;

                    case "Backward":
                    case "backward":
                    case "BACKWARD":
                        activeDirectionVector = part.transform.forward * -1f;
                        activeDirection = "backward";
                        break;

                    case "Right":
                    case "right":
                    case "RIGHT":
                        activeDirectionVector = part.transform.right;
                        activeDirection = "right";
                        break;

                    case "Left":
                    case "left":
                    case "LEFT":
                        activeDirectionVector = part.transform.right * -1f;
                        activeDirection = "left";
                        break;

                    default:
                        activeDirectionVector = part.transform.up;
                        activeDirection = "up";
                        break;
                }

                ModuleDockingNode dockingNode = part.dockingPorts[0].GetComponent<ModuleDockingNode>();
                if (dockingNode != null)
                {
                    originalMinDistanceToReEngage = dockingNode.minDistanceToReEngage;

                    Events["TurnOnMagnetEvent"].active = true;
                    Events["TurnOffMagnetEvent"].active = false;

                    Debug.Log("[WireSnareLock] Start() - (dockingNode != null)");
                    Debug.Log(String.Format(" [WireSnareLock {1}] Start() - activeDistance = {0}", activeDistance, nameID));
                    Debug.Log(String.Format(" [WireSnareLock {1}] Start() - activeDirection = {0}", activeDirection, nameID));
                    Debug.Log(String.Format(" [WireSnareLock {1}] Start() - debugTransforms = {0}", debugTransforms.ToString(), nameID));
                }
                else
                {
                    Debug.Log("[WireSnareLock] Start() - (dockingNode == null)");
                }
                //}
            }
        }

        public void Update()
        {
            ////bool IKRCModuleOk = IkRobotController.IkRobotController.IsInitedModule;
            ////if (HighLogic.LoadedSceneIsFlight && !IsActiveMagnetFixSuccess && VirtualMagnetIsActive /*&& GetIKRCIsInitedModule()*/)
            //if (HighLogic.LoadedSceneIsFlight && !IsActiveMagnetFixSuccess && VirtualMagnetIsActive /*&& IKRCModuleOk*/ && !waitProcess)
            //{
            //    StartCoroutine(Wait());
            //    if (endWait)
            //    {
            //        Debug.Log("[WireSnareLock] Update() - Not IsActiveMagnetFixSuccess");
            //        IsActiveMagnetFixSuccess = VirtualMagnetActivateOnLoad();
            //        if (IsActiveMagnetFixSuccess)
            //            Debug.Log("[WireSnareLock] Update() - IsActiveMagnetFixSuccess");
            //        endWait = false;
            //    }
            //}

            //if (HighLogic.LoadedSceneIsFlight)
            //{
            //    if(dockingNode != null)
            //    {
            //        if (state != dockingNode.state)
            //        {
            //            state = dockingNode.state;
            //            Debug.Log("[WireSnareLock] Update() - state = " + state);
            //        }
            //    }

            //    if (baseDockingNode != null)
            //    {
            //        if (baseState != baseDockingNode.state)
            //        {
            //            baseState = dockingNode.state;
            //            Debug.Log("[WireSnareLock] Update() - baseState = " + baseState);
            //        }
            //    }
            //}
        }

        IEnumerator Wait()
        {
            waitProcess = true;
            Debug.Log("[WireSnareLock] StartWait() time = " + Time.time.ToString());
            yield return new WaitForSeconds(0.05f);
            Debug.Log("[WireSnareLock] EndWait() time = " + Time.time.ToString());
            waitProcess = false;
            endWait = true;
        }

        public bool VirtualMagnetActivateOnLoad()
        {
            bool IsSuccess = false;

            if (HighLogic.LoadedSceneIsFlight)
            {
                RaycastHit hitInfo;
                Ray ray = new Ray(part.transform.position + part.transform.up * activeDistance, part.transform.up);
                int mask = 0;
                if (partsOnly)
                    mask = 19;

                if (Physics.Raycast(ray, out hitInfo, raycastRange, mask))
                {
                    Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - Raycast()");
                    rayCastObject = hitInfo.collider.gameObject;
                    if (rayCastObject != null)
                    {
                        Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - rayCastObject = " + rayCastObject.transform.name);
                        if (rayCastObject.transform.name != "lee-collider")
                        {
                            Base = rayCastObject.GetComponentInParent<Part>();
                            Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - Base.name = " + Base.name);
                            if (Base != null)
                            {
                                baseFlightID = Base.flightID;
                                Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - Base.flightID = " + Base.flightID.ToString());
                                jointVirtualMagnet = new FixedJoint();
                                Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - jointVirtualMagnet = new FixedJoint()");
                                jointVirtualMagnet = part.gameObject.AddComponent<FixedJoint>();
                                Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - jointVirtualMagnet = part.gameObject.AddComponent<FixedJoint>()");
                                if (jointVirtualMagnet != null)
                                {
                                    if (Base.rb != null)
                                    {
                                        jointVirtualMagnet.connectedBody = Base.rb;
                                        Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - jointVirtualMagnet.connectedBody = " + Base.rb.name);

                                        Events["TurnOnMagnetEvent"].active = false;
                                        Events["TurnOffMagnetEvent"].active = true;

                                        VirtualMagnetIsActive = true;
                                        Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - VirtualMagnetIsActive = true");

                                        IsSuccess = true;
                                    }
                                    else
                                        Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - Base.rb == null");
                                }
                            }
                            else
                            {
                                Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - Base == null");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("[WireSnareLock] VirtualMagnetActivateOnLoad() - Raycast() == false");
                }
            }
            return IsSuccess;
        }

        public bool DisableDockingNode()
        {
            bool success = false;

            dockingNode = part.dockingPorts[0].GetComponent<ModuleDockingNode>();
            if (dockingNode != null)
            {
                dockingNode.genderFemale = true;

                dockingNode.minDistanceToReEngage = originalMinDistanceToReEngage;

                //originalCaptureRange = dockingNode.captureRange;
                //originalAcquireRange = dockingNode.acquireRange;
                //originalAcquireForce = dockingNode.acquireForce;
                //originalAcquireTorque = dockingNode.acquireTorque;
                //Debug.Log("[WireSnareLock] DisableDockingNode() -  originalCaptureRange = " + originalCaptureRange.ToString());
                //dockingNode.captureRange = -0.5f;
                //dockingNode.acquireRange = -0.5f;
                //dockingNode.acquireForce = 0f;
                //dockingNode.acquireTorque = 0f;

                success = true;
                //Debug.Log("[WireSnareLock] DisableDockingNode() -  dockingNode.captureRange = " + dockingNode.captureRange.ToString());
            }
            else
                Debug.Log("[WireSnareLock] DisableDockingNode() - (dockingNode == null)");

            return success;
        }

        public bool EnableDockingNode()
        {
            bool success = false;

            dockingNode = part.dockingPorts[0].GetComponent<ModuleDockingNode>();
            if (dockingNode != null)
            {
                dockingNode.genderFemale = false;

                Debug.Log("[WireSnareLock] EnableDockingNode() -  dockingNode.minDistanceToReEngage = " + dockingNode.minDistanceToReEngage.ToString());
                originalMinDistanceToReEngage = dockingNode.minDistanceToReEngage;
                dockingNode.minDistanceToReEngage = 0.01f;
                Debug.Log("[WireSnareLock] EnableDockingNode() -  dockingNode.minDistanceToReEngage = " + dockingNode.minDistanceToReEngage.ToString());

                //dockingNode.captureRange = originalCaptureRange;
                //dockingNode.acquireRange = originalAcquireRange;
                //dockingNode.acquireForce = originalAcquireForce;
                //dockingNode.acquireTorque = originalAcquireTorque;

                RaycastHit hitInfo;
                Ray ray = new Ray(part.transform.position + activeDirectionVector * activeDistance, activeDirectionVector);
                int mask = 0;
                if (partsOnly)
                    mask = 19;

                if (Physics.Raycast(ray, out hitInfo, raycastRange, mask))
                {
                    Debug.Log("[WireSnareLock] EnableDockingNode() - Raycast()");
                    rayCastObject = hitInfo.collider.gameObject;
                    if (rayCastObject != null)
                    {
                        Debug.Log("[WireSnareLock]EnableDockingNode() - rayCastObject = " + rayCastObject.transform.name);
                        if (rayCastObject.transform.name != "lee-collider")
                        {
                            Base = rayCastObject.GetComponentInParent<Part>();
                            Debug.Log("[WireSnareLock] EnableDockingNode() - Base.name = " + Base.name);
                            if (Base != null)
                            {
                                baseDockingNode = Base.dockingPorts[0].GetComponent<ModuleDockingNode>();
                                if (baseDockingNode != null)
                                {
                                    Debug.Log("[WireSnareLock] EnableDockingNode() -  baseDockingNode.minDistanceToReEngage = " + baseDockingNode.minDistanceToReEngage.ToString());
                                    baseOriginalMinDistanceToReEngage = baseDockingNode.minDistanceToReEngage;
                                    baseDockingNode.minDistanceToReEngage = 0.01f;
                                    Debug.Log("[WireSnareLock] EnableDockingNode() -  baseDockingNode.minDistanceToReEngage = " + baseDockingNode.minDistanceToReEngage.ToString());
                                }
                                else
                                {
                                    Debug.Log("[WireSnareLock] EnableDockingNode() - (baseDockingNode == null)");
                                }
                            }
                            else
                            {
                                Debug.Log("[WireSnareLock] EnableDockingNode() - Base == null");
                            }
                        }
                        else
                        {
                            Debug.Log("[WireSnareLock] EnableDockingNode() - (rayCastObject.transform.name == lee - collider)");
                        }
                    }
                    else
                    {
                        Debug.Log("[WireSnareLock] EnableDockingNode() - (rayCastObject == null)");
                    }
                }
                else
                {
                    Debug.Log("[WireSnareLock] EnableDockingNode() - Raycast() == false");
                }

                success = true;
                //Debug.Log("[WireSnareLock] EnableDockingNode() -  dockingNode.captureRange = " + dockingNode.captureRange.ToString());
            }
            else
                Debug.Log("[WireSnareLock] EnableDockingNode() - (dockingNode == null)");

            return success;
        }

        #region KSPEvents
        [KSPEvent(guiActive = true, guiName = "Turn on DockingPort", active = false)]
        public void TurnOnDockingPortEvent()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (EnableDockingNode())
                {
                    Events["TurnOnDockingPortEvent"].active = false;
                    Events["TurnOffDockingPortEvent"].active = true;
                }
            }
        }

        [KSPEvent(guiActive = true, guiName = "Turn off DockingPort", active = true)]
        public void TurnOffDockingPortEvent()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (DisableDockingNode())
                {
                    Events["TurnOnDockingPortEvent"].active = true;
                    Events["TurnOffDockingPortEvent"].active = false;
                }
            }
        }

        [KSPEvent(guiActive = true, guiName = "Activate WSLock", active = false)]
        public void TurnOnMagnetEvent()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                RaycastHit hitInfo;
                ActiveDirection();
                Ray ray = new Ray(part.transform.position + activeDirectionVector * activeDistance, activeDirectionVector);
                int mask = 0;
                if (partsOnly)
                    mask = 19;

                if (Physics.Raycast(ray, out hitInfo, raycastRange, mask))
                {
                    Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Raycast()");
                    rayCastObject = hitInfo.collider.gameObject;
                    if (rayCastObject != null)
                    {
                        Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - rayCastObject = " + rayCastObject.transform.name);
                        if (rayCastObject.transform.name != "lee-collider")
                        {
                            Base = rayCastObject.GetComponentInParent<Part>();
                            Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base.name = " + Base.name);
                            if (Base != null)
                            {
                                baseFlightID = Base.flightID;
                                Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base.flightID = " + Base.flightID.ToString());
                                jointVirtualMagnet = new FixedJoint();
                                Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - jointVirtualMagnet = new FixedJoint()");
                                jointVirtualMagnet = part.gameObject.AddComponent<FixedJoint>();
                                Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - jointVirtualMagnet = part.gameObject.AddComponent<FixedJoint>()");
                                if (jointVirtualMagnet != null)
                                {
                                    if (Base.rb != null)
                                    {
                                        jointVirtualMagnet.connectedBody = Base.rb;
                                        Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - jointVirtualMagnet.connectedBody = " + Base.rb.name);

                                        Events["TurnOnMagnetEvent"].active = false;
                                        Events["TurnOffMagnetEvent"].active = true;

                                        VirtualMagnetIsActive = true;
                                        Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - VirtualMagnetIsActive = true");
                                    }
                                    else
                                        Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base.rb == null");
                                }
                            }
                            else
                            {
                                Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base == null");
                            }
                        }
                    }
                }
                else
                    Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Raycast() == false");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Deactivate WSLock", active = false)]
        public void TurnOffMagnetEvent()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (jointVirtualMagnet != null)
                {
                    Destroy(jointVirtualMagnet);
                    // ???
                    if (jointVirtualMagnet == null)
                        Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - Destroy fixedjoint (jointVirtualMagnet == null)");
                    else
                        Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - Can't destroy fixedjoint (jointVirtualMagnet != null)");
                }
                else
                    Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - jointVirtualMagnet == null");
                Events["TurnOnMagnetEvent"].active = true;
                Events["TurnOffMagnetEvent"].active = false;

                VirtualMagnetIsActive = false;
                Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - VirtualMagnetIsActive = false");
            }
        }
        #endregion KSPEvents

        #region KSPActions
        [KSPAction(guiName = "WireSnareLock")]
        public void actionWireSnareLock(KSPActionParam param)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (Events["TurnOnMagnetEvent"].active)
                {
                    RaycastHit hitInfo;
                    ActiveDirection();
                    Ray ray = new Ray(part.transform.position + activeDirectionVector * activeDistance, activeDirectionVector);
                    int mask = 0;
                    if (partsOnly)
                        mask = 19;

                    if (Physics.Raycast(ray, out hitInfo, raycastRange, mask))
                    {
                        Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Raycast()");
                        rayCastObject = hitInfo.collider.gameObject;
                        if (rayCastObject != null)
                        {
                            Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - rayCastObject = " + rayCastObject.transform.name);
                            if (rayCastObject.transform.name != "lee-collider")
                            {
                                Base = rayCastObject.GetComponentInParent<Part>();
                                Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base.name = " + Base.name);
                                if (Base != null)
                                {
                                    baseFlightID = Base.flightID;
                                    Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base.flightID = " + Base.flightID.ToString());
                                    jointVirtualMagnet = new FixedJoint();
                                    Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - jointVirtualMagnet = new FixedJoint()");
                                    jointVirtualMagnet = part.gameObject.AddComponent<FixedJoint>();
                                    Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - jointVirtualMagnet = part.gameObject.AddComponent<FixedJoint>()");
                                    if (jointVirtualMagnet != null)
                                    {
                                        if (Base.rb != null)
                                        {
                                            jointVirtualMagnet.connectedBody = Base.rb;
                                            Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - jointVirtualMagnet.connectedBody = " + Base.rb.name);

                                            Events["TurnOnMagnetEvent"].active = false;
                                            Events["TurnOffMagnetEvent"].active = true;

                                            VirtualMagnetIsActive = true;
                                            Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - VirtualMagnetIsActive = true");
                                        }
                                        else
                                            Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base.rb == null");
                                    }
                                }
                                else
                                {
                                    Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Base == null");
                                }
                            }
                        }
                    }
                    else
                        Debug.Log("[WireSnareLock] TurnOnMagnetEvent() - Raycast() == false");

                }
                else if (Events["TurnOffMagnetEvent"].active)
                {
                    if (jointVirtualMagnet != null)
                    {
                        Destroy(jointVirtualMagnet);
                        // ???
                        if (jointVirtualMagnet == null)
                            Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - Destroy fixedjoint (jointVirtualMagnet == null)");
                        else
                            Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - Can't destroy fixedjoint (jointVirtualMagnet != null)");
                    }
                    else
                        Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - jointVirtualMagnet == null");
                    Events["TurnOnMagnetEvent"].active = true;
                    Events["TurnOffMagnetEvent"].active = false;

                    VirtualMagnetIsActive = false;
                    Debug.Log("[WireSnareLock] TurnOffMagnetEvent() - VirtualMagnetIsActive = false");
                }
            }
        }

        [KSPAction(guiName = "DockingPort")]
        public void actionDockingPort(KSPActionParam param)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (Events["TurnOffDockingPortEvent"].active && DisableDockingNode())
                {
                    Events["TurnOnDockingPortEvent"].active = true;
                    Events["TurnOffDockingPortEvent"].active = false;
                }
                else if (Events["TurnOnDockingPortEvent"].active && EnableDockingNode())
                {
                    Events["TurnOnDockingPortEvent"].active = false;
                    Events["TurnOffDockingPortEvent"].active = true;
                }
            }
        }
        #endregion KSPActions

        #region OnMethods
        // ??? Exception ???
        public void OnRenderObject()
        {
            if (debugTransforms)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (part.transform != null && activeDirectionVector != null)
                    {
                        //Debug.Log("[WireSnareLock] OnRenderObject() - debugTransforms == True");
                        ActiveDirection();
                        DrawTools.DrawLineElement(part.transform.position + activeDirectionVector * activeDistance, part.transform.position + activeDirectionVector * activeDistance + activeDirectionVector * raycastRange, Color.yellow);
                    }
                    //    else
                    //        Debug.Log("[WireSnareLock] OnRenderObject() - !debugtransforms");

                }
                //else
                //    Debug.Log("[WireSnareLock] OnRenderObject() - part.transform == null");
            }
        }

        //public override void OnSave(ConfigNode node)
        //{
        //    //Debug.Log("[WireSnareLock] OnSave()");
        //    //if (HighLogic.LoadedSceneIsFlight)
        //    //{
        //    //    PluginConfiguration config = PluginConfiguration.CreateForType<WireSnareLock>();

        //    //    config.SetValue("VirtualMagnetIsActive", VirtualMagnetIsActive);
        //    //    Debug.Log(string.Format("[WireSnareLock] OnSave() - save VirtualMagnetIsActive = " + (VirtualMagnetIsActive ? "true" : "false")));
        //    //    config.SetValue("baseFlightID", (int)baseFlightID);
        //    //    Debug.Log(string.Format("[WireSnareLock] OnSave() - save baseFlightID = {0}", baseFlightID));
        //    //    config.save();
        //    //}
        //}

        //public override void OnLoad(ConfigNode node)
        //{
        //    //PluginConfiguration config = PluginConfiguration.CreateForType<WireSnareLock>();

        //    //config.load();

        //    //VirtualMagnetIsActive = config.GetValue<bool>("VirtualMagnetIsActive");
        //    //Debug.Log(string.Format("[WireSnareLock] OnLoad() - save VirtualMagnetIsActive = " + (VirtualMagnetIsActive ? "true" : "false")));
        //    //baseFlightID = (uint)config.GetValue<int>("baseFlightID");
        //    //Debug.Log(string.Format("[WireSnareLock] OnLoad() - save baseFlightID = {0}", baseFlightID));

        //    ////if (VirtualMagnetIsActive)
        //    ////{
        //    ////    TurnOnMagnetEvent();
        //    ////}
        //    ////else
        //    ////{
        //    ////    TurnOffMagnetEvent();
        //    ////}
        //}
        #endregion OnMethods

        private bool GetIKRCIsInitedModule()
        {
            bool IsInitedModule = false;

            var pmIKRC = AssemblyLoader.loadedAssemblies.SingleOrDefault(a => a.assembly.FullName.Contains("IkRobotController"));
            if (pmIKRC != null)
            {
                Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - (pmIKRC != null)");
                Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - pmIKRC.name = " + pmIKRC.name);
                FieldInfo field = pmIKRC.GetType().GetField("IsInitedModule", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                //Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - pmIKRC.assembly.GetType().Name = " + pmIKRC.assembly.GetType().Name);
                if (field != null)
                {
                    Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - (field != null)");
                    IsInitedModule = (bool)(pmIKRC.GetType().GetField("IsInitedModule", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(pmIKRC));
                    Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - IsInitedModule = " + (IsInitedModule ? "true" : "false"));
                }
                else
                {
                    Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - (field == null)");
                }
            }
            else
            {
                Debug.Log("[WireSnareLock] GetIKRCIsInitedModule() - (pmIKRC == null)");
            }

            return IsInitedModule;
        }

        private void ActiveDirection()
        {
            //Debug.Log(String.Format("[WireSnareLock] ActiveDirection() - activeDirection = {0}", activeDirection));
            if (part.transform != null)
            {
                switch (activeDirection)
                {
                    case "up":
                        activeDirectionVector = part.transform.up;
                        break;

                    case "down":
                        activeDirectionVector = part.transform.up * -1f;
                        break;

                    case "forward":
                        activeDirectionVector = part.transform.forward;
                        break;

                    case "backward":
                        activeDirectionVector = part.transform.forward * -1f;
                        break;

                    case "right":
                        activeDirectionVector = part.transform.right;
                        break;

                    case "left":
                        activeDirectionVector = part.transform.right * -1f;
                        break;

                    default:
                        activeDirectionVector = part.transform.up;
                        break;
                }
            }
            //Debug.Log(String.Format("[WireSnareLock] ActiveDirection() - END"));
        }

        private string VectorToString(Vector3 vector, string format)
        {
            string szoveg = "";

            szoveg = string.Format("( " + vector.x.ToString(format) + ", " + vector.y.ToString(format) + ", " + vector.z.ToString(format) + " )");

            return szoveg;
        }

        private string QuaternionToString(Quaternion quaternion, string format)
        {
            string szoveg = "";

            szoveg = string.Format("( " + quaternion.w.ToString(format) + ", " + quaternion.x.ToString(format) + ", " + quaternion.y.ToString(format) + ", " + quaternion.z.ToString(format) + " )");

            return szoveg;
        }
    }
}
