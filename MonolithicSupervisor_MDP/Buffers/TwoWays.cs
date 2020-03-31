using UltraDES;

namespace MultiAgentMarkovMonolithic
{
	public class TwoWays
	{
		public DeterministicFiniteAutomaton Automato { get; set; }

		/// <summary>
		/// Buffer de 3 estados que permite a comunicação em 2 vias entre duas maquinas
		/// </summary>
		/// <param name="empty"></param>
		/// <param name="fullProcessed"></param>
		/// <param name="fullRaw"></param>
		/// <param name="receiveRaw"></param>
		/// <param name="deliverRaw"></param>
		/// <param name="receiveProcessed"></param>
		/// <param name="deliverProcessed"></param>
		/// <param name="nameBuffer"></param>
		public TwoWays(State empty, State fullProcessed, State fullRaw, Event receiveRaw, 
			Event deliverRaw, Event receiveProcessed, Event deliverProcessed, string nameBuffer)
		{
			this.Automato = new DeterministicFiniteAutomaton(
			new[] {
				new Transition(origin: empty, trigger: receiveRaw, destination: fullRaw),
				new Transition(origin: fullRaw, trigger: deliverRaw, destination: empty),
				new Transition(origin: empty, trigger: receiveProcessed, destination: fullProcessed),
				new Transition(origin: fullProcessed, trigger: deliverProcessed, destination: empty)
			}, initial: empty, name: nameBuffer);
		}
	}
}
