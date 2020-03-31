using UltraDES;

public class Robot
{
	public DeterministicFiniteAutomaton Automato { get; set; }

	public Robot(State estadoDesligado, State holdBase, State holdComplement,
		State holdCylinder, State holdCone, State holdMilled,
		Event takeBase, Event deliverBase, Event takeComplement, Event deliverComplement,
		Event takeCylinder, Event deliverCylinder, Event takeCone, Event deliverCone,
		Event takeMilled, Event deliverMilled, string nameRobot)
	{
		this.Automato = new DeterministicFiniteAutomaton(
			new[]
			{		
					// Pass Base
					new Transition(origin: estadoDesligado, trigger: takeBase, destination: holdBase),
					new Transition(origin: holdBase, trigger: deliverBase, destination: estadoDesligado),
					// Pass Complemento
					new Transition(origin: estadoDesligado, trigger: takeComplement, destination: holdComplement),
					new Transition(origin: holdComplement, trigger: deliverComplement, destination: estadoDesligado),
					// Pass Cylinder
					new Transition(origin: estadoDesligado, trigger: takeCylinder, destination: holdCylinder),
					new Transition(origin: holdCylinder, trigger: deliverCylinder, destination: estadoDesligado),
					// Pass Cone
					new Transition(origin: estadoDesligado, trigger: takeCone, destination: holdCone),
					new Transition(origin: holdCone, trigger: deliverCone, destination: estadoDesligado),
					// Pass Milled
					new Transition(origin: estadoDesligado, trigger: takeMilled, destination: holdMilled),
					new Transition(origin: holdMilled, trigger: deliverMilled, destination: estadoDesligado)

			}, initial: estadoDesligado, name: nameRobot);
	}
}
