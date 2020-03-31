using UltraDES;

public class Lathe
{	public DeterministicFiniteAutomaton Automato { get; set; }

	public Lathe(State torneamentoCilindrico, State torneamentoConico, State estadoDesligado, 
		Event tornearCilindro, Event tornearConico, Event desligarConico, Event desligarCilindrico, string nameLathe)
	{
		this.Automato = new DeterministicFiniteAutomaton(
			new[]
			{
					new Transition(origin: estadoDesligado, trigger: tornearCilindro, destination: torneamentoCilindrico),
					new Transition(origin: torneamentoCilindrico, trigger: desligarCilindrico, destination: estadoDesligado),
					new Transition(origin: estadoDesligado, trigger: tornearConico, destination: torneamentoConico),
					new Transition(origin: torneamentoConico, trigger: desligarConico, destination: estadoDesligado)
			}, initial: estadoDesligado, name: nameLathe);
	}
}
