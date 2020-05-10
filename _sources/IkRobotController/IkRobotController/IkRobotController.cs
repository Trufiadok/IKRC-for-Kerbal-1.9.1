using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.IO;
using System.Globalization;

// FELADATOK:

namespace IkRobotController
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class IkRobotController : PartModule
    {
        #region module variable of part.cfg
        [KSPField(isPersistant = false)]
        public string robotArmID = "";

        [KSPField(isPersistant = false)]
        public string servoList = "";

        [KSPField(isPersistant = false)]
        public string independentServoList = "";

        [KSPField(isPersistant = false)]
        public string independentServoPersistentId = "";

        [KSPField(isPersistant = false)]
        public string servoRealAxis = "";

        [KSPField(isPersistant = false)]
        public string minMaxAngles = "";

        [KSPField(isPersistant = false)]
        public string independentMinMaxAngles = "";

        [KSPField(isPersistant = false)]
        public string controlButtonUpDownColor = "";

        [KSPField(isPersistant = false)]
        public string controlButtonLeftRightColor = "";

        [KSPField(isPersistant = false)]
        public string controlButtonForwardBackwardColor = "";

        [KSPField(isPersistant = false)]
        public bool debugTransforms = false;

        [KSPField(isPersistant = false)]
        public float workingRange = 8.0f;

        [KSPField(isPersistant = false)]
        public float MaxPosErr = 10f;

        [KSPField(isPersistant = false)]
        public float MaxOriErr = 180f;

        [KSPField(isPersistant = false)]
        public float DistanceThreshold = 0.01f;

        [KSPField(isPersistant = false)]
        public float AngleThreshold = 0.5f;

        [KSPField(isPersistant = false)]
        public float SamplingAngle = 0.02f;

        [KSPField(isPersistant = false)]
        public bool dynamicSamplingAngle = true;

        [KSPField(isPersistant = false)]
        public bool SLTactive = false;

        [KSPField(isPersistant = false)]
        public bool NormalizedServoRotation = true;
        #endregion module variable of part.cfg

        //private static int counterStart = 0;
        //private static int counterOnLoad = 0;
        //private static int counterOnSave = 0;
        private int counterStart = 0;
        private int counterOnLoad = 0;
        private int counterOnSave = 0;

        //private static bool JointListIsLoaded = false;
        private bool JointListIsLoaded = false;

        private bool buttonIsColored = false;
        private Color colorControlButtonUpDown;
        private Color colorControlButtonLeftRight;
        private Color colorControlButtonForwardBackward;

        //private static int onSaveCounter = 0;
        //private static int onLoadCounter = 0;
        private int onSaveCounter = 0;
        private int onLoadCounter = 0;

        protected static int NextWindowID = 110;
        protected int WindowID = 0;
        protected int MsgWindowID = 0;
        protected int ChkWindowID = 0;
        protected int CfgWindowID = 0;

        public bool IsInitedModule = false;
        public bool IsInitedModule1st = true;
        private bool IsLoadedFkParams = false;
        private bool IsLoadedRdFkParams = false;
        private bool ServosIsBaseInited = false;
        private bool DumpEventIsActive = false;
        private bool IsSaveServoStructure = false;
        private bool SLT1stCycle = false;

        private bool HasIndependentServos = false;

        private string msgWindowTitle = "";
        private string chkWindowTitle = "";
        private string msgText = "";
        private string chkText = "";
        private string[] chkButtonText = new string[3];
        bool chkWindowResult = false;

        private GUIStyle redStyle = null;
        private GUIStyle greenStyle = null;
        private GUIStyle blueStyle = null;

        private Rect windowRectangle;
        private Rect msgWindowRectangle;
        private Rect chkWindowRectangle;
        private Rect cfgWindowRectangle;
        private Vector2 windowPosition = new Vector2(10, 10);
        private bool windowExtended = false;
        private Vector2 windowSize = new Vector2(310, 600);
        private Vector2 msgWindowPosition = new Vector2(100, 100);
        private Vector2 msgWindowSize = new Vector2(250, 100);
        private Vector2 chkWindowPosition = new Vector2(100, 100);
        private Vector2 chkWindowSize = new Vector2(250, 100);
        private Vector2 cfgWindowPosition = new Vector2(200, 200);
        private Vector2 cfgWindowSize = new Vector2(250, 200);
        Vector2 RIDScrollPosition;
        Vector2 ThetascrollPosition;
        private Rect inputRect;
        private ConfigNode nodeInner;
        private bool IK_active = false;
        private bool IKButton_active = false;
        private bool servoTransformHas = false;
        private bool targetIconHas = false;
        private bool zeroThetaIterate = false;
        private bool clearThetasBool = false;
        private bool baseStateBool = false;
        private bool actualStateBool = false;
        private bool pdgfStateBool = false;
        private bool veeStateBool = false;
        private bool refreshPDGFBool = false;
        private bool ircWindowActive = false;
        private bool msgWindowActive = false;
        private bool chkWindowActive = false;
        private bool cfgWindowActive = false;
        private bool emergencyServoStop = false;
        private bool manualMoveReference = false;
        private bool manageConfig = false;
        private float Yoffset = 0;
        private float servoSpeed = 0.25f;
        private float transStep = 0.25f;
        private float rotStep = 0.25f;

        private bool AdjustEnable = false;

        private int SLTposition = 0;
        private Vector3 saveGlobalPosition = new Vector3();
        private float SLTdistance;
        private float SLTelementDistance;
        VectorSM SLTactualState;
        private float SLTdivider;
        private bool SLTBreaking = true;
        private bool SLTBreakingActive = true;
        private bool moveTarget = false;

        private float ArmMaxLength = 8.0f;

        private string iterateString = "500";
        private string forceDistStringZ = "0.6";
        private string forceRollStringZ = "0.0";
        private string forceDistStringX = "0.0";
        private string forceRollStringX = "0.0";
        private string forceDistStringY = "0.0";
        private string forceRollStringY = "0.0";
        private string samplingAngleString = "0.02";
        private string DistanceThresholdString = "0.01";
        private string AngleThresholdString = "0.5";
        private string MaxPosErrString = "10";
        private string MaxOriErrString = "180";
        private string[] thetaString = { "0", "0", "0", "0", "0", "0", "0" };
        private string ArmMaxLengthString = "8.0";

        private string[] iotaString = { "0", "0", "0", "0", "0", "0", "0" };

        private Vector2 trButtonsOrigin = new Vector2(10, 35);
        private Vector2 closeWindowButtonPosition = new Vector2(290, 3);
        private Vector2 closeCfgWindowButtonPosition = new Vector2(290, 3);
        private Rect diagramRectangle;
        private List<float> iterateData;
        private List<float> sampleAngleData;
        private List<float> distanceData;
        private List<List<float>> data;

        private Rect gradientDiagramRectangle;
        private List<List<float>> gradientData;
        private List<float> errorData;

        private bool WindowIsExtended = false;

        public class ConfigXML
        {
            public class BOOL
            {
                public string name;
                public int value;

                public BOOL(string name, bool value)
                {
                    this.name = name;
                    this.value = value ? 1 : 0;
                }
            }

            public class VECTOR2
            {
                public string name;
                public Vector2 value;

                public VECTOR2(string name, Vector2 value)
                {
                    this.name = name;
                    this.value = new Vector2(value.x, value.y);
                }
            }

            public class VECTOR3
            {
                public string name;
                public Vector3 value;

                public VECTOR3(string name, Vector3 value)
                {
                    this.name = name;
                    this.value = new Vector3(value.x, value.y, value.z);
                }
            }

            public class QUATERNION
            {
                public string name;
                public Quaternion value;

                public QUATERNION(string name, Quaternion value)
                {
                    this.name = name;
                    this.value = new Quaternion(value.w, value.x, value.y, value.z);
                }
            }

            public bool ContentIsLoaded = false;
            public bool OperationIsSuccess = false;

            public string boolString;
            public string vector2String;
            public string vector3String;
            public string quaternionString;

            public string variableSequence;
            public List<string> variableList;
            public List<string> robotIDList;

            public List<BOOL> boolList;
            public List<VECTOR2> vector2List;
            public List<VECTOR3> vector3List;
            public List<QUATERNION> quaternionList;

            public ConfigXML()
            {
                ContentIsLoaded = false;
                boolString = "";
                vector2String = "";
                vector3String = "";
                quaternionString = "";

                variableSequence = "";
                variableList = new List<string>();
                robotIDList = new List<string>();

                boolList = new List<BOOL>();
                vector2List = new List<VECTOR2>();
                vector3List = new List<VECTOR3>();
                quaternionList = new List<QUATERNION>();

                Debug.Log(string.Format("[IKRC] ConfigXML() constructed"));
            }

            public bool CheckExistVariables(string preFix, string postFix)
            {
                bool subResult = false;

                subResult = false;
                string variableName = preFix + "-" + postFix;
                foreach (string element in variableList)
                {
                    if (variableName == element)
                    {
                        subResult = true;
                        break;
                    }
                }
                if (!subResult)
                {
                    Debug.Log(string.Format("[IKRC] - CheckExistVariables({0}) don't exist", variableName));
                    return false;
                }

                Debug.Log(string.Format("[IKRC] - CheckExistVariables({0}) exist", variableName));
                return true;
            }

            public bool CheckExistVariables(string preFix, string[] postFixs, int servoCount)
            {
                bool subResult = false;

                for (int i = 0; i < servoCount; i++)
                    for (int j = 0; j < postFixs.Length; j++)
                    {
                        string variableName = preFix + "-" + i.ToString() + "-" + postFixs[j];
                        subResult = false;
                        foreach (string element in variableList)
                        {
                            if (variableName == element)
                            {
                                subResult = true;
                                break;
                            }
                        }
                        if (!subResult)
                        {
                            Debug.Log(string.Format("[IKRC] - CheckExistVariables({0}) don't exist", variableName));
                            return false;
                        }
                    }

                Debug.Log(string.Format("[IKRC] - CheckExistVariables({0}-...) exist", preFix));
                return true;
            }

            public bool CreateVariables(string preFix, string postFix, string postFixType)
            {
                bool result = false;
                bool subResult = false;

                subResult = false;
                string variableName = preFix + "-" + postFix;

                switch (postFixType)
                {
                    case "bool":

                        foreach (BOOL element in boolList)
                        {
                            if (element.name == variableName)
                            {
                                subResult = true;
                                break;
                            }
                        }

                        if (!subResult)
                        {
                            boolString = boolString + "," + variableName;
                            variableSequence = variableSequence + "," + variableName;
                            variableList.Add(variableName);
                            boolList.Add(new BOOL(variableName, false));
                            Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                        }

                        break;

                    case "Vector2":

                        foreach (VECTOR2 element in vector2List)
                        {
                            if (element.name == variableName)
                            {
                                subResult = true;
                                break;
                            }
                        }

                        if (!subResult)
                        {
                            vector2String = vector2String + "," + variableName;
                            variableSequence = variableSequence + "," + variableName;
                            variableList.Add(variableName);
                            vector2List.Add(new VECTOR2(variableName, new Vector2(0f, 0f)));
                            Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                        }

                        break;

                    case "Vector3":

                        foreach (VECTOR3 element in vector3List)
                        {
                            if (element.name == variableName)
                            {
                                subResult = true;
                                break;
                            }
                        }

                        if (!subResult)
                        {
                            vector3String = vector3String + "," + variableName;
                            variableSequence = variableSequence + "," + variableName;
                            variableList.Add(variableName);
                            vector3List.Add(new VECTOR3(variableName, new Vector3(0f, 0f, 0f)));
                            Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                        }

                        break;

                    case "Quaternion":

                        foreach (QUATERNION element in quaternionList)
                        {
                            if (element.name == variableName)
                            {
                                subResult = true;
                                break;
                            }
                        }

                        if (!subResult)
                        {
                            quaternionString = quaternionString + "," + variableName;
                            variableSequence = variableSequence + "," + variableName;
                            variableList.Add(variableName);
                            quaternionList.Add(new QUATERNION(variableName, Quaternion.identity));
                            Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                        }

                        break;
                }

                result = true;

                return result;
            }

            public bool CreateVariables(string preFix, string[] postFixs, string[] postFixTypes, int servoCount)
            {
                bool result = false;
                bool subResult = false;

                for (int i = 0; i < servoCount; i++)
                    for (int j = 0; j < postFixs.Length; j++)
                    {
                        subResult = false;
                        string variableName = preFix + "-" + i.ToString() + "-" + postFixs[j];

                        switch (postFixTypes[j])
                        {
                            case "bool":

                                foreach (BOOL element in boolList)
                                {
                                    if (element.name == variableName)
                                    {
                                        subResult = true;
                                        break;
                                    }
                                }

                                if (!subResult)
                                {
                                    boolString = boolString + "," + variableName;
                                    variableSequence = variableSequence + "," + variableName;
                                    variableList.Add(variableName);
                                    boolList.Add(new BOOL(variableName, false));
                                    Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                                }

                                break;

                            case "Vector2":

                                foreach (VECTOR2 element in vector2List)
                                {
                                    if (element.name == variableName)
                                    {
                                        subResult = true;
                                        break;
                                    }
                                }

                                if (!subResult)
                                {
                                    vector2String = vector2String + "," + variableName;
                                    variableSequence = variableSequence + "," + variableName;
                                    variableList.Add(variableName);
                                    vector2List.Add(new VECTOR2(variableName, new Vector2(0f, 0f)));
                                    Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                                }

                                break;

                            case "Vector3":

                                foreach (VECTOR3 element in vector3List)
                                {
                                    if (element.name == variableName)
                                    {
                                        subResult = true;
                                        break;
                                    }
                                }

                                if (!subResult)
                                {
                                    vector3String = vector3String + "," + variableName;
                                    variableSequence = variableSequence + "," + variableName;
                                    variableList.Add(variableName);
                                    vector3List.Add(new VECTOR3(variableName, new Vector3(0f, 0f, 0f)));
                                    Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                                }

                                break;

                            case "Quaternion":

                                foreach (QUATERNION element in quaternionList)
                                {
                                    if (element.name == variableName)
                                    {
                                        subResult = true;
                                        break;
                                    }
                                }

                                if (!subResult)
                                {
                                    quaternionString = quaternionString + "," + variableName;
                                    variableSequence = variableSequence + "," + variableName;
                                    variableList.Add(variableName);
                                    quaternionList.Add(new QUATERNION(variableName, Quaternion.identity));
                                    Debug.Log(string.Format("[IKRC] - CreateVariables({0}) OK", variableName));
                                }

                                break;
                        }

                    }

                result = true;

                return result;
            }

            public bool DeleteVariables(string preFix)
            {
                bool result = false;

                // clear from variableSequence
                string[] tempString = variableSequence.Split(',');
                List<string> tempList = tempString.OfType<string>().ToList();
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    string element = tempList[i];
                    if (element.StartsWith(preFix))
                    {
                        tempList.Remove(element);
                    }
                }
                variableSequence = "";
                foreach (string element in tempList)
                {
                    if (tempList.IndexOf(element) != (tempList.Count - 1))
                        variableSequence = variableSequence + element + ",";
                    else
                        variableSequence = variableSequence + element;
                }

                // clear from boolString
                Debug.Log(string.Format("[IKRC] - DeleteVariables({0}) boolString = {1}", preFix, boolString));
                string[] tempbString = boolString.Split(',');
                List<string> tempbList = tempbString.OfType<string>().ToList();
                for (int i = tempbList.Count - 1; i >= 0; i--)
                {
                    string element = tempbList[i];
                    if (element.StartsWith(preFix))
                        tempbList.Remove(element);
                }
                boolString = "";
                foreach (string element in tempbList)
                {
                    if (tempbList.IndexOf(element) != (tempbList.Count - 1))
                        vector2String = vector2String + element + ",";
                    else
                        vector2String = vector2String + element;
                }
                Debug.Log(string.Format("[IKRC] - DeleteVariables({0}) boolString = {1}", preFix, boolString));

                // clear from vector2String
                Debug.Log(string.Format("[IKRC] - DeleteVariables({0}) vector2String = {1}", preFix, vector2String));
                string[] temp2String = vector2String.Split(',');
                List<string> temp2List = temp2String.OfType<string>().ToList();
                for (int i = temp2List.Count - 1; i >= 0; i--)
                {
                    string element = temp2List[i];
                    if (element.StartsWith(preFix))
                        temp2List.Remove(element);
                }
                vector2String = "";
                foreach (string element in temp2List)
                {
                    if (temp2List.IndexOf(element) != (temp2List.Count - 1))
                        vector2String = vector2String + element + ",";
                    else
                        vector2String = vector2String + element;
                }
                Debug.Log(string.Format("[IKRC] - DeleteVariables({0}) vector2String = {1}", preFix, vector2String));

                // clear from vector3String
                string[] temp3String = vector3String.Split(',');
                List<string> temp3List = temp3String.OfType<string>().ToList();
                for (int i = temp3List.Count - 1; i >= 0; i--)
                {
                    string element = temp3List[i];
                    if (element.StartsWith(preFix))
                        temp3List.Remove(element);
                }
                vector3String = "";
                foreach (string element in temp3List)
                {
                    if (temp3List.IndexOf(element) != (temp3List.Count - 1))
                        vector3String = vector3String + element + ",";
                    else
                        vector3String = vector3String + element;
                }

                // clear from quaternionString
                string[] tempQString = quaternionString.Split(',');
                List<string> tempQList = tempQString.OfType<string>().ToList();
                for (int i = tempQList.Count - 1; i >= 0; i--)
                {
                    string element = tempQList[i];
                    if (element.StartsWith(preFix))
                        tempQList.Remove(element);
                }
                quaternionString = "";
                foreach (string element in tempQList)
                {
                    if (tempQList.IndexOf(element) != (tempQList.Count - 1))
                        quaternionString = quaternionString + element + ",";
                    else
                        quaternionString = quaternionString + element;
                }

                // clear from variableList
                for (int i = variableList.Count - 1; i >= 0; i--)
                {
                    string element = variableList[i];
                    if (element.StartsWith(preFix))
                        variableList.Remove(element);
                }

                // clear from vector2List
                for (int i = vector2List.Count - 1; i >= 0; i--)
                {
                    VECTOR2 element = vector2List[i];
                    if (element.name.StartsWith(preFix))
                        vector2List.Remove(element);
                }

                // clear from vector3List
                for (int i = vector3List.Count - 1; i >= 0; i--)
                {
                    VECTOR3 element = vector3List[i];
                    if (element.name.StartsWith(preFix))
                        vector3List.Remove(element);
                }

                // clear from quaternionList
                for (int i = quaternionList.Count - 1; i >= 0; i--)
                {
                    QUATERNION element = quaternionList[i];
                    if (element.name.StartsWith(preFix))
                        quaternionList.Remove(element);
                }

                result = true;
                return result;
            }

            public void LoadVariables(PluginConfiguration config)
            {
                Debug.Log(string.Format("[IKRC] LoadVariables() Start"));

                // if don't contain variable then make empty variable !!!

                boolString = config.GetValue<string>("boolString").Replace(" ", String.Empty);
                string[] tempbList = boolString.Split(',');

                //foreach (string text in tempbList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() tempbList - {0}", text));

                foreach (string element in tempbList)
                {
                    boolList.Add(new BOOL(element, config.GetValue<bool>(element)));
                }



                vector2String = config.GetValue<string>("vector2String").Replace(" ", String.Empty);
                string[] tempList = vector2String.Split(',');

                //foreach (string text in tempList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() tempList - {0}", text));

                foreach (string element in tempList)
                {
                    vector2List.Add(new VECTOR2(element, config.GetValue<Vector2>(element)));
                }

                //Debug.Log(string.Format("[IKRC] LoadVariables() vector2List"));
                //foreach (VECTOR2 vector2 in vector2List)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() {0} {1}", vector2.name, VectorToString(vector2.value, "0.00")));

                vector3String = config.GetValue<string>("vector3String").Replace(" ", String.Empty);
                tempList = vector3String.Split(',');

                //foreach (string text in tempList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() tempList - {0}", text));

                foreach (string element in tempList)
                {
                    vector3List.Add(new VECTOR3(element, config.GetValue<Vector3>(element)));
                }

                //Debug.Log(string.Format("[IKRC] LoadVariables() vector3List"));
                //foreach (VECTOR3 vector3 in vector3List)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() {0} {1}", vector3.name, VectorToString(vector3.value, "0.00")));

                quaternionString = config.GetValue<string>("quaternionString").Replace(" ", String.Empty);
                tempList = quaternionString.Split(',');

                //foreach (string text in tempList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() tempList - {0}", text));

                foreach (string element in tempList)
                {
                    quaternionList.Add(new QUATERNION(element, config.GetValue<Quaternion>(element)));
                }

                //Debug.Log(string.Format("[IKRC] LoadVariables() quaternionList"));
                //foreach (QUATERNION quaternion in quaternionList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() {0} {1}", quaternion.name, QuaternionToString(quaternion.value, "0.00")));

                variableSequence = config.GetValue<string>("variableSequence").Replace(" ", String.Empty);
                tempList = variableSequence.Split(',');

                //foreach (string text in tempList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() tempList - {0}", text));

                foreach (string element in tempList)
                {
                    variableList.Add(element);
                }

                //Debug.Log(string.Format("[IKRC] LoadVariables() variableList"));
                //foreach (string variable in variableList)
                //    Debug.Log(string.Format("[IKRC] LoadVariables() {0}", variable));

                foreach (string element in variableList)
                {
                    foreach (BOOL Belement in boolList)
                    {
                        if (Belement.name == element)
                        {
                            Belement.value = config.GetValue<int>(Belement.name);
                            break;
                        }
                    }
                    foreach (VECTOR2 V2element in vector2List)
                    {
                        if (V2element.name == element)
                        {
                            V2element.value = config.GetValue<Vector2>(V2element.name);
                            break;
                        }
                    }
                    foreach (VECTOR3 V3element in vector3List)
                    {
                        if (V3element.name == element)
                        {
                            V3element.value = config.GetValue<Vector3>(V3element.name);
                            break;
                        }
                    }
                    foreach (QUATERNION Qelement in quaternionList)
                    {
                        if (Qelement.name == element)
                        {
                            Qelement.value = config.GetValue<Quaternion>(Qelement.name);
                            break;
                        }
                    }
                }

                foreach (VECTOR2 V2element in vector2List)
                {
                    if (V2element.name.EndsWith("-IRC-Window-Position"))
                    {
                        if (!robotIDList.Contains(V2element.name.Remove(V2element.name.IndexOf("-IRC-Window-Position"))))
                            robotIDList.Add(V2element.name.Remove(V2element.name.IndexOf("-IRC-Window-Position")));
                    }
                }

                ContentIsLoaded = true;

                Debug.Log(string.Format("[IKRC] LoadVariables() End"));
            }

            public void SaveVariables(PluginConfiguration config)
            {
                Debug.Log(string.Format("[IKRC] SaveVariables() Start"));

                config.SetValue("boolString", boolString);
                config.SetValue("vector2String", vector2String);
                config.SetValue("vector3String", vector3String);
                config.SetValue("quaternionString", quaternionString);
                config.SetValue("variableSequence", variableSequence);

                foreach (string element in variableList)
                {
                    foreach (BOOL Belement in boolList)
                    {
                        if (Belement.name == element)
                        {
                            config.SetValue(Belement.name, Belement.value);
                            break;
                        }
                    }
                    foreach (VECTOR2 V2element in vector2List)
                    {
                        if (V2element.name == element)
                        {
                            config.SetValue(V2element.name, V2element.value);
                            break;
                        }
                    }
                    foreach (VECTOR3 V3element in vector3List)
                    {
                        if (V3element.name == element)
                        {
                            config.SetValue(V3element.name, V3element.value);
                            break;
                        }
                    }
                    foreach (QUATERNION Qelement in quaternionList)
                    {
                        if (Qelement.name == element)
                        {
                            config.SetValue(Qelement.name, Qelement.value);
                            break;
                        }
                    }
                }

                Debug.Log(string.Format("[IKRC] SaveVariables() End"));
            }

            public bool GetBool(string BOname)
            {
                bool result = false;
                OperationIsSuccess = false;

                foreach (BOOL elemenet in boolList)
                {
                    if (BOname == elemenet.name)
                    {
                        result = elemenet.value != 0 ? true : false;
                        OperationIsSuccess = true;
                        break;
                    }
                }

                if (result != null)
                {
                    Debug.Log(string.Format("[IKRC] GetBool('{0}')", BOname));
                    Debug.Log(string.Format("[IKRC] {0} = " + result.ToString(), BOname));
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] GetBool('{0}') result == null", BOname));
                }

                return result;
            }

            public Vector2 GetVector2(string V2name)
            {
                Vector2 result = new Vector2();
                OperationIsSuccess = false;

                foreach (VECTOR2 elemenet in vector2List)
                {
                    if (V2name == elemenet.name)
                    {
                        result = elemenet.value;
                        OperationIsSuccess = true;
                        break;
                    }
                }

                if (result != null)
                {
                    Debug.Log(string.Format("[IKRC] GetVector2('{0}')", V2name));
                    Debug.Log(string.Format("[IKRC] {0} = " + VectorToString(result, "0.00"), V2name));
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] GetVector2('{0}') result == null", V2name));
                }

                return result;
            }

            public Vector3 GetVector3(string V3name)
            {
                Vector3 result = new Vector3();
                OperationIsSuccess = false;

                foreach (VECTOR3 elemenet in vector3List)
                {
                    if (V3name == elemenet.name)
                    {
                        result = elemenet.value;
                        OperationIsSuccess = true;
                        break;
                    }
                }

                if (result != null)
                {
                    Debug.Log(string.Format("[IKRC] GetVector3('{0}')", V3name));
                    Debug.Log(string.Format("[IKRC] {0} = " + VectorToString(result, "0.00"), V3name));
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] GetVector3('{0}') result == null", V3name));
                }


                return result;
            }

            public Quaternion GetQuaternion(string Qname)
            {
                Quaternion result = new Quaternion();
                OperationIsSuccess = false;

                foreach (QUATERNION elemenet in quaternionList)
                {
                    if (Qname == elemenet.name)
                    {
                        result = elemenet.value;
                        OperationIsSuccess = true;
                        break;
                    }
                }

                if (result != null)
                {
                    Debug.Log(string.Format("[IKRC] GetQuaternion('{0}')", Qname));
                    Debug.Log(string.Format("[IKRC] {0} = " + QuaternionToString(result, "0.00"), Qname));
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] GetQuaternion('{0}') result == null", Qname));
                }

                return result;
            }

            public void SetValue(string name, bool BOvalue)
            {
                OperationIsSuccess = false;

                foreach (BOOL elemenet in boolList)
                {
                    if (name == elemenet.name)
                    {
                        elemenet.value = BOvalue ? 1 : 0;
                        OperationIsSuccess = true;
                        return;
                    }
                }

                Debug.Log(string.Format("[IKRC] SetValue('{0}') non element", name));
            }

            public void SetValue(string name, Vector2 V2value)
            {
                OperationIsSuccess = false;

                foreach (VECTOR2 elemenet in vector2List)
                {
                    if (name == elemenet.name)
                    {
                        elemenet.value = V2value;
                        OperationIsSuccess = true;
                        return;
                    }
                }

                Debug.Log(string.Format("[IKRC] SetValue('{0}') non element", name));
            }

            public void SetValue(string name, Vector3 V3value)
            {
                OperationIsSuccess = false;

                foreach (VECTOR3 elemenet in vector3List)
                {
                    if (name == elemenet.name)
                    {
                        elemenet.value = V3value;
                        OperationIsSuccess = true;
                        return;
                    }
                }

                Debug.Log(string.Format("[IKRC] SetValue('{0}') non element", name));
            }

            public void SetValue(string name, Quaternion Qvalue)
            {
                OperationIsSuccess = false;

                foreach (QUATERNION elemenet in quaternionList)
                {
                    if (name == elemenet.name)
                    {
                        elemenet.value = Qvalue;
                        OperationIsSuccess = true;
                        return;
                    }
                }

                Debug.Log(string.Format("[IKRC] SetValue('{0}') non element", name));
            }
        }

        ConfigXML configXML;

        public class VectorSM
        {
            public Vector3 Translation;
            public Quaternion Rotation;

            public VectorSM()
            {
                Translation = new Vector3();
                Rotation = Quaternion.identity;
            }
        }

        public class LocationRotationDiff
        {
            public float LocationDiff;
            public float RotationDiff;

            public LocationRotationDiff()
            {
            }
        }

        public class Buttons
        {
            public Vector3 Translation;
            public Quaternion Rotation;

            public Buttons()
            {
                Translation = new Vector3();
                Rotation = Quaternion.identity;
            }
        }

        public class FKparams
        {
            public Quaternion PartRotation;
            public Vector3 ParentOffset;
            public Vector3 Axis;
            //public Vector3 Position;
            public Quaternion Rotation;

            public FKparams()
            {
            }

            public FKparams Clone()
            {
                FKparams destination = new FKparams();

                destination.PartRotation.x = this.PartRotation.x;
                destination.PartRotation.y = this.PartRotation.y;
                destination.PartRotation.z = this.PartRotation.z;
                destination.PartRotation.w = this.PartRotation.w;
                destination.ParentOffset.x = this.ParentOffset.x;
                destination.ParentOffset.y = this.ParentOffset.y;
                destination.ParentOffset.z = this.ParentOffset.z;
                destination.Axis.x = this.Axis.x;
                destination.Axis.y = this.Axis.y;
                destination.Axis.z = this.Axis.z;
                destination.Rotation.x = this.Rotation.x;
                destination.Rotation.y = this.Rotation.y;
                destination.Rotation.z = this.Rotation.z;
                destination.Rotation.w = this.Rotation.w;

                return destination;
            }

            public void Copy(FKparams from)
            {
                this.PartRotation.x = from.PartRotation.x;
                this.PartRotation.y = from.PartRotation.y;
                this.PartRotation.z = from.PartRotation.z;
                this.PartRotation.w = from.PartRotation.w;
                this.ParentOffset.x = from.ParentOffset.x;
                this.ParentOffset.y = from.ParentOffset.y;
                this.ParentOffset.z = from.ParentOffset.z;
                this.Axis.x = from.Axis.x;
                this.Axis.y = from.Axis.y;
                this.Axis.z = from.Axis.z;
                this.Rotation.x = from.Rotation.x;
                this.Rotation.y = from.Rotation.y;
                this.Rotation.z = from.Rotation.z;
                this.Rotation.w = from.Rotation.w;
            }

            //public void ConfigWriteParams(PluginConfiguration config, int servoNumber, FKparams fkParams)
            //{
            //    config.SetValue(servoNumber.ToString() + "-Servo-Params-ParentOffset", fkParams.ParentOffset);
            //    config.SetValue(servoNumber.ToString() + "-Servo-Params-Axis", fkParams.Axis);
            //    config.SetValue(servoNumber.ToString() + "-Servo-Params-Position", fkParams.Position);
            //    config.SetValue(servoNumber.ToString() + "-Servo-Params-Rotation", fkParams.Rotation);

            //    config.save();

            //    Debug.Log(string.Format("[IKRC] {0} - ConfigWriteParams( " + servoNumber.ToString() + "-Servo-Params )", 50));

            //    //config.SetValue("controlWindowPosition", _controlWindowPosition);
            //    //config.SetValue("editorWindowPosition", _editorWindowPosition);
            //    //config.SetValue("editorWindowSize", _editorWindowSize);
            //    //config.SetValue("uiSettingsWindowPosition", _settingsWindowPosition);
            //    //config.SetValue("presetsWindowPosition", _presetsWindowPosition);
            //    //config.SetValue("UIAlphaValue", (double)_UIAlphaValue);
            //    //config.SetValue("UIScaleValue", (double)_UIScaleValue);
            //    //config.SetValue("useEC", UseElectricCharge);
            //    //config.SetValue("useBlizzyToolbar", useBlizzyToolbar);

            //    //config.save();
            //}

            //public FKparams ConfigReadParams(PluginConfiguration config, int servoNumber)
            //{
            //    FKparams fkParams = new FKparams();

            //    config.load();

            //    fkParams.ParentOffset = config.GetValue<Vector3>(servoNumber.ToString() + "-Servo-Params-ParentOffset");
            //    fkParams.Axis = config.GetValue<Vector3>(servoNumber.ToString() + "-Servo-Params-Axis");
            //    fkParams.Position = config.GetValue<Vector3>(servoNumber.ToString() + "-Servo-Params-Position");
            //    fkParams.Rotation = config.GetValue<Quaternion>(servoNumber.ToString() + "-Servo-Params-Rotation");

            //    return fkParams;

            //    //config.load();

            //    //_controlWindowPosition = config.GetValue<Vector3>("controlWindowPosition");
            //    //_editorWindowPosition = config.GetValue<Vector3>("editorWindowPosition");
            //    //_editorWindowSize = config.GetValue<Vector2>("editorWindowSize");
            //    //_settingsWindowPosition = config.GetValue<Vector3>("uiSettingsWindowPosition");
            //    //_presetsWindowPosition = config.GetValue<Vector3>("presetsWindowPosition");

            //    //_UIAlphaValue = (float)config.GetValue<double>("UIAlphaValue", 0.8);
            //    //_UIScaleValue = (float)config.GetValue<double>("UIScaleValue", 1.0);
            //    //UseElectricCharge = config.GetValue<bool>("useEC", true);
            //    //useBlizzyToolbar = config.GetValue<bool>("useBlizzyToolbar", false);
            //}
        }

        public class IKservo
        {
            public Part part;
            public IRminWrapper.IServo iservo;
            //public Vector3 ParentOffset;
            //public Vector3 Position;
            //public Quaternion Rotation;
            //public Quaternion localRotation;
            //public Vector3 Axis;
            public Transform ServoTransform;
            public string name;
            public float MinAngle = -270f;
            public float MaxAngle = 270f;

            public FKparams fkParams;

            //public Vector3 StartRotOffset;

            public IKservo()
            {
            }

            public IKservo(Part part, IRminWrapper.IServo iservo)
            {
                this.part = part;
                this.iservo = iservo;
                this.fkParams = new FKparams();
            }

            public IKservo(Part part)
            {
                this.part = part;
                this.fkParams = new FKparams();
            }

            public IKservo(FKparams fkParams)
            {
                this.fkParams = fkParams;
            }

            public IKservo Clone()
            {
                IKservo destination = new IKservo();

                destination.part = this.part;
                // destination.iservo = this.iservo.Clone();
                destination.iservo = this.iservo;
                destination.ServoTransform = this.ServoTransform;
                destination.name = this.name;
                destination.MinAngle = this.MinAngle;
                destination.MaxAngle = this.MaxAngle;
                destination.fkParams = this.fkParams.Clone();

                return destination;
            }

            public void Copy(IKservo from)
            {
                this.part = from.part;
                this.iservo = from.iservo;
                this.ServoTransform = from.ServoTransform;
                this.name = from.name;
                this.MinAngle = from.MinAngle;
                this.MaxAngle = from.MaxAngle;
                this.fkParams.Copy(from.fkParams);
            }
        }

        Buttons buttons;
        List<IRminWrapper.IServo> allServos;
        List<IKservo> AllIkServo;
        List<IKservo> ArmIkServos = new List<IKservo>();
        List<IKservo> ArmNDIkServos;
        List<IKservo> ArmRDIkServos;
        List<IKservo> FKparamsIkServo;
        List<IKservo> RdFKparamsIkServo;
        List<Part> listOfChildrenPart;
        VectorSM FKvector = new VectorSM();
        VectorSM baseState = new VectorSM();
        //GameObject[] servoGimbal = new GameObject[8];
        GameObject[] servoGimbal;
        GameObject global = new GameObject();
        Part lastPart;
        Quaternion lastPartRotation;

        Part IkBasePart;

        //IKservo partIkServo;

        List<IKservo> indepServo;

        GameObject hoverObject;
        Transform dockingNodeObject;

        IKservo orgLEEikServo;
        bool VeeIsActive = false;

        Mode mode;
        int limitDepth = 2;

        private bool TrajectoryEditorMode = false;
        GameObject TrajectoryCursor = new GameObject();
        public bool HasTrajectoryCursor = false;
        List<GameObject> trajectoryList = new List<GameObject>();
        bool AddTrajectory = false;
        bool RemoveTrajectory = false;
        bool ClearTrajectory = false;
        private bool TrajectoryControlMode = false;
        private bool ReverseIkDirection = false;
        private bool prevReverseIkDirection = false;
        bool StepTrajectory = false;
        private bool TrajectoryStepMode = false;

        private enum Mode
        {
            PART,
            UI,
            OBJECT
        }

        private enum IkDirection { NORMAL, REVERSE }

        string[] JointVariableNames = { "Servo-Params-PartRotation", "Servo-Params-ParentOffset", "Servo-Params-Axis", "Servo-Params-Rotation" };
        string[] JointRdVariableNames = { "Servo-RdParams-PartRotation", "Servo-RdParams-ParentOffset", "Servo-RdParams-Axis", "Servo-RdParams-Rotation" };
        string[] JointVariableTypes = { "Quaternion", "Vector3", "Vector3", "Quaternion" };
        string[] JointRdVariableTypes = { "Quaternion", "Vector3", "Vector3", "Quaternion" };
        string[] JointList;

        public class IndepJoint
        {
            public string name;
            public Part part;
            public uint persistentId;
            public MinMaxAngle minmaxangle;

            public IndepJoint(string name)
            {
                this.name = name;
            }
        }
        List<IndepJoint> IndependentJointList;
        string[] IndependentPerId;

        Vector3[] JointsRealAxis;

        List<BoolText> ridbtList;

        public class MinMaxAngle
        {
            public float Min = 0f;
            public float Max = 0f;

            public MinMaxAngle(float min, float max)
            {
                Min = min;
                Max = max;
            }

        }

        List<MinMaxAngle> minMaxAnglesList;
        List<MinMaxAngle> independentMinMaxAnglesList;

        public float[] theta;
        public float[] currentTheta;

        public float[] iota;
        public float[] currentIota;

        public bool[] blocking;
        public bool nearPDGFBool = false;
        public float distance;
        public float angle;
        public bool Success = false;
        public bool IsForceDist = false;
        public bool IsForceRoll = false;
        public bool IKsuccess = false;
        public bool Bug = true;
        public bool SubIterate = false;

        // endeffector target position
        public Vector3 globalPosition;
        // endeffector target orientation
        public Quaternion globalQuaternion;

        public int iterateNumber = 500;
        public float forceDistNumberZ = 0.6f;
        public float forceRollNumberZ = 0.0f;
        public float forceDistNumberX = 0.0f;
        public float forceRollNumberX = 0.0f;
        public float forceDistNumberY = 0.0f;
        public float forceRollNumberY = 0.0f;
        public float LearningRatePos;
        public float LearningRateOri;
        //public float SamplingAngle = 0.02f;

        //public float DistanceThreshold = 0.01f;
        public float Distance;
        //public float AngleThreshold = 0.5f;
        public float Angle;

        public float stlBit = 0.5f;
        private string stlBitString = "0.5";

        public float workingDistance;

        //public float MaxPosErr = 10f;
        //public float MaxOriErr = 180f;

        public float DinLearningRatePos;
        public float DinLearningRateOri;

        public int failedIterateCycle = 0;
        public int iterateThreshold = 10;

        private Vector3 basePosition;
        private Quaternion baseQuaternion;

        private Vector3 partPosition;
        private Quaternion partRotation;
        private Quaternion storedPartRotation;

        private Vector3 prevPartPosition;
        private Quaternion prevPartRotation;

        private GameObject gameObject;

        //private Transform leeTransform;
        //private Transform leeCamTransform;
        //private Vector3 vesselNewPosition;
        //private Quaternion vesselNewOrientation;
        //private bool servoSwap = false;

        private bool InitIRWrapper()
        {
            //if (!IRWrapper.APIReady)
            //{
            Debug.Log(string.Format("[IKRC] {0} IRminWrapper.InitWrapper() START", 1));
            IRminWrapper.InitWrapper();
            Debug.Log(string.Format("[IKRC] {0} IRminWrapper.InitWrapper() END", 2));
            //}
            //else
            //    Debug.Log(string.Format("[IKRC] {0} InitIRWrapper() IRWrapper.APIReady == true", 1));
            return IRminWrapper.APIReady;
        }

        #region mapping servo structure
        private void CreateFKparams()
        {
            if (FKparamsIkServo == null)
                FKparamsIkServo = new List<IKservo>();

            for (int i = 0; i < GetJointListLengthFromPartConfig(); i++)
            {
                FKparamsIkServo.Add(new IKservo(new FKparams()));
            }

        }

        private void CreateRdFKparams()
        {
            if (RdFKparamsIkServo == null)
                RdFKparamsIkServo = new List<IKservo>();

            for (int i = 0; i < GetJointListLengthFromPartConfig(); i++)
            {
                RdFKparamsIkServo.Add(new IKservo(new FKparams()));
            }

        }

        private void SetFKParamsToBuffer()
        {
            int servoNumber = 0;
            foreach (IKservo ikServo in FKparamsIkServo)
            {
                ikServo.fkParams.PartRotation = ArmNDIkServos[servoNumber].fkParams.PartRotation;
                ikServo.fkParams.ParentOffset = ArmNDIkServos[servoNumber].fkParams.ParentOffset;
                ikServo.fkParams.Axis = ArmNDIkServos[servoNumber].fkParams.Axis;
                //ikServo.fkParams.Position = ArmNDIkServos[servoNumber].fkParams.Position;
                ikServo.fkParams.Rotation = ArmNDIkServos[servoNumber].fkParams.Rotation;

                servoNumber++;
            }

            IsLoadedFkParams = true;
        }

        private void SetRdFKParamsToBuffer()
        {
            int servoNumber = 0;
            foreach (IKservo ikServo in RdFKparamsIkServo)
            {
                ikServo.fkParams.PartRotation = ArmRDIkServos[servoNumber].fkParams.PartRotation;
                ikServo.fkParams.ParentOffset = ArmRDIkServos[servoNumber].fkParams.ParentOffset;
                ikServo.fkParams.Axis = ArmRDIkServos[servoNumber].fkParams.Axis;
                //ikServo.fkParams.Position = ArmRDIkServos[servoNumber].fkParams.Position;
                ikServo.fkParams.Rotation = ArmRDIkServos[servoNumber].fkParams.Rotation;

                servoNumber++;
            }

            IsLoadedRdFkParams = true;
        }

        private bool GetFKParamsFromBuffer()
        {
            if (IsLoadedFkParams)
            {
                Debug.Log(string.Format("[IKRC] {0} - GetFKParamsFromBuffer()", 42));
                Debug.Log(string.Format("[IKRC] {0} - part.transform.rotation " + QuaternionToString(part.transform.rotation, "0.00"), 42));
                Debug.Log(string.Format("[IKRC] {0} - FKparamsIkServo[0].fkParams.PartRotation " + QuaternionToString(FKparamsIkServo[0].fkParams.PartRotation, "0.00"), 42));
                int servoNumber = 0;
                foreach (IKservo ikServo in FKparamsIkServo)
                {
                    ArmNDIkServos[servoNumber].fkParams.PartRotation = ikServo.fkParams.PartRotation;
                    ArmNDIkServos[servoNumber].fkParams.ParentOffset = ikServo.fkParams.ParentOffset;
                    ArmNDIkServos[servoNumber].fkParams.Axis = ikServo.fkParams.Axis;
                    ArmNDIkServos[servoNumber].fkParams.Rotation = ikServo.fkParams.Rotation;

                    Debug.Log(string.Format("[IKRC] {0} - ParentOffset " + VectorToString(ArmNDIkServos[servoNumber].fkParams.ParentOffset, "0.00"), 42));
                    Debug.Log(string.Format("[IKRC] {0} - Axis " + VectorToString(ikServo.fkParams.Axis, "0.0"), 40));
                    Debug.Log(string.Format("[IKRC] {0} - Rotation " + QuaternionToString(ArmNDIkServos[servoNumber].fkParams.Rotation, "0.00"), 42));

                    servoNumber++;
                }

                IkBasePart = part;
                //Debug.Log(string.Format("[IKRC] {1} - IkBasePart.name = {0}", IkBasePart.name, 42));
            }
            else
                return false;

            return true;
        }

        private bool GetRdFKParamsFromBuffer()
        {
            if (IsLoadedRdFkParams)
            {
                Debug.Log(string.Format("[IKRC] {0} - GetRdFKParamsFromBuffer()", 42));
                Debug.Log(string.Format("[IKRC] {0} - part.transform.rotation " + QuaternionToString(part.transform.rotation, "0.00"), 42));
                Debug.Log(string.Format("[IKRC] {0} - RdFKparamsIkServo[0].fkParams.PartRotation " + QuaternionToString(RdFKparamsIkServo[0].fkParams.PartRotation, "0.00"), 42));
                int servoNumber = 0;
                foreach (IKservo ikServo in RdFKparamsIkServo)
                {
                    ArmRDIkServos[servoNumber].fkParams.PartRotation = ikServo.fkParams.PartRotation;
                    ArmRDIkServos[servoNumber].fkParams.ParentOffset = ikServo.fkParams.ParentOffset;
                    ArmRDIkServos[servoNumber].fkParams.Axis = ikServo.fkParams.Axis;
                    ArmRDIkServos[servoNumber].fkParams.Rotation = ikServo.fkParams.Rotation;

                    Debug.Log(string.Format("[IKRC] {0} - ParentOffset " + VectorToString(ArmRDIkServos[servoNumber].fkParams.ParentOffset, "0.00"), 42));
                    Debug.Log(string.Format("[IKRC] {0} - Axis " + VectorToString(ikServo.fkParams.Axis, "0.0"), 40));
                    Debug.Log(string.Format("[IKRC] {0} - Rotation " + QuaternionToString(ArmRDIkServos[servoNumber].fkParams.Rotation, "0.00"), 42));

                    servoNumber++;
                }
            }
            else
                return false;

            return true;
        }

        // servoList = TRFCA2ARoll, TRFCA2AYaw, TRFCA2APitch, TRFCA2CElbow, TRFCA2BPitch, TRFCA2BYaw, TRFCA2BRoll, TRFCA2LEEwCam
        private bool GetJointListFromPartConfig()
        {
            Debug.Log(string.Format("[IKRC] GetJointListFromPartConfig() servoList = " + servoList));
            servoList = servoList.Replace(" ", String.Empty);
            int i = 0;
            JointList = servoList.Split(',');
            if (JointList.Length > 0)
                foreach (string element in JointList)
                {
                    Debug.Log(string.Format("[IKRC] GetJointListFromPartConfig() JointList[{0}] = " + JointList[i], i));
                    i++;
                }
            return (JointList.Length > 1);
        }

        private bool GetIndependentJointListFromPartConfig()
        {
            Debug.Log(string.Format("[IKRC] GetIndependentJointListFromPartConfig() independentServoList = '" + independentServoList + "'"));
            if (independentServoList != "")
            {
                IndependentJointList = new List<IndepJoint>();
                Debug.Log(string.Format("[IKRC] GetIndependentJointListFromPartConfig() independentServoList = " + independentServoList));
                independentServoList = independentServoList.Replace(" ", String.Empty);
                int i = 0;
                string[] IndepJointList = independentServoList.Split(',');
                if (IndepJointList.Length > 0)
                    foreach (string element in IndepJointList)
                    {
                        Debug.Log(string.Format("[IKRC] GetIndependentJointListFromPartConfig() IndepJointList[{0}] = " + IndepJointList[i], i));
                        IndependentJointList.Add(new IndepJoint(element));
                        i++;
                    }
                return (IndepJointList.Length > 0);
            }
            return false;
        }

        private bool GetIndependentPersistentIdFromPartConfig()
        {
            Debug.Log(string.Format("[IKRC] GetIndependentPersistentIdFromPartConfig() independentServoPersistentId = '" + independentServoPersistentId + "'"));
            if (independentServoPersistentId != "")
            {
                Debug.Log(string.Format("[IKRC] GetIndependentPersistentIdFromPartConfig() independentServoPersistentId = " + independentServoPersistentId));
                independentServoPersistentId = independentServoPersistentId.Replace(" ", String.Empty);
                int i = 0;
                IndependentPerId = independentServoPersistentId.Split(',');
                if (IndependentPerId.Length > 0)
                    foreach (string element in IndependentPerId)
                    {
                        Debug.Log(string.Format("[IKRC] GetIndependentPersistentIdFromPartConfig() IndependentPerId[{0}] = " + IndependentPerId[i], i));
                        i++;
                    }
                return (IndependentPerId.Length > 0);
            }
            return false;
        }

        // servoRealAxis = (0 1 0), (1 0 0), (0 0 1), (0 0 -1), (0 0 -1), (1 0 0), (0 1 0), (0 0 0)
        private bool GetJointRealAxisFromPartConfig()
        {
            Debug.Log(string.Format("[IKRC]  GetJointRealAxisFromPartConfig() servoRealAxis = " + servoRealAxis));
            int i = 0;
            // (0 1 0), (1 0 0), (0 0 1), (0 0 -1), (0 0 -1), (1 0 0), (0 1 0), (0 0 0)
            string[] servoRealAxisList = servoRealAxis.Split(',');
            // '(0 1 0)' ' (1 0 0)' ' (0 0 1)' ' (0 0 -1)' ' (0 0 -1)' ' (1 0 0)' ' (0 1 0)' ' (0 0 0)'
            foreach (string element in servoRealAxisList)
            {
                Debug.Log(string.Format("[IKRC]  GetJointRealAxisFromPartConfig() element = {0}", element));
                string[] axisString = element.Replace(" (", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Split(' ');
                Debug.Log(string.Format("[IKRC]  GetJointRealAxisFromPartConfig() axisString.Length = {0}", axisString.Length));
                if (axisString.Length == 3)
                {
                    Debug.Log(string.Format("[IKRC]  GetJointRealAxisFromPartConfig() [0] = {0}, [1] = {1}, [2] = {2} ", axisString[0], axisString[1], axisString[2]));
                    if (JointsRealAxis[i] != null)
                        JointsRealAxis[i] = new Vector3(float.Parse(axisString[0]), float.Parse(axisString[1]), float.Parse(axisString[2]));
                    else
                    {
                        Debug.Log(string.Format("[IKRC]  GetJointRealAxisFromPartConfig() JointsRealAxis[{0}] == null", i));
                        return false;
                    }
                }
                else
                {
                    Debug.Log(string.Format("[IKRC]  GetJointRealAxisFromPartConfig() axisString.Length != 3"));
                    return false;
                }
                i++;
            }
            return (JointList.Length > 1);
        }

        // minMaxAngles = (-180 180), (-2 145), (-160 2), (-120 120), (-120 120), (-447 447), (0 0)
        private bool GetMinMaxAnglesFromPartConfig()
        {
            Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() minMaxAngles = " + minMaxAngles));
            // (-180 180), (-2 145), (2 -160), (-120 120), (-120 120), (-447 447), (0 0)
            string[] minMaxAnglesArray = minMaxAngles.Split(',');
            // '-180 180)' '(-2 145)' '(2 -160)' '(-120 120)' '(-120 120)' '(-447 447)' '(0 0)'
            foreach (string element in minMaxAnglesArray)
            {
                Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() element = {0}", element));
                string[] minMaxString = element.Replace(" (", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Split(' ');
                Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() minMaxString.Length = {0}", minMaxString.Length));
                if (minMaxString.Length == 2)
                {
                    Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() [0] = {0}, [1] = {1}", minMaxString[0], minMaxString[1]));
                    if (minMaxAnglesList != null)
                        minMaxAnglesList.Add(new MinMaxAngle(float.Parse(minMaxString[0]), float.Parse(minMaxString[1])));
                    else
                    {
                        Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() minMaxAnglesList == null"));
                        return false;
                    }
                }
                else
                {
                    Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() minMaxString.Length != 2"));
                    return false;
                }
            }

            //int i = 0;
            //foreach (MinMaxAngle element in minMaxAnglesList)
            //{
            //    Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() - {0} - min = {1} max = {2}", i, element.Min, element.Max));
            //    i++;
            //}

            return (minMaxAnglesList.Count > 0);
        }

        private bool GetIndependentMinMaxAnglesFromPartConfig()
        {
            Debug.Log(string.Format("[IKRC]  GetIndependentMinMaxAnglesFromPartConfig() independentMinMaxAngles = '" + independentMinMaxAngles + "'"));
            if (independentMinMaxAngles != "")
            {
                independentMinMaxAnglesList = new List<MinMaxAngle>();
                Debug.Log(string.Format("[IKRC]  GetIndependentMinMaxAnglesFromPartConfig() independentMinMaxAngles = " + independentMinMaxAngles));
                // (-180 180), (-2 145), (2 -160), (-120 120), (-120 120), (-447 447), (0 0)
                string[] independentMinMaxAnglesArray = independentMinMaxAngles.Split(',');
                // '-180 180)' '(-2 145)' '(2 -160)' '(-120 120)' '(-120 120)' '(-447 447)' '(0 0)'
                foreach (string element in independentMinMaxAnglesArray)
                {
                    Debug.Log(string.Format("[IKRC]  GetIndependentMinMaxAnglesFromPartConfig() element = {0}", element));
                    string[] independentMinMaxString = element.Replace(" (", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Split(' ');
                    Debug.Log(string.Format("[IKRC]  GetIndependentMinMaxAnglesFromPartConfig() independentMinMaxString.Length = {0}", independentMinMaxString.Length));
                    if (independentMinMaxString.Length == 2)
                    {
                        Debug.Log(string.Format("[IKRC]  GetIndependentMinMaxAnglesFromPartConfig() [0] = {0}, [1] = {1}", independentMinMaxString[0], independentMinMaxString[1]));
                        if (independentMinMaxAnglesList != null)
                            independentMinMaxAnglesList.Add(new MinMaxAngle(float.Parse(independentMinMaxString[0]), float.Parse(independentMinMaxString[1])));
                        else
                        {
                            Debug.Log(string.Format("[IKRC]  GetIndependentMinMaxAnglesFromPartConfig() minMaxAnglesList == null"));
                            return false;
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() minMaxString.Length != 2"));
                        return false;
                    }
                }

                //int i = 0;
                //foreach (MinMaxAngle element in minMaxAnglesList)
                //{
                //    Debug.Log(string.Format("[IKRC]  GetMinMaxAnglesFromPartConfig() - {0} - min = {1} max = {2}", i, element.Min, element.Max));
                //    i++;
                //}

                return (minMaxAnglesList.Count > 0);
            }
            return false;
        }

        private int GetJointListLengthFromPartConfig()
        {
            int number = 0;

            number = servoList.Replace(" ", String.Empty).Split(',').Length;

            return number;
        }

        private bool CheckServosZeroState(bool stateToTheta = false)
        {
            if (stateToTheta)
                StatesToThetas();
            foreach (float element in theta)
            {
                if (element != 0f)
                    return false;
            }

            return true;
        }

        private void StatesToThetas()
        {
            // Get angle of Joints
            if (ArmIkServos != null)
                for (int j = 0; j < ArmIkServos.Count - 1; j++)
                    theta[j] = ArmIkServos[j].iservo.Position;
            else
                Debug.Log(string.Format("[IKRC] StatesToThetas() ArmIkServos == null"));
        }

        private bool CheckExistArm()
        {
            // if part exist
            if (part != null)
            {
                // checking list of joint exist, step by step
                for (int i = 0; i < JointList.Length; i++)
                {
                    // 1st element in list of joint
                    if (i == 0)
                    {
                        // if base part is not the 1st joint in the list
                        if (part.name != JointList[0])
                        {
                            // write log: joint is child of the part or not
                            Debug.Log(string.Format("[IKRC] CheckExistArm() - JointList[{0}] = " + JointList[i] + " " + ((part.FindChildPart(JointList[i], true) != null) ? "exist" : "not exist"), i));
                            // if not then list all child element and return by false
                            if (part.FindChildPart(JointList[i], true) == null)
                            {
                                Part[] allChidPart = part.FindChildParts<Part>(true);
                                foreach (Part element in allChidPart)
                                {
                                    if (element != null)
                                        Debug.Log(string.Format("[TRF] CheckExistArm() - allChidPart - {0}", element.name));
                                    else
                                        break;
                                }
                                return false;
                            }
                        }
                        // if base part is the 1st joint in the list
                        else
                            // write log: exist or not
                            Debug.Log(string.Format("[IKRC] CheckExistArm() - JointList[{0}] = " + JointList[i] + " " + ((part.name == JointList[0]) ? "exist" : "not exist"), i));
                    }
                    // not 1st element in list of joint
                    else
                    {
                        // write log: joint is child of the part or not
                        Debug.Log(string.Format("[IKRC] CheckExistArm() - JointList[{0}] = " + JointList[i] + " " + ((part.FindChildPart(JointList[i], true) != null) ? "exist" : "not exist"), i));
                        // if not then list all child element and return by false
                        if (part.FindChildPart(JointList[i], true) == null)
                        {
                            Part[] allChidPart = part.FindChildParts<Part>(true);
                            foreach (Part element in allChidPart)
                            {
                                if (element != null)
                                    Debug.Log(string.Format("[TRF] CheckExistArm() - allChidPart - {0}", element.name));
                                else
                                    break;
                            }
                            return false;
                        }
                    }
                }

                return true;
            }
            // if part don't exist
            else
                return false;
        }

        private bool CheckExistIndependentServos()
        {
            if (part != null)
            {
                for (int i = 0; i < IndependentJointList.Count; i++)
                {
                    // list.Find(x => x.GetId() == "xy");
                    Part indepJoint = vessel.parts.Find(p => p.name == IndependentJointList[i].name);
                    if (indepJoint != null)
                    {
                        Debug.Log(string.Format("[IKRC] CheckExistIndependentServos() - IndependentJointList[{0}] = " + IndependentJointList[i].name + " exist", i));
                        if (indepJoint.persistentId != Convert.ToUInt32(IndependentPerId[i]))
                        {
                            Debug.Log(string.Format("[IKRC] CheckExistIndependentServos() - indepJoint.persistentId = {1} not IndependentPerId[0] = {2}", i, indepJoint.persistentId, IndependentPerId[i]));
                            return false;
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("[IKRC] CheckExistIndependentServos() - IndependentJointList[{0}] = " + IndependentJointList[i].name + " not exist", i));
                        return false;
                    }
                    IndependentJointList[i].part = indepJoint;
                    IndependentJointList[i].persistentId = indepJoint.persistentId;
                    IndependentJointList[i].minmaxangle = new MinMaxAngle(independentMinMaxAnglesList[i].Min, independentMinMaxAnglesList[i].Max);
                }
                return true;
            }
            else
                return false;
        }

        private bool GetArmServos()
        {
            Debug.Log(string.Format("[IKRC]  GetArmServos() - START"));
            if (IRminWrapper.APIReady)
            {
                Debug.Log(string.Format("[IKRC] {0} - IRminWrapper.APIReady", 21));
                if (AllIkServo == null)
                    AllIkServo = new List<IKservo>();
                if (allServos == null)
                    allServos = new List<IRminWrapper.IServo>();

                if (ArmNDIkServos == null)
                {
                    ArmNDIkServos = new List<IKservo>();
                    Debug.Log(string.Format("[IKRC] - GetArmServos() - construct ArmNDIkServos"));
                }
                Debug.Log(string.Format("[IKRC] {0} - Inited ArmNDIkServos ServoLists", 21));

                if (ArmRDIkServos == null)
                {
                    ArmRDIkServos = new List<IKservo>();
                    Debug.Log(string.Format("[IKRC] - GetArmServos() - construct ArmRDIkServos"));
                }
                Debug.Log(string.Format("[IKRC] {0} - Inited ArmRDIkServos ServoLists", 21));

                GetAllChildServo();
                Debug.Log(string.Format("[IKRC] {0} - Get all child servo ({1})", 22, allServos.Count));

                if (AllIkServo.Count >= (JointList.Length - 1))
                {
                    Debug.Log(string.Format("[IKRC] {0} - AllIkServo ({1})", 23, AllIkServo.Count));

                    SortIKservos();

                    // Get last part
                    if (listOfChildrenPart == null)
                        listOfChildrenPart = new List<Part>();
                    GetChildPartRecursive(part);
                    int elementCount = listOfChildrenPart.Count();
                    lastPart = listOfChildrenPart.Find(e => e.name == JointList[JointList.Length - 1]);
                    lastPartRotation = lastPart.transform.rotation;

                    Debug.Log(string.Format("[IKRC] {0} - lastPart " + lastPart.name, 40));

                    // Copy ArmNDIkServos to ArmRDIkServos
                    foreach (IKservo element in ArmNDIkServos)
                    {
                        IKservo clone = element.Clone();
                        ArmRDIkServos.Add(clone);
                    }
                    // make in list reverse sequence
                    ArmRDIkServos.Reverse();

                    // Add EndEffector/LEE/ (last part) to SortIkServo - problem -> if LEE hold other element that will last part
                    ArmNDIkServos.Add(new IKservo(lastPart));
                    Debug.Log(string.Format("[IKRC] {0} - END - ArmNDIkServos.Add(new IKservo(lastPart)", 41));
                    Debug.Log(string.Format("[IKRC] {0} - END - ArmNDIkServos.Count = {1}", 41, ArmNDIkServos.Count));

                    ArmRDIkServos.Add(new IKservo(part));
                    Debug.Log(string.Format("[IKRC] {0} - END - ArmRDIkServos.Add(new IKservo(part)", 41));
                    Debug.Log(string.Format("[IKRC] {0} - END - ArmRDIkServos.Count = {1}", 41, ArmRDIkServos.Count));

                    // Init angle limit of servos
                    int i = 0;
                    if (minMaxAnglesList.Count == ArmNDIkServos.Count)
                        foreach (IKservo element in ArmNDIkServos)
                        {
                            element.MaxAngle = minMaxAnglesList[i].Max;
                            element.MinAngle = minMaxAnglesList[i].Min;
                            i++;
                        }
                    else
                        Debug.Log(string.Format("[IKRC] - GetArmServos() - minMaxAnglesList.Count != ArmNDIkServos.Count"));

                    foreach (IKservo element in ArmNDIkServos)
                    {
                        Debug.Log(string.Format("[IKRC] - GetArmServos() - ArmNDIkServos.MinAngle = {0} ~.MaxAngle = {1}", element.MinAngle, element.MaxAngle));
                    }

                    i = 0;
                    if (minMaxAnglesList.Count == ArmRDIkServos.Count)
                        foreach (IKservo element in ArmRDIkServos)
                        {
                            element.MaxAngle = minMaxAnglesList[i].Max;
                            element.MinAngle = minMaxAnglesList[i].Min;
                            i++;
                        }
                    else
                        Debug.Log(string.Format("[IKRC] - GetArmServos() - minMaxAnglesList.Count != ArmRDIkServos.Count"));

                    foreach (IKservo element in ArmRDIkServos)
                    {
                        Debug.Log(string.Format("[IKRC] - GetArmServos() - ArmRDIkServos.MinAngle = {0} ~.MaxAngle = {1}", element.MinAngle, element.MaxAngle));
                    }
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] - GetArmServos() - AllIkServo.Count <= (JointList.Length - 1)"));
                    return false;
                }
            }
            else
                return false;



            Debug.Log(string.Format("[IKRC]  GetArmServos() - END"));
            return true;
        }

        private bool GetIndepServos()
        {
            Debug.Log(string.Format("[IKRC]  GetIndepServos() - START"));
            if (IRminWrapper.APIReady)
            {
                Debug.Log(string.Format("[IKRC] {0} - IRWrapper.APIReady", 21));
                if (indepServo == null)
                {
                    indepServo = new List<IKservo>();
                    Debug.Log(string.Format("[IKRC] - indepServo == null"));
                }

                // Scanning all servos
                foreach (IRminWrapper.IServo iservo in allServos)
                {
                    foreach (IndepJoint element in IndependentJointList)
                    {
                        //Debug.Log(string.Format("[IKRC] - iservo.HostPart.name = {0}", iservo.HostPart.name));
                        //Debug.Log(string.Format("[IKRC] - element.name = {0}", element.name));
                        //Debug.Log(string.Format("[IKRC] - iservo.HostPart.persistentId = {0}", iservo.HostPart.persistentId.ToString()));
                        //Debug.Log(string.Format("[IKRC] - element.persistentId = {0}", element.persistentId.ToString()));
                        if (iservo.HostPart.name == element.name && iservo.HostPart.persistentId == element.persistentId)
                        {
                            indepServo.Add(new IKservo(element.part, iservo));
                            Debug.Log(string.Format("[IKRC] {0} - indepServo.Add( " + element.name + " )", 22));
                        }
                    }
                }
            }
            Debug.Log(string.Format("[IKRC]  GetIndepServos() - END"));

            if (indepServo.Count != 0)
            {
                return true;
            }
            return false;
        }

        private void GetAllChildServo()
        {
            // Get all servo
            foreach (IRminWrapper.IControlGroup group in IRminWrapper.IRController.ServoGroups)
            {
                allServos.AddRange(group.Servos);
            }

            foreach (IRminWrapper.IServo element in allServos)
            {
                Debug.Log(string.Format("[IKRC] {0} - allServos.Add( " + element.HostPart.name + " )", 22));
            }

            // Scanning all servos
            foreach (IRminWrapper.IServo iservo in allServos)
            {
                //Debug.Log(string.Format("[IKRC] {0} - iservo.HostPart.name( " + iservo.HostPart.name + " )", 22));
                List<Part> partList = new List<Part>();
                foreach (Part element in part.FindChildParts<Part>(true))
                    partList.Add(element);
                // if main element of robot arm is servo then add to list
                partList.Add(part);
                foreach (Part element in partList)
                {
                    //Debug.Log(string.Format("[IKRC] {0} - FindChildParts<Part>( " + element.name + " )", 22));
                    // if childparts contain servo and not hinge then add to IKservos
                    if ((iservo.HostPart.name == element.name) && !iservo.HostPart.name.Contains("Hinge"))
                    {
                        AllIkServo.Add(new IKservo(element, iservo));
                        Debug.Log(string.Format("[IKRC] {0} - AllIkServo.Add( " + element.name + " )", 22));
                    }
                }
            }

            Debug.Log(string.Format("[IKRC] {0} - END GetAllChildServo()", 22));
        }

        private void DebugArmIkServos(List<IKservo> Servos)
        {
            int servonumber = 0;
            foreach (IKservo element in Servos)
            {
                Debug.Log(string.Format("[IKRC] DAIS - Servos[{1}].part.name = {0}", element.part.name, servonumber));
                Debug.Log(string.Format("[IKRC] DAIS - Servos[{1}].fkParams.PartRotation = {0}", QuaternionToString(element.fkParams.PartRotation, "0.00"), servonumber));
                Debug.Log(string.Format("[IKRC] DAIS - Servos[{1}].fkParams.ParentOffset = {0}", VectorToString(element.fkParams.ParentOffset, "0.00"), servonumber));
                Debug.Log(string.Format("[IKRC] DAIS - Servos[{1}].fkParams.Rotation = {0}", QuaternionToString(element.fkParams.Rotation, "0.00"), servonumber));
                Debug.Log(string.Format("[IKRC] DAIS - Servos[{1}].fkParams.Axis = {0}", VectorToString(element.fkParams.Axis, "0.00"), servonumber));

                servonumber++;
            }
        }

        private void SortIKservos()
        {
            for (int i = 0; i < ((JointList[JointList.Length - 2] == JointList[JointList.Length - 1]) ? (JointList.Length - 1) : JointList.Length); i++)
            {
                foreach (IKservo ikServo in AllIkServo)
                {
                    //ikServo.part.partName
                    if (ikServo.part.name == JointList[i])
                    {
                        ArmNDIkServos.Add(ikServo);
                        Debug.Log(string.Format("[IKRC] {0} - ikServo " + ikServo.part.name, 24));
                    }
                }
            }

            Debug.Log(string.Format("[IKRC] {0} - END SortIKservos()", 24));
        }

        private bool DumpServoStructure(List<IKservo> IkServos, Part basePart, IkDirection ikDirection)
        {
            // 
            Vector3 StartPos = basePart.transform.position;
            // 
            Quaternion StartRot = basePart.transform.rotation;
            int i = 0;

            // dumping servo structure
            Debug.Log(string.Format("[IKRC] DumpServoStructure() IkServos.Count = {0}", IkServos.Count));

            // Calculate parameters of servos
            foreach (IKservo ikServo in IkServos)
            {
                // Calculate parameters from 0. servo to last-1. servo
                if (i < (IkServos.Count - 1))
                {
                    // transform of servo
                    ikServo.ServoTransform = ikServo.part.transform;
                    // orientation of main part
                    ikServo.fkParams.PartRotation = basePart.transform.rotation;
                    // position offset for parentpart
                    ikServo.fkParams.ParentOffset = Quaternion.Inverse(StartRot) * (ikServo.ServoTransform.position - StartPos);
                    // axis of servo
                    ikServo.fkParams.Axis = basePart.transform.rotation * (ikDirection == IkDirection.NORMAL ? JointsRealAxis[i] : -1f * JointsRealAxis[IkServos.Count - 2 - i]);
                    // global rotation of servo
                    ikServo.fkParams.Rotation = ikServo.ServoTransform.rotation;
                    // Parent's global position of servo
                    StartPos = ikServo.ServoTransform.position;
                    // Parent's global rotation of servo
                    StartRot = ikServo.fkParams.Rotation;
                }
                // Calculate parameters of last. servo
                else
                {
                    foreach (Transform element in ikServo.part.GetComponentsInChildren<Transform>())
                    {
                        if (element.name == "dockingNode")
                        {
                            // transform of servo
                            ikServo.ServoTransform = element.transform;
                            // orientation of main part
                            ikServo.fkParams.PartRotation = basePart.transform.rotation;
                            // position offset for parentpart
                            ikServo.fkParams.ParentOffset = Quaternion.Inverse(StartRot) * (element.transform.position - StartPos);
                            // axis of servo
                            //ikServo.fkParams.Axis = basePart.transform.rotation * (ikDirection == IkDirection.NORMAL ? JointsRealAxis[i] : JointsRealAxis[IkServos.Count - 2 - i]);
                            ikServo.fkParams.Axis = basePart.transform.rotation * (JointsRealAxis[i]);
                            // global rotation of servo
                            ikServo.fkParams.Rotation = element.transform.rotation;
                            // Parent's global position of servo
                            StartPos = element.transform.position;
                            // Parent's global rotation of servo
                            StartRot = ikServo.fkParams.Rotation;

                            break;
                        }
                    }
                }

                Debug.Log(string.Format("[IKRC] {0} - [{1}]ikServo " + ikServo.part.name, 40, i));
                Debug.Log(string.Format("[IKRC] {0} - ParentOffset " + VectorToString(ikServo.fkParams.ParentOffset, "0.00"), 42));
                Debug.Log(string.Format("[IKRC] {0} - Axis " + VectorToString(ikServo.fkParams.Axis, "0.0"), 40));
                Debug.Log(string.Format("[IKRC] {0} - Rotation " + QuaternionToString(ikServo.fkParams.Rotation, "0.00"), 42));

                i++;
            }

            Debug.Log(string.Format("[IKRC] {0} - END DumpServoStructure()", 42));

            Debug.Log(string.Format("[IKRC] {0} - basePart.transform.rotation " + QuaternionToString(basePart.transform.rotation, "0.00"), 42));

            return true;
        }

        //public bool CreateReverseFkParams(List<IKservo> ArmNDIkServos, List<IKservo> ArmRDIkServos)
        //{
        //    bool result = false;

        //    DebugArmIkServos(ArmNDIkServos);
        //    DebugArmIkServos(ArmRDIkServos);

        //    if (ArmNDIkServos != null && ArmRDIkServos != null)
        //    {
        //        for (int i = 0; i < ArmNDIkServos.Count - 1; i++)
        //        {
        //            ArmRDIkServos[ArmRDIkServos.Count - 2 - i].fkParams.PartRotation = Quaternion.Inverse(ArmNDIkServos[i].fkParams.PartRotation);
        //            ArmNDIkServos[ArmRDIkServos.Count - 2 - i].fkParams.PartRotation.w = ArmNDIkServos[i].fkParams.PartRotation.w * -1f;
        //            ArmRDIkServos[ArmRDIkServos.Count - 2 - i].fkParams.Rotation = ArmNDIkServos[i].fkParams.Rotation;
        //            ArmRDIkServos[ArmRDIkServos.Count - 2 - i].fkParams.Axis = -1f * ArmNDIkServos[i].fkParams.Axis;
        //            ArmRDIkServos[i].fkParams.ParentOffset = ArmNDIkServos[i].fkParams.ParentOffset;
        //        }

        //        ArmRDIkServos[3].fkParams.ParentOffset.y = -1f * ArmNDIkServos[5].fkParams.ParentOffset.y;

        //        result = true;
        //    }

        //    DebugArmIkServos(ArmNDIkServos);
        //    DebugArmIkServos(ArmRDIkServos);
        //    return result;
        //}

        public void ReverseIkDirection_OnValueChanged()
        {
            int i;

            if (ReverseIkDirection && !prevReverseIkDirection)
            {
                Debug.Log(string.Format("[IKRC] ReverseIkDirection && !prevReverseIkDirection START"));

                prevReverseIkDirection = ReverseIkDirection;

                //// change angles
                //i = 0;
                //foreach (float element in theta)
                //{
                //    Debug.Log(string.Format("[IKRC] BEFORE - Theta[{0}] = {1}", i, element.ToString("0.00")));
                //    i++;
                //}

                theta = FloatArrayReverse(theta, 0, theta.Length - 1);

                //i = 0;
                //foreach (float element in theta)
                //{
                //    Debug.Log(string.Format("[IKRC] AFTER - Theta[{0}] = {1}", i, element.ToString("0.00")));
                //    i++;
                //}

                //// change servos
                //i = 0;
                //foreach (IKservo element in ArmIkServos)
                //{
                //    Debug.Log(string.Format("[IKRC] BEFORE - ArmIkServos[{0}].part.name = {1}", i, element.part.name));
                //    i++;
                //}

                //Debug.Log(string.Format("[IKRC] BEFORE - EEpos {0}", ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position.ToString()));
                //Debug.Log(string.Format("[IKRC] BEFORE - EErot {0}", ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation.eulerAngles.ToString()));

                //ArmIkServos = ArmRDIkServos;

                //Debug.Log(string.Format("[IKRC] AFTER - EEpos {0}", ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position.ToString()));
                //Debug.Log(string.Format("[IKRC] AFTER - EErot {0}", ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation.eulerAngles.ToString()));

                //i = 0;
                //foreach (IKservo element in ArmIkServos)
                //{
                //    Debug.Log(string.Format("[IKRC] AFTER - ArmIkServos[{0}].part.name = {1}", i, element.part.name));
                //    Debug.Log(string.Format("[IKRC] AFTER - ArmIkServos[{0}].fkParams.Rotation = {1}", i, QuaternionToString(element.fkParams.Rotation, "0.00")));
                //    i++;
                //}

                //Debug.Log(string.Format("[IKRC] ReverseIkDirection && !prevReverseIkDirection - IkBasePart.name = {0}", IkBasePart.name));

                IkBasePart = lastPart;
                ArmIkServos = ArmRDIkServos;

                //Debug.Log(string.Format("[IKRC] ReverseIkDirection && !prevReverseIkDirection - IkBasePart.name = {0}", IkBasePart.name));

                ForwardKinematicsWithLog(theta, ArmIkServos, IkBasePart);

                Debug.Log(string.Format("[IKRC] ReverseIkDirection && !prevReverseIkDirection END"));
            }
            else if (!ReverseIkDirection && prevReverseIkDirection)
            {
                Debug.Log(string.Format("[IKRC] !ReverseIkDirection && prevReverseIkDirection START"));

                prevReverseIkDirection = ReverseIkDirection;

                // change angles
                theta = FloatArrayReverse(theta, 0, theta.Length - 1);

                //// change servos
                //ArmIkServos = ArmNDIkServos;

                ////basePartRotation = part.transform.rotation;
                //basePartRotation = storedPartRotation;
                //basePart = part;

                IkBasePart = part;
                ArmIkServos = ArmNDIkServos;

                ForwardKinematicsWithLog(theta, ArmIkServos, IkBasePart);

                Debug.Log(string.Format("[IKRC] !ReverseIkDirection && prevReverseIkDirection END"));
            }
        }

        #endregion mapping servo structure

        private bool initPartConfigButtonColors()
        {
            return (ColorUtility.TryParseHtmlString(controlButtonUpDownColor, out colorControlButtonUpDown)
                && ColorUtility.TryParseHtmlString(controlButtonLeftRightColor, out colorControlButtonLeftRight)
                && ColorUtility.TryParseHtmlString(controlButtonForwardBackwardColor, out colorControlButtonForwardBackward));
        }

        private bool InitTrajectoryCursor()
        {
            bool result = false;

            result = true;

            return result;
        }

        // Use this for initialization
        public void Start()
        {
            Debug.Log(string.Format("[IKRC] Start of Start() {0}", counterStart));
            counterStart++;

            ridbtList = new List<BoolText>();

            ArmMaxLength = workingRange;
            ArmMaxLengthString = workingRange.ToString();
            MaxPosErrString = MaxPosErr.ToString();
            MaxOriErrString = MaxOriErr.ToString();
            DistanceThresholdString = DistanceThreshold.ToString();
            AngleThresholdString = AngleThreshold.ToString();
            samplingAngleString = SamplingAngle.ToString();

            WindowIsExtended = windowExtended;

            if (robotArmID == "" )
            {
                Debug.Log(string.Format("[IKRC] Start() robotArmID == 'empty'"));
                return;
            }
            else
                Debug.Log(string.Format("[IKRC] Start() robotArmID == '{0}'", robotArmID));

            if (!JointListIsLoaded)
            {
                if (!GetJointListFromPartConfig())
                {
                    Debug.Log(string.Format("[IKRC] Start() - !GetJointListFromPartConfig()"));
                    return;
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] Start() - GetJointListFromPartConfig() = true"));
                    JointListIsLoaded = true;
                }
            }

            JointsRealAxis = new Vector3[JointList.Length];

            if (!GetJointRealAxisFromPartConfig())
                return;

            minMaxAnglesList = new List<MinMaxAngle>();
            GetMinMaxAnglesFromPartConfig();

            // Get independent servo from partconfig
            if (!HasIndependentServos)
            {
                Debug.Log(string.Format("[IKRC] GetIndependentJointListFromPartConfig() START"));
                HasIndependentServos = GetIndependentJointListFromPartConfig();
                Debug.Log(string.Format("[IKRC] GetIndependentJointListFromPartConfig() END"));
            }

            // Get independent persistentId of servo from partconfig
            // Get independent min-max position of servo from partconfig
            independentMinMaxAnglesList = new List<MinMaxAngle>();
            if (HasIndependentServos)
            {
                Debug.Log(string.Format("[IKRC] GetIndependentPersistentIdFromPartConfig() START"));
                HasIndependentServos = GetIndependentPersistentIdFromPartConfig();
                Debug.Log(string.Format("[IKRC] GetIndependentPersistentIdFromPartConfig() END"));
            }
            if (HasIndependentServos)
            {
                Debug.Log(string.Format("[IKRC] GetIndependentMinMaxAnglesFromPartConfig() START"));
                HasIndependentServos = GetIndependentMinMaxAnglesFromPartConfig();
                Debug.Log(string.Format("[IKRC] GetIndependentMinMaxAnglesFromPartConfig() END"));
            }
            if (HasIndependentServos)
            {
                Debug.Log(string.Format("[IKRC] Check IndependentJointList.Count = {0}", IndependentJointList.Count));
                Debug.Log(string.Format("[IKRC] Check IndependentPerId.Length = {0}", IndependentPerId.Length));
                Debug.Log(string.Format("[IKRC] Check independentMinMaxAnglesList.Count = {0}", independentMinMaxAnglesList.Count));
                if (independentMinMaxAnglesList.Count != IndependentJointList.Count || IndependentPerId.Length != IndependentJointList.Count)
                {
                    HasIndependentServos = false;
                }
            }
            Debug.Log(string.Format("[IKRC] Check END - HasIndependentServos = {0}", HasIndependentServos.ToString()));

            theta = new float[JointList.Length];
            currentTheta = new float[JointList.Length];
            blocking = new bool[JointList.Length];
            servoGimbal = new GameObject[JointList.Length];

            buttonIsColored = initPartConfigButtonColors();
            IsInitedModule = false;
            msgWindowRectangle = new Rect(msgWindowPosition.x, msgWindowPosition.y, msgWindowSize.x, msgWindowSize.y);
            cfgWindowRectangle = new Rect(cfgWindowPosition.x, cfgWindowPosition.y, cfgWindowSize.x, cfgWindowSize.y);
            chkWindowRectangle = new Rect(chkWindowPosition.x, chkWindowPosition.y, chkWindowSize.x, chkWindowSize.y);

            if (HighLogic.LoadedSceneIsFlight)
            {
                Debug.Log(string.Format("[IKRC] Start() - HighLogic.LoadedSceneIsFlight = true"));
                if (FKparamsIkServo == null)
                    CreateFKparams();
                if (RdFKparamsIkServo == null && robotArmID == "CA2")
                    CreateRdFKparams();

                WindowID = NextWindowID;
                NextWindowID++;
                MsgWindowID = NextWindowID;
                NextWindowID++;
                CfgWindowID = NextWindowID;
                NextWindowID++;
                ChkWindowID = NextWindowID;
                NextWindowID++;

                if (part == null)
                    Debug.Log(string.Format("[TRF{1}] {0} Start() part == null", 0, WindowID));
                else
                    Debug.Log(string.Format("[TRF{1}] {0} Start() part.name = " + part.name, 0, WindowID));

                diagramRectangle = new Rect(10, 525, 290, 70);
                gradientDiagramRectangle = new Rect(320, 525, 290, 70);

                for (int i = 0; i < (JointList.Length - 1); i++)
                    blocking[i] = false;

                // Init Buttons
                buttons = new Buttons();

                // Set start position and  start orientation of EndEffector
                globalPosition = new Vector3(0f, 0f, 0f);
                globalQuaternion = Quaternion.Euler(0f, 0f, 0f);

                Debug.Log(string.Format("[IKRC] {0} - START - EndDeclaration", 48));
                windowRectangle = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
                iterateData = new List<float>();
                sampleAngleData = new List<float>();
                distanceData = new List<float>();
                errorData = new List<float>();
                data = new List<List<float>>();
                gradientData = new List<List<float>>();
                Debug.Log(string.Format("[IKRC] {0} - END - EndDeclaration", 49));

                Debug.Log(string.Format("[IKRC] {0} end of Start() WindowID: {1}", 400, WindowID));

                // Init IRMinimal API
                if (!InitIRWrapper())
                {
                    Debug.Log(string.Format("[IKRC] >>> >>> Start() - InitIRWrapper() == false"));
                    return;
                }
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - InitIRWrapper() == true"));

                // Check arm contains the servos
                if (!CheckExistArm())
                    return;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - CheckExistArm() == true"));

                // Create IkServo object from servolist
                if (!GetArmServos())
                    return;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - GetArmServos() == true"));
                // ??? Not used!
                ServosIsBaseInited = true;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - ServosIsBaseInited = true"));

                // Mapping the IK parameters of the Normal Direction Servos.
                if (!DumpServoStructure(ArmNDIkServos, part, IkDirection.NORMAL))
                    return;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - DumpServoStructure(ArmNDIkServos) == true"));

                // Mapping the IK parameters of the Reverse Direction Servos.
                if (!DumpServoStructure(ArmRDIkServos, lastPart, IkDirection.REVERSE))
                    return;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - DumpServoStructure(ArmRDIkServos) == true"));

                //DebugArmIkServos(ArmNDIkServos);
                //DebugArmIkServos(ArmRDIkServos);

                // Are parameters loaded in OnLoad()?
                if (!IsLoadedFkParams)
                {
                    Debug.Log(string.Format("[IKRC] >>> >>> Start() - IsLoadedFkParams == false"));
                    //if (!CheckServosZeroState(true))
                    //    return;
                    //Debug.Log(string.Format("[IKRC] >>> >>> Start() - CheckServosZeroState() == true"));

                    IsLoadedFkParams = true;
                    
                    // Save Forward Kinematics parameters to puffer
                    SetFKParamsToBuffer();
                    Debug.Log(string.Format("[IKRC] >>> >>> Start() - SetFKParamsToBuffer() == true"));

                    // Save all parameters to xml file of plugin
                    OnSave(null);
                    Debug.Log(string.Format("[IKRC] >>> >>> Start() - OnSave()"));
                }

                // Load Forward Kinematics parameters from puffer
                if (!GetFKParamsFromBuffer())
                    return;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - GetFKParamsFromBuffer() == true"));

                if (HasIndependentServos)
                {
                    HasIndependentServos = CheckExistIndependentServos();
                    Debug.Log(string.Format("[IKRC] >>> >>> Start() - CheckExistIndependentServos() == {0}", HasIndependentServos.ToString()));
                    if (HasIndependentServos)
                    {
                        HasIndependentServos = GetIndepServos();
                        Debug.Log(string.Format("[IKRC] >>> >>> Start() - GetIndepServos() == {0}", HasIndependentServos.ToString()));
                        if (HasIndependentServos)
                        {
                            iota = new float[indepServo.Count];
                            currentIota = new float[indepServo.Count];
                        }
                    }
                }

                //IkBasePart = part;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - IkBasePart.name = {0}", IkBasePart.name));
                //CreateReverseFkParams(ArmNDIkServos, ArmRDIkServos);
                ArmIkServos = ArmNDIkServos;

                //Debug.Log(string.Format("[IKRC] >>> >>> Start() - list ArmNDIkServos"));
                //DebugArmIkServos(ArmNDIkServos);
                //Debug.Log(string.Format("[IKRC] >>> >>> Start() - list ArmRNDIkServos"));
                //DebugArmIkServos(ArmRDIkServos);

                // Start() rutin end, module inited.
                IsInitedModule = true;
                Debug.Log(string.Format("[IKRC] >>> >>> Start() - IsInitedModule = true"));
            }
            else
                Debug.Log(string.Format("[IKRC] Start() - HighLogic.LoadedSceneIsFlight = false"));
        }

        // Update is called once per frame
        public void Update()
        {
            //1st cycle after module inited
            if (HighLogic.LoadedSceneIsFlight && IsInitedModule && IsInitedModule1st)
                FirstActiveUpdate();

            //default cycle
            if (ircWindowActive && HighLogic.LoadedSceneIsFlight && IsInitedModule)
                DefaultActiveUpdate();
        }

        #region 1st cycle after module inited 
        private void FirstActiveUpdate()
        {
            IsInitedModule1st = false;
            Debug.Log(string.Format("[IKRC] Update() - IsInitedModule1st = false"));

            Debug.Log(string.Format("[IKRC] {0} - Update() - globalPosition&Quaternion", 44));
            // set position of last part for global position
            globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
            globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;
            Debug.Log(string.Format("[IKRC] {0} - Update() - globalPosition&Quaternion", 45));

            baseState.Translation = globalPosition;
            baseState.Rotation = globalQuaternion;
            Debug.Log(string.Format("[IKRC] Update() -  baseState init"));

            // Store base position and base orientation
            basePosition = globalPosition;
            baseQuaternion = globalQuaternion;
            Debug.Log(string.Format("[IKRC] Update() -  (basePosition, baseQuaternion) init"));

            for (int i = 0; i < ArmIkServos.Count; i++)
                servoGimbal[i] = new GameObject();
            Debug.Log(string.Format("[IKRC] Update() -  baseState init"));

            // Get angle of Joints
            for (int j = 0; j < ArmIkServos.Count - 1; j++)
                theta[j] = ArmIkServos[j].iservo.Position;
            Debug.Log(string.Format("[IKRC] Update() -  Get angle of Joints"));

            prevPartPosition = part.transform.position;
            prevPartRotation = part.transform.rotation;
            Debug.Log(string.Format("[IKRC] Update() -  (prevPartPosition, prevPartRotation) init"));

            // cPos - tartget position = current arm position
            globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
            globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;
            Debug.Log(string.Format("[IKRC] Update() -  (globalPosition, globalQuaternion) init"));

            Events["TurnOnIKRCEvent"].active = true;
        }
        #endregion 1st cycle after module inited

        #region default cycle
        private void DefaultActiveUpdate()
        {
            #region set aim object
            // Set Aim Object (PDGFx)
            //Debug.Log(string.Format("[IKRC] Update() - // Set Aim Object (PDGFx)"));
            GameObject mouseObject = CheckForObjectUnderCursor();
            bool modPressed = Input.GetKey(KeyCode.LeftAlt);
            if (modPressed && (mouseObject != null))
            {
                foreach (Transform element in mouseObject.GetComponentsInChildren<Transform>())
                {
                    if (element.name == "dockingNode")
                    {
                        dockingNodeObject = element;
                        hoverObject = mouseObject;
                        Debug.Log(string.Format("[IKRC] {0} - hoverObject " + hoverObject.name, 501));
                        Debug.Log(string.Format("[IKRC] {0} - dockingNodeObject = " + dockingNodeObject.name, 502));
                        break;
                    }
                }
            }
            if (modPressed && (mouseObject == null))
            {
                dockingNodeObject = null;
                hoverObject = null;
            }
            #endregion set aim object

            #region emergency servo stop
            // emergency servo stop
            //Debug.Log(string.Format("[IKRC] Update() - // emergency servo stop"));
            if (emergencyServoStop)
            {
                IK_active = false;
                servoSpeed = 0f;
                globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
                globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;
            }
            #endregion emergency servo stop

            #region set base state
            // set base state
            //Debug.Log(string.Format("[IKRC] Update() - // set base state"));
            if (baseStateBool)
            {
                globalPosition = basePosition;
                globalQuaternion = baseQuaternion;

                for (int i = 0; i < JointList.Length; i++)
                    theta[i] = 0f;

                IK_active = false;

                // implement angle of joints
                if (!NormalizedServoRotation)
                    Bug = !ImplementServoRotation(ArmIkServos, theta, servoSpeed);
                else
                    Bug = !ImplementNormalServoRotation(ArmIkServos, currentTheta, theta, servoSpeed);

                if (HasIndependentServos)
                {
                    for (int i = 0; i < indepServo.Count; i++)
                        iota[i] = 0f;
                    Bug = !ImplementIndepServoRotation(indepServo, iota, servoSpeed);
                }

            }
            #endregion set base state

            #region actual state
            // set actual state
            //Debug.Log(string.Format("[IKRC] Update() - // set actual state"));
            if (actualStateBool)
            {
                FKvector = ForwardKinematics(theta, ArmIkServos, IkBasePart);
                globalPosition = FKvector.Translation;
                globalQuaternion = FKvector.Rotation;

                IK_active = false;
            }
            #endregion actual state

            //Debug.Log(string.Format("[IKRC] Update() - #region PdgfAimObject"));
            #region PdgfAimObject
            // set pdgf state - set aim object -> deactivate IK function
            if (pdgfStateBool && (dockingNodeObject != null))
            {
                if (!nearPDGFBool)
                {
                    //globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, -90f) * (IsForceRoll ? Quaternion.Euler(0f, 0f, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, 180f) * (IsForceRoll ? Quaternion.Euler(forceRollNumberX, forceRollNumberY, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    //globalPosition = dockingNodeObject.position + (globalQuaternion * (IsForceDist ? new Vector3(0f, 0f, -1.0f * forceDistNumberZ) : new Vector3(0f, 0f, -0.6f)));
                    globalPosition = dockingNodeObject.position + (globalQuaternion * (IsForceDist ? new Vector3(forceDistNumberX, forceDistNumberY, -1.0f * forceDistNumberZ) : new Vector3(0f, 0f, -0.6f)));
                    if (TargetInRange(ArmMaxLength))
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - !nearPDGFBool - TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                    }
                    else
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - !nearPDGFBool - !TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                        globalPosition = TargetToRange(ArmMaxLength);
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = TargetToRange(ArmMaxLength) = " + VectorToString(globalPosition, "0.00")));
                    }

                    // set transform of TrajectoryCursor
                    if (TrajectoryCursor != null)
                    {
                        TrajectoryCursor.transform.position = globalPosition;
                        TrajectoryCursor.transform.rotation = globalQuaternion;
                    }
                }
                else
                {
                    //globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, -90f) * (IsForceRoll ? Quaternion.Euler(0f, 0f, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, 180f) * (IsForceRoll ? Quaternion.Euler(forceRollNumberX, forceRollNumberY, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    globalPosition = dockingNodeObject.position + (globalQuaternion * new Vector3(0.0f, 0.0f, 0.0f));
                    if (TargetInRange(ArmMaxLength))
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - nearPDGFBool - TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                    }
                    else
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - nearPDGFBool - !TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                        globalPosition = TargetToRange(ArmMaxLength);
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = TargetToRange(ArmMaxLength) = " + VectorToString(globalPosition, "0.00")));
                    }
                }
                IK_active = false;

                //for (int i = 0; i < servoGimbal.Length; i++)
                //{
                //    Debug.Log(string.Format("[IKRC] {0} - Update() - (servoGimbal[{1}]) " + VectorToString(servoGimbal[i].transform.position, "0.00") + " " + VectorToString(servoGimbal[i].transform.rotation.eulerAngles, "0.00"), 599, i));
                //}
            }

            if (refreshPDGFBool && (dockingNodeObject != null))
            {
                //if (!nearPDGFBool)
                //{
                //    globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, -90f) * (IsForceRoll ? Quaternion.Euler(0f, 0f, forceRollNumber) : Quaternion.Euler(0f, 0f, 0f)));
                //    globalPosition = dockingNodeObject.position + (globalQuaternion * (IsForceDist ? new Vector3(0f, 0f, -1.0f * forceDistNumber) : new Vector3(0f, 0f, -0.6f)));
                //}
                //else
                //{
                //    globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, -90f) * (IsForceRoll ? Quaternion.Euler(0f, 0f, forceRollNumber) : Quaternion.Euler(0f, 0f, 0f)));
                //    globalPosition = dockingNodeObject.position + (globalQuaternion * new Vector3(0.0f, 0.0f, 0.0f));
                //}

                if (!nearPDGFBool)
                {
                    //globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, -90f) * (IsForceRoll ? Quaternion.Euler(0f, 0f, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, 180f) * (IsForceRoll ? Quaternion.Euler(forceRollNumberX, forceRollNumberY, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    //globalPosition = dockingNodeObject.position + (globalQuaternion * (IsForceDist ? new Vector3(0f, 0f, -1.0f * forceDistNumberZ) : new Vector3(0f, 0f, -0.6f)));
                    globalPosition = dockingNodeObject.position + (globalQuaternion * (IsForceDist ? new Vector3(forceDistNumberX, forceDistNumberY, -1.0f * forceDistNumberZ) : new Vector3(0f, 0f, -0.6f)));
                    if (TargetInRange(ArmMaxLength))
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - !nearPDGFBool - TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                    }
                    else
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - !nearPDGFBool - !TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                        globalPosition = TargetToRange(ArmMaxLength);
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = TargetToRange(ArmMaxLength) = " + VectorToString(globalPosition, "0.00")));
                    }
                }
                else
                {
                    //globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, -90f) * (IsForceRoll ? Quaternion.Euler(0f, 0f, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    globalQuaternion = dockingNodeObject.rotation * (Quaternion.Euler(180f, 0f, 180f) * (IsForceRoll ? Quaternion.Euler(forceRollNumberX, forceRollNumberY, forceRollNumberZ) : Quaternion.Euler(0f, 0f, 0f)));
                    globalPosition = dockingNodeObject.position + (globalQuaternion * new Vector3(0.0f, 0.0f, 0.0f));
                    if (TargetInRange(ArmMaxLength))
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - nearPDGFBool - TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                    }
                    else
                    {
                        Debug.Log(string.Format("[IKRC] - Update() - PdgfAimObject - nearPDGFBool - !TargetInRange"));
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = " + VectorToString(globalPosition, "0.00")));
                        globalPosition = TargetToRange(ArmMaxLength);
                        Debug.Log(string.Format("[IKRC] - Update() - globalPosition = TargetToRange(ArmMaxLength) = " + VectorToString(globalPosition, "0.00")));
                    }
                }

                //for (int i = 0; i < servoGimbal.Length; i++)
                //{
                //    Debug.Log(string.Format("[IKRC] {0} - Update() - (servoGimbal[{1}]) " + VectorToString(servoGimbal[i].transform.position, "0.00") + " " + VectorToString(servoGimbal[i].transform.rotation.eulerAngles, "0.00"), 599, i));
                //}
            }
            #endregion PdgfAimObject

            #region set targetball than aim
            if (pdgfStateBool && moveTarget)
            {
                globalQuaternion = global.transform.rotation;
                globalPosition = global.transform.position;
                IK_active = false;
            }
            #endregion set targetball than aim

            //Debug.Log(string.Format("[IKRC] Update() - #region VirtualEndEffector"));
            #region VirtualEndEffector
            // set selected part than virtual endeffector
            if (veeStateBool && (dockingNodeObject != null) && !VeeIsActive)
            {
                Debug.Log(string.Format("[IKRC] {0} - Update() - set selected part than virtual endeffector", 600));
                IK_active = false;
                VeeIsActive = true;

                IKservo eefIKservo = ArmIkServos[ArmIkServos.Count - 1];
                IKservo prevIKservo = ArmIkServos[ArmIkServos.Count - 2];

                // save original LEE IKservo parameters
                orgLEEikServo = eefIKservo.Clone();
                Debug.Log(string.Format("[IKRC] {0} - Update() - save original LEE IKservo parameters", 600));
                //Debug.Log(string.Format("[IKRC] {0} - Update() - (SortIkServo[SortIkServo.Count - 1]) " + IKServoToString(SortIkServo[SortIkServo.Count - 1]), 600));
                Debug.Log(string.Format("[IKRC] {0} - Update() - (eefIKservo) " + IKServoToString(eefIKservo), 600));
                Debug.Log(string.Format("[IKRC] {0} - Update() - (orgLEEikServo) " + IKServoToString(orgLEEikServo), 600));

                // change FKParams of LEE (last servo object)

                Part partOfHoverObject = hoverObject.GetComponentInParent<Part>() as Part;
                eefIKservo.part = partOfHoverObject;
                eefIKservo.name = partOfHoverObject.name;
                Debug.Log(string.Format("[IKRC] {0} - Update() - partOfHoverObject.name = " + partOfHoverObject.name, 600));
                //eefIKservo.iservo = 
                eefIKservo.MinAngle = 0f;
                eefIKservo.MaxAngle = 0f;

                Debug.Log(string.Format("[IKRC] {0} - Update() - searching for dockingNode Transform", 600));
                foreach (Transform element in partOfHoverObject.GetComponentsInChildren<Transform>())
                {
                    Debug.Log(string.Format("[IKRC] {0} - Update() - element.name =" + element.name, 600));
                    if (element.name == "dockingNode")
                    {
                        // transform of servo
                        eefIKservo.ServoTransform = element.transform;
                        // orientation of main part
                        eefIKservo.fkParams.PartRotation = part.transform.rotation;
                        // position offset for parentpart
                        eefIKservo.fkParams.ParentOffset = Quaternion.Inverse(prevIKservo.part.transform.rotation) * (element.transform.position - prevIKservo.part.transform.position);
                        // axis of servo
                        eefIKservo.fkParams.Axis = part.transform.rotation * new Vector3(0, 0, 0);
                        // global rotation of servo
                        Quaternion basePartRotationDiff = IkBasePart.transform.rotation * Quaternion.Inverse(ArmIkServos[0].fkParams.Rotation);
                        for (int i = 0; i < (ArmIkServos.Count - 1); i++)
                            basePartRotationDiff *= Quaternion.AngleAxis(theta[i], ArmIkServos[i].fkParams.Axis);
                        eefIKservo.fkParams.Rotation = Quaternion.Inverse(basePartRotationDiff) * element.transform.rotation;
                        Debug.Log(string.Format("[IKRC] {0} - Update() - set new FKParams values", 600));
                        break;
                    }
                }

                // initialize actual position and rotation
                Debug.Log(string.Format("[IKRC] {0} - Update() - initialize actual position and rotation", 600));
                globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
                globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;
            }

            if (veeStateBool && (dockingNodeObject == null) && VeeIsActive)
            {
                Debug.Log(string.Format("[IKRC] {0} - Update() - reset original LEE than virtual endeffector", 600));
                // restore original LEE IKservo parameters
                Debug.Log(string.Format("[IKRC] {0} - Update() - (SortIkServo[SortIkServo.Count - 1]) " + IKServoToString(ArmIkServos[ArmIkServos.Count - 1]), 601));
                ArmIkServos[ArmIkServos.Count - 1].Copy(orgLEEikServo);
                Debug.Log(string.Format("[IKRC] {0} - Update() - (SortIkServo[SortIkServo.Count - 1]) " + IKServoToString(ArmIkServos[ArmIkServos.Count - 1]), 601));
                IK_active = false;
                VeeIsActive = false;
                Debug.Log(string.Format("[IKRC] {0} - Update() - VeeIsActive = false", 601));

                // initialize actual position and rotation
                globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
                globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;
            }
            #endregion VirtualEndEffector

            //Debug.Log(string.Format("[IKRC] Update() - #region RobotArm MovingButtons"));
            #region RobotArm MovingButtons
            // detect button activity
            IKButton_active = !ButtonsIsReleased();

            if (!TrajectoryEditorMode)
            {
                if (!moveTarget)
                {
                    // Kiterjesztett endeffektor esetén ez a funkció tiltott kell, hogy legyen !!! ???
                    // if rotate only servo of LEE then not IK only rotate servo to direct
                    if ((buttons.Rotation == Quaternion.Euler(0f, 0f, 0.5f * rotStep) || buttons.Rotation == Quaternion.Euler(0f, 0f, -0.5f * rotStep)) && robotArmID == "CA2")
                    {
                        theta[ArmIkServos.Count - 2] += (360f - buttons.Rotation.eulerAngles.z) > 180f ? buttons.Rotation.eulerAngles.z : (buttons.Rotation.eulerAngles.z - 360f);
                        // implement angle of joints
                        if (!NormalizedServoRotation)
                            Bug = !ImplementServoRotation(ArmIkServos, theta, servoSpeed);
                        else
                            Bug = !ImplementNormalServoRotation(ArmIkServos, currentTheta, theta, servoSpeed);
                    }
                    // calculate global position of aim from buttons command
                    // global orientation
                    globalQuaternion = globalQuaternion * buttons.Rotation;
                    // global position
                    globalPosition = globalPosition + globalQuaternion * buttons.Translation;
                }
                else
                {
                    global.transform.rotation = global.transform.rotation * buttons.Rotation;
                    global.transform.position = global.transform.position + globalQuaternion * buttons.Translation;
                }
            }
            // trajectory Editor
            else
            {
                // TrajectoryCursor orientation
                TrajectoryCursor.transform.rotation = TrajectoryCursor.transform.rotation * buttons.Rotation;
                // TrajectoryCursor position
                TrajectoryCursor.transform.position = TrajectoryCursor.transform.position + TrajectoryCursor.transform.rotation * buttons.Translation;

                if (AddTrajectory)
                {
                    bool add = true;
                    foreach (GameObject element in trajectoryList)
                    {
                        if (element.transform == TrajectoryCursor.transform)
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add)
                    {
                        GameObject trajectoryPoint = new GameObject();
                        trajectoryPoint.transform.position = TrajectoryCursor.transform.position;
                        trajectoryPoint.transform.rotation = TrajectoryCursor.transform.rotation;
                        trajectoryList.Add(trajectoryPoint);
                    }
                }

                if (RemoveTrajectory)
                {
                    if (trajectoryList.Count != 0)
                        trajectoryList.RemoveAt(trajectoryList.Count - 1);
                }

                if (ClearTrajectory)
                {
                    ClearTrajectory = false;

                    if (trajectoryList.Count != 0)
                        trajectoryList.Clear();
                }
            }

            // clear buttons.Translation & .Rotation
            ButtonsReleased();
            #endregion RobotArm MovingButtons

            #region Straight Line Trajectory
            if (SLTactive && IK_active)
            {
                // Init Staight Line Trajectory
                if (SLT1stCycle)
                {
                    //Debug.Log(string.Format("[IKRC] Update() - STL1stCycle"));
                    SLT1stCycle = false;
                    SLTposition = 1;
                    // save actual endeffector target position
                    saveGlobalPosition = globalPosition;
                    if (!SLTBreakingActive)
                        SLTBreaking = false;
                    else
                        SLTBreaking = true;
                    // actual pos & orient from FK(servos)
                    SLTactualState = ForwardKinematics(theta, ArmIkServos, IkBasePart);
                    // different of target positon and actual position
                    SLTdistance = Vector3.Distance(saveGlobalPosition, SLTactualState.Translation);
                    //Debug.Log(string.Format("[IKRC] Update() -STLdistance: {0}", STLdistance));
                    // divide the difference of positions 0.5 length pieces
                    //SLTdivider = (float)Mathf.RoundToInt(SLTdistance / 0.5f);
                    SLTdivider = (float)Mathf.RoundToInt(SLTdistance / stlBit);
                    //Debug.Log(string.Format("[IKRC] Update() - STLdivider: {0}", STLdivider));
                    SLTelementDistance = SLTdistance / SLTdivider;
                    //Debug.Log(string.Format("[IKRC] Update() - STLelementDistance: {0}", STLelementDistance));
                    // 1st target position of SLT
                    globalPosition = Vector3.Lerp(SLTactualState.Translation, saveGlobalPosition, (SLTelementDistance * (float)SLTposition) / SLTdistance);
                    //Debug.Log(string.Format("[IKRC] Update() - (STLelementDistance * (float)STLposition) / STLdistance): {0}", (STLelementDistance * (float)STLposition) / STLdistance));
                    Success = false;
                }
                // active SLT function
                else if (Success && (SLTposition < SLTdivider))
                {
                    //Debug.Log(string.Format("[IKRC] Update() - IKsuccess && (STLposition < STLdivider)"));
                    Success = false;
                    SLTposition++;
                    if (SLTposition >= SLTdivider)
                        SLTBreaking = true;

                    globalPosition = Vector3.Lerp(SLTactualState.Translation, saveGlobalPosition, (SLTelementDistance * (float)SLTposition) / SLTdistance);
                    //Debug.Log(string.Format("[IKRC] Update() - STLposition: {0}", STLposition));
                    //Debug.Log(string.Format("[IKRC] Update() - (STLelementDistance * (float)STLposition) / STLdistance): {0}", (STLelementDistance * (float)STLposition) / STLdistance));
                }
            }
            else
            {
                SLT1stCycle = true;
                SLTBreaking = true;

                // Trajectory Control
                if (TrajectoryControlMode && IK_active && trajectoryList.Count != 0)
                {
                    globalPosition = trajectoryList[trajectoryList.Count - 1].transform.position;
                    globalQuaternion = trajectoryList[trajectoryList.Count - 1].transform.rotation;
                    if (Success && ((TrajectoryStepMode && StepTrajectory) || !TrajectoryStepMode))
                    {
                        trajectoryList.RemoveAt(trajectoryList.Count - 1);
                        if (trajectoryList.Count != 0)
                        {
                            globalPosition = trajectoryList[trajectoryList.Count - 1].transform.position;
                            globalQuaternion = trajectoryList[trajectoryList.Count - 1].transform.rotation;
                        }
                    }
                }
            }
            #endregion Straight Line Trajectory

            if (TrajectoryControlMode || TrajectoryEditorMode)
                HasTrajectoryCursor = true;
            else
                HasTrajectoryCursor = false;

            if (!TrajectoryEditorMode)
            {
                #region Active Inverse Kinematics
                if (IK_active || IKButton_active)
                {
                    //Debug.Log(string.Format("[IKRC] Update() - #region Active Inverse Kinematics"));
                    // calculate position- and orientation different
                    distance = Vector3.Distance(ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position, globalPosition);
                    angle = Quaternion.Angle(Quaternion.Euler(ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation.eulerAngles), globalQuaternion);

                    //Debug.Log(string.Format("[IKRC] Update() - distance = {0} ; {1}", distance, DistanceThreshold));
                    //Debug.Log(string.Format("[IKRC] Update() - angle = {0} ; {1}", angle, AngleThreshold));

                    // if differents over the threshold then calculate IK
                    #region endeffector out of tresholds
                    if (distance > DistanceThreshold || angle > AngleThreshold)
                    //if (distance > DistanceThreshold * 10f || angle > AngleThreshold * 10f)
                    {
                        //Debug.Log(string.Format("[IKRC] Update() - Success = false"));
                        Success = false;

                        // start iterate from zero thetas
                        if (zeroThetaIterate && IKsuccess)
                        {
                            for (int i = 0; i < JointList.Length; i++)
                                theta[i] = 0f;
                        }
                        // clear thetas
                        if (clearThetasBool)
                            for (int i = 0; i < JointList.Length; i++)
                                theta[i] = 0f;

                        // iterate angle of joints
                        int j;
                        for (j = 0; j < iterateNumber; j++)
                        {
                            IKsuccess = InverseKinematics(globalPosition, globalQuaternion, theta);

                            // drawing diagram
                            errorData.Add(Distance / 2f * (gradientDiagramRectangle.height - 30f));
                            if (errorData.Count > (int)(gradientDiagramRectangle.width - 10f))
                                errorData.RemoveAt(0);

                            if (IKsuccess)
                            {
                                break;
                            }
                        }

                        // drawing diagram
                        iterateData.Add((float)j / (float)iterateNumber * (diagramRectangle.height - 20f));
                        if (iterateData.Count > (int)(diagramRectangle.width - 10f))
                            iterateData.RemoveAt(0);

                        #region dinamic SamplingAngle
                        if (!nearPDGFBool)
                        {
                            if (j == iterateNumber)
                            {
                                failedIterateCycle++;
                                if (dynamicSamplingAngle)
                                {
                                    SamplingAngle = 0.005f;
                                }
                            }
                            else
                            {
                                failedIterateCycle = 0;
                                if (j > (iterateNumber / 2))
                                {
                                    if (dynamicSamplingAngle)
                                    {
                                        SamplingAngle = 0.01f;
                                    }
                                }
                                else
                                {
                                    if (dynamicSamplingAngle)
                                    {
                                        SamplingAngle = 0.02f;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (dynamicSamplingAngle)
                            {
                                SamplingAngle = 0.005f;
                            }
                            servoSpeed = 0.031f;
                        }
                        #endregion dinamic SamplingAngle

                        samplingAngleString = SamplingAngle.ToString();

                        // drawing diagram
                        sampleAngleData.Add(SamplingAngle / 0.02f * (diagramRectangle.height - 30f));
                        if (sampleAngleData.Count > (int)(diagramRectangle.width - 10f))
                            sampleAngleData.RemoveAt(0);

                        // drawing diagram
                        distanceData.Add(distance / 2f * (diagramRectangle.height - 30f));
                        if (distanceData.Count > (int)(diagramRectangle.width - 10f))
                            distanceData.RemoveAt(0);

                        if ((failedIterateCycle > iterateThreshold) && IK_active)
                        {
                            failedIterateCycle = 0;

                            // ??? mi a hiba a manuális mozgatásnál ???
                            for (int i = 0; i < JointList.Length; i++)
                                theta[i] = 0f;
                        }

                        //if (manualMoveReference && IKButton_active)
                        //{
                        //    FKvector = ForwardKinematics(theta, SortIkServo);
                        //    globalPosition = FKvector.Translation;
                        //    globalQuaternion = FKvector.Rotation;
                        //}

                        // implement angle of joints
                        if (!NormalizedServoRotation)
                            Bug = !ImplementServoRotation(ArmIkServos, theta, servoSpeed);
                        else
                            Bug = !ImplementNormalServoRotation(ArmIkServos, currentTheta, theta, servoSpeed);
                    }
                    #endregion endeffector out of tresholds
                    #region endeffector in tresholds
                    else
                    {
                        //Debug.Log(string.Format("[IKRC] Update() - Success = true"));
                        Success = true;
                        // if IK success then turn off IK activity or switchable ???
                        if (!refreshPDGFBool && !IKButton_active && !SLTactive && !TrajectoryControlMode)
                            IK_active = false;
                    }
                    #endregion endeffector in tresholds
                    //Debug.Log(string.Format("[IKRC] {0} - Update() END ", 107));
                }
                #endregion Active Inverse Kinematics
                #region Inactive Inverse Kinematics
                else
                {
                    if (!TrajectoryEditorMode && !moveTarget)
                    {
                        //Debug.Log(string.Format("[IKRC] Update() - #region Inactive Inverse Kinematics"));
                        // clear buttons.Translation & .Rotation
                        ButtonsReleased();

                        // Change direction of Arm Inverse Kinematics if arm is Canadarm2
                        if (robotArmID == "CA2")
                            ReverseIkDirection_OnValueChanged();
                        else
                            ReverseIkDirection = false;

                        //Debug.Log(string.Format("[IKRC] {0} - ButtonsReleased()", 100));
                        FKvector = ForwardKinematics(theta, ArmIkServos, IkBasePart);
                        //Debug.Log(string.Format("[IKRC] {0} - ForwardKinematics(..)", 101));

                        // implement angle of joints
                        if (!NormalizedServoRotation)
                            Bug = !ImplementServoRotation(ArmIkServos, theta, servoSpeed);
                        else
                            Bug = !ImplementNormalServoRotation(ArmIkServos, currentTheta, theta, servoSpeed);

                        if (HasIndependentServos)
                            Bug = !ImplementIndepServoRotation(indepServo, iota, servoSpeed);

                        //for (int i = 0; i < (JointList.Length - 1); i++)
                        //{
                        //    if (blocking[i])
                        //    {
                        //        SortIkServo[i].MaxAngle = 0f;
                        //        SortIkServo[i].MinAngle = 0f;
                        //    }
                        //    else
                        //    {
                        //        SortIkServo[i].MaxAngle = 270f;
                        //        SortIkServo[i].MinAngle = -270f;
                        //    }
                        //}
                    }
                }
                #endregion Inactive Inverse Kinematics
                //Debug.Log(string.Format("[IKRC] Update() - #region Refresh LineDiagram Data"));
            }

            #region Refresh LineDiagram Data
            // Refresh data List of LineDiagram
            if (data != null)
            {
                data.Clear();
                data.Add(iterateData);
                data.Add(sampleAngleData);
                data.Add(distanceData);
            }

            if (gradientData != null)
            {
                gradientData.Clear();
                gradientData.Add(errorData);
            }
            #endregion Refresh LineDiagram Data

            if (!TrajectoryEditorMode && !moveTarget)
            {
                global.transform.position = globalPosition;
                global.transform.rotation = globalQuaternion;
            }

            currentTheta = GetActualServoRotation(ArmIkServos);
            if (HasIndependentServos)
                currentIota = GetActualIndepServoRotation(indepServo);
            workingDistance = ArmWorkingDistance();

            if (prevReverseIkDirection != ReverseIkDirection)
            {
                ReverseIkDirection = false;
                prevReverseIkDirection = false;
            }
        }
        #endregion default cycle

        #region get part transform by mouse
        private GameObject CheckForObjectUnderCursor()
        {
            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //    return null;
            //}


            //1000000000000000000101

            if (mode == Mode.UI)
            {
                var pointer = new PointerEventData(EventSystem.current);
                pointer.position = Input.mousePosition;

                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                if (raycastResults.Count == 0)
                {
                    //print("Nothing");
                    return null;
                }
                return raycastResults[0].gameObject;
            }

            if (mode == Mode.PART)
            {
                //if (Mouse.HoveredPart ? Mouse.HoveredPart.gameObject : null != null)
                //{
                //    Debug.Log(string.Format("[IKRC] {0} Mode.PART " + Mouse.HoveredPart.gameObject.ToString(), 410));
                //}
                //else
                //{
                //    Debug.Log(string.Format("[IKRC] {0} Mode.PART null", 410));
                //}

                return Mouse.HoveredPart ? Mouse.HoveredPart.gameObject : null;
            }

            if (mode == Mode.OBJECT)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //int layerMask = ~LayerMask.NameToLayer("UI");
                int layerMask = ~0;

                RaycastHit hit;
                if (!Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    return null;
                }
                return hit.collider.gameObject;
            }

            return null;
        }

        private GameObject GetRootObject(GameObject leaf)
        {
            if (leaf == null)
                return null;

            if (mode == Mode.UI)
            {
                int d = 0;
                while (leaf.transform.parent && !leaf.transform.parent.gameObject.GetComponent<Canvas>() && d < limitDepth)
                {
                    leaf = leaf.transform.parent.gameObject;
                    d++;
                }
                return leaf;
            }

            if (mode == Mode.PART)
            {
                return leaf;
            }

            if (mode == Mode.OBJECT)
            {
                int d = 0;
                while (leaf.transform.parent && d < limitDepth)
                {
                    leaf = leaf.transform.parent.gameObject;
                    d++;
                }
                return leaf;
            }

            return null;
        }
        #endregion get part transform by mouse

        #region KSPEvents
        [KSPEvent(guiActive = true, guiName = "Dump ServoStructure", active = true)]
        public void DumpServoStructureEvent()
        {
            // Genarate servo structure and save
            if (FKparamsIkServo == null)
                CreateFKparams();

            if (RdFKparamsIkServo == null && robotArmID== "CA2")
                CreateRdFKparams();

            if (part == null)
                Debug.Log(string.Format("[TRF{1}] {0} START DumpServoStructureEvent() part == null", 0, WindowID));
            else
                Debug.Log(string.Format("[TRF{1}] {0} START DumpServoStructureEvent() part.name = " + part.name, 0, WindowID));

            // check exist arm on part
            if (!CheckExistArm())
            {
                Debug.Log(string.Format("[IKRC] {0} DumpServoStructureEvent() - Don't Exist Arm on PART", 0));
                return;
            }
            else
            {
                Debug.Log(string.Format("[IKRC] {0} DumpServoStructureEvent() - Exist Arm on PART", 0));
            }

            //Debug.Log(string.Format("[IKRC] {0} IRWrapper.InitWrapper() START", 1));
            //IRWrapper.InitWrapper();
            //Debug.Log(string.Format("[IKRC] {0} IRWrapper.InitWrapper() END", 2));

            // If RIMinimal API inited
            if (IRminWrapper.APIReady)
            {
                Debug.Log(string.Format("[IKRC] {0} - DumpServoStructureEvent() - IRMinWrapper.APIReady", 21));
                AllIkServo = new List<IKservo>();
                allServos = new List<IRminWrapper.IServo>();

                ArmNDIkServos = new List<IKservo>();
                Debug.Log(string.Format("[IKRC] {0} - DumpServoStructureEvent() - Inited ArmNDIkServos ServoLists", 21));

                ArmRDIkServos = new List<IKservo>();
                Debug.Log(string.Format("[IKRC] {0} - DumpServoStructureEvent() - Inited ArmRDIkServos ServoLists", 21));

                GetAllChildServo();
                Debug.Log(string.Format("[IKRC] {0} - DumpServoStructureEvent() - Get all child servo ({1})", 22, allServos.Count));

                if (AllIkServo.Count >= (JointList.Length - 1))
                {
                    Debug.Log(string.Format("[IKRC] {0} - DumpServoStructureEvent() - AllIkServo ({1})", 23, AllIkServo.Count));

                    SortIKservos();

                    // Get last part
                    listOfChildrenPart = new List<Part>();
                    GetChildPartRecursive(part);
                    int elementCount = listOfChildrenPart.Count();
                    //lastPart = listOfChildrenPart[elementCount - 1];
                    lastPart = listOfChildrenPart.Find(e => e.name == JointList[JointList.Length - 1]);
                    lastPartRotation = lastPart.transform.rotation;

                    Debug.Log(string.Format("[IKRC] {0} - DumpServoStructureEvent() - lastPart " + lastPart.name, 40));

                    // Copy ArmNDIkServos to ArmRDIkServos
                    foreach (IKservo element in ArmNDIkServos)
                    {
                        IKservo clone = element.Clone();
                        ArmRDIkServos.Add(clone);
                    }
                    // make in list reverse sequence
                    ArmRDIkServos.Reverse();

                    // Add EndEffector/LEE/ (last part) to SortIkServo - problem -> if LEE hold other element that will last part
                    ArmNDIkServos.Add(new IKservo(lastPart));
                    ArmRDIkServos.Add(new IKservo(part));

                    // if have all IKservo than calculate IK parameters of IKservos 
                    if (ArmNDIkServos.Count == JointList.Length)
                    {
                        IkBasePart = part;

                        // dumping servo structure
                        DumpServoStructure(ArmNDIkServos, part, IkDirection.NORMAL);

                        // dumping servo structure
                        DumpServoStructure(ArmRDIkServos, lastPart, IkDirection.REVERSE);

                        SetFKParamsToBuffer();
                        SetRdFKParamsToBuffer();
                        OnSave(null);

                        Debug.Log(string.Format("[IKRC] {0} DumpServoStructureEvent() - Servo Structure Dumped", 50));
                        //MsgWindow(new Vector2(1920f / 2f, 1080f / 2f), "Servo dumping message", "Servo Structure Dumped");
                        MsgWindow(new Vector2(Screen.width / 2f, Screen.height / 2f), "Servo dumping message", "Servo Structure Dumped");
                    }
                    else
                        //MsgWindow(new Vector2(1920f / 2f, 1080f / 2f), "Servo dumping message", "Servo Structure Not Dumped");
                        MsgWindow(new Vector2(Screen.width / 2f, Screen.height / 2f), "Servo dumping message", "Servo Structure Not Dumped");

                }
                else
                    Debug.Log(string.Format("[IKRC] - DumpServoStructureEvent() - AllIkServo.Count <= (JointList.Length - 1)"));
            }
            else
                return;

            if (GetFKParamsFromBuffer())
            {
                IsInitedModule = true;
                Debug.Log(string.Format("[IKRC] DumpServoStructureEvent() - IsInitedModule = true"));
            }
        }

        [KSPEvent(guiActive = true, guiName = "Turn on IKRC", active = false)]
        public void TurnOnIKRCEvent()
        {
            if (IsInitedModule)
            {
                GetWinParamsFromBuffer();
                // This will hide the TurnOnIKRCEvent event, and show the TurnOffIKRCEvene event.
                Events["TurnOnIKRCEvent"].active = false;
                Events["TurnOffIKRCEvent"].active = true;

                // cPos - tartget position = current arm position
                globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
                globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;

                if (windowExtended)
                {
                    WindowIsExtended = true;
                    windowSize = new Vector2(620, 600);
                    if (windowRectangle != null)
                        windowRectangle = new Rect(windowRectangle.x, windowRectangle.y, windowSize.x, windowSize.y);
                    closeWindowButtonPosition = new Vector2(windowSize.x - 20, 3);
                }

                ircWindowActive = true;
            }
        }

        [KSPEvent(guiActive = true, guiName = "Turn off IKRC", active = false)]
        public void TurnOffIKRCEvent()
        {
            // This will hide the TurnOffIKRCEven event, and show the TurnOnIKRCEvent event.
            Events["TurnOnIKRCEvent"].active = true;
            Events["TurnOffIKRCEvent"].active = false;
            ircWindowActive = false;
            //Debug.Log("[IKRC] TurnOffIKRCEvent()}");
            SetWinParamsToBuffer();
            OnSave(nodeInner);
        }
        #endregion KSPEvents

        #region OnMethods
        public void OnRenderObject()
        {
            if (ircWindowActive && HighLogic.LoadedSceneIsFlight && IsInitedModule)
            {
                if (targetIconHas)
                {
                    if (ArmIkServos[ArmIkServos.Count - 1] != null)
                    {
                        DrawTools.DrawSphere(ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position, Color.white, 0.1f);
                    }

                    if (dockingNodeObject != null)
                    {
                        DrawTools.DrawSphere(dockingNodeObject.position, Color.yellow, 0.1f);

                        if (global != null)
                        {
                            //DrawTools.DrawTransform(global.transform, 0.2f);
                            DrawTools.DrawSphere(global.transform.position, Color.red, 0.05f);
                        }
                    }
                }

                if (HasTrajectoryCursor)
                {
                    if (TrajectoryCursor != null && TrajectoryEditorMode)
                    {
                        DrawTools.DrawSphere(TrajectoryCursor.transform.position, Color.blue, 0.1f);
                        DrawTools.DrawTransform(TrajectoryCursor.transform, 0.3f);
                    }

                    if (trajectoryList.Count != 0)
                    {
                        for (int i = 0; i < trajectoryList.Count - 1; i++)
                        {
                            DrawTools.DrawTransform(trajectoryList[i].transform, 0.15f);
                            DrawTools.DrawSphere(trajectoryList[i].transform.position, Color.white, 0.03f);
                            DrawTools.DrawLine(trajectoryList[i].transform, trajectoryList[i + 1].transform, Color.white);
                        }
                        DrawTools.DrawTransform(trajectoryList[trajectoryList.Count - 1].transform, 0.15f);
                        DrawTools.DrawSphere(trajectoryList[trajectoryList.Count - 1].transform.position, Color.white, 0.03f);
                    }
                }

                if (servoTransformHas)
                {
                    //Debug.Log(string.Format("[IKRC] {0} - !IK_active ", 102));
                    for (int i = 0; i < servoGimbal.Length; i++)
                    {
                        DrawTools.DrawTransform(servoGimbal[i].transform, 0.3f);
                        //Debug.Log(string.Format("[IKRC] {0} - DrawTransform[{1}]", 103, i));
                    }

                    //if (hoverObject != null)
                    //    DrawTools.DrawTransform(hoverObject.transform, 0.3f);

                    if (dockingNodeObject != null)
                    {
                        DrawTools.DrawTransform(dockingNodeObject, 0.3f);
                    }

                    if (global != null)
                    {
                        DrawTools.DrawTransform(global.transform, 0.2f);
                    }

                    if (debugTransforms)
                    {
                        // Show part.transform
                        //if (part != null)
                        //    DrawTools.DrawTransform(part.transform, 1.0f);
                        if (IkBasePart != null)
                            DrawTools.DrawTransform(IkBasePart.transform, 1.0f);
                        // Show Global Zero Transform
                        if (gameObject == null)
                        {
                            gameObject = new GameObject();
                            gameObject.transform.position = new Vector3(0f, 0f, 0f);
                            gameObject.transform.rotation = Quaternion.identity;
                        }
                        else
                            DrawTools.DrawTransform(gameObject.transform, 2.0f);
                    }
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            //base.OnSave(node);

            Debug.Log(string.Format("[IKRC] Start of OnSave() {0}", counterOnSave));
            counterOnSave++;

            if (HighLogic.LoadedSceneIsFlight && IsInitedModule)
            {
                PluginConfiguration config = PluginConfiguration.CreateForType<IkRobotController>();

                if (configXML != null && configXML.ContentIsLoaded)
                {
                    Debug.Log(string.Format("[IKRC] OnSave() configXML != null && ContentIsLoaded"));

                    // save window parameters from buffer to config.xml
                    //config.SetValue(robotArmID + "-" + "IRC Window Position", windowPosition);
                    configXML.SetValue(robotArmID + "-" + "IRC-Window-Position", windowPosition);
                    Debug.Log(string.Format("[IKRC] {0} - OnSave() {3}windowPosition {1}, {2}", 0, windowPosition.x, windowPosition.y, robotArmID + "-"));

                    configXML.SetValue(robotArmID + "-" + "IRC-Window-Extended", windowExtended);
                    Debug.Log(string.Format("[IKRC] {0} - OnSave() {2}windowExtended {1}", 0, windowExtended.ToString(), robotArmID + "-"));

                    // save FK parameters from buffer to config.xml
                    if (FKparamsIkServo != null && JointList != null)
                    {
                        if (FKparamsIkServo.Count == JointList.Length)
                        {
                            int servoNumber = 0;
                            foreach (IKservo ikServo in FKparamsIkServo)
                            {
                                //config.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-PartRotation", ikServo.fkParams.PartRotation);
                                configXML.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-PartRotation", ikServo.fkParams.PartRotation);
                                Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-Params-PartRotation - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.PartRotation));
                                //config.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-ParentOffset", ikServo.fkParams.ParentOffset);
                                configXML.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-ParentOffset", ikServo.fkParams.ParentOffset);
                                Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-Params-ParentOffset - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.ParentOffset));
                                //config.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Axis", ikServo.fkParams.Axis);
                                configXML.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Axis", ikServo.fkParams.Axis);
                                Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-Params-Axis - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.Axis));
                                ////config.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Position", ikServo.fkParams.Position);
                                //config.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Rotation", ikServo.fkParams.Rotation);
                                configXML.SetValue(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Rotation", ikServo.fkParams.Rotation);
                                Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-Params-Rotation - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.Rotation));

                                servoNumber++;
                            }

                            // save RdFK parameters from buffer to config.xml
                            if (RdFKparamsIkServo != null && JointList != null && robotArmID == "CA2")
                            {
                                if (RdFKparamsIkServo.Count == JointList.Length)
                                {
                                    int RdservoNumber = 0;
                                    foreach (IKservo ikServo in RdFKparamsIkServo)
                                    {
                                        //config.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-PartRotation", ikServo.fkParams.PartRotation);
                                        configXML.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-PartRotation", ikServo.fkParams.PartRotation);
                                        Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-RdParams-PartRotation - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.PartRotation));
                                        //config.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-ParentOffset", ikServo.fkParams.ParentOffset);
                                        configXML.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-ParentOffset", ikServo.fkParams.ParentOffset);
                                        Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-RdParams-ParentOffset - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.ParentOffset));
                                        //config.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Axis", ikServo.fkParams.Axis);
                                        configXML.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Axis", ikServo.fkParams.Axis);
                                        Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-RdParams-Axis - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.Axis));
                                        ////config.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Position", ikServo.fkParams.Position);
                                        //config.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Rotation", ikServo.fkParams.Rotation);
                                        configXML.SetValue(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Rotation", ikServo.fkParams.Rotation);
                                        Debug.Log(string.Format("[IKRC] - OnSave() {0}{1}-Servo-RdParams-Rotation - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.Rotation));

                                        RdservoNumber++;
                                    }
                                }
                            }

                            configXML.SaveVariables(config);
                            config.save();

                            ridbtList.Clear();
                            foreach (string element in configXML.robotIDList)
                            {
                                ridbtList.Add(new BoolText(false, element));
                            }

                            Debug.Log(string.Format("[IKRC] {0} end of OnSave()", 0));
                        }
                    }
                }
                else
                {
                    Debug.Log(string.Format("[IKRC] OnSave() configXML {0}", configXML == null ? "== null" : "!= null"));
                    if (configXML != null)
                        Debug.Log(string.Format("[IKRC] OnSave() ContentIsLoaded == {0}", configXML.ContentIsLoaded ? "true" : "false"));
                }
            }
            else
            {
                Debug.Log(string.Format("[IKRC] - OnSave() HighLogic.LoadedSceneIsFlight = {0}", HighLogic.LoadedSceneIsFlight.ToString()));
                Debug.Log(string.Format("[IKRC] - OnSave() IsInitedModule = {0}", IsInitedModule.ToString()));
            }

            Debug.Log(string.Format("[IKRC] - OnSave() " + HighLogic.LoadedScene.ToString()));
            Debug.Log(string.Format("[IKRC] - OnSave() {0}", onSaveCounter));
            onSaveCounter++;
        }

        public override void OnLoad(ConfigNode node)
        {
            //base.OnLoad(node);

            Debug.Log(string.Format("[IKRC] Start of OnLoad() {0}", counterOnLoad));
            counterOnLoad++;

            //if (HighLogic.LoadedSceneIsFlight)
            //{
            Debug.Log(string.Format("[IKRC] - OnLoad() HighLogic.LoadedSceneIsFlight = {0}", HighLogic.LoadedSceneIsFlight.ToString()));

            if (configXML == null)
                configXML = new ConfigXML();

            PluginConfiguration config = PluginConfiguration.CreateForType<IkRobotController>();

            config.load();

            if (configXML != null)
            {
                configXML.LoadVariables(config);

                // load window parameters from config.xml to buffer
                //windowPosition = config.GetValue<Vector2>(robotArmID + "-" + "IRC Window Position");
                windowPosition = configXML.GetVector2(robotArmID + "-" + "IRC-Window-Position");
                Debug.Log(string.Format("[IKRC] {0} - OnLoad() {3}windowPosition {1}, {2}", 42, windowPosition.x, windowPosition.y, robotArmID + "-"));

                windowExtended = configXML.GetBool(robotArmID + "-" + "IRC-Window-Extended");
                Debug.Log(string.Format("[IKRC] {0} - OnLoad() {2}windowExtended {1}", 42, windowExtended.ToString(), robotArmID + "-"));

                if (FKparamsIkServo == null)
                    CreateFKparams();

                if (RdFKparamsIkServo == null && robotArmID == "CA2")
                    CreateRdFKparams();

                // load FK parameters from config.xml to buffer
                int servoNumber = 0;
                int RdservoNumber = 0;

                Debug.Log(string.Format("[IKRC] - OnLoad() robotArmID = {0}", robotArmID));

                JointListIsLoaded = GetJointListFromPartConfig();

                if (!JointListIsLoaded)
                {
                    if (!GetJointListFromPartConfig())
                    {
                        Debug.Log(string.Format("[IKRC] OnLoad() - !GetJointListFromPartConfig()"));
                        return;
                    }
                    else
                        JointListIsLoaded = true;
                }

                // check exist variables with robotArmID
                if (!configXML.CheckExistVariables(robotArmID, "IRC-Window-Position") || !configXML.CheckExistVariables(robotArmID, JointVariableNames, JointList.Length))
                {
                    if (!configXML.CreateVariables(robotArmID, "IRC-Window-Position", "Vector2"))
                        Debug.Log(string.Format("[IKRC] - Start() configXML.CreateVariables() IRC-Window-Position fault"));
                    if (!configXML.CreateVariables(robotArmID, "IRC-Window-Extended", "bool"))
                        Debug.Log(string.Format("[IKRC] - Start() configXML.CreateVariables() IRC-Window-Extended fault"));
                    if (!configXML.CreateVariables(robotArmID, JointVariableNames, JointVariableTypes, JointList.Length))
                        Debug.Log(string.Format("[IKRC] - Start() configXML.CreateVariables() JointVariables fault"));
                }
                else
                    Debug.Log(string.Format("[IKRC] - Start() configXML.CheckExistVariables() OK"));

                if ( robotArmID == "CA2" && !configXML.CheckExistVariables(robotArmID, JointRdVariableNames, JointList.Length))
                {
                    if (!configXML.CreateVariables(robotArmID, JointRdVariableNames, JointRdVariableTypes, JointList.Length))
                        Debug.Log(string.Format("[IKRC] - Start() configXML.CreateVariables() JointRdVariables fault"));
                }

                foreach (IKservo ikServo in FKparamsIkServo)
                {
                    //ikServo.fkParams.PartRotation = config.GetValue<Quaternion>(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-PartRotation");
                    ikServo.fkParams.PartRotation = configXML.GetQuaternion(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-PartRotation");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-Params-PartRotation - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.PartRotation));
                    //ikServo.fkParams.ParentOffset = config.GetValue<Vector3>(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-ParentOffset");
                    ikServo.fkParams.ParentOffset = configXML.GetVector3(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-ParentOffset");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-Params-ParentOffset - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.ParentOffset));
                    //ikServo.fkParams.Axis = config.GetValue<Vector3>(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Axis");
                    ikServo.fkParams.Axis = configXML.GetVector3(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Axis");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-Params-Axis - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.Axis));
                    ////ikServo.fkParams.Position = config.GetValue<Vector3>(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Position");
                    //ikServo.fkParams.Rotation = config.GetValue<Quaternion>(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Rotation");
                    ikServo.fkParams.Rotation = configXML.GetQuaternion(robotArmID + "-" + servoNumber.ToString() + "-Servo-Params-Rotation");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-Params-Rotation - {2}", robotArmID + "-", servoNumber.ToString(), ikServo.fkParams.Rotation));

                    Debug.Log(string.Format("[IKRC] {0} - OnLoad() " + VectorToString(ikServo.fkParams.ParentOffset, "0.00"), 42));

                    servoNumber++;
                }

                if (robotArmID == "CA2")
                    foreach (IKservo ikServo in RdFKparamsIkServo)
                {
                    //ikServo.fkParams.PartRotation = config.GetValue<Quaternion>(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-PartRotation");
                    ikServo.fkParams.PartRotation = configXML.GetQuaternion(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-PartRotation");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-RdParams-PartRotation - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.PartRotation));
                    //ikServo.fkParams.ParentOffset = config.GetValue<Vector3>(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-ParentOffset");
                    ikServo.fkParams.ParentOffset = configXML.GetVector3(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-ParentOffset");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-RdParams-ParentOffset - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.ParentOffset));
                    //ikServo.fkParams.Axis = config.GetValue<Vector3>(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Axis");
                    ikServo.fkParams.Axis = configXML.GetVector3(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Axis");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-RdParams-Axis - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.Axis));
                    ////ikServo.fkParams.Position = config.GetValue<Vector3>(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Position");
                    //ikServo.fkParams.Rotation = config.GetValue<Quaternion>(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Rotation");
                    ikServo.fkParams.Rotation = configXML.GetQuaternion(robotArmID + "-" + RdservoNumber.ToString() + "-Servo-RdParams-Rotation");
                    Debug.Log(string.Format("[IKRC] - OnLoad() {0}{1}-Servo-RdParams-Rotation - {2}", robotArmID + "-", RdservoNumber.ToString(), ikServo.fkParams.Rotation));

                    Debug.Log(string.Format("[IKRC] {0} - OnLoad() " + VectorToString(ikServo.fkParams.ParentOffset, "0.00"), 42));

                    RdservoNumber++;
                }

                GetWinParamsFromBuffer();
                ircWindowActive = false;
                nodeInner = node;

                Debug.Log(string.Format("[IKRC] {0} end of OnLoad()", 0));

                Debug.Log(string.Format("[IKRC] - OnLoad() " + HighLogic.LoadedScene.ToString()));
                Debug.Log(string.Format("[IKRC] - OnLoad() {0}", onLoadCounter));
                onLoadCounter++;

                foreach (IKservo ikServo in FKparamsIkServo)
                {
                    if (ikServo.fkParams.ParentOffset != (new Vector3(0f, 0f, 0f)))
                    {
                        IsLoadedFkParams = true;
                        return;
                    }
                }

                if (robotArmID == "CA2")
                foreach (IKservo ikServo in RdFKparamsIkServo)
                {
                    if (ikServo.fkParams.ParentOffset != (new Vector3(0f, 0f, 0f)))
                    {
                        IsLoadedRdFkParams = true;
                        return;
                    }
                }
            }
            else
                Debug.Log(string.Format("[IKRC] OnLoad() configXML == null"));
            //}
            //else
            //    Debug.Log(string.Format("[IKRC] - OnLoad() HighLogic.LoadedSceneIsFlight = {0}", HighLogic.LoadedSceneIsFlight.ToString()));

            ridbtList.Clear();
            foreach (string element in configXML.robotIDList)
            {
                ridbtList.Add(new BoolText(false, element));
            }
        }

        private void SetWinParamsToBuffer()
        {
            windowPosition.x = windowRectangle.x;
            windowPosition.y = windowRectangle.y;
        }

        private void GetWinParamsFromBuffer()
        {
            if (windowRectangle != null)
                windowRectangle = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);
        }

        public void OnDisable()
        {
            //if (ircWindowActive)
            //{
            //    Debug.Log("[IKRC] - OnDisable()");
            //    ircWindowActive = false;
            //    OnSave(nodeInner);
            //}
            ircWindowActive = false;
            IK_active = false;
            servoTransformHas = false;
            targetIconHas = false;
            IsInitedModule = false;
        }

        public void OnDestroy()
        {
            if (ircWindowActive)
            {
                Debug.Log("[IKRC] - Destroy()");
                SetWinParamsToBuffer();
                ircWindowActive = false;
                OnSave(nodeInner);
            }
            IK_active = false;
            servoTransformHas = false;
            targetIconHas = false;
            IsInitedModule = false;
        }

        public void Destroy()
        {
            if (ircWindowActive)
            {
                Debug.Log("[IKRC] - Destroy()");
                SetWinParamsToBuffer();
                ircWindowActive = false;
                OnSave(nodeInner);
            }
            IK_active = false;
            servoTransformHas = false;
            targetIconHas = false;
            IsInitedModule = false;
        }
        #endregion OnMethods

        #region GUI
        public bool ButtonsIsReleased()
        {
            if ((buttons.Translation == new Vector3(0f, 0f, 0f)) && (buttons.Rotation.eulerAngles == new Vector3(0f, 0f, 0f)))
                return true;
            else
                return false;
        }

        public void ButtonsReleased()
        {
            buttons.Translation = new Vector3(0f, 0f, 0f);
            buttons.Rotation = Quaternion.identity;
        }

        public void OnGUI()
        {
            if (ircWindowActive)
            {
                // Draw window
                windowRectangle = GUILayout.Window(WindowID, windowRectangle, OnWindow, robotArmID + " IK Robot Controller");
                //Debug.Log(string.Format("[TRF{1}] {0} ircWindowActive", 1000, WindowID));
            }

            if (msgWindowActive)
            {
                // Draw message window
                //msgWindowRectangle = GUILayout.Window(MsgWindowID, msgWindowRectangle, OnMsgWindow, msgWindowTitle);
                msgWindowRectangle = GUILayout.Window(MsgWindowID, new Rect(msgWindowPosition, new Vector2(msgWindowRectangle.width, msgWindowRectangle.height)), OnMsgWindow, msgWindowTitle);
            }

            if (chkWindowActive)
            {
                // Draw check window
                chkWindowRectangle = GUILayout.Window(ChkWindowID, new Rect(chkWindowPosition, new Vector2(chkWindowRectangle.width, chkWindowRectangle.height)), OnChkWindow, chkWindowTitle);
            }

            if (cfgWindowActive)
            {
                // Draw window
                cfgWindowRectangle = GUILayout.Window(CfgWindowID, cfgWindowRectangle, OnCfgWindow, "Manage Config File");
            }
        }

        public void OnWindow(int windowID)
        {
            if (IsInitedModule)
            {
                GUILayout.BeginHorizontal();

                #region Translation & Rotation Buttons
                // Coloring translation and rotation buttons
                if (buttonIsColored)
                {
                    InitStyles();
                    // Translation buttons
                    GUI.Box(new Rect(trButtonsOrigin.x - 2, trButtonsOrigin.y + 33, 34, 34), "", greenStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 68, trButtonsOrigin.y + 33, 34, 34), "", greenStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 33, trButtonsOrigin.y - 2, 34, 34), "", redStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 33, trButtonsOrigin.y + 68, 34, 34), "", redStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x - 2, trButtonsOrigin.y + 68, 34, 34), "", blueStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 68, trButtonsOrigin.y - 2, 34, 34), "", blueStyle);
                    // Rotation buttons
                    GUI.Box(new Rect(trButtonsOrigin.x + 108, trButtonsOrigin.y + 33, 34, 34), "", blueStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 178, trButtonsOrigin.y + 33, 34, 34), "", blueStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 143, trButtonsOrigin.y - 2, 34, 34), "", greenStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 143, trButtonsOrigin.y + 68, 34, 34), "", greenStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 108, trButtonsOrigin.y + 68, 34, 34), "", redStyle);
                    GUI.Box(new Rect(trButtonsOrigin.x + 178, trButtonsOrigin.y - 2, 34, 34), "", redStyle);
                }

                // Translation buttons
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x, trButtonsOrigin.y + 35, 30, 30), "◄"))
                    buttons.Translation.y = 0.025f * transStep;
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 70, trButtonsOrigin.y + 35, 30, 30), "►"))
                    buttons.Translation.y = -0.025f * transStep;
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 35, trButtonsOrigin.y, 30, 30), "▲"))
                    buttons.Translation.x = 0.025f * transStep;
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 35, trButtonsOrigin.y + 70, 30, 30), "▼"))
                    buttons.Translation.x = -0.025f * transStep;
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x, trButtonsOrigin.y + 70, 30, 30), "●"))
                    buttons.Translation.z = -0.025f * transStep;
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 70, trButtonsOrigin.y, 30, 30), "•"))
                    buttons.Translation.z = 0.025f * transStep;

                // Rotation buttons

                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 110, trButtonsOrigin.y + 35, 30, 30), "˅ʘ"))
                    buttons.Rotation = Quaternion.Euler(0f, 0f, 0.5f * rotStep);
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 180, trButtonsOrigin.y + 35, 30, 30), "ʘ˅"))
                    buttons.Rotation = Quaternion.Euler(0f, 0f, -0.5f * rotStep);
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 145, trButtonsOrigin.y, 30, 30), "˄"))
                    buttons.Rotation = Quaternion.Euler(0f, 0.5f * rotStep, 0f);
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 145, trButtonsOrigin.y + 70, 30, 30), "˅"))
                    buttons.Rotation = Quaternion.Euler(0f, -0.5f * rotStep, 0f);
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 110, trButtonsOrigin.y + 70, 30, 30), "˂"))
                    buttons.Rotation = Quaternion.Euler(-0.5f * rotStep, 0f, 0f);
                if (GUI.RepeatButton(new Rect(trButtonsOrigin.x + 180, trButtonsOrigin.y, 30, 30), "˃"))
                    buttons.Rotation = Quaternion.Euler(0.5f * rotStep, 0f, 0f);
                #endregion Translation & Rotation Buttons

                // IK active toggle
                IK_active = GUI.Toggle(new Rect(15, 15, 70, 20), IK_active, " IK active");
                if (TrajectoryEditorMode)
                    IK_active = false;
                // icon of target toggle
                targetIconHas = GUI.Toggle(new Rect(90, 15, 60, 20), targetIconHas, " trgICO");
                // transform of servos toggle
                servoTransformHas = GUI.Toggle(new Rect(150, 15, 75, 20), servoTransformHas, " servoTRF");
                // zeroTheta iterate toggle
                zeroThetaIterate = GUI.Toggle(new Rect(230, 15, 60, 20), zeroThetaIterate, " 0ƟIter");
                // clear values of Theta
                clearThetasBool = GUI.Button(new Rect(250, 35, 45, 20), "clrƟ");
                // set aim position and orientation to actual state
                actualStateBool = GUI.Button(new Rect(250, 60, 45, 20), "acSt");
                // set base position and orientation
                baseStateBool = GUI.Button(new Rect(250, 85, 45, 20), "0St");

                // reset position and rotation of endeffector
                if (GUI.Button(new Rect(250, 110, 45, 20), "origin"))
                {
                    globalPosition = baseState.Translation;
                    globalQuaternion = baseState.Rotation;
                }

                // set current position and rotation of endeffector
                if (GUI.Button(new Rect(250, 135, 45, 20), "cPos"))
                {
                    globalPosition = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position;
                    globalQuaternion = ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation;
                }

                #region Target Definition
                // set selected part than virtual endeffector
                veeStateBool = GUI.Button(new Rect(10, 210, 40, 20), "VEE");
                // set aim position and orientation to selected PDGF
                pdgfStateBool = GUI.Button(new Rect(60, 210, 50, 20), "target");
                // set selected position and orientation than near aim
                nearPDGFBool = GUI.Toggle(new Rect(120, 210, 70, 20), nearPDGFBool, " dockPos");
                // set refresh selected position and orientation
                refreshPDGFBool = GUI.Toggle(new Rect(200, 210, 70, 20), refreshPDGFBool, " rePos");

                // Force Distance and ForceRoll
                Yoffset = 0;
                inputRect = new Rect(10, 235, 40, 20);
                // Force DistZ
                IsForceDist = GUI.Toggle(new Rect(10, 235, 90, 20), IsForceDist, " Force DistZ");
                AddInputValue(inputRect, forceDistStringZ, out forceDistStringZ, "", out forceDistNumberZ, 90f);
                // Force RollZ
                IsForceRoll = GUI.Toggle(new Rect(10, 255, 90, 20), IsForceRoll, " Force RollZ");
                AddInputValue(inputRect, forceRollStringZ, out forceRollStringZ, "", out forceRollNumberZ, 90f);

                Yoffset = 0;
                inputRect = new Rect(140, 235, 40, 20);
                // Force DistX
                AddInputValue(inputRect, forceDistStringX, out forceDistStringX, "DistX", out forceDistNumberX, 35f);
                // Force RollX
                AddInputValue(inputRect, forceRollStringX, out forceRollStringX, "RollX", out forceRollNumberX, 35f);

                Yoffset = 0;
                inputRect = new Rect(225, 235, 40, 20);
                // Force DistZ
                AddInputValue(inputRect, forceDistStringY, out forceDistStringY, "DistY", out forceDistNumberY, 35f);
                // Force RollZ
                AddInputValue(inputRect, forceRollStringY, out forceRollStringY, "RollY", out forceRollNumberY, 35f);
                #endregion Target Definition

                #region close window
                // Close window button
                if (GUI.Button(new Rect(closeWindowButtonPosition.x, closeWindowButtonPosition.y, 17, 15), "x"))
                {
                    ircWindowActive = false;
                    // This will hide the TurnOffIKRCEven event, and show the TurnOnIKRCEvent event.
                    Events["TurnOnIKRCEvent"].active = true;
                    Events["TurnOffIKRCEvent"].active = false;
                    SetWinParamsToBuffer();
                    ircWindowActive = false;
                    Debug.Log("[IKRC] Close window button");
                    OnSave(nodeInner);
                }
                //Debug.Log(string.Format("[IKRC] {0} - Close window button", 1004));
                #endregion close window

                #region window extender
                windowExtended = WindowIsExtended;
                // Extend windows button
                if (!WindowIsExtended)
                {
                    if (GUI.Button(new Rect(3, 3, 17, 15), ">"))
                    {
                        WindowIsExtended = true;
                        windowSize = new Vector2(620, 600);
                        if (windowRectangle != null)
                            windowRectangle = new Rect(windowRectangle.x, windowRectangle.y, windowSize.x, windowSize.y);
                        closeWindowButtonPosition = new Vector2(windowSize.x - 20, 3);
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(3, 3, 17, 15), "<"))
                    {
                        WindowIsExtended = false;
                        windowSize = new Vector2(310, 600);
                        if (windowRectangle != null)
                            windowRectangle = new Rect(windowRectangle.x, windowRectangle.y, windowSize.x, windowSize.y);
                        closeWindowButtonPosition = new Vector2(windowSize.x - 20, 3);
                    }

                    #region IK display of parameters
                    // IK values
                    Yoffset = 0;
                    inputRect = new Rect(320, 20, 55, 20);
                    AddInputValue(inputRect, iterateString, out iterateString, "IK_iterate", out iterateNumber, 150f);
                    AddInputValue(inputRect, samplingAngleString, out samplingAngleString, "IK_samplingAngle", out SamplingAngle, 150f);
                    AddInputValue(inputRect, DistanceThresholdString, out DistanceThresholdString, "IK_DistanceThreshold", out DistanceThreshold, 150f);
                    AddOutputValue(inputRect, "IK_Distance", Distance, 150f, "0.0000");
                    AddInputValue(inputRect, AngleThresholdString, out AngleThresholdString, "IK_AngleThreshold", out AngleThreshold, 150f);
                    AddOutputValue(inputRect, "IK_Angle", Angle, 150f, "0.0000");
                    AddInputValue(inputRect, MaxPosErrString, out MaxPosErrString, "IK_MaxPosErr", out MaxPosErr, 150f);
                    AddInputValue(inputRect, MaxOriErrString, out MaxOriErrString, "IK_MaxOriErr", out MaxOriErr, 150f);
                    AddOutputValue(inputRect, "IK_DinLearningRatePos", DinLearningRatePos, 150f, "0.0000");
                    AddOutputValue(inputRect, "IK_DinLearningRateOri", DinLearningRateOri, 150f, "0.0000");
                    #endregion IK display of parameters

                    //// MinMax values
                    //Yoffset = 0;
                    //inputRect = new Rect(320, 245, 50, 20);
                    //for (int i = 0; i < (JointList.Length - 1); i++)
                    //{
                    //    AddOutputValue(inputRect, "min[" + i.ToString() + "]", SortIkServo[i].MinAngle, 4f);
                    //}
                    //Yoffset = 0;
                    //inputRect = new Rect(420, 245, 50, 20);
                    //for (int i = 0; i < (JointList.Length - 1); i++)
                    //{
                    //    AddOutputValue(inputRect, "max[" + i.ToString() + "]", SortIkServo[i].MaxAngle, 4f);
                    //}

                    // ReverseIkDirection
                    ReverseIkDirection = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 150, 150, 20), ReverseIkDirection, " ReverseIkDirection");

                    // TrajectoryControlMode
                    TrajectoryControlMode = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 130, 120, 20), TrajectoryControlMode, " TrajectoryControl");
                    // TrajectoryStepMode
                    TrajectoryStepMode = GUI.Toggle(new Rect(gradientDiagramRectangle.x + 130, gradientDiagramRectangle.y - 130, 120, 20), TrajectoryStepMode, " TrajectoryStep");
                    // TrajectoryStep
                    StepTrajectory = GUI.Button(new Rect(gradientDiagramRectangle.x + 240, gradientDiagramRectangle.y - 130, 50, 20), "step");
                    // TrajectoryEditorMode
                    TrajectoryEditorMode = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 110, 110, 20), TrajectoryEditorMode, " TrajectoryEditor");
                    if (IK_active)
                        TrajectoryEditorMode = false;
                    // Add to trajectory list
                    AddTrajectory = GUI.Button(new Rect(gradientDiagramRectangle.x + 120, gradientDiagramRectangle.y - 110, 40, 20), "add");
                    // Remove from trajectory list
                    RemoveTrajectory = GUI.Button(new Rect(gradientDiagramRectangle.x + 170, gradientDiagramRectangle.y - 110, 60, 20), "remove");
                    // Delete trajectory list
                    ClearTrajectory = GUI.Button(new Rect(gradientDiagramRectangle.x + 240, gradientDiagramRectangle.y - 110, 50, 20), "clear");

                    // Move target toggle
                    moveTarget = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 90, 110, 20), moveTarget, " MoveTarget");
                    // Normalized servo rotation toggle
                    NormalizedServoRotation = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 70, 110, 20), NormalizedServoRotation, " NormServRot");
                    // SLT active toggle
                    SLTactive = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 50, 100, 20), SLTactive, " STL active");
                    Yoffset = 0;
                    inputRect = new Rect(gradientDiagramRectangle.x + 90, gradientDiagramRectangle.y - 50, 55, 20);
                    AddInputValue(inputRect, stlBitString, out stlBitString, "STL bit", out stlBit, 50f);
                    //// SLT breaking active toggle
                    //SLTBreakingActive = GUI.Toggle(new Rect(gradientDiagramRectangle.x + 100, gradientDiagramRectangle.y - 50, 100, 20), SLTBreakingActive, " STL breaking");
                    // Enable adjust parameters toggle
                    dynamicSamplingAngle = GUI.Toggle(new Rect(gradientDiagramRectangle.x, gradientDiagramRectangle.y - 30, 100, 20), dynamicSamplingAngle, " Dyn.SAngle");

                    Color[] debugColor = { Color.red, Color.yellow, Color.magenta };
                    LineDiagram(gradientDiagramRectangle, gradientData, debugColor);
                }
                #endregion window extender

                using (var scrollScope = new GUI.ScrollViewScope(new Rect(10, 361, 190, 162), ThetascrollPosition, new Rect(0, 0, (10 + 20), (float)((JointList.Length - 1 + indepServo.Count) * 20 + 2)), false, true))
                {
                    ThetascrollPosition = scrollScope.scrollPosition;

                    #region Theta values
                    // Theta values
                    Yoffset = 0;
                    inputRect = new Rect(1, 2, 50, 20);
                    for (int i = 0; i < (JointList.Length - 1); i++)
                    {
                        if (IK_active)
                            AddOutputValue(inputRect, "Ɵ[" + i.ToString() + "]", theta[i], 47f);
                        else
                        {
                            thetaString[i] = theta[i].ToString("0.000");
                            AddInputValue(inputRect, thetaString[i], out thetaString[i], "Ɵ[" + i.ToString() + "]", out theta[i], 47f);
                            theta[i] = Mathf.Clamp(theta[i], ArmIkServos[i].MinAngle, ArmIkServos[i].MaxAngle);
                        }
                    }
                    float valueOffset = Yoffset;

                    // Control buttons of theta
                    Yoffset = 0;
                    inputRect = new Rect(27, 2, 20, 20);
                    for (int i = 0; i < (JointList.Length - 1); i++)
                    {
                        if (IK_active)
                            AddRepeatButton(inputRect, "◄");
                        else
                        {
                            if (AddRepeatButton(inputRect, "◄"))
                            {
                                theta[i] = theta[i] - 0.25f * rotStep;
                                theta[i] = Mathf.Clamp(theta[i], ArmIkServos[i].MinAngle, ArmIkServos[i].MaxAngle);
                                //Debug.Log(string.Format("[IKRC] OnWindow() - {0}. - MinAngle = {1} ({3}) MaxAngle = {2}", i, SortIkServo[i].MinAngle, SortIkServo[i].MaxAngle, theta[i]));
                            }
                        }

                    }
                    float cb1Offset = Yoffset;

                    Yoffset = 0;
                    inputRect = new Rect(99, 2, 20, 20);
                    for (int i = 0; i < (JointList.Length - 1); i++)
                    {
                        if (IK_active)
                            AddRepeatButton(inputRect, "►");
                        else
                        {
                            if (AddRepeatButton(inputRect, "►"))
                            {
                                theta[i] = theta[i] + 0.25f * rotStep;
                                theta[i] = Mathf.Clamp(theta[i], ArmIkServos[i].MinAngle, ArmIkServos[i].MaxAngle);
                                //Debug.Log(string.Format("[IKRC] OnWindow() - {0}. - MinAngle = {1} ({3}) MaxAngle = {2}", i, SortIkServo[i].MinAngle, SortIkServo[i].MaxAngle, theta[i]));
                            }
                        }
                    }
                    float cb2Offset = Yoffset;

                    // Theta current values
                    Yoffset = 0;
                    inputRect = new Rect(99, 2, 50, 20);
                    for (int i = 0; i < (JointList.Length - 1); i++)
                    {
                        //AddOutputValue(inputRect, "Ɵ[" + i.ToString() + "]", currentTheta[i], 30f);
                        AddOutputValue(inputRect, "", currentTheta[i], 20f);
                    }
                    float currentValueOffset = Yoffset;

                    //Theta blocker
                    //for (int i = 0; i < (JointList.Length - 1); i++)
                    //{
                    //    blocking[i] = GUI.Toggle(new Rect(92, 365 + (i * 20), 20, 20), blocking[i], "");
                    //}
                    #endregion Theta values

                    #region IndepServo values
                    if (HasIndependentServos)
                    {
                        // IndepServo values
                        Yoffset = valueOffset;
                        inputRect = new Rect(1, 2, 50, 20);
                        for (int i = 0; i < indepServo.Count; i++)
                        {
                            if (IK_active)
                                AddOutputValue(inputRect, "I[" + i.ToString() + "]", iota[i], 47f);
                            else
                            {
                                iotaString[i] = iota[i].ToString("0.000");
                                AddInputValue(inputRect, iotaString[i], out iotaString[i], "I[" + i.ToString() + "]", out iota[i], 47f);
                                iota[i] = Mathf.Clamp(iota[i], indepServo[i].MinAngle, indepServo[i].MaxAngle);
                            }
                        }
                        // Control buttons of IndepServo
                        Yoffset = cb1Offset;
                        inputRect = new Rect(27, 2, 20, 20);
                        for (int i = 0; i < indepServo.Count; i++)
                        {
                            if (IK_active)
                                AddRepeatButton(inputRect, "◄");
                            else
                            {
                                if (AddRepeatButton(inputRect, "◄"))
                                {
                                    iota[i] = iota[i] - 0.25f * rotStep;
                                    iota[i] = Mathf.Clamp(iota[i], indepServo[i].MinAngle, indepServo[i].MaxAngle);
                                }
                            }

                        }
                        Yoffset = cb2Offset;
                        inputRect = new Rect(99, 2, 20, 20);
                        for (int i = 0; i < indepServo.Count; i++)
                        {
                            if (IK_active)
                                AddRepeatButton(inputRect, "►");
                            else
                            {
                                if (AddRepeatButton(inputRect, "►"))
                                {
                                    iota[i] = iota[i] + 0.25f * rotStep;
                                    iota[i] = Mathf.Clamp(iota[i], indepServo[i].MinAngle, indepServo[i].MaxAngle);
                                }
                            }
                        }
                        // IndepServo current values
                        Yoffset = currentValueOffset;
                        inputRect = new Rect(99, 2, 50, 20);
                        for (int i = 0; i < indepServo.Count; i++)
                        {
                            AddOutputValue(inputRect, "", currentIota[i], 20f);
                        }
                    }
                    #endregion IndepServo values
                }

                #region Debug Text
                //string dbgText = String.Format("{0} {1}", VectorToString(SortIkServo[0].fkParams.Axis, "0.00"), VectorToString(JointsRealAxis[0], "0"));
                //GUI.Label(new Rect(20, 500, 290, 20), dbgText);
                #endregion Debug Text

                #region position information
                Yoffset = 0;
                inputRect = new Rect(10, 280, 40, 20);
                AddOutputValue(inputRect, "IK_Position", globalPosition, 80f, 90f);
                AddOutputValue(inputRect, "IK_Rotation", globalQuaternion.eulerAngles, 80f, 90f);
                // Bug !? -->
                AddOutputValue(inputRect, "EE_Position", ArmIkServos[ArmIkServos.Count - 1].ServoTransform.position, 80f, 90f);
                AddOutputValue(inputRect, "EE_Rotation", ArmIkServos[ArmIkServos.Count - 1].ServoTransform.rotation.eulerAngles, 80f, 90f);
                inputRect = new Rect(200, 280, 40, 20);
                AddOutputValue(inputRect, "distance", distance, 55f, 45f);
                AddOutputValue(inputRect, "angle", angle, 55f, 45f);
                AddOutputValue(inputRect, "wDist", workingDistance, 55f, 45f);
                AddInputValue(inputRect, ArmMaxLengthString, out ArmMaxLengthString, "wRange", out ArmMaxLength, 55f);
                #endregion position information

                // emergency servo stop
                emergencyServoStop = GUI.Toggle(new Rect(225, 280, 80, 20), emergencyServoStop, " EMGstp");

                // Success value
                GUI.Toggle(new Rect(225, 300, 80, 20), Success, " Success");
                // IK_Success value
                GUI.Toggle(new Rect(225, 320, 80, 20), IKsuccess, " IK Success");
                // Bug value
                GUI.Toggle(new Rect(225, 340, 80, 20), Bug, " Bug");

                // slider of servo speed
                //float[] speedSliderValues = { 0.0f, 0.031f, 0.063f, 0.125f, 0.25f, 0.5f, 1.0f, 2.0f, 4.0f };
                float[] speedSliderValues = { 0.0f, 0.031f, 0.063f, 0.125f, 0.25f, 0.5f, 1.0f, 2.0f, 4.0f, 8.0f };
                servoSpeed = FixValuesLogSlider(new Rect(10, 140, 150, 20), 50f, "speed", servoSpeed, speedSliderValues);
                // slider of control button translation's step
                float[] transSliderValues = { 0.0f, 0.0375f, 0.075f, 0.125f, 0.25f, 0.5f, 1.0f, 2.0f };
                transStep = FixValuesLogSlider(new Rect(10, 160, 150, 20), 50f, "TRLstp", transStep, transSliderValues);
                // slider of control button rotation's step
                float[] rotSliderValues = { 0.0f, 0.0375f, 0.075f, 0.125f, 0.25f, 0.5f, 1.0f, 2.0f };
                rotStep = FixValuesLogSlider(new Rect(10, 180, 150, 20), 50f, "ROTstp", rotStep, rotSliderValues);

                // set position and rotation of endeffector ???

                Color[] color = { Color.red, Color.yellow, Color.magenta };
                LineDiagram(diagramRectangle, data, color);

                manageConfig = GUI.Button(new Rect(225, 495, 70, 20), "manCfg");

                //manualMoveReference = GUI.Toggle(new Rect(225, 470, 90, 20), manualMoveReference, " ManMovRef");

                if (manageConfig)
                {
                    cfgWindowActive = true;
                    foreach (BoolText element in ridbtList)
                    {
                        element.active = false;
                    }
                }

                GUILayout.EndHorizontal();

                GUI.DragWindow();
            }
        }

        public void OnCfgWindow(int cfgWindowID)
        {
            GUILayout.BeginHorizontal();

            closeCfgWindowButtonPosition = new Vector2(cfgWindowSize.x - 20, 3);

            // Close window button
            if (GUI.Button(new Rect(closeCfgWindowButtonPosition.x, closeCfgWindowButtonPosition.y, 17, 15), "x"))
            {
                cfgWindowActive = false;
                chkWindowResult = false;
            }

            RIDScrollPosition = ToggleListScrollScope(ridbtList, new Rect(10, 20, 150, 150), RIDScrollPosition, true, true);

            bool deleteSelected = GUI.Button(new Rect(180, 20, 60, 20), "delete");
            // ??? close button

            if (deleteSelected)
            {
                bool result = false;
                for (int i = 0; i < ridbtList.Count; i++)
                {
                    result = ridbtList[i].active;
                    break;
                }
                if (result)
                    ChkWindow(new Vector2(Screen.width / 2f, Screen.height / 2f), "Delete selected configuration", "Are you sure delete the selected configuration?");
            }

            if (chkWindowResult)
            {
                chkWindowResult = false;
                for (int i = ridbtList.Count - 1; i >= 0; i--)
                {
                    if (ridbtList[i].active)
                    {
                        configXML.DeleteVariables(ridbtList[i].text);
                        ridbtList.Remove(ridbtList[i]);
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        public void OnMsgWindow(int msgWindowID)
        {
            GUILayout.BeginHorizontal();

            GUI.Label(new Rect(20, msgWindowRectangle.height / 2 - 15, msgWindowRectangle.width - 60, 20), msgText);

            // Close window button
            //if (GUI.Button(new Rect(180, 3, 17, 15), "x"))
            if (GUI.Button(new Rect(msgWindowRectangle.width - 20, 3, 17, 15), "x"))
            {
                msgWindowActive = false;
            }
            // set current position and rotation of endeffector
            //if (GUI.Button(new Rect((200 - 45) / 2, 70, 45, 25), "OK"))
            if (GUI.Button(new Rect((msgWindowRectangle.width - 45) / 2, 70, 45, 25), "OK"))
            {
                msgWindowActive = false;
            }

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        public void OnChkWindow(int chkWindowID)
        {
            chkWindowResult = false;

            GUILayout.BeginHorizontal();

            GUI.Label(new Rect(20, chkWindowRectangle.height / 2 - 15, chkWindowRectangle.width - 60, 20), chkText);

            // Close window button
            if (GUI.Button(new Rect(chkWindowRectangle.width - 20, 3, 17, 15), "x"))
            {
                chkWindowActive = false;
            }
            if (GUI.Button(new Rect((chkWindowRectangle.width - 45) / 2 - 30, 70, 45, 25), chkButtonText[0]))
            {
                chkWindowActive = false;
                chkWindowResult = true;
            }
            if (GUI.Button(new Rect((chkWindowRectangle.width - 45) / 2 + 30, 70, 45, 25), chkButtonText[1]))
            {
                chkWindowActive = false;
            }

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        public void MsgWindow(Vector2 pos, string title, string text)
        {
            msgWindowPosition = pos;
            msgWindowTitle = title;
            msgText = text;

            msgWindowActive = true;
        }

        public void ChkWindow(Vector2 pos, string title, string text, string trueButton = "yes", string falseButton = "no")
        {
            chkWindowPosition = pos;
            chkWindowTitle = title;
            chkText = text;
            chkButtonText[0] = trueButton;
            chkButtonText[1] = falseButton;
            chkWindowActive = true;
        }

        private void InitStyles()
        {
            if (redStyle == null)
            {
                redStyle = new GUIStyle(GUI.skin.box);
                redStyle.normal.background = MakeTexWithRound(34, 34, colorControlButtonUpDown);
            }
            if (greenStyle == null)
            {
                greenStyle = new GUIStyle(GUI.skin.box);
                greenStyle.normal.background = MakeTexWithRound(34, 34, colorControlButtonLeftRight);
            }
            if (blueStyle == null)
            {
                blueStyle = new GUIStyle(GUI.skin.box);
                blueStyle.normal.background = MakeTexWithRound(34, 34, colorControlButtonForwardBackward);
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D MakeTexWithRound(int width, int height, Color col)
        {
            Color transColor = new Color(col.r, col.g, col.b, 0f);
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                int x = i % width;
                int y = i / width;
                //Debug.Log("i = " + i + " x = " + x + " y = " + y);
                if (x == 0 || x == (width - 1))
                {
                    if (y == 0 || y == (height - 1) || y == 1 || y == (height - 2))
                    {
                        pix[i] = transColor;
                    }
                    else
                        pix[i] = col;
                }
                else if (x == 1 || x == (width - 2))
                {
                    if (y == 0 || y == (height - 1))
                    {
                        pix[i] = transColor;
                    }
                    else
                        pix[i] = col;
                }
                else
                    pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public class BoolText
        {
            public bool active = false;
            public string text = "";

            public BoolText()
            {
                active = false;
                text = "";
            }

            public BoolText(bool active, string text)
            {
                this.active = active;
                this.text = text;
            }
        }

        public class ThetaIOField
        {
            public ThetaIOField()
            {
            }
        }

        private Vector2 ToggleListScrollScope(List<BoolText> boolTextList, Rect position, Vector2 scrollPosition, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
        {
            float maxTextLength = 0f;

            foreach (BoolText element in boolTextList)
            {
                float textLen = (float)(element.text.Length + 1) * 9f;
                if (textLen >= maxTextLength)
                    maxTextLength = textLen;
            }

            using (var scrollScope = new GUI.ScrollViewScope(new Rect(position.x + 1, position.y + 2, position.width - 3, position.height - 3), scrollPosition, new Rect(0, 0, (10 + 20 + maxTextLength), (float)(boolTextList.Count * 18)), alwaysShowHorizontal, alwaysShowVertical))
            {

                scrollPosition = scrollScope.scrollPosition;

                int i = 0;
                foreach (BoolText element in boolTextList)
                {
                    float textLength = (float)(element.text.Length + 1) * 9f;
                    element.active = GUI.Toggle(new Rect(10, (i * 18), 20 + textLength, 20), element.active, " " + element.text);
                    i++;
                }
            }

            DrawRectangle(position, Color.black);
            DrawRectangle(new Rect(position.x + 2, position.y + 2, position.width - 20, position.height - 20), Color.black);
            DrawRectangle(new Rect(position.x + position.width - 15, position.y + position.height - 15, 13, 13), Color.black);

            return scrollPosition;
        }

        //private Vector2 ThetaListScrollScope(List<ThetaIOField> thetaList, Rect position, Vector2 scrollPosition, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false)
        //{
        //    float maxTextLength = 0f;

        //    foreach (ThetaIOField element in thetaList)
        //    {
        //        float textLen = (float)(element.text.Length + 1) * 9f;
        //        if (textLen >= maxTextLength)
        //            maxTextLength = textLen;
        //    }

        //    using (var scrollScope = new GUI.ScrollViewScope(new Rect(position.x + 1, position.y + 2, position.width - 3, position.height - 3), scrollPosition, new Rect(0, 0, (10 + 20 + maxTextLength), (float)(boolTextList.Count * 18)), alwaysShowHorizontal, alwaysShowVertical))
        //    {

        //        scrollPosition = scrollScope.scrollPosition;

        //        int i = 0;
        //        foreach (ThetaIOField element in thetaList)
        //        {
        //            float textLength = (float)(element.text.Length + 1) * 9f;
        //            element.active = GUI.Toggle(new Rect(10, (i * 18), 20 + textLength, 20), element.active, " " + element.text);
        //            i++;
        //        }
        //    }

        //    DrawRectangle(position, Color.black);
        //    DrawRectangle(new Rect(position.x + 2, position.y + 2, position.width - 20, position.height - 20), Color.black);
        //    DrawRectangle(new Rect(position.x + position.width - 15, position.y + position.height - 15, 13, 13), Color.black);

        //    return scrollPosition;
        //}

        public bool AddRepeatButton(Rect rectangle, string icon, float offset = 20f)
        {
            bool valueInBool = false;
            valueInBool = GUI.RepeatButton(new Rect(rectangle.x, rectangle.y + Yoffset, rectangle.width, rectangle.height), icon);
            Yoffset += offset;
            return valueInBool;
        }

        public void AddInputValue(Rect rectangle, string valueInString, out string valueOutString, string name, out int valueInt, float labelLength)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            valueOutString = GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, rectangle.width, rectangle.height), valueInString);
            try
            {
                valueInt = int.Parse(valueOutString);
            }
            catch
            {
                valueOutString = valueInString;
                valueInt = int.Parse(valueOutString);
            }
            Yoffset += 20f;
        }

        public void AddInputValue(Rect rectangle, string valueInString, out string valueOutString, string name, out float valueFloat, float labelLength)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            valueOutString = GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, rectangle.width, rectangle.height), valueInString);
            try
            {
                valueFloat = float.Parse(valueOutString);
            }
            catch
            {
                valueOutString = valueInString;
                valueFloat = float.Parse(valueOutString);
            }
            Yoffset += 20f;
        }

        public void AddOutputValue(Rect rectangle, string name, float valueFloat, float labelLength)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, rectangle.width, rectangle.height), valueFloat.ToString("0.000"));
            Yoffset += 20f;
        }

        public void AddOutputValue(Rect rectangle, string name, float valueFloat, float labelLength, string format)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, rectangle.width, rectangle.height), valueFloat.ToString(format));
            Yoffset += 20f;
        }

        public void AddOutputValue(Rect rectangle, string name, float valueFloat, float labelLength, float fieldLength)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, fieldLength, rectangle.height), valueFloat.ToString("0.000"));
            Yoffset += 20f;
        }

        private void AddOutputValue(Rect rectangle, string name, Vector3 valueVector3, float labelLength, float fieldLength)
        {
            GUI.Label(new Rect(rectangle.x, rectangle.y + Yoffset, labelLength, rectangle.height), name);
            GUI.TextField(new Rect(rectangle.x + labelLength, rectangle.y + Yoffset, rectangle.width + fieldLength, rectangle.height), VectorToString(valueVector3, "0.00"));
            Yoffset += 20f;
        }

        private float FixValuesSlider(Rect rectangle, float nameLenght, string name, float value, float[] Values)
        {
            float mindiff = Values[Values.Length - 1] - Values[0];
            int index = 0;
            Rect valueRect = rectangle;
            valueRect.x += valueRect.width + nameLenght + 5;
            GUI.Label(rectangle, name);
            GUI.Label(valueRect, value.ToString());
            rectangle.x += nameLenght;
            rectangle.y += 7;
            value = GUI.HorizontalSlider(rectangle, value, Values[0], Values[Values.Length - 1]);
            for (int i = 0; i < Values.Length; i++)
            {
                if (Math.Abs(value - Values[i]) < mindiff)
                {
                    mindiff = Math.Abs(value - Values[i]);
                    index = i;
                }
            }
            value = Values[index];

            return value;
        }

        private float FixValuesLogSlider(Rect rectangle, float nameLenght, string name, float value, float[] Values)
        {
            float[] LogValues = new float[Values.Length];

            for (int i = 0; i < Values.Length; i++)
            {
                if (i == 0)
                    LogValues[0] = Mathf.Log10((Values[1] / 2f));
                else
                    LogValues[i] = Mathf.Log10(Values[i]);
            }

            float mindiff = Values[Values.Length - 1] - Values[0];
            int index = 0;
            Rect valueRect = rectangle;

            valueRect.x += valueRect.width + nameLenght + 5f;
            GUI.Label(rectangle, name);
            GUI.Label(valueRect, value.ToString());
            rectangle.x += nameLenght;

            Rect fixLine = new Rect(rectangle);
            fixLine.width = 10f;
            fixLine.y += 2f;
            Rect fixLineRect = new Rect(fixLine);

            for (int i = 0; i < LogValues.Length; i++)
            {
                float linePosition = (rectangle.width - 12f) / Mathf.Abs(LogValues[0] - LogValues[LogValues.Length - 1]) * (LogValues[i] + LogValues[0] * -1f) + 2f;
                fixLineRect.x = fixLine.x + linePosition;
                GUI.Label(fixLineRect, "│");
            }

            rectangle.y += 6f;
            float valueLog;
            if (value == 0f)
                valueLog = LogValues[0];
            else
                valueLog = Mathf.Log10(value);

            valueLog = GUI.HorizontalSlider(rectangle, valueLog, LogValues[0], LogValues[Values.Length - 1]);
            value = (float)Math.Pow(10d, (double)valueLog);
            for (int i = 0; i < Values.Length; i++)
            {
                if (Math.Abs(value - Values[i]) < mindiff)
                {
                    mindiff = Math.Abs(value - Values[i]);
                    index = i;
                }
            }
            if (index == 0)
                value = 0f;
            else
                value = Values[index];

            return value;
        }

        private void DrawRectangle(Rect rectangle, Color color)
        {
            Drawing.DrawLine(new Vector2(rectangle.x, rectangle.y), new Vector2(rectangle.x + rectangle.width, rectangle.y), color);
            Drawing.DrawLine(new Vector2(rectangle.x, rectangle.y), new Vector2(rectangle.x, rectangle.y + rectangle.height), color);
            Drawing.DrawLine(new Vector2(rectangle.x + rectangle.width, rectangle.y), new Vector2(rectangle.x + rectangle.width, rectangle.y + rectangle.height), color);
            Drawing.DrawLine(new Vector2(rectangle.x, rectangle.y + rectangle.height), new Vector2(rectangle.x + rectangle.width, rectangle.y + rectangle.height), color);
        }

        private void LineDiagram(Rect rectangle, List<float> data, Color color)
        {
            // set border deathzone
            Vector2 border = new Vector2(5, 5);
            Rect realDataField = new Rect(new Vector2(rectangle.x + border.x, rectangle.y + border.y), new Vector2(rectangle.width - (2 * border.x), rectangle.height - (2 * border.y)));
            Rect activeDataField = new Rect(new Vector2(0, 0), new Vector2(rectangle.width - (2 * border.x), rectangle.height - (2 * border.y)));
            // draw border line
            DrawRectangle(rectangle, Color.white);

            //DrawRectangle (realDataField, Color.yellow);

            GUI.BeginGroup(realDataField);
            //GUILayout.BeginArea(realDataField);

            // draw diagram data
            if (data.Count > 1)
            {
                for (int i = 1; i < data.Count; i++)
                {
                    Drawing.DrawLine(new Vector2((float)(i - 1), activeDataField.height - data[i - 1]), new Vector2((float)(i), activeDataField.height - data[i]), color);
                }
            }

            GUI.EndGroup();
            //GUILayout.EndArea();
        }

        private void LineDiagram(Rect rectangle, List<List<float>> data, Color[] color)
        {
            // set border deathzone
            Vector2 border = new Vector2(5, 5);
            Rect realDataField = new Rect(new Vector2(rectangle.x + border.x, rectangle.y + border.y), new Vector2(rectangle.width - (2 * border.x), rectangle.height - (2 * border.y)));
            Rect activeDataField = new Rect(new Vector2(0, 0), new Vector2(rectangle.width - (2 * border.x), rectangle.height - (2 * border.y)));
            // draw border line
            DrawRectangle(rectangle, Color.white);

            //DrawRectangle (realDataField, Color.yellow);

            GUI.BeginGroup(realDataField);
            //GUILayout.BeginArea(realDataField);

            if (data.Count > 0)
                for (int j = 0; j < data.Count; j++)
                {
                    // draw diagram data
                    if (data[j].Count > 1)
                    {
                        for (int i = 1; i < data[j].Count; i++)
                        {
                            Drawing.DrawLine(new Vector2((float)(i - 1), activeDataField.height - (data[j][i - 1] + 1)), new Vector2((float)(i), activeDataField.height - (data[j][i] + 1)), color[j]);
                        }
                    }
                }

            GUI.EndGroup();
            //GUILayout.EndArea();
        }


        //static System.Reflection.FieldInfo s_focusedWindow = null;

        //static public int FocusedWindow
        //{
        //    get
        //    {
        //        if (s_focusedWindow == null)
        //        {
        //            s_focusedWindow = typeof(GUI).GetField("focusedWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        //        }

        //        return (int)s_focusedWindow.GetValue(null);
        //    }
        //}

        #endregion GUI

        #region Tools
        private float ArmWorkingDistance()
        {
            float armWorkingDistance;

            armWorkingDistance = Mathf.Abs(Vector3.Distance(globalPosition, part.transform.position));

            return armWorkingDistance;
        }

        private bool TargetInRange(float range)
        {
            float distance = 0f;
            bool InRange = false;

            distance = Mathf.Abs(Vector3.Distance(globalPosition, part.transform.position));
            //Debug.Log(string.Format("[IKRC] - Update() - TargetInRange({0})  distance = " + distance.ToString(), range));

            InRange = distance <= range;
            //Debug.Log(string.Format("[IKRC] - Update() - TargetInRange({0}) = " + (InRange ? "true" : "false"), range));

            return InRange;
        }

        private Vector3 TargetToRange(float range)
        {
            Vector3 target = globalPosition;
            float scale = 1.0f;

            scale = range / Mathf.Abs(Vector3.Distance(globalPosition, part.transform.position));
            target = Vector3.Lerp(part.transform.position, globalPosition, scale);

            return target;
        }

        private bool ImplementServoRotation(List<IKservo> Servos, float[] theta, float speed = 0.25f)
        {
            // A SortIkServo helyett a Servos kellene használni !!!???

            bool success = true;
            float servoTheta = 0f;

            for (int j = 0; j < ArmIkServos.Count - 1; j++)
                if (!double.IsNaN(theta[j]))
                {
                    // blocking fullrotate of servo
                    if (theta[j] > 180f)
                        servoTheta = theta[j] - 360f;
                    else if (theta[j] < -180f)
                        servoTheta = theta[j] + 360f;
                    else
                        servoTheta = theta[j];
                    // eliminating bouncing noise - if servo target position the same that current position then don't send move command.
                    if (servoTheta != ArmIkServos[j].iservo.Position)
                    {
                        //SortIkServo[j].iservo.SetModulo(SLTBreaking);
                        ArmIkServos[j].iservo.MoveTo(servoTheta, speed);
                    }
                }
                else
                    success = false;
            return success;
        }

        private bool ImplementNormalServoRotation(List<IKservo> Servos, float[] currentTheta, float[] theta, float maxspeed)
        {
            // A SortIkServo helyett a Servos kellene használni !!!???

            bool success = true;
            float[] servoTheta;
            float[] diffTheta;
            float speed = 0f;

            diffTheta = new float[ArmIkServos.Count];
            servoTheta = new float[ArmIkServos.Count];

            for (int j = 0; j < ArmIkServos.Count - 1; j++)
            {
                if (!double.IsNaN(theta[j]))
                {
                    // blocking fullrotate of servo
                    if (theta[j] > 180f)
                        servoTheta[j] = theta[j] - 360f;
                    else if (theta[j] < -180f)
                        servoTheta[j] = theta[j] + 360f;
                    else
                        servoTheta[j] = theta[j];

                    //diffTheta[j] = Math.Abs(theta[j] - currentTheta[j]);
                    diffTheta[j] = Math.Abs(servoTheta[j] - currentTheta[j]);
                }
                else
                    success = false;
            }

            float maxTheta = diffTheta.Max();

            for (int j = 0; j < ArmIkServos.Count - 1; j++)
            {
                speed = diffTheta[j] / maxTheta * maxspeed;
                // eliminating bouncing noise - if servo target position the same that current position then don't send move command. 
                if (servoTheta[j] != ArmIkServos[j].iservo.Position)
                {
                    //SortIkServo[j].iservo.SetModulo(SLTBreaking);
                    ArmIkServos[j].iservo.MoveTo(servoTheta[j], speed);
                }
            }

            return success;
        }

        private void FloatArrayReverse(ref float[] array, int from, int length)
        {
            float temp;

            for (int i = 0; i < ((float)length % 2f == 0 ? length / 2 : length / 2 + 1); i++)
            {
                temp = array[i + from];
                array[i + from] = array[length + from - 1 - i];
                array[length + from - 1 - i] = temp;
            }
        }

        private float[] FloatArrayReverse(float[] array, int from, int length)
        {
            float[] result = new float[array.Length];
            float temp;
            int j = 0;

            foreach (float element in array)
            {
                result[j] = element;
                j++;
            }

            for (int i = 0; i < ((float)length % 2f == 0 ? length / 2 : length / 2 + 1); i++)
            {
                temp = result[i + from];
                result[i + from] = result[length + from - 1 - i];
                result[length + from - 1 - i] = temp;
            }

            return result;
        }

        //private bool ImplementServoSwap(List<IKservo> Servos, ref float[] theta)
        //{
        //    bool success = true;
        //    float servoTheta = 0f;

        //    FloatArrayReverse(ref theta, 0, Servos.Count - 1);

        //    for (int j = 0; j < Servos.Count - 1; j++)
        //    {
        //        if (!double.IsNaN(theta[j]))
        //        {
        //            // blocking fullrotate of servo
        //            if (theta[j] > 180f)
        //                servoTheta = theta[j] - 360f;
        //            else if (theta[j] < -180f)
        //                servoTheta = theta[j] + 360f;
        //            else
        //                servoTheta = theta[j];
        //            // eliminating bouncing noise - if servo target position the same that current position then don't send move command.
        //            if (servoTheta != Servos[j].iservo.Position)
        //            {
        //                Servos[j].iservo.SetTo(servoTheta);
        //            }
        //        }
        //        else
        //            success = false;
        //    }
        //    return success;
        //}

        private bool ImplementIndepServoRotation(List<IKservo> Servos, float[] iota, float speed = 0.25f)
        {
            bool success = true;
            float servoIota = 0f;

            for (int j = 0; j < Servos.Count; j++)
                if (!double.IsNaN(iota[j]))
                {
                    // blocking fullrotate of servo
                    if (iota[j] > 180f)
                        servoIota = iota[j] - 360f;
                    else if (iota[j] < -180f)
                        servoIota = iota[j] + 360f;
                    else
                        servoIota = iota[j];
                    // eliminating bouncing noise - if servo target position the same that current position then don't send move command.
                    if (servoIota != Servos[j].iservo.Position)
                    {
                        Servos[j].iservo.MoveTo(servoIota, speed);
                    }
                }
                else
                    success = false;
            return success;
        }

        private float[] GetActualServoRotation(List<IKservo> Servos)
        {
            float[] currentTheta = new float[JointList.Length];

            for (int j = 0; j < ArmIkServos.Count - 1; j++)
                currentTheta[j] = ArmIkServos[j].iservo.Position;

            return currentTheta;
        }

        private float[] GetActualIndepServoRotation(List<IKservo> Servos)
        {
            float[] currentIota = new float[Servos.Count];

            for (int j = 0; j < Servos.Count; j++)
                currentIota[j] = Servos[j].iservo.Position;

            return currentIota;
        }

        private void GetChildPartRecursive(Part obj)
        {
            if (null == obj)
                return;

            foreach (Part child in obj.children)
            {
                if (null == child)
                    continue;
                listOfChildrenPart.Add(child);
                GetChildPartRecursive(child);
            }
        }
        #endregion Tools

        #region ..ToSting
        private static string VectorToString(Vector3 vector, string format)
        {
            string szoveg = "";

            szoveg = string.Format("( " + vector.x.ToString(format) + ", " + vector.y.ToString(format) + ", " + vector.z.ToString(format) + " )");

            return szoveg;
        }

        private static string VectorToString(Vector2 vector, string format)
        {
            string szoveg = "";

            szoveg = string.Format("( " + vector.x.ToString(format) + ", " + vector.y.ToString(format) + " )");

            return szoveg;
        }

        private static string QuaternionToString(Quaternion quaternion, string format)
        {
            string szoveg = "";

            szoveg = string.Format("( " + quaternion.w.ToString(format) + ", " + quaternion.x.ToString(format) + ", " + quaternion.y.ToString(format) + ", " + quaternion.z.ToString(format) + " )");

            return szoveg;
        }

        private static string IKServoToString(IKservo ikServo)
        {
            string szoveg = "";

            szoveg = string.Format("ServoTransform = " + VectorToString(ikServo.ServoTransform.position, "0.00") + " " + QuaternionToString(ikServo.ServoTransform.rotation, "0.00"));

            return szoveg;
        }
        #endregion #region ..ToSting

        #region Kinematics
        public bool InverseKinematics(Vector3 targetPosition, Quaternion targetOrientation, float[] angles)
        {
            // calculate different of distance and -rotation
            LocationRotationDiff locRotDiff = DistanceAndAngleFromTarget(targetPosition, targetOrientation, angles);

            Distance = locRotDiff.LocationDiff;
            Angle = locRotDiff.RotationDiff;

            // if different lower than threshold (distance and rotation) then stop
            if (locRotDiff.LocationDiff < DistanceThreshold && locRotDiff.RotationDiff < AngleThreshold)
                return true;

            for (int i = 0; i < ArmIkServos.Count; i++)
            {
                // Gradient descent
                // Update : Solution -= LearningRate * Gradient

                // calculate partial gradient
                LocationRotationDiff gradient = PartialGradient(targetPosition, targetOrientation, angles, i);

                // calculate dynamic learning rate of location different 
                DinLearningRatePos = locRotDiff.LocationDiff / MaxPosErr * 100.0f;
                // calculate dynamic learning rate of rotation different
                DinLearningRateOri = locRotDiff.RotationDiff / MaxOriErr * 100.0f;

                // calculate angle of joint with location gradient and rotation gradient - why sum ???
                angles[i] -= DinLearningRatePos * gradient.LocationDiff + DinLearningRateOri * gradient.RotationDiff;

                // Clamp - limited angle of joint
                angles[i] = Mathf.Clamp(angles[i], ArmIkServos[i].MinAngle, ArmIkServos[i].MaxAngle);

                // calculate different of distance and -rotation
                locRotDiff = DistanceAndAngleFromTarget(targetPosition, targetOrientation, angles);

                Distance = locRotDiff.LocationDiff;
                Angle = locRotDiff.RotationDiff;

                // if different lower than threshold (distance and rotation) then stop
                if (locRotDiff.LocationDiff < DistanceThreshold && locRotDiff.RotationDiff < AngleThreshold)
                    return true;
            }
            return false;
        }

        public LocationRotationDiff PartialGradient(Vector3 targetPosition, Quaternion targetOrientation, float[] angles, int i)
        {
            // Saves the angle,
            // it will be restored later
            float angle = angles[i];
            LocationRotationDiff gradient = new LocationRotationDiff();

            // calculate different
            LocationRotationDiff f_xGlobal = DistanceAndAngleFromTarget(targetPosition, targetOrientation, angles);

            // change angles with samplingangle (a little bit)
            // Use MinAngle & MaxAngle limits in change ?!
            angles[i] += SamplingAngle;

            // recalculate different
            LocationRotationDiff f_x_plus_dGlobal = DistanceAndAngleFromTarget(targetPosition, targetOrientation, angles);

            // Gradient : [F(x+SamplingDistance) - F(x)] / h
            // calculate gradient of position
            gradient.LocationDiff = (f_x_plus_dGlobal.LocationDiff - f_xGlobal.LocationDiff) / SamplingAngle;
            // calculate gradient of orientation
            gradient.RotationDiff = (f_x_plus_dGlobal.RotationDiff - f_xGlobal.RotationDiff) / SamplingAngle;

            // Restore angles
            angles[i] = angle;

            return gradient;
        }

        public LocationRotationDiff DistanceAndAngleFromTarget(Vector3 targetPosition, Quaternion targetOrientation, float[] angles)
        {
            LocationRotationDiff different = new LocationRotationDiff();

            VectorSM DistanceAndAngle = ForwardKinematics(angles, ArmIkServos, IkBasePart);

            different.LocationDiff = Vector3.Distance(DistanceAndAngle.Translation, targetPosition);

            different.RotationDiff = Quaternion.Angle(DistanceAndAngle.Rotation, targetOrientation);

            return different;
        }

        public VectorSM ForwardKinematics(float[] angles, List<IKservo> Servos, Part IKBasePart)
        {
            // Used parameters of servo
            // angles[] - actual angle of servo
            // Servos[].fkParams.Axis - define rotation axis of servo
            // Servos[].fkParams.Rotation - global rotation of servo
            // Servos[].fkParams.ParentOffset - position offset for parentpart

            VectorSM endEffector = new VectorSM();
            Vector3 prevPoint = Servos[0].ServoTransform.position;

            //Quaternion rotation = Servos[0].ServoTransform.rotation * Quaternion.Inverse(Servos[0].fkParams.Rotation);
            Quaternion rotation = IkBasePart.transform.rotation * Quaternion.Inverse(Servos[0].fkParams.PartRotation);

            for (int i = 1; i < Servos.Count; i++)
            {
                // change rotation by previous servo angle - 'relative change'
                rotation *= Quaternion.AngleAxis(angles[i - 1], Servos[i - 1].fkParams.Axis);
                // change position by ( servo's ParentOffset modified by previous servo angle ) - 'relative change'
                Vector3 nextPoint = prevPoint + rotation * Servos[i - 1].fkParams.Rotation * Servos[i].fkParams.ParentOffset;
                // set position of servotransform's sign
                servoGimbal[i - 1].transform.position = prevPoint;
                // set rotation of servotransform's sign
                servoGimbal[i - 1].transform.rotation = rotation * Servos[i - 1].fkParams.Rotation;

                prevPoint = nextPoint;
            }
            // set endeffector position data
            endEffector.Translation = prevPoint;
            // set endeffector rotation data
            endEffector.Rotation = rotation * Servos[Servos.Count - 1].fkParams.Rotation;

            // set position of EEFservotransform's sign
            servoGimbal[Servos.Count - 1].transform.position = endEffector.Translation;
            // set rotation of EEFservotransform's sign
            servoGimbal[Servos.Count - 1].transform.rotation = endEffector.Rotation;

            return endEffector;
        }

        public VectorSM ForwardKinematicsWithLog(float[] angles, List<IKservo> Servos, Part IKBasePart)
        {
            // Used parameters of servo
            // angles[] - actual angle of servo
            // Servos[].fkParams.Axis - define rotation axis of servo
            // Servos[].fkParams.Rotation - global rotation of servo
            // Servos[].fkParams.ParentOffset - position offset for parentpart

            VectorSM endEffector = new VectorSM();
            Vector3 prevPoint = Servos[0].ServoTransform.position;

            //Quaternion rotation = part.transform.rotation * Quaternion.Inverse(partRotation); // partRotation => part.transform.rotation in Dumping State = ArmNDIkServos[0].fkParams.PartRotation
            Quaternion rotation = IKBasePart.transform.rotation * Quaternion.Inverse(Servos[0].fkParams.PartRotation);

            for (int i = 1; i < Servos.Count; i++)
            {
                // change rotation by previous servo angle - 'relative change'
                rotation *= Quaternion.AngleAxis(angles[i - 1], Servos[i - 1].fkParams.Axis);
                // change position by ( servo's ParentOffset modified by previous servo angle ) - 'relative change'
                Vector3 nextPoint = prevPoint + rotation * Servos[i - 1].fkParams.Rotation * Servos[i].fkParams.ParentOffset;
                // set position of servotransform's sign
                servoGimbal[i - 1].transform.position = prevPoint;
                // set rotation of servotransform's sign
                servoGimbal[i - 1].transform.rotation = rotation * Servos[i - 1].fkParams.Rotation;

                prevPoint = nextPoint;
            }
            // set endeffector position data
            endEffector.Translation = prevPoint;
            // set endeffector rotation data
            endEffector.Rotation = rotation * Servos[Servos.Count - 1].fkParams.Rotation;

            // set position of EEFservotransform's sign
            servoGimbal[Servos.Count - 1].transform.position = endEffector.Translation;
            // set rotation of EEFservotransform's sign
            servoGimbal[Servos.Count - 1].transform.rotation = endEffector.Rotation;

            return endEffector;
        }
        #endregion Kinematics
    }
}
