using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using static ElementEditorExtensions;
#endif

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Quest | If Mission Active
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Quest/If Mission Active")]
public class IfMissionActiveEvent : EventBase {
    // Editor
    #if UNITY_EDITOR
    public class IfMissionActiveEventNode : EventNodeBase {
        IfMissionActiveEvent I => target as IfMissionActiveEvent;

        public IfMissionActiveEventNode() : base() {
            mainContainer.style.width = Node2U;
        }

        public override void ConstructData() {
            var q = TextField(I.QuestId, v => I.QuestId = v);
            q.textEdition.placeholder = "Quest ID";
            var idx = IntField(I.MissionIndex, v => I.MissionIndex = v);
            mainContainer.Add(q);
            mainContainer.Add(idx);
        }

        public override void ConstructPort() {
            CreatePort(Direction.Input);
            CreatePort(Direction.Output).portName = "True";
            CreatePort(Direction.Output).portName = "False";
            RefreshExpandedState();
            RefreshPorts();
        }
    }
    #endif

    // Fields
    [SerializeField] string m_QuestId = "";
    [SerializeField] int m_MissionIndex = 0;

    // Properties
    bool m_Result;

    public string QuestId {
        get => m_QuestId;
        set => m_QuestId = value;
    }
    public int MissionIndex {
        get => m_MissionIndex;
        set => m_MissionIndex = Mathf.Max(0, value);
    }

    // Methods
    public override void CopyFrom(EventBase src) {
        base.CopyFrom(src);
        if (src is IfMissionActiveEvent e) {
            m_QuestId = e.m_QuestId;
            m_MissionIndex = e.m_MissionIndex;
        }
    }
    public override void Start() {
        m_Result = false;
        if (QuestManager.Instance != null) {
            m_Result = QuestManager.Instance.IsMissionActive(QuestId, MissionIndex);
        }
    }

    // 바로 다음 프레임으로 진행
    public override bool Update() => true;

    public override void GetNexts(List<EventBase> list) {
        // 0 = True, 1 = False 포트로 분기
        int index = m_Result ? 0 : 1;
        foreach (var next in Nexts) {
            if (next.oPortType == PortType.Default && next.oPort == index) {
                list.Add(next.eventBase);
            }
        }
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Quest | Raise Dialogue Completed
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Quest/Raise Dialogue Completed")]
public class RaiseDialogueCompletedEvent : EventBase {
    // Editor
    #if UNITY_EDITOR
    public class RaiseDialogueCompletedEventNode : EventNodeBase {
        RaiseDialogueCompletedEvent I => target as RaiseDialogueCompletedEvent;

        public RaiseDialogueCompletedEventNode() : base() {
            mainContainer.style.width = Node1U;
        }

        public override void ConstructData() {
            var tf = TextField(I.DialogueId, v => I.DialogueId = v);
            tf.textEdition.placeholder = "Dialogue ID";
            mainContainer.Add(tf);
        }

        public override void ConstructPort() {
            CreatePort(Direction.Input);
            CreatePort(Direction.Output);
            RefreshExpandedState();
            RefreshPorts();
        }
    }
    #endif

    // Fields
    [SerializeField] string m_DialogueId = "";

    // Properties
    public string DialogueId {
        get => m_DialogueId;
        set => m_DialogueId = value ?? "";
    }

    // Methods
    public override void CopyFrom(EventBase other) {
        base.CopyFrom(other);
        if (other is RaiseDialogueCompletedEvent e) {
            DialogueId = e.DialogueId;
        }
    }

    // 바로 다음 프레임으로 진행
    public override bool Update() => true;

    public override void End() {
        if (!string.IsNullOrEmpty(DialogueId)) {
            GameplayEvents.RaiseDialogueCompleted(DialogueId);
        }
    }
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Quest | Add Item
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Quest/Add Item")]
public class AddItemEvent : EventBase {
	// Editor

	#if UNITY_EDITOR
	public class AddItemEventNode : EventNodeBase {
		AddItemEvent I => target as AddItemEvent;

		public AddItemEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var itemData = ObjectField(I.ItemData, v => I.ItemData = v);
			mainContainer.Add(itemData);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	[SerializeField] ItemData m_ItemData;



	// Properties

	public ItemData ItemData {
		get => m_ItemData;
		set => m_ItemData = value;
	}



	// Methods
	public override void CopyFrom(EventBase other) {
		base.CopyFrom(other);
		if (other is AddItemEvent addItemEvent) {
			ItemData = addItemEvent.ItemData;
		}
	}

	public override void End() {
		var itemBase = ItemDatabase.Instance.CreateItem(ItemData, 1);
		InventoryManager.Instance.AddItem(itemBase);
	}
}
