using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
#nullable disable
    private static EventManager _instance;
#nullable enable
    public static EventManager Instance => _instance ??= new EventManager();

    // Event declarations
    public event Action<Player>? OnGameOver;
    public event Action<Utility, NPC>? OnUtilityComplete;
    public event Action<Utility, NPC>? OnUtilityFailure;
    public event Action<Player, int>? OnScoreChanged;


    // Triggers
    public void TriggerGameOver(Player winningPlayer) => EnqueueEvent(OnGameOver, winningPlayer);
    public void TriggerScoreChanged(Player player, int score) => EnqueueEvent(OnScoreChanged, player, score);
    public void TriggerUtilityComplete(Utility u, NPC npc) => EnqueueEvent(OnUtilityComplete, u, npc);
    public void TriggerUtilityFailure(Utility u, NPC npc) => EnqueueEvent(OnUtilityFailure, u, npc);
   


    /*
    QUEUEING SYSTEM!
    DO NOT TOUCH
    Used to make sure we can handle event triggers that trigger other events
    */
    private readonly Queue<QueuedEvent> _eventQueue = new();
    private bool _isProcessingEvents;

    private abstract class QueuedEvent
    {
        public abstract void Execute();
    }

    private class QueuedEvent<T> : QueuedEvent
    {
        private readonly Action<T> _handler;
        private readonly T _parameter;

        public QueuedEvent(Action<T> handler, T parameter)
        {
            _handler = handler;
            _parameter = parameter;
        }

        public override void Execute() => _handler?.Invoke(_parameter);
    }

    private class QueuedEvent<T1, T2> : QueuedEvent
    {
        private readonly Action<T1, T2> _handler;
        private readonly T1 _parameter1;
        private readonly T2 _parameter2;

        public QueuedEvent(Action<T1, T2> handler, T1 parameter1, T2 parameter2)
        {
            _handler = handler;
            _parameter1 = parameter1;
            _parameter2 = parameter2;
        }

        public override void Execute() => _handler?.Invoke(_parameter1, _parameter2);
    }

    private class QueuedEventNoParams : QueuedEvent
    {
        private readonly Action _handler;

        public QueuedEventNoParams(Action handler)
        {
            _handler = handler;
        }

        public override void Execute() => _handler?.Invoke();
    }

    private void EnqueueEvent(Action? handler)
    {
        if (handler != null)
        {
            _eventQueue.Enqueue(new QueuedEventNoParams(handler));
            ProcessEventQueue();
        }
    }

    private void EnqueueEvent<T>(Action<T>? handler, T parameter)
    {
        if (handler != null)
        {
            _eventQueue.Enqueue(new QueuedEvent<T>(handler, parameter));
            ProcessEventQueue();
        }
    }

    private void EnqueueEvent<T1, T2>(Action<T1, T2>? handler, T1 parameter1, T2 parameter2)
    {
        if (handler != null)
        {
            _eventQueue.Enqueue(new QueuedEvent<T1, T2>(handler, parameter1, parameter2));
            ProcessEventQueue();
        }
    }

    private void EnqueueEvent<T1, T2, T3>(Action<T1, T2, T3>? handler, T1 parameter1, T2 parameter2, T3 parameter3)
    {
        if (handler != null)
        {
            _eventQueue.Enqueue(new QueuedEvent<T1, T2, T3>(handler, parameter1, parameter2, parameter3));
            ProcessEventQueue();
        }
    }

    private void ProcessEventQueue()
    {
        if (_isProcessingEvents) return;
        _isProcessingEvents = true;

        try
        {
            while (_eventQueue.Count > 0)
            {
                var queuedEvent = _eventQueue.Dequeue();
                queuedEvent.Execute();
            }
        }
        finally
        {
            _isProcessingEvents = false;
        }
    }

    public static void ResetInstance()
    {
        Debug.Log("Resetting event manager");
        if (_instance != null)
        {
            _instance._eventQueue.Clear();
            _instance._isProcessingEvents = false;
        }
        _instance = new EventManager();
    }

    private class QueuedEvent<T1, T2, T3> : QueuedEvent
    {
        private readonly Action<T1, T2, T3> _handler;
        private readonly T1 _parameter1;
        private readonly T2 _parameter2;
        private readonly T3 _parameter3;

        public QueuedEvent(Action<T1, T2, T3> handler, T1 parameter1, T2 parameter2, T3 parameter3)
        {
            _handler = handler;
            _parameter1 = parameter1;
            _parameter2 = parameter2;
            _parameter3 = parameter3;
        }

        public override void Execute() => _handler?.Invoke(_parameter1, _parameter2, _parameter3);
    }
}
