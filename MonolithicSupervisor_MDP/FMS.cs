using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UltraDES;
using Scheduler = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, float>;
using Restriction = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, uint>;
using Update = System.Func<System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, float>, UltraDES.AbstractEvent, System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, float>>;
using TableTime = System.Collections.Generic.Dictionary<UltraDES.AbstractEvent, (UltraDES.AbstractEvent eventoNaoControlavel, float tempo)>;

namespace MultiAgentMarkovMonolithic
{
     
    public class FMS
    {
        public readonly Dictionary<int, Event> e;

        public readonly TableTime table;      
        public DeterministicFiniteAutomaton Supervisor { get; set; }

        public FMS()
        {
            e = new int[]
            {
                11, 12, 21, 22, 41,
                42, 51, 52, 53, 54, 31,
                32, 33, 34, 35, 36, 37, 38, 39, 30, 61,
                63, 65, 64, 66, 71, 72, 73, 74, 81, 82
            }.ToDictionary(alias => alias,
                 alias =>
                     new Event(alias.ToString(),
                         alias % 2 == 0 ? Controllability.Uncontrollable : Controllability.Controllable)); ;

            table = aproximatedTime();

            // var s = Enumerable.Range(0, 6).Select(i => new State($"s{i}", i == 0 ? Marking.Marked : Marking.Unmarked)).ToArray();
            
            // Criando os estados. O estado 0 não terá nenhuma tarefa ativa e os demais terão 1 tarefa ativa 
            var s = Enumerable.Range(0, 6)
               .ToDictionary(i => i,
                   i => new ExpandedState(i.ToString(), i == 0 ? 0u : 1u, i == 0 ? Marking.Marked : Marking.Unmarked));

            // Maquinas
            var c1 = new Conveyor(estadoLigado: s[1], estadoDesligado: s[0], eventoLigar: e[11], eventoDesligar: e[12], nameConveyor: "C1");
            var c2 = new Conveyor(estadoLigado: s[1], estadoDesligado: s[0], eventoLigar: e[21], eventoDesligar: e[22], nameConveyor: "C2");
            var mill = new Mill(estadoLigado: s[1], estadoDesligado: s[0], eventoLigar: e[41], eventoDesligar: e[42], nameMill: "Mill");
            var pm = new PaintingMachine(estadoLigado: s[1], estadoDesligado: s[0], eventoLigar: e[81], eventoDesligar: e[82], namePaintingMachine: "PM");
            var lathe = new Lathe(torneamentoCilindrico: s[2], torneamentoConico: s[1], estadoDesligado: s[0], tornearCilindro: e[53],
                tornearConico: e[51], desligarConico: e[52], desligarCilindrico: e[54], nameLathe: "lathe");
            var c3 = new TwoWayConveyor(off: s[0], toRight: e[71], movingRight: s[1], offRight: e[72], toLeft: e[73], movingLeft: s[2],
                offLeft: e[74], nameConveyor: "C3");
            var robot = new Robot(estadoDesligado: s[0], takeBase: e[31], holdBase: s[1], deliverBase: e[32],
                takeComplement: e[33], holdComplement: s[2], deliverComplement: e[34],
                takeCylinder: e[39], holdCylinder: s[3], deliverCylinder: e[30],
                takeCone: e[37], holdCone: s[4], deliverCone: e[38],
                takeMilled: e[35], holdMilled: s[5], deliverMilled: e[36], nameRobot: "Robot");
            var am = new AssemblyMachine(estadoDesligado: s[0], takeBase: e[61], holdBase: s[1],
                takeCone: e[63], assemblyConical: s[2], deliverConical: e[64], takeCylinder: e[65],
                assemblyCylindrical: s[3], deliverCylindrical: e[66], nameAssembler: "AM");


            // Especificações
            var b1 = new ReceiveDeliver(empty: s[0], full: s[1], wait: e[11], receive: e[12], deliver: e[31], bufferName: "B1");
            var b2 = new ReceiveDeliver(empty: s[0], full: s[1], wait: e[21], receive: e[22], deliver: e[33], bufferName: "B2");
            var b3 = new TwoWaysOneMultiTask(waitTask1: e[31], waitTask2: e[36], empty: s[0], receiveMultiTask: e[32], deliverDedicated: e[41],
                fullMultiTask: s[1], receiveDedicated: e[42], fullDedicated: s[2], deliverMultiTask: e[35], nameBuffer: "B3");
            var b4 = new RawAndTwoForms(waitRaw: e[33], waitDeliver1: e[38], waitDeliver2: e[30], empty: s[0], receiveRaw: e[34],
                deliverToForm1: e[51], deliverToForm2: e[53], fullRaw: s[1], receiveProcessed1: e[52], fullProcessed1: s[2],
                deliverProcessed1: e[37], receiveProcessed2: e[54], fullProcessed2: s[3], deliverProcessed2: e[39], nameBuffer: "B4");
            var b5 = new ReceiveDeliver(empty: s[0], full: s[1], wait: e[35], receive: e[36], deliver: e[61], bufferName: "B5");
            var b6 = new ReceiveDeliver(empty: s[0], full: s[1], wait: e[37], receive: e[38], deliver: e[63], bufferName: "B6");
            var b7 = new TwoWays(empty: s[0], receiveRaw: e[30], deliverRaw: e[71], fullRaw: s[1],
                receiveProcessed: e[74], fullProcessed: s[2], deliverProcessed: e[65], nameBuffer: "B7");
            var b8 = new TwoWays(empty: s[0], receiveRaw: e[72], deliverRaw: e[81], fullRaw: s[1], receiveProcessed: e[82],
                fullProcessed: s[2], deliverProcessed: e[73], nameBuffer: "B8");


            
            Supervisor = DeterministicFiniteAutomaton.MonolithicSupervisor(new[] { c1.Automato, c2.Automato, mill.Automato, pm.Automato, lathe.Automato, c3.Automato, robot.Automato, am.Automato },
                new[] { b1.Automato, b2.Automato, b3.Automato, b4.Automato, b5.Automato, b6.Automato, b7.Automato, b8.Automato }, true);
                      

        }

