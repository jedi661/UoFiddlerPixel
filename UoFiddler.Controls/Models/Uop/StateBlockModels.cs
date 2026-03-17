using System;
using System.Collections.Generic;
using Ultima;
using UoFiddler.Controls.Uop; // For CreatureState

namespace UoFiddler.Controls.Models.Uop
{
    public class ActionRemapping
    {
        public int ActionIndex { get; set; }
        public int TargetGroupId { get; set; }
    }

    public class StateSubdivision
    {
        public int Count { get; set; }
        public List<ActionRemapping> Actions { get; set; } = new List<ActionRemapping>();
        public int? Modifier { get; set; }
        public bool HasClosingFF { get; set; } // Indicates if this subdivision ended with FF marker
        public List<StateSubdivision> SubDetails { get; set; } = new List<StateSubdivision>();
    }

    public class StateBlock
    {
        public CreatureState State { get; set; }
        public int SubdivisionCount { get; set; } // Corresponds to complexModCount or number of implicit triplets
        public List<StateSubdivision> Subdivisions { get; set; } = new List<StateSubdivision>();
        
        // This is a simplified representation. For full fidelity,
        // we might need to store more raw context or a richer model
        // of implicit triplets and other block structures.
    }
}
