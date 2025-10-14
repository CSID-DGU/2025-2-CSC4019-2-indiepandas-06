using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using System;

using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Element Editor Extensions
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
public static class ElementEditorExtensions {

	// Constants

	public const float Node1U = 128f;
	public const float Node2U = 224f;
	public const float Node2ULabel = 56f;



	// Layout Methods

	public static Label Label(string text) {
		var element = new Label(text);
		element.style.marginTop = 2f;
		element.style.marginBottom = 2f;
		element.style.marginLeft = 3f;
		element.style.marginRight = -1f;
		element.style.width = Node2ULabel;
		return element;
	}

	public static Button Button(string text, Action onClick) {
		var element = new Button(onClick) { text = text };
		element.style.marginTop = 1f;
		element.style.marginBottom = 1f;
		element.style.marginLeft = 3f;
		element.style.marginRight = 3f;
		element.style.height = 18f;
		return element;
	}



	// Field Methods

	public static IntegerField IntField(
		int value, Action<int> onValueChanged) {
		var element = new IntegerField() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static IntegerField IntField(
		string label, int value, Action<int> onValueChanged) {
		var element = new IntegerField(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}

	public static Toggle Toggle(
		bool value, Action<bool> onValueChanged) {
		var element = new Toggle() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		return element;
	}

	public static Toggle Toggle(
		string label, bool value, Action<bool> onValueChanged) {
		var element = new Toggle(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		return element;
	}

	public static FloatField FloatField(
		float value, Action<float> onValueChanged) {
		var element = new FloatField() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static FloatField FloatField(
		string label, float value, Action<float> onValueChanged) {
		var element = new FloatField(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}

	public static VisualElement Slider(
		float value, float min, float max, Action<float> onValueChanged) {
		var element = Slider(string.Empty, value, min, max, onValueChanged);
		element.RemoveAt(0);
		var child0 = element.ElementAt(0);
		var child1 = element.ElementAt(1);
		child0.style.minWidth = Node1U - 14f - 35f;
		child0.style.maxWidth = Node1U - 14f - 35f;
		child1.style.minWidth = 35f;
		child1.style.maxWidth = 35f;
		return element;
	}

	public static VisualElement Slider(
		string label, float value, float min, float max, Action<float> onValueChanged) {
		var element = new VisualElement();
		element.style.flexDirection = FlexDirection.Row;
		element.Add(Label(label));
		var child1 = new Slider(min, max) { value = value };
		var child2 = new FloatField() { value = value };
		child1.RegisterValueChangedCallback(callback => {
			child2.value = callback.newValue;
			onValueChanged(callback.newValue);
		});
		child2.RegisterValueChangedCallback(callback => {
			child1.value = callback.newValue;
			onValueChanged(callback.newValue);
		});
		child1.style.minWidth = Node2U - Node2ULabel - 16f - 35f;
		child1.style.maxWidth = Node2U - Node2ULabel - 16f - 35f;
		child2.style.minWidth = 35f;
		child2.style.maxWidth = 35f;
		element.Add(child1);
		element.Add(child2);
		return element;
	}

	public static Vector2Field Vector2Field(
		Vector2 value, Action<Vector2> onValueChanged) {
		var element = new Vector2Field() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static Vector2Field Vector2Field(
		string label, Vector2 value, Action<Vector2> onValueChanged) {
		var element = new Vector2Field(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}

	public static Vector3Field Vector3Field(
		Vector3 value, Action<Vector3> onValueChanged) {
		var element = new Vector3Field() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static Vector3Field Vector3Field(
		string label, Vector3 value, Action<Vector3> onValueChanged) {
		var element = new Vector3Field(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		element.ElementAt(1).ElementAt(0).style.width = 35f;
		element.ElementAt(1).ElementAt(1).style.width = 35f;
		element.ElementAt(1).ElementAt(2).style.width = 35f;
		return element;
	}

	public static ColorField ColorField(
		Color value, Action<Color> onValueChanged) {
		var element = new ColorField() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static ColorField ColorField(
		string label, Color value, Action<Color> onValueChanged) {
		var element = new ColorField(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}

	public static EnumField EnumField<T>(
		T value, Action<T> onValueChanged) where T : Enum {
		var element = new EnumField(default(T)) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged((T)callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static EnumField EnumField<T>(
		string label, T value, Action<T> onValueChanged) where T : Enum {
		var element = new EnumField(label, default(T)) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged((T)callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}

	public static VisualElement TextEnumField<T>(
		T value, Action<T> onValueChanged) where T : Enum {
		var element = TextEnumField(string.Empty, value, onValueChanged);
		element.RemoveAt(0);
		var child0 = element.ElementAt(0);
		var child1 = element.ElementAt(1);
		child0.style.minWidth = (Node1U - 14f) * 0.5f;
		child0.style.maxWidth = (Node1U - 14f) * 0.5f;
		child1.style.minWidth = (Node1U - 14f) * 0.5f;
		child1.style.maxWidth = (Node1U - 14f) * 0.5f;
		return element;
	}

	public static VisualElement TextEnumField<T>(
		string label, T value, Action<T> onValueChanged) where T : Enum {
		var element = new VisualElement();
		element.style.flexDirection = FlexDirection.Row;
		element.Add(Label(label));
		var child1 = new TextField() { value = value.ToString() };
		var child2 = new EnumField(default(T)) { value = value };
		child1.RegisterValueChangedCallback(callback => {
			var name = callback.newValue;
			if (Enum.TryParse(typeof(T), name, out var t)) {
				child2.value = (T)t;
				onValueChanged((T)t);
			}
		});
		child2.RegisterValueChangedCallback(callback => {
			child1.value = callback.newValue.ToString();
			onValueChanged((T)callback.newValue);
		});
		child1.style.minWidth = (Node2U - Node2ULabel - 16f) * 0.5f;
		child1.style.maxWidth = (Node2U - Node2ULabel - 16f) * 0.5f;
		child2.style.minWidth = (Node2U - Node2ULabel - 16f) * 0.5f;
		child2.style.maxWidth = (Node2U - Node2ULabel - 16f) * 0.5f;
		element.Add(child1);
		element.Add(child2);
		return element;
	}

	public static TextField TextField(
		string value, Action<string> onValueChanged) {
		var element = new TextField() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static TextField TextField(
		string label, string value, Action<string> onValueChanged) {
		var element = new TextField(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}

	public static ObjectField ObjectField<T>(
		T value, Action<T> onValueChanged) where T : Object {
		var element = new ObjectField() { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue as T);
		});
		element.style.width = Node1U - 8f;
		return element;
	}

	public static ObjectField ObjectField<T>(
		string label, T value, Action<T> onValueChanged) where T : Object {
		var element = new ObjectField(label) { value = value };
		element.RegisterValueChangedCallback(callback => {
			onValueChanged(callback.newValue as T);
		});
		element.style.width = Node2U - 8f;
		element.labelElement.style.minWidth = Node2ULabel;
		element.labelElement.style.maxWidth = Node2ULabel;
		return element;
	}



	// Mathematics Field Methods

	public static VisualElement Toggle2(
		bool2 value, Action<bool2> onValueChanged) {
		var element = new VisualElement();
		element.style.flexDirection = FlexDirection.Row;
		var child0 = new Label("X");
		child0.style.marginTop = 1f;
		child0.style.marginBottom = 1f;
		child0.style.marginLeft = 3f;
		child0.style.marginRight = 0f;
		child0.style.width = 12f;
		element.Add(child0);
		var child1 = Toggle(value.x, valueX => {
			value.x = valueX;
			onValueChanged(value);
		});
		element.Add(child1);
		var child2 = new Label("Y");
		child2.style.marginTop = 1f;
		child2.style.marginBottom = 1f;
		child2.style.marginLeft = 3f;
		child2.style.marginRight = 0f;
		child2.style.width = 12f;
		element.Add(child2);
		var child3 = Toggle(value.y, valueY => {
			value.y = valueY;
			onValueChanged(value);
		});
		element.Add(child3);
		return element;
	}

	public static VisualElement Toggle3(
		bool3 value, Action<bool3> onValueChanged) {
		var element = new VisualElement();
		element.style.flexDirection = FlexDirection.Row;
		var child0 = new Label("X");
		child0.style.marginTop = 1f;
		child0.style.marginBottom = 1f;
		child0.style.marginLeft = 3f;
		child0.style.marginRight = 0f;
		child0.style.width = 12f;
		element.Add(child0);
		var child1 = Toggle(value.x, valueX => {
			value.x = valueX;
			onValueChanged(value);
		});
		element.Add(child1);
		var child2 = new Label("Y");
		child2.style.marginTop = 1f;
		child2.style.marginBottom = 1f;
		child2.style.marginLeft = 3f;
		child2.style.marginRight = 0f;
		child2.style.width = 12f;
		element.Add(child2);
		var child3 = Toggle(value.y, valueY => {
			value.y = valueY;
			onValueChanged(value);
		});
		element.Add(child3);
		var child4 = new Label("Z");
		child4.style.marginTop = 1f;
		child4.style.marginBottom = 1f;
		child4.style.marginLeft = 3f;
		child4.style.marginRight = 0f;
		child4.style.width = 12f;
		element.Add(child4);
		var child5 = Toggle(value.z, valueZ => {
			value.z = valueZ;
			onValueChanged(value);
		});
		element.Add(child5);
		return element;
	}
}
#endif