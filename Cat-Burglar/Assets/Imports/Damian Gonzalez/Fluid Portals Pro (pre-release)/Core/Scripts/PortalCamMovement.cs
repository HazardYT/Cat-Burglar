using System.Collections.Generic;
using System;
using UnityEngine;


/* 
 * this script controls what the plane on THIS portal displays,
 * moving the camera around the OTHER portal
 */

namespace DamianGonzalez.Portals {
    public class PortalCamMovement : MonoBehaviour {
        //these 10 variables are assigned automatically by the Setup script

        [HideInInspector] public Transform playerCamera;
        [HideInInspector] public PortalSetup setup;
        [HideInInspector] public Transform mainObject;
        [HideInInspector] public PortalCamMovement otherScript;
        [HideInInspector] public Transform portal;
        [HideInInspector] public Camera _camera;
        [HideInInspector] public Transform _plane;
        [HideInInspector] public Renderer _renderer;
        [HideInInspector] public MeshFilter _filter;
        [HideInInspector] public Collider _collider;
        [HideInInspector] public bool inverted;
        [HideInInspector] public Transform shadowClone;

        public List<PortalCamMovement> dep = new List<PortalCamMovement>();

        Camera playerCameraComp;

        public string cameraId; //useful if some debugging is needed. This is automatically assigned by portalSetup
        public Vector3 offset; //only used for a special case in the "long tunnel effect".
        public Vector3 angOffset;


        [Header ("Debug info")]
        [SerializeField] private string renderPasses;   //as text
        [HideInInspector] public int int_renderPasses;  //as int, just for counting

        [System.Serializable]
        public class posAndRot {
            public Vector3 position;
            public Quaternion rotation;
        }
        List<posAndRot> recursions = new List<posAndRot>();


        private void Start() {
            if (playerCamera == null) playerCamera = Camera.main.transform;
            playerCameraComp = playerCamera.GetComponent<Camera>();
        }

        public void CalculateNormalPositionAndRotation(Transform tr, Transform reference, Transform thisPortal, Transform otherPortal) {
            //rotation
            tr.rotation =
                 (thisPortal.rotation
                 * Quaternion.Inverse(otherPortal.rotation))
                 * Quaternion.Euler(angOffset)
                 * reference.rotation
            ;


            //position
            Vector3 distanceFromPlayerToPortal = reference.position - (otherPortal.position);
            Vector3 whereTheOtherCamShouldBe = thisPortal.position + (distanceFromPlayerToPortal) + offset;
            tr.position = RotatePointAroundPivot(
                whereTheOtherCamShouldBe,
                thisPortal.position,
                (thisPortal.rotation * Quaternion.Inverse(otherPortal.rotation)).eulerAngles
            );

        }


        void ApplyAdvancedOffset(bool ignoreNearFilter = true) {
            //first, if player is too near to the plane, don't apply the advanced offset
            if (!ignoreNearFilter && (Vector3.Distance(
                    otherScript._collider.ClosestPoint(playerCamera.position),
                    playerCamera.position
                ) < setup.advanced.dontAlignNearerThan)) {
                _camera.projectionMatrix = playerCameraComp.projectionMatrix;
                return;
            }
            //not too near, continue.

            Vector3 point =
            _plane.position
            + portal.forward * setup.advanced.dotCalculationOffset * (inverted ? -1f : 1f)
            - transform.position;

            int dot = Math.Sign(Vector3.Dot(portal.forward, point));

            //rotate near clipping plane, so it matches portal rotation

            Plane p = new Plane(portal.forward * dot, _plane.position);
            Vector4 clipPlane = new Vector4(p.normal.x, p.normal.y, p.normal.z,
            p.distance + playerCameraComp.nearClipPlane + setup.advanced.currentClippingOffset);

            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(_camera.worldToCameraMatrix)) * clipPlane;
            var newMatrix = playerCameraComp.CalculateObliqueMatrix(clipPlaneCameraSpace);

