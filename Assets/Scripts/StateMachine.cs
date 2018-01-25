using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{

    public State state { get; private set; }
	enum ActiveAction { none, enter, update, exit }
	ActiveAction activeAction;

    public virtual void Update()
    {
		activeAction = ActiveAction.update;
        State newState = state.updateAction();
		activeAction = ActiveAction.none;
		if (newState != null)
		{
			TransitionToState(newState);
		}
    }

    public bool TransitionToState(State newState)
    {
        if (newState == null)
        {
            Debug.LogError("New state is null! AHHHHH");
            return false;
        }
		if (activeAction != ActiveAction.none)
		{
			Debug.LogError("Attempt to transition state during action '" + activeAction.ToString() + "' in state '" + state.name + "' .");
			return false;
		}
		State oldState = state;
        if (state != null)
        {

            if (state.priority > newState.priority)
            {
                return false;//overruled
            }
            if (state == newState)
            {
                return false;
            }

			//print("changing state from " + state.name + " to " + s.name);
			activeAction = ActiveAction.exit;
            state.exitAction(newState);
            state = newState;
			activeAction = ActiveAction.enter;
            state.enterAction(oldState);
			activeAction = ActiveAction.none;
            return true;
        }
        else
        {
            state = newState;
			activeAction = ActiveAction.enter;
			state.enterAction(oldState);
			activeAction = ActiveAction.none;
			return true;
        }

    }

}
