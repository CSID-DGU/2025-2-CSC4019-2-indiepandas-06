using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using static ElementEditorExtensions;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Dialogue
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Dialogue")]
public class DialogueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
		public class DialogueEventNode : EventNodeBase {
			DialogueEvent I => target as DialogueEvent;

			public override void ConstructData() {
				var root = new VisualElement();
				mainContainer.Add(root);
				for (int i = 0; i < I.texts.Count; i++) {
					var index = i;

					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					root.Add(element);

					var name = new TextField() { value = I.names[index] };
					name.style.minWidth = name.style.maxWidth = 180f;
					name.textEdition.placeholder = "Name";
					name.RegisterValueChangedCallback(evt => I.names[index] = evt.newValue);
					element.Add(name);

					var removeButton = new Button(() => {
						mainContainer.Remove(root);
						I.names.RemoveAt(index);
						I.texts.RemoveAt(index);
						ConstructData();
					}) { text = "-" };
					removeButton.style.width = 18f;
					element.Add(removeButton);

					var text = new TextField() { value = I.texts[index], multiline = true };
					text.style.minWidth = text.style.maxWidth = 204f;
					text.style.whiteSpace = WhiteSpace.Normal;
					text.textEdition.placeholder = "Text";
					var field = text.Q<VisualElement>(className: "unity-text-field__input");
					if (field != null) field.style.minHeight = 46f;
					text.RegisterValueChangedCallback(evt => I.texts[index] = evt.newValue);
					root.Add(text);
				}
				var addButton = new Button(() => {
					mainContainer.Remove(root);
					I.names.Add("");
					I.texts.Add("");
					ConstructData();
				}) { text = "Add Element" };
				root.Add(addButton);
			}
		}
	#endif



	// Fields

	public List<string> names = new() { "", };
	public List<string> texts = new() { "", };
	bool end;
	bool held;

	static readonly Type[] chainTypes = new[] {
        typeof(DialogueEvent),
        typeof(BeginChoicesEvent),
        typeof(DialogueInputEvent),
        typeof(AddChoiceEvent),
        typeof(ShowChoicesEvent),
    };



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is DialogueEvent dialogueEvent) {
			names.CopyFrom(dialogueEvent.names);
			texts.CopyFrom(dialogueEvent.texts);
		}
	}

	public override void Start() {
		end = false;

        // 다음이 대화/선택지 계열이면 홀드
        held = IsNextDialogueChain();
        if (held) {
			UIManager.HoldDialogue();
		}

		
		for (int i = 0; i < texts.Count; i++) {
			Action onEnd = (i == texts.Count - 1) ? () => end = true : null;
			UIManager.EnqueueDialogue(names[i], texts[i], onEnd);
		}
	}

	public override bool Update() {
		if (!end) { return false; }

		// 내가 잡은 홀드는 다음 프레임에 풀기 (다음 노드 Start가 먼저 실행되도록)
		if (held) {
            held = false;
            UIManager.ReleaseDialogueDeferred();
        }
        return true;
		//return end;
	}

	public bool IsNextDialogueChain() {
        foreach (var n in Nexts) if (n.oPortType == PortType.Default) {
            if (n.eventBase == null) { continue; }
            var t = n.eventBase.GetType();
            if (chainTypes.Any(ct => ct.IsAssignableFrom(t))) { return true; }
        }
        return false;
    }
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Branch
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Branch")]
public class BranchEvent : EventBase {

	// Node

	#if UNITY_EDITOR
		public class BranchEventNode : EventNodeBase {
			BranchEvent I => target as BranchEvent;

			public override void ConstructData() {
				var root = new VisualElement();
				mainContainer.Add(root);
				for (int i = 0; i < I.texts.Count; i++) {
					var index = i;

					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					root.Add(element);

					var text = new TextField() { value = I.texts[index] };
					text.style.minWidth = text.style.maxWidth = 180f;
					text.textEdition.placeholder = "Text";
					text.RegisterValueChangedCallback(evt => {
						I.texts[index] = evt.newValue;
						(outputContainer[index] as Port).portName = evt.newValue;
					});
					element.Add(text);

					var removeButton = new Button(() => {
						mainContainer.Remove(root);
						I.texts.RemoveAt(index);
						ConstructData();
						outputContainer.RemoveAt(index);
					}) { text = "-" };
					removeButton.style.width = 18f;
					element.Add(removeButton);
				}
				var addButton = new Button(() => {
					mainContainer.Remove(root);
					I.texts.Add("");
					ConstructData();
					var port = CreatePort(Direction.Output);
					port.portName = I.texts[^1];
					outputContainer.Add(port);
				}) { text = "Add Element" };
				root.Add(addButton);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input);
				for (int i = 0; i < I.texts.Count; i++) {
					var port = CreatePort(Direction.Output);
					port.style.maxWidth = 154f;
					port.portName = I.texts[i];
				}
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	public List<string> texts = new() { "", "", };



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is BranchEvent branchEvent) {
			texts.Clear();
			texts.AddRange(branchEvent.texts);
		}
	}

	public override void GetNexts(List<EventBase> list) {
		// Get Index from UI Manager, User Selection
		int index = 0;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Dialogue Input
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Dialogue Input")]
public class DialogueInputEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class DialogueInputEventNode : EventNodeBase {
		DialogueInputEvent I => target as DialogueInputEvent;

		public DialogueInputEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.MultimodalData);
		}
	}
	#endif



	// Fields

	MultimodalData data;
	bool end;



	// Methods

	public override void Start() {
		end = false;
		UIManager.BeginDialogueInput(data => {
			this.data = data;
			end = true;
		});
	}

	public override bool Update() {
		return end;
	}

	protected override void GetMultimodalData(List<MultimodalData> list) {
		list.Add(data);
	}
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 선택지 진행을 위한 static 클래스
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
static class ChoiceState {
    public static int SelectedIndex { get; private set; } = -1;
    public static int RegisteredCount { get; private set; } = 0;

