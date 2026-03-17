using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ultima;
using UoFiddler.Controls.Models.Uop;
using UoFiddler.Controls.Models.Uop.Imaging;
using IndexDataFileInfo = UoFiddler.Controls.Uop.IndexDataFileInfo;

namespace UoFiddler.Controls.Uop
{
    public class UopAnimationDataManager
    {
        private IndexDataAnimation[] _dataIndex;
        private List<UopFileReader> _animationFrameReaders;
        private UopFileReader _animationSequenceReader;

        public Dictionary<uint, List<AnimationSequenceEntry>> SequenceEntries { get; private set; }
        public Dictionary<uint, byte[]> RawStateBlocks { get; private set; }
        public Dictionary<uint, ulong> AnimationIdToHash { get; private set; }

        private Dictionary<ulong, byte[]> _unknownEntries = new Dictionary<ulong, byte[]>();
        private Dictionary<ulong, UopDataHeader> _unknownMetadata = new Dictionary<ulong, UopDataHeader>();

        private MainMiscManager _mainMiscManager = new MainMiscManager();
        public bool MainMiscModified => _mainMiscManager.IsModified;

        internal Dictionary<string, UopAnimIdx> _animCache = new Dictionary<string, UopAnimIdx>();

        public UopAnimationDataManager()
        {
            _dataIndex = new IndexDataAnimation[UopConstants.MAX_ANIMATIONS_DATA_INDEX_COUNT];
            for (int i = 0; i < _dataIndex.Length; i++)
            {
                _dataIndex[i] = new IndexDataAnimation();
            }
            _animationFrameReaders = new List<UopFileReader>();
            SequenceEntries = new Dictionary<uint, List<AnimationSequenceEntry>>();
            RawStateBlocks = new Dictionary<uint, byte[]>();
            AnimationIdToHash = new Dictionary<uint, ulong>();
        }

        public List<string> LoadedUopFiles => _animationFrameReaders.Select(r => r.FilePath).ToList();

