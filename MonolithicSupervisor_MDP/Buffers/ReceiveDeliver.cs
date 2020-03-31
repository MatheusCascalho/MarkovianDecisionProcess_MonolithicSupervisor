using UltraDES;

namespace MultiAgentMarkovMonolithic
{
	public class ReceiveDeliver
	{
		public DeterministicFiniteAutomaton Automato { get; set; }

		///<summary>
		///Buffer de 2 estados que so pode ser ocupado por um objeto por vez
		///</summary> 
		public ReceiveDeliver(State empty, State full, Event wait, Event receive, Event deliver, string bufferName)
		{
			this.Automato = new DeterministicFiniteAutomaton(
				new[] {
					new Transition(origin: empty, trigger: wait, destination: empty),
					new Transition(origin: empty, trigger: receive, destination: full),
					new Transition(origin: full, trigger: deliver, destination: empty)
				}, initial: empty, name: bufferName);
		}
	}

}
