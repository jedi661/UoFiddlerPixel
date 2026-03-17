using System.Collections.Generic;

namespace UoFiddler.Controls.Uop
{
    public enum CreatureState
    {
        Base,
        War,
        Peace,
        MountPeace,
        MountWar
    }

    public class StateOverride
    {
        public int GroupId { get; set; }
        public ushort? Modifier { get; set; }
    }

    public class IndexDataAnimation
    {
        private List<IndexDataAnimationGroupUOP?> _uopGroups;
        public Dictionary<int, int> UopRemap { get; private set; }
        public Dictionary<CreatureState, Dictionary<int, StateOverride>> StateOverrides { get; private set; }

        public IndexDataAnimation()
        {
            _uopGroups = new List<IndexDataAnimationGroupUOP?>();
            UopRemap = new Dictionary<int, int>();
            StateOverrides = new Dictionary<CreatureState, Dictionary<int, StateOverride>>();
        }

        public IndexDataAnimationGroupUOP AddUopGroup(int groupIndex, IndexDataAnimationGroupUOP group)
        {
            while (_uopGroups.Count <= groupIndex)
            {
                _uopGroups.Add(null);
            }
            _uopGroups[groupIndex] = group;
            return group;
        }

        public IndexDataAnimationGroupUOP? GetUopGroup(int groupIndex, bool createIfNull)
        {
            if (groupIndex < _uopGroups.Count && _uopGroups[groupIndex] != null)
            {
                return _uopGroups[groupIndex];
            }
            else if (createIfNull)
            {
                IndexDataAnimationGroupUOP newGroup = new IndexDataAnimationGroupUOP();
                AddUopGroup(groupIndex, newGroup);
                return newGroup;
            }
            return null;
        }
    }
}
