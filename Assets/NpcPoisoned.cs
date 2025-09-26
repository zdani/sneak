using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/NPCPoisoned")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "NPCPoisoned", message: "[NPC] is poisoned", category: "Events", id: "c3029303e801cf15f93599f78bb4cad5")]
public sealed partial class NpcPoisoned : EventChannel<GameObject> { }

