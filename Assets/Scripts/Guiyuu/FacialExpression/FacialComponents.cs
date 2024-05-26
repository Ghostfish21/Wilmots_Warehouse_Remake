using System.Collections.Generic;
using ModuleManager;
using UnityEngine;

namespace Guiyuu.FacialExpression {
    public class FacialComponets : Module {
        public Transform player { get; private set; }

        public Transform noseController;
        public Transform mouthController;
        public Transform eyesController;
        public Transform mouseZoneDetection;

        public override void construct() {
            GameObject player = GameObject.Find("Player");
            this.player = player.transform;

            this.noseController.SetParent(this.player);
            this.noseController.transform.localPosition = new Vector3(0, 0, 0);
            this.mouthController.SetParent(this.player);
            this.mouthController.transform.localPosition = new Vector3(0, 0, 0);
            this.eyesController.SetParent(this.player);
            this.eyesController.transform.localPosition = new Vector3(0, 0, 0);
            this.mouseZoneDetection.SetParent(this.player);
            this.mouseZoneDetection.transform.localPosition = new Vector3(0, 0, 0);
        }

        public override List<string> getDependencies() {
            List<string> dependencies = new List<string>();
            dependencies.Add("GridModule");
            return dependencies;
        }

        public override string getName() {
            return "FacialComponents";
        }
    }
}