        public UopFileReader GetReaderByPath(string path)
        {
            return _animationFrameReaders.FirstOrDefault(r => r.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        public bool LoadUopFiles()
        {
            string root = Files.RootDir;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return false;
            }

            string animSeqPath = Files.GetFilePath("AnimationSequence.uop") ?? Path.Combine(root, "AnimationSequence.uop");
            if (File.Exists(animSeqPath))
            {
                _animationSequenceReader = new UopFileReader(animSeqPath);
                _animationSequenceReader.Load();
            }

            for (int i = 1; i <= UopConstants.MAX_ANIMATION_FRAME_UOP_FILES; i++)
            {
                string uopFileName = $"AnimationFrame{i}.uop";
                string uopFilePath = Files.GetFilePath(uopFileName) ?? Path.Combine(root, uopFileName);

                if (File.Exists(uopFilePath))
                {
                    var reader = new UopFileReader(uopFilePath);
                    if (reader.Load())
                    {
                        _animationFrameReaders.Add(reader);
                    }
                }
            }

            return _animationFrameReaders.Count > 0 || (_animationSequenceReader != null && _animationSequenceReader.IsLoaded);
        }

        public void ProcessUopData()
        {
            FindBaseAnimations();
            ParseAnimationSequence();
        }

        private void FindBaseAnimations()
        {
            for (int animId = 0; animId < UopConstants.MAX_ANIMATIONS_DATA_INDEX_COUNT; animId++)
            {
                for (int group = 0; group < UopConstants.ANIMATION_UOP_GROUPS_COUNT; group++)
                {
                    string hashString = $"build/animationlegacyframe/{animId:D6}/{group:D2}.bin";
                    ulong hash = UopFileReader.CreateHash(hashString);

                    foreach (var reader in _animationFrameReaders)
                    {
                        UopDataHeader? header = reader.GetEntryByHash(hash);
                        if (header.HasValue)
                        {
                            var existingGroup = _dataIndex[animId].GetUopGroup(group, false);

                            if (existingGroup == null)
                            {
                                var indexGroup = _dataIndex[animId].AddUopGroup(group, new IndexDataAnimationGroupUOP());
                                var sharedFileInfo = new IndexDataFileInfo(reader, header.Value);

                                for (int dir = 0; dir < 5; dir++)
                                {
                                    indexGroup.m_Direction[dir] = sharedFileInfo;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void ParseAnimationSequence()
        {
            if (_animationSequenceReader == null || !_animationSequenceReader.IsLoaded)
            {
                return;
            }

            _unknownEntries.Clear();
            _unknownMetadata.Clear();

            foreach (var entry in _animationSequenceReader.GetAllEntries())
            {
                byte[] entryData = _animationSequenceReader.ReadData(entry.Value);
                if (entryData == null)
                {
                    byte[] raw = _animationSequenceReader.ReadRawData(entry.Value);
                    if (raw != null)
                    {
                        _unknownEntries[entry.Key] = raw;
                        _unknownMetadata[entry.Key] = entry.Value;
                    }
                    continue;
                }

                bool parsed = ParseAnimationSequenceEntry(entryData, entry.Key);

                if (!parsed)
                {
                    byte[] raw = _animationSequenceReader.ReadRawData(entry.Value);
                    if (raw != null)
                    {
                        _unknownEntries[entry.Key] = raw;
                        _unknownMetadata[entry.Key] = entry.Value;
                    }
                    continue;
                }

                uint animId = BitConverter.ToUInt32(entryData, 0);

                // ✅ CORRECTION CRITIQUE : Le fichier original utilise UNIQUEMENT le format D8
                string standardPath = $"build/animationsequence/{animId:D8}.bin";
                ulong standardHash = UopFileReader.CreateHash(standardPath);

                // ✅ Si ce n'est PAS le hash D8 standard, c'est une entrée "exotic" (rare)
                if (entry.Key != standardHash)
                {
                    byte[] raw = _animationSequenceReader.ReadRawData(entry.Value);
                    if (raw != null)
                    {
                        _unknownEntries[entry.Key] = raw;
                        _unknownMetadata[entry.Key] = entry.Value;
                        System.Diagnostics.Debug.WriteLine($"[UOP] Preserving NON-STANDARD hash for ID {animId}: Hash={entry.Key:X16} (expected D8: {standardHash:X16})");
                    }
                }
            }
        }

        public bool ParseAnimationSequenceEntry(byte[] entryData, ulong hash = 0)
        {
            if (entryData == null) return false;

            using var ms = new MemoryStream(entryData);
            using var reader = new CustomBinaryReader(ms);

            if (reader.BaseStream.Length < 52) return false;

            uint animationIndex = reader.ReadUInt32LE();
            if (animationIndex < _dataIndex.Length && _dataIndex[animationIndex] == null)
            {
                _dataIndex[animationIndex] = new IndexDataAnimation();
            }

            if (animationIndex >= _dataIndex.Length) return false;

            if (hash != 0)
            {
                // ✅ CORRECTION : Stocker le hash D8 au lieu de D6
                AnimationIdToHash[animationIndex] = hash;
            }

            _dataIndex[animationIndex].UopRemap.Clear();
            _dataIndex[animationIndex].StateOverrides.Clear();

            if (!SequenceEntries.ContainsKey(animationIndex))
            {
                SequenceEntries[animationIndex] = new List<AnimationSequenceEntry>();
            }
            SequenceEntries[animationIndex].Clear();

            reader.Move(16);
            reader.Move(32);
            uint replacesCount = reader.ReadUInt32LE();

            for (int i = 0; i < replacesCount; i++)
            {
                if (reader.BaseStream.Position + 72 > reader.BaseStream.Length) break;

                uint uopGroupIndex = reader.ReadUInt32LE();
                int framesCount = reader.ReadInt32LE();
                uint mulGroupIndex = reader.ReadUInt32LE();
                float speed = reader.ReadSingle();
                byte[] extraData = reader.ReadBytes(56);

                var entry = new AnimationSequenceEntry
                {
                    UopGroupIndex = uopGroupIndex,
                    FrameCount = framesCount,
                    MulGroupIndex = mulGroupIndex,
                    Speed = speed,
                    ExtraData = extraData
                };
                SequenceEntries[animationIndex].Add(entry);

                if (framesCount == 0 && uopGroupIndex < UopConstants.ANIMATION_GROUPS_COUNT && mulGroupIndex < UopConstants.ANIMATION_GROUPS_COUNT)
                    _dataIndex[animationIndex].UopRemap[(int)uopGroupIndex] = (int)mulGroupIndex;
                else if (framesCount > 0 && uopGroupIndex < UopConstants.ANIMATION_UOP_GROUPS_COUNT)
                    _dataIndex[animationIndex].GetUopGroup((int)uopGroupIndex, true)?.SetFrameCount(framesCount);
            }

            if (reader.BaseStream.Position + 6 > reader.BaseStream.Length) return true;

            long savePos = reader.BaseStream.Position;
            long remainingBytes = reader.BaseStream.Length - savePos;
            if (remainingBytes > 0)
            {
                reader.BaseStream.Seek(savePos, SeekOrigin.Begin);
                RawStateBlocks[animationIndex] = reader.ReadBytes((int)remainingBytes);
                reader.BaseStream.Seek(savePos, SeekOrigin.Begin);
            }

            uint stateBlockCount = reader.ReadUInt32LE();
            byte pad0 = reader.ReadByte();
            byte padFF = reader.ReadByte();

            if (pad0 != 0x00 || padFF != 0xFF)
            {
                reader.BaseStream.Position = savePos;
                stateBlockCount = 0;
            }

            if (stateBlockCount >= 4)
            {
                var forcedStates = new[] { CreatureState.War, CreatureState.Peace, CreatureState.MountPeace, CreatureState.MountWar };
                for (int i = 0; i < 4; i++)
                {
                    ParseStateSection(reader, animationIndex, forcedStates[i]);
                }
            }
            else
            {
                for (int stateIndex = 0; stateIndex < stateBlockCount; stateIndex++)
                {
                    ParseStateSection(reader, animationIndex, (CreatureState)(stateIndex + 1));
                }
            }

            return true;
        }

        private void ParseStateSection(CustomBinaryReader reader, uint animationIndex, CreatureState state)
        {
            var currentStateOverrides = new Dictionary<int, StateOverride>();

            if (!SeekLiteral(reader, new byte[] { 0x03, 0x00, 0x00, 0x00 }, 8192))
            {
                _dataIndex[animationIndex].StateOverrides[state] = currentStateOverrides;
                return;
            }

            ReadImplicitTriplet9(reader, animationIndex, state);

            if (_dataIndex[animationIndex].StateOverrides.TryGetValue(state, out var implicitDict))
            {
                foreach (var kvp in implicitDict)
                    currentStateOverrides[kvp.Key] = kvp.Value;
            }

            if (reader.BaseStream.Position + 6 > reader.BaseStream.Length)
            {
                _dataIndex[animationIndex].StateOverrides[state] = currentStateOverrides;
                return;
            }

            long complexModPos = reader.BaseStream.Position;
            uint complexModCount = reader.ReadUInt32LE();
            byte hdr0 = reader.ReadByte();
            byte hdrFF = reader.ReadByte();

            if (hdrFF != 0xFF)
            {
                reader.BaseStream.Position = complexModPos;
                _dataIndex[animationIndex].StateOverrides[state] = currentStateOverrides;
                return;
            }

            for (int g = 0; g < complexModCount; g++)
            {
                if (reader.BaseStream.Position + 4 > reader.BaseStream.Length) break;

                uint actionCount = reader.ReadUInt32LE();
                if (actionCount > 255) break;

                var groupIds = new List<int>();
                for (int k = 0; k < actionCount; k++)
                {
                    if (reader.BaseStream.Position + 4 > reader.BaseStream.Length) break;
                    groupIds.Add(reader.ReadInt32LE());
                }

                if (!groupIds.Any()) continue;

                ushort? modifier = null;
                bool isLastMountWarSubdivision = (state == CreatureState.MountWar && g == complexModCount - 1);

                if (!isLastMountWarSubdivision)
                {
                    if (reader.BaseStream.Position + 2 <= reader.BaseStream.Length)
                    {
                        byte b1 = reader.ReadByte();
                        byte b2 = reader.ReadByte();
                        modifier = (ushort)((b1 << 8) | b2);
                    }
                }
                ApplySimpleOverrides(state, currentStateOverrides, groupIds, modifier);
            }

            _dataIndex[animationIndex].StateOverrides[state] = currentStateOverrides;
        }

        private void ApplySimpleOverrides(CreatureState state, Dictionary<int, StateOverride> overrides, List<int> groupIds, ushort? modifier)
        {
            if (!groupIds.Any()) return;

            var warActionMap = new Dictionary<int, int>
            {
                {0x0A, 10}, {0x0F, 15}, {0x02, 2}, {0x03, 3}, {0x1B, 27}, {0x1C, 28},
                {0x0C, 12}, {0x0B, 25}, {0x17, 26}, {0x13, 19}, {0x15, 21}
            };

            var warGroupedActions = new Dictionary<int, int[]>
            {
                {0x04, new[] { 4, 5, 6, 7, 8, 9 } },
                {0x0F, new[] { 15, 16 } },
                {0x02, new[] { 2, 3 } },
                {0x1B, new[] { 27, 23 } },
                {0x0C, new[] { 12, 13, 14 } }
            };

            if (state == CreatureState.War)
            {
                int firstGroupId = groupIds[0];
                if (warGroupedActions.TryGetValue(firstGroupId, out var actionIds))
                {
                    for (int i = 0; i < groupIds.Count && i < actionIds.Length; i++)
                    {
                        overrides[actionIds[i]] = new StateOverride { GroupId = groupIds[i], Modifier = modifier };
                    }
                }
                else if (warActionMap.TryGetValue(firstGroupId, out int actionId))
                {
                    overrides[actionId] = new StateOverride { GroupId = firstGroupId, Modifier = modifier };
                }
            }
            else
            {
                foreach (int groupId in groupIds)
                {
                    if (overrides.TryGetValue(groupId, out var existing) && modifier == null)
                    {
                        continue;
                    }
                    overrides[groupId] = new StateOverride { GroupId = groupId, Modifier = modifier };
                }
            }
        }

        private void ReadImplicitTriplet9(CustomBinaryReader reader, uint animationIndex, CreatureState state)
        {
            int[] targetActions = state switch
            {
                CreatureState.War => new[] { 1, 0, 24 },
                CreatureState.Peace => new[] { 25, 22, 24 },
                CreatureState.MountPeace => new[] { 31, 29, 30 },
                CreatureState.MountWar => new[] { 31, 29, 30 },
                _ => new[] { 25, 22, 24 }
            };

            var indexEntry = _dataIndex[animationIndex];
            if (!indexEntry.StateOverrides.ContainsKey(state))
                indexEntry.StateOverrides[state] = new Dictionary<int, StateOverride>();
            var stateDict = indexEntry.StateOverrides[state];

            for (int i = 0; i < 3; i++)
            {
                if (reader.BaseStream.Position + 9 > reader.BaseStream.Length) break;
                byte idx = reader.ReadByte();
                int groupA = reader.ReadInt32LE();
                int groupB = reader.ReadInt32LE();
                stateDict[targetActions[i]] = new StateOverride { GroupId = groupA, Modifier = null };
            }
        }

        private bool SeekLiteral(CustomBinaryReader reader, byte[] pattern, int maxScanBytes)
        {
            long start = reader.BaseStream.Position;
            long end = Math.Min(reader.BaseStream.Length, start + maxScanBytes);
            int plen = pattern.Length;

            for (long pos = start; pos <= end - plen; pos++)
            {
                reader.BaseStream.Position = pos;
                bool match = true;
                for (int i = 0; i < plen; i++)
                {
                    if (reader.BaseStream.ReadByte() != pattern[i]) { match = false; break; }
                }
                if (match)
                {
                    reader.BaseStream.Position = pos + plen;
                    return true;
                }
            }
            reader.BaseStream.Position = start;
            return false;
        }

        public bool IgnoreAnimationSequence { get; set; }

        public bool IsActionReal(int animId, int action)
        {
            if (animId >= _dataIndex.Length) return false;
            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return false;

            return indexAnim.GetUopGroup(action, false) != null;
        }

        public List<int> GetAvailableActions(int animationId)
        {
            var availableActions = new HashSet<int>();
            if (animationId >= _dataIndex.Length)
            {
                return new List<int>();
            }

            IndexDataAnimation indexAnim = _dataIndex[animationId];
            if (indexAnim == null)
            {
                return new List<int>();
            }

            for (int group = 0; group < UopConstants.ANIMATION_UOP_GROUPS_COUNT; group++)
            {
                if (indexAnim.GetUopGroup(group, false) != null)
                {
                    availableActions.Add(group);
                }
            }

            foreach (var state in indexAnim.StateOverrides.Values)
            {
                foreach (var action in state.Keys)
                {
                    availableActions.Add(action);
                }
            }

            foreach (var remapEntry in indexAnim.UopRemap)
            {
                availableActions.Add(remapEntry.Key);
            }

            var result = availableActions.ToList();
            result.Sort();
            return result;
        }

        public List<int> GetAvailableUopGroupIndexes(int animId)
        {
            var availableGroups = new HashSet<int>();
            if (animId >= _dataIndex.Length) return new List<int>();

            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return new List<int>();

            for (int group = 0; group < UopConstants.ANIMATION_UOP_GROUPS_COUNT; group++)
            {
                if (indexAnim.GetUopGroup(group, false) != null)
                {
                    availableGroups.Add(group);
                }
            }

            var result = availableGroups.ToList();
            result.Sort();
            return result;
        }

        public IndexDataFileInfo GetAnimationData(int animId, int action, int direction, CreatureState previewState = CreatureState.Base)
        {
            if (animId >= _dataIndex.Length) return null;
            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return null;

            int groupIndex = -1;

            if (!IgnoreAnimationSequence)
            {
                if (previewState == CreatureState.Base)
                {
                    if (indexAnim.UopRemap.TryGetValue(action, out int remappedAction))
                    {
                        groupIndex = remappedAction;
                    }
                }
                else
                {
                    Dictionary<int, StateOverride> currentOverrides = null;
                    if (previewState == CreatureState.War && indexAnim.StateOverrides.TryGetValue(CreatureState.War, out currentOverrides)) { }
                    else if (previewState == CreatureState.Peace && indexAnim.StateOverrides.TryGetValue(CreatureState.Peace, out currentOverrides)) { }
                    else if (previewState == CreatureState.MountWar && indexAnim.StateOverrides.TryGetValue(CreatureState.MountWar, out currentOverrides)) { }
                    else if (previewState == CreatureState.MountPeace && indexAnim.StateOverrides.TryGetValue(CreatureState.MountPeace, out currentOverrides)) { }

                    if (currentOverrides != null && currentOverrides.ContainsKey(action))
                    {
                        groupIndex = currentOverrides[action].GroupId;
                    }
                    else if (indexAnim.UopRemap.TryGetValue(action, out int remappedAction))
                    {
                        groupIndex = remappedAction;
                    }
                }
            }

            if (groupIndex == -1)
            {
                groupIndex = action;
            }

            if (groupIndex < 0 || groupIndex >= UopConstants.ANIMATION_UOP_GROUPS_COUNT)
            {
                return null;
            }

            IndexDataAnimationGroupUOP group = indexAnim.GetUopGroup(groupIndex, false);
            if (group == null) return null;

            if (direction < group.m_Direction.Length)
            {
                return group.m_Direction[direction];
            }

            return null;
        }

        public void SetAnimationDataForDirection(int animId, int action, int direction, byte[] data)
        {
            if (animId >= _dataIndex.Length) return;
            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return;

            int groupIndex = -1;

            if (!IgnoreAnimationSequence)
            {
                if (indexAnim.StateOverrides.TryGetValue(CreatureState.War, out var warOverrides) && warOverrides.ContainsKey(action))
                {
                    groupIndex = warOverrides[action].GroupId;
                }
                else if (indexAnim.StateOverrides.TryGetValue(CreatureState.Peace, out var peaceOverrides) && peaceOverrides.ContainsKey(action))
                {
                    groupIndex = peaceOverrides[action].GroupId;
                }
                else if (indexAnim.StateOverrides.TryGetValue(CreatureState.MountWar, out var mountWarOverrides) && mountWarOverrides.ContainsKey(action))
                {
                    groupIndex = mountWarOverrides[action].GroupId;
                }
                else if (indexAnim.StateOverrides.TryGetValue(CreatureState.MountPeace, out var mountPeaceOverrides) && mountPeaceOverrides.ContainsKey(action))
                {
                    groupIndex = mountPeaceOverrides[action].GroupId;
                }

                if (groupIndex == -1 && indexAnim.UopRemap.TryGetValue(action, out int remappedAction))
                {
                    groupIndex = remappedAction;
                }
            }

            if (groupIndex == -1)
            {
                groupIndex = action;
            }

            IndexDataAnimationGroupUOP group = indexAnim.GetUopGroup(groupIndex, false);
            if (group == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SetAnimationDataForDirection: Group {groupIndex} not found for animId={animId}, action={action}");
                return;
            }

            if (direction >= 0 && direction < group.m_Direction.Length && group.m_Direction[direction] != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ SetAnimationDataForDirection: animId={animId}, action={action}, groupId={groupIndex}, direction={direction}, data.Length={data.Length}");
                group.m_Direction[direction].SetModifiedData(direction, data);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ SetAnimationDataForDirection: Direction {direction} not found in group {groupIndex} for animId={animId}, action={action}");
            }
        }

        public Dictionary<int, StateOverride> GetStateOverrides(int animId, CreatureState state)
        {
            if (animId >= _dataIndex.Length) return null;
            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return null;

            if (indexAnim.StateOverrides.TryGetValue(state, out var overrides))
            {
                return overrides;
            }

            return null;
        }

        public Dictionary<int, int> GetUopRemapping(int animId)
        {
            if (animId >= _dataIndex.Length) return null;
            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return null;
            return indexAnim.UopRemap;
        }

        public void SetAnimationData(int animId, int action, byte[] data)
        {
            if (animId >= _dataIndex.Length) return;
            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return;

            int groupIndex = -1;

            if (!IgnoreAnimationSequence)
            {
                if (indexAnim.StateOverrides.TryGetValue(CreatureState.War, out var warOverrides) && warOverrides.ContainsKey(action))
                {
                    groupIndex = warOverrides[action].GroupId;
                }
                else if (indexAnim.StateOverrides.TryGetValue(CreatureState.Peace, out var peaceOverrides) && peaceOverrides.ContainsKey(action))
                {
                    groupIndex = peaceOverrides[action].GroupId;
                }
                else if (indexAnim.StateOverrides.TryGetValue(CreatureState.MountWar, out var mountWarOverrides) && mountWarOverrides.ContainsKey(action))
                {
                    groupIndex = mountWarOverrides[action].GroupId;
                }
                else if (indexAnim.StateOverrides.TryGetValue(CreatureState.MountPeace, out var mountPeaceOverrides) && mountPeaceOverrides.ContainsKey(action))
                {
                    groupIndex = mountPeaceOverrides[action].GroupId;
                }

                if (groupIndex == -1 && indexAnim.UopRemap.TryGetValue(action, out int remappedAction))
                {
                    groupIndex = remappedAction;
                }
            }

            if (groupIndex == -1)
            {
                groupIndex = action;
            }

            IndexDataAnimationGroupUOP group = indexAnim.GetUopGroup(groupIndex, false);
            if (group == null) return;

            for (int dir = 0; dir < 5; dir++)
            {
                if (group.m_Direction[dir] != null)
                {
                    group.m_Direction[dir].SetModifiedData(dir, data);
                }
            }
        }

        public UOAnimation GetAnimationExportData(int animId, int groupId)
        {
            if (animId >= _dataIndex.Length) return null;

            IndexDataAnimation indexAnim = _dataIndex[animId];
            if (indexAnim == null) return null;

            IndexDataAnimationGroupUOP group = indexAnim.GetUopGroup(groupId, false);
            if (group == null) return null;

            IndexDataFileInfo fileInfo = group.m_Direction[0];
            if (fileInfo == null) return null;

            byte[] originalBinData = fileInfo.GetData();
            if (originalBinData == null) return null;

            UopAnimationGlobalHeader globalHeader = ReadAmouHeader(originalBinData);
            if (globalHeader == null) return null;

            var frames = new List<FrameEntry>();
            var allPalettes = new List<ColourEntry>();

            int framesPerDirection = (int)globalHeader.FrameCount / 5;
            if (framesPerDirection == 0 && globalHeader.FrameCount > 0)
            {
                framesPerDirection = 1;
            }

            for (int dir = 0; dir < 5; dir++)
            {
                var uopAnim = GetUopAnimation(animId, groupId, dir);

                if (uopAnim == null || uopAnim.Frames.Count == 0)
                {
                    for (int i = 0; i < framesPerDirection; i++)
                    {
                        DecodedUopFrame decodedFrame = LoadFromUopBin(originalBinData, dir, i);
                        if (decodedFrame != null)
                        {
                            var frameData = new UopFrameExportData
                            {
                                Image = new DirectBitmap(decodedFrame.Image),
                                Palette = decodedFrame.Palette.Select(c => new ColourEntry(c)).ToList(),
                                CenterX = decodedFrame.Header.CenterX,
                                CenterY = decodedFrame.Header.CenterY,
                                Width = decodedFrame.Header.Width,
                                Height = decodedFrame.Header.Height,
                                ID = (ushort)animId,
                                Frame = (ushort)i,
                                InitCoordsX = globalHeader.InitCoordsX,
                                InitCoordsY = globalHeader.InitCoordsY,
                                EndCoordsX = globalHeader.EndCoordsX,
                                EndCoordsY = globalHeader.EndCoordsY,
                                DataOffset = 0
                            };
                            frames.Add(new FrameEntry(frameData));
                            allPalettes.AddRange(frameData.Palette);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < uopAnim.Frames.Count; i++)
                    {
                        var cachedFrame = uopAnim.Frames[i];

                        var frameData = new UopFrameExportData
                        {
                            Image = new DirectBitmap(cachedFrame.Image),
                            Palette = cachedFrame.Palette.Select(c => new ColourEntry(c)).ToList(),
                            CenterX = cachedFrame.Header.CenterX,
                            CenterY = cachedFrame.Header.CenterY,
                            Width = cachedFrame.Header.Width,
                            Height = cachedFrame.Header.Height,
                            ID = (ushort)animId,
                            Frame = (ushort)i,
                            InitCoordsX = globalHeader.InitCoordsX,
                            InitCoordsY = globalHeader.InitCoordsY,
                            EndCoordsX = globalHeader.EndCoordsX,
                            EndCoordsY = globalHeader.EndCoordsY,
                            DataOffset = 0
                        };
                        frames.Add(new FrameEntry(frameData));
                        allPalettes.AddRange(frameData.Palette);
                    }
                }
            }

            return new UOAnimation(
                (uint)animId, groupId, globalHeader.CellWidth, globalHeader.CellHeight,
                globalHeader.InitCoordsX, globalHeader.InitCoordsY, globalHeader.EndCoordsX, globalHeader.EndCoordsY,
                frames, allPalettes.Distinct().ToList(), globalHeader.FrameCount
            );
        }

        public static UopAnimationGlobalHeader ReadAmouHeader(byte[] binData)
        {
            if (binData == null || binData.Length < 48) return null;

            using (var stream = new MemoryStream(binData))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadUInt32() != 0x554F4D41) return null;

                reader.ReadUInt32();
                reader.ReadUInt32();

                uint animId = reader.ReadUInt32();

                reader.BaseStream.Seek(16, SeekOrigin.Current);

                uint frameCount = reader.ReadUInt32();
                uint frameIndexOffset = reader.ReadUInt32();

                return new UopAnimationGlobalHeader
                {
                    AnimationId = animId,
                    FrameCount = frameCount
                };
            }
        }

        public static DecodedUopFrame LoadFromUopBin(byte[] binData, int direction, int frameIndex)
        {
            if (binData == null || binData.Length == 0) return null;

            using (var stream = new MemoryStream(binData))
            using (var reader = new BinaryReader(stream))
            {
                var header = ReadUopBinHeader(reader);
                if (header.Magic != 0x554F4D41 || header.FrameCount == 0) return null;

                uint framesPerDirection = header.FrameCount / 5;
                if (framesPerDirection == 0 && header.FrameCount > 0) framesPerDirection = 1;
                uint frameIndexToLoad = (uint)direction * framesPerDirection + (uint)frameIndex;

                var frameIndexEntries = ReadFrameIndex(reader, header);

                if (frameIndexToLoad >= frameIndexEntries.Count) return null;

                UopFrameIndex targetFrameIndex = frameIndexEntries[(int)frameIndexToLoad];
                long frameDataOffset = targetFrameIndex.StreamPosition + targetFrameIndex.FrameDataOffset;
                stream.Seek(frameDataOffset, SeekOrigin.Begin);

                var palette = ReadPalette(reader);
                var frameHeader = ReadFrameHeader(reader);

                if (frameHeader.Width <= 0 || frameHeader.Height <= 0) return null;

                var sortedAllFrames = frameIndexEntries.OrderBy(f => f.StreamPosition + f.FrameDataOffset).ToList();
                int currentOverallIndex = sortedAllFrames.FindIndex(f => f.StreamPosition == targetFrameIndex.StreamPosition && f.FrameDataOffset == targetFrameIndex.FrameDataOffset);

                long endOfFrameData;
                if (currentOverallIndex != -1 && currentOverallIndex + 1 < sortedAllFrames.Count)
                {
                    endOfFrameData = sortedAllFrames[currentOverallIndex + 1].StreamPosition + sortedAllFrames[currentOverallIndex + 1].FrameDataOffset;
                }
                else
                {
                    endOfFrameData = stream.Length;
                }

                var image = DecodeRleFrame(reader, frameHeader, palette, endOfFrameData);
                if (image == null) return null;

                return new DecodedUopFrame { Header = frameHeader, IndexInfo = targetFrameIndex, Palette = palette, Image = image };
            }
        }

        public static UopBinHeader ReadUopBinHeader(BinaryReader reader)
        {
            var header = new UopBinHeader
            {
                Magic = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                TotalSize = reader.ReadUInt32(),
                AnimationId = reader.ReadUInt32(),
            };
            // reader.ReadBytes(16);
            header.BoundLeft = reader.ReadInt16();
            header.BoundTop = reader.ReadInt16();
            header.BoundRight = reader.ReadInt16();
            header.BoundBottom = reader.ReadInt16();
            reader.ReadUInt32(); // Palette count (256)
            reader.ReadUInt32(); // Header size (40)

            header.FrameCount = reader.ReadUInt32();
            header.FrameIndexOffset = reader.ReadUInt32();
            return header;
        }

        public static List<UopFrameIndex> ReadFrameIndex(BinaryReader reader, UopBinHeader header)
        {
            var entries = new List<UopFrameIndex>();
            reader.BaseStream.Seek(header.FrameIndexOffset, SeekOrigin.Begin);

            for (int i = 0; i < header.FrameCount; i++)
            {
                long currentStreamPos = reader.BaseStream.Position;
                var entry = new UopFrameIndex
                {
                    Direction = reader.ReadUInt16(),
                    FrameNumber = reader.ReadUInt16(),
                    StreamPosition = currentStreamPos
                };
                entry.Left = reader.ReadInt16();
                entry.Top = reader.ReadInt16();
                entry.Right = reader.ReadInt16();
                entry.Bottom = reader.ReadInt16();
                entry.FrameDataOffset = reader.ReadUInt32();
                entries.Add(entry);
            }
            return entries;
        }

        public static List<Color> ReadPalette(BinaryReader reader)
        {
            var palette = new List<Color>(256);
            for (int i = 0; i < 256; i++)
            {
                ushort color555 = reader.ReadUInt16();
                color555 ^= 0x8000;

                if ((color555 & 0x8000) == 0 || i == 0)
                {
                    palette.Add(Color.Transparent);
                }
                else
                {
                    byte r = (byte)(((color555 >> 10) & 0x1F) << 3);
                    byte g = (byte)(((color555 >> 5) & 0x1F) << 3);
                    byte b = (byte)((color555 & 0x1F) << 3);
                    palette.Add(Color.FromArgb(255, r, g, b));
                }
            }
            return palette;
        }

        public static UopFrameHeader ReadFrameHeader(BinaryReader reader)
        {
            return new UopFrameHeader
            {
                CenterX = reader.ReadInt16(),
                CenterY = reader.ReadInt16(),
                Width = reader.ReadUInt16(),
                Height = reader.ReadUInt16()
            };
        }

        public static unsafe Bitmap DecodeRleFrame(BinaryReader reader, UopFrameHeader frameHeader, List<Color> palette, long streamEnd)
        {
            if (frameHeader.Width <= 0 || frameHeader.Height <= 0)
            {
                return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }

            Bitmap bmp;
            try
            {
                bmp = new Bitmap(frameHeader.Width, frameHeader.Height, PixelFormat.Format32bppArgb);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ DecodeRleFrame Error: Invalid dimensions {frameHeader.Width}x{frameHeader.Height}. Error: {ex.Message}");
                return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }
            var bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            uint[] pixels = new uint[frameHeader.Width * frameHeader.Height];

            try
            {
                while (reader.BaseStream.Position < streamEnd)
                {
                    if (reader.BaseStream.Position + 4 > streamEnd) break;
                    uint rleHeader = reader.ReadUInt32();
                    if (rleHeader == 0x7FFF7FFF)
                    {
                        break;
                    }

                    int runLength = (int)(rleHeader & 0x0FFF);
                    int y = (int)((rleHeader >> 12) & 0x03FF);
                    int x = (int)((rleHeader >> 22) & 0x03FF);

                    if ((x & 0x200) != 0) x = -(0x400 - x);
                    if ((y & 0x200) != 0) y = -(0x400 - y);

                    if (runLength == 0) continue;
                    if (reader.BaseStream.Position + runLength > streamEnd) break;

                    int finalY = frameHeader.Height - 1 - (-y - frameHeader.CenterY);

                    for (int i = 0; i < runLength; i++)
                    {
                        byte paletteIndex = reader.ReadByte();
                        Color color = palette[paletteIndex];
                        uint pixelColor = (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);

                        int finalX = frameHeader.CenterX + x + i;

                        if (finalX >= 0 && finalX < frameHeader.Width && finalY >= 0 && finalY < frameHeader.Height)
                        {
                            pixels[finalY * frameHeader.Width + finalX] = pixelColor;
                        }
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(Array.ConvertAll(pixels, item => (int)item), 0, bd.Scan0, pixels.Length);
            }
            catch (EndOfStreamException) { }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        public UopAnimIdx GetUopAnimation(int animId, int action, int direction)
        {
            string key = $"{animId}_{action}_{direction}";

            if (_animCache.TryGetValue(key, out var cachedAnim))
            {
                return cachedAnim;
            }

            var fileInfo = GetAnimationData(animId, action, direction, CreatureState.Base);
            if (fileInfo == null) return null;

            byte[] binData = fileInfo.GetData(direction);
            if (binData == null) return null;

            var uopAnim = new UopAnimIdx(animId, action, direction);
            uopAnim.LoadFrames(binData, direction);

            _animCache[key] = uopAnim;

            return uopAnim;
        }

        public void SaveUopAnimationToMemory(UopAnimIdx uopAnim)
        {
            if (!uopAnim.IsModified) return;

            System.Diagnostics.Debug.WriteLine($"💾 SaveUopAnimationToMemory: Saving {uopAnim.AnimId}_{uopAnim.Action}_{uopAnim.Direction}");

            CommitChanges(uopAnim.AnimId, uopAnim.Action);

            uopAnim.IsModified = false;
        }

        public UopAnimIdx CreateNewUopAnimation(int animId, int action, int direction, string targetUopPath = null)
        {
            if (animId < _dataIndex.Length)
            {
                if (_dataIndex[animId] == null) _dataIndex[animId] = new IndexDataAnimation();
                var group = _dataIndex[animId].GetUopGroup(action, true);

                UopFileReader targetFile = null;
                if (!string.IsNullOrEmpty(targetUopPath))
                {
                    targetFile = GetReaderByPath(targetUopPath);
                }
                
                if (targetFile == null && _animationFrameReaders.Count > 0)
                {
                    targetFile = _animationFrameReaders[0];
                }

                var dummyHeader = new UopDataHeader();

                for (int d = 0; d < 5; d++)
                {
                    if (group.m_Direction[d] == null)
                    {
                        group.m_Direction[d] = new IndexDataFileInfo(targetFile, dummyHeader);
                    }
                }
            }

            var uopAnim = new UopAnimIdx(animId, action, direction);
            string key = $"{animId}_{action}_{direction}";
            _animCache[key] = uopAnim;

            return uopAnim;
        }

        public void SaveAnimationSequence(string outputPath)
        {
            var fileDataList = new List<UopFileData>();
            var existingHashes = new HashSet<ulong>();

            System.Diagnostics.Debug.WriteLine($"[SAVE] Adding {SequenceEntries.Count} parsed sequences:");
            foreach (var animId in SequenceEntries.Keys)
            {
                if (!AnimationIdToHash.TryGetValue(animId, out ulong hash))
                {
                    System.Diagnostics.Debug.WriteLine($"[SAVE] ⚠️ Skipping AnimID {animId} (no hash)");
                    continue;
                }

                if (!existingHashes.Add(hash))
                {
                    System.Diagnostics.Debug.WriteLine($"[SAVE] ⚠️ Duplicate hash for AnimID {animId}, skipping: {hash:X16}");
                    continue;
                }

                byte[] data;
                using (var ms = new MemoryStream())
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((uint)animId);
                    writer.Write(new byte[48]);

                    var entries = SequenceEntries[animId];
                    writer.Write((uint)entries.Count);

                    foreach (var entry in entries)
                    {
                        writer.Write(entry.UopGroupIndex);
                        writer.Write(entry.FrameCount);
                        writer.Write(entry.MulGroupIndex);
                        writer.Write(entry.Speed);
                        if (entry.ExtraData != null && entry.ExtraData.Length == 56)
                            writer.Write(entry.ExtraData);
                        else
                            writer.Write(new byte[56]);
                    }

                    if (RawStateBlocks.TryGetValue(animId, out byte[] stateData))
                    {
                        writer.Write(stateData);
                    }
                    else
                    {
                        writer.Write((uint)0);
                        writer.Write((byte)0);
                        writer.Write((byte)0xFF);
                    }

                    data = ms.ToArray();
                }

                fileDataList.Add(new UopFileData
                {
                    Hash = hash,
                    Data = data,
                    DecompressedSize = (uint)data.Length,
                    IsCompressed = true,
                    IsEmpty = false
                });
            }

            System.Diagnostics.Debug.WriteLine($"[SAVE] Adding {_unknownEntries.Count} unknown entries:");
            foreach (var kvp in _unknownEntries)
            {
                if (!existingHashes.Add(kvp.Key))
                {
                    System.Diagnostics.Debug.WriteLine($"[SAVE] ⚠️ Skipping unknown entry (hash already present): {kvp.Key:X16}");
                    continue;
                }

                if (!_unknownMetadata.TryGetValue(kvp.Key, out var meta))
                {
                    // fallback metadata
                    meta = new UopDataHeader
                    {
                        DecompressedSize = (uint)kvp.Value.Length,
                        HeaderSize = 34,
                        Flag = 1
                    };
                }

                bool isAlreadyCompressed = (meta.Flag != 0);

                fileDataList.Add(new UopFileData
                {
                    Hash = kvp.Key,
                    PrecompressedData = isAlreadyCompressed ? kvp.Value : null,
                    Data = isAlreadyCompressed ? null : kvp.Value,
                    DecompressedSize = meta.DecompressedSize,
                    HeaderSize = meta.HeaderSize,
                    IsCompressed = isAlreadyCompressed,
                    IsEmpty = false
                });
            }

            // 3. Re-add any original entries from the source UOP that were neither in SequenceEntries nor in _unknownEntries.
            if (_animationSequenceReader != null)
            {
                var originalEntries = _animationSequenceReader.GetAllEntries();
                foreach (var entry in originalEntries)
                {
                    if (existingHashes.Contains(entry.Key)) continue;

                    var header = entry.Value;
                    byte[] raw = _animationSequenceReader.ReadRawData(header);
                    if (raw == null) continue;

                    // Try to read header bytes if available (some UopFileReader implementations provide this)
                    byte[] headerBytes = null;
                    try
                    {
                        headerBytes = _animationSequenceReader.ReadHeaderData(header);
                    }
                    catch
                    {
                        headerBytes = null;
                    }

                    fileDataList.Add(new UopFileData
                    {
                        Hash = entry.Key,
                        HeaderSize = header.HeaderSize,
                        HeaderBytes = headerBytes,
                        PrecompressedData = raw,
                        DecompressedSize = header.DecompressedSize,
                        IsCompressed = (header.Flag & 1) != 0,
                        IsEmpty = false
                    });

                    existingHashes.Add(entry.Key);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[SAVE] Total entries to save: {fileDataList.Count}");

            fileDataList = fileDataList.OrderBy(e => e.Hash).ToList();
            UopFileWriter.WriteUopFile(outputPath, fileDataList, 1000);
            System.Diagnostics.Debug.WriteLine($"[SAVE] File saved: {new FileInfo(outputPath).Length} bytes");
        }

        public byte[] GetBinaryDataForAnimationId(uint animId)
        {
            if (!SequenceEntries.TryGetValue(animId, out var entries))
            {
                throw new InvalidOperationException($"Animation ID {animId} not found.");
            }

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write((uint)animId);
                writer.Write(new byte[48]);
                writer.Write((uint)entries.Count);

                foreach (var entry in entries)
                {
                    writer.Write(entry.UopGroupIndex);
                    writer.Write(entry.FrameCount);
                    writer.Write(entry.MulGroupIndex);
                    writer.Write(entry.Speed);
                    if (entry.ExtraData != null && entry.ExtraData.Length == 56)
                        writer.Write(entry.ExtraData);
                    else
                        writer.Write(new byte[56]);
                }

                if (RawStateBlocks.TryGetValue(animId, out byte[] stateData))
                {
                    writer.Write(stateData);
                }
                else
                {
                    writer.Write((uint)0);
                    writer.Write((byte)0);
                    writer.Write((byte)0xFF);
                }

                return ms.ToArray();
            }
        }

        public void UpdateSequenceEntry(uint animId, uint uopGroupIndex, int newFrameCount, uint newMulGroupIndex, float newSpeed, byte[] newExtraData, bool autoPopulate = false)
        {
            if (!SequenceEntries.TryGetValue(animId, out var entries))
            {
                return;
            }

            AnimationSequenceEntry entryToUpdate = entries.FirstOrDefault(e => e.UopGroupIndex == uopGroupIndex);

            if (entryToUpdate != null)
            {
                if (autoPopulate)
                {
                    if (newFrameCount > 0)
                    {
                        entryToUpdate.FrameCount = newFrameCount;
                        entryToUpdate.MulGroupIndex = 0xFFFFFFFF;
                        entryToUpdate.Speed = 9.0f;

                        byte[] extra = new byte[56];
                        for (int i = 0; i < 16; i++) extra[i] = 0x80;
                        entryToUpdate.ExtraData = extra;
                    }
                    else
                    {
                        entryToUpdate.FrameCount = 0;
                        entryToUpdate.MulGroupIndex = 4;
                        entryToUpdate.Speed = 0.0f;
                        entryToUpdate.ExtraData = new byte[56];
                    }
                }
                else
                {
                    entryToUpdate.FrameCount = newFrameCount;
                    entryToUpdate.MulGroupIndex = newMulGroupIndex;
                    entryToUpdate.Speed = newSpeed;
                    entryToUpdate.ExtraData = newExtraData;
                }

                if (animId < _dataIndex.Length && _dataIndex[animId] != null)
                {
                    if (entryToUpdate.FrameCount == 0 && uopGroupIndex < UopConstants.ANIMATION_UOP_GROUPS_COUNT && entryToUpdate.MulGroupIndex < UopConstants.ANIMATION_UOP_GROUPS_COUNT)
                    {
                        _dataIndex[animId].UopRemap[(int)uopGroupIndex] = (int)entryToUpdate.MulGroupIndex;
                    }
                    else if (entryToUpdate.FrameCount > 0 && uopGroupIndex < UopConstants.ANIMATION_UOP_GROUPS_COUNT)
                    {
                        _dataIndex[animId].UopRemap.Remove((int)uopGroupIndex);
                        _dataIndex[animId].GetUopGroup((int)uopGroupIndex, true)?.SetFrameCount(entryToUpdate.FrameCount);
                    }
                }
            }
            ClearAnimationCacheForAction((int)animId, (int)uopGroupIndex);
        }

        public void CommitAllChanges()
        {
            var animationsToSave = _animCache.Values.ToList();
            foreach (var anim in animationsToSave)
            {
                if (anim.IsModified)
                {
                    SaveUopAnimationToMemory(anim);
                }
            }
        }

        public void CommitChanges(int animId, int action)
        {
            var allDirectionsFrames = new Dictionary<int, List<DecodedUopFrame>>();

            for (int dir = 0; dir < 5; dir++)
            {
                var uopAnim = GetUopAnimation(animId, action, dir);
                if (uopAnim != null)
                {
                    allDirectionsFrames[dir] = uopAnim.Frames;
                }
            }

            if (allDirectionsFrames.Count > 0)
            {
                byte[] encodedData = VdImportHelper.EncodeActionToAmouBin(animId, action, allDirectionsFrames);
                if (encodedData != null)
                {
                    SetAnimationData(animId, action, encodedData);

                    // Update cached headers to reflect new Bounding Box immediately
                    using (var ms = new MemoryStream(encodedData))
                    using (var reader = new BinaryReader(ms))
                    {
                        var newHeader = ReadUopBinHeader(reader);
                        for (int dir = 0; dir < 5; dir++)
                        {
                            // Get from cache directly to avoid reloading
                            string key = $"{animId}_{action}_{dir}";
                            if (_animCache.TryGetValue(key, out var uopAnim))
                            {
                                uopAnim.Header = newHeader;
                            }
                        }
                    }
                }
            }
        }

        public void ClearCache()
        {
            _animCache.Clear();
        }

        public void ClearAnimationCacheForAction(int animId, int action)
        {
            var keysToRemove = new List<string>();
            foreach (var key in _animCache.Keys)
            {
                var parts = key.Split('_');
                if (parts.Length == 3 && int.TryParse(parts[0], out int cachedAnimId) && int.TryParse(parts[1], out int cachedAction))
                {
                    if (cachedAnimId == animId && cachedAction == action)
                    {
                        keysToRemove.Add(key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _animCache.Remove(key);
                System.Diagnostics.Debug.WriteLine($"✅ Cache cleared for {key}");
            }
        }

        public void CloneAnimationSequenceEntry(uint sourceAnimId, uint newAnimId)
        {
            if (!SequenceEntries.ContainsKey(sourceAnimId))
            {
                throw new InvalidOperationException($"Source Animation ID {sourceAnimId} not found.");
            }

            if (newAnimId < _dataIndex.Length && _dataIndex[newAnimId] == null)
            {
                _dataIndex[newAnimId] = new IndexDataAnimation();
            }

            byte[] sourceBinaryData = GetBinaryDataForAnimationId(sourceAnimId);
            byte[] clonedBinaryData = new byte[sourceBinaryData.Length];
            Array.Copy(sourceBinaryData, clonedBinaryData, sourceBinaryData.Length);

            // ✅ Modifier l'AnimID dans le header (offset 0)
            using (var ms = new MemoryStream(clonedBinaryData))
            using (var writer = new BinaryWriter(ms))
            {
                ms.Seek(0, SeekOrigin.Begin);
                writer.Write((uint)newAnimId);
            }

            // ✅ Patch ciblé : modifier uniquement le mot 16 bits à l'offset 12 si et seulement si
            // il contient l'ID source (lower 16 bits). Cela correspond à votre demande (78 05 -> 49 04).
            const int id16Offset = 12;
            if (clonedBinaryData.Length >= id16Offset + 2)
            {
                ushort found = BitConverter.ToUInt16(clonedBinaryData, id16Offset);
                ushort expected = (ushort)(sourceAnimId & 0xFFFF);
                ushort replaceWith = (ushort)(newAnimId & 0xFFFF);

                if (found == expected)
                {
                    byte[] bytes = BitConverter.GetBytes(replaceWith);
                    clonedBinaryData[id16Offset] = bytes[0];
                    clonedBinaryData[id16Offset + 1] = bytes[1];
                    System.Diagnostics.Debug.WriteLine($"✅ Clone: patched 16-bit ID at offset {id16Offset}: {found:X4} -> {replaceWith:X4}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Clone: not patching offset {id16Offset} because found {found:X4} (expected {expected:X4})");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Clone: buffer too small to patch offset {id16Offset}");
            }

            // ✅ Créer UNIQUEMENT le hash D8 (8 chiffres)
            string standardPath = $"build/animationsequence/{newAnimId:D8}.bin";
            ulong standardHash = UopFileReader.CreateHash(standardPath);

            if (!ParseAnimationSequenceEntry(clonedBinaryData, standardHash))
            {
                throw new InvalidOperationException($"Failed to parse cloned animation sequence for ID {newAnimId}");
            }

            System.Diagnostics.Debug.WriteLine($"✅ Cloned Animation {sourceAnimId} → {newAnimId}");
            System.Diagnostics.Debug.WriteLine($"✅ Created hash (D8): {standardHash:X16} from path: {standardPath}");

            // Réutilisation des métadonnées source si disponibles
            string sourceStandardPath = $"build/animationsequence/{sourceAnimId:D8}.bin";
            ulong sourceStandardHash = UopFileReader.CreateHash(sourceStandardPath);

            UopDataHeader? sourceMetadata = null;
            if (_unknownMetadata.ContainsKey(sourceStandardHash))
            {
                sourceMetadata = _unknownMetadata[sourceStandardHash];
            }

            // Ajouter l'entrée pour le nouvel ID sans toucher aux autres octets
            if (sourceMetadata.HasValue)
            {
                _unknownEntries[standardHash] = clonedBinaryData;
                _unknownMetadata[standardHash] = new UopDataHeader
                {
                    Offset = 0,
                    HeaderSize = sourceMetadata.Value.HeaderSize,
                    CompressedSize = 0,
                    DecompressedSize = (uint)clonedBinaryData.Length,
                    Hash = standardHash,
                    Flag = sourceMetadata.Value.Flag
                };
            }
            else
            {
                _unknownEntries[standardHash] = clonedBinaryData;
                _unknownMetadata[standardHash] = new UopDataHeader
                {
                    Offset = 0,
                    HeaderSize = 34,
                    CompressedSize = 0,
                    DecompressedSize = (uint)clonedBinaryData.Length,
                    Hash = standardHash,
                    Flag = 1
                };
            }

            System.Diagnostics.Debug.WriteLine($"✅ Added D8 entry to _unknownEntries for ID {newAnimId}");
        }

        public void CreateNewAnimationSequenceEntry(uint animId)
        {
            if (SequenceEntries.ContainsKey(animId))
            {
                throw new InvalidOperationException($"Animation ID {animId} already exists in sequence entries.");
            }

            SequenceEntries[animId] = new List<AnimationSequenceEntry>();

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write((uint)0);
                writer.Write((byte)0);
                writer.Write((byte)0xFF);
                RawStateBlocks[animId] = ms.ToArray();
            }

            string entryPath = $"build/animationsequence/{animId:D6}.bin";
            ulong hash = UopFileReader.CreateHash(entryPath);
            AnimationIdToHash[animId] = hash;

            if (animId < _dataIndex.Length && _dataIndex[animId] == null)
            {
                _dataIndex[animId] = new IndexDataAnimation();
            }
            _dataIndex[animId].UopRemap.Clear();
            _dataIndex[animId].StateOverrides.Clear();
        }

        public bool LoadMainMisc() => _mainMiscManager.Load();
        public void SaveMainMisc(string outputPath) => _mainMiscManager.Save(outputPath);
        public bool IsIdInMainMisc(uint id, uint flag = 0x0C000000) => _mainMiscManager.ContainsId(id, flag);
        public void AddIdToMainMisc(uint id, uint flag = 0x0C000000) => _mainMiscManager.AddId(id, flag);
        public bool RemoveIdFromMainMisc(uint id, uint flag = 0x0C000000) => _mainMiscManager.RemoveId(id, flag);
        public void ClearMainMiscTable() => _mainMiscManager.ClearTable();
        public int GetMainMiscEntryCount() => _mainMiscManager.EntryCount;
        public uint GetMainMiscFlag(uint id) => _mainMiscManager.GetIdFlag(id);

        public void EnsureIdInMainMisc(uint id, bool forceCreatureFlag = false)
        {
            if (!_mainMiscManager.IsLoaded) return;

            const uint CREATURE_FLAG = 0x0C000000;
            bool hasCreatureFlag = IsIdInMainMisc(id, CREATURE_FLAG);

            if (!hasCreatureFlag)
            {
                AddIdToMainMisc(id, CREATURE_FLAG);
            }
        }

        public int CheckMissingMainMiscEntries(bool dryRun = false)
        {
            int addedCount = 0;
            if (!_mainMiscManager.IsLoaded)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [MainMisc] Check skipped: Table not loaded.");
                return 0;
            }

            const uint CREATURE_FLAG = 0x0C000000;

            for (int i = 0; i < UopConstants.MAX_ANIMATIONS_DATA_INDEX_COUNT; i++)
            {
                if (IsIdInMainMisc((uint)i, CREATURE_FLAG)) continue;

                var actions = GetAvailableActions(i);
                if (actions.Count == 0) continue;

                bool hasRealUopData = false;
                foreach (int action in actions)
                {
                    if (IsActionReal(i, action))
                    {
                        hasRealUopData = true;
                        break;
                    }
                }

                if (hasRealUopData)
                {
                    if (!dryRun) AddIdToMainMisc((uint)i, CREATURE_FLAG);
                    addedCount++;
                }
            }

            foreach (var key in _animCache.Keys)
            {
                var parts = key.Split('_');
                if (parts.Length >= 3 && uint.TryParse(parts[0], out uint animId))
                {
                    if (!IsIdInMainMisc(animId, CREATURE_FLAG))
                    {
                        if (!dryRun) AddIdToMainMisc(animId, CREATURE_FLAG);
                        addedCount++;
                    }
                }
            }

            return addedCount;
        }
    }
}