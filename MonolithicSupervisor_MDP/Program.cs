using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UltraDES;
using Politica = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>;
using Scheduler = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, float>;
using Restriction = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, uint>;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


namespace MultiAgentMarkovMonolithic
{
    class Program
    {
        private static Dictionary<AbstractState, Transition[]> allowedEvents;

        static void Main(string[] args)
        {
            Console.WriteLine("Programa iniciado!!\n");

            var timer = new Stopwatch();
            timer.Start();
            var problema = new FMS();
            var supervisor = problema.Supervisor;
            timer.Stop();

            Console.WriteLine($"Supervisor monolitico criado em: {timer.ElapsedMilliseconds / 1000.0} seg");
            Console.WriteLine($"O supervisor do problema tem {supervisor.States.ToArray().Length} estados");

            // Relacionando cada estado com suas respectivas transições
            allowedEvents = supervisor.Transitions.GroupBy(t => t.Origin).ToDictionary(g => g.Key, g => g.ToArray());
            var pathPolitica = @"C:\Users\Administrador\Documents\Matheus Cascalho\lacsed\MarkovianDecisionProcess_MonolithicSupervisor\MonolithicSupervisor_MDP\politica.bin";

            ////////////////////////// CRIAR POLITICA E SALVAR EM ARQUIVO .BIN
            ////////////////////var timerOptimization = new Stopwatch();
            ////////////////////timerOptimization.Start();
            ////////////////////Politica map = ValueIterationMethod(supervisor.States, supervisor.Events);
            ////////////////////timerOptimization.Stop();
            ////////////////////Console.WriteLine($"\nPolitica de otimizacao encontrada em {timerOptimization.ElapsedMilliseconds / 1000.0} seg");
            ////////////////////// Guardando a politica em um arquivo .bin
            ////////////////////try
            ////////////////////{
            ////////////////////    FileStream fs = new FileStream(pathPolitica, FileMode.Create);
            ////////////////////    BinaryFormatter bf = new BinaryFormatter();
            ////////////////////    bf.Serialize(fs, map);
            ////////////////////    fs.Close();

            ////////////////////}
            ////////////////////catch
            ////////////////////{
            ////////////////////    Console.WriteLine("Nao Foi possível gerar o arquivo BIN com a politica");
            ////////////////////}


            ///LENDO ARQUIVO DA POLITICA
            Politica map = new Politica();
            FileInfo fi = new FileInfo(pathPolitica);
            if (fi.Exists)
            {
                Console.WriteLine("Politica encontrada!");

                FileStream fsNovo = new FileStream(pathPolitica, FileMode.Open);
                BinaryFormatter bfNovo = new BinaryFormatter();
                map = (Politica)bfNovo.Deserialize(fsNovo);
                fsNovo.Close();

                Console.WriteLine("Politica lida!!");

                Console.WriteLine("Resultados das simulações:\n");
                var pathCSV = @"C:\Users\Administrador\Documents\Matheus Cascalho\lacsed\MarkovianDecisionProcess_MonolithicSupervisor\MonolithicSupervisor_MDP\resultado.csv";
                StreamWriter sw = new StreamWriter(pathCSV, true, Encoding.UTF8);

                var pathTXT = @"C:\Users\Administrador\Documents\Matheus Cascalho\lacsed\MarkovianDecisionProcess_MonolithicSupervisor\MonolithicSupervisor_MDP\eventos.txt";
                StreamWriter swTXT = new StreamWriter(pathTXT, true, Encoding.UTF8);


                //  sw = new StreamWriter(pathCSV, Enco)
                var cabecalho = "quantidade de pares de produtos;makespan;paralelismo;";

                for (var j = 0; j < 30; j++) // nome das colunas de execucao
                {
                    cabecalho += $"execucao {j};";
                }

                sw.WriteLine(cabecalho);
                Console.WriteLine(cabecalho);
                List<int> pares = new List<int>() { 1, 5, 10, 15, 50, 100, 500, 750, 1000 };
                // for (var i = 0; i < 1000; i += 20) // variando a quantidade de produtos
                foreach (var i in pares)
                {

                    Console.WriteLine($"{i} pares de produto:\n");

                    var stringCSV = i.ToString() + ";";
                    var stringTXT = "";
                    for (var execucao = 0; execucao < 30; execucao++)
                    {
                        var executionTime = new Stopwatch();
                        executionTime.Start();
                        SimulationResult simulacao = TransicoesProducao(politica: map, qtdProdutos: i, problema: problema);
                        executionTime.Stop();
                        var timeExecution = executionTime.ElapsedMilliseconds / 1000.0;

                        if (execucao == 0)
                        {
                            var makespan = simulacao.makespan;
                            var paralelismo = simulacao.parallelism;
                            stringCSV += makespan.ToString() + ";" + paralelismo.ToString() + ";";
                            foreach(var e in simulacao.eventSequence)
                            {
                                stringTXT += e + ";";
                            }
                            swTXT.WriteLine(stringTXT);
                        }

                        stringCSV += timeExecution.ToString() + ";";
                    }

                    sw.WriteLine(stringCSV);
                    Console.WriteLine(stringCSV);
                }

                sw.Close();
                Console.WriteLine("Simulacao finalizada!!!!!!!!");

            }

            else
            {
                Console.WriteLine("Politica não encontrada :(");
            }
        }

