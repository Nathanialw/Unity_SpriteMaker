#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;


//instantiates this every frame so I can't store multiframe variables here
public class SpriteSheetImages : MonoBehaviour
{
    public MyRecorderController myRecorderController;

    public enum Directions {
        four = 0,
        eight,
        sixteen
    }
    bool debug = false;
    bool output = false;

    private bool inactive = true;
    private string name = "";
    private string camera = "";
    private int size = 80;
    private Directions directions = Directions.sixteen;

    private string animation = "Idle";
    private float totalFrames = 80.0f; 
    private int numFrames = 3;
    private int maxFrame = 1;
    private float defaultFPS = 30.0f;
    private float FPS;
    private int time = 0;
    
    private RecorderController m_RecorderController;
    private string frame = DefaultWildcard.Frame;

    private string Pad(string rawDirection) {
        //check length
        int length = rawDirection.Length;
        //if less that 3 chars, pad with underscores to 3
        for (int i = length; i < 3; i++) {
            rawDirection = rawDirection + "_";
        }
        return rawDirection;    
    }

    private bool Init () {            
        debug = myRecorderController.debug; 
        if (debug) Debug.Log(camera + " Starting Init() for action: " + animation);        
        output = myRecorderController.debug; 
        camera = this.gameObject.name;
        name = myRecorderController.name;
        size = myRecorderController.size;
        directions = (Directions)myRecorderController.directions;
        if (debug) Debug.Log(camera + " Init() complete for action: " + animation);  
        return true;
    }
    
    private bool isRecording = false;
    
    private void Init_Camera() {
        if (debug) Debug.Log(camera + " Starting Init_Camera() for action: " + animation);          
        animation = myRecorderController.animation;
        totalFrames = myRecorderController.totalFrames; 
        numFrames = myRecorderController.numFrames;
        defaultFPS = myRecorderController.defaultFPS;
        FPS = myRecorderController.outputFPS;
        maxFrame = numFrames - 1;
        isRecording = true;        
        StartRecorder(); 
        if (output) Debug.Log(camera + " Starting Capture for action: " + animation);          
    }

    void Check_If_Active() {
        if (camera.Length == 1) 
            inactive = false;        
        else if (camera.Length == 2) {
            if (directions >= Directions.eight) 
                inactive = false;
        }
        else if (camera.Length == 3) {
            if (directions >= Directions.sixteen) 
                inactive = false;
        }
    }

    bool Run_Camera() {      
        if (inactive) {
            if (debug) Debug.Log("camera" + name + "inactive");
            return false;        
        }
        if (!myRecorderController.Get_Action(camera, ref inactive)) return false;
        
        Init_Camera();
        return true;
    }

    private void Run() {
        if (!Init()) {
           if (debug) Debug.Log("Init Failed!!");
            return;
        };
        Check_If_Active();
        if (!Run_Camera()) {
           if (debug) Debug.Log("Run_Camera() Failed!!");
        }
    }
    
    private void Start() {
        Run();
    }
   
    private bool done = false;
    private void Update()
    {
        if (inactive) return;    
        if (myRecorderController.actionIndex < myRecorderController.Actions.Count) {         
            if (isRecording) {
                if (!m_RecorderController.IsRecording()) {
                    if (output) Debug.Log("finished recording action: " + animation + " for Direction: " + camera);
                    isRecording = false;
                    myRecorderController.finishedCameras++;                    
                }
            }
            else if (myRecorderController.finishedCameras == 0) {
                if (!Run_Camera()) {
                    isRecording = false;
                    if (debug) Debug.Log("Reset Camera failed for: " + animation + " for Direction: " + camera);;
                }
            }            
        }
        else {
            if (!done) {
                done = true;
                if (output) Debug.Log("finished recording all actions for: " + camera);        
                return;
            }
        }        
    }

    //all it needs is the tagged camera name

    //runs once to start the recorder, I assume there is a method that tells me when it is done
    private void StartRecorder()
    {    
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "..", "..", "..", "Unity Sprite Sheet Exports");
        // Image Sequence
        var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        imageRecorder.name = "My Image Recorder";
        imageRecorder.Enabled = true;

        imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        imageRecorder.CaptureAlpha = true;

        string fullName = name;
        if (myRecorderController.replace) {
            fullName = myRecorderController.itemName;
        }
        else if (myRecorderController.itemName != "") {
            fullName = fullName + "_" + myRecorderController.itemName;
        }

        if (myRecorderController.seperateFolders) {
            imageRecorder.OutputFile = (myRecorderController.captureDead && myRecorderController.XmlOutputActions[myRecorderController.actionIndex] == "death") ? Path.Combine(mediaOutputFolder, fullName, animation, fullName) + "_" + "dead" + "_" + Pad(camera) + "_0000" : Path.Combine(mediaOutputFolder, fullName, animation, fullName) + "_" + animation + "_" + Pad(camera) + "_" +  frame;
        }
        else {
            imageRecorder.OutputFile = (myRecorderController.captureDead && myRecorderController.XmlOutputActions[myRecorderController.actionIndex] == "death") ? Path.Combine(mediaOutputFolder, fullName, fullName) + "_" + "dead" + "_" + Pad(camera) + "_0000" : Path.Combine(mediaOutputFolder, fullName, fullName) + "_" + animation + "_" + Pad(camera) + "_" +  frame;
        }
        
        imageRecorder.imageInputSettings = new CameraInputSettings
        {
            CameraTag = camera,
            Source = ImageSource.TaggedCamera,
            RecordTransparency = true,
            OutputWidth = size,
            OutputHeight = size
        };

        // Setup Recording
        controllerSettings.AddRecorderSettings(imageRecorder);
        if ((myRecorderController.captureDead && myRecorderController.XmlOutputActions[myRecorderController.actionIndex] == "death")) {
            controllerSettings.SetRecordModeToSingleFrame(maxFrame);
        }

        else {
            controllerSettings.SetRecordModeToFrameInterval(0, maxFrame);        
        }
        
        controllerSettings.FrameRate = FPS;
        
        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }
}


#endif
