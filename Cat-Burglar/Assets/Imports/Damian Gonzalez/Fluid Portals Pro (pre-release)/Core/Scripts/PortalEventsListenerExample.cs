using System.Collections.Generic;
using UnityEngine;
using DamianGonzalez.Portals;


    [DefaultExecutionOrder(999)]
    public class PortalEventsListenerExample : MonoBehaviour {
        private void Start() {
            //subscription to public events
            PortalEvents.teleport += somethingTeleported;
            PortalEvents.setupComplete += portalSetupComplete;
            PortalEvents.gameResized += gameWindowHasResized;
            //PortalEvents.rayThroughPortals += rayWentThroughPortals;


        }

        void somethingTeleported(string groupId, Transform portalFrom, Transform portalTo, Transform objectTeleported, Vector3 positionFrom, Vector3 positionTo) {
            Debug.Log(
                objectTeleported.name + " teleported" +
                " from " + groupId + "." + portalFrom.name + " " + positionFrom.ToString() +
                " to " + groupId + "." + portalTo.name + " " + positionTo.ToString()
            , objectTeleported);
        }


        void portalSetupComplete(string groupId, Transform portal) {

        }

        void gameWindowHasResized(string groupId, Transform portal, Vector2 oldSize, Vector2 newSize) {
            Debug.Log(
                "Game window has resized from " + oldSize + " to " + newSize + ". " +
                "Therefore, " + portal.name + "(" + groupId + ") updated its cameras."
            );
        }

    }