        /// <summary>
        /// Retorna a probabilidade de um evento "e" ocorrer quando se deseja sair de um estado "origem" até um estado "destino"
        /// </summary>
        /// <param name="origem"></param>
        /// <param name="destino"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static double Probabilidade(AbstractState origem, AbstractState destino, AbstractEvent e)
        {
            AbstractState destinoReal = origem; //se nao houver transicao com o evento "e", o destino sera o proprio estado de origem
            bool isControlable = false;
            int qtd_uncontrollables = 0;
            double prob;

            foreach (var t in allowedEvents[origem])
            {
                // Procurando a transição que ocorre com o evento informado
                if (t.Trigger == e)
                {
                    destinoReal = t.Destination;
                    isControlable = t.IsControllableTransition;
                }

                // Contando os eventos não controláveis
                if (!t.IsControllableTransition) qtd_uncontrollables += 1;
            }

            if (isControlable)
            {
                if (destino != destinoReal) prob = 0;
                else prob = 1;
            }
            else
            {
                if (destino != destinoReal) prob = 1 - (1.0 / qtd_uncontrollables);
                else prob = 1.0 / qtd_uncontrollables;
            }

            return prob;
        }

        /// <summary>
        /// método de iteração de valor modificado. Retorna um dicionário que relaciona cada estado com uma lista de melhores eventos
        /// para ocorrer.
        /// </summary>
        /// <param name="stateSet"></param>
        /// <param name="eventSet"></param>
        /// <returns></returns>
        static Politica ValueIterationMethod(IEnumerable<AbstractState> stateSet,
            IEnumerable<AbstractEvent> eventSet)
        {
            // v = dicionário que relaciona cada estado com sua esperança de ocorrencia
            Dictionary<AbstractState, double> v = new Dictionary<AbstractState, double>();
            // mapping = dicionário que relaciona cada estado com uma lista de ações otimas 
            Politica mapping = new Politica();
            // d = constante de desconto
            double d = 0.3;
            // e = Σ_s (v_k[s] - v_{k-1}[s])
            double erro = 1.0;
            double precisao = 1e-20;
            int iteradorVIM = 0;


            // Atribuindo valores arbitrarios para a v inicial e criando as keys para o mapping
            foreach (var s in stateSet)
            {
                v.Add(s, 0.0);
                var n = new List<string>();
                mapping.Add(s.ToString(), n);
            }


            // Value Iteration Mathod
            while ((erro > (precisao * (1 - d)) / (2 * d)) || (iteradorVIM == 1000))            // for (var i = 0; i < 100; i++) // RESTRIÇÃO: 100 ITERAÇÕES
            {
                Dictionary<AbstractState, double> vAntigo = new Dictionary<AbstractState, double>(v);
                foreach (var s in stateSet)
                {
                    // Registrando os eventos permitidos para o estado s e os eventos destino possíveis
                    List<AbstractState> estadosDestino = new List<AbstractState>();
                    List<AbstractEvent> eventosPermitidos = new List<AbstractEvent>();
                    foreach (var t in allowedEvents[s])
                    {
                        estadosDestino.Add(t.Destination);
                        eventosPermitidos.Add(t.Trigger);
                    }

                    // Calculando a esperança de maximização de paralelismo para cada ação
                    List<double> esperancas = new List<double>();
                    List<(double esperanca, AbstractEvent evento)> tuplasEventos = new List<(double esperanca, AbstractEvent evento)>();
                    foreach (var a in eventosPermitidos)
                    {
                        double sum = 0.0;
                        foreach (var sDest in estadosDestino)
                        {
                            sum += (double)Probabilidade(s, sDest, a) * (sDest.ActiveTasks() + d * vAntigo[sDest]);
                        }
                        tuplasEventos.Add((esperanca: sum, evento: a));
                        esperancas.Add(sum);
                    }

                    v[s] = Max(esperancas);
                    mapping[s.ToString()] = tuplasEventos.OrderByDescending(i => i.esperanca).ThenBy(i => i.evento.ToString()).Select(i => i.evento.ToString()).ToList();


                }
                erro = Math.Sqrt(v.Zip(vAntigo, (esp, espAntiga) => { return Math.Pow(esp.Value - espAntiga.Value, 2); }).ToList().Sum());
                iteradorVIM += 1;
            }

            return mapping;
        }

