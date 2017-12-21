﻿using NeighborDiscovery.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeighborDiscovery.Nodes
{
    public class AccGossipPNihao : PNihao
    {
        public AccGossipPNihao(int id, int duty, int communicationRange, int channelOccupancyRate, int startUpTime, bool randomInitialState = false) : base(id, duty, communicationRange, channelOccupancyRate, startUpTime, randomInitialState)
        {

        }

        public override bool ListenTo(Transmission transmission, out List<IDiscovery> discoveredNodes)
        {
            discoveredNodes = new List<IDiscovery>();
            if (transmission == null)
                throw new Exception("Null transmission received.");
            if (IsListening(transmission.Slot))
            {
                if (!WasDiscovered(transmission.Sender))
                {
                    discoveredNodes.Add(transmission.Sender);
                    Neighbors[transmission.Sender] = new ContactInfo(transmission.Slot);
                }
                else
                {
                    //update
                    if (transmission.Slot > Neighbors[transmission.Sender].LastMet)
                    {
                        Neighbors[transmission.Sender].Update(transmission.Slot);
                    }
                }
                //gossip
                foreach (var twoHopTuple in transmission.Sender.NeighborsDiscovered())
                {
                    var twoHopNeighbor = twoHopTuple.Item1;//two hop neighbor
                    var lastMet = twoHopTuple.Item2.LastMet;
                    var firstDiscovered = twoHopTuple.Item2.FirstDiscovered;
                    if (!WasDiscovered(twoHopNeighbor) && firstDiscovered < transmission.Slot)//I havn't discovered this 2-hop neighbor
                    {
                        //look up for me
                        foreach (var threeHopTuple in twoHopNeighbor.NeighborsDiscovered())//add this later: .Where(x => x.Item2 < lastMet)
                        {
                            var threeHopNeighbor = threeHopTuple.Item1;
                            if (threeHopNeighbor.Equals(this))//he already discovered me
                            {
                                var discoveredMe = threeHopTuple.Item2.FirstDiscovered;
                                if (discoveredMe < lastMet)
                                {
                                    Neighbors[twoHopNeighbor] = new ContactInfo(transmission.Slot);
                                    Neighbors[twoHopNeighbor].Update(lastMet);
                                    discoveredNodes.Add(twoHopNeighbor);
                                }
                            }
                        }
                    }
                    else if (firstDiscovered < transmission.Slot)
                    {
                        //update
                        if (lastMet > Neighbors[twoHopNeighbor].LastMet)
                        {
                            Neighbors[twoHopNeighbor].Update(lastMet);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Acc " + base.ToString();
        }
    }
}
