// Based on : https://medium.com/dotcrossdot/hierarchical-finite-state-machine-c9e3f4ce0d9e

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransitionStateMachine<T>
{
    private TransitionStateMachine<T> currentSubState;
    private TransitionStateMachine<T> defaultSubState;
    private TransitionStateMachine<T> parent;
    protected T owner;

    private readonly Dictionary<Type, TransitionStateMachine<T>> subStates = new();
    private readonly List<Transition<T>> transitions = new();

    public void EnterStateMachine(T owner)
    {
        this.owner = owner;
        OnEnter();
        if (currentSubState == null && defaultSubState != null)
        {
            currentSubState = defaultSubState;
        }
        currentSubState?.EnterStateMachine(owner);
    }

    public void UpdateStateMachine()
    {
        foreach (Transition<T> transition in transitions)
        {
            if (transition.Evalutate())
            {
                ChangeState(transition.toState);
                return;
            }
        }

        OnUpdate();
        currentSubState?.UpdateStateMachine();
    }

    public void ExitStateMachine()
    {
        currentSubState?.ExitStateMachine();
        OnExit();
    }

    protected virtual void OnEnter() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnExit() { }

    public void LoadSubState(TransitionStateMachine<T> subState)
    {
        if (subStates.Count == 0)
        {
            defaultSubState = subState;
        }

        subState.parent = this;
        try
        {
            subStates.Add(subState.GetType(), subState);
        }
        catch (ArgumentException)
        {
            Debug.LogError($"State {GetType()} already contains a substate of type {subState.GetType()}");
        }

    }

    public void AddTransition(Transition<T> transition)
    {


        if (!subStates.TryGetValue(transition.fromState.GetType(), out _))
        {
            Debug.LogError($"State {GetType()} does not have a substate of type {transition.fromState.GetType()} to transition from.");
        }

        if (!subStates.TryGetValue(transition.toState.GetType(), out _))
        {
            Debug.LogError($"State {GetType()} does not have a substate of type {transition.toState.GetType()} to transition from.");
        }

        transition.fromState.transitions.Add(transition);
    }

    private void ChangeState(TransitionStateMachine<T> state)
    {
        currentSubState?.ExitStateMachine();
        TransitionStateMachine<T> newState = subStates[state.GetType()];
        currentSubState = newState;
        newState.EnterStateMachine(owner);
    }
}