        /// <summary>
        /// Retorna o maior valor de um array
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        static double Max(List<double> dados)
        {
            double max = 0;
            foreach (var value in dados)
            {
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        /// <summary>
        /// Retorna uma lista de transições com base em uma politica e nas carateristicas da FMS
        /// </summary>
        /// <param name="politica"></param>
        /// <param name="qtdProdutos"></param>
        /// <param name="problema"></param>
        /// <returns></returns>
        static SimulationResult TransicoesProducao(Politica politica, int qtdProdutos, FMS problema)
        {
            AbstractState estadoAtual = problema.Supervisor.InitialState;
            Restriction restricao = problema.InitialRestriction(qtdProdutos);
            Scheduler scheduler = problema.InitialScheduler();
            IEnumerable<AbstractEvent> eventSet = problema.Supervisor.Events;
            string nomeEventoOtimo = politica[estadoAtual.ToString()][0];
            AbstractEvent eventoOtimo = eventSet.ToArray()[0]; // Assinalamento arbitrario
            foreach (var e in eventSet)
            {
                if (e.ToString() == nomeEventoOtimo) {
                    eventoOtimo = e;
                    break;
                }
            }


            int qtdEventosPossiveis = politica[estadoAtual.ToString()].Count;
            float tempoDeProducao = 0;
            List<AbstractEvent> sequenciaEventos = new List<AbstractEvent>();
            uint paralelismoAcumulado = 0;
            int depth = 44 * qtdProdutos;



            for (var i = 0; i < depth; i++)
            {
                var minimumTime = float.PositiveInfinity;
                foreach (var e in allowedEvents[estadoAtual])
                {
                    if (!e.Trigger.IsControllable)
                    {
                        if (scheduler[e.Trigger] < minimumTime) minimumTime = scheduler[e.Trigger];
                    }
                }

                List<AbstractEvent> forbidenEvents = allowedEvents[estadoAtual].Where(e => restricao.ContainsKey(e.Trigger) ? restricao[e.Trigger] == 0 : ((scheduler[e.Trigger] > minimumTime) || 
                (float.IsNaN(scheduler[e.Trigger])))).Select(i => i.Trigger).ToList();

                foreach (var e in politica[estadoAtual.ToString()])
                {
                    if (!forbidenEvents.Select(i => i.ToString()).Contains(e))
                    {
                        foreach (var t in allowedEvents[estadoAtual])
                        {
                            if (t.Trigger.ToString() == e)
                            {
                                eventoOtimo = t.Trigger;
                                estadoAtual = t.Destination;
                                break;
                            }
                        }
                        break;
                    }
                }

                sequenciaEventos.Add(eventoOtimo);
                tempoDeProducao += scheduler[eventoOtimo];
                paralelismoAcumulado += estadoAtual.ActiveTasks();
                scheduler = problema.UpdatedScheduler(scheduler, eventoOtimo);
                restricao = problema.UpdateRestriction(restricao, eventoOtimo);
            }

            var result = new SimulationResult(makespan: tempoDeProducao, parallelism: paralelismoAcumulado, eventSequence: sequenciaEventos);

            return result;
        }

    }

    public class SimulationResult {
        public float makespan {get;set;}
        public uint parallelism { get; set; }
        public List<AbstractEvent> eventSequence { get; set; }
        public SimulationResult(float makespan, uint parallelism, List<AbstractEvent> eventSequence)
        {
            this.makespan = makespan;
            this.parallelism = parallelism;
            this.eventSequence = eventSequence;
        }
    }
}
