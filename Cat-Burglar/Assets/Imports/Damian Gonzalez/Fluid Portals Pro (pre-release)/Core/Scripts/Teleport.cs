//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DamianGonzalez.Portals {
    public class Teleport : MonoBehaviour {

        private float elasticPlaneValue = 0f;
        private Vector3 originalPlanePosition;


        public BoxCollider _collider;               //
        public Transform plane;                     //
        public Transform portal;                    // these variables are automatically writen
        public PortalSetup setup;                   // by PortalSetup 
        public Transform mainObject;                //
        public PortalCamMovement cameraScript;      //
        public Teleport otherScript;                //


        public bool planeIsInverted; //only for elastic mode

        public bool changeGravity = true;

        public bool teleportPlayerOnExit = false;

        public bool dontTeleport = false;


        //List<Transform> clones = new List<Transform>();
        //List<Transform> originals = new List<Transform>();
        Dictionary<Transform, Transform> clones = new Dictionary<Transform, Transform>(); //original => clone
        GameObject cloneParent;

        public bool rot2Y;

        public Vector3 angOffset;
        private void Start() {
            originalPlanePosition = plane.position;
        }

        Vector3 GetVelocity(Transform obj) {
            if (obj.TryGetComponent(out Rigidbody rb)) return rb.velocity;

            if (obj.TryGetComponent(out CharacterController cc)) return cc.velocity;

            return Vector3.zero;
        }

        Vector3 TowardDestination() => planeIsInverted ? -portal.forward : portal.forward;

        public bool IsGoodSide(Transform obj) {
            //not about facing, but about velocity. where is it going?
            Vector3 velocityOrPosition = GetVelocity(obj);
            float dotProduct;
            if (velocityOrPosition != Vector3.zero) {
                //it has velocity
                dotProduct = Vector3.Dot(-TowardDestination(), velocityOrPosition);
            } else {
                //it hasn't velocity, let's try with its position (it may fail with very fast objects)
                dotProduct = Vector3.Dot(-TowardDestination(), portal.position - velocityOrPosition);
            }

            if (setup.verboseDebug) Debug.Log($"{obj.name} crossing. Good side: {dotProduct < 0}");
            return dotProduct < 0;
        }

        public bool IsGoodSide(Vector3 dir) {
            float dotProduct = Vector3.Dot(-TowardDestination(), dir);
            return dotProduct < 0;
        }

        //bool playerGoodSideOnTrigger;
        //bool otherGoodSideOnTrigger;

        bool CandidateToTeleport(Transform objectToTeleport) {
            /*
             * an object is candidate to teleport now if:
             * a) it's player, or not player but it passes the tag filters
             * and
             * b) portal is double-sided, or single-sided but in the good side
             * and
             * c) object is not too far from the portal
             */

            //a)
            //bool isPlayer = objectToTeleport.CompareTag(setup.filters.playerTag); //for better readability
            if (!ThisObjectCanCross(objectToTeleport)) {
                if (setup.verboseDebug) Debug.Log($"{objectToTeleport.name} will not teleport. Reason: filters");
                return false;
            }

            //b) 
            if (!setup.doubleSided && !IsGoodSide(objectToTeleport)) {
                if (setup.verboseDebug) Debug.Log($"{objectToTeleport.name} will not teleport. Reason: not the good side"); 
                return false;
            }

            //c)
            bool tooFar = Vector3.Distance(objectToTeleport.position, portal.position) > setup.advanced.maximumDistance;
            if (tooFar) {
                if (setup.verboseDebug) Debug.Log($"{objectToTeleport.name} will not teleport. Reason: too far");
                return false;
            }

            return true;
        }

        void DoTeleport(Transform objectToTeleport, bool fireEvents = true) {
            bool isPlayer = objectToTeleport.CompareTag(setup.filters.playerTag); //for better readability

            if (isPlayer) {
                /*
                 * If you need to do something to your player RIGHT BEFORE teleporting,
                 * this is when.
                */
            }


            // Teleport the object
            Vector3 oldPosition = objectToTeleport.position;
            Vector3 rbOffset = Vector3.zero;
            Rigidbody rb = objectToTeleport.GetComponent<Rigidbody>();
            if (rb != null) rbOffset = rb.position - transform.position;


            //position
            objectToTeleport.position = otherScript.portal.TransformPoint(
                portal.InverseTransformPoint(objectToTeleport.position)
            );

            //rotation

            objectToTeleport.rotation =
                otherScript.portal.rotation
                * Quaternion.Inverse(portal.rotation)
                * objectToTeleport.rotation;



            //velocity (if object has rigidbody)
            if (rb != null) {
                rb.velocity = otherScript.portal.TransformDirection(
                    portal.InverseTransformDirection(rb.velocity)
                );
                rb.position = transform.position + rbOffset; //not entirely necessary
            }




            if (isPlayer) {

                //player has crossed. If using clones, may be necessary to swap clones and originals (see documentation)
                /*
                if (setup.clones.useClones && setup.clones.whichSideRealObject == portalSetup.WhichSideRealObjectOptions.keepOnPlayerSide) {
                    int howManyOnTheOtherSide = otherScript.originals.Count;
                    SwapSidesOfClonesOnThisSide(originals.Count);
                    otherScript.SwapSidesOfClonesOnThisSide(howManyOnTheOtherSide);
                }
                */


                //refresh camera position before rendering, in order to avoid flickering
                otherScript.cameraScript.Recalculate();
                cameraScript.Recalculate();

                if (setup.afterTeleport.tryResetCharacterController) {
                    //reset player's character controller (if there is one)
                    //otherwise it won't allow the change of position
                    if (objectToTeleport.TryGetComponent(out CharacterController cc)) {
                        cc.enabled = false;
                        cc.enabled = true;
                    }
                }

                if (setup.afterTeleport.tryResetCameraObject) {
                    //reset player's camera (may solve issues)
                    setup.playerCamera.gameObject.SetActive(false);
                    setup.playerCamera.gameObject.SetActive(true);
                }

                if (setup.afterTeleport.tryResetCameraScripts) {
                    //reset scripts in camera
                    foreach (MonoBehaviour scr in setup.playerCamera.GetComponents<MonoBehaviour>()) {
                        if (scr.isActiveAndEnabled) {
                            scr.enabled = false;
                            scr.enabled = true;
                        }
                    }
                }



                /*
                 * If you need to do something to your player when it's teleporting, this is when.
                 * See online documentation about controllers (pipasjourney.com/damianGonzalez/portals/#controller)
                 * and how to implement a 3rd party controller with these portals
                */

                if (setup.afterTeleport.pauseWhenPlayerTeleports) Debug.Break();

            } else {
                // If you need to do something to other crossing object, this is when.
                if (setup.afterTeleport.pauseWhenOtherTeleports) Debug.Break();
            }

            //finally, fire event
            if (fireEvents){

                PortalEvents.teleport?.Invoke(
                    setup.groupId,
                    portal,
                    otherScript.portal,
                    objectToTeleport,
                    oldPosition,
                    objectToTeleport.position
                );

                try{

                    foreach (KeyValuePair<Transform, Transform> cl in clones) {

                        //this is a clone. Delete the object
                        Destroy(cl.Value.gameObject);

                        //and the reference
                        clones.Remove(cl.Key);

                        return;
                    }
                }
                catch{


                }
                

            }
        }

        /*
        public void SwapSidesOfClonesOnThisSide(int howMany) {
            for (int i = howMany - 1; i >= 0; i--) {
                DoTeleport(originals[i], false); //the other side will create a clone in this side
                DestroyCloneFromOriginal(originals[i]);
            }

        }
        */

        bool thisOne = false;
        void OnTriggerEnter(Collider other) {
            //non-players teleports on enter
            //players teleports on enter when elastic mode is off, or when he's crossing very fast



            bool isPlayer = other.CompareTag(setup.filters.playerTag);

            if (!isPlayer) {
                ConsiderTeleporting(other);
            } else {
                Vector3 vel = GetVelocity(other.transform);
                Debug.Log("velocity " + vel.magnitude);

                if (!setup.advanced.useElasticPlane || vel.magnitude > setup.advanced.maxVelocityForElasticPlane) {
                    if (setup.verboseDebug) Debug.Log($"{other.name} considered for teleporting, because is crossing too fast ({vel.magnitude} > {setup.advanced.maxVelocityForElasticPlane})");
                    ConsiderTeleporting(other);
                }
            }

        }

        void ConsiderTeleporting(Collider other) {

            //process timeout (and continues)
            if (Time.time > setup.lastTeleportTime + .05f) {
                thisOne = false;
                otherScript.thisOne = false;
                setup.teleportInProgress = false;
            }

            //process ends, but doesn't continue
            if (setup.teleportInProgress) {
                if (!thisOne) {
                    otherScript.thisOne = false;
                    setup.teleportInProgress = false;
                }
                if (setup.verboseDebug) Debug.Log($"{other.name} will not teleport. Reason: too soon");
                return;
            }



            if (!CandidateToTeleport(other.transform)) {
                return;
            }

            if (setup.verboseDebug) Debug.Log($"{other.name} passed the filters, will teleport.");
            //ok, it's candidate to teleport

            setup.teleportInProgress = true;
            setup.lastTeleportTime = Time.time;
            thisOne = true;

            bool create = true;
            Vector3 vel = GetVelocity(other.transform);

            if (vel.magnitude > setup.advanced.maxVelocityForClones) create = false;


            if (create) {
                if (setup.verboseDebug) Debug.Log($"Creating clone for {other.name}...");
                otherScript.CreateClone(other.transform, otherScript.transform, false); //create clone where the original is
            }


            
            DoTeleport(other.transform);
            if (create) otherScript.UpdateClone(other.transform);

            /*if (!isPlayer) {
                otherScript.SwapSidesOfClonesOnThisSide(1);
            }*/


        }

        void TryDestroyClone(Transform clone_or_original) {
            foreach (KeyValuePair<Transform, Transform> cl in clones) {
                if (cl.Value.Equals(clone_or_original) || cl.Key.Equals(clone_or_original)) {
                    //this is a clone. Delete the object
                    Destroy(cl.Value.gameObject);

                    //and the reference
                    clones.Remove(cl.Key);

                    return;
                }
            }

        }

        void OnTriggerExit(Collider other) {
            TryDestroyClone(other.transform);



            bool isPlayer = other.CompareTag(setup.filters.playerTag);
            if (setup.advanced.useElasticPlane) {
                //if (isPlayer) plane.position = portal.TransformVector(originalPlanePosition);
                if (isPlayer) {
                    plane.position = originalPlanePosition;
                    setup.advanced.currentClippingOffset = setup.advanced.clippingOffset;
                }
            }

            if (setup.teleportInProgress) return; //do not disturb


            //if using elastic plane and player crosses too fast (1 or 2 frames), it should consider to be teleported
            if (setup.advanced.useElasticPlane && isPlayer && Time.time > setup.lastTeleportTime + .05f) {
                if (setup.verboseDebug) Debug.Log($"Emergency teleport! {other.name} has left the trigger.");
                ConsiderTeleporting(other);
                
            }
        }



        public float trespassProgress;
        void OnTriggerStay(Collider other) {

            //other side has a clone? update it
            if (setup.clones.useClones && clones.ContainsKey(other.transform)) UpdateClone(other.transform);

            //elastic plane
            bool isPlayer = other.CompareTag(setup.filters.playerTag);
            if (isPlayer && setup.advanced.useElasticPlane) {
                //first, reposition the plane on the other side
                otherScript.plane.position = otherScript.originalPlanePosition;

                //calculate trespass progress
                Vector3 elasticPlaneDistance = portal.InverseTransformPoint(setup.playerCamera.position);
                trespassProgress =  elasticPlaneDistance.z * (planeIsInverted ? -1 : 1)
                                    + setup.advanced.elasticPlaneOffset;

                //move the plane
                if (trespassProgress > (0 - setup.advanced.minDistPlayerPlane)) {
                    plane.position = originalPlanePosition + (
                        TowardDestination() * (trespassProgress + setup.advanced.minDistPlayerPlane)
                    );

                    setup.advanced.currentClippingOffset = -(trespassProgress + setup.advanced.minDistPlayerPlane);
                } else {
                    plane.position = originalPlanePosition;
                    setup.advanced.currentClippingOffset = setup.advanced.clippingOffset;
                }

                //teleport player when the progress treshold is reached
                if (setup.advanced.useElasticPlane && trespassProgress > setup.advanced.elasticPlaneTeleportTreshold) {
                    Debug.Log($"teleported because {trespassProgress} > {setup.advanced.elasticPlaneTeleportTreshold}");
                    ConsiderTeleporting(other);
                }
            }
        }


        Transform CreateClone(Transform original, Transform originPortal, bool doTeleport = true) {
            if (cloneParent == null) {
                cloneParent = GameObject.Find("Portal Clones") ?? new GameObject("Portal Clones");
            }

            //this clone already exists? remove it
            if (clones.ContainsKey(original)) TryDestroyClone(original);


            Transform clone = null;
            if (setup.clones.whichSideRealObject == PortalSetup.WhichSideRealObjectOptions.destinationPortal && originPortal == transform) {
                Debug.Log("cc - 1 -- " + portal.name);
                if (doTeleport) DoTeleport(original, true);   //put it on the other side
                otherScript.CreateClone(original, transform); //and then ask the other portal to create here a clone
                return clone;
            } else {
                //originPortal or playerSide
                Debug.Log("cc - 2 -- " + portal.name);
                clone = Instantiate(original.gameObject, cloneParent.transform).transform;
                clone.name = "(portal clone) " + original.name;

                //destroy some components from itself and childrens
                foreach (Rigidbody rb in clone.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
                foreach (Collider col in clone.GetComponentsInChildren<Collider>()) Destroy(col);
                foreach (Camera cam in clone.GetComponentsInChildren<Camera>()) Destroy(cam);
                foreach (CharacterController cc in clone.GetComponentsInChildren<CharacterController>()) Destroy(cc);
                foreach (AudioListener lis in clone.GetComponentsInChildren<AudioListener>()) Destroy(lis);
                foreach (MonoBehaviour scr in clone.GetComponentsInChildren<MonoBehaviour>()) {
                    //if (scr.GetType() != typeof(TMPro.TextMeshPro))
                    Destroy(scr);
                }


                clone.gameObject.tag = "Untagged";



                if (doTeleport) DoTeleport(clone, false);
                clones.Add(original, clone);
                return clone;
            }
        }

        void UpdateClone(Transform original) {
            
            try{

                Transform clone = clones[original];


                //and update it (similar calculations to the actual teleporting)

                //--position
                clone.position = otherScript.portal.TransformPoint(
                    portal.InverseTransformPoint(original.position)
                );

                //--rotation
                clone.rotation =
                    (Quaternion.Inverse(portal.rotation)
                    * otherScript.portal.rotation)
                    * original.rotation
                ;
            }
            catch{

                
            }

        }

        bool ThisObjectCanCross(Transform obj) {
            //player always can cross
            if (obj.CompareTag(setup.filters.playerTag)) return true;

            //main filter
            if (!setup.filters.otherObjectsCanCross) return false;

            //filter: these objects can cross
            if (setup.filters.tagsCanCross.Count > 0 && !setup.filters.tagsCanCross.Contains(obj.tag)) return false;

            //filter: these objects cannot cross
            if (setup.filters.tagsCannotCross.Contains(obj.tag)) return false;

            return true;
        }



    }
}