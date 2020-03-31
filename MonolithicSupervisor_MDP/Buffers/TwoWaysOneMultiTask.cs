using UltraDES;

public class TwoWaysOneMultiTask
{
	public DeterministicFiniteAutomaton Automato { get; set; }

	///<summary>
	///Buffer de 3 estados que permite a comunicação entre 2 maquinas. 
	/// A maquina 1 pode enviar um produto à maquina 2 e vice versa. 
	/// Uma das maquinas é dedicada (só se comunica com esse buffer), enquanto a outra se comunica com outros 2 buffers, 
	/// sendo portanto necessario informar o evento de espera de cada tarefa realizada entre os outros buffers.
	///</summary>
	public TwoWaysOneMultiTask(State empty, State fullDedicated, State fullMultiTask,
		Event waitTask1, Event waitTask2, Event receiveMultiTask, Event deliverDedicated,
		Event receiveDedicated, Event deliverMultiTask, string nameBuffer)
	{
		this.Automato = new DeterministicFiniteAutomaton(
			new[] {
				new Transition(origin: empty, trigger: waitTask1, destination: empty),
				new Transition(origin: empty, trigger: waitTask2, destination: empty),
				new Transition(origin: empty, trigger: receiveMultiTask, destination: fullMultiTask),
				new Transition(origin: fullMultiTask, trigger: deliverDedicated, destination: fullMultiTask),
				new Transition(origin: fullMultiTask, trigger: receiveDedicated, destination: fullDedicated),
				new Transition(origin: fullDedicated, trigger: deliverMultiTask, destination: empty)
			}, initial: empty, name: nameBuffer);
	}
}
