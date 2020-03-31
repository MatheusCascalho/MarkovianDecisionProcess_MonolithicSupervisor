using UltraDES;

namespace MultiAgentMarkovMonolithic
{

	public class RawAndTwoForms
	{
		public DeterministicFiniteAutomaton Automato { get; set; }

		///<summary>
		/// Buffer de 4 estados que permite a comunicação em duas vias entre duas máquinas
		/// Uma maquina é dedicadam, e recebe uma materia prima crua e processa de duas formas diferentes, entregando
		/// o resultado ao buffer.
		///</summary>
		public RawAndTwoForms(State empty, State fullRaw, State fullProcessed1, State fullProcessed2, 
			Event waitRaw, Event waitDeliver1, Event waitDeliver2, Event receiveRaw,
			Event deliverToForm1, Event deliverToForm2, Event receiveProcessed1, Event receiveProcessed2,
			Event deliverProcessed1, Event deliverProcessed2, string nameBuffer)
		{
			this.Automato = new DeterministicFiniteAutomaton(
				new[] { 
					new Transition(origin: empty, trigger: waitRaw, destination: empty),
					new Transition(origin: empty, trigger: waitDeliver1, destination: empty),
					new Transition(origin: empty, trigger: waitDeliver2, destination: empty),
					new Transition(origin: empty, trigger: receiveRaw, destination: fullRaw),
					
					// Produto A
					new Transition(origin: fullRaw, trigger: deliverToForm1, destination: fullRaw),
					new Transition(origin: fullRaw, trigger: receiveProcessed1, destination: fullProcessed1),
					new Transition(origin: fullProcessed1, trigger: deliverProcessed1, destination: empty),

					// Produto B
					new Transition(origin: fullRaw, trigger: deliverToForm2, destination: fullRaw),
					new Transition(origin: fullRaw, trigger: receiveProcessed2, destination: fullProcessed2),
					new Transition(origin: fullProcessed2, trigger: deliverProcessed2, destination: empty)

				}, initial: empty, name: nameBuffer);
		}
	}

}
