using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UltraDES;

namespace MultiAgentMarkovMonolithic
{
    class Program
    {
        private static Dictionary<AbstractState, Transition[]> allowedEvents;

        static void Main(string[] args)
        {
            var problema = new FMS();

            var supervisor = problema.Supervisor;
            
            // Relacionando cada estado com suas respectivas transições
            allowedEvents = supervisor.Transitions.GroupBy(t => t.Origin).ToDictionary(g => g.Key, g => g.ToArray());



        }

        /// <summary>
        /// Retorna a probabilidade de um evento "e" ocorrer quando se deseja sair de um estado "origem" até um estado "destino"
        /// </summary>
        /// <param name="origem"></param>
        /// <param name="destino"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static double Probabilidade(AbstractState origem, AbstractState destino, Event e)
        {
            AbstractState destinoReal = origem; //se nao houver transicao com o evento "e", o destino sera o proprio estado de origem
            bool isControlable = false;
            int qtd_uncontrollables = 0;

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
                if (destino != destinoReal) return 0;
                else return 1;
            }
            else
            {
                if (destino != destinoReal) return 1 - (1 / qtd_uncontrollables);
                else return 1 / qtd_uncontrollables;
            }
        }


    }
}
