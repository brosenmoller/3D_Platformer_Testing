// Based on : https://medium.com/dotcrossdot/hierarchical-finite-state-machine-c9e3f4ce0d9e

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine<T> {

    private StateMachine<T> currentSubState;
    private StateMachine<T> defaultSubState;
    private StateMachine<T> parent;
    protected T owner;
        
    private readonly Dictionary<Type, StateMachine<T>> subStates = new();
    private readonly Dictionary<int, StateMachine<T>> transitions = new();

    public void EnterStateMachine(T owner) {
        this.owner = owner;
        OnEnter();
        if (currentSubState == null && defaultSubState != null) {
            currentSubState = defaultSubState;
        }
        currentSubState?.EnterStateMachine(owner);
    }

    public void UpdateStateMachine() {
        OnUpdate();
        currentSubState?.UpdateStateMachine();
    }

    public void ExitStateMachine() {
        currentSubState?.ExitStateMachine();
        OnExit();
    }

    protected virtual void OnEnter() { }
    
    protected virtual void OnUpdate() { }
        
    protected virtual void OnExit() { }

    public void LoadSubState(StateMachine<T> subState) {
        if (subStates.Count == 0) {
            defaultSubState = subState;
        }

        subState.parent = this;
        try {
            subStates.Add(subState.GetType(), subState);
        }
        catch (ArgumentException) {
            Debug.LogError($"State {GetType()} already contains a substate of type {subState.GetType()}");
        }
            
    }
        
    public void AddTransition(StateMachine<T> from, StateMachine<T> to, int trigger) {
        if (!subStates.TryGetValue(from.GetType(), out _)) {
            Debug.LogError($"State {GetType()} does not have a substate of type {from.GetType()} to transition from.");
        }
            
        if (!subStates.TryGetValue(to.GetType(), out _)) {
            Debug.LogError($"State {GetType()} does not have a substate of type {to.GetType()} to transition from.");
        }
            
        try {
            from.transitions.Add(trigger, to);
        }
        catch (ArgumentException) {
            Debug.LogError($"State {from} already has a transition defined for trigger {trigger}");
        }
    }

    public void SendTrigger(int trigger) {
        StateMachine<T> root = this;
        while (root?.parent != null) {
            root = root.parent;
        }

        while (root != null) {
            if (root.transitions.TryGetValue(trigger, out StateMachine<T> toState)) {
                root.parent?.ChangeSubState(toState);
                return;
            }

            root = root.currentSubState;
        }
            
        Debug.LogError($"Trigger {trigger} was not consumed by any transition!");
    }
        
    private void ChangeSubState(StateMachine<T> state) {
        currentSubState?.ExitStateMachine();
        var newState = subStates[state.GetType()];
        currentSubState = newState;
        newState.EnterStateMachine(owner);
    }
}
