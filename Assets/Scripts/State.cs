using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void StateTransitionAction(State state);	// gets passed in the state it came from or is going to depending on if enter or exit transition, respecitvely
public delegate State StateUpdateAction();

public interface IStateable{
    bool TransitionState(State s);
}

public class State 
{
    public string name;
    public int priority;
    public StateTransitionAction enterAction;
    public StateUpdateAction updateAction;
    public StateTransitionAction exitAction;

    public bool grounded = true;


    public State(string nam, StateTransitionAction ent, StateUpdateAction upd, StateTransitionAction ext)
    {
        name = nam;
        enterAction = ent;
        updateAction = upd;
        exitAction = ext;
        priority = 0;
    }

    public State(string nam, int pri, StateTransitionAction ent, StateUpdateAction upd, StateTransitionAction ext)
    {
        name = nam;
        enterAction = ent;
        updateAction = upd;
        exitAction = ext;
        priority = pri;
    }

    public static bool operator ==(State obj1, String str)
    {
        if (object.ReferenceEquals(obj1, null))
        {
            return object.ReferenceEquals(str, null);
        }
        return (obj1.name == str);
    }

    public static bool operator !=(State obj1, String str)
    {
        if (object.ReferenceEquals(obj1, null))
        {
            return !object.ReferenceEquals(str, null);
        }
        return (obj1.name != str);
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        return name == ((State)obj).name;
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
}
