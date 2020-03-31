using UltraDES;

public class TwoWayConveyor 
{
	public DeterministicFiniteAutomaton Automato { get; set; }

	/// <summary>
	/// Esteira que permite a movimentação em duas direções
	/// </summary>
	/// <param name="movingRight"></param>
	/// <param name="movingLeft"></param>
	/// <param name="off"></param>
	/// <param name="toRight"></param>
	/// <param name="toLeft"></param>
	/// <param name="disable"></param>
	/// <param name="nameLathe"></param>
	public TwoWayConveyor(State movingRight, State movingLeft, State off,
		Event toRight, Event toLeft, Event offRight, Event offLeft, string nameConveyor)
	{
		this.Automato = new DeterministicFiniteAutomaton(
			new[]
			{
					new Transition(origin: off, trigger: toRight, destination: movingRight),
					new Transition(origin: movingRight, trigger: offRight, destination: off),
					new Transition(origin: off, trigger: toLeft, destination: movingLeft),
					new Transition(origin: movingLeft, trigger: offLeft, destination: off)
			}, initial: off, name: nameConveyor);

	}
}
