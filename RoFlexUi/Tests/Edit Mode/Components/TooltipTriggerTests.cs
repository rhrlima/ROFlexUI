using NUnit.Framework;
using RO_Flex_UI.Components;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RO_Flex_UI.Tests
{
    public class TooltipTriggerTests
    {
        private GameObject root;
        private TooltipBox box;
        private TMP_Text displayedText;
        private TooltipTrigger first;
        private TooltipTrigger second;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                Setup.PrefabRoot + "Components/Tooltip Proxy.prefab");
            Assert.IsNotNull(prefab);
            var proxy = Object.Instantiate(prefab, root.transform);
            box = proxy.GetComponent<TooltipBox>();
            displayedText = proxy.GetComponentInChildren<TMP_Text>(true);
            first = CreateTrigger("First");
            second = CreateTrigger("Second");
            first.SetText("First item");
            second.SetText("Second item");
            box.HideTooltip();
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy triggers before the proxy referenced by their OnDisable callbacks.
            if (first != null) Object.DestroyImmediate(first.gameObject);
            if (second != null) Object.DestroyImmediate(second.gameObject);
            if (root != null) Object.DestroyImmediate(root);
        }

        [Test]
        public void UpdatingAnotherTriggerDoesNotReplaceVisibleTooltip()
        {
            first.OnPointerEnter(new PointerEventData(null));
            Assert.AreEqual("First item", displayedText.text);

            second.SetText("Second item updated");

            Assert.AreEqual("Second item updated", second.TooltipText);
            Assert.AreEqual("First item", displayedText.text);
            Assert.IsTrue(box.gameObject.activeSelf);
        }

        [Test]
        public void UpdatingHoveredTriggerOnlyChangesDisplayOnNextEntry()
        {
            var pointer = new PointerEventData(null);
            first.OnPointerEnter(pointer);

            first.SetText("First item updated");

            Assert.AreEqual("First item updated", first.TooltipText);
            Assert.AreEqual("First item", displayedText.text);
            first.OnPointerExit(pointer);
            first.OnPointerEnter(pointer);
            Assert.AreEqual("First item updated", displayedText.text);
            Assert.IsTrue(box.gameObject.activeSelf);
        }

        [Test]
        public void EnteringAnotherTriggerDisplaysItsLatestStoredText()
        {
            var pointer = new PointerEventData(null);
            first.OnPointerEnter(pointer);
            second.SetText("Second item updated");
            first.OnPointerExit(pointer);

            second.OnPointerEnter(pointer);

            Assert.AreEqual("Second item updated", displayedText.text);
            Assert.IsTrue(box.gameObject.activeSelf);
        }

        [Test]
        public void OnTriggerCanRefreshTextForTheSamePointerEntry()
        {
            first.OnTrigger += () => first.SetText("Refreshed on entry");

            first.OnPointerEnter(new PointerEventData(null));

            Assert.AreEqual("Refreshed on entry", displayedText.text);
            Assert.IsTrue(box.gameObject.activeSelf);
        }

        [TestCase("window")]
        [TestCase("exit")]
        [TestCase("drag")]
        [TestCase("disabled-entry")]
        public void UnrelatedTriggerCannotHideVisibleTooltip(string action)
        {
            var window = new GameObject("Other window");
            window.transform.SetParent(root.transform, false);
            second.transform.SetParent(window.transform, false);
            var pointer = new PointerEventData(null);
            // Transfer ownership without relying on an exit event arriving first.
            second.OnPointerEnter(pointer);
            first.OnPointerEnter(pointer);

            Dismiss(second, action, window);

            Assert.IsTrue(box.gameObject.activeSelf);
            Assert.AreEqual("First item", displayedText.text);
        }

        [TestCase("window")]
        [TestCase("exit")]
        [TestCase("drag")]
        [TestCase("disabled-entry")]
        public void CurrentTriggerCanHideItsTooltip(string action)
        {
            var window = new GameObject("Current window");
            window.transform.SetParent(root.transform, false);
            first.transform.SetParent(window.transform, false);
            first.OnPointerEnter(new PointerEventData(null));

            Dismiss(first, action, window);

            Assert.IsFalse(box.gameObject.activeSelf);
            second.OnPointerEnter(new PointerEventData(null));
            Assert.IsTrue(box.gameObject.activeSelf);
            Assert.AreEqual("Second item", displayedText.text);
        }

        private static void Dismiss(TooltipTrigger trigger, string action, GameObject window)
        {
            var pointer = new PointerEventData(null);
            switch (action)
            {
                case "window":
                    window.SetActive(false);
                    // Edit Mode does not dispatch runtime MonoBehaviour lifecycle callbacks.
                    typeof(TooltipTrigger).GetMethod("OnDisable",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        .Invoke(trigger, null);
                    break;
                case "exit": trigger.OnPointerExit(pointer); break;
                case "drag": trigger.OnBeginDrag(pointer); break;
                case "disabled-entry":
                    trigger.Enabled = false;
                    trigger.OnPointerEnter(pointer);
                    break;
            }
        }

        private TooltipTrigger CreateTrigger(string name)
        {
            var target = new GameObject(name, typeof(RectTransform));
            target.transform.SetParent(root.transform, false);
            var trigger = target.AddComponent<TooltipTrigger>();
            var serialized = new SerializedObject(trigger);
            serialized.FindProperty("tooltipComponent").objectReferenceValue = box;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return trigger;
        }
    }
}
