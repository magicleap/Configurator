using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace Microsoft.MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Implements a generic movelogic that works for most/all XRI interactors,
    /// assuming a well-defined attachTransform. 
    /// 
    /// Usage:
    /// When a manipulation starts, call Setup.
    /// Call Update any time to update the move logic and get a new rotation for the object.
    /// </summary>
    public class SnappingMoveLogic : ManipulationLogic<Vector3>
    {
        private Vector3 attachToObject;
        private Vector3 objectLocalAttachPoint;

        /// <inheritdoc />
        public override void Setup(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget)
        {
            base.Setup(interactors, interactable, currentTarget);

            Vector3 attachCentroid = GetAttachCentroid(interactors, interactable);

            attachToObject = currentTarget.Position - attachCentroid;
            objectLocalAttachPoint = Quaternion.Inverse(currentTarget.Rotation) * (attachCentroid - currentTarget.Position);
            objectLocalAttachPoint = objectLocalAttachPoint.Div(currentTarget.Scale);
        }

        private float snapDist = 0.55f;
        private float detachDist = 1.5f;

        private bool shouldDetach = false;

        private Timer detachTimer;

        /// <inheritdoc />
        public override Vector3 Update(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget, bool centeredAnchor)
        {
            base.Update(interactors, interactable, currentTarget, centeredAnchor);

            Vector3 attachCentroid = GetAttachCentroid(interactors, interactable);
            bool snappedToFloor = false;


            if (centeredAnchor)
            {
                Vector3 endPos = attachCentroid + attachToObject;
                GameObject curConfigurable = UIManager.Instance.CurrentConfigurable.gameObject;
                //Get nearest floor plane
                ARPlane floorPlane =
                    PlanesManager.Instance.GetNearestPlane(curConfigurable.transform.position,
                        PlaneClassification.Floor);

                bool shouldSnap = floorPlane != null && Settings.FloorSnappingEnabled && !shouldDetach;

                //Ray from object position, down
                Ray snapRay = new Ray(curConfigurable.transform.position, Vector3.down);

                Transform interactorTransform = interactors.First().transform;
                if (Physics.Raycast(snapRay, out RaycastHit floorHitInfo, snapDist, LayerMask.GetMask("Planes")) &&
                    shouldSnap)
                {
                    Vector3 detachEndPoint = interactorTransform.position +
                                             (interactorTransform.forward *
                                              Vector3.Distance(curConfigurable.transform.position,
                                                  interactorTransform.position));

                    float currentDetachDistance = Vector3.Distance(detachEndPoint, floorHitInfo.point);
                    if (currentDetachDistance >= detachDist)
                    {
                        shouldDetach = true;
                        //set timer for 1s
                        //This is to delay attaching logic and avoid 'jitter' when snapping or detaching
                        detachTimer = new Timer(1000);
                        detachTimer.Elapsed += ResetDetach;
                        detachTimer.Start();
                    }
                    else
                    {
                        endPos.y = floorHitInfo.point.y;
                        snappedToFloor = true;
                    }
                }


                //attempt to snap to walls
                //WALL SNAPPING LOGIC***************
                if (Physics.Raycast(interactorTransform.position, interactorTransform.forward,
                        out RaycastHit controllerHitInfo, float.MaxValue, LayerMask.GetMask("Planes")))
                {
                    //get the wall plane
                    ARPlane wallPlane = controllerHitInfo.collider.gameObject.GetComponent<ARPlane>();

                    //if we should snap
                    shouldSnap = wallPlane != null && wallPlane.classification == PlaneClassification.Wall &&
                                 Settings.WallSnappingEnabled && !shouldDetach;

                    if (shouldSnap)
                    {
                        Vector3 hitPos = controllerHitInfo.point;
                        //get all snap anchors
                        List<SnapAnchor> snapAnchors = curConfigurable
                            .GetComponentsInChildren<SnapAnchor>().ToList();

                        float closestAnchorDistance = float.MaxValue;
                        SnapAnchor closestAnchor = null;
                        float largestAngle = 0;
                        foreach (SnapAnchor anchor in snapAnchors)
                        {
                            //check every snap anchor to see if it is behind the current wall or not
                            float angle = Vector3.SignedAngle(wallPlane.transform.up, anchor.Position - hitPos,
                                Vector3.up);
                            //make angle 0-360
                            if (angle < 0)
                            {
                                angle = 360 - angle * -1;
                            }

                            if (angle > 110 && angle < 250)
                            {
                                //the anchor is behind the plane, it should be used
                                if (angle > largestAngle)
                                {
                                    largestAngle = angle;
                                    closestAnchor = anchor;
                                }
                            }
                        }

                        //if no anchors are behind the wall, do a basic distance check to find the closest anchor
                        //this will be the anchor we use as the offset
                        if (closestAnchor == null)
                        {
                            foreach (SnapAnchor anchor in snapAnchors)
                            {
                                float dist = Vector3.Distance(hitPos, anchor.Position);
                                if (dist < closestAnchorDistance)
                                {
                                    closestAnchorDistance = dist;
                                    closestAnchor = anchor;
                                }
                            }
                        }
                        
                        Vector3 offset = Vector3.zero;
                        
                        //if we have found an anchor to use, apply the offset
                        if (closestAnchor != null)
                        {
                            Vector3 anchorPos = closestAnchor.Position;
                            anchorPos.y = 0;
                            Vector3 offsetTargetPosition = curConfigurable.transform.position;
                            offsetTargetPosition.y = 0;
                            offset += wallPlane.transform.up * Vector3.Distance(offsetTargetPosition, anchorPos);
                        }

                        //the offset = hitPos + wall.transform.up * distance
                        endPos = hitPos + offset;

                        //if we are snapped to the floor, stay snapped to the floor plane
                        if (snappedToFloor)
                        {
                            endPos.y = floorHitInfo.point.y;
                        }
                    }
                }

                return endPos;
            }
            else
            {
                Vector3 scaledLocalAttach = Vector3.Scale(objectLocalAttachPoint, currentTarget.Scale);
                Vector3 worldAttachPoint = currentTarget.Rotation * scaledLocalAttach + currentTarget.Position;
                return currentTarget.Position + (attachCentroid - worldAttachPoint);
            }
        }

        private Vector3 GetAttachCentroid(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable)
        {
            // TODO: This uses the attachTransform ONLY, which can possibly be
            // unstable/imprecise (see GrabInteractor, etc.) Old version used to use the interactor
            // transform in the case where there was only one interactor, and the attachTransform
            // when there were 2+. The interactor should stabilize its attachTransform
            // to get a similar effect. Possibly, we should stabilize grabs on the thumb, or some
            // other technique.

            Vector3 sumPos = Vector3.zero;
            int count = 0;
            foreach (IXRSelectInteractor interactor in interactors)
            {
                sumPos += interactor.GetAttachTransform(interactable).position;
                count++;
            }

            return sumPos / Mathf.Max(1, count);
        }

        void ResetDetach(object sender, ElapsedEventArgs e)
        {
            shouldDetach = false;
            detachTimer.Stop();
        }
    }
}