using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.IO;
using System.Reflection;

namespace CA2_LEE_CAM
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Ca2LeeCam : PartModule
    {
        #region module variable of part.cfg
        [KSPField(isPersistant = false)]
        public string camTransformString = "CamExt";

        [KSPField(isPersistant = false)]
        public string cameraID = "CA2 Lee Camera";

        [KSPField(isPersistant = false)]
        public float fieldOfView = 60f;

        [KSPField(isPersistant = false)]
        public bool changeableFieldOfView = false;

        [KSPField(isPersistant = false)]
        public float wideFieldOfView = 60f;

        [KSPField(isPersistant = false)]
        public float narrowFieldOfView = 30f;

        [KSPField(isPersistant = false)]
        public string hudTextureFile = "leehud real.png";
        #endregion module variable of part.cfg

        protected static int NextWindowID = 100;
        protected int WindowID = 0;

        private bool IsInitedModule = false;
        private Transform partTransform;

        private Rect windowRectangle;
        private Vector2 windowPosition;
        private Vector2 windowBorder;
        private float windowTitle = 15f;
        private Vector2 renderSize;

        private Texture _textureBackGroundCamera;
        internal Rect TexturePosition;
        protected RenderTexture RenderTexture;
        public Texture2D texLEEHUD;

        protected UnityEngine.Camera eeCamera;
        protected string eeCameraName;

        protected Transform cameraPositionTransform;

        private bool clcWindowActive = false;
        private ConfigNode nodeInner;

        private float Yoffset;

        public bool[] enabling = new bool[9];

        //[LOG 21:04:00.375] [TRF CLC] KSP camera[0].GalaxyCamera
        //[LOG 21:04:00.375] [TRF CLC] KSP camera[1].Camera ScaledSpace
        //[LOG 21:04:00.375] [TRF CLC] KSP camera[2].Camera 01
        //[LOG 21:04:00.376] [TRF CLC] KSP camera[3].Camera 00
        //[LOG 21:04:00.376] [TRF CLC] KSP camera[4].Canvas Camera
        //[LOG 21:04:00.376] [TRF CLC] KSP camera[5].FXCamera
        //[LOG 21:04:00.376] [TRF CLC] KSP camera[6].UIVectorCamera
        //[LOG 21:04:00.376] [TRF CLC] KSP camera[7].Camera
        //[LOG 21:04:00.376] [TRF CLC] KSP camera[8].velocity camera

        private readonly string[] knownCameraNames =
        {
            "GalaxyCamera",
            "Camera ScaledSpace",
            "Camera VE Underlay", // Environmental Visual Enhancements plugin camera
            "Camera VE Overlay",  // Environmental Visual Enhancements plugin camera
            "Camera 01",
            "Camera 00",
            "FXCamera"

            //"GalaxyCamera",
            //"Camera ScaledSpace",
            //"Camera 01",
            //"Camera 00",
            //"Canvas Camera",
            //"FXCamera",
            ////"UIVectorCamera",
            //"Camera",
            ////"velocity camera"
        };

        private readonly Camera[] cameraObject = { null, null, null, null, null, null, null, null, null };
        private readonly float cameraAspect = 1.33f;

        [KSPEvent(guiActive = true, guiName = "Turn on wideFOV", active = false)]
        public void TurnOnwideFOVEvent()
        {
            Events["TurnOnwideFOVEvent"].active = false;
            Events["TurnOnnarrowFOVEvent"].active = true;
            fieldOfView = wideFieldOfView;
            CameraChangeFOV();
        }

        [KSPEvent(guiActive = true, guiName = "Turn on narrowFOV", active = false)]
        public void TurnOnnarrowFOVEvent()
        {
            Events["TurnOnwideFOVEvent"].active = true;
            Events["TurnOnnarrowFOVEvent"].active = false;
            fieldOfView = narrowFieldOfView;
            CameraChangeFOV();
        }

        [KSPEvent(guiActive = true, guiName = "Turn on Ca2LeeCam", active = false)]
        public void TurnOnCLCEvent()
        {
            // This will hide the TurnOnCLCEvent event, and show the TurnOffCLCEvene event.
            Events["TurnOnCLCEvent"].active = false;
            Events["TurnOffCLCEvent"].active = true;
            clcWindowActive = true;
        }

        [KSPEvent(guiActive = true, guiName = "Turn off Ca2LeeCam", active = false)]
        public void TurnOffCLCEvent()
        {
            // This will hide the TurnOffCLCEven event, and show the TurnOnCLCEvent event.
            Events["TurnOnCLCEvent"].active = true;
            Events["TurnOffCLCEvent"].active = false;
            clcWindowActive = false;
            OnSave(nodeInner);
        }

        public override void OnSave(ConfigNode node)
        {
            if ((HighLogic.LoadedSceneIsFlight && IsInitedModule) /*|| HighLogic.LoadedSceneIsEditor*/)
            {
                PluginConfiguration config = PluginConfiguration.CreateForType<Ca2LeeCam>();

                windowPosition.x = windowRectangle.x;
                windowPosition.y = windowRectangle.y;
                config.SetValue("CLC Window Position", windowPosition);
                config.save();
                nodeInner = node;
                Debug.Log(string.Format("[TRF CLC{0}] OnSave() -  windowRectangle = " + windowRectangle.x.ToString() + " " + windowRectangle.y.ToString(), WindowID));
                Debug.Log(string.Format("[TRF CLC{0}] OnSave() -  windowPosition = " + windowPosition.x.ToString() + " " + windowPosition.y.ToString(), WindowID));
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<Ca2LeeCam>();

            config.load();
            windowPosition = config.GetValue<Vector2>("CLC Window Position");
            windowRectangle = new Rect(windowPosition.x, windowPosition.y, renderSize.x + 2 * windowBorder.x, renderSize.y + 2 * windowBorder.y + windowTitle);
            clcWindowActive = false;
            nodeInner = node;
            Debug.Log(string.Format("[TRF CLC{0}] OnLoad() -  windowPosition = " + windowPosition.x.ToString() + " " + windowPosition.y.ToString(), WindowID));
        }

        private void OnDisable()
        {
            Debug.Log(string.Format("[TRF CLC{0}] OnDisable() WindowID: {0}", WindowID));
            OnSave(nodeInner);
            clcWindowActive = false;
            IsInitedModule = false;
            if (RenderTexture != null)
            {
                DestroyTexture();
                RenderTexture = null;
            }
            DestroyCameras();
        }

        private void OnDestroy()
        {
            Debug.Log(string.Format("[TRF CLC{0}] OnDestroy() WindowID: {0}", WindowID));
            OnSave(nodeInner);
            clcWindowActive = false;
            IsInitedModule = false;
            if (RenderTexture != null)
            {
                DestroyTexture();
                RenderTexture = null;
            }
            DestroyCameras();
        }

        public void Start()
        {
            if (part != null)
            {
                //Debug.Log(string.Format("[TRF CLC{1}] {0} Start() - part.name = " + part.name, 0, WindowID));
                //Debug.Log(string.Format("[TRF CLC{1}] {0} Start() - part.partName = " + part.partName, 0, WindowID));
                //Debug.Log(string.Format("[TRF CLC{1}] {0} Start() - part.craftID = " + part.craftID.ToString(), 0, WindowID));

                if (HighLogic.LoadedSceneIsFlight)
                {
                    Events["TurnOnCLCEvent"].guiName = "Turn on " + cameraID;
                    Events["TurnOffCLCEvent"].guiName = "Turn off " + cameraID;

                    Events["TurnOnCLCEvent"].active = true;
                    Events["TurnOffCLCEvent"].active = false;

                    if (changeableFieldOfView)
                    {
                        Events["TurnOnwideFOVEvent"].active = false;
                        Events["TurnOnnarrowFOVEvent"].active = true;
                        fieldOfView = wideFieldOfView;
                    }

                    WindowID = NextWindowID;
                    NextWindowID++;
                    //WindowID = (int)UnityEngine.Random.Range(0, 2147483647);

                    Debug.Log(string.Format("[TRF CLC{1}] {0} end of Start() WindowID: {1} cameraID: {2}", 0, WindowID, cameraID));
                }
            }
        }

        public void Update()
        {
            if (part != null)
            {
                //if (part == null)
                //    Debug.Log(string.Format("[TRF CLC{1}] {0} START Update() part == null", 0, WindowID));
                //else if (part.name == null)
                //    Debug.Log(string.Format("[TRF CLC{1}] {0} START Update() part.name == null", 0, WindowID));
                //else
                //    Debug.Log(string.Format("[TRF CLC{1}] {0} START Update() - part.name = " + part.name, 0, WindowID));
                //Debug.Log(string.Format("[TRF CLC{1}] {0} START Update()", 0, WindowID));

                if (HighLogic.LoadedSceneIsFlight && !IsInitedModule)
                {
                    for (int i = 0; i < enabling.Length; i++)
                        enabling[i] = true;

                    //windowPosition = new Vector2(10, 30);
                    windowBorder = new Vector2(10, 10);
                    renderSize = new Vector2(401, 301);

                    eeCamera = new UnityEngine.Camera();
                    eeCameraName = "EEcamera";

                    partTransform = part.transform;

                    Debug.Log(string.Format("[TRF CLC{1}] {0} START InitTexture()", 1, WindowID));
                    InitTexture();
                    Debug.Log(string.Format("[TRF CLC{1}] {0} END InitTexture()", 2, WindowID));
                    Debug.Log(string.Format("[TRF CLC{1}] {0} START InitCamera()", 3, WindowID));
                    InitCameras();
                    Debug.Log(string.Format("[TRF CLC{1}] {0} END InitCamera()", 4, WindowID));
                    Debug.Log(string.Format("[TRF CLC{1}] {0} START set cameraPositionTransform", 7, WindowID));
                    cameraPositionTransform = FindChildTransformRecursive(camTransformString, partTransform);
                    //cameraPositionTransform.position += new Vector3(0.04f, 0f, 0f);
                    if (cameraPositionTransform == null)
                    {
                        Debug.Log(string.Format("[TRF CLC{1}] {0} '{2}' transform not found", 8, WindowID, camTransformString));
                        cameraPositionTransform = part.transform;
                    }
                    Debug.Log(string.Format("[TRF CLC{1}] {0} END set cameraPositionTransform", 9, WindowID));
                    Debug.Log(string.Format("[TRF CLC{1}] {0} START set windowRectangle", 10, WindowID));
                    windowRectangle = new Rect(windowPosition.x, windowPosition.y, renderSize.x + 2 * windowBorder.x, renderSize.y + 2 * windowBorder.y + windowTitle);
                    Debug.Log(string.Format("[TRF CLC{1}] {0} END set windowRectangle", 11, WindowID));

                    IsInitedModule = true;
                }

                //Debug.Log(string.Format("[TRF CLC{1}] {0} END Update()", 12, WindowID));

            }
        }

        private void OnGUI()
        {
            if (clcWindowActive)
            {
                // Debug.Log(string.Format("[TRF CLC] {0} clcWindowActive = true", 16));
                // Debug.Log(string.Format("[TRF CLC] {0} START GUILayout.Window()", 17));
                // Draw window
                windowRectangle = GUILayout.Window(WindowID, windowRectangle, OnWindow, cameraID);
                //Debug.Log(string.Format("[TRF CLC{1}] {0} END GUILayout.Window()", 18, WindowID));
            }
        }

        private void OnWindow(int windowID)
        {
            if (IsInitedModule)
            {
                GUILayout.BeginHorizontal();

                // Close window button
                if (GUI.Button(new Rect((renderSize.x + 2 * windowBorder.x) - 20f, 3, 17, 15), "x"))
                {
                    clcWindowActive = false;
                    // This will hide the TurnOffCLCEven event, and show the TurnOnCLCEvent event.
                    Events["TurnOnCLCEvent"].active = true;
                    Events["TurnOffCLCEvent"].active = false;
                    clcWindowActive = false;
                    OnSave(nodeInner);
                }

                //GUI.DrawTexture(TexturePosition, _textureBackGroundCamera);

                // rendering camera picture
                if (Event.current.type.Equals(EventType.Repaint))
                    Graphics.DrawTexture(TexturePosition, Render(), null);

                // draw HUD picture over picture of camera
                GUI.DrawTexture(TexturePosition, texLEEHUD);

                //Yoffset = 0;
                //Rect inputRect = new Rect(20, 40, 40, 20);
                //AddOutputValue(inputRect, "eePosition", eeCamera.transform.position, 110f, 110f);
                //AddOutputValue(inputRect, "eeRotation", eeCamera.transform.rotation.eulerAngles, 110f, 110f);

                //// blocking camera layers
                //for (int i = 0; i < knownCameraNames.Length; i++)
                //{
                //    enabling[i] = GUI.Toggle(new Rect(30, 40 + (float)i * 20, 200, 20), enabling[i], " " + knownCameraNames[i]);
                //}

                GUILayout.EndHorizontal();

                GUI.DragWindow();
            }
        }

        protected virtual RenderTexture Render()
        {
            for (int i = 0; i < cameraObject.Length; i++)
            {
                if (cameraObject[i] != null && enabling[i])
                {
                    // ScaledSpace camera and its derived cameras from Visual Enhancements mod are special - they don't move.
                    if (i >= 2)
                    {
                        cameraObject[i].transform.position = cameraPositionTransform.position;
                    }
                    cameraObject[i].targetTexture = RenderTexture;
                    cameraObject[i].transform.rotation = cameraPositionTransform.rotation;
                    //cameraObject[i].fieldOfView = FOV;
                    //cameraObject[i].rect = new Rect(0, 0, 4.01f, 3.01f);
                    cameraObject[i].Render();
                }
            }
            return RenderTexture;
        }

        protected virtual void InitTexture()
        {
            Debug.Log(string.Format("[TRF CLC{1}] {0} START LoadPNG()", 1, WindowID));
            //string pngPath = KSPUtil.ApplicationRootPath + "GameData/Ca2LeeCam/PNGs/";
            string pngPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace('\\', '/') + "/PNGs/" + hudTextureFile;
            texLEEHUD = LoadPNG(pngPath);
            Debug.Log(string.Format("[TRF CLC{1}] {0} pngPath = " + pngPath, 1, WindowID));
            Debug.Log(string.Format("[TRF CLC{1}] {0} END LoadPNG()", 1, WindowID));
            _textureBackGroundCamera = MonoColorRectTexture(new Color(0.45f, 0.45f, 0.45f, 1f));
            TexturePosition = new Rect(windowBorder.x, windowBorder.y + windowTitle, renderSize.x, renderSize.y);
            RenderTexture = new RenderTexture((int)renderSize.x, (int)renderSize.y, 24);
            RenderTexture.active = RenderTexture;
            Debug.Log(string.Format("[TRF CLC{1}] {0} RenderTexture.active", 1, WindowID));
            RenderTexture.Create();
        }

        protected virtual void DestroyTexture()
        {
            RenderTexture.Release();
        }

        protected virtual void InitCameras()
        {
            //for (int i = 0; i < UnityEngine.Camera.allCameras.Length; i++)
            //    Debug.Log(string.Format("[TRF CLC] KSP camera[{0}]." + UnityEngine.Camera.allCameras[i].name, i));

            for (int i = 0; i < knownCameraNames.Length; ++i)
            {
                CameraSetup(i, knownCameraNames[i]);
            }
        }

        protected virtual void DestroyCameras()
        {
            for (int i = 0; i < knownCameraNames.Length; ++i)
            {
                try
                {
                    UnityEngine.Object.Destroy(cameraObject[i]);
                    // Analysis disable once EmptyGeneralCatchClause
                }
                catch
                {
                    // Yes, that's really what it's supposed to be doing.
                }
                finally
                {
                    cameraObject[i] = null;
                }
            }
        }

        private void CameraSetup(int index, string sourceName)
        {
            Camera sourceCam = GetCameraByName(sourceName);

            if (sourceCam != null)
            {
                var cameraBody = new GameObject();
                cameraBody.name = typeof(Ca2LeeCam).Name + index + cameraBody.GetInstanceID();
                cameraObject[index] = cameraBody.AddComponent<Camera>();

                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".cullingMask {1}", 50, sourceCam.cullingMask));

                // Just in case to support JSITransparentPod.
                cameraObject[index].cullingMask &= ~(1 << 16 | 1 << 20);
                
                cameraObject[index].CopyFrom(sourceCam);
                cameraObject[index].enabled = false;
                cameraObject[index].aspect = cameraAspect;
                cameraObject[index].fieldOfView = fieldOfView;
                cameraObject[index].rect = new Rect(0, 0, 4.01f, 3.01f);

                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".fieldOfView " + sourceCam.fieldOfView.ToString(), 50));

                // Minor hack to bring the near clip plane for the "up close"
                // cameras drastically closer to where the cameras notionally
                // are.  Experimentally, these two cameras have N/F of 0.4 / 300.0,
                // or 750:1 Far/Near ratio.  Changing this to 8192:1 brings the
                // near plane to 37cm or so, which hopefully is close enough to
                // see nearby details without creating z-fighting artifacts.
                if (index == 5 || index == 6)
                {
                    cameraObject[index].nearClipPlane = cameraObject[index].farClipPlane / 8192.0f;
                }

                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".actualRenderingPath " + sourceCam.actualRenderingPath.ToString(), 50));
                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".RenderingPath " + sourceCam.renderingPath.ToString(), 50));
                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".useOcclusionCulling " + sourceCam.useOcclusionCulling.ToString(), 50));
                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".nearClipPlane {1}", 50, sourceCam.nearClipPlane));
                //Debug.Log(string.Format("[TRF CLC] {0} " + sourceName + ".farClipPlane {1}", 50, sourceCam.farClipPlane));

                Debug.Log(string.Format("[TRF CLC{1}] {0} END CameraSetup " + cameraBody.name + "." + sourceName, 50, WindowID));
            }
            else
            {
                Debug.Log(string.Format("[TRF CLC{1}] {0} Fail CameraSetup " + sourceName, 51, WindowID));
            }
        }

        private void CameraChangeFOV()
        {
            for (int i = 0; i < cameraObject.Length; i++)
            {
                if (cameraObject[i] != null && enabling[i])
                {
                    cameraObject[i].fieldOfView = fieldOfView;
                }
            }
        }

        public static Camera GetCameraByName(string name)
        {
            for (int i = 0; i < Camera.allCamerasCount; ++i)
            {
                if (Camera.allCameras[i].name == name)
                {
                    return Camera.allCameras[i];
                }
            }
            return null;
        }

        private Transform FindChildTransformRecursive(string objName, Transform obj)
        {
            //Debug.Log(string.Format("[TRF CLC{1}] {0} FCTR " + objName + " " + obj.name, 52, WindowID));

            Transform end = null;

            if (null == obj)
                return null;

            if (end != null)
                return end;

            foreach (Transform child in obj)
            {
                if (null == child)
                    continue;
                if (end != null)
                    return end;
                if (child.name == objName)
                {
                    end = child.transform;
                    Debug.Log(string.Format("[TRF CLC{1}] {0} child.name == objName " + end.name, 53, WindowID));
                    break;
                }
                end = FindChildTransformRecursive(objName, child);
            }
            return end;
        }

        public static Texture2D MonoColorRectTexture(Color color)
        {
            return MonoColorTexture(color, 4, 4);
        }

        public static Texture2D MonoColorTexture(Color color, int width, int height)
        {
            var texture2D = new Texture2D(width, height);
            for (var i = 0; i < width; i++)
                for (var j = 0; j < height; j++)
                    texture2D.SetPixel(i, j, color);
            texture2D.Apply();
            return texture2D;
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (System.IO.File.Exists(filePath))
            {
                fileData = System.IO.File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(fileData); // .. this will auto-resize the texture dimensions.
            }
            return tex;
        }

        private string VectorToString(Vector3 vector, string format)
        {
            string szoveg = "";

            szoveg = string.Format("( " + vector.x.ToString(format) + ", " + vector.y.ToString(format) + ", " + vector.z.ToString(format) + " )");

            return szoveg;
        }

        private void AddOutputValue(Rect rectangle, string name, Vector3 valueVector3, float labelLength, float fieldLength)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, rectangle.width + fieldLength, rectangle.height), VectorToString(valueVector3, "0.00"));
            Yoffset += 20f;
        }
    }
}
