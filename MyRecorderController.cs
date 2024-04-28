using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.Linq;
using UnityEditor.Animations;

[System.Serializable]
public class ActionOptions {
    // [SerializeField]
    public string action = "";
    [SerializeField]
    public bool capture = true;
    [SerializeField]
    public bool applyRootMotion = true;

}

[CustomPropertyDrawer(typeof(ActionOptions))]
public class ActionDrawer : PropertyDrawer 
{
    private SerializedProperty action;
    private SerializedProperty capture;
    private SerializedProperty applyRootMotion;

    private SerializedProperty FramesDataByActionState;
    private SerializedProperty actionsList;
    private SerializedProperty actionOptions;
    private int i = 0;
    //  void OnEnable()
    // {
    // }

    //how to draw to inspector window 
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    
        action = property.FindPropertyRelative("action");
        capture = property.FindPropertyRelative("capture");
        applyRootMotion = property.FindPropertyRelative("applyRootMotion");
 
        FramesDataByActionState = property.serializedObject.FindProperty("FramesDataByActionState");
        actionsList = property.serializedObject.FindProperty("XmlOutputActions");
        actionOptions = property.serializedObject.FindProperty("actionOptions");

        EditorGUI.BeginProperty(position, label, property);        
            Rect foldOutBox = new Rect(position.min.x, position.min.y, position.size.x * 0.33f, EditorGUIUtility.singleLineHeight);
            EditorGUI.Foldout(foldOutBox, property.isExpanded, action.stringValue);
            DrawCaptureProperty(position);
            DrawApplyMotionProperty(position);
        EditorGUI.EndProperty();
    }

    private void DrawCaptureProperty (Rect position) {
        EditorGUIUtility. labelWidth = 60;
        float x = position.min.x + (position.size.x * .33f);
        float y = position.min.y;
        float w = position.size.x * .33f;
        float h = EditorGUIUtility.singleLineHeight;

        Rect drawArea = new Rect(x, y, w, h);
        EditorGUI.PropertyField(drawArea, capture, new GUIContent("Capture"));
        
    }

    private void DrawApplyMotionProperty (Rect position) {
        EditorGUIUtility. labelWidth = 80;
                /// rect
        float x = position.min.x + (position.size.x * .66f);
        float y = position.min.y;
        float w = position.size.x * .33f;
        float h = EditorGUIUtility.singleLineHeight;

        Rect drawArea = new Rect(x, y, w, h);
        EditorGUI.PropertyField(drawArea, applyRootMotion, new GUIContent("Root Motion"));
    }

    // request more vertical spacing, return it
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        int totalLines = 1;

        return totalLines * EditorGUIUtility.singleLineHeight; 
    }    
}

[CustomEditor(typeof(MyRecorderController))]
public class DataEditor : Editor 
{   
    private SerializedProperty debug;
    private SerializedProperty output; 
    private SerializedProperty enable;

    private SerializedProperty seperateFolders;
    private SerializedProperty replace;
    private SerializedProperty itemName;
    private SerializedProperty directions;
    private SerializedProperty size; 
    private SerializedProperty targetFPS; 
    private SerializedProperty minFrames; 
    private SerializedProperty maxFrames; 
    private SerializedProperty applyRootMotion; 
    private SerializedProperty FramesDataByActionState;

    private SerializedProperty Actions;
    private SerializedProperty actionIndex;
    private SerializedProperty actionCount;

    private SerializedProperty actionsList;
    private SerializedProperty actionOptions;
    private SerializedProperty name;
    private SerializedProperty capture;
    private SerializedProperty rootMotion;

