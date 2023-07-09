//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace DamianGonzalez.Portals {
    [DefaultExecutionOrder(100)]
    public class PortalSetup : MonoBehaviour {

        #region declarations

        [SerializeField] LayerMask cullingMaskA;
        [SerializeField] LayerMask cullingMaskB;

        public static Dictionary <string, PortalSetup> allPortals = new Dictionary <string, PortalSetup> ();

        public Transform playerCamera;  //this is optional. Default value is main camera.
        public Shader refShader;        //this is already assigned. Should be "screenRender.shader"
        public string groupId = "";     //not necessary, but useful for debugging

        [HideInInspector] public bool setupComplete = false;

        [System.Serializable]
        public class InternalReferences {
            public  int screenHeight;
            public  int screenWidth;

            public GameObject objCamA;
            public GameObject objCamB;

            public Camera cameraA;
            public Camera cameraB;

            public Transform planeA;
            public Transform planeB;

            public Transform portalA;
            public Transform portalB;

            public PortalCamMovement scriptCamA;
            public PortalCamMovement scriptCamB;

            public Renderer rendererA;
            public Renderer rendererB;

            public Transform functionalFolderA;
            public Transform functionalFolderB;
        }

        public InternalReferences refs;

        private float timeStartResize = -1;

        public bool doubleSided = false;



        [Serializable]
        public class Filters {
            public string playerTag = "Player";

            [Header("------ Main filter. If unchecked, only the player will pass ---------------------------")]
            public bool otherObjectsCanCross = true;

            [Header("------ Positive filter. Only objects with these tags can pass. ------------------------")]
            [Space(height: 20f)]
            public List<string> tagsCanCross = new List<string>();

            [Header("------ Gandalf filter. Objects with these tags shall not pass. ------------------------")]
            [Space(height: 20f)]
            public List<string> tagsCannotCross = new List<string>();
        }
        public Filters filters;

        public enum WhichSideRealObjectOptions { originPortal, destinationPortal, keepOnPlayerSide }
        [Serializable]
        public class Clones {
            [Header("Use of clones")]
            public bool useClones = true;
            public WhichSideRealObjectOptions whichSideRealObject = WhichSideRealObjectOptions.keepOnPlayerSide;
            //public ClonePlayerOptions clonePlayerToo = ClonePlayerOptions.onlyIfRecursiveOrDoubleSide;
        }
        public Clones clones;



        [Serializable]
        public class AfterTeleportOptions {
            [Header("When player teleports")]
            public bool tryResetCharacterController = true;
            public bool tryResetCameraObject = true;
            public bool tryResetCameraScripts = true;
            [Header("Useful for debugging")]
            public bool pauseWhenPlayerTeleports = false;
            public bool pauseWhenOtherTeleports = false;

        }
        public AfterTeleportOptions afterTeleport;
        public bool verboseDebug = false;


        [Serializable]
        public class Advanced {
            public float maximumDistance = 5f;
            [Range(10f, 100f)] public float maxVelocityForClones = 40f;

            public bool alwaysMimicPlayersFOV = false;

            [Header("Elastic plane")]
            public bool useElasticPlane = true;
            public float elasticPlaneOffset = -.44f;
            [Range(.1f, 1f)] public float minDistPlayerPlane = .4f;
            [Range(-.3f, .3f)] public float elasticPlaneTeleportTreshold = .2f;
            [Range(5f, 40f)] public float maxVelocityForElasticPlane = 15f;

            /*
            [Header("Make these portals visible behind other portals?")]
            public Transform[] depA = new Transform[0];
            public Transform[] depB = new Transform[0];
            */

            [Header("Performance optimization")]
            //options for optimizing performance, by reducing camera rendering as much as possible
            public bool disableUnnecessaryCameras = true;
            public bool disableDistantCameras = false;
            public float fartherThan = 100f;
            public bool renderCamerasOnFirstFrame = true; //so the portals with disabled cameras can show something

            [Header("Advanced visual settings")]
            public bool alignNearClippingPlane = true;
            [HideInInspector] public float currentClippingOffset = .5f;
            [Range(-5f, 5f)] public float clippingOffset = .5f;
            public float dontAlignNearerThan = .5f;
            [Range(-2f, 2f)] public float dotCalculationOffset = 0.5f;
            public int depthTexture = 16;

            [Header("Recursive rendering (expensive, handle with care!)")]
            public bool useRecursiveRendering = false;
            public int maximumRenderPasses = 5;
        }
        public Advanced advanced;

        private bool isFirstFrame = true;
        [HideInInspector] public bool teleportInProgress = false;
        [HideInInspector] public float lastTeleportTime = 0;
        [HideInInspector] public bool forceActivateCamsInNextFrame = false;


        public bool useShadowClone;
        Transform shadowClone;

        public enum StartWorkflow { Runtime, Deployed }
        public StartWorkflow startWorkflow = StartWorkflow.Runtime;


        #endregion


        void Awake() {
            PortalInitialization();
        }

        void PortalInitialization() {
            if (startWorkflow == StartWorkflow.Runtime) {
                DeployAndSetupMaterials();
            } else {
                SetupMaterials();
            }

            if (allPortals.ContainsKey(groupId)) allPortals.Remove(groupId);
            allPortals.Add(groupId, this);

            setupComplete = true;
            PortalEvents.setupComplete?.Invoke(groupId, transform);
        }

        [ContextMenu("Deploy and setup")]
        public void DeployAndSetupMaterials() {
            //first, quick search if it's already deployed
            if (transform.GetComponentInChildren<Camera>() != null) {
                Debug.LogWarning("This portal seems to be already deployed. Operation aborted.");
                return;
            }


            //if not provided, use default values
            if (playerCamera == null) playerCamera = Camera.main.transform;
            if (groupId == "") groupId = transform.name;

            //reference to each portal of this set
            refs.portalA = transform.GetChild(0);
            refs.portalB = transform.GetChild(1);

            refs.functionalFolderA = refs.portalA.GetChild(0);
            refs.functionalFolderB = refs.portalB.GetChild(0);

            Transform triggerA = refs.functionalFolderA.Find("trigger");
            Transform triggerB = refs.functionalFolderB.Find("trigger");

            refs.planeA = refs.functionalFolderA.Find("plane");
            refs.planeB = refs.functionalFolderB.Find("plane");

            refs.rendererA = refs.planeA.GetComponent<Renderer>();
            refs.rendererB = refs.planeB.GetComponent<Renderer>();


            //generate the empty objects for the cameras
            refs.objCamA = new GameObject("Camera (around A on plane B)");
            refs.objCamB = new GameObject("Camera (around B on plane A)");

            //and put them inside the containers. 
            refs.objCamA.transform.SetParent(refs.functionalFolderA, true);
            refs.objCamB.transform.SetParent(refs.functionalFolderB, true);

            //add camera components to the cameras
            refs.cameraA = refs.objCamA.AddComponent<Camera>();
            refs.cameraB = refs.objCamB.AddComponent<Camera>();

            //and its scripts
            refs.scriptCamA = refs.cameraA.gameObject.AddComponent<PortalCamMovement>();
            refs.scriptCamB = refs.cameraB.gameObject.AddComponent<PortalCamMovement>();

            //I give this new cameras same setup than main camera
            Camera cameraComp = playerCamera.GetComponent<Camera>();
            refs.cameraA.CopyFrom(cameraComp);
            refs.cameraB.CopyFrom(cameraComp);

            refs.cameraA.cullingMask = cullingMaskA;
            refs.cameraB.cullingMask = cullingMaskB;

            //...but a different order (if provided)
            //cameraA.depth = advanced.cameraOrder;
            //cameraB.depth = advanced.cameraOrder;


            //Setup both camera's scripts
            refs.scriptCamA.playerCamera = playerCamera;
            refs.scriptCamA._camera = refs.cameraA;
            refs.scriptCamA._plane = refs.planeA;
            refs.scriptCamA.portal = refs.portalA;
            refs.scriptCamA._renderer = refs.rendererA;
            refs.scriptCamA._filter = refs.planeA.GetComponent<MeshFilter>();
            refs.scriptCamA._collider = triggerA.GetComponent<Collider>();
            refs.scriptCamA.otherScript = refs.scriptCamB;
            refs.scriptCamA.setup = this;
            refs.scriptCamA.mainObject = transform;
            refs.scriptCamA.cameraId = groupId + ".a";
            refs.scriptCamA.inverted = false;

            refs.scriptCamB.playerCamera = playerCamera;
            refs.scriptCamB._camera = refs.cameraB;
            refs.scriptCamB._plane = refs.planeB;
            refs.scriptCamB.portal = refs.portalB;
            refs.scriptCamB._renderer = refs.rendererB;
            refs.scriptCamB._filter = refs.planeB.GetComponent<MeshFilter>();
            refs.scriptCamB._collider = triggerB.GetComponent<Collider>();
            refs.scriptCamB.otherScript = refs.scriptCamA;
            refs.scriptCamB.setup = this;
            refs.scriptCamB.mainObject = transform;
            refs.scriptCamB.cameraId = groupId + ".b";
            refs.scriptCamB.inverted = true;

            //and setup both portal's script
            Teleport scriptPortalA = triggerA.GetComponent<Teleport>();
            Teleport scriptPortalB = triggerB.GetComponent<Teleport>();

            scriptPortalA.setup = this;
            scriptPortalA.cameraScript = refs.scriptCamA;
            scriptPortalA.otherScript = scriptPortalB;
            scriptPortalA.mainObject = transform;
            scriptPortalA.portal = refs.portalA;
            scriptPortalA.plane = refs.planeA;
            scriptPortalA._collider = scriptPortalA.GetComponent<BoxCollider>();
            scriptPortalA.planeIsInverted = false;


            scriptPortalB.setup = this;
            scriptPortalB.cameraScript = refs.scriptCamB;
            scriptPortalB.otherScript = scriptPortalA;
            scriptPortalB.mainObject = transform;
            scriptPortalB.portal = refs.portalB;
            scriptPortalB.plane = refs.planeB;
            scriptPortalB._collider = scriptPortalB.GetComponent<BoxCollider>();
            scriptPortalB.planeIsInverted = true;


            //Create materials with the shader
            //and asign those materials to the planes (here is where they cross)
            SetupMaterials();


            //camera objects enabled, but cameras components disabled, we'll use manual rendering, even if is not recursive
            refs.objCamA.SetActive(true);
            refs.objCamB.SetActive(true);
            refs.cameraA.enabled = false;
            refs.cameraB.enabled = false;


            if (useShadowClone) {
                //create shadow clones
                refs.scriptCamA.shadowClone = CreateShadowClone(playerCamera.parent.gameObject);
                refs.scriptCamB.shadowClone = CreateShadowClone(playerCamera.parent.gameObject);

                if (refs.portalA.eulerAngles != refs.portalB.eulerAngles) {
                    Debug.LogWarning(
                        $"useShadowClone is active in portal '{groupId}', but its portals rotation don't match, " +
                        "so shadows from directional lights won't match", transform
                    );
                }
            }



            if (Application.isEditor) {
                //finally, mark changes as dirty, otherwise they won't be saved
                UnityEditor.EditorUtility.SetDirty(gameObject);
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(refs.scriptCamA);
                UnityEditor.EditorUtility.SetDirty(refs.scriptCamB);
                UnityEditor.EditorUtility.SetDirty(scriptPortalA);
                UnityEditor.EditorUtility.SetDirty(scriptPortalB);

                startWorkflow = StartWorkflow.Deployed;

            }

        }

        [ContextMenu("Undeploy (back to 'runtime' mode")]
        public void Undeploy() {

            //restaure all internal references
            refs = new InternalReferences();


            //delete cameras
            foreach (Camera _cam in transform.GetComponentsInChildren<Camera>()) {
                DestroyImmediate(_cam.gameObject);
            }

            startWorkflow = StartWorkflow.Runtime;

        }

        void Start() {
            /*
            foreach (Transform tr in advanced.depA) {
                refs.scriptCamB.dep.Add(tr.GetChild(0).GetChild(1).GetComponent<PortalCamMovement>());
            }
            foreach (Transform tr in advanced.depB) {
                refs.scriptCamA.dep.Add(tr.GetChild(0).GetChild(1).GetComponent<PortalCamMovement>());
            }
            */
        }

        private void Update() {
            
            //1 second after resize is done, it updates the render textures
            if (timeStartResize == -1 && (refs.screenHeight != Screen.height || refs.screenWidth != Screen.width)) timeStartResize = Time.time;

            if (timeStartResize > 0 && Time.time > timeStartResize + 1f) ResizeScreen();



            //manually render cameras (only if necessary)
            refs.scriptCamA.ManualRenderIfNecessary();
            refs.scriptCamB.ManualRenderIfNecessary();



            if (forceActivateCamsInNextFrame) forceActivateCamsInNextFrame = false;
            if (advanced.renderCamerasOnFirstFrame && isFirstFrame) forceActivateCamsInNextFrame = true; //renders 2nd frame, actually, when every camera are set and placed
            isFirstFrame = false;


            if (Input.GetKeyDown(KeyCode.P)) teleportInProgress = false;
        }



        void SetupMaterials() {

            timeStartResize = -1;

            //Create materials with the shader
            Material matA = new Material(refShader);
            refs.cameraA.targetTexture?.Release();
            refs.cameraA.targetTexture = new RenderTexture(Screen.width, Screen.height, advanced.depthTexture);
            matA.mainTexture = refs.cameraA.targetTexture;
            matA.SetTexture("_MainTex", refs.cameraA.targetTexture);

            Material matB = new Material(refShader);
            refs.cameraB.targetTexture?.Release();
            refs.cameraB.targetTexture = new RenderTexture(Screen.width, Screen.height, advanced.depthTexture);
            matB.mainTexture = refs.cameraB.targetTexture;

            //and asign those materials to the planes (here is where they cross)
            refs.rendererA.material = matB;
            refs.rendererB.material = matA;

            /*
            //also to the "emergency" plane (see online documentation)
            if (functionalFolderA.Find("plane2") != null) functionalFolderA.Find("plane2").GetComponent<MeshRenderer>().material = matB;
            if (functionalFolderB.Find("plane2") != null) functionalFolderB.Find("plane2").GetComponent<MeshRenderer>().material = matA;
            */

            refs.screenHeight = Screen.height;
            refs.screenWidth = Screen.width;

        }

        void ResizeScreen() {
            SetupMaterials();
            PortalEvents.gameResized?.Invoke(
                groupId,
                transform,
                new Vector2(refs.screenHeight, refs.screenWidth),
                new Vector2(Screen.height, Screen.width)
            );
        }

        Transform CreateShadowClone(GameObject obj) {
            Transform tr = Instantiate(obj).transform;

            //delete cameras
            foreach (Camera _cam in tr.GetComponentsInChildren<Camera>()) {
                Destroy(_cam);
            }

            //delete scripts
            foreach (MonoBehaviour _scr in tr.GetComponentsInChildren<MonoBehaviour>()) {
                Destroy(_scr);
            }

            //delete colliders
            foreach (Collider _col in tr.GetComponentsInChildren<Collider>()) {
                Destroy(_col);
            }

            //set renderers to only cast shadows
            foreach (MeshRenderer _mr in tr.GetComponentsInChildren<MeshRenderer>()) {
                _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            return tr;
        }

    }
}