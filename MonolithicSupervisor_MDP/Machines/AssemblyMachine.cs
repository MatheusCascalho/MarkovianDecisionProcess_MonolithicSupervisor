using System;
using System.Collections.Generic;
using UltraDES;

namespace MultiAgentMarkovMonolithic
{
	public class AssemblyMachine
	{
		public DeterministicFiniteAutomaton Automato { get; set; }

		public AssemblyMachine(State estadoDesligado, State holdBase, State assemblyConical, State assemblyCylindrical,
			Event takeBase, Event takeCylinder, Event takeCone, Event deliverCylindrical, Event deliverConical, string nameAssembler)
		{
			this.Automato = new DeterministicFiniteAutomaton(
				new[]
				{
					new Transition(origin: estadoDesligado, trigger: takeBase, destination: holdBase),
					// Produto Cilindrico
					new Transition(origin: holdBase, trigger: takeCylinder, destination: assemblyCylindrical),
					new Transition(origin: assemblyCylindrical, trigger: deliverCylindrical, destination: estadoDesligado),
					// Produto Conico
					new Transition(origin: holdBase, trigger: takeCone, destination: assemblyConical),
					new Transition(origin: assemblyConical, trigger: deliverConical, destination: estadoDesligado)
				}, initial: estadoDesligado, name: nameAssembler);
		}
	}

}
