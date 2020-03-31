using System;
using UltraDES;

public class PaintingMachine
{
	public DeterministicFiniteAutomaton Automato { get; set; }

	public PaintingMachine(State estadoLigado, State estadoDesligado, Event eventoLigar, Event eventoDesligar, string namePaintingMachine)
	{
		this.Automato = new DeterministicFiniteAutomaton(
			new[]
			{
					new Transition(origin: estadoDesligado, trigger: eventoLigar, destination: estadoLigado),
					new Transition(origin: estadoLigado, trigger: eventoDesligar, destination: estadoDesligado)
			}, initial: estadoDesligado, name: namePaintingMachine);
	}
}
