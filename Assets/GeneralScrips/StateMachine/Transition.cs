using System;

public class Transition<T>
{
    public TransitionStateMachine<T> fromState;
    public TransitionStateMachine<T> toState;
    public Func<bool> condition;

    public Transition(TransitionStateMachine<T> fromState, TransitionStateMachine<T> toState, Func<bool> condition)
    {
        this.fromState = fromState;
        this.toState = toState;
        this.condition = condition;
    }

    public bool Evalutate()
    {
        return condition();
    }
}