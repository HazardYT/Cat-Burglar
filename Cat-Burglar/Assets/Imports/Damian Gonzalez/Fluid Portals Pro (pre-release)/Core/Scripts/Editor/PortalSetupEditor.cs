#if (UNITY_EDITOR) 

using UnityEngine;
using UnityEditor;

/*
 *  All this code does is to provide the "Build" and "Clear" buttons in the inspector for the 
 *  Elevator Builder script. This is purely cosmetic, since you can achieve the same through
 *  the context menu of the script in the inspector (The same menu of "Remove Component"...)
 *  
 *  So, if you eventually get any warnings or errors, 
 *  consider deleting this file althogheter or moving it temporally out of the \Editor folder
 */

namespace DamianGonzalez.Portals {
    [CustomEditor(typeof(PortalSetup))]
    [HelpURL("https://www.pipasjourney.com/damianGonzalez/portals/")]

    public class PortalSetupEditor : Editor {
        public Texture editorLogo;
        public override void OnInspectorGUI() {
            //logo
            if (editorLogo != null) {
                GUI.DrawTexture(new Rect(0, 20, Screen.width, 90), editorLogo, ScaleMode.ScaleAndCrop);
                GUILayout.Space(20 + 90 + 20);
            }

            //display exposed variables
            base.OnInspectorGUI();

            //and buttons below
            PortalSetup script = (PortalSetup)target;

            GUILayout.Space(50);


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Switch to 'Runtime' mode", GUILayout.Height(20))) {
                script.Undeploy();
                Debug.Log("Changed to 'runtime' mode");
            }
            if (GUILayout.Button("Switch to 'Deployed' mode", GUILayout.Height(20))) {
                script.DeployAndSetupMaterials();
                Debug.Log("Changed to 'deployed' mode");
            }

            GUILayout.EndHorizontal();

            //help box

            EditorGUILayout.HelpBox("If you need assistance " +
                "from the developer, please visit the online documentation " +
                "(you can click the info button next to the component name)", MessageType.Info, true);


            //margin below
            GUILayout.Space(10);
        }

    }
}
#endif