using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UltraDES;
using Politica = System.Collections.Generic.Dictionary<UltraDES.AbstractState, System.Collections.Generic.List<UltraDES.AbstractEvent>>;
using Scheduler = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, float>;
using Restriction = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, uint>;

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


            var timerOptimization = new Stopwatch();
            timerOptimization.Start();
                Politica map = ValueIterationMethod(supervisor.States, supervisor.Events);
            timerOptimization.Stop();

            Console.WriteLine($"\nPolitica de otimizacao encontrada em {timerOptimization.ElapsedMilliseconds / 1000.0} seg");

            var timerProducao = new Stopwatch();
            timerProducao.Start();
                List<Transition> transicoes = TransicoesProducao(politica: map, qtdProdutos: 2, problema: problema);
            timerProducao.Stop();

            Console.WriteLine($"\nProducao realizada em {timerProducao.ElapsedMilliseconds / 1000.0} seg");
            Console.WriteLine($"Foram realizadads {transicoes.Count} transições, sendo elas:\n");
            foreach(var t in transicoes)
            {
                Console.WriteLine(t);
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
            Politica mapping = new Dictionary<AbstractState, List<AbstractEvent>>();
            
            // d = constante de desconto
            double d = 0.7;
            
            // Atribuindo valores arbitrarios para a v inicial e criando as keys para o mapping
            foreach (var s in stateSet)
            {
                v.Add(s, 0.0);
                var n = new List<AbstractEvent>();
                mapping.Add(s, n);
            }

            // Value Iteration Mathod
            for (var i = 0; i <= 100; i++) // RESTRIÇÃO: 100 ITERAÇÕES
            {
                foreach (var s in stateSet)
                {
                    // Registrando os eventos permitidos para o estado s e os eventos destino possíveis
                    List<AbstractState> estadosDestino = new List<AbstractState>();
                    List<AbstractEvent> eventosPermitidos = new List<AbstractEvent>();
                    foreach(var t in allowedEvents[s])
                    {
                        estadosDestino.Add(t.Destination);
                        eventosPermitidos.Add(t.Trigger);
                    }
                    
                    // Calculando a esperança de maximização de paralelismo para cada ação
                    List<double> esperancas = new List<double>();
                    SortedDictionary<double, AbstractEvent> eventosOrdenadosPorEsperanca = new SortedDictionary<double, AbstractEvent>();
                    foreach (var a in eventosPermitidos)
                    {                        
                        double sum = 0.0;
                        foreach (var sDest in estadosDestino)
                        {
                            sum += (double)Probabilidade(s, sDest, a) * (sDest.ActiveTasks() + d * v[sDest]);
                        }
                        if (!eventosOrdenadosPorEsperanca.ContainsKey(sum))
                        {
                            eventosOrdenadosPorEsperanca.Add(sum, a);
                        }
                        else
                        {
                            double diff = 1e-10;
                            while (eventosOrdenadosPorEsperanca.ContainsKey(sum - diff))
                            {
                                diff = diff + diff / 10;
                            }
                            eventosOrdenadosPorEsperanca.Add(sum - diff, a); //adiciona uma chave diferente ao dicionário
                        }
                        esperancas.Add(sum);                        
                    }
                    
                    v[s] = Max(esperancas);
                    // v[s] = eventosOrdenadosPorEsperanca.First().Key;
                    var eventosOrdenados = eventosOrdenadosPorEsperanca.Values.ToList();
                    eventosOrdenados.Reverse();
                    mapping[s] = eventosOrdenados;
                }
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

        static List<Transition> TransicoesProducao(Politica politica, int qtdProdutos, FMS problema)
        {
            AbstractState estadoAtual = problema.Supervisor.InitialState;
            Restriction restricao = problema.InitialRestriction(qtdProdutos);
            Scheduler scheduler = problema.InitialScheduler();
            AbstractEvent eventoOtimo = politica[estadoAtual][0];
            int qtdEventosPossiveis = politica[estadoAtual].Count;
            int qtdTransicoes = 0;
            int itEventOtimo = 0;
            float tempoDeProducao = 0;
            List<Transition> transicoes = new List<Transition>();
            bool pararProducao = false;

            // Enquanto existirem eventos permitidos na restricao
            while (qtdEventosPossiveis > 0)
            {
                if (restricao.ContainsKey(eventoOtimo))
                {
                    if (restricao[eventoOtimo] == 0) 
                    {
                        while (restricao[eventoOtimo] == 0)
                        {
                            qtdEventosPossiveis -= 1;
                            itEventOtimo += 1;
                            if (itEventOtimo < politica[estadoAtual].Count)
                            {
                                eventoOtimo = politica[estadoAtual][itEventOtimo];
                            }
                            else if ((!float.IsNaN(scheduler[eventoOtimo])) || (itEventOtimo >= politica[estadoAtual].Count))
                            { 
                                pararProducao = true;
                                break;
                            }
                            if (!restricao.ContainsKey(eventoOtimo)) break;
                        }                        
                        itEventOtimo = 0;
                    } 
                }
                if (!(scheduler[eventoOtimo] is float.NaN) && !(scheduler[eventoOtimo] is float.PositiveInfinity)) tempoDeProducao += scheduler[eventoOtimo];
                if (pararProducao) break;
                // Atualizando restricao e scheduler
                restricao = problema.UpdateRestriction(restricao, eventoOtimo);
                scheduler = problema.UpdatedScheduler(old: scheduler, ev: eventoOtimo);

                // Procurando o proximo estado
                foreach (var t in allowedEvents[estadoAtual])
                {
                    if (t.Trigger == eventoOtimo)
                    {
                        estadoAtual = t.Destination;
                        eventoOtimo = politica[estadoAtual][0];
                        transicoes.Add(t);
                        break;
                    }
                }
                qtdTransicoes += 1;
                qtdEventosPossiveis = politica[estadoAtual].Count;
            }

            Console.WriteLine("Já nao existem mais eventos permitidos pela politica");
            Console.WriteLine($"Tempo de execução: {tempoDeProducao} u.t.");
            
            return transicoes;
        }
    }
}
