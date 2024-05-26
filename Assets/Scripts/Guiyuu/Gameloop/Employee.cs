using System.Collections.Generic;
using Guiyuu.BlockDocument;
using Modules.Tween.Scripts;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu.Gameloop {
    public class Employee : MonoBehaviour {
        private static List<Employee> employees = new();
        private static List<Tag> tags = new();

        public static void addTag(Tag tag) {
            tags.Add(tag);
        }

        private static Dictionary<int, bool> secret = new();
        public static Dictionary<string, bool> secret1 = new();
        private static Dictionary<string, (int amount, Employee employee)> deliverables = new();

        private static int hidedEmployee = 0;
        private bool isHided = false;
        public TMP_Text message;

        public GameObject threeAPrefab;
        public GameObject fiveAPrefab;

        private StarCollectionManager scm = null;

        public static int getRequestedAmount(string blockType) {
            if (!deliverables.ContainsKey(blockType)) return 0;
            return deliverables[blockType].amount;
        }

        public static (int amount, Employee employee) getRequestedInfo(string blockType) {
            return deliverables[blockType];
        }

        public static void addDeliverable(string name, (int, Employee) var) {
            deliverables[name] = var;
        }

        public void removeDeliverable(string type, int amount) {
            (int amount, Employee employee) var = deliverables[type];
            var.amount -= amount;
            if (var.amount > 0) deliverables[type] = var;
            else deliverables.Remove(type);

            tag.removeDeliverable(type, amount);

            if (tag.isCleared()) {
                hideAnimation();
                if (scm != null) scm.settleStars();
            }
        }

        public static void startAll() {
            isAllSatisfied = false;
            int fiveA = Random.Range(0, 4);
            int threeA = Random.Range(0, 4);
            while (threeA == fiveA) threeA = Random.Range(0, 4);

            for (int i = 0; i < employees.Count; i++) {
                Employee employee = employees[i];
                if (i == fiveA) employee.start(5);
                else if (i == threeA) employee.start(3);
                else employee.start(0);
            }
        }

        public static void stopAll() {
            foreach (Employee employee in employees) {
                employee.stop();
            }

            secret.Clear();
            secret1.Clear();
            deliverables.Clear();
            hidedEmployee = 0;
        }

        private SpriteRenderer spriteRenderer;
        public Tag tag;

        private void Start() {
            employees.Add(this);
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public static bool isAllSatisfied { get; private set; } = false;

        private void Update() {
            if (hidedEmployee == 4) {
                isAllSatisfied = true;
                CounterCollider.inst.hide();
                hidedEmployee = 0;
                showMessage();
                SkipCounterModule.inst.isSkipAllowed = true;
                Invoke(nameof(hideMessage), 3f);
            }
        }

        private void showMessage() {
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: 0;
                        end-value: 1;
                        duration: 0.375;
                        ease: InOutSine;
                    ");
            tb.setSetter(x => { message.color = new Color(message.color.r, message.color.g, message.color.b, x); });
            tb.register<float>();
        }

        private void hideMessage() {
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: 1;
                        end-value: 0;
                        duration: 0.375;
                        ease: InOutSine;
                    ");
            tb.setSetter(x => { message.color = new Color(message.color.r, message.color.g, message.color.b, x); });
            tb.register<float>();
        }

        private void start(int arg) {
            Sprite employee = BlockDocuments.getRandomEmployee(secret);
            spriteRenderer.sprite = employee;
            tag.start();
            showAnimation();

            GameObject go = null;
            if (arg == 3)
                go = Instantiate(threeAPrefab, this.transform);
            else if (arg == 5)
                go = Instantiate(fiveAPrefab, this.transform);
            if (go is not null) {
                go.transform.localPosition =
                    new Vector3(go.transform.localPosition.x, -0.425f, go.transform.localPosition.z);
                scm = go.GetComponent<StarCollectionManager>();
            }
        }

        private void stop() {
            spriteRenderer.sprite = null;
            tag.stop();
            if (!isHided) hideAnimation();
        }

        private void showAnimation() {
            isHided = false;
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: -0.2,0,0.1;
                        end-value: 1,0.62,1;
                        duration: 0.375;
                        ease: InOutSine;
                    ");
            tb.setSetterX(x => {
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g,
                        spriteRenderer.color.b, x);
                    tag.spriteRenderer.color = new Color(tag.spriteRenderer.color.r, tag.spriteRenderer.color.g,
                        tag.spriteRenderer.color.b, x + 0.2f);
                })
                .setSetterY(x => { tag.transform.localPosition = new Vector3(0, x, 0); })
                .setSetterZ(x => { tag.transform.localScale = new Vector3(x, x, x); });
            tb.register<Vector3>();
        }

        private void hideAnimation() {
            isHided = true;
            hidedEmployee++;
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: 1,0.62,1;
                        end-value: -0.2,0,0.1;
                        duration: 0.375;
                        ease: InOutSine;
                    ");
            tb.setSetterX(x => {
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g,
                        spriteRenderer.color.b, x);
                    tag.spriteRenderer.color = new Color(tag.spriteRenderer.color.r, tag.spriteRenderer.color.g,
                        tag.spriteRenderer.color.b, x + 0.2f);
                })
                .setSetterY(x => { tag.transform.localPosition = new Vector3(0, x, 0); })
                .setSetterZ(x => { tag.transform.localScale = new Vector3(x, x, x); });
            tb.register<Vector3>();

            if (scm is not null) scm.hideAll();
        }
    }
}