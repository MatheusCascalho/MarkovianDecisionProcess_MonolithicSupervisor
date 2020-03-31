using UltraDES;

namespace MultiAgentMarkovMonolithic
{
	public class Conveyor
	{
		public DeterministicFiniteAutomaton Automato { get; set; }

		public Conveyor(State estadoLigado, State estadoDesligado, Event eventoLigar, 
			Event eventoDesligar, string nameConveyor)
		{
			this.Automato = new DeterministicFiniteAutomaton(
				new[]
				{
					new Transition(origin: estadoDesligado, trigger: eventoLigar, destination: estadoLigado),
					new Transition(origin: estadoLigado, trigger: eventoDesligar, destination: estadoDesligado)
				}, initial: estadoDesligado, name: nameConveyor);
		}
		
	}

}