            _camera.projectionMatrix = newMatrix;
        }


        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }


        public void Recalculate() {
            if (setup.useShadowClone && shadowClone != null)
                CalculateNormalPositionAndRotation(shadowClone, playerCamera.parent, portal, otherScript.portal);

            CalculateNormalPositionAndRotation(transform, playerCamera, portal, otherScript.portal);
        }


        public bool[] opc = new bool[10];
        public void ManualRenderIfNecessary() {
            Recalculate();

            foreach (PortalCamMovement c in dep) {
                if (ShouldRenderCamera(c._renderer, c.playerCameraComp, c._plane)) {
                    transform.position -= (c.portal.position - c.otherScript.portal.position);

                    ManualRenderNotRecursive();
                    return;
                }
            }





            //only render cameras when player is seeing the planes that render them
            if (!ShouldRenderCamera(otherScript._renderer, playerCameraComp, otherScript._plane)) {
                int_renderPasses = 0;
                renderPasses = "0 of 1 (not rendering)";
                return;
            }

            //if asked, mimic the field of view of the main camera
            if (setup.advanced.alwaysMimicPlayersFOV) {
                _camera.fieldOfView = playerCameraComp.fieldOfView;
            }

            if (setup.advanced.useRecursiveRendering)
                ManualRenderRecursive();
            else
                ManualRenderNotRecursive();
        }

        void ManualRenderNotRecursive() {

            if (setup.advanced.alignNearClippingPlane) ApplyAdvancedOffset();
            _renderer.enabled = false; //the other plane may get in the way
            _camera.Render();
            _renderer.enabled = true;

            int_renderPasses = 1;
            renderPasses = "1 of 1 (rendering)";
        }

        public float recursivePosOffset;
        void ManualRenderRecursive() {

            if (setup.doubleSided) _renderer.enabled = false; //if double sided, the other plane gets in the way

            int_renderPasses = 0;

            recursions.Clear();

            Matrix4x4 localToWorldMatrix = playerCamera.transform.localToWorldMatrix;
            _camera.projectionMatrix = playerCameraComp.projectionMatrix;


            //from first "normal" position, it calculates the inner rendering positions until the plane is not visible
            for (int i = 0; i < setup.advanced.maximumRenderPasses; i++) {

                //calculate
                localToWorldMatrix = portal.localToWorldMatrix * otherScript.portal.transform.worldToLocalMatrix * localToWorldMatrix;

                transform.SetPositionAndRotation(
                    localToWorldMatrix.GetColumn(3),
                    localToWorldMatrix.rotation
                );

                recursions.Insert(0, new posAndRot() {
                    position = transform.position - transform.forward * recursivePosOffset,
                    rotation = transform.rotation
                });

                //check: does this recursion sees the portal? If not, stop recursion
                //if (!ShouldRenderCamera(otherScript._renderer, _camera, otherScript._plane)) {
                //if (ShouldRenderCamera(_renderer, _camera, _plane) {
                //if (!CameraUtility.BoundsOverlap(_filter, otherScript._filter, _camera)) {
                //break;
                //}
            }


            //with positions and rotations calculated, now render them in reverse order
            //(inner recursions first)
            for (int i = 0; i < recursions.Count; i++) {
                transform.SetPositionAndRotation(recursions[i].position, recursions[i].rotation);
                //if (setup.advanced.alignNearClippingPlane) 
                ApplyAdvancedOffset();
                try {
                    _camera.Render();
                }
                catch {
                    
                }


                int_renderPasses++;
            }

            if (setup.doubleSided) _renderer.enabled = true;



            renderPasses = int_renderPasses + " of " + setup.advanced.maximumRenderPasses; //display info, useful for debug

        }

        public bool debugThis = false;
        public bool ShouldRenderCamera(Renderer renderer, Camera camera, Transform plane) {
            if (setup.forceActivateCamsInNextFrame) return true;
            if (!setup.advanced.disableUnnecessaryCameras) return true;
            

            //1st filter: distance
            if (setup.advanced.disableDistantCameras) {
                if (Vector3.Distance(plane.position, playerCamera.position) > setup.advanced.fartherThan) return false;
            }

            //2nd filter: portal is visible?
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds)) return false;

            //3rd filter (only for one-sided portals): portal visible, but is the "good" side visible?
            if (setup.doubleSided) return true;

            float dotProduct = Vector3.Dot(
                inverted ? -otherScript.portal.forward : otherScript.portal.forward,
                playerCamera.position - otherScript.portal.position
            );

            

            //if (debugThis) DebugText.Show(dotProduct.ToString());
            return (dotProduct > 0 - dotMarginForCams); //instead of 0, let's give it a safe margin in case player is crossing sideways

        }
        public float dotMarginForCams = .5f;


    }
}