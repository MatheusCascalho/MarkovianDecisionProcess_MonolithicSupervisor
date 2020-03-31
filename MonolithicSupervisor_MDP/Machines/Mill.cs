using UltraDES;

public class Mill
{
	public DeterministicFiniteAutomaton Automato { get; set; }

	public Mill(State estadoLigado, State estadoDesligado, Event eventoLigar,Event eventoDesligar, string nameMill)
	{
		this.Automato = new DeterministicFiniteAutomaton(
			new[]
			{
					new Transition(origin: estadoDesligado, trigger: eventoLigar, destination: estadoLigado),
					new Transition(origin: estadoLigado, trigger: eventoDesligar, destination: estadoDesligado)
			}, initial: estadoDesligado, name: nameMill);
	}
}
