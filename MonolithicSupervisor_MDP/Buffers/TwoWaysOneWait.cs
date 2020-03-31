using UltraDES;

namespace MultiAgentMarkovMonolithic
{
	public class TwoWaysOneWait
	{
		public DeterministicFiniteAutomaton Automato { get; set; }

		/// <summary>
		/// Buffer de 3 estados que permite a comunicação de 2 vias entre 2 maquinas sendo uma delas dedicada
		/// (comunica apenas com o buffer) e outra multi tarefa		/// 
		/// </summary>
		/// <param name="empty"></param>
		/// <param name="fullDedicated"></param>
		/// <param name="fullMultiTask"></param>
		/// <param name="waitTask"></param>
		/// <param name="receiveMultiTask"></param>
		/// <param name="deliverDedicated"></param>
		/// <param name="receiveDedicated"></param>
		/// <param name="deliverMultiTask"></param>
		/// <param name="nameBuffer"></param>
		public TwoWaysOneWait(State empty, State fullDedicated, State fullMultiTask,
		Event waitTask, Event receiveMultiTask, Event deliverDedicated,
		Event receiveDedicated, Event deliverMultiTask, string nameBuffer)
		{
			this.Automato = new DeterministicFiniteAutomaton(
			new[] {
				new Transition(origin: empty, trigger: waitTask, destination: empty),
				new Transition(origin: empty, trigger: receiveMultiTask, destination: fullMultiTask),
				new Transition(origin: fullMultiTask, trigger: deliverDedicated, destination: fullMultiTask),
				new Transition(origin: fullMultiTask, trigger: receiveDedicated, destination: fullDedicated),
				new Transition(origin: fullDedicated, trigger: deliverMultiTask, destination: empty)
			}, initial: empty, name: nameBuffer);
		}
	}

}
