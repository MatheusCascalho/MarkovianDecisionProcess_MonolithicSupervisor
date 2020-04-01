using System;
using UltraDES;
using System.Linq;

namespace MultiAgentMarkovMonolithic
{
    [Serializable]
    class ExpandedState : State
    {
        public uint Tasks { get; private set; }

        public ExpandedState(string alias, uint tasks, Marking marking = Marking.Unmarked)
            : base(alias, marking)
        {
            Tasks = tasks;
        }

        public override AbstractState ToMarked
        {
            get
            {
                return IsMarked ? this : new ExpandedState(Alias, Tasks, Marking.Marked);
            }
        }

        public override AbstractState ToUnmarked
        {
            get
            {
                return !IsMarked ? this : new ExpandedState(Alias, Tasks, Marking.Unmarked);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            // If parameter cannot be cast to Point return false.
            State p = obj as State;
            if (p is null) return false;

            // Return true if the fields match:
            return Alias == p.Alias && Marking == p.Marking;
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        public override string ToString()
        {
            return Alias;
        }

    }



    public static class Extensions
    {
        /// <summary>
        /// Retruns the number of active tasks of a state of the automaton
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static uint ActiveTasks(this AbstractState state)
        {
            if (state is AbstractCompoundState)
                return (uint)(state as AbstractCompoundState).S.OfType<ExpandedState>().Sum(s => s.Tasks);
            if (state is ExpandedState)
                return (state as ExpandedState).Tasks;
            return 0;
        }

    }

}