        public Restriction InitialRestriction(int products)
        {
            return new Restriction
            {
                {e[11], (uint) (2*products)},
                {e[21], (uint) (2*products)},
                {e[31], (uint) (2*products)},
                {e[33], (uint) (2*products)},
                {e[35], (uint) (2*products)},
                {e[37], (uint) (1*products)},
                {e[39], (uint) (1*products)},
                {e[41], (uint) (2*products)},
                {e[51], (uint) (1*products)},
                {e[53], (uint) (1*products)},
                {e[61], (uint) (2*products)},
                {e[63], (uint) (1*products)},
                {e[65], (uint) (1*products)},
                {e[71], (uint) (1*products)},
                {e[73], (uint) (1*products)},
                {e[81], (uint) (1*products)}
            };
        }

        public Scheduler InitialScheduler()
        {
            Scheduler sch = new Scheduler();
            // var eventosFSM = e.ToArray();
            foreach(var ev in e)
            {
                var tempo = ev.Value.IsControllable ? 0.0f : float.PositiveInfinity;
                sch.Add(ev.Value, tempo);
            }

            return sch;
        }

        public Scheduler UpdatedScheduler(Scheduler old, AbstractEvent ev)
        {
            // kvp = par chave-valor
            var newScheduler = old.ToDictionary(kvp => kvp.Key, kvp =>
            {
                float v = kvp.Value - old[ev];
                if ((table.ContainsKey(ev)) && (table[ev].eventoNaoControlavel == kvp.Key)) v = table[ev].tempo;
                else if (kvp.Key.IsControllable) return v < 0 ? 0.0f : v;
                else if (v < 0) return float.NaN;
                else if (v == 0) v = float.PositiveInfinity;
                return v;
            });
            return newScheduler;
        }

        public Restriction UpdateRestriction (Restriction old, AbstractEvent ev)
        {
            if (old.ContainsKey(ev)) {
                old[ev] -= 1;
            } 
            return old;
        }

        public TableTime aproximatedTime() 
        {
            return new TableTime
            {
                { e[11], (eventoNaoControlavel: e[12], tempo: 25) },
                { e[21], (eventoNaoControlavel: e[22], tempo: 25) },
                { e[31], (eventoNaoControlavel: e[32], tempo: 21) },
                { e[33], (eventoNaoControlavel: e[34], tempo: 19) },
                { e[35], (eventoNaoControlavel: e[36], tempo: 16) },
                { e[37], (eventoNaoControlavel: e[38], tempo: 24) },
                { e[39], (eventoNaoControlavel: e[30], tempo: 20) },
                { e[51], (eventoNaoControlavel: e[52], tempo: 38) },
                { e[53], (eventoNaoControlavel: e[54], tempo: 32) },
                { e[41], (eventoNaoControlavel: e[42], tempo: 30) },
                { e[71], (eventoNaoControlavel: e[72], tempo: 25) },
                { e[73], (eventoNaoControlavel: e[74], tempo: 25) },
                { e[63], (eventoNaoControlavel: e[64], tempo: 25) },
                { e[65], (eventoNaoControlavel: e[66], tempo: 25) },
                { e[81], (eventoNaoControlavel: e[82], tempo: 24) }
            };
        }
    }

}