    void OnEnable()
    {
        debug = serializedObject.FindProperty("debug");
        output = serializedObject.FindProperty("output");
        enable = serializedObject.FindProperty("enable");

        seperateFolders = serializedObject.FindProperty("seperateFolders");
        replace = serializedObject.FindProperty("replace");
        itemName = serializedObject.FindProperty("itemName");
        directions = serializedObject.FindProperty("directions");
        size = serializedObject.FindProperty("size");
        targetFPS = serializedObject.FindProperty("targetFPS");
        minFrames = serializedObject.FindProperty("minFrames");
        maxFrames = serializedObject.FindProperty("maxFrames");
        applyRootMotion = serializedObject.FindProperty("ApplyRootMotionAll");
        FramesDataByActionState = serializedObject.FindProperty("FramesDataByActionState");

        Actions = serializedObject.FindProperty("Actions");
        actionIndex = serializedObject.FindProperty("actionIndex");
        actionCount = serializedObject.FindProperty("numActions");

        actionOptions = serializedObject.FindProperty("actionOptions");
        actionsList = serializedObject.FindProperty("XmlOutputActions");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        
        // fill action array
        Set_Actions(FramesDataByActionState.arraySize);      

        // update action array
        Update_Actions();
        
        EditorGUILayout.Space(15);
        DrawProgressBar(actionIndex.intValue, actionCount.intValue);
        
        EditorGUILayout.LabelField("OPTIONS:", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(enable, new GUIContent("Enable Script"));
        EditorGUILayout.PropertyField(debug, new GUIContent("Show Debug"));
        EditorGUILayout.PropertyField(output, new GUIContent("Show Output"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("ANIMATION OUTPUT:", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        EditorGUIUtility.labelWidth = 150;
        EditorGUILayout.PropertyField(seperateFolders, new GUIContent("Seperate Folders"));        
        EditorGUILayout.PropertyField(replace, new GUIContent("Replace"));        
        EditorGUILayout.PropertyField(itemName, new GUIContent("Item Name"));        
        EditorGUILayout.PropertyField(directions, new GUIContent("Capture Directions"));        
        EditorGUILayout.PropertyField(size, new GUIContent("Resolution"));

        EditorGUILayout.PropertyField(targetFPS, new GUIContent("Target FPS"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(minFrames, new GUIContent("Min/Max Frames"));
        EditorGUIUtility.labelWidth = 40;
        EditorGUILayout.PropertyField(maxFrames, new GUIContent("-"));
        EditorGUILayout.EndHorizontal();

        if (maxFrames.intValue < minFrames.intValue)
            maxFrames.intValue = minFrames.intValue;
        else if (minFrames.intValue > maxFrames.intValue)
            minFrames.intValue = maxFrames.intValue;
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("ACTION STATES:", EditorStyles.boldLabel);
        Buttons();
        EditorGUILayout.PropertyField(FramesDataByActionState, new GUIContent("Actions"));

        
        serializedObject.ApplyModifiedProperties();
    }

    void Update_Actions() {
        for (int i = 0; i < FramesDataByActionState.arraySize; i++) {
            if (i >= actionsList.arraySize) {
                FramesDataByActionState.DeleteArrayElementAtIndex(i);
                break;
            }
            name = FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("action");
            name.stringValue = actionsList.GetArrayElementAtIndex(i).stringValue;
        }
    }

    void Set_Actions(int startIndex) {
        for (int i = startIndex; i < actionsList.arraySize; i++) {
            if (i >= actionsList.arraySize) break;
            FramesDataByActionState.InsertArrayElementAtIndex(i);

            name = FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("action");
            name.stringValue = actionsList.GetArrayElementAtIndex(i).stringValue;
            
            capture = FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("capture");
            if (name.stringValue == "idle" || 
                name.stringValue == "run" || 
                name.stringValue == "combatIdle" || 
                name.stringValue == "kneel" || 
                name.stringValue == "struck1" || 
                name.stringValue == "singleStrike1" || 
                name.stringValue == "singleStrike2" || 
                name.stringValue == "castStart" || 
                name.stringValue == "castEnd" || 
                name.stringValue == "summon" ||
                name.stringValue == "death")
            {
                capture.boolValue = true;
            }
            else {
                capture.boolValue = false;
            }

            rootMotion = FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("applyRootMotion");
            if (name.stringValue == "dodge" || 
                name.stringValue == "idle" || 
                name.stringValue == "combatIdle" || 
                name.stringValue == "thunderClap" || 
                name.stringValue == "unsheath1h" || 
                name.stringValue == "attack1h" || 
                name.stringValue == "unsheath2h" || 
                name.stringValue == "singleStrike1" || 
                name.stringValue == "singleStrike2" || 
                name.stringValue == "shootBow" || 
                name.stringValue == "summon" || 
                name.stringValue == "death") 
            {
                rootMotion.boolValue = true;
            }
            else {
                rootMotion.boolValue = false;
            }
        }
    }

    void DrawProgressBar(int actionIndex, int size){
        Rect rect = GUILayoutUtility.GetRect(18,18);

        string progress = "Progress...";
        if (!enable.boolValue) progress = "Done!";
        EditorGUI.ProgressBar(rect, (float)actionIndex/(float)size, progress);
        EditorGUILayout.Space(15);
    }

    void Buttons() {
        EditorGUILayout.BeginHorizontal();
        int n = 8;
        EditorGUI.indentLevel += n;
        int w = 55;
        if (GUILayout.Button("All", GUILayout.Width(w))) {
                for (int i = 0; i < FramesDataByActionState.arraySize; i++) {
                FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("capture").boolValue = true;
            }
        }
        else if (GUILayout.Button("None", GUILayout.Width(w))) {
                for (int i = 0; i < FramesDataByActionState.arraySize; i++) {
                FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("capture").boolValue = false;
            }
        }
        else if (GUILayout.Button("All", GUILayout.Width(w))) {
                for (int i = 0; i < FramesDataByActionState.arraySize; i++) {
                FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("applyRootMotion").boolValue = true; 
            }
        }
        else if (GUILayout.Button("None", GUILayout.Width(w))) {
                for (int i = 0; i < FramesDataByActionState.arraySize; i++) {
                FramesDataByActionState.GetArrayElementAtIndex(i).FindPropertyRelative("applyRootMotion").boolValue = false;
            }
        }
        else if (GUILayout.Button("Default", GUILayout.Width(w))) {
            Set_Actions(0); 
        }
        EditorGUI.indentLevel -= n;
        EditorGUILayout.EndHorizontal();
    }
}


public class MyRecorderController : MonoBehaviour
{
    // //list of Cameras
    // int numCameras = 0;
    // public List<GameObject> cameras;
    // public GameObject plane;

    // void Create_Cameras() {
    //     for (int i = 0; i < numCameras; i++)
    //     {
    //         cameras[i] = new GameObject();            
    //         //get transform
    //         cameras[i].GetComponent<Transform>();
    //             //set position 0, 0, 0
    //             //set rotation
    //                 // x rotation to 45
    //                 // y increments by 22.5 (0 being North of the rotation of the target is 0) 22.5 being 360 / 16 or 360 / numCameras
    //                 // z to 0
            
    //         //attach camera
    //         cameras[i].AddComponent<Camera>();
    //         //set solid color BLACK
    //         //set background  FULL ALPHA
    //         //set distance  
    //         //set clipping planes
    //         //set to orthographic
    //     }

    //     //create plane
    //     plane = new GameObject();
    //     plane.GetComponent<Transform>();
    //     //set position 0, 0, 0
        
    //     //attach Plane(Mesh Filter)
        
    //     //attach mesh render
    //         //set material
        
    //     //attach mesh collider
    //         //set shader
    // }

    public bool debug = false;
    public bool output = false;

    public enum Directions {
        four = 0,
        eight,
        sixteen
    }

    public bool enable = true;
    [HideInInspector]
    public string name = "";
    [HideInInspector]
    public float totalFrames;       
    [HideInInspector]
    public int numFrames;
    [HideInInspector]
    public int finishedCameras = 0;
    [HideInInspector]
    public float defaultFPS = 30.0f;
    [HideInInspector]
    public float outputFPS = 30.0f; 
    
    public bool seperateFolders = false;
    public bool replace = false;
    public string itemName = "";
    public Directions directions = Directions.sixteen;
    public int size = 720; 
    public float targetFPS = 10.0f; 
    public int minFrames = 8; 
    public int maxFrames = 16; 
    public bool CaptureAll;
    public bool ApplyRootMotionAll;
    public List<ActionOptions>FramesDataByActionState;

    [HideInInspector]
    public List<string> Actions;
    [HideInInspector]
    // public List<string> XmlOutputActions;
    public string[] XmlOutputActions = new string[29] {
        "idle",
        "idleBreak01",
        "idleBreak02",
        "combatIdle",
        "kneel",
        "run",
        "walk",
        "struck1",
        "dodge",
        "parry",
        "prayKneelingStart",        
        "prayStandingLoop",        
        "cleave",
        "thunderClap",
        "unsheath1h",
        "attack1h",
        "blockStart",
        "blockLoop",
        "blockStruck",
        "blockEnd",        
        "unsheath2h",
        "singleStrike1", 
        "singleStrike2",        
        "shootBow",         
        "castStart",
        "castLoop",
        "castEnd",
        "summon",         
        "death"};
        


    [HideInInspector]
    public string animation = "Idle";
    [HideInInspector]
    public bool captureDead = false;
    [HideInInspector]
    ActionOptions actionOptions;
    [HideInInspector]
    public int actionIndex = 0;
    [HideInInspector]
    public int numActions = 0;
    [HideInInspector]
    private Animator animator;
    [HideInInspector]
    private string currentState = "";
    [HideInInspector]
    private bool init = false;

    public bool Outside_Array_Bounds(int array_size, int index, string arrayName) {
        if (index >= array_size) {
            if (output) Debug.Log("Failure! index: '" + index + "' array: '" + arrayName + "' size: '" + array_size + "'");
            return true;        
        }
        if (output) Debug.Log("Success! index: '" + index + "' array: '" + arrayName + "' size: '" + array_size + "'");
        return false;
    }

    public void Init() {
        if (!init) {
            if (Outside_Array_Bounds(FramesDataByActionState.Count, actionIndex, "FramesDataByActionState")) {
                if (output) Debug.Log("Script not set on camera!");
                return;
            }

            name = gameObject.name; 
            if (name == "SerpentWarrior") Actions = new List<string> {
                "Idle", "Idlebreak01", "Idlebreak02", "CombatIdle", "Kneeling Down", "Run", "Walk", "GotHit", "Dodge",  "Parry",  
                "PrayKneelingStart", "PrayStandingLoop",
                "Cleave", "Sword And Shield Casting", 
                "Withdrawing Sword", "Attack2",
                "BlockStart", "BlockLoop", "BlockHit", "BlockEnd",                
                "Unarmed Equip Underarm", "Standing Melee Run Jump Attack", "attack2",
                "Shooting Arrow",
                "Cast1 Start", "Cast1 Maintain", "Cast1 End", "Standing 2H Cast Spell 01",
                "Death"
            };

            else if (name == "FishmanLP") Actions = new List<string> {
                "Idle", "Idlebreak01", "Idlebreak02", "CombatIdle", "Kneeling Down", "Run", "Walk", "Get_hit", "Dodge",  "Parry",  
                "PrayKneelingStart", "PrayStandingLoop",
                "Cleave", "Sword And Shield Casting", 
                "Withdrawing Sword", "Attack2",
                "BlockStart", "BlockLoop", "BlockHit", "BlockEnd",                
                "Unarmed Equip Underarm", "Standing Melee Run Jump Attack", "Attack_1",
                "Shooting Arrow",
                "Cast1 Start", "Cast1 Maintain", "Cast1 End", "Standing 2H Cast Spell 01",
                "Death"
            };
            // if (name == "Scorpion") Actions = new List<string> {"Idle", "Attack 2", "Walk", "Got Hit", "Death" };
            // if (name == "FishmanLP") Actions = new List<string> {"Idle", "Attack_1", "Walk", "Get_hit", "Death" };
            // if (name == "F_Dwarf_Axe" || name == "F_Dwarf") Actions = new List<string> {"Idle", "Great Sword Slash", "Run", "GotHit", "Death"} ;
            // if (name == "F_Ogre") Actions = new List<string> {"Idle", "Attack3", "Run", "GotHit", "Death"};
            // if (name == "Devils") Actions = new List<string> {"idle", "attack1", "walk forward", "hit reaction", "death"};
            else Actions = new List<string> {
                "Idle", "Idlebreak01", "Idlebreak02", "CombatIdle", "Kneeling Down", "Run", "Walk", "GotHit", "Dodge",  "Parry",  
                "PrayKneelingStart", "PrayStandingLoop",
                "Cleave", "Sword And Shield Casting", 
                "Withdrawing Sword", "Attack2",
                "BlockStart", "BlockLoop", "BlockHit", "BlockEnd",                
                "Unarmed Equip Underarm", "Standing Melee Run Jump Attack", "Great Sword Slash",
                "Shooting Arrow",
                "Cast1 Start", "Cast1 Maintain", "Cast1 End", "Standing 2H Cast Spell 01",
                "Death"
            };

            numActions = Actions.Count;
            animator = GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;    
            while (!FramesDataByActionState[actionIndex].capture) {
                actionIndex++;
                if (Outside_Array_Bounds(Actions.Count, actionIndex, "Actions")) {  
                    if (output) Debug.Log("No action state selected!");         
                    enable = false;
                    return;
                }
            }
            Set_Action(Actions[actionIndex]);    
            if (output) Debug.Log("Action set to; " + Actions[actionIndex]);
            init = true; 
        }
    }

    private bool CalcFrames = true;
    private void Set_Action(string newState){
        if (currentState == newState) return;        
        animator.Play(newState);
        currentState = newState;

        animation = XmlOutputActions[actionIndex];                
        animator.applyRootMotion = (FramesDataByActionState[actionIndex].applyRootMotion);
        CalcFrames = true;
    }

    public bool Get_Action(string direction, ref bool disableCamera) {
        if (!enable) {  
            if (output) Debug.Log("Script Disabled");
            return false;
        }

        if (Outside_Array_Bounds(Actions.Count, actionIndex, "Actions")) {            
            if (debug) Debug.Log("action index of: " + actionIndex + " is outside the bounds of Actions[] size: " + Actions.Count + " disabling camera");
            disableCamera = true;
            return false;        
        }

        if (Actions[actionIndex] != animator.GetCurrentAnimatorClipInfo(0)[0].clip.name) {
            if (debug) Debug.Log("change actiont to: " + Actions[actionIndex] + ", current: " + animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            return false;
        }

        if (CalcFrames) {
            CalcFrames = false;
            if (debug) Debug.Log("Number of frames for action " + Actions[actionIndex] + ": " + totalFrames + ", name: " + animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);

            //30
            defaultFPS = animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;      
            //62                                 //duration
            totalFrames = (int)Math.Round(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * defaultFPS);
            //so how may frames of animation should equal 1 frame of sprite movement?
            numFrames = (int)((float)totalFrames * (targetFPS / defaultFPS)); 
            outputFPS = targetFPS;   
            //if frames is too low or too high recalculate
            if (debug) Debug.Log("initial Calc() " + totalFrames + " x (" + targetFPS + " / " + defaultFPS + ") = " + numFrames);
            if (numFrames < 8) {
                if (debug) Debug.Log("too few frames: " + numFrames);
                numFrames = minFrames;
                outputFPS = (float)((float)numFrames / (float)totalFrames) * defaultFPS;
                if (debug) Debug.Log("Re Calc() " + defaultFPS + " x (" + numFrames + " / " + totalFrames + ") = " + outputFPS);
            } 
            if (numFrames > maxFrames) {
                if (debug) Debug.Log("too many frames: " + numFrames);
                numFrames = maxFrames;
                outputFPS = (float)((float)numFrames / (float)totalFrames) * defaultFPS;
                if (debug) Debug.Log("Re Calc() " + defaultFPS + " x (" + numFrames + " / " + totalFrames + ") = " + outputFPS);
            } 
        }
        return true;
    }

    private void Awake()
    {
        Init();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void Next_Action() {
        if (Outside_Array_Bounds(Actions.Count, actionIndex, "Actions")) {
            Debug.Log("Done!");
            enable = false;
            return;
        }
        
        if (XmlOutputActions[actionIndex] == "death" && !captureDead) {
            captureDead = true;
            finishedCameras = 0;
            return;
        }

        finishedCameras = 0;         
        actionIndex++;
        if (Outside_Array_Bounds(Actions.Count, actionIndex, "Actions")) {
            Debug.Log("Done!");
            enable = false;
            return;
        }
        while (!FramesDataByActionState[actionIndex].capture) {
            actionIndex++;
            if (Outside_Array_Bounds(Actions.Count, actionIndex, "Actions")) {
                enable = false;
                Debug.Log("Done!");
                return;
            }
        }
        Set_Action(Actions[actionIndex]);
    }

    // Update is called once per frame
    void Update()
    {   
        if (directions == Directions.four && finishedCameras == 4) 
            Next_Action();
        else if (directions == Directions.eight && finishedCameras == 8) 
            Next_Action();
        else if (directions == Directions.sixteen && finishedCameras == 16) 
            Next_Action();
    }
}


