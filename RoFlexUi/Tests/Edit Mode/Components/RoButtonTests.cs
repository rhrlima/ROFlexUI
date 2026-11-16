using RO_Flex_UI.Components;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RO_Flex_UI.Tests
{
    public class RoButtonTests
    {
        private const string PrefabPath = Setup.PrefabRoot + "Components/RoButton.prefab";

        [Test]
        public void SetLabelFromCode()
        {
            var button = CreateRoButton();
            button.Label = "Test Label";

            Assert.IsNotNull(button.Label, "RoButton.Label should not be null");
            Assert.AreEqual("Test Label", button.Label, $"RoButton.Label expected: {"Test Label"}. Got: {button.Label}");

            Object.DestroyImmediate(button.gameObject);
        }

        [Test]
        public void InteractableCanBeChangedFromCode()
        {
            var button = CreateRoButton();

            button.interactable = false;
            Assert.IsFalse(button.interactable, "RoButton should report interactable false after being disabled from code.");

            button.interactable = true;
            Assert.IsTrue(button.interactable, "RoButton should report interactable true after being enabled from code.");

            Object.DestroyImmediate(button.gameObject);
        }

        [Test]
        public void OnClickInvokesRegisteredListener()
        {
            var button = CreateRoButton();
            var clickCount = 0;

            button.onClick.AddListener(() => clickCount++);
            button.onClick.Invoke();

            Assert.AreEqual(1, clickCount, "RoButton.onClick should invoke a registered listener exactly once.");

            Object.DestroyImmediate(button.gameObject);
        }

        [Test]
        public void OnClickDoesNotInvokeRemovedListener()
        {
            var button = CreateRoButton();
            var clickCount = 0;
            void listener() => clickCount++;

            button.onClick.AddListener(listener);
            button.onClick.RemoveListener(listener);
            button.onClick.Invoke();

            Assert.AreEqual(0, clickCount, "RoButton.onClick should not invoke a listener after it has been removed.");

            Object.DestroyImmediate(button.gameObject);
        }

        [Test]
        public void CanBeInstantiatedFromPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, $"Expected to load prefab at path: {PrefabPath}");

            var instance = Object.Instantiate(prefab);
            Assert.IsNotNull(instance, $"Expected to instantiate prefab at path: {PrefabPath}");

            var button = instance.GetComponent<RoButton>();
            Assert.IsNotNull(button, $"Expected root object of {PrefabPath} to contain {typeof(RoButton).FullName}.");

            Object.DestroyImmediate(instance);
        }

        [TestCase(PointerEventData.InputButton.Left, 1, 1)]
        [TestCase(PointerEventData.InputButton.Left, 2, 1)]
        [TestCase(PointerEventData.InputButton.Right, 1, 0)]
        [TestCase(PointerEventData.InputButton.Middle, 1, 0)]
        public void PointerClicksPreserveDataAndOrdinaryActivation(
            PointerEventData.InputButton input, int count, int expectedActivation)
        {
            var button = CreateRoButton();
            try
            {
                var activations = 0;
                var pointerCalls = 0;
                PointerEventData received = null;
                button.onClick.AddListener(() => activations++);
                button.onPointerClick.AddListener(data => { received = data; pointerCalls++; });
                var click = new PointerEventData(null) { button = input, clickCount = count };

                button.OnPointerClick(click);

                Assert.AreEqual(expectedActivation, activations);
                Assert.AreEqual(1, pointerCalls);
                Assert.AreSame(click, received);
            }
            finally { Object.DestroyImmediate(button.gameObject); }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UnavailableButtonDoesNotEmitClicks(bool deactivate)
        {
            var button = CreateRoButton();
            try
            {
                var calls = 0;
                button.onClick.AddListener(() => calls++);
                button.onPointerClick.AddListener(_ => calls++);
                if (deactivate) button.gameObject.SetActive(false);
                else button.interactable = false;

                button.OnPointerClick(new PointerEventData(null));
                button.OnPointerClick(new PointerEventData(null) { button = PointerEventData.InputButton.Right });
                button.OnSubmit(new BaseEventData(null));

                Assert.AreEqual(0, calls);
            }
            finally { Object.DestroyImmediate(button.gameObject); }
        }

        [Test]
        public void SubmitActivatesWithoutEmittingPointerClick()
        {
            var button = CreateRoButton();
            try
            {
                var activations = 0;
                var pointers = 0;
                // Avoid running the submit animation coroutine in Edit Mode.
                button.onClick.AddListener(() => { activations++; button.interactable = false; });
                button.onPointerClick.AddListener(_ => pointers++);

                button.OnSubmit(new BaseEventData(null));

                Assert.AreEqual(1, activations);
                Assert.AreEqual(0, pointers);
            }
            finally { Object.DestroyImmediate(button.gameObject); }
        }

        [Test]
        public void ActivationThatDisablesButtonStopsPointerDispatch()
        {
            var button = CreateRoButton();
            try
            {
                var calls = 0;
                button.onClick.AddListener(() => button.gameObject.SetActive(false));
                button.onPointerClick.AddListener(_ => calls++);
                button.OnPointerClick(new PointerEventData(null));
                Assert.AreEqual(0, calls);
            }
            finally { Object.DestroyImmediate(button.gameObject); }
        }

        [Test]
        public void PointerListenerCanBeRemoved()
        {
            var button = CreateRoButton();
            try
            {
                var calls = 0;
                void Listener(PointerEventData _) => calls++;
                button.onPointerClick.AddListener(Listener);
                button.onPointerClick.RemoveListener(Listener);
                button.OnPointerClick(new PointerEventData(null));
                Assert.AreEqual(0, calls);
            }
            finally { Object.DestroyImmediate(button.gameObject); }
        }

        [Test]
        public void InspectorListenerSurvivesSerialization()
        {
            var button = CreateRoButton();
            var target = CreateRoButton();
            target.transform.SetParent(button.transform, false);
            var path = "Assets/RoButtonClickTest-" + System.Guid.NewGuid().ToString("N") + ".prefab";
            try
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    button.onPointerClick, target.OnPointerClick);
                PrefabUtility.SaveAsPrefabAsset(button.gameObject, path);
                var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<RoButton>();

                Assert.AreEqual(1, saved.onPointerClick.GetPersistentEventCount());
                Assert.AreSame(saved.transform.GetChild(saved.transform.childCount - 1).GetComponent<RoButton>(),
                    saved.onPointerClick.GetPersistentTarget(0));
                Assert.AreEqual("OnPointerClick", saved.onPointerClick.GetPersistentMethodName(0));
                var editor = UnityEditor.Editor.CreateEditor(button);
                try { Assert.IsInstanceOf<RO_Flex_UI.Editor.RoButtonEditor>(editor); }
                finally { Object.DestroyImmediate(editor); }
            }
            finally
            {
                Object.DestroyImmediate(button.gameObject);
                AssetDatabase.DeleteAsset(path);
            }
        }

        private RoButton CreateRoButton()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var instance = Object.Instantiate(prefab);
            var button = instance.GetComponent<RoButton>();
            return button;
        }
    }
}