    // 어느 AddChoice가 선택됐는지 저장
    public static AddChoiceEvent SelectedAddNode { get; private set; } = null;

    public static void Reset() {
        SelectedIndex = -1;
        RegisteredCount = 0;
        SelectedAddNode = null;
    }

    public static int RegisterOption() => RegisteredCount++;

    public static void Select(int idx, AddChoiceEvent node) {
        SelectedIndex = idx;
        SelectedAddNode = node;
    }

	public static void SelectFromCode(int idx) {
        SelectedIndex = idx;
        SelectedAddNode = null;
    }

    public static bool HasSelection => SelectedIndex >= 0; // SelectedAddNode != null
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Begin Choices
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Begin Choices")]
public sealed class BeginChoicesEvent : EventBase {
	// Editor
	#if UNITY_EDITOR
    public sealed class BeginChoicesEventNode : EventNodeBase {
        public override void ConstructPort() {
            CreatePort(Direction.Input);
            CreatePort(Direction.Output).portName = "Add Choices";
        }
    }
	#endif

	// Methods

    public override void Start() {
		// 이전의 선택 상황 초기화
        ChoiceState.Reset();
        UIManager.BeginChoices();
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Add Choice
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Add Choice")]
public sealed class AddChoiceEvent : EventBase {
	// Editor
	#if UNITY_EDITOR
    public sealed class AddChoiceEventNode : EventNodeBase {
        AddChoiceEvent I => target as AddChoiceEvent;

        public AddChoiceEventNode() : base() {
            mainContainer.style.width = Node2U;
        }

        public override void ConstructData() {
            var l = TextField(I.ChoiceLabel, v => I.ChoiceLabel = v);
            l.textEdition.placeholder = "Choice Label";
			mainContainer.Add(l);
        }

        public override void ConstructPort() {
            CreatePort(Direction.Input);
            CreatePort(Direction.Output).portName = "Flow";
            CreatePort(Direction.Output).portName = "If Chosen Then";
            RefreshExpandedState();
            RefreshPorts();
        }
    }
	#endif

    // Fields
    [SerializeField] public string m_ChoiceLabel = "";

    // Properties
    public string ChoiceLabel {
        get => m_ChoiceLabel;
        set => m_ChoiceLabel = value ?? "";
    }

    // Methods
    public override void CopyFrom(EventBase other) {
        base.CopyFrom(other);
        if (other is AddChoiceEvent e) {
            ChoiceLabel = e.ChoiceLabel;
        }
    }

    public override void Start() {
        // 등록 순서가 선택 인덱스
        int idx = ChoiceState.RegisterOption();
        string label = ChoiceLabel ?? string.Empty;

        // onChosen: 이 노드가 선택되었다고 기록
        UIManager.AddChoice(label, () => {
            ChoiceState.Select(idx, this);
        });
    }

    // AddChoice 노드는 등록만 하고 즉시 다음으로 넘어가야 하므로 즉시 완료
    public override bool Update() => true;

    // 평소엔 Flow(0번 포트)로 이어진다.
    public override void GetNexts(List<EventBase> list) {
        foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
            if (next.oPort == 0) list.Add(next.eventBase); // Flow
        }
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Show Choices
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Show Choices")]
public sealed class ShowChoicesEvent : EventBase {
	// Editor
	#if UNITY_EDITOR
    public sealed class ShowChoicesEventNode : EventNodeBase {
        public ShowChoicesEventNode() : base() { mainContainer.style.width = Node1U; }
        public override void ConstructPort() {
            CreatePort(Direction.Input);
            // CreatePort(Direction.Output);
            RefreshExpandedState();
            RefreshPorts();
        }
    }
	#endif

    private bool done;

	// Methods
    public override void Start() {
        done = false;
        UIManager.ShowChoices();
    }

    public override bool Update() {
        if (ChoiceState.HasSelection) { done = true; }
        return done;
    }

    // 선택 완료 시 선택된 AddChoice 노드의 1번 포트로 직접 분기
    public override void GetNexts(List<EventBase> list) {
        if (!ChoiceState.HasSelection) { return; }

        var src = ChoiceState.SelectedAddNode;
        if (src == null) { return; }

        // 선택된 AddChoice의 Nexts 중 oPort 1 연결을 찾아 그 대상 이벤트로 분기
        foreach (var cn in src.Nexts) if (cn.oPortType == PortType.Default) {
            if (cn.oPort == 1 && cn.eventBase != null) {
                list.Add(cn.eventBase);
            }
        }
    }
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UIManager | Show Gem Collect Message
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Show Gem Collect Message")]
public sealed class ShowGemCollectMessageEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class ShowGemCollectMessageEventNode : EventNodeBase {
		ShowGemCollectMessageEvent I => target as ShowGemCollectMessageEvent;

		public ShowGemCollectMessageEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var message = TextField(I.Message, value => I.Message = value);
			message.textEdition.placeholder = "Message";
			message.multiline = true;
			mainContainer.Add(message);
		}
	}
	#endif



	// Fields

	[SerializeField] string m_Message = string.Empty;



	// Properties

	public string Message {
		get => m_Message;
		set => m_Message = value;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is ShowGemCollectMessageEvent showGemCollectMessageEvent) {
			Message = showGemCollectMessageEvent.Message;
		}
	}

	public override void End() {
		UIManager.ShowGemCollectMessage(Message);
	}
}
