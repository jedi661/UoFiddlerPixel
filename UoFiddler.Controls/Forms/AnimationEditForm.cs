/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Models.Uop;
using UoFiddler.Controls.Models.Uop.Imaging;
using UoFiddler.Controls.Uop;
using Models = UoFiddler.Controls.Models;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationEditForm : Form
    {
        private static readonly int[] _animCx = new int[5];
        private static readonly int[] _animCy = new int[5];
        private bool _loaded;
        private int _fileType;
        private int _currentAction;
        private int _currentBody;
        private int _currentDir;
        private Point _framePoint; // Gemini: point for the first animation (original one)
        private Point _additionalFramePoint; // Gemini: separate point for the second animation to allow independent positioning
        private bool _showOnlyValid;
        private static bool _drawEmpty;
        private static bool _drawFull;
        private static bool _drawBoundingBox;
        private static bool _drawCroppingBox;
        private static readonly Color _whiteConvert = Color.FromArgb(255, 255, 255, 255);

        private UopAnimationDataManager _uopManager;
        private bool _useUopMapping = true;

        private const int UOP_FILE_TYPE = 6;

        private static readonly Pen _blackUnDrawTransparent = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
        private static readonly Pen _blackUnDrawOpaque = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        private static Pen _blackUndraw = _blackUnDrawOpaque;

        private static readonly SolidBrush _whiteUnDrawTransparent = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
        private static readonly SolidBrush _whiteUnDrawOpaque = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        private static SolidBrush _whiteUnDraw = _whiteUnDrawOpaque;
        private int _previousBody = -1;
        private int _previousAction = -1;

        // Gemini Added Fields
        private float _zoomFactor = 1.0f;
        private bool _relativeMode = false;
        private int _accumulatedRelativeX = 0;
        private int _accumulatedRelativeY = 0;
        private int _lastRelativeX = 0;
        private int _lastRelativeY = 0;
        private bool _updatingUi = false;

        // Second Animation
        private bool _secondAnimActivated = false;
        private int _secondAnimID = 0;
        private int _secondAnimFileIndex = 0; // 0=anim, 1=anim2...
        private bool _secondAnimPseudoVisu = false;
        private bool _isSecondAnimInFront = false;
        private UopAnimationDataManager _secondUopManager;

        // Sequence Tab – Runtime State        
        private System.ComponentModel.BindingList<SequenceViewModelItem> _sequenceViewModelList; // ViewModel for the DataGridView
        private int _seqPreviewDirection; // Direction for the preview in the Sequence tab
        private int _seqPreviewFrameIndex; // Frame index for the preview in the Sequence tab
        private int _seqCurrentAction = -1; // Current action for the sequence being edited
        private Models.Uop.AnimationSequenceEntry _currentSeqEntry; // Current sequence entry being edited

        private bool isAnimationVisible = false; // Second animation
        private AnimIdx additionalAnimation = null; // Second animation

        private static bool _drawCrosshair = true; // Default = visible

        private HexCompareForm _hexCompare; // Form for hex comparison of frames between two animations

        #region [ Offsets Human ]
        private static readonly int[][][] Offsets = new int[][][]
        {
            new int[][] // Direction 0
            {
                new int[] { -12, -53 }, // Frame 0
                new int[] { -13, -55 }, // Frame 1
                new int[] { -12, -55 }, // Frame 2
                new int[] { -12, -53 }, // Frame 3
                new int[] { -12, -53 }, // Frame 4
                new int[] { -12, -53 }, // Frame 5
                new int[] { -12, -55 }, // Frame 6
                new int[] { -11, -54 }, // Frame 7
                new int[] { -12, -53 }, // Frame 8
                new int[] { -12, -53 }  // Frame 9
            },
            new int[][] // Direction 1
            {
                new int[] { -16, -54 }, // Frame 0
                new int[] { -14, -55 }, // Frame 1
                new int[] { -13, -55 }, // Frame 2
                new int[] { -13, -55 }, // Frame 3
                new int[] { -16, -53 }, // Frame 4
                new int[] { -19, -53 }, // Frame 5
                new int[] { -12, -55 }, // Frame 6
                new int[] { -9, -56 },  // Frame 7
                new int[] { -10, -55 }, // Frame 8
                new int[] { -14, -54 }  // Frame 9
            },
            new int[][] // Direction 2
            {
                new int[] { -22, -54 }, // Frame 0
                new int[] { -14, -56 }, // Frame 1
                new int[] { -11, -57 }, // Frame 2
                new int[] { -13, -55 }, // Frame 3
                new int[] { -18, -55 }, // Frame 4
                new int[] { -22, -54 }, // Frame 5
                new int[] { -13, -56 }, // Frame 6
                new int[] { -14, -56 }, // Frame 7
                new int[] { -15, -56 }, // Frame 8
                new int[] { -16, -55 }  // Frame 9
            },
            new int[][] // Direction 3
            {
                new int[] { -17, -56 }, // Frame 0
                new int[] { -12, -56 }, // Frame 1
                new int[] { -13, -57 }, // Frame 2
                new int[] { -15, -57 }, // Frame 3
                new int[] { -15, -56 }, // Frame 4
                new int[] { -16, -56 }, // Frame 5
                new int[] { -16, -57 }, // Frame 6
                new int[] { -14, -57 }, // Frame 7
                new int[] { -14, -57 }, // Frame 8
                new int[] { -15, -56 }  // Frame 9
            },
            new int[][] // Direction 4
            {
                new int[] { -10, -56 }, // Frame 0
                new int[] { -11, -57 }, // Frame 1
                new int[] { -11, -58 }, // Frame 2
                new int[] { -11, -58 }, // Frame 3
                new int[] { -12, -57 }, // Frame 4
                new int[] { -12, -57 }, // Frame 5
                new int[] { -12, -58 }, // Frame 6
                new int[] { -12, -58 }, // Frame 7
                new int[] { -11, -57 }, // Frame 8
                new int[] { -10, -57 }  // Frame 9
            }
        };
        #endregion

        #region [ HorseRunOffsets ]
        private static readonly int[][][] HorseRunOffsets = new int[][][]
        {
            new int[][] // Direction 0
            {
                new int[] { -14, -73 }, // Frame 0
                new int[] { -16, -74 }, // Frame 1
                new int[] { -17, -69 }, // Frame 2
                new int[] { -16, -73 }, // Frame 3
                new int[] { -16, -74 }, // Frame 4                
            },
            new int[][] // Direction 1
            {
                new int[] { -18, -74 }, // Frame 0
                new int[] { -20, -74 }, // Frame 1
                new int[] { -21, -69 }, // Frame 2
                new int[] { -20, -73 }, // Frame 3
                new int[] { -18, -75 }, // Frame 4                
            },
            new int[][] // Direction 2
            {
                new int[] { -14, -75 }, // Frame 0
                new int[] { -15, -76 }, // Frame 1
                new int[] { -15, -71 }, // Frame 2
                new int[] { -15, -75 }, // Frame 3
                new int[] { -14, -76 }, // Frame 4                
            },
            new int[][] // Direction 3
            {
                new int[] { -18, -76 }, // Frame 0
                new int[] { -19, -77 }, // Frame 1
                new int[] { -20, -72 }, // Frame 2
                new int[] { -19, -76 }, // Frame 3
                new int[] { -19, -76 }, // Frame 4                
            },
            new int[][] // Direction 4
            {
                new int[] { -13, -76 }, // Frame 0
                new int[] { -14, -77 }, // Frame 1
                new int[] { -15, -73 }, // Frame 2
                new int[] { -14, -76 }, // Frame 3
                new int[] { -14, -77 }, // Frame 4                
            }
        };
        #endregion

        public AnimationEditForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            FramesListView.MultiSelect = true;
            FramesListView.ItemSelectionChanged += FramesListView_ItemSelectionChanged;

            _fileType = 0;
            _currentDir = 0;
            _framePoint = new Point(AnimationPictureBox.Width / 2, AnimationPictureBox.Height / 2);
            _showOnlyValid = false;
            _loaded = false;

            InitializeSequenceTab();
        }

        private readonly string[][] _animNames =
        {
            new string[]
            {
                "Walk",
                "Run",
                "Idle",
                "Eat",
                "Alert",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1",
                "Idle",
                "Fidget",
                "LieDown",
                "Die2"
            }, //animal
            new string[]
            {
                "Walk",
                "Idle",
                "Die1",
                "Die2",
                "Attack1",
                "Attack2",
                "Attack3",
                "AttackBow",
                "AttackCrossBow",
                "AttackThrow",
                "GetHit",
                "Pillage",
                "Stomp",
                "Cast2",
                "Cast3",
                "BlockRight",
                "BlockLeft",
                "Idle",
                "Fidget",
                "Fly",
                "TakeOff",
                "GetHitInAir"
            }, //Monster
            new string[]
            {
                "Walk_01",
                "WalkStaff_01",
                "Run_01",
                "RunStaff_01",
                "Idle_01",
                "Idle_01",
                "Fidget_Yawn_Stretch_01",
                "CombatIdle1H_01",
                "CombatIdle1H_01",
                "AttackSlash1H_01",
                "AttackPierce1H_01",
                "AttackBash1H_01",
                "AttackBash2H_01",
                "AttackSlash2H_01",
                "AttackPierce2H_01",
                "CombatAdvance_1H_01",
                "Spell1",
                "Spell2",
                "AttackBow_01",
                "AttackCrossbow_01",
                "GetHit_Fr_Hi_01",
                "Die_Hard_Fwd_01",
                "Die_Hard_Back_01",
                "Horse_Walk_01",
                "Horse_Run_01",
                "Horse_Idle_01",
                "Horse_Attack1H_SlashRight_01",
                "Horse_AttackBow_01",
                "Horse_AttackCrossbow_01",
                "Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01",
                "Punch_Punch_Jab_01",
                "Bow_Lesser_01",
                "Salute_Armed1h_01",
                "Ingest_Eat_01"
            }, //human
            new string[]
            {
                "Walk", "Run", "Idle", "Eat", "Alert", "Attack1", "Attack2", "GetHit", "Die1", "Idle",
                "Fidget", "LieDown", "Die2", "Attack3", "AttackBow", "AttackCrossBow", "AttackThrow",
                "Pillage", "Stomp", "Cast2", "Cast3", "BlockRight", "BlockLeft", "Fly", "TakeOff", "GetHitInAir"
            } // UOP Creature (Extended)
        };

        private void LoadMulAnimations()
        {
            AnimationListTreeView.BeginUpdate();
            try
            {
                AnimationListTreeView.Nodes.Clear();
                _uopManager = null;

                int count = Animations.GetAnimCount(_fileType);
                TreeNode[] nodes = new TreeNode[count];
                int animationCount = 0;

                for (int i = 0; i < count; ++i)
                {
                    int animLength = Animations.GetAnimLength(i, _fileType);
                    string type = animLength == 22 ? "H" : animLength == 13 ? "L" : "P";
                    TreeNode node = new TreeNode
                    {
                        Tag = i,
                        Text = $"{type}: {i} ({BodyConverter.GetTrueBody(_fileType, i)})"
                    };

                    bool valid = false;
                    for (int j = 0; j < animLength; ++j)
                    {
                        TreeNode treeNode = new TreeNode
                        {
                            Tag = j,
                            Text = string.Format("{0:D2} {1}", j,
                                _animNames[animLength == 22 ? 1 : animLength == 13 ? 0 : 2][j])
                        };

                        if (AnimationEdit.IsActionDefined(_fileType, i, j))
                        {
                            valid = true;
                        }
                        else
                        {
                            treeNode.ForeColor = Color.Red;
                        }

                        node.Nodes.Add(treeNode);
                    }

                    if (valid)
                    {
                        animationCount++;
                    }
                    else
                    {
                        if (_showOnlyValid)
                        {
                            continue;
                        }
                        // ✅ checkBoxIDBlue berücksichtigen
                        node.ForeColor = checkBoxIDBlue.Checked ? Color.Blue : Color.Red;
                    }

                    nodes[i] = node;
                }

                AnimationListTreeView.Nodes.AddRange(nodes.Where(n => n != null).ToArray());

                // ✅ Status-Label aktualisieren
                toolStripStatusDisplayLabelAnimation.Text =
                    $"Number of animations: {animationCount}";
            }
            finally
            {
                AnimationListTreeView.EndUpdate();
            }

            if (AnimationListTreeView.Nodes.Count > 0)
            {
                AnimationListTreeView.SelectedNode = AnimationListTreeView.Nodes[0];
            }
        }

        #region [ OnLoad ]
        private void OnLoad(object sender, EventArgs e)
        {
            if (_loaded)
            {
                return;
            }
            _loaded = true;

            Options.LoadedUltimaClass["AnimationEdit"] = true;
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;

            // ── File selection for main animation 
            SelectFileToolStripComboBox.Items.Clear();
            SelectFileToolStripComboBox.Items.Add("Choose anim file");

            string root = Files.RootDir;
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
            {
                var mulFiles = new List<string>();
                for (int i = 1; i <= 5; ++i)
                {
                    string fileName = (i == 1) ? "anim.mul" : $"anim{i}.mul";
                    if (!string.IsNullOrEmpty(Files.GetFilePath(fileName)))
                    {
                        mulFiles.Add(Path.GetFileNameWithoutExtension(fileName));
                    }
                }
                mulFiles.Sort();
                SelectFileToolStripComboBox.Items.AddRange(mulFiles.ToArray());

                var uopFiles = Directory.GetFiles(root, "AnimationFrame*.uop")
                                       .Select(Path.GetFileName)
                                       .ToList();
                uopFiles.Sort();
                SelectFileToolStripComboBox.Items.AddRange(uopFiles.ToArray());
            }
            SelectFileToolStripComboBox.SelectedIndex = 0;

            // ── Zoom 
            ZoomNumericUpDown.Items.Clear();
            ZoomNumericUpDown.Items.AddRange(new object[] { "25%", "50%", "100%", "200%", "300%", "400%", "500%" });
            ZoomNumericUpDown.SelectedIndex = ZoomNumericUpDown.Items.IndexOf("100%");

            // ── Second animation file 
            SecondAnimFileComboBox.Items.Clear();
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
            {
                var mulFiles = new List<string>();
                for (int i = 1; i <= 5; ++i)
                {
                    string fileName = (i == 1) ? "anim.mul" : $"anim{i}.mul";
                    if (!string.IsNullOrEmpty(Files.GetFilePath(fileName)))
                    {
                        mulFiles.Add(Path.GetFileNameWithoutExtension(fileName));
                    }
                }
                mulFiles.Sort();
                SecondAnimFileComboBox.Items.AddRange(mulFiles.ToArray());

                var uopFiles = Directory.GetFiles(root, "AnimationFrame*.uop")
                                       .Select(Path.GetFileName)
                                       .ToList();
                uopFiles.Sort();
                SecondAnimFileComboBox.Items.AddRange(uopFiles.ToArray());
            }
            if (SecondAnimFileComboBox.Items.Count > 0)
            {
                SecondAnimFileComboBox.SelectedIndex = 0;
            }

            // ── UOP Mapping Checkbox
            /*var toolStrip = SelectFileToolStripComboBox.GetCurrentParent();
            if (toolStrip != null)
            {
                var mappingCheckBox = new ToolStripButton("Map UOP")
                {
                    CheckOnClick = true,
                    Checked = _useUopMapping
                };
                mappingCheckBox.Click += (s, args) =>
                {
                    _useUopMapping = mappingCheckBox.Checked;
                    if (_uopManager != null)
                    {
                        _uopManager.IgnoreAnimationSequence = !_useUopMapping;
                        LoadUopAnimations();
                    }
                };
                toolStrip.Items.Insert(toolStrip.Items.IndexOf(SelectFileToolStripComboBox) + 1, mappingCheckBox);
            }*/

            SetupExportVdMenu();

            // ── TreeView logic
            AnimationListTreeView.BeginUpdate();
            try
            {
                AnimationListTreeView.Nodes.Clear();

                if (_fileType != 0)
                {
                    int count = Animations.GetAnimCount(_fileType);
                    var nodes = new TreeNode[count];
                    int animationCount = 0;

                    for (int i = 0; i < count; ++i)
                    {
                        int animLength = Animations.GetAnimLength(i, _fileType);
                        string type = animLength switch
                        {
                            22 => "H",
                            13 => "L",
                            _ => "P"
                        };

                        var node = new TreeNode
                        {
                            Tag = i,
                            Text = $"{type}: {i} ({BodyConverter.GetTrueBody(_fileType, i)})"
                        };

                        bool valid = false;

                        for (int j = 0; j < animLength; ++j)
                        {
                            var treeNode = new TreeNode
                            {
                                Tag = j,
                                Text = string.Format("{0:D2} {1}", j,
                                    _animNames[animLength == 22 ? 1 : animLength == 13 ? 0 : 2][j])
                            };

                            if (AnimationEdit.IsActionDefined(_fileType, i, j))
                            {
                                valid = true;
                            }
                            else
                            {
                                treeNode.ForeColor = Color.Red;
                            }

                            node.Nodes.Add(treeNode);
                        }

                        if (valid)
                        {
                            animationCount++;
                        }
                        else
                        {
                            if (_showOnlyValid)
                            {
                                continue;
                            }

                            // Color logic for invalid animations
                            node.ForeColor = checkBoxIDBlue.Checked ? Color.Blue : Color.Red;
                        }

                        nodes[i] = node;
                    }

                    AnimationListTreeView.Nodes.AddRange(nodes.Where(n => n != null).ToArray());

                    toolStripStatusDisplayLabelAnimation.Text = $"Number of animations: {animationCount}"; // Update the label with the number of animations
                }
            }
            finally
            {
                AnimationListTreeView.EndUpdate();
            }

            if (AnimationListTreeView.Nodes.Count > 0)
            {
                AnimationListTreeView.SelectedNode = AnimationListTreeView.Nodes[0];
            }
        }
        #endregion

        private void SetupExportVdMenu()
        {
            // The search revealed the name is tovdToolStripMenuItem
            // This field is defined in the Designer.cs file, but is accessible here.
            if (this.tovdToolStripMenuItem != null)
            {
                tovdToolStripMenuItem.Click -= OnClickExportToVD;
                tovdToolStripMenuItem.DropDownItems.Clear();
                tovdToolStripMenuItem.Text = "To vd..";

                var animalMul = new ToolStripMenuItem("Animal (mul)");
                animalMul.Tag = "animal_mul";
                animalMul.Click += OnClickExportVdRemap;

                var monsterMul = new ToolStripMenuItem("MONSTER (mul)");
                monsterMul.Tag = "monster_mul";
                monsterMul.Click += OnClickExportVdRemap;

                var seaMonsterMul = new ToolStripMenuItem("SEA-MONSTER (mul)");
                seaMonsterMul.Tag = "sea_monster_mul";
                seaMonsterMul.Click += OnClickExportVdRemap;

                var equipmentMul = new ToolStripMenuItem("EQUIPMENT (mul)");
                equipmentMul.Tag = "equipment_mul";
                equipmentMul.Click += OnClickExportVdRemap;

                var creaturesUop = new ToolStripMenuItem("CREATURES (UOP)");
                creaturesUop.Tag = "creatures_uop";
                creaturesUop.Click += OnClickExportVdRemap;

                var equipmentUop = new ToolStripMenuItem("EQUIPEMENT (UOP)");
                equipmentUop.Tag = "equipement_uop";
                equipmentUop.Click += OnClickExportVdRemap;

                tovdToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                    animalMul,
                    monsterMul,
                    seaMonsterMul,
                    equipmentMul,
                    new ToolStripSeparator(),
                    creaturesUop,
                    equipmentUop
                });

                // Add "To .bin" option
                var toBinToolStripMenuItem = new ToolStripMenuItem("To .bin");
                toBinToolStripMenuItem.Click += OnClickExportToBin;
                exportToolStripMenuItem1.DropDownItems.Add(toBinToolStripMenuItem);
            }
        }

        private void OnFilePathChangeEvent()
        {
            if (!_loaded)
            {
                return;
            }

            _fileType = 0;
            _currentDir = 0;
            _currentAction = 0;
            _currentBody = 0;
            SelectFileToolStripComboBox.SelectedIndex = 0;
            _framePoint = new Point(AnimationPictureBox.Width / 2, AnimationPictureBox.Height / 2);
            _showOnlyValid = false;
            ShowOnlyValidToolStripMenuItem.Checked = false;

            // Clear UOP related data on file path change
            _uopManager = null;

            OnLoad(null);
        }

        private TreeNode GetNode(int tag)
        {
            foreach (TreeNode node in AnimationListTreeView.Nodes)
            {
                if (node.Tag is int val && val == tag) return node;
                if (node.Tag is ushort uval && uval == tag) return node;
            }
            return null;
        }

        private unsafe void SetPaletteBox()
        {
            if (_fileType == 0)
            {
                return;
            }

            const int bitmapWidth = 256;
            int bitmapHeight = PalettePictureBox.Height;

            if (_fileType == 6)
            {
                if (FramesListView.SelectedItems.Count == 0)
                {
                    PalettePictureBox.Image = null;
                    return;
                }
                int frameIndex = (int)FramesListView.SelectedItems[0].Tag;
                var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                if (uopAnim == null || frameIndex < 0 || frameIndex >= uopAnim.Frames.Count)
                {
                    PalettePictureBox.Image = null;
                    return;
                }
                var frame = uopAnim.Frames[frameIndex];
                if (frame == null)
                {
                    PalettePictureBox.Image = null;
                    return;
                }
                Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < bd.Height; ++y, line += delta)
                {
                    ushort* cur = line;
                    for (int i = 0; i < bitmapWidth; ++i)
                    {
                        Color c = frame.Palette[i];
                        *cur++ = (ushort)((1 << 15) | ((c.R & 0xF8) << 7) | ((c.G & 0xF8) << 2) | (c.B >> 3));
                    }
                }
                bmp.UnlockBits(bd);
                PalettePictureBox.Image?.Dispose();
                PalettePictureBox.Image = bmp;
                return;
            }

            // TODO: why is bitmapWidth constant and height is taken from picturebox?
            // TODO: looks like the value is the same as array size for pallete in AnimIdx
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            Bitmap bmp1 = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format16bppArgb1555);
            if (edit != null)
            {
                BitmapData bd = bmp1.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < bd.Height; ++y, line += delta)
                {
                    ushort* cur = line;
                    for (int i = 0; i < bitmapWidth; ++i)
                    {
                        *cur++ = edit.Palette[i];
                    }
                }

                bmp1.UnlockBits(bd);
            }

            PalettePictureBox.Image?.Dispose();
            PalettePictureBox.Image = bmp1;
        }

        private void AfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            if (AnimationListTreeView.SelectedNode == null)
            {
                return;
            }

            // ✅ GÉRER LES ANIMATIONS UOP
            if (_fileType == 6)
            {
                // Déterminer le NOUVEAU body/action AVANT de sauvegarder
                int newBody = _currentBody;
                int newAction = _currentAction;

                if (AnimationListTreeView.SelectedNode.Parent == null)
                {
                    if (AnimationListTreeView.SelectedNode.Tag != null)
                        newBody = (int)(ushort)AnimationListTreeView.SelectedNode.Tag;

                    if (AnimationListTreeView.SelectedNode.Nodes.Count > 0)
                    {
                        newAction = (int)AnimationListTreeView.SelectedNode.Nodes[0].Tag;
                    }
                    else
                    {
                        newAction = 0;
                    }
                }
                else
                {
                    if (AnimationListTreeView.SelectedNode.Parent.Tag != null)
                        newBody = (int)(ushort)AnimationListTreeView.SelectedNode.Parent.Tag;
                    newAction = (int)AnimationListTreeView.SelectedNode.Tag;
                }

                // ✅ SAUVEGARDER les modifications de l'ancien body/action AVANT de changer
                bool hasChanged = (_previousBody != -1 && (_previousBody != newBody || _previousAction != newAction));

                if (hasChanged && FramesListView.Items.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"💾 Saving previous animation: Body={_previousBody}, Action={_previousAction}, Direction={_currentDir}");
                    UpdateUopData(applyToAllFrames: true);
                }

                // Mettre à jour les valeurs actuelles
                _currentBody = newBody;
                _currentAction = newAction;

                PopulateSequenceGrid(_currentBody);

                // Enregistrer pour la prochaine fois
                _previousBody = _currentBody;
                _previousAction = _currentAction;

                DisplayUopAnimation();
                NotifyHexEditor();
                return; // ← SORTIR ICI pour les animations UOP
            }

            // ✅ GÉRER LES ANIMATIONS MUL (le reste du code existant)
            if (AnimationListTreeView.SelectedNode.Parent == null)
            {
                if (AnimationListTreeView.SelectedNode.Tag != null)
                {
                    _currentBody = (int)AnimationListTreeView.SelectedNode.Tag;
                }
                _currentAction = 0;
            }
            else
            {
                if (AnimationListTreeView.SelectedNode.Parent.Tag != null)
                {
                    _currentBody = (int)AnimationListTreeView.SelectedNode.Parent.Tag;
                }
                _currentAction = (int)AnimationListTreeView.SelectedNode.Tag;
            }

            FramesListView.BeginUpdate();
            try
            {
                FramesListView.Clear();
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit != null)
                {
                    int width = 80;
                    int height = 110;
                    Bitmap[] currentBits = edit.GetFrames();
                    if (currentBits != null)
                    {
                        for (int i = 0; i < currentBits.Length; ++i)
                        {
                            if (currentBits[i] == null)
                            {
                                continue;
                            }

                            ListViewItem item = new ListViewItem(i.ToString(), 0)
                            {
                                Tag = i
                            };
                            FramesListView.Items.Add(item);

                            if (currentBits[i].Width > width)
                            {
                                width = currentBits[i].Width;
                            }

                            if (currentBits[i].Height > height)
                            {
                                height = currentBits[i].Height;
                            }
                        }
                        FramesListView.TileSize = new Size(width + 5, height + 5);

                        FramesTrackBar.Maximum = currentBits.Length - 1;
                        FramesTrackBar.Value = 0;
                        FramesTrackBar.Invalidate();

                        _updatingUi = true;
                        if (!_relativeMode)
                        {
                            CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                            CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
                        }
                        else
                        {
                            CenterXNumericUpDown.Value = 0;
                            CenterYNumericUpDown.Value = 0;
                            _lastRelativeX = 0;
                            _lastRelativeY = 0;
                            _accumulatedRelativeX = 0;
                            _accumulatedRelativeY = 0;
                        }
                        _updatingUi = false;
                    }
                    else
                    {
                        FramesTrackBar.Maximum = 0;
                        FramesTrackBar.Value = 0;
                        FramesTrackBar.Invalidate();
                    }
                }
            }
            finally
            {
                FramesListView.EndUpdate();
            }

            AnimationPictureBox.Invalidate();
            SetPaletteBox();

            // Sequence Tab für MUL befüllen
            PopulateSequenceGrid(_currentBody);
            NotifyHexEditor();
        }

        private void DisplayUopAnimation()
        {
            if (_uopManager == null) return;

            // ✅ CHANGEMENT CRUCIAL : Utiliser GetUopAnimation au lieu de charger depuis le fichier !
            var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
            if (uopAnim == null)
            {
                FramesListView.Clear();
                return;
            }

            FramesListView.BeginUpdate();
            try
            {
                FramesListView.Clear();

                // ✅ Afficher les frames depuis le cache
                int width = 80;
                int height = 110;
                for (int i = 0; i < uopAnim.Frames.Count; i++)
                {
                    var frame = uopAnim.Frames[i];

                    ListViewItem item = new ListViewItem(i.ToString(), 0) { Tag = i };
                    FramesListView.Items.Add(item);

                    if (frame.Image.Width > width) width = frame.Image.Width;
                    if (frame.Image.Height > height) height = frame.Image.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = uopAnim.Frames.Count > 0 ? uopAnim.Frames.Count - 1 : 0;
                FramesTrackBar.Value = 0;

                if (uopAnim.Frames.Count > 0)
                {
                    _updatingUi = true;
                    if (!_relativeMode)
                    {
                        CenterXNumericUpDown.Value = uopAnim.Frames[0].Header.CenterX;
                        CenterYNumericUpDown.Value = uopAnim.Frames[0].Header.CenterY;
                    }
                    else
                    {
                        CenterXNumericUpDown.Value = 0;
                        CenterYNumericUpDown.Value = 0;
                        _lastRelativeX = 0;
                        _lastRelativeY = 0;
                        _accumulatedRelativeX = 0;
                        _accumulatedRelativeY = 0;
                    }
                    _updatingUi = false;
                    FramesListView.Items[0].Selected = true;
                }
            }
            finally
            {
                FramesListView.EndUpdate();
            }

            AnimationPictureBox.Invalidate();
            SetPaletteBox();
        }

        private void ApplyCenterChange(int? newX, int? newY, int? deltaX, int? deltaY)
        {
            if (_fileType == 0) return;

            int actionStart = _currentAction;
            int actionEnd = _currentAction;
            if (CheckBoxAllAction.Checked)
            {
                actionStart = 0;
                if (_fileType == 6)
                {
                    actionEnd = 100; // Common upper bound for UOP actions
                }
                else
                {
                    actionEnd = Animations.GetAnimLength(_currentBody, _fileType) - 1;
                }
            }

            for (int action = actionStart; action <= actionEnd; action++)
            {
                int dirStart = _currentDir;
                int dirEnd = _currentDir;
                if (CheckBoxAction.Checked || CheckBoxAllAction.Checked)
                {
                    dirStart = 0;
                    dirEnd = 4;
                }

                for (int dir = dirStart; dir <= dirEnd; dir++)
                {
                    if (_fileType == 6 && _uopManager != null)
                    {
                        var uopAnim = _uopManager.GetUopAnimation(_currentBody, action, dir);
                        if (uopAnim != null)
                        {
                            for (int i = 0; i < uopAnim.Frames.Count; i++)
                            {
                                if (!CheckBoxAction.Checked && !CheckBoxAllAction.Checked && i != FramesTrackBar.Value) continue;

                                var frame = uopAnim.Frames[i];
                                int finalX = frame.Header.CenterX;
                                int finalY = frame.Header.CenterY;

                                if (newX.HasValue) finalX = newX.Value;
                                if (newY.HasValue) finalY = newY.Value;
                                if (deltaX.HasValue) finalX += deltaX.Value;
                                if (deltaY.HasValue) finalY += deltaY.Value;

                                uopAnim.ChangeCenter(i, finalX, finalY);
                            }
                        }
                    }
                    else if (_fileType != 6)
                    {
                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, action, dir);
                        if (edit != null && edit.Frames != null)
                        {
                            for (int i = 0; i < edit.Frames.Count; i++)
                            {
                                if (!CheckBoxAction.Checked && !CheckBoxAllAction.Checked && i != FramesTrackBar.Value) continue;

                                var frame = edit.Frames[i];
                                int finalX = frame.Center.X;
                                int finalY = frame.Center.Y;

                                if (newX.HasValue) finalX = newX.Value;
                                if (newY.HasValue) finalY = newY.Value;
                                if (deltaX.HasValue) finalX += deltaX.Value;
                                if (deltaY.HasValue) finalY += deltaY.Value;

                                frame.ChangeCenter(finalX, finalY);
                            }
                        }
                    }
                }
            }
            Options.ChangedUltimaClass["Animations"] = true;
            AnimationPictureBox.Invalidate();
        }

        private void OnCenterXValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi)
            {
                _lastRelativeX = (int)CenterXNumericUpDown.Value;
                return;
            }

            if (_relativeMode)
            {
                int currentValue = (int)CenterXNumericUpDown.Value;
                int delta = currentValue - _lastRelativeX;
                _lastRelativeX = currentValue;

                if (delta == 0) return;

                _accumulatedRelativeX += delta;
                ApplyCenterChange(null, null, delta, null);
            }
            else
            {
                ApplyCenterChange((int)CenterXNumericUpDown.Value, null, null, null);
            }
        }



        private void DrawFrameItem(object sender, DrawListViewItemEventArgs e)
        {
            if (_fileType == 6)
            {
                int index = (int)e.Item.Tag;
                var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                if (uopAnim != null && index >= 0 && index < uopAnim.Frames.Count)
                {
                    var frame = uopAnim.Frames[index];
                    var penColor = FramesListView.SelectedItems.Contains(e.Item) ? Color.Red : Color.Gray;
                    e.Graphics.DrawRectangle(new Pen(penColor), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                    e.Graphics.DrawImage(frame.Image, e.Bounds.X, e.Bounds.Y, frame.Image.Width, frame.Image.Height);
                }
                e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
                return;
            }
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            Bitmap[] currentBits = edit.GetFrames();
            Bitmap bmp = currentBits[(int)e.Item.Tag];
            var penColor1 = FramesListView.SelectedItems.Contains(e.Item) ? Color.Red : Color.Gray;
            e.Graphics.DrawRectangle(new Pen(penColor1), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, bmp.Width, bmp.Height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
        }

        private void FramesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;

            int frameIndex = (int)e.Item.Tag;

            if (_fileType == 6)
            {
                // UOP animation
                var uopAnim = _uopManager?.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                if (uopAnim != null && frameIndex >= 0 && frameIndex < uopAnim.Frames.Count)
                {
                    var frame = uopAnim.Frames[frameIndex];
                    toolStripStatusLabelFrameSize.Text = $"Frame {frameIndex}: {frame.Image.Width} x {frame.Image.Height} px";
                }
                else
                {
                    toolStripStatusLabelFrameSize.Text = string.Empty;
                }
            }
            else if (_fileType != 0)
            {
                // MUL animation
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit != null)
                {
                    Bitmap[] currentBits = edit.GetFrames();
                    if (currentBits != null && frameIndex >= 0 && frameIndex < currentBits.Length && currentBits[frameIndex] != null)
                    {
                        Bitmap bmp = currentBits[frameIndex];
                        toolStripStatusLabelFrameSize.Text = $"Frame {frameIndex}: {bmp.Width} x {bmp.Height} px";
                    }
                    else
                    {
                        toolStripStatusLabelFrameSize.Text = string.Empty;
                    }
                }
            }
            else
            {
                toolStripStatusLabelFrameSize.Text = string.Empty;
            }
        }

        private void OnAnimChanged(object sender, EventArgs e)
        {
            if (SelectFileToolStripComboBox.SelectedIndex < 1)
            {
                _fileType = 0;
                _uopManager = null;
                AnimationListTreeView.Nodes.Clear();
                FramesListView.Clear();
                PalettePictureBox.Image = null;
                return;
            }

            string selection = SelectFileToolStripComboBox.SelectedItem.ToString();
            if (selection.EndsWith(".uop"))
            {
                groupBox1.Visible = false;
                groupBox2.Visible = false;
                groupBox3.Visible = false;

                _fileType = UOP_FILE_TYPE;
                Cursor = Cursors.WaitCursor;
                try
                {
                    _uopManager = new UopAnimationDataManager();
                    if (_uopManager.LoadUopFiles())
                    {
                        _uopManager.ProcessUopData();
                        _uopManager.LoadMainMisc(); // Load MainMisc but don't auto-scan everything
                        RefreshMainMiscButtonState();
                        LoadUopAnimations();
                    }
                    else
                    {
                        MessageBox.Show("No AnimationFrame*.uop files found or loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _uopManager = null;
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            else // It's a .mul file
            {
                groupBox1.Visible = true;
                groupBox2.Visible = true;
                groupBox3.Visible = true;

                _uopManager = null;
                if (selection == "anim")
                {
                    _fileType = 1;
                }
                else
                {
                    if (selection.StartsWith("anim") && !int.TryParse(selection.Substring(4), out _fileType))
                    {
                        _fileType = 0; // fallback
                    }
                }

                if (_fileType > 0)
                {
                    LoadMulAnimations();
                }
            }
        }

        private string GetUopMulMapping(int animId)
        {
            int body = animId;
            bool bodyDefUsed = false;

            if (BodyTable.Entries != null && BodyTable.Entries.TryGetValue(body, out BodyTableEntry entry))
            {
                if (!BodyConverter.Contains(body))
                {
                    body = entry.OldId;
                    bodyDefUsed = true;
                }
            }

            int originalMappedBody = body;
            int fileType = BodyConverter.Convert(ref body);

            string result = "";
            if (bodyDefUsed)
            {
                result += $" (body {originalMappedBody})";
            }

            if (fileType > 1)
            {
                result += $" (anim{fileType} {body})";
            }

            return result;
        }

        private void LoadUopAnimations()
        {
            if (_uopManager == null) return;

            // ❌ ENLEVÉ : Ne plus effacer le cache automatiquement !
            // _uopManager.ClearCache();

            // ✅ Réinitialiser l'état précédent lors du rechargement
            _previousBody = -1;
            _previousAction = -1;

            // Capture current TreeView state
            List<string> expandedPaths = GetExpandedNodePaths(AnimationListTreeView);
            string? selectedPath = GetSelectedNodePath(AnimationListTreeView);

            AnimationListTreeView.BeginUpdate();
            try
            {
                AnimationListTreeView.Nodes.Clear();
                FramesListView.Clear();
                AnimationPictureBox.Invalidate();

                // ✅ NOUVEAU : Scanner d'abord le CACHE pour les animations importées
                HashSet<int> animationsInCache = new HashSet<int>();
                foreach (var cacheKey in _uopManager._animCache.Keys)
                {
                    var parts = cacheKey.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[0], out int animId))
                    {
                        animationsInCache.Add(animId);
                    }
                }

                for (int animId = 0; animId < UopConstants.MAX_ANIMATIONS_DATA_INDEX_COUNT; ++animId)
                {
                    List<int> availableActions = _uopManager.GetAvailableActions(animId);

                    // 1. Check if effectively empty in UOP (no real actions with frames)
                    bool hasRealActions = false;

                    // ✅ CHANGEMENT : Vérifier d'abord le CACHE !
                    if (animationsInCache.Contains(animId))
                    {
                        hasRealActions = true;
                    }
                    else if (availableActions.Count > 0)
                    {
                        foreach (int action in availableActions)
                        {
                            if (_uopManager.IsActionReal(animId, action))
                            {
                                var fileInfo = _uopManager.GetAnimationData(animId, action, 0);
                                if (fileInfo != null)
                                {
                                    byte[] data = fileInfo.GetData();
                                    if (data != null && data.Length > 0)
                                    {
                                        using (var ms = new MemoryStream(data))
                                        using (var r = new BinaryReader(ms))
                                        {
                                            var header = Uop.UopAnimationDataManager.ReadUopBinHeader(r);
                                            if (header != null && header.FrameCount > 0)
                                            {
                                                hasRealActions = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // 2. Check MUL existence (reste identique)
                    int mulBody = animId;
                    if (BodyTable.Entries != null && BodyTable.Entries.TryGetValue(mulBody, out BodyTableEntry entry))
                    {
                        if (!BodyConverter.Contains(mulBody))
                        {
                            mulBody = entry.OldId;
                        }
                    }

                    int fileType = BodyConverter.Convert(ref mulBody);
                    int length = Animations.GetAnimLength(mulBody, fileType);
                    bool existsInMul = false;
                    for (int i = 0; i < length; ++i)
                    {
                        if (AnimationEdit.IsActionDefined(fileType, mulBody, i))
                        {
                            existsInMul = true;
                            break;
                        }
                    }

                    bool isValid = hasRealActions || existsInMul;
                    bool shouldShow = _showOnlyValid ? isValid : (animId <= 4096 || isValid);

                    if (shouldShow)
                    {
                        string mappingInfo = GetUopMulMapping(animId);
                        string nodeText = $"UOP ID: {animId}{mappingInfo}";

                        TreeNode node = new TreeNode
                        {
                            Tag = (ushort)animId,
                            Text = nodeText
                        };

                        if (!hasRealActions && existsInMul)
                        {
                            node.ForeColor = Color.Orange;
                        }
                        else if (!isValid)
                        {
                            node.ForeColor = Color.Red;
                        }

                        foreach (int action in availableActions)
                        {
                            bool isReal = _uopManager.IsActionReal(animId, action);
                            Color nodeColor = Color.Black;

                            if (!isReal) // It's a mapped action
                            {
                                nodeColor = _useUopMapping ? Color.Blue : Color.Red;
                            }

                            string uopFileNumber = "N/A";
                            Uop.IndexDataFileInfo fileInfo = _uopManager.GetAnimationData(animId, action, 0);
                            if (fileInfo != null && fileInfo.File != null && !string.IsNullOrEmpty(fileInfo.File.FilePath))
                            {
                                string fileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.File.FilePath);
                                var match = System.Text.RegularExpressions.Regex.Match(fileName, @"AnimationFrame(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    uopFileNumber = match.Groups[1].Value;
                                }
                            }

                            TreeNode actionNode = new TreeNode
                            {
                                Tag = action,
                                Text = $"{action:D2}_{GetActionDescription(animId, action)} ({uopFileNumber})",
                                ForeColor = nodeColor
                            };
                            node.Nodes.Add(actionNode);
                        }
                        AnimationListTreeView.Nodes.Add(node);
                    }
                }
            }
            finally
            {
                AnimationListTreeView.EndUpdate();
            }

            // Restore TreeView state
            ExpandNodesByPath(AnimationListTreeView, expandedPaths);
            SelectNodeByPath(AnimationListTreeView, selectedPath);

            if (AnimationListTreeView.Nodes.Count > 0 && AnimationListTreeView.SelectedNode == null)
            {
                AnimationListTreeView.SelectedNode = AnimationListTreeView.Nodes[0];
            }
        }

        #region [ OnDirectionChanged ] // Direction change event handler, crucial for UOP animations to save changes before switching and to update the display correctly
        private void OnDirectionChanged(object sender, EventArgs e)
        {
            // UOP: save Changes
            if (_fileType == 6 && FramesListView.Items.Count > 0)
            {
                UpdateUopData(applyToAllFrames: true);
            }

            // Set _currentDir FIRST
            _currentDir = DirectionTrackBar.Value;

            if (checkBoxMount.Checked)
            {
                _currentAction = 24;
            }

            // Second animation BEFORE AfterSelectTreeView — just like in the old version
            if (isAnimationVisible)
            {
                string selectedGender = comboBoxMenWoman.SelectedItem?.ToString() ?? "men";
                int animId = selectedGender == "men" ? 400 : 401;

                additionalAnimation = AnimationEdit.GetAnimation(
                    _fileType, animId, _currentAction, _currentDir);

                AdjustAdditionalAnimationPosition();
                AnimationPictureBox.Invalidate();
            }

            // AfterSelectTreeView LAST — loads main animation, but does not overwrite isAnimationVisible
            AfterSelectTreeView(null, null);
            NotifyHexEditor();
        }
        #endregion

        #region [  AnimationPictureBox_OnSizeChanged ] // Recalculate frame point on size change to keep it centered
        private void AnimationPictureBox_OnSizeChanged(object sender, EventArgs e)
        {
            _framePoint = new Point(AnimationPictureBox.Width / 2, AnimationPictureBox.Height / 2);
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ AdjustAdditionalAnimationPosition ]  
        private void AdjustAdditionalAnimationPosition()
        {
            if (additionalAnimation != null)
            {
                Bitmap[] additionalBits = additionalAnimation.GetFrames();
                if (additionalBits?.Length > 0 && FramesTrackBar.Value >= 0 && FramesTrackBar.Value < additionalBits.Length && additionalBits[FramesTrackBar.Value] != null)
                {
                    int[] offsets;
                    if (checkBoxMount.Checked)
                    {
                        if (_currentDir >= 0 && _currentDir < HorseRunOffsets.Length && FramesTrackBar.Value < HorseRunOffsets[_currentDir].Length)
                        {
                            offsets = HorseRunOffsets[_currentDir][FramesTrackBar.Value];
                        }
                        else
                        {
                            // Log or handle error
                            return;
                        }
                    }
                    else
                    {
                        if (_currentDir >= 0 && _currentDir < Offsets.Length && FramesTrackBar.Value < Offsets[_currentDir].Length)
                        {
                            offsets = Offsets[_currentDir][FramesTrackBar.Value];
                        }
                        else
                        {
                            // Log or handle error
                            return;
                        }
                    }

                    if (offsets.Length >= 2) // Ensure there are at least two elements
                    {
                        int xOffset = offsets[0];
                        int yOffset = offsets[1];

                        int additionalX = _framePoint.X + xOffset;
                        int additionalY = _framePoint.Y + yOffset;

                        _additionalFramePoint = new Point(additionalX, additionalY);
                    }
                    else
                    {
                        // Log or handle error for invalid offsets
                    }
                }
            }
        }
        #endregion


        private void AnimationPictureBox_OnPaintFrame(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            e.Graphics.Clear(Color.LightGray);

            if (_drawCrosshair)
            {
                e.Graphics.DrawLine(Pens.Black, new Point(_framePoint.X, 0), new Point(_framePoint.X, AnimationPictureBox.Height));
                e.Graphics.DrawLine(Pens.Black, new Point(0, _framePoint.Y), new Point(AnimationPictureBox.Width, _framePoint.Y));
            }

            // Men Woman Animation alignment
            if (isAnimationVisible && additionalAnimation != null)
            {
                AdjustAdditionalAnimationPosition(); // Ensure this method is called

                Bitmap[] additionalBits = additionalAnimation.GetFrames();
                if (additionalBits?.Length > 0 && FramesTrackBar.Value >= 0 && FramesTrackBar.Value < additionalBits.Length && additionalBits[FramesTrackBar.Value] != null)
                {
                    // Draw the additional animation at the specified position
                    e.Graphics.DrawImage(additionalBits[FramesTrackBar.Value], _additionalFramePoint.X, _additionalFramePoint.Y);
                }
            }
            // Men Woman Animation alignment

            if (_secondAnimActivated && !_isSecondAnimInFront) DrawSecondAnimation(e.Graphics);

            if (_fileType == 6)
            {
                if (FramesListView.Items.Count == 0 || FramesTrackBar.Value >= FramesListView.Items.Count)
                {
                    if (_secondAnimActivated && _isSecondAnimInFront) DrawSecondAnimation(e.Graphics);
                    return;
                }

                int frameIndex = (int)FramesListView.Items[FramesTrackBar.Value].Tag;
                var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                if (uopAnim == null || frameIndex < 0 || frameIndex >= uopAnim.Frames.Count)
                {
                    if (_secondAnimActivated && _isSecondAnimInFront) DrawSecondAnimation(e.Graphics);
                    return;
                }
                var frame = uopAnim.Frames[frameIndex];
                if (frame == null)
                {
                    if (_secondAnimActivated && _isSecondAnimInFront) DrawSecondAnimation(e.Graphics);
                    return;
                }

                if (_drawEmpty)
                {
                    rawDimensionsToolStripLabel.Text = $" {frame.Header.Width}x{frame.Header.Height}";
                }
                else
                {
                    rawDimensionsToolStripLabel.Text = "";
                }

                int x = _framePoint.X - (int)(frame.Header.CenterX * _zoomFactor);
                int y = _framePoint.Y - (int)(frame.Header.CenterY * _zoomFactor) - (int)(frame.Image.Height * _zoomFactor);
                int w = (int)(frame.Image.Width * _zoomFactor);
                int h = (int)(frame.Image.Height * _zoomFactor);

                if (_drawFull)
                {
                    using (var whiteTransparent = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                    {
                        e.Graphics.FillRectangle(whiteTransparent, new Rectangle(x, y, w, h));
                    }
                }

                e.Graphics.DrawImage(frame.Image, new Rectangle(x, y, w, h));

                if (_drawEmpty)
                {
                    e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, w, h));
                }

                // Bounding box : calculer sur TOUTES les frames de l'action (toutes directions)
                if (_drawBoundingBox)
                {
                    float z = _zoomFactor;

                    // valeurs initiales hors plage
                    int minLeft = int.MaxValue;
                    int maxRight = int.MinValue;
                    int minTop = int.MaxValue;
                    int maxBottom = int.MinValue;
                    bool foundAny = false;

                    // Parcourir les 5 directions et toutes les frames disponibles
                    for (int dir = 0; dir < 5; dir++)
                    {
                        var animDir = _uopManager.GetUopAnimation(_currentBody, _currentAction, dir);
                        if (animDir == null || animDir.Frames == null) continue;

                        foreach (var f in animDir.Frames)
                        {
                            if (f == null || f.Image == null || f.Header == null) continue;

                            int cX = f.Header.CenterX;
                            int cY = f.Header.CenterY;
                            int iw = f.Image.Width;
                            int ih = f.Image.Height;

                            // coordonnées relatives au pivot : left/top sont négatifs si l'image dépasse à gauche/haut
                            int left = -cX;
                            int right = iw - cX;
                            int top = -cY - ih;
                            int bottom = -cY;

                            if (left < minLeft) minLeft = left;
                            if (right > maxRight) maxRight = right;
                            if (top < minTop) minTop = top;
                            if (bottom > maxBottom) maxBottom = bottom;

                            foundAny = true;
                        }
                    }

                    if (foundAny)
                    {
                        int bbX = _framePoint.X + (int)(minLeft * z);
                        int bbY = _framePoint.Y + (int)(minTop * z);
                        int bbW = (int)((maxRight - minLeft) * z);
                        int bbH = (int)((maxBottom - minTop) * z);

                        using (Pen bluePen = new Pen(Color.Blue, 2))
                        {
                            e.Graphics.DrawRectangle(bluePen, bbX, bbY, bbW, bbH);
                        }
                    }
                    else if (uopAnim.Header != null)
                    {
                        // fallback si aucune frame trouvée : utiliser header existant
                        int bbWidth = (int)((uopAnim.Header.BoundRight - uopAnim.Header.BoundLeft) * z);
                        int bbHeight = (int)((uopAnim.Header.BoundBottom - uopAnim.Header.BoundTop) * z);
                        int bbX = x + (w / 2) - (bbWidth / 2);
                        int bbY = y + (h / 2) - (bbHeight / 2);

                        using (Pen bluePen = new Pen(Color.Blue, 2))
                        {
                            e.Graphics.DrawRectangle(bluePen, bbX, bbY, bbWidth, bbHeight);
                        }
                    }
                }

                // --- remplace le calcul précédent du cropping box X/Y par ce bloc ---
                // --- Cropping box : calcul corrigé pour rester cohérent avec le dessin de l'image ---
                if (_drawCroppingBox && frame.IndexInfo != null)
                {
                    int origLeft = (int)frame.IndexInfo.Left;
                    int origRight = (int)frame.IndexInfo.Right;
                    int origTop = (int)frame.IndexInfo.Top;
                    int origBottom = (int)frame.IndexInfo.Bottom;

                    int cw = (int)((origRight - origLeft) * _zoomFactor);
                    int ch = (int)((origBottom - origTop) * _zoomFactor);

                    // Use direct Pivot-relative coordinates as stored in IndexInfo
                    int cx = _framePoint.X + (int)(origLeft * _zoomFactor);
                    int cy = _framePoint.Y + (int)(origTop * _zoomFactor);

                    using (Pen greenPen = new Pen(Color.Lime, 2))
                    {
                        e.Graphics.DrawRectangle(greenPen, cx, cy, cw, ch);
                    }
                }
            }
            else
            {
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit != null)
                {
                    Bitmap[] currentBits = edit.GetFrames();

                    if (_drawEmpty && currentBits?.Length > 0 && currentBits[FramesTrackBar.Value] != null)
                    {
                        rawDimensionsToolStripLabel.Text = $" {currentBits[FramesTrackBar.Value].Width}x{currentBits[FramesTrackBar.Value].Height}";
                    }
                    else
                    {
                        rawDimensionsToolStripLabel.Text = "";
                    }

                    if (currentBits?.Length > 0 && currentBits[FramesTrackBar.Value] != null)
                    {
                        int varW = _drawEmpty ? currentBits[FramesTrackBar.Value].Width : 0;
                        int varH = _drawEmpty ? currentBits[FramesTrackBar.Value].Height : 0;
                        int varFw = _drawFull ? currentBits[FramesTrackBar.Value].Width : 0;
                        int varFh = _drawFull ? currentBits[FramesTrackBar.Value].Height : 0;

                        int x = _framePoint.X - (int)(edit.Frames[FramesTrackBar.Value].Center.X * _zoomFactor);
                        int y = _framePoint.Y - (int)(edit.Frames[FramesTrackBar.Value].Center.Y * _zoomFactor) - (int)(currentBits[FramesTrackBar.Value].Height * _zoomFactor);

                        int scaledW = (int)(currentBits[FramesTrackBar.Value].Width * _zoomFactor);
                        int scaledH = (int)(currentBits[FramesTrackBar.Value].Height * _zoomFactor);
                        int scaledFw = (int)(varFw * _zoomFactor);
                        int scaledFh = (int)(varFh * _zoomFactor);

                        using (var whiteTransparent = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                        {
                            if (_drawFull) e.Graphics.FillRectangle(whiteTransparent, new Rectangle(x, y, scaledFw, scaledFh));
                        }

                        if (_drawEmpty) e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, scaledW, scaledH));
                        e.Graphics.DrawImage(currentBits[FramesTrackBar.Value], new Rectangle(x, y, scaledW, scaledH));
                    }
                }
            }

            if (_secondAnimActivated && _isSecondAnimInFront) DrawSecondAnimation(e.Graphics);

            // Draw Reference Point Arrow
            int refX = (int)(RefXNumericUpDown.Value * (decimal)_zoomFactor);
            int refY = (int)(RefYNumericUpDown.Value * (decimal)_zoomFactor);
            Point[] arrayPoints = {
                new Point(_framePoint.X - refX, _framePoint.Y - refY),
                new Point(_framePoint.X - refX, _framePoint.Y + (int)(17 * _zoomFactor) - refY),
                new Point(_framePoint.X + (int)(4 * _zoomFactor) - refX, _framePoint.Y + (int)(13 * _zoomFactor) - refY),
                new Point(_framePoint.X + (int)(7 * _zoomFactor) - refX, _framePoint.Y + (int)(18 * _zoomFactor) - refY),
                new Point(_framePoint.X + (int)(9 * _zoomFactor) - refX, _framePoint.Y + (int)(17 * _zoomFactor) - refY),
                new Point(_framePoint.X + (int)(7 * _zoomFactor) - refX, _framePoint.Y + (int)(12 * _zoomFactor) - refY),
                new Point(_framePoint.X + (int)(12 * _zoomFactor) - refX, _framePoint.Y + (int)(12 * _zoomFactor) - refY)
            };

            e.Graphics.FillPolygon(_whiteUnDraw, arrayPoints);
            e.Graphics.DrawPolygon(_blackUndraw, arrayPoints);

            // Draw additional animation on top if checkbox is checked
            if (checkBoxMount.Checked && isAnimationVisible && additionalAnimation != null)
            {
                AdjustAdditionalAnimationPosition();

                Bitmap[] additionalBits = additionalAnimation.GetFrames();
                if (additionalBits?.Length > 0 && FramesTrackBar.Value >= 0 && FramesTrackBar.Value < additionalBits.Length && additionalBits[FramesTrackBar.Value] != null)
                {
                    e.Graphics.DrawImage(additionalBits[FramesTrackBar.Value], _additionalFramePoint.X, _additionalFramePoint.Y);
                }
            }
        }
        //End of Soulblighter Modification

        //Soulblighter Modification
        private void OnFrameCountBarChanged(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }
            if (_fileType == 6)
            {
                if (FramesTrackBar.Value < FramesListView.Items.Count)
                {
                    FramesListView.SelectedItems.Clear();
                    var item = FramesListView.Items[FramesTrackBar.Value];
                    item.Selected = true;
                    item.EnsureVisible();

                    int frameIndex = (int)item.Tag;
                    var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                    if (uopAnim != null && frameIndex >= 0 && frameIndex < uopAnim.Frames.Count)
                    {
                        var frame = uopAnim.Frames[frameIndex];
                        _updatingUi = true;
                        if (!_relativeMode)
                        {
                            CenterXNumericUpDown.Value = frame.Header.CenterX;
                            CenterYNumericUpDown.Value = frame.Header.CenterY;
                        }
                        else
                        {
                            CenterXNumericUpDown.Value = 0;
                            CenterYNumericUpDown.Value = 0;
                            _lastRelativeX = 0;
                            _lastRelativeY = 0;
                            _accumulatedRelativeX = 0;
                            _accumulatedRelativeY = 0;
                        }
                        _updatingUi = false;
                    }
                }
                AnimationPictureBox.Invalidate();
                return;
            }

            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit != null && edit.Frames.Count > FramesTrackBar.Value)
            {
                _updatingUi = true;
                if (!_relativeMode)
                {
                    CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                    CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
                }
                else
                {
                    CenterXNumericUpDown.Value = 0;
                    CenterYNumericUpDown.Value = 0;
                    _lastRelativeX = 0;
                    _lastRelativeY = 0;
                    _accumulatedRelativeX = 0;
                    _accumulatedRelativeY = 0;
                }
                _updatingUi = false;
            }

            AnimationPictureBox.Invalidate();
            NotifyHexEditor();
        }
        //End of Soulblighter Modification


        private void OnCenterYValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi)
            {
                _lastRelativeY = (int)CenterYNumericUpDown.Value;
                return;
            }

            try
            {
                if (_fileType == 0) return;

                if (_relativeMode)
                {
                    int currentValue = (int)CenterYNumericUpDown.Value;
                    int delta = currentValue - _lastRelativeY;
                    _lastRelativeY = currentValue;

                    if (delta == 0) return;

                    _accumulatedRelativeY += delta;
                    ApplyCenterChange(null, null, null, delta);
                }
                else
                {
                    ApplyCenterChange(null, (int)CenterYNumericUpDown.Value, null, null);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnClickExtractImages(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            ImageFormat format;

            switch ((string)menu.Tag)
            {
                case ".tiff":
                    format = ImageFormat.Tiff;
                    break;
                case ".png":
                    format = ImageFormat.Png;
                    break;
                case ".jpg":
                    format = ImageFormat.Jpeg;
                    break;
                default:
                    format = ImageFormat.Bmp;
                    break;
            }

            string path = Options.OutputPath;

            int body;
            int action;

            if (AnimationListTreeView.SelectedNode.Parent == null)
            {
                if (AnimationListTreeView.SelectedNode.Tag is ushort ushortBody)
                    body = (int)ushortBody;
                else
                    body = (int)AnimationListTreeView.SelectedNode.Tag;
                action = -1;
            }
            else
            {
                // Correctly cast from UInt16 (ushort) to int
                if (AnimationListTreeView.SelectedNode.Parent.Tag is ushort ushortBody)
                {
                    body = (int)ushortBody;
                }
                else
                {
                    body = (int)AnimationListTreeView.SelectedNode.Parent.Tag;
                }
                action = (int)AnimationListTreeView.SelectedNode.Tag;
            }

            if (_fileType == 6)
            {
                if (_uopManager == null) return;

                List<int> actionsToExport = new List<int>();
                if (action == -1)
                {
                    actionsToExport = _uopManager.GetAvailableActions(body);
                }
                else
                {
                    actionsToExport.Add(action);
                }

                foreach (int a in actionsToExport)
                {
                    for (int dir = 0; dir < 5; dir++)
                    {
                        var uopAnim = _uopManager.GetUopAnimation(body, a, dir);
                        if (uopAnim == null || uopAnim.Frames.Count == 0) continue;

                        for (int f = 0; f < uopAnim.Frames.Count; f++)
                        {
                            var frame = uopAnim.Frames[f];
                            if (frame.Image == null) continue;

                            string filename = $"animUOP_{body}_{a}_{dir}_{f}{menu.Tag}";
                            string file = Path.Combine(path, filename);

                            try
                            {
                                using (Bitmap bit = new Bitmap(frame.Image))
                                {
                                    bit.Save(file, format);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to save image {filename}: {ex.Message}");
                            }
                        }
                    }
                }

                MessageBox.Show($"Images saved to {path}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (action == -1)
            {
                for (int a = 0; a < Animations.GetAnimLength(body, _fileType); ++a)
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, body, a, i);
                        Bitmap[] bits = edit?.GetFrames();
                        if (bits == null)
                        {
                            continue;
                        }

                        for (int j = 0; j < bits.Length; ++j)
                        {
                            if (bits[j] is null)
                            {
                                continue;
                            }

                            string filename = string.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, a, i, j, menu.Tag, _fileType);
                            string file = Path.Combine(path, filename);

                            using (Bitmap bit = new Bitmap(bits[j]))
                            {
                                bit.Save(file, format);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 5; ++i)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, body, action, i);
                    Bitmap[] bits = edit?.GetFrames();
                    if (bits == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < bits.Length; ++j)
                    {
                        if (bits[j] is null)
                        {
                            continue;
                        }

                        string filename = string.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, action, i, j, menu.Tag, _fileType);
                        string file = Path.Combine(path, filename);

                        using (Bitmap bit = new Bitmap(bits[j]))
                        {
                            bit.Save(file, format);
                        }
                    }
                }
            }

            MessageBox.Show($"Frames saved to {path}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickRemoveAction(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            if (_fileType == 6)
            {
                if (_uopManager == null) return;

                if (AnimationListTreeView.SelectedNode.Parent == null)
                {
                    // Remove Body (Animation)
                    DialogResult result = MessageBox.Show($"Are you sure to remove UOP animation {_currentBody}?", "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes) return;

                    AnimationListTreeView.SelectedNode.ForeColor = Color.Red;
                    foreach (TreeNode child in AnimationListTreeView.SelectedNode.Nodes)
                    {
                        child.ForeColor = Color.Red;
                    }

                    // Loop all possible actions for this body: mark modified (do NOT commit now)
                    bool anyModified = false;
                    for (int action = 0; action < 100; action++)
                    {
                        for (int dir = 0; dir < 5; dir++)
                        {
                            var uopAnim = _uopManager.GetUopAnimation(_currentBody, action, dir);
                            if (uopAnim != null)
                            {
                                uopAnim.Frames.Clear();
                                uopAnim.IsModified = true;
                                anyModified = true;
                            }
                        }
                    }

                    if (anyModified)
                    {
                        // Track the AnimID so SaveModifiedAnimationsToUopHybrid will process it later
                        UoFiddler.Controls.Uop.VdImportHelper.MarkAnimIdModified(_currentBody);
                        System.Diagnostics.Debug.WriteLine($"🔖 Removed Body {_currentBody}: flagged for save (deferred commit).");
                    }
                }
                else
                {
                    // Remove Action
                    DialogResult result = MessageBox.Show($"Are you sure to remove UOP action {_currentAction}?", "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes) return;

                    AnimationListTreeView.SelectedNode.ForeColor = Color.Red;

                    bool modified = false;
                    for (int dir = 0; dir < 5; dir++)
                    {
                        var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, dir);
                        if (uopAnim != null)
                        {
                            uopAnim.Frames.Clear();
                            uopAnim.IsModified = true;
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        // Flag for saving later (single Save will persist all flagged IDs)
                        UoFiddler.Controls.Uop.VdImportHelper.MarkAnimIdModified(_currentBody);
                        System.Diagnostics.Debug.WriteLine($"🔖 Removed Action {_currentAction} for Body {_currentBody}: flagged for save (deferred commit).");
                    }
                }

                if (_showOnlyValid && AnimationListTreeView.SelectedNode.ForeColor == Color.Red)
                {
                    if (AnimationListTreeView.SelectedNode.Parent == null)
                        AnimationListTreeView.SelectedNode.Remove();
                    else
                        AnimationListTreeView.SelectedNode.Parent.Remove();
                }

                Options.ChangedUltimaClass["Animations"] = true;
                AfterSelectTreeView(this, null);
                return;
            }

            if (AnimationListTreeView.SelectedNode.Parent == null)
            {
                DialogResult result = MessageBox.Show($"Are you sure to remove animation {_currentBody}", "Remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                AnimationListTreeView.SelectedNode.ForeColor = Color.Red;
                for (int i = 0; i < AnimationListTreeView.SelectedNode.Nodes.Count; ++i)
                {
                    AnimationListTreeView.SelectedNode.Nodes[i].ForeColor = Color.Red;
                    for (int d = 0; d < 5; ++d)
                    {
                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, i, d);
                        edit?.ClearFrames();
                    }
                }

                if (_showOnlyValid)
                {
                    AnimationListTreeView.SelectedNode.Remove();
                }

                Options.ChangedUltimaClass["Animations"] = true;
                AfterSelectTreeView(this, null);
            }
            else
            {
                DialogResult result = MessageBox.Show($"Are you sure to remove action {_currentAction}", "Remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                for (int i = 0; i < 5; ++i)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, i);
                    edit?.ClearFrames();
                }

                AnimationListTreeView.SelectedNode.Parent.Nodes[_currentAction].ForeColor = Color.Red;
                bool valid = false;
                foreach (TreeNode node in AnimationListTreeView.SelectedNode.Parent.Nodes)
                {
                    if (node.ForeColor == Color.Red)
                    {
                        continue;
                    }

                    valid = true;
                    break;
                }

                if (!valid)
                {
                    if (_showOnlyValid)
                    {
                        AnimationListTreeView.SelectedNode.Parent.Remove();
                    }
                    else
                    {
                        AnimationListTreeView.SelectedNode.Parent.ForeColor = Color.Red;
                    }
                }

                Options.ChangedUltimaClass["Animations"] = true;
                AfterSelectTreeView(this, null);
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                SaveUopAnimation();
                return;
            }

            if (_fileType == 0)
            {
                return;
            }

            AnimationEdit.Save(_fileType, Options.OutputPath);
            Options.ChangedUltimaClass["Animations"] = false;

            MessageBox.Show($"AnimationFile saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        //My Soulblighter Modification
        private void OnClickRemoveFrame(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                MessageBox.Show("Unavailable for .UOP format", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (FramesListView.SelectedItems.Count <= 0)
            {
                return;
            }

            int corrector = 0;
            int[] frameIndex = new int[FramesListView.SelectedItems.Count];
            for (int i = 0; i < FramesListView.SelectedItems.Count; i++)
            {
                frameIndex[i] = FramesListView.SelectedIndices[i] - corrector;
                corrector++;
            }

            foreach (var index in frameIndex)
            {
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    continue;
                }

                edit.RemoveFrame(index);
                FramesListView.Items.RemoveAt(FramesListView.Items.Count - 1);
                FramesTrackBar.Maximum = edit.Frames.Count != 0 ? edit.Frames.Count - 1 : 0;
                FramesListView.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
            }
        }
        //End of Soulblighter Modification

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (FramesListView.SelectedItems.Count <= 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                int frameIndex = (int)FramesListView.SelectedItems[0].Tag;

                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to replace at {frameIndex}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (_fileType == 6)
                    {
                        var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                        if (uopAnim != null && frameIndex >= 0 && frameIndex < uopAnim.Frames.Count)
                        {
                            var newFrame = new UoFiddler.Controls.Uop.DecodedUopFrame();
                            newFrame.Image = new Bitmap(bitmap);
                            newFrame.Header = new UoFiddler.Controls.Uop.UopFrameHeader
                            {
                                Width = (ushort)newFrame.Image.Width,
                                Height = (ushort)newFrame.Image.Height,
                                CenterX = (short)(newFrame.Image.Width / 2),
                                CenterY = (short)(newFrame.Image.Height)
                            };

                            var paletteEntries = UoFiddler.Controls.Uop.VdExportHelper.GenerateProperPaletteFromImage(new UoFiddler.Controls.Models.Uop.Imaging.DirectBitmap(newFrame.Image));
                            newFrame.Palette = paletteEntries.Select(p => Color.FromArgb(p.Alpha, p.R, p.G, p.B)).ToList();

                            uopAnim.Frames[frameIndex] = newFrame;
                            uopAnim.IsModified = true;

                            FramesListView.Invalidate();
                            Options.ChangedUltimaClass["Animations"] = true;
                        }
                    }
                    else
                    {
                        if (dialog.FileName.Contains(".bmp") || dialog.FileName.Contains(".tiff") ||
                            dialog.FileName.Contains(".png") || dialog.FileName.Contains(".jpeg") ||
                            dialog.FileName.Contains(".jpg"))
                        {
                            bitmap = ConvertBmpAnim(bitmap, (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);
                        }

                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                        if (edit == null)
                        {
                            return;
                        }

                        edit.ReplaceFrame(bitmap, frameIndex);

                        FramesListView.Invalidate();

                        Options.ChangedUltimaClass["Animations"] = true;
                    }
                }
            }
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                MessageBox.Show("Unavailable for .UOP format", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose image file to add";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif;)|*.gif; |Bitmap files (*.bmp;)|*.bmp; |Tiff files (*.tif;*.tiff)|*.tif;*.tiff; |Png files (*.png;)|*.png; |Jpeg files (*.jpeg;*.jpg;)|*.jpeg;*.jpg;";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        FramesListView.BeginUpdate();
                        try
                        {
                            //My Soulblighter Modifications
                            foreach (var fileName in dialog.FileNames)
                            {
                                using (var bmpTemp = new Bitmap(fileName))
                                {
                                    Bitmap bitmap = new Bitmap(bmpTemp);

                                    if (dialog.FileName.Contains(".bmp") || dialog.FileName.Contains(".tiff") ||
                                        dialog.FileName.Contains(".png") || dialog.FileName.Contains(".jpeg") ||
                                        dialog.FileName.Contains(".jpg"))
                                    {
                                        bitmap = ConvertBmpAnim(bitmap, (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);

                                        //edit.GetImagePalette(bitmap);
                                    }

                                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                                    if (edit == null)
                                    {
                                        continue;
                                    }

                                    //Gif Especial Properties
                                    if (dialog.FileName.Contains(".gif"))
                                    {
                                        FrameDimension dimension = new FrameDimension(bitmap.FrameDimensionsList[0]);

                                        // Number of frames
                                        int frameCount = bitmap.GetFrameCount(dimension);

                                        Bitmap[] bitBmp = new Bitmap[frameCount];

                                        bitmap.SelectActiveFrame(dimension, 0);
                                        UpdateGifPalette(bitmap, edit);

                                        ProgressBar.Maximum = frameCount;

                                        AddImageAtCertainIndex(frameCount, bitBmp, bitmap, dimension, edit);

                                        ProgressBar.Value = 0;
                                        ProgressBar.Invalidate();

                                        SetPaletteBox();

                                        _updatingUi = true;
                                        if (!_relativeMode)
                                        {
                                            CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                                            CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
                                        }
                                        else
                                        {
                                            CenterXNumericUpDown.Value = 0;
                                            CenterYNumericUpDown.Value = 0;
                                        }
                                        _updatingUi = false;

                                        Options.ChangedUltimaClass["Animations"] = true;
                                    }
                                    //End of Soulblighter Modifications
                                    else
                                    {
                                        edit.AddFrame(bitmap);

                                        TreeNode node = GetNode(_currentBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currentAction].ForeColor = Color.Black;
                                        }

                                        int i = edit.Frames.Count - 1;
                                        var item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };

                                        FramesListView.Items.Add(item);

                                        int width = FramesListView.TileSize.Width - 5;
                                        if (bitmap.Width > FramesListView.TileSize.Width)
                                        {
                                            width = bitmap.Width;
                                        }

                                        int height = FramesListView.TileSize.Height - 5;
                                        if (bitmap.Height > FramesListView.TileSize.Height)
                                        {
                                            height = bitmap.Height;
                                        }

                                        FramesListView.TileSize = new Size(width + 5, height + 5);
                                        FramesTrackBar.Maximum = i;

                                        _updatingUi = true;
                                        if (!_relativeMode)
                                        {
                                            CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                                            CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
                                        }
                                        else
                                        {
                                            CenterXNumericUpDown.Value = 0;
                                            CenterYNumericUpDown.Value = 0;
                                        }
                                        _updatingUi = false;

                                        Options.ChangedUltimaClass["Animations"] = true;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            FramesListView.EndUpdate();
                        }

                        FramesListView.Invalidate();
                    }
                }
            }

            // Refresh List
            _currentDir = DirectionTrackBar.Value;
            AfterSelectTreeView(null, null);
        }

        private void AddImageAtCertainIndex(int frameCount, Bitmap[] bitBmp, Bitmap bmp, FrameDimension dimension, AnimIdx edit)
        {
            // Return an Image at a certain index
            for (int index = 0; index < frameCount; index++)
            {
                bmp.SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnim(bmp, (int)numericUpDownRed.Value,
                    (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }
            }
        }

        private void OnClickExtractPalette(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
            {
                return;
            }

            string name = $"palette_anim{_fileType}_{_currentBody}_{_currentAction}_{_currentDir}";
            if ((string)menu.Tag == "txt")
            {
                string path = Path.Combine(Options.OutputPath, name + ".txt");
                edit.ExportPalette(path, 0);
            }
            else
            {
                string path = Path.Combine(Options.OutputPath, name + "." + (string)menu.Tag);
                edit.ExportPalette(path, (string)menu.Tag == "bmp" ? 1 : 2);
            }

            MessageBox.Show($"Palette saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickImportPalette(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose palette file";
                dialog.CheckFileExists = true;
                dialog.Filter = "txt files (*.txt)|*.txt";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    return;
                }

                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    ushort[] palette = new ushort[Animations.PaletteCapacity];

                    int i = 0;
                    while (sr.ReadLine() is { } line)
                    {
                        if ((line = line.Trim()).Length == 0 || line.StartsWith('#'))
                        {
                            continue;
                        }

                        i++;

                        if (i >= Animations.PaletteCapacity)
                        {
                            break;
                        }

                        palette[i] = ushort.Parse(line);

                        // My Soulblighter Modification
                        // Convert color 0,0,0 to 0,0,8
                        // TODO: find out why do we need this replacement
                        if (palette[i] == 32768)
                        {
                            palette[i] = 32769;
                        }
                        // End of Soulblighter Modification
                    }

                    edit.ReplacePalette(palette);
                }

                SetPaletteBox();

                FramesListView.Invalidate();

                Options.ChangedUltimaClass["Animations"] = true;
            }
        }

        private void OnClickImportFromVD(object sender, EventArgs e)
        {
            if (_fileType == UOP_FILE_TYPE && _uopManager != null)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose VD file to import to UOP";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "VD files (*.vd)|*.vd";
                    if (dialog.ShowDialog() != DialogResult.OK)
                        return;

                    try
                    {
                        // ✅ Choisir le fichier UOP cible
                        string targetUopPath = null;
                        using (var fileSelectForm = new UopFileSelectionForm(_uopManager.LoadedUopFiles))
                        {
                            if (fileSelectForm.ShowDialog() == DialogResult.OK)
                            {
                                targetUopPath = fileSelectForm.SelectedPath;
                            }
                            else
                            {
                                return;
                            }
                        }

                        // ✅ Import du VD
                        bool success = VdImportHelper.ImportCreaturesVdToUop(dialog.FileName, _uopManager, _currentBody, targetUopPath);
                        if (!success)
                        {
                            MessageBox.Show("Failed to import VD file.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // ✅ Sauvegarder en mode HYBRIDE
                        string destinationPath = Path.Combine(Options.OutputPath, Path.GetFileName(targetUopPath));
                        if (VdImportHelper.SaveModifiedAnimationsToUopHybrid(_uopManager, _currentBody, destinationPath))
                        {
                            MessageBox.Show(
                                $"✅ Animation {_currentBody} importée en mode HYBRIDE !\n\n" +
                                $"📁 Fichier : {destinationPath}\n\n" +
                                $"🔹 32 entrées Jenkins créées (actions 0-31)\n" +
                                $"🔹 1 entrée numérique créée (action 0 - reconnaissance client)",
                                "Import Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            _uopManager.ClearCache();
                            LoadUopAnimations();
                            Options.ChangedUltimaClass["Animations"] = false;
                        }
                        else
                        {
                            MessageBox.Show("❌ Import successful but save failed. Check logs.",
                                "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            _uopManager.ClearCache();
                            LoadUopAnimations();
                            Options.ChangedUltimaClass["Animations"] = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error importing VD: {ex.Message}\n\n{ex.StackTrace}",
                            "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return;
            }

            // ================================================
            // === MUL / LEGACY TEIL (mit alten guten Features) ===
            // ================================================
            if (_fileType == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose VD file";
                dialog.CheckFileExists = true;
                dialog.Filter = "VD files (*.vd)|*.vd";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                int animLength = Animations.GetAnimLength(_currentBody, _fileType);

                int currentType = animLength switch
                {
                    22 => 0,  // Monster
                    13 => 1,  // Animal
                    35 => 2,  // Human/Equipment
                    _ => -1
                };

                if (currentType == -1)
                {
                    MessageBox.Show($"Unknown animation length: {animLength}", "Import Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader bin = new BinaryReader(fs))
                    {
                        short firstShort;
                        short animType;

                        try
                        {
                            firstShort = bin.ReadInt16();
                            animType = bin.ReadInt16();
                        }
                        catch (EndOfStreamException)
                        {
                            MessageBox.Show("Not a valid VD file (too short).", "Import",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        toolStripStatusLabelVDAminInfo.Text = $"File Type: {firstShort}, Animation Type: {animType}";

                        bool recognized = (firstShort == 0x5644) || (firstShort == 6);

                        if (!recognized)
                        {
                            toolStripStatusLabelVDAminInfo.Text += " - Not an Anim File.";
                            MessageBox.Show("Not an Anim File.", "Import",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        if (animType != currentType)
                        {
                            toolStripStatusLabelVDAminInfo.Text += $" - Wrong Anim Id (Type). Expected: {currentType}, Got: {animType}";

                            MessageBox.Show(
                                $"The selected .vd file has an animation type of {animType} (Got: {animType}),\n" +
                                $"but the program expects an animation type of {currentType} (Expected: {currentType}).\n\n" +
                                "This results in a 'Wrong Anim Id ( Type )' error.\n" +
                                "Please check the .vd file and ensure it has the correct animation type.",
                                "Wrong Animation Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            return;
                        }

                        AnimationEdit.LoadFromVD(_fileType, _currentBody, bin);
                    }
                }

                // Update tree
                bool valid = false;
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    for (int j = 0; j < animLength; ++j)
                    {
                        if (AnimationEdit.IsActionDefined(_fileType, _currentBody, j))
                        {
                            node.Nodes[j].ForeColor = Color.Black;
                            valid = true;
                        }
                        else
                        {
                            node.Nodes[j].ForeColor = Color.Red;
                        }
                    }
                    node.ForeColor = valid ? Color.Black : Color.Red;
                }

                toolStripStatusLabelVDAminInfo.Text += $" - Successfully imported into slot {_currentBody}";

                Options.ChangedUltimaClass["Animations"] = true;
                AfterSelectTreeView(this, null);

                MessageBox.Show("Successfully imported animation to the slot", "Import",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExportToVD(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            double scale = 1.0;
            string input = Microsoft.VisualBasic.Interaction.InputBox("Redimensionnement (%)", "Export VD", "100");
            if (!string.IsNullOrEmpty(input))
            {
                if (double.TryParse(input, out double percent))
                {
                    scale = percent / 100.0;
                }
                else
                {
                    MessageBox.Show("Valeur invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"anim{_fileType}_0x{_currentBody:X}.vd");
            AnimationEdit.ExportToVD(_fileType, _currentBody, fileName, scale);

            MessageBox.Show($"Animation saved to {Options.OutputPath}", "Export", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportToBin(object sender, EventArgs e)
        {
            if (_fileType != 6 || _uopManager == null) return;

            int body;
            int action;

            if (AnimationListTreeView.SelectedNode.Parent == null)
            {
                MessageBox.Show("Please select a specific action to export as .bin", "Export .bin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (AnimationListTreeView.SelectedNode.Parent.Tag is ushort ushortBody)
                body = (int)ushortBody;
            else
                body = (int)AnimationListTreeView.SelectedNode.Parent.Tag;

            action = (int)AnimationListTreeView.SelectedNode.Tag;

            var fileInfo = _uopManager.GetAnimationData(body, action, 0);
            if (fileInfo == null)
            {
                MessageBox.Show("No data found for this action.", "Export .bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // GetData(0) retrieves the full blob for UOP (AMOU) format
            byte[] data = fileInfo.GetData(0);
            if (data == null || data.Length == 0)
            {
                MessageBox.Show("Data is empty.", "Export .bin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "BIN File (*.bin)|*.bin";
                sfd.Title = $"Save Action {action} to .bin";
                sfd.FileName = $"anim_{body}_{action}.bin";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, data);
                    MessageBox.Show($"Saved to {sfd.FileName}", "Export .bin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnClickExportVdRemap(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            string targetType = (string)clickedItem.Tag;

            (short animType, int vdLength) = GetVdTargetType(targetType);

            // 1. Determine Target Action Names (Left Column Labels)
            // If we are exporting to a specific MUL format, we prefer the standard MUL action names for that type.
            string[] targetSystemActionNames;
            if (targetType == "monster_mul") targetSystemActionNames = _animNames[1];
            else if (targetType == "animal_mul" || targetType == "sea_monster_mul") targetSystemActionNames = _animNames[0];
            else if (targetType == "equipment_mul") targetSystemActionNames = _animNames[2];
            else targetSystemActionNames = GetActionNameArray(animType);

            var targetActionNames = new List<string>();
            for (int i = 0; i < vdLength; i++)
            {
                targetActionNames.Add(i < targetSystemActionNames.Length && !string.IsNullOrEmpty(targetSystemActionNames[i])
                    ? $"{i:D2} {targetSystemActionNames[i]}"
                    : $"{i:D2} (Action)");
            }

            // 2. Determine Source Action Names (Right Column Dropdown Options)
            Dictionary<int, string> sourceActionNames;
            if (_uopManager != null)
            {
                sourceActionNames = GetCreatureActionNames();
            }
            else
            {
                sourceActionNames = new Dictionary<int, string>();
                int sourceAnimLength = Animations.GetAnimLength(_currentBody, _fileType);
                string[] names = null;
                if (sourceAnimLength == 13) names = _animNames[0];
                else if (sourceAnimLength == 22) names = _animNames[1];
                else if (sourceAnimLength == 35) names = _animNames[2];

                if (names != null)
                {
                    for (int i = 0; i < names.Length; i++) sourceActionNames[i] = names[i];
                }
            }

            // 3. Prepare Mapping Data
            var vdSlotToAbstractMap = new Dictionary<int, int[]>();
            bool isDirectUopMapping = false;

            if (_uopManager == null)
            {
                for (int i = 0; i < vdLength; i++) vdSlotToAbstractMap[i] = new[] { i };
            }
            else
            {
                switch (targetType)
                {
                    case "monster_mul":
                        vdSlotToAbstractMap = new Dictionary<int, int[]> {
                                        {0, new[] {22, 24}}, {1, new[] {25}}, {2, new[] {2}}, {3, new[] {3}}, {4, new[] {4}}, {5, new[] {5}}, {6, new[] {6}}, {7, new[] {7}}, {8, new[] {8}}, {9, new[] {9}}, {10, new[] {10}},
                                        {11, new[] {11}}, {12, new[] {12}}, {13, new[] {13}}, {14, new[] {14}}, {15, new[] {15}}, {16, new[] {16}}, {17, new[] {1}},  {18, new[] {26}}, {19, new[] {19}},  {20, new[] {20}}, {21, new[] {21}}
                                    };
                        break;
                    case "animal_mul":
                        vdSlotToAbstractMap = new Dictionary<int, int[]> {
                                        {  0, new[] { 22 } },
                                        {  1, new[] { 24 } },
                                        {  2, new[] { 25 } },
                                        {  3, new[] { 27 } },
                                        {  4, new[] { 23 } },
                                        {  5, new[] { 4 } },
                                        {  6, new[] { 5 } },
                                        {  7, new[] { 10 } },
                                        {  8, new[] { 2 } },
                                        {  9, new[] { 1 } },
                                        { 10, new[] { 26 } },
                                        { 11, new[] { 11 } },
                                        { 12, new[] { 3 } }
                                    };
                        break;
                    case "sea_monster_mul":
                        vdSlotToAbstractMap = new Dictionary<int, int[]> { { 0, new[] { 0 } }, { 1, new[] { 24 } }, { 2, new[] { 25 } }, { 3, new[] { 1 } }, { 4, new[] { 26 } }, { 5, new[] { 4 } }, { 6, new[] { 5 } }, { 7, new[] { 10 } }, { 8, new[] { 2 } } };
                        break;
                    case "equipment_mul":
                        for (int j = 0; j < vdLength; j++) vdSlotToAbstractMap[j] = new[] { j };
                        break;
                    case "creatures_uop":
                    case "equipement_uop":
                        isDirectUopMapping = true;
                        break;
                }
            }

            List<int> availableUopGroupIndexes;
            Dictionary<int, int> remapMap;

            if (_uopManager != null)
            {
                availableUopGroupIndexes = _uopManager.GetAvailableUopGroupIndexes(_currentBody);
                if (_uopManager.IgnoreAnimationSequence)
                {
                    remapMap = new Dictionary<int, int>();
                }
                else
                {
                    remapMap = _uopManager.GetUopRemapping(_currentBody) ?? new Dictionary<int, int>();
                }
            }
            else
            {
                availableUopGroupIndexes = new List<int>();
                int maxActions = Animations.GetAnimLength(_currentBody, _fileType);
                for (int i = 0; i < maxActions; i++)
                {
                    if (AnimationEdit.IsActionDefined(_fileType, _currentBody, i))
                    {
                        availableUopGroupIndexes.Add(i);
                    }
                }
                remapMap = new Dictionary<int, int>();
            }

            var uopGroupIndexOptions = new List<Models.Uop.UopIndexOption> { new Models.Uop.UopIndexOption { Id = -1, DisplayName = "--- None ---" } };
            uopGroupIndexOptions.AddRange(availableUopGroupIndexes.Select(uopGroupIndex =>
            {
                string name = sourceActionNames.TryGetValue(uopGroupIndex, out var n) ? n : "Unknown";
                return new Models.Uop.UopIndexOption { Id = uopGroupIndex, DisplayName = $"{uopGroupIndex} ({name})" };
            }));

            var initialMapping = new Dictionary<int, int>();

            for (int i = 0; i < vdLength; i++)
            {
                int abstractId;
                if (isDirectUopMapping)
                {
                    abstractId = i;
                }
                else
                {
                    if (!vdSlotToAbstractMap.TryGetValue(i, out int[] abstractIds) || abstractIds.Length == 0)
                    {
                        abstractId = -1;
                    }
                    else
                    {
                        abstractId = abstractIds[0];
                    }
                }

                int finalGroup = -1;
                if (abstractId != -1)
                {
                    int targetId;
                    if (remapMap.TryGetValue(abstractId, out int remappedId))
                    {
                        targetId = remappedId;
                    }
                    else
                    {
                        targetId = abstractId;
                    }

                    if (availableUopGroupIndexes.Contains(targetId))
                    {
                        finalGroup = targetId;
                    }
                }

                initialMapping[i] = finalGroup;
            }

            using (var remapperForm = new VdExportRemapperForm())
            {
                remapperForm.PopulateForm(targetActionNames, uopGroupIndexOptions, initialMapping);

                if (remapperForm.ShowDialog(this) == DialogResult.OK)
                {
                    double scale = 1.0;
                    string input = Microsoft.VisualBasic.Interaction.InputBox("Redimensionnement (%)", "Export VD", "100");
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (double.TryParse(input, out double percent))
                        {
                            scale = percent / 100.0;
                        }
                        else
                        {
                            MessageBox.Show("Valeur invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    var finalMapping = remapperForm.GetRemapping();
                    var targetActionNameToIndex = new Dictionary<string, int>();
                    for (int i = 0; i < targetActionNames.Count; ++i)
                    {
                        targetActionNameToIndex[targetActionNames[i]] = i;
                    }

                    var exportedAnimations = new Models.Uop.UOAnimation[vdLength];

                    foreach (var mappingEntry in finalMapping)
                    {
                        string targetActionName = mappingEntry.Key;
                        int sourceGroupId = mappingEntry.Value;

                        if (targetActionNameToIndex.TryGetValue(targetActionName, out int targetActionIndex) && sourceGroupId != -1)
                        {
                            if (targetActionIndex < vdLength)
                            {
                                if (_uopManager != null)
                                {
                                    exportedAnimations[targetActionIndex] = _uopManager.GetAnimationExportData(_currentBody, sourceGroupId);
                                }
                                else
                                {
                                    exportedAnimations[targetActionIndex] = CreateUoAnimationFromMulAction(_fileType, _currentBody, sourceGroupId);
                                }
                            }
                        }
                    }

                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "VD File (*.vd)|*.vd";
                        sfd.Title = "Save VD File";
                        sfd.FileName = $"anim_{targetType}_{_currentBody}.vd";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                            using (BinaryWriter writer = new BinaryWriter(fs))
                            {
                                if (animType == 4) // Creatures (UOP) - Use Optimized Export
                                {
                                    var remapping = new Dictionary<int, int>();
                                    foreach (var kvp in finalMapping)
                                    {
                                        if (targetActionNameToIndex.TryGetValue(kvp.Key, out int targetIdx))
                                        {
                                            remapping[targetIdx] = kvp.Value;
                                        }
                                    }

                                    // Ensure existing modified animations are serialized first
                                    if (_uopManager != null)
                                    {
                                        _uopManager.CommitAllChanges();
                                        System.Diagnostics.Debug.WriteLine("Export VD: CommitAllChanges called to serialize modified UOP animations before VD export.");
                                    }

                                    // NEW: For exported animations that exist only as UOAnimation (generated from MUL)
                                    // create UopAnimIdx in cache and populate frames so CommitChanges can encode them.
                                    if (_uopManager != null)
                                    {
                                        foreach (var kvp in remapping)
                                        {
                                            int targetIdx = kvp.Key;
                                            int sourceGroup = kvp.Value;

                                            if (sourceGroup < 0) continue;
                                            var exported = (targetIdx >= 0 && targetIdx < exportedAnimations.Length) ? exportedAnimations[targetIdx] : null;
                                            if (exported == null) continue;

                                            try
                                            {
                                                var fi = _uopManager.GetAnimationData(_currentBody, sourceGroup, 0);
                                                if (fi != null)
                                                {
                                                    // already has source data, nothing to do
                                                    continue;
                                                }

                                                System.Diagnostics.Debug.WriteLine($"Export VD: creating in-memory UOP entry for Anim={_currentBody} Action={sourceGroup}");

                                                // Determine frames per direction
                                                int framesPerDir = exported.FramesPerDirection;
                                                if (framesPerDir <= 0)
                                                {
                                                    if (exported.Frames != null && exported.Frames.Count > 0)
                                                        framesPerDir = Math.Max(1, exported.Frames.Count / 5);
                                                    else
                                                        framesPerDir = 0;
                                                }

                                                for (int dir = 0; dir < 5; dir++)
                                                {
                                                    // create new UopAnimIdx and ensure in cache
                                                    var uopAnim = _uopManager.CreateNewUopAnimation(_currentBody, sourceGroup, dir);
                                                    uopAnim.Frames.Clear();

                                                    if (framesPerDir == 0) continue;

                                                    for (int f = 0; f < framesPerDir; f++)
                                                    {
                                                        int globalIndex = dir * framesPerDir + f;
                                                        if (globalIndex < 0 || exported.Frames == null || globalIndex >= exported.Frames.Count) break;

                                                        // Use reflection to extract the frame data (ureliable names: try common variations)
                                                        object frameEntry = exported.Frames[globalIndex];
                                                        if (frameEntry == null) continue;

                                                        object frameData = frameEntry.GetType().GetProperty("FrameData")?.GetValue(frameEntry)
                                                                           ?? frameEntry.GetType().GetProperty("Data")?.GetValue(frameEntry)
                                                                           ?? frameEntry.GetType().GetProperty("Frame")?.GetValue(frameEntry);

                                                        if (frameData == null) continue;

                                                        // Extract image (DirectBitmap or Bitmap)
                                                        object imageObj = frameData.GetType().GetProperty("Image")?.GetValue(frameData)
                                                                          ?? frameData.GetType().GetProperty("Bitmap")?.GetValue(frameData)
                                                                          ?? frameData.GetType().GetProperty("Img")?.GetValue(frameData);

                                                        Bitmap bmp = null;
                                                        if (imageObj is Bitmap b) bmp = new Bitmap(b);
                                                        else if (imageObj != null)
                                                        {
                                                            // try property "Bitmap"
                                                            var p = imageObj.GetType().GetProperty("Bitmap")?.GetValue(imageObj);
                                                            if (p is Bitmap pb) bmp = new Bitmap(pb);
                                                            else
                                                            {
                                                                // try ToBitmap method
                                                                var mi = imageObj.GetType().GetMethod("ToBitmap") ?? imageObj.GetType().GetMethod("GetBitmap");
                                                                if (mi != null)
                                                                {
                                                                    var res = mi.Invoke(imageObj, null);
                                                                    if (res is Bitmap rb) bmp = new Bitmap(rb);
                                                                }
                                                            }
                                                        }

                                                        if (bmp == null)
                                                        {
                                                            System.Diagnostics.Debug.WriteLine($"Export VD: unable to retrieve Bitmap for frame {globalIndex} (Anim {_currentBody} Act {sourceGroup} Dir {dir})");
                                                            continue;
                                                        }

                                                        var decoded = new UoFiddler.Controls.Uop.DecodedUopFrame();
                                                        decoded.Image = new Bitmap(bmp);

                                                        // Header: try to read CenterX/CenterY/Width/Height from frameData
                                                        short centerX = 0, centerY = 0;
                                                        ushort width = (ushort)decoded.Image.Width, height = (ushort)decoded.Image.Height;

                                                        try
                                                        {
                                                            var hdr = frameData.GetType().GetProperty("Header")?.GetValue(frameData);
                                                            if (hdr != null)
                                                            {
                                                                var cx = hdr.GetType().GetProperty("CenterX")?.GetValue(hdr);
                                                                var cy = hdr.GetType().GetProperty("CenterY")?.GetValue(hdr);
                                                                var w = hdr.GetType().GetProperty("Width")?.GetValue(hdr);
                                                                var h = hdr.GetType().GetProperty("Height")?.GetValue(hdr);
                                                                if (cx != null) centerX = Convert.ToInt16(cx);
                                                                if (cy != null) centerY = Convert.ToInt16(cy);
                                                                if (w != null) width = Convert.ToUInt16(w);
                                                                if (h != null) height = Convert.ToUInt16(h);
                                                            }
                                                            else
                                                            {
                                                                var cx = frameData.GetType().GetProperty("CenterX")?.GetValue(frameData);
                                                                var cy = frameData.GetType().GetProperty("CenterY")?.GetValue(frameData);
                                                                if (cx != null) centerX = Convert.ToInt16(cx);
                                                                if (cy != null) centerY = Convert.ToInt16(cy);
                                                            }
                                                        }
                                                        catch { /* best effort only */ }

                                                        decoded.Header = new UoFiddler.Controls.Uop.UopFrameHeader
                                                        {
                                                            CenterX = centerX,
                                                            CenterY = centerY,
                                                            Width = width,
                                                            Height = height
                                                        };

                                                        // Palette: try to extract list<Color>
                                                        try
                                                        {
                                                            var paletteObj = frameData.GetType().GetProperty("Palette")?.GetValue(frameData);
                                                            if (paletteObj is System.Collections.IEnumerable palEnum)
                                                            {
                                                                var palList = new List<Color>();
                                                                foreach (var entry in palEnum)
                                                                {
                                                                    if (entry is Color c) palList.Add(c);
                                                                    else
                                                                    {
                                                                        // try properties R,G,B,A
                                                                        var r = entry.GetType().GetProperty("R")?.GetValue(entry);
                                                                        var g = entry.GetType().GetProperty("G")?.GetValue(entry);
                                                                        var b2 = entry.GetType().GetProperty("B")?.GetValue(entry);
                                                                        var a = entry.GetType().GetProperty("Alpha")?.GetValue(entry) ?? entry.GetType().GetProperty("A")?.GetValue(entry);
                                                                        if (r != null && g != null && b2 != null)
                                                                        {
                                                                            palList.Add(Color.FromArgb(a != null ? Convert.ToInt32(a) : 255, Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b2)));
                                                                        }
                                                                    }
                                                                }
                                                                decoded.Palette = palList;
                                                            }
                                                        }
                                                        catch { /* ignore palette if unavailable */ }

                                                        uopAnim.Frames.Add(decoded);
                                                    } // framesPerDir
                                                    uopAnim.IsModified = true;
                                                    // Commit per action after filling all directions for reliability (will be called below)
                                                } // dir loop

                                                // After populating the cached UopAnimIdx entries for this action, request commit
                                                _uopManager.CommitChanges(_currentBody, sourceGroup);
                                                System.Diagnostics.Debug.WriteLine($"Export VD: committed in-memory UOP data for Anim={_currentBody} Action={sourceGroup}");
                                            }
                                            catch (Exception ex)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"Export VD: failed to create in-memory UOP entry for Anim {_currentBody} Action {sourceGroup}: {ex.Message}");
                                            }
                                        } // foreach remapping
                                    } // if _uopManager != null

                                    Uop.VdExportHelper.WriteVDCreaturesUop(writer, _uopManager, _currentBody, remapping, scale);
                                }
                                else
                                {
                                    Uop.VdExportHelper.WriteVDHeader(writer, animType);
                                    Uop.VdExportHelper.WriteVDAnimations(writer, exportedAnimations, animType, scale);
                                }
                            }
                            MessageBox.Show($"Successfully exported to {sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private Dictionary<int, string> GetCreatureActionNames()
        {
            return new Dictionary<int, string>
            {
                {0, "Walk Combat"}, {1, "Idle Combat"}, {2, "Die Backward"}, {3, "Die Forward"}, {4, "Attack 1"},
                {5, "Attack 2"}, {6, "Attack 3"}, {7, "AttackBow"}, {8, "AttackCrossBow"}, {9, "AttackThrow"},
                {10, "Get Hit"}, {11, "Rummage"}, {12, "Spellcast"}, {13, "Spellcast2"}, {14, "Spellcast3"},
                {15, "BlockRight"}, {16, "BlockLeft"}, {17, "Idle"}, {18, "Fidget"}, {19, "Fly"}, {20, "TakeOff"},
                {21, "GetHitInAir"}, {22, "Walk"}, {23, "Special"}, {24, "Run"}, {25, "Idle"}, {26, "Fidget"},
                {27, "Roar"}, {28, "Peace to Combat"}, {29, "Mounted - Walk"}, {30, "Mounted - Run"}, {31, "Mounted - Idle"}
            };
        }

        private static Dictionary<int, string> _charActionNames;

        private static Dictionary<int, string> GetCharActionNamesDictionary()
        {
            if (_charActionNames == null)
            {
                _charActionNames = new Dictionary<int, string>();
                for (int i = 0; i < Uop.UopConstants.ActionNames.CharActions.Length; i++)
                {
                    if (!string.IsNullOrEmpty(Uop.UopConstants.ActionNames.CharActions[i]))
                    {
                        _charActionNames[i] = Uop.UopConstants.ActionNames.CharActions[i];
                    }
                }
            }
            return _charActionNames;
        }

        private string GetActionDescription(int animId, int actionId)
        {
            Dictionary<int, string> namesDictionary;
            // Access _uopManager directly since GetActionDescription is no longer static
            List<int> availableActions = _uopManager.GetAvailableActions(animId);

            if (availableActions.Count > 32) // Heuristic based on user's input
            {
                namesDictionary = GetCharActionNamesDictionary();
            }
            else
            {
                namesDictionary = GetCreatureActionNames();
            }

            if (namesDictionary.TryGetValue(actionId, out string name))
            {
                return name;
            }
            return "Unknown Action";
        }


        private string[] GetActionNameArray(short animType)
        {
            switch (animType)
            {
                case 0: return Uop.UopConstants.ActionNames.MonsterActions;
                case 1: return Uop.UopConstants.ActionNames.AnimalActions;
                case 2: return Uop.UopConstants.ActionNames.HumanActions;
                case 3: return Uop.UopConstants.ActionNames.CharActions;
                case 4:
                    var creatureActions = new string[32];
                    var names = GetCreatureActionNames();
                    for (int i = 0; i < creatureActions.Length; i++)
                    {
                        creatureActions[i] = names.TryGetValue(i, out var name) ? name : "";
                    }
                    return creatureActions;
                default: return _animNames[1];
            }
        }

        private (short animType, int vdLength) GetVdTargetType(string targetType)
        {
            switch (targetType)
            {
                case "monster_mul":
                    return (0, 22);
                case "animal_mul":
                    return (1, 13);
                case "sea_monster_mul":
                    return (1, 13);
                case "equipment_mul":
                    return (2, 35);
                case "equipement_uop":
                    return (3, 78);
                case "creatures_uop":
                    return (4, 32);
                default:
                    return (0, 22);
            }
        }


        private void OnClickShowOnlyValid(object sender, EventArgs e)
        {
            _showOnlyValid = !_showOnlyValid;

            if (_fileType == UOP_FILE_TYPE)
            {
                LoadUopAnimations();
            }
            else
            {
                LoadMulAnimations();
            }
        }

        //My Soulblighter Modification
        private void SameCenterButton_Click(object sender, EventArgs e)
        {
            // TODO: there is no undo for same center button
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                if (_relativeMode)
                {
                    int dX = _accumulatedRelativeX;
                    int dY = _accumulatedRelativeY;
                    _accumulatedRelativeX = 0;
                    _accumulatedRelativeY = 0;

                    if (dX == 0 && dY == 0) return;

                    if (_fileType == 6)
                    {
                        var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                        if (uopAnim != null)
                        {
                            for (int i = 0; i < uopAnim.Frames.Count; i++)
                            {
                                if (i == FramesTrackBar.Value) continue;
                                var f = uopAnim.Frames[i];
                                uopAnim.ChangeCenter(i, f.Header.CenterX + dX, f.Header.CenterY + dY);
                            }
                        }
                    }
                    else
                    {
                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                        if (edit != null)
                        {
                            for (int i = 0; i < edit.Frames.Count; i++)
                            {
                                if (i == FramesTrackBar.Value) continue;
                                var f = edit.Frames[i];
                                f.ChangeCenter(f.Center.X + dX, f.Center.Y + dY);
                            }
                        }
                    }
                    Options.ChangedUltimaClass["Animations"] = true;
                    AnimationPictureBox.Invalidate();
                    return;
                }

                if (_fileType == 6)
                {
                    if (FramesListView.Items.Count == 0) return;
                    int newCenterX = (int)CenterXNumericUpDown.Value;
                    int newCenterY = (int)CenterYNumericUpDown.Value;

                    UpdateUopData(newCenterX: newCenterX, newCenterY: newCenterY, applyToAllFrames: true);
                    Options.ChangedUltimaClass["Animations"] = true;
                    AnimationPictureBox.Invalidate();
                    return;
                }

                AnimIdx edit2 = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit2 == null || edit2.Frames.Count < FramesTrackBar.Value)
                {
                    return;
                }

                FrameEdit[] frame = new FrameEdit[edit2.Frames.Count];
                for (int index = 0; index < edit2.Frames.Count; index++)
                {
                    frame[index] = edit2.Frames[index];
                    frame[index].ChangeCenter((int)CenterXNumericUpDown.Value, (int)CenterYNumericUpDown.Value);
                    Options.ChangedUltimaClass["Animations"] = true;
                    AnimationPictureBox.Invalidate();
                }
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }
        //End of Soulblighter Modification

        //My Soulblighter Modification
        private void FromGifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose palette file";
                dialog.CheckFileExists = true;
                dialog.Filter = "Gif files (*.gif)|*.gif";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bit = new Bitmap(dialog.FileName);
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    return;
                }

                FrameDimension dimension = new FrameDimension(bit.FrameDimensionsList[0]);
                // Number of frames
                //int frameCount = bit.GetFrameCount(dimension); // TODO: unused variable?
                bit.SelectActiveFrame(dimension, 0);
                UpdateGifPalette(bit, edit);
                SetPaletteBox();
                FramesListView.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
            }
        }

        private void ReferencePointX(object sender, EventArgs e)
        {
            AnimationPictureBox.Invalidate();
        }

        private void ReferencePointY(object sender, EventArgs e)
        {
            AnimationPictureBox.Invalidate();
        }

        private static bool _lockButton;

        private void AnimationPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_lockButton || !ToolStripLockButton.Enabled)
            {
                return;
            }

            RefXNumericUpDown.Value = (decimal)((_framePoint.X - e.X) / _zoomFactor);
            RefYNumericUpDown.Value = (decimal)((_framePoint.Y - e.Y) / _zoomFactor);

            AnimationPictureBox.Invalidate();
        }

        // Change center of frame on key press
        private void TxtSendData_KeyDown(object sender, KeyEventArgs e)
        {
            if (AnimationTimer.Enabled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        CenterXNumericUpDown.Value--;
                        CenterXNumericUpDown.Invalidate();
                        break;
                    }
                case Keys.Left:
                    {
                        CenterXNumericUpDown.Value++;
                        CenterXNumericUpDown.Invalidate();
                        break;
                    }
                case Keys.Up:
                    {
                        CenterYNumericUpDown.Value++;
                        CenterYNumericUpDown.Invalidate();
                        break;
                    }
                case Keys.Down:
                    {
                        CenterYNumericUpDown.Value--;
                        CenterYNumericUpDown.Invalidate();
                        break;
                    }
            }
            AnimationPictureBox.Invalidate();
        }

        // Change center of Reference Point on key press
        private void TxtSendData_KeyDown2(object sender, KeyEventArgs e)
        {
            if (_lockButton || !ToolStripLockButton.Enabled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Right:
                    {
                        RefXNumericUpDown.Value--;
                        RefXNumericUpDown.Invalidate();
                        break;
                    }
                case Keys.Left:
                    {
                        RefXNumericUpDown.Value++;
                        RefXNumericUpDown.Invalidate();
                        break;
                    }
                case Keys.Up:
                    {
                        RefYNumericUpDown.Value++;
                        RefYNumericUpDown.Invalidate();
                        break;
                    }
                case Keys.Down:
                    {
                        RefYNumericUpDown.Value--;
                        RefYNumericUpDown.Invalidate();
                        break;
                    }
            }
            AnimationPictureBox.Invalidate();
        }

        private void ToolStripLockButton_Click(object sender, EventArgs e)
        {
            _lockButton = !_lockButton;
            RefXNumericUpDown.Enabled = !_lockButton;
            RefYNumericUpDown.Enabled = !_lockButton;
        }

        // Add in all Directions
        private void AllDirectionsAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                // ✅ UOP: Check if we need to select a target file
                string targetUopPath = null;
                if (_fileType == 6 && _uopManager != null)
                {
                    bool animationExists = false;
                    for (int d = 0; d < 5; d++)
                    {
                        if (_uopManager.GetUopAnimation(_currentBody, _currentAction, d) != null)
                        {
                            animationExists = true;
                            break;
                        }
                    }

                    if (!animationExists)
                    {
                        using (var fileSelectForm = new UopFileSelectionForm(_uopManager.LoadedUopFiles))
                        {
                            if (fileSelectForm.ShowDialog() == DialogResult.OK)
                            {
                                targetUopPath = fileSelectForm.SelectedPath;
                            }
                            else
                            {
                                return; // Cancelled
                            }
                        }
                    }
                }

                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose images (Multiple of 5) to add to all directions";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.tiff, *.gif)|*.bmp;*.jpg;*.png;*.tiff;*.gif";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.FileNames.Length == 0 || dialog.FileNames.Length % 5 != 0)
                        {
                            MessageBox.Show("Please select a number of images that is a multiple of 5 (e.g., 5, 10, 15...).\nThe images will be distributed evenly across the 5 directions.",
                                "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        DirectionTrackBar.Enabled = false;
                        AddFilesAllDirections(dialog, targetUopPath);
                        DirectionTrackBar.Enabled = true;
                    }
                }
            }

            // Refresh List
            _currentDir = DirectionTrackBar.Value;
            AfterSelectTreeView(null, null);
        }

        private void AddFilesAllDirections(OpenFileDialog dialog, string targetUopPath = null)
        {
            // Ensure strict alphabetical order
            var fileList = dialog.FileNames.OrderBy(f => f).ToList();

            int totalFiles = fileList.Count;
            int filesPerDir = totalFiles / 5;

            if (_fileType == 6) // UOP - Batch Process
            {
                for (int dir = 0; dir < 5; dir++)
                {
                    int startIndex = dir * filesPerDir;
                    int endIndex = startIndex + filesPerDir;

                    var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, dir);
                    if (uopAnim == null)
                    {
                        uopAnim = _uopManager.CreateNewUopAnimation(_currentBody, _currentAction, dir, targetUopPath);
                    }

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        string fileName = fileList[i];
                        if (!System.IO.File.Exists(fileName)) continue;

                        using (Bitmap bmpTemp = new Bitmap(fileName))
                        {
                            Bitmap bitmap = new Bitmap(bmpTemp);
                            FrameDimension dimension = new FrameDimension(bitmap.FrameDimensionsList[0]);
                            int frameCount = bitmap.GetFrameCount(dimension);

                            for (int f = 0; f < frameCount; f++)
                            {
                                bitmap.SelectActiveFrame(dimension, f);
                                using (var frameBmp = new Bitmap(bitmap))
                                {
                                    var frame = new UoFiddler.Controls.Uop.DecodedUopFrame();
                                    frame.Image = new Bitmap(frameBmp);
                                    frame.Header = new UoFiddler.Controls.Uop.UopFrameHeader
                                    {
                                        Width = (ushort)frame.Image.Width,
                                        Height = (ushort)frame.Image.Height,
                                        CenterX = (short)(frame.Image.Width / 2),
                                        CenterY = (short)(frame.Image.Height)
                                    };

                                    var paletteEntries = UoFiddler.Controls.Uop.VdExportHelper.GenerateProperPaletteFromImage(new UoFiddler.Controls.Models.Uop.Imaging.DirectBitmap(frame.Image));
                                    frame.Palette = paletteEntries.Select(p => Color.FromArgb(p.Alpha, p.R, p.G, p.B)).ToList();

                                    uopAnim.Frames.Add(frame);
                                    uopAnim.IsModified = true;
                                }
                            }
                        }
                    }
                }
                _uopManager.CommitChanges(_currentBody, _currentAction);
                _uopManager.ClearCache(); // Force reload for BB update
            }
            else // MUL
            {
                for (int dir = 0; dir < 5; dir++)
                {
                    DirectionTrackBar.Value = dir;
                    _currentDir = dir;

                    int startIndex = dir * filesPerDir;
                    int endIndex = startIndex + filesPerDir;

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        string fileName = fileList[i];
                        if (!System.IO.File.Exists(fileName)) continue;

                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                        if (edit != null)
                        {
                            using (Bitmap bmpTemp = new Bitmap(fileName))
                            {
                                Bitmap bitmap = new Bitmap(bmpTemp);
                                FrameDimension dimension = new FrameDimension(bitmap.FrameDimensionsList[0]);
                                int frameCount = bitmap.GetFrameCount(dimension);

                                if (frameCount > 1 || fileName.ToLower().EndsWith(".gif"))
                                {
                                    bitmap.SelectActiveFrame(dimension, 0);
                                    UpdateGifPalette(bitmap, edit);
                                    Bitmap[] bitBmp = new Bitmap[frameCount];
                                    AddImageAtCertainIndex(frameCount, bitBmp, bitmap, dimension, edit);
                                }
                                else
                                {
                                    edit.AddFrame(bitmap);
                                }
                            }
                        }
                    }
                    FramesListView.Invalidate();
                }
            }
        }

        private void DrawEmptyRectangleToolStripButton_Click(object sender, EventArgs e)
        {
            _drawEmpty = !_drawEmpty;
            AnimationPictureBox.Invalidate();
        }

        private void DrawFullRectangleToolStripButton_Click(object sender, EventArgs e)
        {
            _drawFull = !_drawFull;
            AnimationPictureBox.Invalidate();
        }

        private void DrawBoundingBoxToolStripButton_Click(object sender, EventArgs e)
        {
            _drawBoundingBox = !_drawBoundingBox;
            AnimationPictureBox.Invalidate();
        }

        private void DrawCroppingBoxToolStripButton_Click(object sender, EventArgs e)
        {
            _drawCroppingBox = !_drawCroppingBox;
            AnimationPictureBox.Invalidate();
        }

        private void AnimationEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            AnimationTimer.Enabled = false;
            _drawFull = false;
            _drawEmpty = false;
            _lockButton = false;
            _loaded = false;

            ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (FramesTrackBar.Value < FramesTrackBar.Maximum)
            {
                FramesTrackBar.Value++;
            }
            else
            {
                FramesTrackBar.Value = 0;
            }

            AnimationPictureBox.Invalidate();
        }

        private void ToolStripButtonPlayAnimation_Click(object sender, EventArgs e)
        {
            if (AnimationTimer.Enabled)
            {
                AnimationTimer.Enabled = false;
                FramesTrackBar.Enabled = true;
                SameCenterButton.Enabled = true;
                CenterXNumericUpDown.Enabled = true;
                CenterYNumericUpDown.Enabled = true;

                if (DrawReferencialPointToolStripButton.Checked)
                {
                    ToolStripLockButton.Enabled = false;
                    _blackUndraw = _blackUnDrawTransparent;
                    _whiteUnDraw = _whiteUnDrawTransparent;
                }
                else
                {
                    ToolStripLockButton.Enabled = true;
                    _blackUndraw = _blackUnDrawOpaque;
                    _whiteUnDraw = _whiteUnDrawOpaque;
                }

                if (ToolStripLockButton.Checked || DrawReferencialPointToolStripButton.Checked)
                {
                    RefXNumericUpDown.Enabled = false;
                    RefYNumericUpDown.Enabled = false;
                }
                else
                {
                    RefXNumericUpDown.Enabled = true;
                    RefYNumericUpDown.Enabled = true;
                }
            }
            else
            {
                AnimationTimer.Enabled = true;
                FramesTrackBar.Enabled = false;
                SameCenterButton.Enabled = false;

                CenterXNumericUpDown.Enabled = false;
                CenterYNumericUpDown.Enabled = false;
            }

            AnimationPictureBox.Invalidate();
        }

        private void AnimationSpeedTrackBar_ValueChanged(object sender, EventArgs e)
        {
            AnimationTimer.Interval = 50 + (AnimationSpeedTrackBar.Value * 30);
        }

        private void DrawReferencialPointToolStripButton_Click(object sender, EventArgs e)
        {
            if (!DrawReferencialPointToolStripButton.Checked)
            {
                _blackUndraw = _blackUnDrawOpaque;
                _whiteUnDraw = _whiteUnDrawOpaque;

                ToolStripLockButton.Enabled = true;
                if (ToolStripLockButton.Checked)
                {
                    RefXNumericUpDown.Enabled = false;
                    RefYNumericUpDown.Enabled = false;
                }
                else
                {
                    RefXNumericUpDown.Enabled = true;
                    RefYNumericUpDown.Enabled = true;
                }
            }
            else
            {
                _blackUndraw = _blackUnDrawTransparent;
                _whiteUnDraw = _whiteUnDrawTransparent;
                ToolStripLockButton.Enabled = false;
                RefXNumericUpDown.Enabled = false;
                RefYNumericUpDown.Enabled = false;
            }
            AnimationPictureBox.Invalidate();
        }

        // All Directions with Canvas
        private void AllDirectionsAddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                MessageBox.Show("Unavailable for .UOP format", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = true;
                        dialog.Title = "Choose 5 GIFs to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Image Files (*.bmp, *.jpg, *.png, *.tiff, *.gif)|*.bmp;*.jpg;*.png;*.tiff;*.gif";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);
                            DirectionTrackBar.Enabled = false;
                            if (dialog.FileNames.Length == 5)
                            {
                                DirectionTrackBar.Value = 0;
                                AddSelectedFiles(dialog, customConvert);
                            }

                            // Looping if dialog.FileNames.Length != 5
                            while (dialog.FileNames.Length != 5)
                            {
                                if (dialog.ShowDialog() == DialogResult.Cancel)
                                {
                                    break;
                                }

                                if (dialog.FileNames.Length != 5)
                                {
                                    dialog.ShowDialog();
                                }

                                if (dialog.FileNames.Length != 5)
                                {
                                    continue;
                                }

                                DirectionTrackBar.Value = 0;
                                AddSelectedFiles(dialog, customConvert);
                            }

                            DirectionTrackBar.Enabled = true;
                        }
                    }
                }

                // Refresh List after Canvas reduction
                _currentDir = DirectionTrackBar.Value;
                AfterSelectTreeView(null, null);
            }
            catch (OutOfMemoryException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }

        private void AddSelectedFiles(OpenFileDialog dialog, Color customConvert)
        {
            for (int w = 0; w < dialog.FileNames.Length; w++)
            {
                if (w >= 5)
                {
                    continue;
                }

                // dialog.Filename replaced by dialog.FileNames[w]
                if (!System.IO.File.Exists(dialog.FileNames[w])) continue;

                using (Bitmap bmpTemp = new Bitmap(dialog.FileNames[w]))
                {
                    Bitmap bmp = new Bitmap(bmpTemp);

                    if (_fileType == 6)
                    {
                        AddAnimationX1Uop(customConvert, bmp);
                    }
                    else
                    {
                        AddAnimationX1(customConvert, bmp);
                    }
                }

                if ((w < 4) && (w < dialog.FileNames.Length - 1))
                {
                    DirectionTrackBar.Value++;
                    _currentDir = DirectionTrackBar.Value;
                }
            }
        }

        private void AddAnimationX1Uop(Color customConvert, Bitmap bmp)
        {
            var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
            if (uopAnim == null)
            {
                uopAnim = _uopManager.CreateNewUopAnimation(_currentBody, _currentAction, _currentDir);
            }

            FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
            int frameCount = bmp.GetFrameCount(dimension);
            ProgressBar.Maximum = frameCount;

            // Extract frames to array
            Bitmap[] bitBmp = new Bitmap[frameCount];
            for (int index = 0; index < frameCount; index++)
            {
                bmp.SelectActiveFrame(dimension, index);
                bitBmp[index] = new Bitmap(bmp); // Clone
            }

            // --- Canvas Algorithm (Duplicate of AddAnimationX1 logic) ---
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[0].Height; yf++)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    for (int xf = 0; xf < bitBmp[0].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf--;
                                xf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT != -1)
                        {
                            top -= regressT;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk) break;
                }
                if (yf < bitBmp[0].Height - 9) yf += 9;
                if (breakOk) { breakOk = false; break; }
            }

            // Bottom
            for (int yf = bitBmp[0].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    for (int xf = 0; xf < bitBmp[0].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0) var = true;

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[0].Height - 1)
                            {
                                regressB++;
                                yf++;
                                xf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB == -1 && yf > 9) bottom += 10;
                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB == -1 && yf <= 9) bottom++;
                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB != -1)
                        {
                            bottom -= regressB;
                            breakOk = true;
                            break;
                        }
                    }
                    if (breakOk) break;
                }
                if (yf > 9) yf -= 9;
                if (breakOk) { breakOk = false; break; }
            }

            // Left
            for (int xf = 0; xf < bitBmp[0].Width; xf++)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    for (int yf = 0; yf < bitBmp[0].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0) var = true;

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf--;
                                yf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL == -1 && xf < bitBmp[0].Width - 9) left += 10;
                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL == -1 && xf >= bitBmp[0].Width - 9) left++;
                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL != -1)
                        {
                            left -= regressL;
                            breakOk = true;
                            break;
                        }
                    }
                    if (breakOk) break;
                }
                if (xf < bitBmp[0].Width - 9) xf += 9;
                if (breakOk) { breakOk = false; break; }
            }

            // Right
            for (int xf = bitBmp[0].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    for (int yf = 0; yf < bitBmp[0].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0) var = true;

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[0].Width - 1)
                            {
                                regressR++;
                                xf++;
                                yf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR == -1 && xf > 9) right += 10;
                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR == -1 && xf <= 9) right++;
                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR != -1)
                        {
                            right -= regressR;
                            breakOk = true;
                            break;
                        }
                    }
                    if (breakOk) break;
                }
                if (xf > 9) xf -= 9;
                if (breakOk) { breakOk = false; break; }
            }

            for (int index = 0; index < frameCount; index++)
            {
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                if (rect.Width <= 0 || rect.Height <= 0) continue;
                bitBmp[index] = bitBmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            // --- End Canvas Algorithm ---

            // Add frames
            for (int index = 0; index < frameCount; index++)
            {
                var frame = new UoFiddler.Controls.Uop.DecodedUopFrame();
                frame.Image = new Bitmap(bitBmp[index]);
                frame.Header = new UoFiddler.Controls.Uop.UopFrameHeader
                {
                    Width = (ushort)frame.Image.Width,
                    Height = (ushort)frame.Image.Height,
                    CenterX = (short)(frame.Image.Width / 2),
                    CenterY = (short)(frame.Image.Height)
                };

                var paletteEntries = UoFiddler.Controls.Uop.VdExportHelper.GenerateProperPaletteFromImage(new UoFiddler.Controls.Models.Uop.Imaging.DirectBitmap(frame.Image));
                frame.Palette = paletteEntries.Select(p => Color.FromArgb(p.Alpha, p.R, p.G, p.B)).ToList();

                uopAnim.Frames.Add(frame);

                int newIndex = uopAnim.Frames.Count - 1;
                ListViewItem item = new ListViewItem(newIndex.ToString(), 0) { Tag = newIndex };
                FramesListView.Items.Add(item);

                // Update TileSize to fit new image
                int width = FramesListView.TileSize.Width - 5;
                if (frame.Image.Width > width) width = frame.Image.Width;
                int height = FramesListView.TileSize.Height - 5;
                if (frame.Image.Height > height) height = frame.Image.Height;
                FramesListView.TileSize = new Size(width + 5, height + 5);

                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }
            }
            uopAnim.IsModified = true;

            ProgressBar.Value = 0;
            ProgressBar.Invalidate();
            FramesListView.Invalidate();
            Options.ChangedUltimaClass["Animations"] = true;
        }

        private void ApplyTransparency(Bitmap bmp, Color keyColor)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* ptr = (byte*)bd.Scan0;
                int bytes = bd.Stride * bmp.Height;
                byte kR = keyColor.R;
                byte kG = keyColor.G;
                byte kB = keyColor.B;

                for (int i = 0; i < bytes; i += 4)
                {
                    // BGRA
                    byte b = ptr[i];
                    byte g = ptr[i + 1];
                    byte r = ptr[i + 2];

                    if (r == kR && g == kG && b == kB)
                    {
                        ptr[i + 3] = 0; // Transparent
                    }
                    else
                    {
                        ptr[i + 3] = 255; // Opaque
                    }
                }
            }
            bmp.UnlockBits(bd);
        }

        private void AddAnimationX1(Color customConvert, Bitmap bmp)
        {
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
            {
                return;
            }

            FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);

            // Number of frames
            int frameCount = bmp.GetFrameCount(dimension);
            ProgressBar.Maximum = frameCount;
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);

            // Return an Image at a certain index
            Bitmap[] bitBmp = new Bitmap[frameCount];
            for (int index = 0; index < frameCount; index++)
            {
                bitBmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                bmp.SelectActiveFrame(dimension, index);
                bitBmp[index] = bmp;
            }

            // Canvas algorithm
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[0].Height; yf++)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[0].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf--;
                                xf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT != -1)
                        {
                            top -= regressT;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (yf < bitBmp[0].Height - 9)
                {
                    yf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Bottom
            for (int yf = bitBmp[0].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[0].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[0].Height - 1)
                            {
                                regressB++;
                                yf++;
                                xf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB == -1 &&
                            yf > 9)
                        {
                            bottom += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB == -1 &&
                            yf <= 9)
                        {
                            bottom++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB != -1)
                        {
                            bottom -= regressB;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (yf > 9)
                {
                    yf -= 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Left
            for (int xf = 0; xf < bitBmp[0].Width; xf++)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[0].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf--;
                                yf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL == -1 &&
                            xf < bitBmp[0].Width - 9)
                        {
                            left += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL == -1 &&
                            xf >= bitBmp[0].Width - 9)
                        {
                            left++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL != -1)
                        {
                            left -= regressL;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (xf < bitBmp[0].Width - 9)
                {
                    xf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Right
            for (int xf = bitBmp[0].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[0].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[0].Width - 1)
                            {
                                regressR++;
                                xf++;
                                yf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR == -1 &&
                            xf > 9)
                        {
                            right += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR == -1 &&
                            xf <= 9)
                        {
                            right++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR != -1)
                        {
                            right -= regressR;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (xf > 9)
                {
                    xf -= 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    //breakOk = false;
                    break;
                }
            }

            for (int index = 0; index < frameCount; index++)
            {
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = bitBmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
            }

            // End of Canvas algorithm

            for (int index = 0; index < frameCount; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnim(bitBmp[index], (int)numericUpDownRed.Value,
                    (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }
            }

            ProgressBar.Value = 0;
            ProgressBar.Invalidate();
            SetPaletteBox();
            FramesListView.Invalidate();

            _updatingUi = true;
            if (!_relativeMode)
            {
                CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
            }
            else
            {
                CenterXNumericUpDown.Value = 0;
                CenterYNumericUpDown.Value = 0;
            }
            _updatingUi = false;

            Options.ChangedUltimaClass["Animations"] = true;
        }

        //Add with Canvas
        private void AddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                MessageBox.Show("Unavailable for .UOP format", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose image file to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDownRed.Value,
                                (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);
                            //My Soulblighter Modifications
                            for (int w = 0; w < dialog.FileNames.Length; w++)
                            {
                                // dialog.Filename replaced by dialog.FileNames[w]
                                Bitmap bmp = new Bitmap(dialog.FileNames[w]);

                                // TODO: fix checking file extension
                                // Gif Especial Properties
                                if (!dialog.FileNames[w].Contains(".gif"))
                                {
                                    continue;
                                }

                                AddAnimationX1(customConvert, bmp);
                            }
                        }
                    }
                }

                // Refresh List after Canvas reduction
                _currentDir = DirectionTrackBar.Value;
                AfterSelectTreeView(null, null);
            }
            catch (OutOfMemoryException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }

        private void OnClickGeneratePalette(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = "Choose images to generate from";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg)|*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                foreach (string filename in dialog.FileNames)
                {
                    Bitmap bit = new Bitmap(filename);
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    if (edit != null)
                    {
                        bit = ConvertBmpAnim(bit, (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);
                        UpdateImagePalette(bit, edit);
                    }
                    SetPaletteBox();
                    FramesListView.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                }
            }
        }
        //End of Soulblighter Modification

        private static unsafe Bitmap ConvertBmpAnim(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = (red / 8 * 1024) + (green / 8 * 32) + (blue / 8) + 32768;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        private void OnClickExportAllToVD(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < AnimationListTreeView.Nodes.Count; ++i)
                {
                    int index = (int)AnimationListTreeView.Nodes[i].Tag;
                    if (index < 0 || AnimationListTreeView.Nodes[i].Parent != null ||
                        AnimationListTreeView.Nodes[i].ForeColor == Color.Red)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"anim{_fileType}_0x{index:X}.vd");
                    AnimationEdit.ExportToVD(_fileType, index, fileName);
                }

                MessageBox.Show($"All Animations saved to {dialog.SelectedPath}",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void CbSaveCoordinates_CheckedChanged(object sender, EventArgs e)
        {
            // Get position of all animations in array
            if (SaveCoordinatesCheckBox.Checked)
            {
                DirectionTrackBar.Enabled = false;
                FramesTrackBar.Value = 0;
                SetCoordinatesButton.Enabled = true;
                for (int count = 0; count < 5;)
                {
                    if (DirectionTrackBar.Value < 4)
                    {
                        _animCx[DirectionTrackBar.Value] = (int)CenterXNumericUpDown.Value;
                        _animCy[DirectionTrackBar.Value] = (int)CenterYNumericUpDown.Value;
                        DirectionTrackBar.Value++;
                        count++;
                    }
                    else
                    {
                        _animCx[DirectionTrackBar.Value] = (int)CenterXNumericUpDown.Value;
                        _animCy[DirectionTrackBar.Value] = (int)CenterYNumericUpDown.Value;
                        DirectionTrackBar.Value = 0;
                        count++;
                    }
                }

                SaveCoordinatesLabel1.Text = $"1: {_animCx[0]}/{_animCy[0]}";
                SaveCoordinatesLabel2.Text = $"2: {_animCx[1]}/{_animCy[1]}";
                SaveCoordinatesLabel3.Text = $"3: {_animCx[2]}/{_animCy[2]}";
                SaveCoordinatesLabel4.Text = $"4: {_animCx[3]}/{_animCy[3]}";
                SaveCoordinatesLabel5.Text = $"5: {_animCx[4]}/{_animCy[4]}";

                DirectionTrackBar.Enabled = true;
            }
            else
            {
                SaveCoordinatesLabel1.Text = "1:    /    ";
                SaveCoordinatesLabel2.Text = "2:    /    ";
                SaveCoordinatesLabel3.Text = "3:    /    ";
                SaveCoordinatesLabel4.Text = "4:    /    ";
                SaveCoordinatesLabel5.Text = "5:    /    ";
                SetCoordinatesButton.Enabled = false;
            }
        }

        private void SetButton_Click(object sender, EventArgs e)
        {
            int originalDir = DirectionTrackBar.Value;
            DirectionTrackBar.Enabled = false;

            int max = DirectionTrackBar.Maximum;
            for (int i = 0; i <= max; i++)
            {
                // positionne la direction courante pour l'itération
                DirectionTrackBar.Value = i;
                _currentDir = i;

                try
                {
                    if (_fileType == 0)
                    {
                        continue;
                    }

                    if (_fileType == UOP_FILE_TYPE)
                    {
                        // UOP: utiliser le gestionnaire UOP et sa structure de frames
                        if (_uopManager == null) continue;

                        var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                        if (uopAnim == null || uopAnim.Frames == null || uopAnim.Frames.Count == 0) continue;

                        for (int fi = 0; fi < uopAnim.Frames.Count; fi++)
                        {
                            uopAnim.ChangeCenter(fi, _animCx[i], _animCy[i]);
                        }

                        uopAnim.IsModified = true;
                        Options.ChangedUltimaClass["Animations"] = true;
                        AnimationPictureBox.Invalidate();
                    }
                    else
                    {
                        // MUL / legacy: protéger l'accès à Frames
                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                        if (edit == null || edit.Frames == null) continue;
                        if (FramesTrackBar.Value >= edit.Frames.Count) continue;

                        foreach (var editFrame in edit.Frames)
                        {
                            editFrame.ChangeCenter(_animCx[i], _animCy[i]);
                        }

                        Options.ChangedUltimaClass["Animations"] = true;
                        AnimationPictureBox.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SetButton_Click error (dir={i}): {ex.Message}");
                }
            }

            // restaure la direction initiale
            DirectionTrackBar.Value = originalDir;
            _currentDir = originalDir;
            DirectionTrackBar.Enabled = true;
        }

        // Gemini Added Methods

        private void OnZoomChanged(object sender, EventArgs e)
        {
            if (ZoomNumericUpDown.SelectedItem == null) return;
            string val = ZoomNumericUpDown.SelectedItem.ToString().Replace("%", "");
            if (float.TryParse(val, out float result))
            {
                _zoomFactor = result / 100.0f;
                AnimationPictureBox.Invalidate();
            }
        }

        private void RelativeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _relativeMode = RelativeCheckBox.Checked;
            if (_relativeMode)
            {
                // Reset NUDs to 0
                _accumulatedRelativeX = 0;
                _accumulatedRelativeY = 0;
                _lastRelativeX = 0;
                _lastRelativeY = 0;
                _updatingUi = true;
                CenterXNumericUpDown.Value = 0;
                CenterYNumericUpDown.Value = 0;
                _updatingUi = false;
            }
            else
            {
                // Restore absolute values? 
                // Actually when unchecking, we usually want to see the current absolute value.
                // Trigger refresh
                AfterSelectTreeView(null, null);
            }
        }

        private void CheckBoxAction_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAction.Checked)
            {
                CheckBoxAllAction.Checked = false;
                CheckBoxAllAction.Enabled = false;
                SameCenterButton.Enabled = false;
            }
            else
            {
                CheckBoxAllAction.Enabled = true;
                SameCenterButton.Enabled = true;
            }
        }

        private void CheckBoxAllAction_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAllAction.Checked)
            {
                CheckBoxAction.Checked = false;
                CheckBoxAction.Enabled = false;
                SameCenterButton.Enabled = false;
            }
            else
            {
                CheckBoxAction.Enabled = true;
                SameCenterButton.Enabled = true;
            }
        }

        private void SecondAnimCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _secondAnimActivated = SecondAnimCheckBox.Checked;
            AnimationPictureBox.Invalidate();
        }

        private void SecondAnimFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SecondAnimFileComboBox.SelectedItem == null) return;
            LoadSecondAnimationFile(SecondAnimFileComboBox.SelectedItem.ToString());
            AnimationPictureBox.Invalidate();
        }

        private void SecondAnimIdNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _secondAnimID = (int)SecondAnimIdNumericUpDown.Value;
            AnimationPictureBox.Invalidate();
        }

        private void SecondAnimPseudoVisuCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _secondAnimPseudoVisu = SecondAnimPseudoVisuCheckBox.Checked;
            AnimationPictureBox.Invalidate();
        }

        private void SecondAnimFirstPlanCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _isSecondAnimInFront = SecondAnimFirstPlanCheckBox.Checked;
            AnimationPictureBox.Invalidate();
        }

        private void ZoomNumericUpDown_Click(object sender, EventArgs e)
        {
            // This might be triggered if configured as button, but we use OnZoomChanged for ComboBox
        }

        private void LoadSecondAnimationFile(string fileName)
        {
            string root = Files.RootDir;

            // Check for UOP
            if (fileName.EndsWith(".uop", StringComparison.OrdinalIgnoreCase))
            {
                string fullPath = System.IO.Path.Combine(root, fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    _secondUopManager = new UopAnimationDataManager();
                    _secondUopManager.LoadUopFiles();
                    _secondUopManager.ProcessUopData();
                    _secondAnimFileIndex = 6; // UOP type
                }
                return;
            }

            // Handle MUL (combobox items are "anim", "anim2" etc. without extension)
            _secondUopManager = null;
            if (fileName.Equals("anim", StringComparison.OrdinalIgnoreCase))
            {
                _secondAnimFileIndex = 1;
            }
            else if (fileName.StartsWith("anim", StringComparison.OrdinalIgnoreCase))
            {
                // anim2, anim3...
                string numPart = fileName.Substring(4);
                if (int.TryParse(numPart, out int idx))
                {
                    _secondAnimFileIndex = idx;
                }
            }
        }

        private int GetAnimationType(int body, int fileType)
        {
            if (fileType == 6) return 3; // UOP Creature mapping
            int length = Animations.GetAnimLength(body, fileType);
            if (length == 13) return 0; // Animal
            if (length == 22) return 1; // Monster
            if (length == 35) return 2; // Human
            return 1;
        }

        private int MapAction(int action, int sourceType, int targetType)
        {
            // Forced Remapping: People (2) <-> UOP Creature (3)

            // Case 1: Main is People (MUL), Second is UOP Creature
            if (sourceType == 2 && targetType == 3)
            {
                if (action == 23) return 29; // Horse_Walk_01 -> MountWalk
                if (action == 24) return 30; // Horse_Run_01 -> MountRun
                if (action == 25) return 31; // Horse_Idle_01 -> MountIdle
            }

            // Case 2: Main is UOP Creature, Second is People (MUL)
            if (sourceType == 3 && targetType == 2)
            {
                // Mapping priority 1
                if (action == 29) return 23; // MountWalk -> Horse_Walk_01
                if (action == 30) return 24; // MountRun -> Horse_Run_01
                if (action == 31) return 25; // MountIdle -> Horse_Idle_01

                // Mapping priority 2 (fallback requests)
                if (action == 22) return 23; // BlockLeft -> Horse_Walk_01
                if (action == 24) return 24; // TakeOff -> Horse_Run_01
                if (action == 25) return 25; // GetHitInAir -> Horse_Idle_01
            }

            if (sourceType == targetType) return action;
            if (sourceType < 0 || sourceType >= _animNames.Length) return action;
            if (targetType < 0 || targetType >= _animNames.Length) return action;
            if (action < 0 || action >= _animNames[sourceType].Length) return 0;

            string actionName = _animNames[sourceType][action];
            for (int i = 0; i < _animNames[targetType].Length; i++)
            {
                if (_animNames[targetType][i].Equals(actionName, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return 0;
        }


        private void DrawSecondAnimation(Graphics graphics)
        {
            if (!_secondAnimActivated) return;

            int mainAnimType = GetAnimationType(_currentBody, _fileType);
            int secondAnimType = GetAnimationType(_secondAnimID, _secondAnimFileIndex);

            int mappedAction = MapAction(_currentAction, mainAnimType, secondAnimType);

            Bitmap frame = null;
            int centerX = 0;
            int centerY = 0;

            if (_secondAnimFileIndex == 6 && _secondUopManager != null)
            {
                // UOP
                var uopAnim = _secondUopManager.GetUopAnimation(_secondAnimID, mappedAction, _currentDir);
                if (uopAnim != null && uopAnim.Frames.Count > 0)
                {
                    int index = FramesTrackBar.Value;
                    if (index >= uopAnim.Frames.Count) index = 0; // Loop or clamp?
                    var f = uopAnim.Frames[index];
                    frame = f.Image;
                    centerX = f.Header.CenterX;
                    centerY = f.Header.CenterY;
                }
            }
            else
            {
                // MUL
                // Use AnimationEdit.GetAnimation with _secondAnimFileIndex
                AnimIdx edit = AnimationEdit.GetAnimation(_secondAnimFileIndex, _secondAnimID, mappedAction, _currentDir);
                if (edit != null)
                {
                    Bitmap[] frames = edit.GetFrames();
                    if (frames != null && frames.Length > 0)
                    {
                        int index = FramesTrackBar.Value;
                        if (index >= frames.Length) index = 0;
                        frame = frames[index];
                        if (frame != null)
                        {
                            centerX = edit.Frames[index].Center.X;
                            centerY = edit.Frames[index].Center.Y;
                        }
                    }
                }
            }

            if (frame != null)
            {
                int x = _framePoint.X - (int)(centerX * _zoomFactor);
                int y = _framePoint.Y - (int)(centerY * _zoomFactor) - (int)(frame.Height * _zoomFactor);

                if (_secondAnimPseudoVisu)
                {
                    // Draw colored overlay (translucent)
                    using (var attr = new ImageAttributes())
                    {
                        ColorMatrix matrix = new ColorMatrix(new float[][]{
                                new float[] {0, 0, 0, 0, 0},
                                new float[] {0, 1, 0, 0, 0}, // Green
                                new float[] {0, 0, 0, 0, 0},
                                new float[] {0, 0, 0, 0.5f, 0}, // Alpha 0.5
                                new float[] {0, 0, 0, 0, 1}
                            });
                        attr.SetColorMatrix(matrix);
                        graphics.DrawImage(frame, new Rectangle(x, y, (int)(frame.Width * _zoomFactor), (int)(frame.Height * _zoomFactor)),
                            0, 0, frame.Width, frame.Height, GraphicsUnit.Pixel, attr);
                    }
                }
                else
                {
                    graphics.DrawImage(frame, new Rectangle(x, y, (int)(frame.Width * _zoomFactor), (int)(frame.Height * _zoomFactor)));
                }
            }
        }

        // Add Directions with Canvas ( CV5 style GIF )
        private void AddDirectionsAddWithCanvasUniqueImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                MessageBox.Show("Unavailable for .UOP format", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in CV5 Style ) to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value, (int)numericUpDownBlue.Value);

                            DirectionTrackBar.Enabled = false;
                            DirectionTrackBar.Value = 0;

                            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);

                            if (edit != null)
                            {
                                // Gif Especial Properties
                                if (dialog.FileName.Contains(".gif"))
                                {
                                    using (Bitmap bmpTemp = new Bitmap(dialog.FileName))
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bmpTemp.Save(ms, ImageFormat.Gif);
                                        Bitmap bitmap = new Bitmap(ms);

                                        FrameDimension dimension = new FrameDimension(bitmap.FrameDimensionsList[0]);

                                        // Number of frames
                                        int frameCount = bitmap.GetFrameCount(dimension);

                                        ProgressBar.Maximum = frameCount;

                                        bitmap.SelectActiveFrame(dimension, 0);

                                        UpdateGifPalette(bitmap, edit);

                                        Bitmap[] bitBmp = new Bitmap[frameCount];

                                        // Return an Image at a certain index
                                        for (int index = 0; index < frameCount; index++)
                                        {
                                            bitBmp[index] = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format16bppArgb1555);
                                            bitmap.SelectActiveFrame(dimension, index);
                                            bitBmp[index] = bitmap;
                                        }

                                        Cv5CanvasAlgorithm(bitBmp, frameCount, dimension, customConvert);

                                        edit = Cv5AnimIdxPositions(frameCount, bitBmp, dimension, edit, bitmap);

                                        ProgressBar.Value = 0;
                                        ProgressBar.Invalidate();

                                        SetPaletteBox();
                                        FramesListView.Invalidate();

                                        CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                                        CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;

                                        Options.ChangedUltimaClass["Animations"] = true;
                                    }
                                }
                            }

                            DirectionTrackBar.Enabled = true;
                        }
                    }
                }

                // Refresh List after Canvas reduction
                _currentDir = DirectionTrackBar.Value;
                AfterSelectTreeView(null, null);
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                DirectionTrackBar.Enabled = true;
            }
        }

        private AnimIdx Cv5AnimIdxPositions(int frameCount, Bitmap[] bitBmp, FrameDimension dimension, AnimIdx edit, Bitmap bmp)
        {
            // position 0
            for (int index = frameCount / 8 * 4; index < frameCount / 8 * 5; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 8 * 5) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 1
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = 0; index < frameCount / 8; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                ListViewItem item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 8) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 2
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 8 * 5; index < frameCount / 8 * 6; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 8 * 6) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 3
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 8 * 1; index < frameCount / 8 * 2; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 8 * 2) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 4
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 8 * 6; index < frameCount / 8 * 7; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }
            }

            return edit;
        }

        private static unsafe Bitmap ConvertBmpAnimCv5(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = (red / 8 * 1024) + (green / 8 * 32) + (blue / 8) + 32768;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != 54965 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        private static readonly Color _greyConvert = Color.FromArgb(255, 170, 170, 170);

        private static void Cv5CanvasAlgorithm(Bitmap[] bitBmp, int frameCount, FrameDimension dimension, Color customConvert)
        {
            // TODO: Needs better names
            // TODO: This code needs documentation. This algorithm is not really readable

            // Order of calls looks important
            // Looks like it is import for Gif/bmps from Diablo cv5 format
            // Some materials about Diablo formats:
            // - https://d2mods.info/resources/infinitum/tut_files/dcc_tutorial/
            // - https://d2mods.info/resources/infinitum/tut_files/dcc_tutorial/chapter4.html
            //
            const int frameDivider = 8;

            // position 0
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 4), GetMaximumFrameIndex(frameCount, frameDivider, 4));
            // position 1
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 0), GetMaximumFrameIndex(frameCount, frameDivider, 0));
            // position 2
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 5), GetMaximumFrameIndex(frameCount, frameDivider, 5));
            // position 3
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 1), GetMaximumFrameIndex(frameCount, frameDivider, 1));
            // position 4
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 6), GetMaximumFrameIndex(frameCount, frameDivider, 6));
        }

        private static int GetInitialFrameIndex(int frameCount, int frameDivider, int position)
        {
            return frameCount / frameDivider * position;
        }

        private static int GetMaximumFrameIndex(int frameCount, int frameDivider, int position)
        {
            return frameCount / frameDivider * (position + 1);
        }

        private static void Cv5ProcessFrames(Bitmap[] bitBmp, FrameDimension dimension, Color customConvert, int initialFrameIndex, int maximumFrameIndex)
        {
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf--;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressT != -1)
                        {
                            top -= regressT;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (yf < bitBmp[initialFrameIndex].Height - 9)
                {
                    yf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Bottom
            for (int yf = bitBmp[initialFrameIndex].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[initialFrameIndex].Height - 1)
                            {
                                regressB++;
                                yf++;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressB == -1 &&
                            yf > 9)
                        {
                            bottom += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressB == -1 &&
                            yf <= 9)
                        {
                            bottom++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressB != -1)
                        {
                            bottom -= regressB;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (yf > 9)
                {
                    yf -= 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Left
            for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf--;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressL == -1 &&
                            xf < bitBmp[0].Width - 9)
                        {
                            left += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressL == -1 &&
                            xf >= bitBmp[0].Width - 9)
                        {
                            left++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 && regressL != -1)
                        {
                            left -= regressL;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (xf < bitBmp[initialFrameIndex].Width - 9)
                {
                    xf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Right
            for (int xf = bitBmp[initialFrameIndex].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[initialFrameIndex].Width - 1)
                            {
                                regressR++;
                                xf++;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressR == -1 &&
                            xf > 9)
                        {
                            right += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressR == -1 &&
                            xf <= 9)
                        {
                            right++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 && regressR != -1)
                        {
                            right -= regressR;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (xf > 9)
                {
                    xf -= 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    //breakOk = false;
                    break;
                }
            }

            for (int index = initialFrameIndex; index < maximumFrameIndex; index++)
            {
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = bitBmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
            }
        }

        private void CbLockColorControls_CheckedChanged(object sender, EventArgs e)
        {
            if (!LockColorControlsCheckBox.Checked)
            {
                numericUpDownRed.Enabled = true;
                numericUpDownGreen.Enabled = true;
                numericUpDownBlue.Enabled = true;
            }
            else
            {
                numericUpDownRed.Enabled = false;
                numericUpDownGreen.Enabled = false;
                numericUpDownBlue.Enabled = false;

                numericUpDownRed.Value = 255;
                numericUpDownGreen.Value = 255;
                numericUpDownBlue.Value = 255;
            }
        }

        // All directions Add KRFrameViewer
        private void AllDirectionsAddWithCanvasKRFrameEditorColorCorrectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType == 6)
            {
                MessageBox.Show("Unavailable for .UOP format", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_fileType == 0)
            {
                return;
            }
        }

        private AnimIdx KrAnimIdxPositions(int frameCount, Bitmap[] bitBmp, FrameDimension dimension, AnimIdx edit, Bitmap bmp)
        {
            // position 0
            for (int index = frameCount / 5 * 0; index < frameCount / 5 * 1; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 5 * 1) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 1
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 5 * 1; index < frameCount / 5 * 2; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 5 * 2) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 2
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 5 * 2; index < frameCount / 5 * 3; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 5 * 3) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 3
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 5 * 3; index < frameCount / 5 * 4; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }

                if (index == (frameCount / 5 * 4) - 1)
                {
                    DirectionTrackBar.Value++;
                }
            }

            // position 4
            edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            UpdateGifPalette(bmp, edit);
            for (int index = frameCount / 5 * 4; index < frameCount / 5 * 5; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int)numericUpDownRed.Value, (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value);
                edit.AddFrame(bitBmp[index], bitBmp[index].Width / 2);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                FramesListView.Items.Add(item);
                int width = FramesListView.TileSize.Width - 5;
                if (bmp.Width > FramesListView.TileSize.Width)
                {
                    width = bmp.Width;
                }

                int height = FramesListView.TileSize.Height - 5;
                if (bmp.Height > FramesListView.TileSize.Height)
                {
                    height = bmp.Height;
                }

                FramesListView.TileSize = new Size(width + 5, height + 5);
                FramesTrackBar.Maximum = i;
                Options.ChangedUltimaClass["Animations"] = true;
                if (ProgressBar.Value < ProgressBar.Maximum)
                {
                    ProgressBar.Value++;
                    ProgressBar.Invalidate();
                }
            }

            return edit;
        }

        private static unsafe Bitmap ConvertBmpAnimKr(Bitmap bmp, int red, int green, int blue)
        {
            // Extra background
            int extraBack = (red / 8 * 1024) + (green / 8 * 32) + (blue / 8) + 32768;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    //if (cur[X] != 53235)
                    //{
                    // Convert back to RGB
                    int blueTemp = (cur[x] - 32768) / 32;
                    blueTemp *= 32;
                    blueTemp = cur[x] - 32768 - blueTemp;

                    int greenTemp = (cur[x] - 32768) / 1024;
                    greenTemp *= 1024;
                    greenTemp = cur[x] - 32768 - greenTemp - blueTemp;
                    greenTemp /= 32;

                    int redTemp = (cur[x] - 32768) / 1024;

                    // remove green colors
                    if (greenTemp > blueTemp && greenTemp > redTemp && greenTemp > 10)
                    {
                        cur[x] = 65535;
                    }
                    //}

                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != 54965 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        private static void KrCanvasAlgorithm(Bitmap[] bitBmp, int frameCount, FrameDimension dimension, Color customConvert)
        {
            /*
             * TODO: both methods needs better names.
             *
             *      Duplication here was huge. Now it is reduced to one method with parameter.
             *      It still needs further reducing.
             *      It may be possible to merge code with CV5 routines.
             */
            // TODO: Needs better names
            // TODO: This code needs documentation. This algorithm is not really readable

            // Order of calls looks important
            // Looks like it is import for Gif/bmps from KR client format
            const int frameDivider = 5;

            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 0), GetMaximumFrameIndex(frameCount, frameDivider, 0));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 1), GetMaximumFrameIndex(frameCount, frameDivider, 1));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 2), GetMaximumFrameIndex(frameCount, frameDivider, 2));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 3), GetMaximumFrameIndex(frameCount, frameDivider, 3));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 4), GetMaximumFrameIndex(frameCount, frameDivider, 4));
        }

        private static void KrProcessFrames(Bitmap[] bitBmp, FrameDimension dimension, Color customConvert, int initialFrameIndex, int maximumFrameIndex)
        {
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf--;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressT != -1)
                        {
                            top -= regressT;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (yf < bitBmp[initialFrameIndex].Height - 9)
                {
                    yf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Bottom
            for (int yf = bitBmp[initialFrameIndex].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[initialFrameIndex].Height - 1)
                            {
                                regressB++;
                                yf++;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressB == -1 &&
                            yf > 9)
                        {
                            bottom += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressB == -1 &&
                            yf <= 9)
                        {
                            bottom++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == maximumFrameIndex - 1 && regressB != -1)
                        {
                            bottom -= regressB;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (yf > 9)
                {
                    yf -= 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Left
            for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf--;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressL == -1 &&
                            xf < bitBmp[0].Width - 9)
                        {
                            left += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressL == -1 &&
                            xf >= bitBmp[0].Width - 9)
                        {
                            left++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 && regressL != -1)
                        {
                            left -= regressL;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (xf < bitBmp[initialFrameIndex].Width - 9)
                {
                    xf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Right
            for (int xf = bitBmp[initialFrameIndex].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[initialFrameIndex].Width - 1)
                            {
                                regressR++;
                                xf++;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressR == -1 &&
                            xf > 9)
                        {
                            right += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 &&
                            regressR == -1 &&
                            xf <= 9)
                        {
                            right++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == maximumFrameIndex - 1 && regressR != -1)
                        {
                            right -= regressR;
                            breakOk = true;
                            break;
                        }
                    }

                    if (breakOk)
                    {
                        break;
                    }
                }

                if (xf > 9)
                {
                    xf -= 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    //breakOk = false;
                    break;
                }
            }

            for (int index = initialFrameIndex; index < maximumFrameIndex; index++)
            {
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = bitBmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
            }
        }

        private void SetPaletteButton_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                // RGB
                if (rbRGB.Checked)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    PaletteConverter(1, edit);
                }

                // RBG
                if (rbRBG.Checked)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    PaletteConverter(2, edit);
                }

                // GRB
                if (rbGRB.Checked)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    PaletteConverter(3, edit);
                }

                // GBR
                if (rbGBR.Checked)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    PaletteConverter(4, edit);
                }

                // BGR
                if (rbBGR.Checked)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    PaletteConverter(5, edit);
                }

                // BRG
                if (rbBRG.Checked)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    PaletteConverter(6, edit);
                }

                SetPaletteBox();
                FramesListView.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (DirectionTrackBar.Value != DirectionTrackBar.Maximum)
                {
                    DirectionTrackBar.Value++;
                }
                else
                {
                    DirectionTrackBar.Value = 0;
                }
            }
        }

        // TODO: check why there is no RadioButton1_CheckedChanged event for selector 1?

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(2);
        }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(3);
        }

        private void RadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(4);
        }

        private void RadioButton5_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(5);
        }

        private void RadioButton6_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(6);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            ConvertAndSetPaletteWithReducer();
        }

        private void ConvertAndSetPaletteWithReducer()
        {
            // TODO: except calling reducer here the whole logic is the same as in ConvertAndSetPalette()
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                PaletteReducer((int)numericUpDown6.Value, (int)numericUpDown7.Value, (int)numericUpDown8.Value, edit);
                SetPaletteBox();
                FramesListView.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (DirectionTrackBar.Value != DirectionTrackBar.Maximum)
                {
                    DirectionTrackBar.Value++;
                }
                else
                {
                    DirectionTrackBar.Value = 0;
                }
            }
        }

        private void ConvertAndSetPalette(int selector)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                PaletteConverter(selector, edit);
                SetPaletteBox();
                FramesListView.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (DirectionTrackBar.Value != DirectionTrackBar.Maximum)
                {
                    DirectionTrackBar.Value++;
                }
                else
                {
                    DirectionTrackBar.Value = 0;
                }
            }
        }

        public void UpdateGifPalette(Bitmap bit, AnimIdx animIdx)
        {
            // Reset palette
            for (int k = 0; k < Animations.PaletteCapacity; k++)
            {
                animIdx.Palette[k] = 0;
            }

            List<Color> entries = new List<Color>();
            bool handled = false;

            // Try to use WPF decoder for GIFs
            if (ImageFormat.Gif.Equals(bit.RawFormat))
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bit.Save(ms, ImageFormat.Gif);
                        ms.Position = 0;
                        GifBitmapDecoder decoder = new GifBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        if (decoder.Palette != null)
                        {
                            entries = decoder.Palette.Colors.Select(c => Color.FromArgb(c.A, c.R, c.G, c.B)).ToList();
                            handled = true;
                        }
                    }
                }
                catch { }
            }

            if (!handled)
            {
                // Use GDI+ palette if available and valid
                if (bit.Palette != null && bit.Palette.Entries.Length > 0)
                {
                    entries = bit.Palette.Entries.ToList();
                }
                else
                {
                    // Generate palette from image content
                    var generated = UoFiddler.Controls.Uop.VdExportHelper.GenerateProperPaletteFromImage(new UoFiddler.Controls.Models.Uop.Imaging.DirectBitmap(bit));
                    entries = generated.Select(c => Color.FromArgb(c.Alpha, c.R, c.G, c.B)).ToList();
                }
            }

            int i = 0;
            while (i < Animations.PaletteCapacity && i < entries.Count)
            {
                int red = entries[i].R / 8;
                int green = entries[i].G / 8;
                int blue = entries[i].B / 8;

                int contaFinal = (0x400 * red) + (0x20 * green) + blue + 0x8000;
                if (contaFinal == 0x8000)
                {
                    contaFinal = 0x8001;
                }

                animIdx.Palette[i] = (ushort)contaFinal;
                i++;
            }

            for (i = 0; i < Animations.PaletteCapacity; i++)
            {
                if (animIdx.Palette[i] < 0x8000)
                {
                    animIdx.Palette[i] = 0x8000;
                }
            }
        }

        public unsafe void UpdateImagePalette(Bitmap bit, AnimIdx animIdx)
        {
            int count = 0;
            var bmp = new Bitmap(bit);
            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int i = 0;
            while (i < Animations.PaletteCapacity)
            {
                animIdx.Palette[i] = 0;
                i++;
            }

            int y = 0;

            while (y < bmp.Height)
            {
                ushort* cur = line;
                for (int x = 0; x < bmp.Width; x++)
                {
                    ushort c = cur[x];
                    if (c == 0)
                    {
                        continue;
                    }

                    bool found = false;
                    i = 0;

                    while (i < animIdx.Palette.Length)
                    {
                        if (animIdx.Palette[i] == c)
                        {
                            found = true;
                            break;
                        }
                        i++;
                    }

                    if (!found)
                    {
                        animIdx.Palette[count++] = c;
                    }

                    if (count >= Animations.PaletteCapacity)
                    {
                        break;
                    }
                }

                for (i = 0; i < Animations.PaletteCapacity; i++)
                {
                    if (animIdx.Palette[i] < 0x8000)
                    {
                        animIdx.Palette[i] = 0x8000;
                    }
                }

                if (count >= Animations.PaletteCapacity)
                {
                    break;
                }

                y++;
                line += delta;
            }
        }

        public void PaletteConverter(int selector, AnimIdx animIdx)
        {
            int i;
            for (i = 0; i < Animations.PaletteCapacity; i++)
            {
                int blueTemp = (animIdx.Palette[i] - 0x8000) / 0x20;
                blueTemp *= 0x20;
                blueTemp = animIdx.Palette[i] - 0x8000 - blueTemp;

                int greenTemp = (animIdx.Palette[i] - 0x8000) / 0x400;
                greenTemp *= 0x400;
                greenTemp = animIdx.Palette[i] - 0x8000 - greenTemp - blueTemp;
                greenTemp /= 0x20;

                int redTemp = (animIdx.Palette[i] - 0x8000) / 0x400;

                int contaFinal = 0;
                switch (selector)
                {
                    case 1:
                        contaFinal = (((0x400 * redTemp) + (0x20 * greenTemp)) + blueTemp) + 0x8000;
                        break;
                    case 2:
                        contaFinal = (((0x400 * redTemp) + (0x20 * blueTemp)) + greenTemp) + 0x8000;
                        break;
                    case 3:
                        contaFinal = (((0x400 * greenTemp) + (0x20 * redTemp)) + blueTemp) + 0x8000;
                        break;
                    case 4:
                        contaFinal = (((0x400 * greenTemp) + (0x20 * blueTemp)) + redTemp) + 0x8000;
                        break;
                    case 5:
                        contaFinal = (((0x400 * blueTemp) + (0x20 * greenTemp)) + redTemp) + 0x8000;
                        break;
                    case 6:
                        contaFinal = (((0x400 * blueTemp) + (0x20 * redTemp)) + greenTemp) + 0x8000;
                        break;
                }

                if (contaFinal == 0x8000)
                {
                    contaFinal = 0x8001;
                }

                animIdx.Palette[i] = (ushort)contaFinal;
            }

            for (i = 0; i < Animations.PaletteCapacity; i++)
            {
                if (animIdx.Palette[i] < 0x8000)
                {
                    animIdx.Palette[i] = 0x8000;
                }
            }
        }

        public void PaletteReducer(int redP, int greenP, int blueP, AnimIdx animIdx)
        {
            int i;
            redP /= 8;
            greenP /= 8;
            blueP /= 8;
            for (i = 0; i < Animations.PaletteCapacity; i++)
            {
                int blueTemp = (animIdx.Palette[i] - 0x8000) / 0x20;
                blueTemp *= 0x20;
                blueTemp = animIdx.Palette[i] - 0x8000 - blueTemp;

                int greenTemp = (animIdx.Palette[i] - 0x8000) / 0x400;
                greenTemp *= 0x400;
                greenTemp = animIdx.Palette[i] - 0x8000 - greenTemp - blueTemp;
                greenTemp /= 0x20;

                int redTemp = (animIdx.Palette[i] - 0x8000) / 0x400;
                redTemp += redP;
                greenTemp += greenP;
                blueTemp += blueP;

                if (redTemp < 0)
                {
                    redTemp = 0;
                }

                if (redTemp > 0x1f)
                {
                    redTemp = 0x1f;
                }

                if (greenTemp < 0)
                {
                    greenTemp = 0;
                }

                if (greenTemp > 0x1f)
                {
                    greenTemp = 0x1f;
                }

                if (blueTemp < 0)
                {
                    blueTemp = 0;
                }

                if (blueTemp > 0x1f)
                {
                    blueTemp = 0x1f;
                }

                int contaFinal = (0x400 * redTemp) + (0x20 * greenTemp) + blueTemp + 0x8000;
                if (contaFinal == 0x8000)
                {
                    contaFinal = 0x8001;
                }

                animIdx.Palette[i] = (ushort)contaFinal;
            }

            for (i = 0; i < Animations.PaletteCapacity; i++)
            {
                if (animIdx.Palette[i] < 0x8000)
                {
                    animIdx.Palette[i] = 0x8000;
                }
            }
        }

        private static Color GetColorFromUltima16Bit(ushort color)
        {
            const int scale = 255 / 31;
            return Color.FromArgb(
                255,
                ((color >> 10) & 0x1F) * scale,
                ((color >> 5) & 0x1F) * scale,
                (color & 0x1F) * scale
                );
        }

        private Models.Uop.UOAnimation CreateUoAnimationFromMulAction(int fileType, int body, int action)
        {
            var allFrames = new List<FrameEntry>();
            var finalPalette = new List<ColourEntry>();
            uint totalFrames = 0;

            for (int dir = 0; dir < 5; dir++)
            {
                AnimIdx edit = AnimationEdit.GetAnimation(fileType, body, action, dir);
                if (edit == null || edit.Frames == null || edit.Frames.Count == 0)
                {
                    continue;
                }

                Bitmap[] bitmaps = edit.GetFrames();
                if (bitmaps == null) continue;

                totalFrames += (uint)bitmaps.Length;

                if (finalPalette.Count == 0)
                {
                    for (int i = 0; i < edit.Palette.Length; i++)
                    {
                        Color c = GetColorFromUltima16Bit(edit.Palette[i]);
                        finalPalette.Add(new ColourEntry(c.R, c.G, c.B, c.A));
                    }
                }

                for (int i = 0; i < bitmaps.Length; i++)
                {
                    if (bitmaps[i] == null) continue;

                    FrameEdit frameEdit = edit.Frames[i];
                    using (Bitmap frameBitmap = new Bitmap(bitmaps[i]))
                    {
                        var frameData = new UopFrameExportData
                        {
                            Image = new DirectBitmap(frameBitmap),
                            Palette = finalPalette,
                            CenterX = (short)frameEdit.Center.X,
                            CenterY = (short)frameEdit.Center.Y,
                            Width = (ushort)frameBitmap.Width,
                            Height = (ushort)frameBitmap.Height,
                            ID = (ushort)body,
                            Frame = (ushort)i
                        };
                        allFrames.Add(new FrameEntry(frameData));
                    }
                }
            }

            if (allFrames.Count == 0)
            {
                return null;
            }

            // Global coords are not available for MULs, so we pass 0.
            return new Models.Uop.UOAnimation((uint)body, action, 0, 0, 0, 0, 0, 0, allFrames, finalPalette, totalFrames);
        }

        private List<string> GetExpandedNodePaths(TreeView treeView)
        {
            var expandedPaths = new List<string>();
            foreach (TreeNode node in treeView.Nodes)
            {
                AddExpandedPaths(node, expandedPaths);
            }
            return expandedPaths;
        }

        private void AddExpandedPaths(TreeNode node, List<string> expandedPaths)
        {
            if (node.IsExpanded)
            {
                expandedPaths.Add(node.FullPath);
            }
            foreach (TreeNode childNode in node.Nodes)
            {
                AddExpandedPaths(childNode, expandedPaths);
            }
        }

        private string? GetSelectedNodePath(TreeView treeView)
        {
            return treeView.SelectedNode?.FullPath;
        }

        private void ExpandNodesByPath(TreeView treeView, List<string> expandedPaths)
        {
            treeView.BeginUpdate();
            foreach (TreeNode node in treeView.Nodes)
            {
                ExpandNodeByPath(node, expandedPaths);
            }
            treeView.EndUpdate();
        }

        private void ExpandNodeByPath(TreeNode node, List<string> expandedPaths)
        {
            if (expandedPaths.Contains(node.FullPath))
            {
                node.Expand();
            }
            foreach (TreeNode childNode in node.Nodes)
            {
                ExpandNodeByPath(childNode, expandedPaths);
            }
        }

        private void SelectNodeByPath(TreeView treeView, string? selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath)) return;

            foreach (TreeNode node in treeView.Nodes)
            {
                TreeNode? foundNode = FindNodeByPath(node, selectedPath);
                if (foundNode != null)
                {
                    treeView.SelectedNode = foundNode;
                    // Ensure the selected node is visible
                    foundNode.EnsureVisible();
                    break;
                }
            }
        }

        private TreeNode? FindNodeByPath(TreeNode node, string targetPath)
        {
            if (node.FullPath == targetPath)
            {
                return node;
            }
            foreach (TreeNode childNode in node.Nodes)
            {
                TreeNode? foundNode = FindNodeByPath(childNode, targetPath);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }
        private void UpdateUopData(int? newCenterX = null, int? newCenterY = null, bool applyToAllFrames = false)
        {
            if (_uopManager == null) return;

            if (applyToAllFrames && (newCenterX.HasValue || newCenterY.HasValue))
            {
                var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                if (uopAnim != null)
                {
                    foreach (var frame in uopAnim.Frames)
                    {
                        if (newCenterX.HasValue) frame.Header.CenterX = (short)newCenterX.Value;
                        if (newCenterY.HasValue) frame.Header.CenterY = (short)newCenterY.Value;
                    }
                    uopAnim.IsModified = true;
                }
            }

            try
            {
                _uopManager.CommitChanges(_currentBody, _currentAction);
                System.Diagnostics.Debug.WriteLine($"✅ UpdateUopData: Commited changes for Body={_currentBody} Action={_currentAction}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating UOP data in memory: {ex.Message}");
            }
        }

        private void SaveUopAnimation()
        {
            if (_uopManager == null) return;

            string outputPath = Options.OutputPath;
            if (string.IsNullOrEmpty(outputPath))
            {
                MessageBox.Show("Output path is not configured. Please set it in Options.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Attempt to find the source file to construct the output filename (default value if unknown)
            string uopFileName = "AnimationFrame1.uop";
            var fileInfo = _uopManager.GetAnimationData(_currentBody, _currentAction, 0);
            if (fileInfo != null && fileInfo.File != null)
            {
                uopFileName = Path.GetFileName(fileInfo.File.FilePath);
            }
            else
            {
                for (int a = 0; a < 100; a++)
                {
                    var fi = _uopManager.GetAnimationData(_currentBody, a, 0);
                    if (fi != null && fi.File != null)
                    {
                        uopFileName = Path.GetFileName(fi.File.FilePath);
                        break;
                    }
                }
            }

            string destinationUopFilePath = Path.Combine(outputPath, uopFileName);

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // IMPORTANT: Do NOT call CommitAllChanges here BEFORE saving.
                // The SaveModifiedAnimationsToUopHybrid method detects modified IDs
                // (param + import-tracking + cache) and writes the grouped files.
                bool success = VdImportHelper.SaveModifiedAnimationsToUopHybrid(_uopManager, -1, destinationUopFilePath);

                if (success)
                {
                    // After writing, the internal state can be switched / reloaded if needed.
                    // CommitAllChanges here ensures memory consistency if necessary.
                    try
                    {
                        _uopManager.CommitAllChanges();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: CommitAllChanges after save failed: {ex.Message}");
                    }

                    Options.ChangedUltimaClass["Animations"] = false;
                    MessageBox.Show($"Animation(s) saved to {destinationUopFilePath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Force reload to apply the written changes.
                    try
                    {
                        _uopManager.ClearCache();
                        LoadUopAnimations();
                    }
                    catch { /* best-effort reload */ }
                }
                else
                {
                    MessageBox.Show("Failed to save animation(s). Check debug logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }



        private void OnClickExportToSpritesheet(object sender, EventArgs e)
        {
            if (_fileType == 0) return;

            using (var form = new PackOptionsForm())
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                // Ask for resize percentage
                double scale = 1.0;
                string input = ShowInputDialog("Enter resize percentage (e.g. 100 for original size, 50 for half size):", "Resize Export", "100");
                if (int.TryParse(input, out int percentage) && percentage > 0 && percentage != 100)
                {
                    scale = percentage / 100.0;
                }

                using (var fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() != DialogResult.OK) return;

                    if (form.ExportAllAnimations)
                    {
                        int actionCount = (_fileType == 6) ? 32 : Animations.GetAnimLength(_currentBody, _fileType);

                        for (int action = 0; action < actionCount; action++)
                        {
                            var frames = GetFramesForExport(_currentBody, action, form.SelectedDirections);
                            if (frames.Count > 0)
                            {
                                if (scale != 1.0) ResizeFrames(frames, scale); // Use helper or inline logic

                                string baseName = $"anim{_fileType}_{_currentBody}_{action:D2}";
                                AnimationPacker.ExportToSpritesheet(frames, fbd.SelectedPath, baseName, form.MaxWidth, form.FrameSpacing, form.OneRowPerDirection);
                            }
                        }
                        MessageBox.Show($"Batch Export complete. (Scale: {scale:P0})", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var frames = GetFramesForExport(_currentBody, _currentAction, form.SelectedDirections);
                        if (frames.Count == 0)
                        {
                            MessageBox.Show("No frames to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (scale != 1.0) ResizeFrames(frames, scale);

                        string baseName = $"anim{_fileType}_{_currentBody}_{_currentAction}";
                        AnimationPacker.ExportToSpritesheet(frames, fbd.SelectedPath, baseName, form.MaxWidth, form.FrameSpacing, form.OneRowPerDirection);
                        MessageBox.Show($"Export complete. (Scale: {scale:P0})", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void ResizeFrames(List<AnimationPacker.FrameInfo> frames, double scale)
        {
            foreach (var frame in frames)
            {
                int newWidth = (int)Math.Max(1, frame.Image.Width * scale);
                int newHeight = (int)Math.Max(1, frame.Image.Height * scale);

                var resized = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(frame.Image, 0, 0, newWidth, newHeight);
                }

                frame.Image.Dispose();
                frame.Image = resized;
                frame.Center = new Point((int)(frame.Center.X * scale), (int)(frame.Center.Y * scale));
            }
        }

        private void OnClickImportFromSpritesheet(object sender, EventArgs e)
        {
            if (_fileType == UOP_FILE_TYPE && _uopManager != null)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "JSON files (*.json)|*.json";
                    ofd.Multiselect = true;
                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    try
                    {
                        // Choose the target UOP file
                        string targetUopPath = null;
                        using (var fileSelectForm = new UopFileSelectionForm(_uopManager.LoadedUopFiles))
                        {
                            if (fileSelectForm.ShowDialog() == DialogResult.OK)
                            {
                                targetUopPath = fileSelectForm.SelectedPath;
                            }
                            else
                            {
                                return;
                            }
                        }

                        int successCount = 0;
                        int failCount = 0;
                        int lastBody = -1;

                        foreach (string file in ofd.FileNames)
                        {
                            try
                            {
                                var info = ParseAnimationFileName(file);
                                int targetBody = _currentBody;
                                int targetAction = (info.action != -1) ? info.action : _currentAction;
                                lastBody = targetBody;

                                var frames = AnimationPacker.ImportFromSpritesheet(file);
                                ImportFramesToAnimation(frames, targetBody, targetAction, targetUopPath);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                failCount++;
                                System.Diagnostics.Debug.WriteLine($"Failed to import {file}: {ex.Message}");
                            }
                        }

                        if (successCount > 0)
                        {
                            // Save in HYBRID mode
                            _uopManager.CommitAllChanges();
                            string destinationPath = Path.Combine(Options.OutputPath, Path.GetFileName(targetUopPath));

                            if (VdImportHelper.SaveModifiedAnimationsToUopHybrid(_uopManager, _currentBody, destinationPath))
                            {
                                MessageBox.Show(
                                    $"✅ {successCount} fichier(s) Spritesheet importés en mode HYBRIDE !\n" +
                                    $"📁 Fichier : {destinationPath}",
                                    "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                _uopManager.ClearCache();
                                LoadUopAnimations();
                                Options.ChangedUltimaClass["Animations"] = false;
                            }
                            else
                            {
                                MessageBox.Show("❌ Import successful but save failed. Check logs.",
                                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                _uopManager.ClearCache();
                                LoadUopAnimations();
                                Options.ChangedUltimaClass["Animations"] = true;
                            }

                            if (lastBody != -1)
                            {
                                TreeNode node = GetNode(lastBody);
                                if (node != null)
                                {
                                    AnimationListTreeView.SelectedNode = node;
                                    node.Expand();
                                    node.EnsureVisible();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error importing Spritesheet: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return;
            }

            // Fallback for MUL
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON files (*.json)|*.json";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() != DialogResult.OK) return;

                int successCount = 0;
                int failCount = 0;
                int lastBody = -1;

                foreach (string file in ofd.FileNames)
                {
                    try
                    {
                        var info = ParseAnimationFileName(file);
                        int targetBody = _currentBody;
                        int targetAction = (info.action != -1) ? info.action : _currentAction;
                        lastBody = targetBody;

                        var frames = AnimationPacker.ImportFromSpritesheet(file);
                        ImportFramesToAnimation(frames, targetBody, targetAction);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        System.Diagnostics.Debug.WriteLine($"Failed to import {file}: {ex.Message}");
                    }
                }

                if (successCount > 0)
                {
                    Options.ChangedUltimaClass["Animations"] = true;
                    LoadMulAnimations();

                    if (lastBody != -1)
                    {
                        TreeNode node = GetNode(lastBody);
                        if (node != null)
                        {
                            AnimationListTreeView.SelectedNode = node;
                            node.Expand();
                            node.EnsureVisible();
                        }
                    }
                }

                if (failCount == 0)
                {
                    MessageBox.Show($"Successfully imported {successCount} spritesheets.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Imported {successCount} spritesheets.\nFailed to import {failCount} files.", "Import Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void OnClickExportFramesXml(object sender, EventArgs e)
        {
            if (_fileType == 0) return;

            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;

                var frames = GetFramesForExport(_currentBody, _currentAction, new List<int> { 0, 1, 2, 3, 4 });
                if (frames.Count == 0)
                {
                    MessageBox.Show("No frames to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string baseName = $"anim{_fileType}_{_currentBody}_{_currentAction}";
                AnimationPacker.ExportToFramesXml(frames, fbd.SelectedPath, baseName);
                MessageBox.Show("Export complete.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClickExportAllFramesXml(object sender, EventArgs e)
        {
            if (_fileType == 0) return;

            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;

                // Ask for resize percentage
                double scale = 1.0;
                string input = ShowInputDialog("Enter resize percentage (e.g. 100 for original size, 50 for half size):", "Resize Export", "100");
                if (int.TryParse(input, out int percentage) && percentage > 0 && percentage != 100)
                {
                    scale = percentage / 100.0;
                }

                int actionCount = (_fileType == 6) ? 32 : Animations.GetAnimLength(_currentBody, _fileType);

                for (int action = 0; action < actionCount; action++)
                {
                    var frames = GetFramesForExport(_currentBody, action, new List<int> { 0, 1, 2, 3, 4 });
                    if (frames.Count > 0)
                    {
                        // Apply scaling if requested
                        if (scale != 1.0)
                        {
                            foreach (var frame in frames)
                            {
                                int newWidth = (int)Math.Max(1, frame.Image.Width * scale);
                                int newHeight = (int)Math.Max(1, frame.Image.Height * scale);

                                var resized = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
                                using (Graphics g = Graphics.FromImage(resized))
                                {
                                    // Use NearestNeighbor to preserve original palette colors and avoid blur/artifacts
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                                    g.DrawImage(frame.Image, 0, 0, newWidth, newHeight);
                                }

                                // Features original clone and replace with resized
                                frame.Image.Dispose();
                                frame.Image = resized;

                                // Scale center coordinates
                                frame.Center = new Point((int)(frame.Center.X * scale), (int)(frame.Center.Y * scale));
                            }
                        }

                        string baseName = $"anim{_fileType}_{_currentBody}_{action:D2}";
                        AnimationPacker.ExportToFramesXml(frames, fbd.SelectedPath, baseName);
                    }
                }
                MessageBox.Show($"Batch Export complete. (Scale: {scale:P0})", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClickImportFramesXml(object sender, EventArgs e)
        {
            if (_fileType == UOP_FILE_TYPE && _uopManager != null)
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "XML files (*.xml)|*.xml";
                    ofd.Multiselect = true;
                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    try
                    {
                        // Choose the target UOP file
                        string targetUopPath = null;
                        using (var fileSelectForm = new UopFileSelectionForm(_uopManager.LoadedUopFiles))
                        {
                            if (fileSelectForm.ShowDialog() == DialogResult.OK)
                            {
                                targetUopPath = fileSelectForm.SelectedPath;
                            }
                            else
                            {
                                return;
                            }
                        }

                        int successCount = 0;
                        int failCount = 0;
                        int lastBody = -1;

                        foreach (string file in ofd.FileNames)
                        {
                            try
                            {
                                var info = ParseAnimationFileName(file);
                                int targetBody = _currentBody;
                                int targetAction = (info.action != -1) ? info.action : _currentAction;
                                lastBody = targetBody;

                                var frames = AnimationPacker.ImportFromFramesXml(file);
                                ImportFramesToAnimation(frames, targetBody, targetAction, targetUopPath);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                failCount++;
                                System.Diagnostics.Debug.WriteLine($"Failed to import {file}: {ex.Message}");
                            }
                        }

                        if (successCount > 0)
                        {
                            // Save in HYBRID mode (as for the VD)
                            _uopManager.CommitAllChanges();
                            string destinationPath = Path.Combine(Options.OutputPath, Path.GetFileName(targetUopPath));

                            if (VdImportHelper.SaveModifiedAnimationsToUopHybrid(_uopManager, _currentBody, destinationPath))
                            {
                                MessageBox.Show(
                                    $"✅ {successCount} fichier(s) Spritesheet importés en mode HYBRIDE !\n" +
                                    $"📁 Fichier : {destinationPath}",
                                    "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                _uopManager.ClearCache(); // Force reload to update Bounding Boxes
                                LoadUopAnimations();
                                Options.ChangedUltimaClass["Animations"] = false;
                            }
                            else
                            {
                                MessageBox.Show("❌ Import successful but save failed. Check logs.",
                                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                LoadUopAnimations();
                                Options.ChangedUltimaClass["Animations"] = true;
                            }

                            if (lastBody != -1)
                            {
                                TreeNode node = GetNode(lastBody);
                                if (node != null)
                                {
                                    AnimationListTreeView.SelectedNode = node;
                                    node.Expand();
                                    node.EnsureVisible();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error importing XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return;
            }

            // Fallback for MUL files (original behavior)
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "XML files (*.xml)|*.xml";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() != DialogResult.OK) return;

                int successCount = 0;
                int failCount = 0;

                int lastBody = -1;

                foreach (string file in ofd.FileNames)
                {
                    try
                    {
                        var info = ParseAnimationFileName(file);
                        int targetBody = _currentBody;
                        int targetAction = (info.action != -1) ? info.action : _currentAction;
                        lastBody = targetBody;

                        var frames = AnimationPacker.ImportFromFramesXml(file);
                        ImportFramesToAnimation(frames, targetBody, targetAction);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        System.Diagnostics.Debug.WriteLine($"Failed to import {file}: {ex.Message}");
                        if (failCount == 1)
                        {
                            MessageBox.Show($"Error importing file {Path.GetFileName(file)}:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Import Error Detail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                if (successCount > 0)
                {
                    Options.ChangedUltimaClass["Animations"] = true;
                    LoadMulAnimations();

                    if (lastBody != -1)
                    {
                        TreeNode node = GetNode(lastBody);
                        if (node != null)
                        {
                            AnimationListTreeView.SelectedNode = node;
                            node.Expand();
                            node.EnsureVisible();
                        }
                    }
                }

                if (failCount == 0)
                {
                    MessageBox.Show($"Successfully imported {successCount} XML files.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Imported {successCount} XML files.\nFailed to import {failCount} files.", "Import Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private List<AnimationPacker.FrameInfo> GetFramesForExport(int body, int action, List<int> directions)
        {
            var result = new List<AnimationPacker.FrameInfo>();

            if (_fileType == 6) // UOP
            {
                if (_uopManager == null) return result;
                foreach (int dir in directions)
                {
                    var uopAnim = _uopManager.GetUopAnimation(body, action, dir);
                    if (uopAnim != null)
                    {
                        for (int i = 0; i < uopAnim.Frames.Count; i++)
                        {
                            result.Add(new AnimationPacker.FrameInfo
                            {
                                Image = new Bitmap(uopAnim.Frames[i].Image),
                                Center = new Point(uopAnim.Frames[i].Header.CenterX, uopAnim.Frames[i].Header.CenterY),
                                Direction = dir,
                                Index = i
                            });
                        }
                    }
                }
            }
            else // MUL
            {
                foreach (int dir in directions)
                {
                    AnimIdx anim = AnimationEdit.GetAnimation(_fileType, body, action, dir);
                    if (anim != null)
                    {
                        Bitmap[] bitmaps = anim.GetFrames();
                        if (bitmaps != null)
                        {
                            for (int i = 0; i < bitmaps.Length; i++)
                            {
                                var frame = anim.Frames[i];
                                result.Add(new AnimationPacker.FrameInfo
                                {
                                    Image = new Bitmap(bitmaps[i]),
                                    Center = frame.Center,
                                    Direction = dir,
                                    Index = i
                                });
                            }
                        }
                    }
                }
            }
            return result;
        }

        private (int type, int body, int action) ParseAnimationFileName(string fileName)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(fileName);
                var match = System.Text.RegularExpressions.Regex.Match(name, @"anim(\d+)_(\d+)_(\d+)");
                if (match.Success)
                {
                    return (
                        int.Parse(match.Groups[1].Value),
                        int.Parse(match.Groups[2].Value),
                        int.Parse(match.Groups[3].Value)
                    );
                }
            }
            catch { }
            return (-1, -1, -1);
        }

        private void ImportFramesToAnimation(List<AnimationPacker.FrameInfo> frames, int body = -1, int action = -1, string targetUopPath = null)
        {
            int targetBody = (body != -1) ? body : _currentBody;
            int targetAction = (action != -1) ? action : _currentAction;

            // STEP 1: Prepare the data (Grouping by direction for MUL)
            var framesByDir = frames.GroupBy(f => f.Direction).ToList();

            foreach (var group in framesByDir)
            {
                int direction = group.Key;

                if (_fileType == 6) // UOP
                {
                    // UOP Logic (Existing)
                    var uopAnim = _uopManager.GetUopAnimation(targetBody, targetAction, direction);
                    if (uopAnim == null)
                    {
                        uopAnim = _uopManager.CreateNewUopAnimation(targetBody, targetAction, direction, targetUopPath);
                    }

                    foreach (var frame in group)
                    {
                        while (uopAnim.Frames.Count <= frame.Index)
                        {
                            var dummy = new UoFiddler.Controls.Uop.DecodedUopFrame();
                            dummy.Image = new Bitmap(1, 1);
                            uopAnim.Frames.Add(dummy);
                        }

                        var newFrame = new UoFiddler.Controls.Uop.DecodedUopFrame();
                        newFrame.Image = new Bitmap(frame.Image);
                        newFrame.Header = new UoFiddler.Controls.Uop.UopFrameHeader
                        {
                            Width = (ushort)newFrame.Image.Width,
                            Height = (ushort)newFrame.Image.Height,
                            CenterX = (short)frame.Center.X,
                            CenterY = (short)frame.Center.Y
                        };

                        var paletteEntries = UoFiddler.Controls.Uop.VdExportHelper.GenerateProperPaletteFromImage(new UoFiddler.Controls.Models.Uop.Imaging.DirectBitmap(newFrame.Image));
                        newFrame.Palette = paletteEntries.Select(p => Color.FromArgb(p.Alpha, p.R, p.G, p.B)).ToList();

                        uopAnim.Frames[frame.Index] = newFrame;
                        uopAnim.IsModified = true;
                    }
                }
                else // MUL
                {
                    AnimIdx anim = AnimationEdit.GetAnimation(_fileType, targetBody, targetAction, direction);
                    if (anim == null) continue;

                    // PALETTE GENERATION (Fix for Empty Frames / Crash)
                    // Check if we need a new palette (if current is empty/black)
                    bool needsPalette = true;
                    for (int i = 0; i < 256; i++)
                    {
                        if (anim.Palette[i] != 0)
                        {
                            needsPalette = false; // Palette exists, do not overwrite (unless forced? Let's assume strict append/replace uses existing)
                            break;
                        }
                    }

                    // If it's a new animation (empty palette), generate one from ALL frames in this group
                    if (needsPalette)
                    {
                        var allImages = group.Select(f => f.Image).ToList();
                        ushort[] newPalette = GenerateMulPalette(allImages);
                        anim.ReplacePalette(newPalette);
                    }

                    // Process Frames
                    foreach (var frame in group)
                    {
                        int currentCount = (anim.Frames != null) ? anim.Frames.Count : 0;

                        // Convert to safe 16bpp
                        using (Bitmap bmp16 = ConvertToUltima16Bpp(frame.Image))
                        {
                            if (frame.Index < currentCount)
                            {
                                // Update Center BEFORE creating FrameEdit (ReplaceFrame uses current Center)
                                // If we update after, the RawData offsets (baked with old center) + New Center causes GetFrames to write out of bounds.
                                anim.Frames[frame.Index].Center = frame.Center;
                                anim.ReplaceFrame(bmp16, frame.Index);
                            }
                            else
                            {
                                while (currentCount < frame.Index)
                                {
                                    using (Bitmap dummy = new Bitmap(1, 1, PixelFormat.Format16bppArgb1555))
                                    {
                                        anim.AddFrame(dummy, 0, 0);
                                    }
                                    currentCount++;
                                }

                                anim.AddFrame(bmp16, frame.Center.X, frame.Center.Y);
                            }
                        }
                    }
                }
            }

            // STEP 2: Update the directory tree WITHOUT clearing the cache
            if (_fileType == 6) // UOP
            {
                Options.ChangedUltimaClass["Animations"] = true;

                // NO LoadUopAnimations() here! We're just updating the nodes manually.
                AnimationListTreeView.BeginUpdate();
                try
                {
                    // Check if the Body already exists in the directory tree
                    TreeNode bodyNode = GetNode(targetBody);

                    if (bodyNode == null)
                    {
                        // Create a new Body node if it does not already exist
                        string mappingInfo = GetUopMulMapping(targetBody);
                        bodyNode = new TreeNode
                        {
                            Tag = (ushort)targetBody,
                            Text = $"UOP ID: {targetBody}{mappingInfo}",
                            ForeColor = Color.Black
                        };
                        AnimationListTreeView.Nodes.Add(bodyNode);
                        System.Diagnostics.Debug.WriteLine($"✅ Created new Body node: {targetBody}");
                    }
                    else
                    {
                        // The bodysuit exists, switch to black (valid)
                        bodyNode.ForeColor = Color.Black;
                    }

                    // Check if the Action already exists in this Body
                    TreeNode actionNode = null;
                    foreach (TreeNode child in bodyNode.Nodes)
                    {
                        if (child.Tag is int actionTag && actionTag == targetAction)
                        {
                            actionNode = child;
                            break;
                        }
                    }

                    if (actionNode == null)
                    {
                        // CREATE a new Action node if it does not exist
                        actionNode = new TreeNode
                        {
                            Tag = targetAction,
                            Text = $"{targetAction:D2}_{GetActionDescription(targetBody, targetAction)} (Imported)",
                            ForeColor = Color.Black
                        };
                        bodyNode.Nodes.Add(actionNode);
                        System.Diagnostics.Debug.WriteLine($"✅ Created new Action node: {targetAction} for Body {targetBody}");
                    }
                    else
                    {
                        // The action exists, switch to black (valid)
                        actionNode.ForeColor = Color.Black;
                        actionNode.Text = $"{targetAction:D2}_{GetActionDescription(targetBody, targetAction)} (Modified)";
                    }

                    // Expand le Body node pour montrer la nouvelle action
                    bodyNode.Expand();
                }
                finally
                {
                    AnimationListTreeView.EndUpdate();
                }
            }
            else // MUL
            {
                TreeNode node = GetNode(targetBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    if (targetAction >= 0 && targetAction < node.Nodes.Count)
                    {
                        node.Nodes[targetAction].ForeColor = Color.Black;
                    }
                    node.Expand();
                    node.EnsureVisible();
                }
            }

            // ✅ ÉTAPE 3 : Rafraîchir l'affichage si c'est l'animation actuellement sélectionnée
            if (targetBody == _currentBody && targetAction == _currentAction)
            {
                DisplayUopAnimation();
                FramesListView.Invalidate();
                AnimationPictureBox.Invalidate();
            }
        }

        private ushort[] GenerateMulPalette(List<Bitmap> images)
        {
            HashSet<ushort> uniqueColors = new HashSet<ushort>();
            uniqueColors.Add(0); // Ensure transparency is present

            foreach (var img in images)
            {
                if (img == null) continue;

                // Convert to 16bpp first to ensure we get the exact colors Ultima will see
                using (var bmp16 = ConvertToUltima16Bpp(img))
                {
                    BitmapData bd = bmp16.LockBits(new Rectangle(0, 0, bmp16.Width, bmp16.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                    unsafe
                    {
                        ushort* ptr = (ushort*)bd.Scan0;
                        int stride = bd.Stride / 2;
                        for (int y = 0; y < bmp16.Height; y++)
                        {
                            for (int x = 0; x < bmp16.Width; x++)
                            {
                                ushort color = ptr[y * stride + x];
                                if (color != 0) // Ignore transparent, already added
                                {
                                    uniqueColors.Add(color);
                                }
                            }
                        }
                    }
                    bmp16.UnlockBits(bd);
                }

                if (uniqueColors.Count >= 256) break; // Limit reached (optimization)
            }

            ushort[] palette = new ushort[256];
            int i = 0;
            foreach (var c in uniqueColors.Take(256))
            {
                palette[i++] = c;
            }
            return palette;
        }

        private Bitmap ConvertToUltima16Bpp(Bitmap source)
        {
            if (source == null || source.Width == 0 || source.Height == 0)
            {
                return new Bitmap(1, 1, PixelFormat.Format16bppArgb1555);
            }

            try
            {
                // Use Clone to safely convert pixel formats (handles Indexed -> RGB and 32bpp -> 16bpp)
                return source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format16bppArgb1555);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting bitmap: {ex.Message}");
                // Fallback: create a blank 16bpp bitmap and try to draw (slow but safe fallback, though Clone usually works)
                Bitmap bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format16bppArgb1555);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    // DrawImage handles format conversion too, but might OOM on Indexed source if we tried to create Graphics FROM source. 
                    // Here we create Graphics FROM dest (16bpp), which is safe.
                    g.DrawImage(source, 0, 0, source.Width, source.Height);
                }
                return bmp;
            }
        }

        #region [ Edit Ultima Online Bodyconv.def and mobtypes.txt ]
        private EditUoBodyconvMobtypes editUoBodyconvMobtypesForm;

        private void editUoBodyconvAndMobtypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the form is already open
            if (editUoBodyconvMobtypesForm == null || editUoBodyconvMobtypesForm.IsDisposed)
            {
                // Open form if it is not already open
                editUoBodyconvMobtypesForm = new EditUoBodyconvMobtypes();
                editUoBodyconvMobtypesForm.Show();
            }
            else
            {
                // Bring form to foreground if it is already open
                editUoBodyconvMobtypesForm.BringToFront();
            }

            // Set the value of textBoxID in the EditUoBodyconvMobtypes form
            editUoBodyconvMobtypesForm.TextBoxID = _currentBody.ToString(); // ID
            editUoBodyconvMobtypesForm.TextBoxBody = BodyConverter.GetTrueBody(_fileType, _currentBody).ToString(); //Body ID
        }
        #endregion

        #region [ Copy image to Clipboard ]
        private void copyFrameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FramesListView.SelectedItems.Count == 0)
                return;

            int frameIndex = (int)FramesListView.SelectedItems[0].Tag;

            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
                return;

            Bitmap[] frames = edit.GetFrames();
            if (frames == null || frameIndex >= frames.Length)
                return;

            Bitmap frame = frames[frameIndex];

            // Frame in Zwischenablage kopieren
            Clipboard.SetImage(frame);

            MessageBox.Show("The selected frame was successfully copied to the clipboard.", "Copy Frame", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region [ Import Image from Clipboard ]
        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage() && FramesListView.SelectedItems.Count > 0)
            {
                int frameIndex = (int)FramesListView.SelectedItems[0].Tag;

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                    return;

                Bitmap newFrame = (Bitmap)Clipboard.GetImage();

                edit.ReplaceFrame(newFrame, frameIndex);

                FramesListView.Items[frameIndex].ImageIndex = frameIndex;
                FramesListView.Invalidate();

                Options.ChangedUltimaClass["Animations"] = true;

                MessageBox.Show("Frame image imported from clipboard");
            }
        }

        private void FramesImportandCopyListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                importImageToolStripMenuItem_Click(null, null);
            }

            if (e.Control && e.KeyCode == Keys.X)
            {
                copyFrameToClipboardToolStripMenuItem_Click(null, null);
            }
        }
        #endregion

        #region [ Mirror Image ]
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FramesListView.SelectedItems.Count > 0)
            {
                int frameIndex = (int)FramesListView.SelectedItems[0].Tag;

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null) return;

                // Bitmap loaded
                Bitmap frame = edit.GetFrames()[frameIndex];

                // mirror image
                frame.RotateFlip(RotateFlipType.RotateNoneFlipX);

                // replace frames
                edit.ReplaceFrame(frame, frameIndex);

                // Rebuild ListView
                FramesListView.BeginUpdate();
                FramesListView.Items.Clear();
                // Reload all frames
                Bitmap[] frames = edit.GetFrames();
                for (int i = 0; i < frames.Length; i++)
                {
                    ListViewItem item = new ListViewItem(i.ToString()) { Tag = i };
                    if (frames[i] != null)
                    {
                        item.ImageIndex = i;
                    }
                    FramesListView.Items.Add(item);
                }
                FramesListView.EndUpdate();

                // update display
                FramesListView.Items[frameIndex].Selected = true;
                FramesListView.Select();

                Options.ChangedUltimaClass["Animations"] = true;

                MessageBox.Show("Frame mirrored");
            }
        }
        #endregion

        #region [ RotateLeft90Degrees ]
        private void rotateLeft90DegreesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FramesListView.SelectedItems.Count > 0)
            {
                int index = (int)FramesListView.SelectedItems[0].Tag;

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);

                if (edit != null)
                {
                    Bitmap frame = edit.GetFrames()[index];

                    if (frame != null)
                    {
                        frame.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        edit.ReplaceFrame(frame, index);
                    }
                }

                FramesListView.Invalidate();
            }
            // Mark change
            Options.ChangedUltimaClass["Animations"] = true;
        }
        #endregion

        #region [ Search Animation ] toolStripTextBoxSearch_TextChanged
        private void toolStripTextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            var searchText = toolStripTextBoxSearch.Text;
            foreach (TreeNode node in AnimationListTreeView.Nodes)
            {
                if (node.Tag.ToString() == searchText)
                {
                    AnimationListTreeView.SelectedNode = node;
                    break;
                }
            }
        }
        #endregion

        #region [ Find IDs ]
        private async void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show progress bars
            ProgressBar.Visible = true;
            ProgressBar.Maximum = AnimationListTreeView.Nodes.Count;
            ProgressBar.Value = 0;

            await Task.Run(() =>
            {
                // Iterate over TreeView Nodes
                foreach (TreeNode node in AnimationListTreeView.Nodes)
                {
                    int bodyIndex = (int)node.Tag;

                    // Check Action 0
                    if (!AnimationEdit.IsActionDefined(_fileType, bodyIndex, 0))
                    {
                        // Wenn undefined, ist Slot frei
                        this.Invoke(new Action(() =>
                        {
                            node.ForeColor = Color.Blue;
                            node.Text += " - FREE";
                        }));
                    }

                    // Update progress bars
                    this.Invoke(new Action(() =>
                    {
                        ProgressBar.Value++;
                    }));
                }
            });

            // Hide progress bars
            ProgressBar.Visible = false;

            AnimationListTreeView.Invalidate();
        }
        #endregion

        #region [ listsAllIDsToolStripMenuItem ]
        private void listsAllIDsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string defaultFileName = "AminID.txt"; // Set default file name

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "Text file (*.txt)|*.txt",
                Title = "Save the ID overview"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                DialogResult result = MessageBox.Show("Would you also like to list the free IDs?",
                                                      "ID selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                bool includeFreeIDs = result == DialogResult.Yes; // If "Yes", free IDs are also listed

                // Counter for used and free IDs
                int occupiedCount = 0;
                int freeCount = 0;

                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("Overview of IDs:");
                        writer.WriteLine("--------------------");

                        // Looping through all nodes in the AnimationListTreeView
                        foreach (TreeNode node in AnimationListTreeView.Nodes)
                        {
                            if (node.Tag != null)
                            {
                                int id = (int)node.Tag;
                                bool valid = false;

                                // Check all actions for this ID
                                foreach (TreeNode actionNode in node.Nodes)
                                {
                                    int actionId = (int)actionNode.Tag;
                                    if (AnimationEdit.IsActionDefined(_fileType, id, actionId))
                                    {
                                        valid = true; // At least one action defined
                                        break;
                                    }
                                }

                                if (valid)
                                {
                                    writer.WriteLine($"ID: {id} - Status: Occupied");
                                    occupiedCount++;
                                }
                                else if (includeFreeIDs)
                                {
                                    writer.WriteLine($"ID: {id} - Status: Not occupied");
                                    freeCount++;
                                }
                            }
                        }

                        writer.WriteLine("\n--------------------");
                        writer.WriteLine("Summary:");
                        writer.WriteLine($"Occupied IDs: {occupiedCount}");
                        if (includeFreeIDs)
                        {
                            writer.WriteLine($"Free IDs: {freeCount}");
                        }
                    }

                    MessageBox.Show("The ID overview was saved successfully.", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region [ showToolStripMenuItem ]
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showOnlyValid = !_showOnlyValid;

            if (_showOnlyValid)
            {
                AnimationListTreeView.BeginUpdate();
                try
                {
                    for (int i = AnimationListTreeView.Nodes.Count - 1; i >= 0; --i)
                    {
                        if (AnimationListTreeView.Nodes[i].ForeColor == Color.Red)
                        {
                            AnimationListTreeView.Nodes[i].Remove();
                        }
                    }
                }
                finally
                {
                    AnimationListTreeView.EndUpdate();
                }
            }
            else
            {
                OnLoad(null);
            }
        }
        #endregion

        #region [ saveToolStripMenuItem1 ] // Save current file
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            AnimationEdit.Save(_fileType, Options.OutputPath);
            Options.ChangedUltimaClass["Animations"] = false;

            MessageBox.Show($"AnimationFile saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnCheckBoxIDBlueChanged ] - Recheck for loadable animations and update TreeView accordingly
        private void OnCheckBoxIDBlueChanged(object sender, EventArgs e)
        {
            if (_fileType == UOP_FILE_TYPE)
            {
                LoadUopAnimations();
            }
            else if (_fileType != 0)
            {
                LoadMulAnimations();
            }
        }
        #endregion

        #region [ checkBoxMount_CheckedChanged ]
        private void checkBoxMount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAnimations();
        }
        #endregion

        #region [ UpdateAnimation ]
        private void UpdateAnimations()
        {
            if (isAnimationVisible)
            {
                // Animation ausblenden
                additionalAnimation = null;
                buttonShow.Text = "Show";
            }
            else
            {
                // Show animation based on selected gender
                string selectedGender = comboBoxMenWoman.SelectedItem.ToString();
                int animId = selectedGender == "men" ? 400 : 401;

                if (checkBoxMount.Checked)
                {
                    _currentAction = 24; // Riding animation ID
                }

                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);
                buttonShow.Text = "Hide";
            }

            isAnimationVisible = !isAnimationVisible;
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ buttonShow ] animation man woman
        private void buttonShow_Click(object sender, EventArgs e)
        {
            if (isAnimationVisible)
            {
                // Hide animation
                additionalAnimation = null;
                buttonShow.Text = "Show";
            }
            else
            {
                // Show animation based on selected gender
                string selectedGender = comboBoxMenWoman.SelectedItem.ToString();
                int animId = selectedGender == "men" ? 400 : 401;

                if (checkBoxMount.Checked)
                {
                    _currentAction = 24; // Sets the action to 24 when riding animation is selected
                }

                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);
                buttonShow.Text = "Hide";
            }

            isAnimationVisible = !isAnimationVisible;
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ comboBoxMenWoman_SelectedIndexChanged ] animation man woman
        private void comboBoxMenWoman_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isAnimationVisible)
            {
                string selectedGender = comboBoxMenWoman.SelectedItem.ToString();
                int animId = selectedGender == "men" ? 400 : 401;

                if (checkBoxMount.Checked)
                {
                    _currentAction = 24; // Sets the action to 24 when riding animation is selected
                }

                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);
                AnimationPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ btnUp ]
        private void btnUp_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;

            if (checkBoxMount.Checked)
            {
                HorseRunOffsets[direction][frame][1]--;
            }
            else
            {
                Offsets[direction][frame][1]--;
            }
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btnDown ]
        private void btnDown_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;

            if (checkBoxMount.Checked)
            {
                HorseRunOffsets[direction][frame][1]++;
            }
            else
            {
                Offsets[direction][frame][1]++;
            }
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btnLeft ]
        private void btnLeft_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;

            if (checkBoxMount.Checked)
            {
                HorseRunOffsets[direction][frame][0]--;
            }
            else
            {
                Offsets[direction][frame][0]--;
            }
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btnRight ]
        private void btnRight_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;

            if (checkBoxMount.Checked)
            {
                HorseRunOffsets[direction][frame][0]++;
            }
            else
            {
                Offsets[direction][frame][0]++;
            }
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btn_ScreenShot ]
        private void btn_ScreenShot_Click(object sender, EventArgs e)
        {
            // Define the path to save the screenshot
            string path = Options.OutputPath;

            // Create a unique filename using the current date and time
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = Path.Combine(path, $"AnimationScreenshot_{timestamp}.png");

            // Create a bitmap of the same size as the AnimationPictureBox
            using (Bitmap bmp = new Bitmap(AnimationPictureBox.Width, AnimationPictureBox.Height))
            {
                // Draw the AnimationPictureBox onto the bitmap
                AnimationPictureBox.DrawToBitmap(bmp, new Rectangle(0, 0, AnimationPictureBox.Width, AnimationPictureBox.Height));

                // Save the bitmap as a .png file
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
            }

            MessageBox.Show($"Screenshot saved to {fileName}");
        }
        #endregion

        #region [ MapUopToolStripButton ] Toggle UOP Mapping and reload animations
        private void MapUopToolStripButton_Click(object sender, EventArgs e)
        {
            _useUopMapping = MapUopToolStripButton.Checked;
            if (_uopManager != null)
            {
                _uopManager.IgnoreAnimationSequence = !_useUopMapping;
                LoadUopAnimations();
            }
        }
        #endregion


        #region [ ShowCrosshairtoolStripButton ] Toggle crosshair display on animation preview
        private void ShowCrosshairtoolStripButton_Click(object sender, EventArgs e)
        {
            _drawCrosshair = !_drawCrosshair;
            AnimationPictureBox.Invalidate();
        }
        #endregion


        #region Sequence UOP Tab

        private void InitializeSequenceTab()
        {
            _sequenceGrid.Columns.Clear();
            // UOP columns as standard
            AddUopColumns();
            _sequenceGrid.DataSource = _sequenceBindingSource;
            _sequenceTimer.Interval = 150;
            _btnSaveMainMisc.Enabled = false;
            _gridIsInMulMode = false;
        }

        // ── Grid-Modus ────────────────────────────────────────────────────────

        private bool _gridIsInMulMode = false;

        /// <summary>
        /// Switch the grid to MUL columns.
        /// Always executed if _fileType is MUL — no early return.
        /// </summary>
        private void SetupSequenceGridForMul()
        {
            _sequenceGrid.DataSource = null;
            _sequenceGrid.Columns.Clear();
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ActionIndex", HeaderText = "Act", Width = 35, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ActionName", HeaderText = "Name", Width = 130, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FramesDir0", HeaderText = "D0", Width = 35, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FramesDir1", HeaderText = "D1", Width = 35, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FramesDir2", HeaderText = "D2", Width = 35, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FramesDir3", HeaderText = "D3", Width = 35, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FramesDir4", HeaderText = "D4", Width = 35, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CenterX", HeaderText = "CX", Width = 40, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CenterY", HeaderText = "CY", Width = 40, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IdxOffset", HeaderText = "IDX-Off", Width = 80, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IdxLength", HeaderText = "IDX-Len", Width = 70, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StatusText", HeaderText = "Status", Width = 90, ReadOnly = true });
            _gridIsInMulMode = true;
        }

        /// <summary>
        /// Switch the grid to UOP columns.
        /// Always executed if _fileType is UOP — no early return.
        /// </summary>
        private void SetupSequenceGridForUop()
        {
            _sequenceGrid.DataSource = null;
            _sequenceGrid.Columns.Clear();
            AddUopColumns();
            _sequenceGrid.DataSource = _sequenceBindingSource;
            _gridIsInMulMode = false;
        }

        private void AddUopColumns()
        {
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UopGroupIndex", HeaderText = "UOP Group", Width = 60 });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FrameCount", HeaderText = "Frames", Width = 50 });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MulGroupIndex", HeaderText = "MUL", Width = 60 });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Speed", HeaderText = "Speed", Width = 50 });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BaseGroup", HeaderText = "Base", Width = 50, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "WarGroupId", HeaderText = "War", Width = 50, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "WarModifier", HeaderText = "Mod", Width = 40, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PeaceGroupId", HeaderText = "Peace", Width = 50, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PeaceModifier", HeaderText = "Mod", Width = 40, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MountPeaceGroupId", HeaderText = "M.Peace", Width = 50, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MountPeaceModifier", HeaderText = "Mod", Width = 40, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MountWarGroupId", HeaderText = "M.War", Width = 50, ReadOnly = true });
            _sequenceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MountWarModifier", HeaderText = "Mod", Width = 40, ReadOnly = true });
        }

        // ── PopulateSequenceGrid — Dispatcher ────────────────────────────────

        private void PopulateSequenceGrid(int bodyId)
        {
            if (_fileType == UOP_FILE_TYPE)
            {
                SetupSequenceGridForUop();
                PopulateSequenceGridUop(bodyId);
            }
            else if (_fileType >= 1 && _fileType <= 5)
            {
                SetupSequenceGridForMul();
                PopulateSequenceGridMul(bodyId);
            }
            else
            {
                _sequenceGrid.DataSource = null;
                _sequenceBindingSource.DataSource = null;
            }
        }

        // ── PopulateSequenceGridUop ───────────────────────────────────────────

        private void PopulateSequenceGridUop(int bodyId)
        {
            if (_uopManager == null || _uopManager.SequenceEntries == null)
            { _sequenceBindingSource.DataSource = null; return; }

            uint uid = (uint)bodyId;
            if (!_uopManager.SequenceEntries.TryGetValue(uid, out var list))
            {
                _sequenceBindingSource.DataSource = null;
                _seqCurrentAction = -1;
                _currentSeqEntry = null;
                _sequencePreviewBox.Image = null;
                return;
            }

            _sequenceViewModelList = new System.ComponentModel.BindingList<SequenceViewModelItem>();

            var warOverrides = _uopManager.GetStateOverrides((int)uid, CreatureState.War);
            var peaceOverrides = _uopManager.GetStateOverrides((int)uid, CreatureState.Peace);
            var mountPeaceOverrides = _uopManager.GetStateOverrides((int)uid, CreatureState.MountPeace);
            var mountWarOverrides = _uopManager.GetStateOverrides((int)uid, CreatureState.MountWar);

            foreach (var entry in list)
            {
                var vm = new SequenceViewModelItem(entry);
                int action = (int)entry.UopGroupIndex;

                void FillState(Dictionary<int, StateOverride> overrides,
                               Action<int?> setGroupId,
                               Action<ushort?> setMod)
                {
                    if (overrides != null && overrides.TryGetValue(action, out var ov))
                    { setGroupId(ov.GroupId); setMod(ov.Modifier); }
                    else
                    { setGroupId(vm.BaseGroup); setMod(null); }
                }

                FillState(warOverrides, s => vm.WarGroupId = s, s => vm.WarModifier = s);
                FillState(peaceOverrides, s => vm.PeaceGroupId = s, s => vm.PeaceModifier = s);
                FillState(mountPeaceOverrides, s => vm.MountPeaceGroupId = s, s => vm.MountPeaceModifier = s);
                FillState(mountWarOverrides, s => vm.MountWarGroupId = s, s => vm.MountWarModifier = s);

                _sequenceViewModelList.Add(vm);
            }

            _sequenceBindingSource.DataSource = _sequenceViewModelList;

            if (_sequenceViewModelList.Count > 0)
            {
                _seqCurrentAction = (int)_sequenceViewModelList[0].UopGroupIndex;
                _currentSeqEntry = _sequenceViewModelList[0].Entry;
                _seqPreviewFrameIndex = 0;
                if (_sequenceGrid.Rows.Count > 0)
                    _sequenceGrid.Rows[0].Selected = true;
            }

            UpdateSequencePreviewUop();
        }

        // ── PopulateSequenceGridMul ───────────────────────────────────────────

        private void PopulateSequenceGridMul(int bodyId)
        {
            var items = new System.ComponentModel.BindingList<MulSequenceViewItem>();

            int animLength = Animations.GetAnimLength(bodyId, _fileType);
            if (animLength <= 0) animLength = 22;

            // Remapping
            int resolvedBody = bodyId;
            int resolvedFileType = _fileType;

            int btBody = bodyId;
            if (Ultima.BodyTable.Entries != null
                && Ultima.BodyTable.Entries.TryGetValue(bodyId, out Ultima.BodyTableEntry btEntry)
                && !Ultima.BodyConverter.Contains(bodyId))
                btBody = btEntry.OldId;

            int bcBody = btBody;
            int bcFt = Ultima.BodyConverter.Convert(ref bcBody);
            if (bcFt >= 1 && bcFt <= 5)
            { resolvedBody = bcBody; resolvedFileType = bcFt; }

            string[] nameTable = animLength == 13
                ? new[] { "Walk","Run","Stand","Eat","Alert","Attack1","Attack2",
                  "GetHit","Die1","Fidget1","Fidget2","LieDown","Die2" }
                : animLength == 22
                ? new[] { "Walk","Stand","Die1","Die2","Attack1","Attack2","Attack3",
                  "AttackBow","AttackCrossBow","AttackThrow","GetHit","Pillage",
                  "Stomp","Cast2","Cast3","BlockRight","BlockLeft","Idle",
                  "Fidget","Fly","TakeOff","GetHitInAir" }
                : new[] { "Walk","WalkStaff","Run","RunStaff","Idle","Idle2","Fidget",
                  "CombatIdle1H","CombatIdle1H2","AttackSlash1H","AttackPierce1H",
                  "AttackBash1H","AttackBash2H","AttackSlash2H","AttackPierce2H",
                  "CombatAdv1H","Spell1","Spell2","AttackBow","AttackCrossbow",
                  "GetHit","DieFwd","DieBack","HorseWalk","HorseRun","HorseIdle",
                  "HorseAtk1H","HorseAtkBow","HorseAtkXBow","HorseAtk2H",
                  "BlockShield","PunchJab","BowLesser","Salute","Ingest" };

            string ixName = resolvedFileType == 1 ? "anim.idx" : $"anim{resolvedFileType}.idx";
            string ixPath = Ultima.Files.GetFilePath(ixName);

            for (int action = 0; action < animLength; action++)
            {
                var item = new MulSequenceViewItem
                {
                    ActionIndex = action,
                    ActionName = action < nameTable.Length ? nameTable[action] : $"Action{action}",
                };

                int totalFrames = 0;
                bool hasAnyData = false;

                for (int dir = 0; dir < 5; dir++)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(_fileType, bodyId, action, dir);
                    if (edit?.Frames == null || edit.Frames.Count == 0) continue;

                    hasAnyData = true;
                    totalFrames += edit.Frames.Count;

                    if (dir == 0) { item.CenterX = edit.Frames[0].Center.X; item.CenterY = edit.Frames[0].Center.Y; item.FramesDir0 = edit.Frames.Count; }
                    if (dir == 1) item.FramesDir1 = edit.Frames.Count;
                    if (dir == 2) item.FramesDir2 = edit.Frames.Count;
                    if (dir == 3) item.FramesDir3 = edit.Frames.Count;
                    if (dir == 4) item.FramesDir4 = edit.Frames.Count;
                }

                item.TotalFrames = totalFrames;
                item.HasData = hasAnyData;

                // IDX-Eintrag lesen (alle 3 Felder: offset, length, extra)
                if (!string.IsNullOrEmpty(ixPath) && File.Exists(ixPath))
                {
                    long pos = (long)(resolvedBody * 110 * 5 + action * 5) * 12;
                    try
                    {
                        using var fs = new FileStream(ixPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        if (pos + 12 <= fs.Length)
                        {
                            fs.Seek(pos, SeekOrigin.Begin);
                            using var br = new BinaryReader(fs);
                            int off = br.ReadInt32();
                            int len = br.ReadInt32();
                            int ext = br.ReadInt32();
                            int act = len > 0 ? len : ext > 0 ? ext : 0;
                            item.IdxOffset = off;
                            item.IdxLength = act;
                            item.InFile = (off >= 0 && act > 0);
                            item.OnlyInCache = hasAnyData && !item.InFile;
                        }
                    }
                    catch { }
                }

                items.Add(item);
            }

            // Decouple event while DataSource is being set —
            // Prevents NullReferenceException in OnSequenceGridSelectionChanged
            // when the grid automatically selects rows during binding
            _sequenceGrid.SelectionChanged -= OnSequenceGridSelectionChanged;
            try
            {
                _sequenceBindingSource.DataSource = items;
                _sequenceGrid.DataSource = _sequenceBindingSource;
            }
            finally
            {
                _sequenceGrid.SelectionChanged += OnSequenceGridSelectionChanged;
            }

            // Select the first row containing data
            _seqCurrentAction = -1;
            for (int i = 0; i < _sequenceGrid.Rows.Count; i++)
            {
                var boundItem = _sequenceGrid.Rows[i].DataBoundItem as MulSequenceViewItem;
                if (boundItem != null && boundItem.HasData)
                {
                    _seqCurrentAction = boundItem.ActionIndex;
                    _sequenceGrid.Rows[i].Selected = true;
                    break;
                }
            }

            UpdateSequencePreviewMul();
        }

        // ── MulSequenceViewItem ───────────────────────────────────────────────

        public sealed class MulSequenceViewItem
        {
            public int ActionIndex { get; set; }
            public string ActionName { get; set; }
            public int FramesDir0 { get; set; }
            public int FramesDir1 { get; set; }
            public int FramesDir2 { get; set; }
            public int FramesDir3 { get; set; }
            public int FramesDir4 { get; set; }
            public int TotalFrames { get; set; }
            public int CenterX { get; set; }
            public int CenterY { get; set; }
            public int IdxOffset { get; set; }
            public int IdxLength { get; set; }
            public bool HasData { get; set; }
            public bool InFile { get; set; }
            public bool OnlyInCache { get; set; }

            public string StatusText =>
                !HasData ? "leer" :
                OnlyInCache ? "nur Cache" :
                InFile ? "in Datei" : "?";
        }

        // ── TryFindAnimationForPreview ────────────────────────────────────────

        private UopAnimIdx TryFindAnimationForPreview(int body, int action,
            int preferredDir, out int foundDir)
        {
            foundDir = -1;
            if (_uopManager == null || body < 0 || action < 0) return null;

            int[] dirs = new int[5];
            dirs[0] = preferredDir;
            int idx = 1;
            for (int d = 0; d < 5; d++)
                if (d != preferredDir) dirs[idx++] = d;

            foreach (int dir in dirs)
            {
                var cached = _uopManager.GetUopAnimation(body, action, dir);
                if (cached != null && cached.Frames.Count > 0)
                { foundDir = dir; return cached; }

                var fileInfo = _uopManager.GetAnimationData(body, action, dir);
                if (fileInfo == null) continue;
                byte[] raw = fileInfo.GetData();
                if (raw == null || raw.Length == 0) continue;

                var afterLoad = _uopManager.GetUopAnimation(body, action, dir);
                if (afterLoad != null && afterLoad.Frames.Count > 0)
                { foundDir = dir; return afterLoad; }

                if (raw.Length >= 12)
                {
                    try
                    {
                        using var ms = new System.IO.MemoryStream(raw);
                        using var br = new System.IO.BinaryReader(ms);
                        var header = UopAnimationDataManager.ReadUopBinHeader(br);
                        if (header != null && header.FrameCount > 0)
                            System.Diagnostics.Debug.WriteLine(
                                $"[SeqPreview] Body={body} Act={action} Dir={dir}: " +
                                $"FrameCount={header.FrameCount} aber GetUopAnimation=null.");
                    }
                    catch { }
                }
            }
            return null;
        }

        // ── UpdateSequencePreview ─────────────────────────────────────────────

        private void UpdateSequencePreview()
        {
            if (_fileType == UOP_FILE_TYPE) UpdateSequencePreviewUop();
            else UpdateSequencePreviewMul();
        }

        private void UpdateSequencePreviewUop()
        {
            if (_uopManager == null || _seqCurrentAction < 0)
            { _sequencePreviewBox.Image = null; return; }

            var anim = TryFindAnimationForPreview(
                _currentBody, _seqCurrentAction, _seqPreviewDirection, out _);

            if (anim == null || anim.Frames.Count == 0)
            { _sequencePreviewBox.Image = null; return; }

            if (_seqPreviewFrameIndex >= anim.Frames.Count) _seqPreviewFrameIndex = 0;
            _sequencePreviewBox.Image = anim.Frames[_seqPreviewFrameIndex].Image;
        }

        private void UpdateSequencePreviewMul()
        {
            // Guard: intercept invalid states
            if (_seqCurrentAction < 0 || _fileType == 0 || _currentBody < 0)
            {
                _sequencePreviewBox.Image = null;
                return;
            }

            // If the currently selected row contains no data → cancel immediately
            if (_gridIsInMulMode && _sequenceGrid.SelectedRows.Count > 0)
            {
                var boundItem = _sequenceGrid.SelectedRows[0].DataBoundItem as MulSequenceViewItem;
                if (boundItem != null && !boundItem.HasData)
                {
                    _sequencePreviewBox.Image = null;
                    return;
                }
            }

            AnimIdx edit = AnimationEdit.GetAnimation(
                _fileType, _currentBody, _seqCurrentAction, _seqPreviewDirection);

            if (edit == null || edit.Frames?.Count == 0)
            {
                for (int dir = 0; dir < 5; dir++)
                {
                    if (dir == _seqPreviewDirection) continue;
                    edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _seqCurrentAction, dir);
                    if (edit?.Frames?.Count > 0) break;
                }
            }

            if (edit == null || edit.Frames?.Count == 0)
            {
                _sequencePreviewBox.Image = null;
                return;
            }

            if (_seqPreviewFrameIndex >= edit.Frames.Count)
                _seqPreviewFrameIndex = 0;

            var bitmaps = edit.GetFrames();
            if (bitmaps == null || bitmaps.Length == 0)
            {
                _sequencePreviewBox.Image = null;
                return;
            }

            if (_seqPreviewFrameIndex < bitmaps.Length)
                _sequencePreviewBox.Image = bitmaps[_seqPreviewFrameIndex];
            else
                _sequencePreviewBox.Image = null;
        }

        // ── OnSequenceGridSelectionChanged ───────────────────────────────────

        private void OnSequenceGridSelectionChanged(object sender, EventArgs e)
        {
            if (_sequenceGrid.SelectedRows.Count == 0) return;
            if (_fileType == 0) return;

            if (_fileType == UOP_FILE_TYPE)
            {
                var vm = _sequenceGrid.SelectedRows[0].DataBoundItem as SequenceViewModelItem;
                if (vm == null) return;  // ← leere Zeile angeklickt
                _currentSeqEntry = vm.Entry;
                _seqCurrentAction = (int)vm.UopGroupIndex;
                _seqPreviewFrameIndex = 0;
                UpdateSequencePreviewUop();
            }
            else
            {
                var item = _sequenceGrid.SelectedRows[0].DataBoundItem as MulSequenceViewItem;
                if (item == null) return;  // ← leere Zeile angeklickt
                _seqCurrentAction = item.ActionIndex;
                _seqPreviewFrameIndex = 0;
                UpdateSequencePreviewMul();
            }
        }

        // ── OnSequenceTimerTick ───────────────────────────────────────────────

        private void OnSequenceTimerTick(object sender, EventArgs e)
        {
            if (_fileType == UOP_FILE_TYPE)
            {
                if (_uopManager == null || _seqCurrentAction < 0) return;
                var anim = TryFindAnimationForPreview(
                    _currentBody, _seqCurrentAction, _seqPreviewDirection, out _);
                if (anim == null || anim.Frames.Count == 0) { _sequencePreviewBox.Image = null; return; }
                _seqPreviewFrameIndex++;
                if (_seqPreviewFrameIndex >= anim.Frames.Count) _seqPreviewFrameIndex = 0;
                _sequencePreviewBox.Image = anim.Frames[_seqPreviewFrameIndex].Image;
            }
            else
            {
                if (_seqCurrentAction < 0) return;
                AnimIdx edit = AnimationEdit.GetAnimation(
                    _fileType, _currentBody, _seqCurrentAction, _seqPreviewDirection);
                if (edit == null || edit.Frames?.Count == 0) return;
                var bitmaps = edit.GetFrames();
                if (bitmaps == null) return;
                _seqPreviewFrameIndex++;
                if (_seqPreviewFrameIndex >= bitmaps.Length) _seqPreviewFrameIndex = 0;
                _sequencePreviewBox.Image = bitmaps[_seqPreviewFrameIndex];
            }
        }

        // ── OnSequenceGridCellValueChanged ───────────────────────────────────

        private void OnSequenceGridCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_fileType != UOP_FILE_TYPE) return;
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _uopManager == null || _currentBody == -1) return;

            var grid = (DataGridView)sender;
            var vm = grid.Rows[e.RowIndex].DataBoundItem as SequenceViewModelItem;
            if (vm == null) return;

            if (grid.IsCurrentCellDirty)
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);

            bool isFrameCountChange = grid.Columns[e.ColumnIndex].DataPropertyName == "FrameCount";
            try
            {
                _uopManager.UpdateSequenceEntry(
                    (uint)_currentBody, vm.UopGroupIndex,
                    vm.FrameCount, vm.MulGroupIndex, vm.Speed,
                    vm.Entry.ExtraData, autoPopulate: isFrameCountChange);
                if (isFrameCountChange) grid.InvalidateRow(e.RowIndex);
                UpdateSequencePreviewUop();
            }
            catch (Exception ex)
            { MessageBox.Show($"Error updating sequence entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ── Save / Clone / MainMisc ───────────────────────────────────────────

        private void OnSaveSequenceClick(object sender, EventArgs e)
        {
            if (_uopManager == null || _sequenceViewModelList == null) return;
            using var dialog = new SaveFileDialog
            { InitialDirectory = Options.OutputPath, FileName = "AnimationSequence.uop", Filter = "UOP Files (*.uop)|*.uop", Title = "Save Animation Sequence" };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                _uopManager.SequenceEntries[(uint)_currentBody] = _sequenceViewModelList.Select(vm => vm.Entry).ToList();
                _uopManager.SaveAnimationSequence(dialog.FileName);
                MessageBox.Show($"Saved successfully to:\n{dialog.FileName}");
            }
            catch (Exception ex) { MessageBox.Show($"Error saving: {ex.Message}"); }
        }

        private void OnSaveBinClick(object sender, EventArgs e)
        {
            if (_uopManager == null || _sequenceViewModelList == null) return;
            using var dialog = new SaveFileDialog
            { InitialDirectory = Options.OutputPath, FileName = $"Sequence_{_currentBody}.bin", Filter = "Binary Files (*.bin)|*.bin", Title = "Save Sequence Binary Data" };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                File.WriteAllBytes(dialog.FileName, _uopManager.GetBinaryDataForAnimationId((uint)_currentBody));
                MessageBox.Show($"Saved .bin successfully to:\n{dialog.FileName}");
            }
            catch (Exception ex) { MessageBox.Show($"Error saving .bin: {ex.Message}"); }
        }

        private void OnCloneSequenceIdClick(object sender, EventArgs e)
        {
            if (_uopManager == null) return;
            string srcInput = ShowInputDialog("Enter Source Animation ID:", "Clone Sequence ID", "");
            if (!uint.TryParse(srcInput, out uint srcId)) { MessageBox.Show("Invalid Source ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!_uopManager.SequenceEntries.ContainsKey(srcId)) { MessageBox.Show($"Source ID {srcId} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            string newInput = ShowInputDialog("Enter New Animation ID:", "Clone Sequence ID", "");
            if (!uint.TryParse(newInput, out uint newId)) { MessageBox.Show("Invalid New ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (_uopManager.SequenceEntries.ContainsKey(newId))
                if (MessageBox.Show($"ID {newId} exists. Overwrite?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                _uopManager.CloneAnimationSequenceEntry(srcId, newId);
                _uopManager.EnsureIdInMainMisc(newId, forceCreatureFlag: true);
                RefreshMainMiscButtonState();
                LoadUopAnimations();
                MessageBox.Show($"Cloned {srcId} → {newId}.\nRemember to save MainMisc Table.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Error cloning: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void OnAddCurrentToMainMiscClick(object sender, EventArgs e)
        {
            if (_uopManager == null || _currentBody == -1) return;
            uint uid = (uint)_currentBody;
            const uint CREATURE_FLAG = 0x0C000000;
            if (_uopManager.IsIdInMainMisc(uid) && _uopManager.GetMainMiscFlag(uid) == CREATURE_FLAG)
            { MessageBox.Show($"ID {uid} already present.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            _uopManager.EnsureIdInMainMisc(uid, forceCreatureFlag: true);
            RefreshMainMiscButtonState();
            MessageBox.Show($"ID {uid} added to MainMisc.\nDon't forget to save.", "ID Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSyncMainMiscClick(object sender, EventArgs e)
        {
            if (_uopManager == null) return;
            Cursor = Cursors.WaitCursor;
            try
            {
                int missing = _uopManager.CheckMissingMainMiscEntries(dryRun: true);
                if (missing > 0)
                {
                    if (MessageBox.Show($"Found {missing} missing entries.\nAdd now?", "Sync", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int added = _uopManager.CheckMissingMainMiscEntries(dryRun: false);
                        RefreshMainMiscButtonState();
                        MessageBox.Show($"{added} IDs added.", "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                    MessageBox.Show("MainMisc is up to date.", "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void RefreshMainMiscButtonState()
        {
            if (_btnSaveMainMisc != null && _uopManager != null)
            {
                _btnSaveMainMisc.Enabled = _uopManager.MainMiscModified;
                System.Diagnostics.Debug.WriteLine($"[MainMisc] Enabled={_btnSaveMainMisc.Enabled}");
            }
        }

        private void OnSaveMainMiscClick(object sender, EventArgs e)
        {
            if (_uopManager == null) return;
            using var dialog = new SaveFileDialog
            { InitialDirectory = Options.OutputPath, FileName = "MainMisc.uop", Filter = "UOP Files (*.uop)|*.uop", Title = "Save MainMisc Table" };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                _uopManager.SaveMainMisc(dialog.FileName);
                MessageBox.Show($"MainMisc.uop saved to:\n{dialog.FileName}\n\nEntries: {_uopManager.GetMainMiscEntryCount()}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Error saving MainMisc: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ── ShowInputDialog ───────────────────────────────────────────────────

        private string ShowInputDialog(string text, string caption, string defaultValue)
        {
            using var prompt = new Form { Width = 500, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, Text = caption, StartPosition = FormStartPosition.CenterScreen };
            var lbl = new Label { Left = 50, Top = 20, Width = 400, Text = text };
            var tb = new TextBox { Left = 50, Top = 50, Width = 400, Text = defaultValue };
            var btn = new Button { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            btn.Click += (s, a) => prompt.Close();
            prompt.Controls.AddRange(new Control[] { lbl, tb, btn });
            prompt.AcceptButton = btn;
            return prompt.ShowDialog() == DialogResult.OK ? tb.Text : "";
        }

        // ── Buttons ───────────────────────────────────────────────────────────

        private void BtnSeqPlay_Click(object sender, EventArgs e) => _sequenceTimer.Start();
        private void BtnSeqStop_Click(object sender, EventArgs e) => _sequenceTimer.Stop();

        private void SeqDirTrackBar_ValueChanged(object sender, EventArgs e)
        {
            var tb = (TrackBar)sender;
            _seqPreviewDirection = tb.Value;
            _seqDirLabel.Text = $"Direction: {_seqPreviewDirection}";
            _seqPreviewFrameIndex = 0;
            UpdateSequencePreview();
        }

        #endregion

        #region [ Hex Editor ] - Button handler and logic to open the hex editor with the corresponding data

        private AnimationHexEditorForm _hexEditor;

        private void btnOpenHexEditor_Click(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                MessageBox.Show("Please select an animation file first.",
                    "Hex Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (AnimationListTreeView.SelectedNode == null)
            {
                MessageBox.Show("Please select an animation first.",
                    "Hex Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_hexEditor == null || _hexEditor.IsDisposed)
            {
                _hexEditor = new AnimationHexEditorForm();

                _hexEditor.OnByteWritten += (absoluteOffset, newByte) =>
                {
                    AnimationPictureBox.Invalidate();
                };

                // ── Wire up "Pin as A / Pin as B" from HexEditor to HexCompareForm
                _hexEditor.OnPinAsA += buf =>
                {
                    EnsureHexCompare();
                    _hexCompare.LoadBufferA(buf);
                    _hexCompare.Show();
                    _hexCompare.BringToFront();
                };

                _hexEditor.OnPinAsB += buf =>
                {
                    EnsureHexCompare();
                    _hexCompare.LoadBufferB(buf);
                    _hexCompare.Show();
                    _hexCompare.BringToFront();
                };
            }

            if (_fileType == 6)
                OpenHexEditorUop();
            else
                OpenHexEditorMul();

            _hexEditor.Show();
            _hexEditor.BringToFront();
        }

        private void EnsureHexCompare()
        {
            if (_hexCompare == null || _hexCompare.IsDisposed)
                _hexCompare = new HexCompareForm();
        }

        // ──────────────────────────────────────────────────────────────────────
        //  UOP – Retrieve raw data and regions from the UopManager
        // ──────────────────────────────────────────────────────────────────────

        private void OpenHexEditorUop()
        {
            if (_uopManager == null) return;

            // Get raw data - try current direction first, then all others
            var fileInfo = _uopManager.GetAnimationData(_currentBody, _currentAction, _currentDir);
            byte[] rawData = null;
            long fileOffset = 0;          // ← use 0 since DataOffset doesn't exist
            string filePath = "(UOP – memory)";

            if (fileInfo != null)
            {
                rawData = fileInfo.GetData();
                // fileInfo.DataOffset does NOT exist → use 0 as placeholder
                fileOffset = 0;
                filePath = fileInfo.File?.FilePath ?? filePath;
            }

            // Fallback: try all directions
            if (rawData == null || rawData.Length == 0)
            {
                for (int dir = 0; dir < 5; dir++)
                {
                    var fi = _uopManager.GetAnimationData(_currentBody, _currentAction, dir);
                    if (fi == null) continue;
                    byte[] d = fi.GetData();
                    if (d != null && d.Length > 0)
                    {
                        rawData = d;
                        fileOffset = 0;   // ← same here
                        filePath = fi.File?.FilePath ?? filePath;
                        break;
                    }
                }
            }

            if (rawData == null || rawData.Length == 0)
            {
                MessageBox.Show("No raw data found for this animation.",
                    "Hex Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var regions = BuildUopRegions(rawData);
            var preview = GetCurrentFrameBitmap();

            _hexEditor.LoadUopAnimation(rawData, fileOffset, filePath,
                _currentBody, _currentAction, _currentDir, _currentAction, regions);

            if (preview != null)
                _hexEditor.SetPreviewImage(preview, regions.Count > 0 ? regions[0] : null);
        }

        // ──────────────────────────────────────────────────────────────────────
        //  MUL – Read raw data directly from the idx/mul file
        // ──────────────────────────────────────────────────────────────────────       

        private void OpenHexEditorMul()
        {
            //DebugBodyIdxEntry(_currentBody); // ← temporär, nach Debug wieder raus

            // 1. AnimIdx via SDK — exactly the same as when displaying frames
            AnimIdx edit = AnimationEdit.GetAnimation(
                _fileType, _currentBody, _currentAction, _currentDir);

            if (edit == null)
            {
                MessageBox.Show(
                    $"No AnimIdx data for body {_currentBody}, " +
                    $"Action {_currentAction}, Dir {_currentDir}.",
                    "Hex Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Bitmap[] frames = edit.GetFrames();
            if (frames == null || frames.Length == 0)
            {
                MessageBox.Show(
                    $"AnimIdx for Body {_currentBody} has no frames.",
                    "Hex Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] rawData = null;
            long dataOffset = 0;
            string mulPath = null;

            rawData = TryFindRawDataForAnimIdx(edit, out dataOffset, out mulPath, out int mappedBody);

            // 3. If raw data is found: Fill the hex editor with the actual MUL file.
            if (rawData != null && rawData.Length > 0)
            {
                var regions = BuildMulRegions(rawData, edit);
                var preview = GetCurrentFrameBitmap();

                _hexEditor.LoadMulAnimation(rawData, dataOffset, mulPath,
                    mappedBody, _currentAction, _currentDir,
                    FramesTrackBar.Value, regions);

                if (preview != null)
                    _hexEditor.SetPreviewImage(preview,
                        regions.Count > 0 ? regions[0] : null);
                return;
            }

            // 4. Fallback: Reconstructing data from AnimIdx frames
            // (for bodies that only exist in the RAM cache, e.g., imported ones)
            rawData = SerializeAnimIdxToMulFormat(edit);
            if (rawData == null)
            {
                MessageBox.Show("Error serializing AnimIdx data.",
                    "Hex Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string pseudoPath = $"(RAM-Cache: Body {_currentBody} / " +
                                $"anim{_fileType}.mul not found)";
            var regions2 = BuildMulRegions(rawData, edit);
            var preview2 = GetCurrentFrameBitmap();

            _hexEditor.LoadMulAnimation(rawData, 0, pseudoPath,
                mappedBody, _currentAction, _currentDir,
                FramesTrackBar.Value, regions2);

            if (preview2 != null)
                _hexEditor.SetPreviewImage(preview2,
                    regions2.Count > 0 ? regions2[0] : null);
        }

        /*
         * This method is for debugging purposes only. It reads the corresponding IDX entry for the given body and logs all relevant information about it, including:
         * - Calculated IDX entry position
         * - Whether the entry is within the bounds of the IDX file
         * - The raw offset, length, and extra fields from the IDX entry
         * - Whether the offset appears valid (not negative or 0xFFFFFFFF)
         * - Information from body.def and bodyconv.def related to this body
         * - Reflection data from the AnimIdx instance returned by the SDK
         */
        private void DebugBodyIdxEntry(int body)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== DEBUG Body {body} FileType {_fileType} Action {_currentAction} Dir {_currentDir} ===");

            string mulName = _fileType == 1 ? "anim.mul" : $"anim{_fileType}.mul";
            string idxName = _fileType == 1 ? "anim.idx" : $"anim{_fileType}.idx";
            string mulPath = Ultima.Files.GetFilePath(mulName);
            string idxPath = Ultima.Files.GetFilePath(idxName);

            long idxEntry = (long)(body * 110 * 5 + _currentAction * 5 + _currentDir) * 12;
            sb.AppendLine($"IDX Entry-Pos : {idxEntry} (0x{idxEntry:X})");

            try
            {
                using var idxFs = new System.IO.FileStream(idxPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                sb.AppendLine($"IDX File size : {idxFs.Length} bytes");
                sb.AppendLine($"IDX Entry in bounds: {idxEntry + 12 <= idxFs.Length}");
                if (idxEntry + 12 <= idxFs.Length)
                {
                    idxFs.Seek(idxEntry, System.IO.SeekOrigin.Begin);
                    using var br = new System.IO.BinaryReader(idxFs);
                    int rawOff = br.ReadInt32();
                    int rawLen = br.ReadInt32();
                    int rawExt = br.ReadInt32();
                    sb.AppendLine($"IDX Offset    : 0x{rawOff:X8} ({rawOff})");
                    sb.AppendLine($"IDX Length    : {rawLen} (0x{rawLen:X})");
                    sb.AppendLine($"IDX Extra     : {rawExt} (0x{rawExt:X})");
                    sb.AppendLine($"Offset valid  : {rawOff >= 0 && rawOff != unchecked((int)0xFFFFFFFF)}");
                }
            }
            catch (Exception ex) { sb.AppendLine($"IDX read error: {ex.Message}"); }

            sb.AppendLine();
            sb.AppendLine("--- SDK AnimIdx ---");
            AnimIdx editForReflection = null;
            try
            {
                editForReflection = AnimationEdit.GetAnimation(_fileType, body, _currentAction, _currentDir);
                if (editForReflection == null)
                {
                    sb.AppendLine("SDK: edit == null");
                }
                else
                {
                    var frames = editForReflection.GetFrames();
                    sb.AppendLine($"SDK: edit != null");
                    sb.AppendLine($"SDK: Frames.Count = {editForReflection.Frames?.Count}");
                    sb.AppendLine($"SDK: GetFrames() = {frames?.Length} bitmaps");
                    if (frames != null && frames.Length > 0 && frames[0] != null)
                        sb.AppendLine($"SDK: Frame[0] size = {frames[0].Width}x{frames[0].Height}");
                }
            }
            catch (Exception ex) { sb.AppendLine($"SDK error: {ex.Message}"); }



            // ── body.def check ───────────────────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine("--- body.def (BodyTable) ---");
            if (Ultima.BodyTable.Entries != null
                && Ultima.BodyTable.Entries.TryGetValue(body, out var btEntry))
                sb.AppendLine($"body.def: Body {body} → OldId {btEntry.OldId}");
            else
                sb.AppendLine($"body.def: Body {body} not listed");

            // ── BodyConverter check ──────────────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine("--- BodyConverter (bodyconv.def) ---");
            int bcBody = body;
            int bcFileType = Ultima.BodyConverter.Convert(ref bcBody);
            sb.AppendLine($"BodyConverter.Convert: Body {body} → resolvedBody {bcBody}  fileType {bcFileType}");
            sb.AppendLine($"BodyConverter.Contains({body}): {Ultima.BodyConverter.Contains(body)}");

            // ── AnimIdx Reflection ───────────────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine("--- AnimIdx Reflection ---");
            if (editForReflection != null)
            {
                try
                {
                    var type = editForReflection.GetType();
                    sb.AppendLine($"Type: {type.FullName}");
                    sb.AppendLine("Fields:");
                    foreach (var f in type.GetFields(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public))
                    {
                        try { sb.AppendLine($"  [{f.FieldType.Name}] {f.Name} = {f.GetValue(editForReflection)}"); }
                        catch { sb.AppendLine($"  {f.Name} = [error]"); }
                    }
                    sb.AppendLine("Properties:");
                    foreach (var p in type.GetProperties(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public))
                    {
                        try { sb.AppendLine($"  [{p.PropertyType.Name}] {p.Name} = {p.GetValue(editForReflection)}"); }
                        catch { sb.AppendLine($"  {p.Name} = [error]"); }
                    }
                    // After the Properties block, but before writing to the file:
                    sb.AppendLine();
                    sb.AppendLine("--- FrameEdit Reflection (Frame[0]) ---");
                    try
                    {
                        var edit2 = AnimationEdit.GetAnimation(_fileType, body, _currentAction, _currentDir);
                        if (edit2?.Frames != null && edit2.Frames.Count > 0)
                        {
                            var frame0 = edit2.Frames[0];
                            var ftype = frame0.GetType();
                            sb.AppendLine($"FrameEdit Type: {ftype.FullName}");
                            sb.AppendLine("Fields:");
                            foreach (var f in ftype.GetFields(
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Public))
                            {
                                try { sb.AppendLine($"  [{f.FieldType.Name}] {f.Name} = {f.GetValue(frame0)}"); }
                                catch { sb.AppendLine($"  {f.Name} = [error]"); }
                            }
                            sb.AppendLine("Properties:");
                            foreach (var p in ftype.GetProperties(
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Public))
                            {
                                try { sb.AppendLine($"  [{p.PropertyType.Name}] {p.Name} = {p.GetValue(frame0)}"); }
                                catch { sb.AppendLine($"  {p.Name} = [error]"); }
                            }
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"FrameEdit reflection error: {ex.Message}"); }
                }
                catch (Exception ex) { sb.AppendLine($"Reflection error: {ex.Message}"); }
            }
            else
            {
                sb.AppendLine("(edit == null, no reflection possible)");
            }

            // ── Write to file + MessageBox ──────────────────────────────────
            string dumpPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"AnimIdxDump_Body{body}_FT{_fileType}_A{_currentAction}_D{_currentDir}.txt");
            System.IO.File.WriteAllText(dumpPath, sb.ToString());

            MessageBox.Show(
                $"Dump gespeichert:\n{dumpPath}\n\n" +
                sb.ToString().Substring(0, Math.Min(1000, sb.Length)),
                $"Debug Body {body}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        // ──────────────────────────────────────────────────────────────────────
        //  Searches for the raw data for an AnimIdx in all anim files.
        //  Compares FrameCount from the IDX with edit.Frames.Count.
        // ──────────────────────────────────────────────────────────────────────

        private byte[] TryFindRawDataForAnimIdx(AnimIdx edit, out long foundOffset, out string foundMulPath, out int foundMappedBody)
        {
            foundOffset = 0;
            foundMulPath = null;
            foundMappedBody = _currentBody;

            string mulName = _fileType == 1 ? "anim.mul" : $"anim{_fileType}.mul";
            string idxName = _fileType == 1 ? "anim.idx" : $"anim{_fileType}.idx";
            string mulPathFb = Ultima.Files.GetFilePath(mulName);
            string idxPathFb = Ultima.Files.GetFilePath(idxName);

            bool mulExists = !string.IsNullOrEmpty(mulPathFb) && System.IO.File.Exists(mulPathFb);
            bool idxExists = !string.IsNullOrEmpty(idxPathFb) && System.IO.File.Exists(idxPathFb);
            if (!mulExists || !idxExists) return null;

            // ── Retrieve fingerprint from SDK-FrameEdit ─────────────────────────────
            // Nur W + H + frameCount — kein CX/CY da SDK-Center != MUL-Header CX/CY
            int refW = -1, refH = -1;
            int refCount = edit?.Frames?.Count ?? -1;
            if (edit?.Frames != null && edit.Frames.Count > 0)
            {
                var f0 = edit.Frames[0];
                var ftype = f0.GetType();
                var fW = ftype.GetField("Width",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                var fH = ftype.GetField("Height",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                if (fW != null) refW = (int)fW.GetValue(f0);
                if (fH != null) refH = (int)fH.GetValue(f0);
            }

            // ── Step 1: _currentBody directly + verify ────────────────────
            var candidate = ReadRawFromIdx(_currentBody, _fileType, idxPathFb, mulPathFb,
                out long off1, out string path1);
            if (candidate != null && VerifyBlockEx(candidate, refW, refH, refCount))
            {
                foundOffset = off1; foundMulPath = path1;
                foundMappedBody = _currentBody;
                return candidate;
            }

            // ── Step 2: bodyconv.def ───────────────────────────────────────────
            string bodyconvPath = Ultima.Files.GetFilePath("bodyconv.def");
            if (!string.IsNullOrEmpty(bodyconvPath) && System.IO.File.Exists(bodyconvPath))
            {
                int targetCol = _fileType - 1;
                if (targetCol >= 1 && targetCol <= 4)
                {
                    try
                    {
                        foreach (string rawLine in System.IO.File.ReadLines(bodyconvPath))
                        {
                            int ci = rawLine.IndexOf('#');
                            string clean = (ci >= 0 ? rawLine.Substring(0, ci) : rawLine).Trim();
                            if (clean.Length == 0) continue;
                            string[] parts = clean.Split(
                                new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length < 2) continue;
                            if (!int.TryParse(parts[0], out int lineBody)) continue;
                            if (lineBody != _currentBody) continue;
                            if (targetCol >= parts.Length) break;
                            if (!int.TryParse(parts[targetCol], out int mappedBody)) break;
                            if (mappedBody < 0) break;
                            var r = ReadRawFromIdx(mappedBody, _fileType, idxPathFb, mulPathFb,
                                out long off2, out string path2);
                            if (r != null && VerifyBlockEx(r, refW, refH, refCount))
                            {
                                foundOffset = off2; foundMulPath = path2;
                                foundMappedBody = mappedBody;
                                return r;
                            }
                            break;
                        }
                    }
                    catch { }
                }
            }

            // ── Step 3: body.def ───────────────────────────────────────────────
            if (Ultima.BodyTable.Entries != null
                && Ultima.BodyTable.Entries.TryGetValue(_currentBody, out Ultima.BodyTableEntry btEntry))
            {
                var r = ReadRawFromIdx(btEntry.OldId, _fileType, idxPathFb, mulPathFb,
                    out long off3, out string path3);
                if (r != null && VerifyBlockEx(r, refW, refH, refCount))
                {
                    foundOffset = off3; foundMulPath = path3;
                    foundMappedBody = btEntry.OldId;
                    return r;
                }
            }

            // ── Step 4: Scan the entire IDX ──────────────────────────────────
            // Only if all other steps have failed.
            // Read only 600 bytes per entry — Frame[0] can be at an offset >530.
            if (refW > 0 && refH > 0)
            {
                try
                {
                    using var idxFs = new System.IO.FileStream(idxPathFb,
                        System.IO.FileMode.Open, System.IO.FileAccess.Read,
                        System.IO.FileShare.Read);
                    using var mulFs = new System.IO.FileStream(mulPathFb,
                        System.IO.FileMode.Open, System.IO.FileAccess.Read,
                        System.IO.FileShare.Read);
                    using var idxBr = new System.IO.BinaryReader(idxFs);

                    long entryCount = idxFs.Length / 12;
                    for (long e = 0; e < entryCount; e++)
                    {
                        idxFs.Seek(e * 12, System.IO.SeekOrigin.Begin);
                        int rawOff = idxBr.ReadInt32();
                        int rawLen = idxBr.ReadInt32();
                        idxBr.ReadInt32();

                        if (rawOff < 0 || rawOff == unchecked((int)0xFFFFFFFF)) continue;
                        if (rawLen < 520) continue;
                        if ((long)rawOff + rawLen > mulFs.Length) continue;

                        mulFs.Seek(rawOff, System.IO.SeekOrigin.Begin);
                        var hdr = new byte[600];
                        int hdrRead = mulFs.Read(hdr, 0, 600);
                        if (hdrRead < 520) continue;

                        ushort fc = (ushort)(hdr[512] | (hdr[513] << 8));
                        if (refCount > 0 && fc != refCount) continue;
                        if (fc == 0 || fc > 256) continue;

                        int relOff = hdr[514 + 2] | (hdr[514 + 3] << 8);
                        int absOff = 512 + relOff;
                        if (absOff + 8 > hdrRead) continue;

                        ushort w = (ushort)(hdr[absOff + 4] | (hdr[absOff + 5] << 8));
                        ushort h = (ushort)(hdr[absOff + 6] | (hdr[absOff + 7] << 8));
                        if (w != refW || h != refH) continue;

                        // Match — Load entire block
                        mulFs.Seek(rawOff, System.IO.SeekOrigin.Begin);
                        var buf = new byte[rawLen];
                        int read = mulFs.Read(buf, 0, buf.Length);
                        if (read < 514) continue;
                        if (read < buf.Length) System.Array.Resize(ref buf, read);

                        foundOffset = rawOff;
                        foundMulPath = mulPathFb;
                        foundMappedBody = (int)(e / (110 * 5));
                        return buf;
                    }
                }
                catch { }
            }
            return null;
        }

        private bool VerifyBlockEx(byte[] data, int refW, int refH, int refCount)
        {
            if (data == null || data.Length < 520) return false;
            if (refW <= 0 || refH <= 0) return true;

            ushort fc = (ushort)(data[512] | (data[513] << 8));
            if (refCount > 0 && fc != refCount) return false;
            if (fc == 0 || fc > 256) return false;

            int relOff = data[514 + 2] | (data[514 + 3] << 8);
            int absOff = 512 + relOff;
            if (absOff + 8 > data.Length) return false;

            ushort w = (ushort)(data[absOff + 4] | (data[absOff + 5] << 8));
            ushort h = (ushort)(data[absOff + 6] | (data[absOff + 7] << 8));
            return w == refW && h == refH;
        }

        private byte[] ReadRawFromIdx(int body, int ft, string idxPath, string mulPath,
            out long foundOffset, out string foundMulPath)
        {
            foundOffset = 0;
            foundMulPath = null;

            long idxEntry = (long)(body * 110 * 5 + _currentAction * 5 + _currentDir) * 12;
            try
            {
                using var idxFs = new System.IO.FileStream(idxPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read,
                    System.IO.FileShare.Read);
                if (idxEntry + 12 > idxFs.Length) return null;

                idxFs.Seek(idxEntry, System.IO.SeekOrigin.Begin);
                using var br = new System.IO.BinaryReader(idxFs);
                int rawOff = br.ReadInt32();
                int rawLen = br.ReadInt32();
                int rawExt = br.ReadInt32();

                if (rawOff < 0 || rawOff == unchecked((int)0xFFFFFFFF)) return null;

                int actualLen = rawLen > 0 ? rawLen : rawExt > 0 ? rawExt : 0;
                if (actualLen <= 0) return null;

                using var mulFs = new System.IO.FileStream(mulPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read,
                    System.IO.FileShare.Read);
                if (rawOff + actualLen > mulFs.Length) return null;

                mulFs.Seek(rawOff, System.IO.SeekOrigin.Begin);
                var buf = new byte[actualLen];
                int read = mulFs.Read(buf, 0, buf.Length);
                if (read < 514) return null;

                if (read < buf.Length) System.Array.Resize(ref buf, read);
                foundOffset = rawOff;
                foundMulPath = mulPath;
                return buf;
            }
            catch { return null; }
        }


        // ──────────────────────────────────────────────────────────────────────
        //  Serializes AnimIdx frames to MUL format (fallback for RAM cache)
        //  Format: Palette(512) + FrameCount(2) + Lookup(N*4) + FrameData
        // ──────────────────────────────────────────────────────────────────────

        private byte[] SerializeAnimIdxToMulFormat(AnimIdx edit)
        {
            try
            {
                if (edit?.Frames == null) return null;

                using var ms = new System.IO.MemoryStream();
                using var bw = new System.IO.BinaryWriter(ms);

                // Palette (256 × uint16 = 512 Bytes)
                for (int i = 0; i < 256; i++)
                    bw.Write(i < edit.Palette.Length ? edit.Palette[i] : (ushort)0x8000);

                // FrameCount
                ushort fc = (ushort)edit.Frames.Count;
                bw.Write(fc);

                // Lookup table: Placeholder, will be filled later
                long lookupStart = ms.Position;
                for (int i = 0; i < fc; i++) bw.Write((int)0);

                // Write frame data and store offsets
                var offsets = new int[fc];
                var bitmaps = edit.GetFrames();

                for (int i = 0; i < fc; i++)
                {
                    offsets[i] = (int)ms.Position;

                    var frame = edit.Frames[i];
                    var bmp = (bitmaps != null && i < bitmaps.Length) ? bitmaps[i] : null;

                    // Frame-Header: CX (int16), CY (int16), Width (uint16), Height (uint16)
                    bw.Write((short)(frame?.Center.X ?? 0));
                    bw.Write((short)(frame?.Center.Y ?? 0));

                    if (bmp != null)
                    {
                        bw.Write((ushort)bmp.Width);
                        bw.Write((ushort)bmp.Height);

                        // Pixel data (simplified: RLE would be too complex)
                        // We simply write the pixel values ​​as uint16
                        for (int y = 0; y < bmp.Height; y++)
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                var c = bmp.GetPixel(x, y);
                                ushort v = c.A == 0 ? (ushort)0
                                    : (ushort)(0x8000
                                        | ((c.R >> 3) << 10)
                                        | ((c.G >> 3) << 5)
                                        | (c.B >> 3));
                                bw.Write(v);
                            }
                    }
                    else
                    {
                        bw.Write((ushort)0);
                        bw.Write((ushort)0);
                    }
                }

                // Fill lookup table with real offsets
                long endPos = ms.Position;
                ms.Seek(lookupStart, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < fc; i++) bw.Write(offsets[i]);
                ms.Seek(endPos, System.IO.SeekOrigin.Begin);

                return ms.ToArray();
            }
            catch { return null; }
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Building regions for UOP
        //  Analyzes the raw data block and marks header and frame starts.
        // ──────────────────────────────────────────────────────────────────────

        private List<HexRegion> BuildUopRegions(byte[] data)
        {
            var regions = new List<HexRegion>();
            if (data == null || data.Length < 8) return regions;

            // Colors for the 28 sequences (cyclical)
            System.Drawing.Color[] palette =
            {
        System.Drawing.Color.FromArgb(60, 100, 200, 255),
        System.Drawing.Color.FromArgb(60, 255, 140,  0),
        System.Drawing.Color.FromArgb(60,   0, 200, 80),
        System.Drawing.Color.FromArgb(60, 200,  50, 50),
        System.Drawing.Color.FromArgb(60, 180,  60, 220),
        System.Drawing.Color.FromArgb(60,  60, 200, 200),
        System.Drawing.Color.FromArgb(60, 220, 220,  50),
    };

            try
            {
                using var ms = new System.IO.MemoryStream(data);
                using var br = new System.IO.BinaryReader(ms);

                // UOP-Header (24 Bytes fest laut UopAnimationDataManager)
                int headerLen = 24;
                if (data.Length >= headerLen)
                {
                    regions.Add(new HexRegion
                    {
                        Offset = 0,
                        Length = headerLen,
                        Label = "UOP Header",
                        Tooltip = $"Body:{_currentBody}  Action:{_currentAction}",
                        HighlightColor = System.Drawing.Color.FromArgb(80, 255, 215, 0),
                        IsSequenceStart = true,
                        SequenceIndex = 0,
                        DirectionIndex = _currentDir
                    });
                }

                // Frame table: from offset 24, 12 bytes per frame (offset + length + extra)
                // FrameCount steht in Bytes 8–11 (int32)
                if (data.Length >= 12)
                {
                    br.BaseStream.Seek(8, System.IO.SeekOrigin.Begin);
                    int frameCount = br.ReadInt32();

                    if (frameCount > 0 && frameCount < 10000)
                    {
                        long tableOffset = headerLen;
                        long tableLen = frameCount * 12;

                        if (tableOffset + tableLen <= data.Length)
                        {
                            regions.Add(new HexRegion
                            {
                                Offset = tableOffset,
                                Length = tableLen,
                                Label = $"Frame-Tabelle ({frameCount} Frames)",
                                Tooltip = "Offset/Länge je Frame (12 B pro Eintrag)",
                                HighlightColor = System.Drawing.Color.FromArgb(50, 100, 255, 100),
                                IsSequenceStart = false
                            });

                            // Marking individual frame data
                            br.BaseStream.Seek(tableOffset, System.IO.SeekOrigin.Begin);
                            for (int i = 0; i < frameCount && i < 28; i++)
                            {
                                int fOff = br.ReadInt32();
                                int fLen = br.ReadInt32();
                                br.ReadInt32(); // extra

                                if (fOff < 0 || fLen <= 0) continue;
                                if (fOff + fLen > data.Length) continue;

                                var color = palette[i % palette.Length];
                                var preview = GetFramePreviewBitmap(i);

                                regions.Add(new HexRegion
                                {
                                    Offset = fOff,
                                    Length = fLen,
                                    Label = $"Frame {i}  Dir {_currentDir}",
                                    Tooltip = $"Länge: {fLen} Bytes  Offset: 0x{fOff:X}",
                                    HighlightColor = color,
                                    IsSequenceStart = (i == 0),
                                    SequenceIndex = _currentAction,
                                    DirectionIndex = _currentDir,
                                    FrameIndex = i,
                                    PreviewImage = preview
                                });
                            }
                        }
                    }
                }
            }
            catch { /* best-effort – partial regions sind ok */ }

            return regions;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Establish regions for MUL
        //  MUL format: Palette (512 B) + FrameCount (2 B) + Frames
        // ──────────────────────────────────────────────────────────────────────

        private List<HexRegion> BuildMulRegions(byte[] data, Ultima.AnimIdx edit)
        {
            var regions = new List<HexRegion>();
            if (data == null || data.Length < 514) return regions;

            System.Drawing.Color[] palette =
            {
        System.Drawing.Color.FromArgb(60, 100, 200, 255),
        System.Drawing.Color.FromArgb(60, 255, 140,  0),
        System.Drawing.Color.FromArgb(60,   0, 200, 80),
        System.Drawing.Color.FromArgb(60, 200,  50, 50),
        System.Drawing.Color.FromArgb(60, 180,  60, 220),
        System.Drawing.Color.FromArgb(60,  60, 200, 200),
        System.Drawing.Color.FromArgb(60, 220, 220,  50),
    };

            // Palette: 256 × 2 Bytes = 512 Bytes
            regions.Add(new HexRegion
            {
                Offset = 0,
                Length = 512,
                Label = "Palette (256 × uint16)",
                Tooltip = "16bpp ARGB1555 color palette",
                HighlightColor = System.Drawing.Color.FromArgb(80, 255, 215, 0),
                IsSequenceStart = true,
                SequenceIndex = 0,
                DirectionIndex = _currentDir
            });

            // FrameCount: 2 Bytes bei Offset 512
            if (data.Length >= 514)
            {
                ushort frameCount = (ushort)(data[512] | (data[513] << 8));
                regions.Add(new HexRegion
                {
                    Offset = 512,
                    Length = 2,
                    Label = $"FrameCount = {frameCount}",
                    Tooltip = "Number of frames in this direction",
                    HighlightColor = System.Drawing.Color.FromArgb(80, 200, 100, 255)
                });

                // Frame-Lookup-Tabelle: frameCount × 4 Bytes (Offset je Frame)
                long lookupLen = frameCount * 4L;
                if (514 + lookupLen <= data.Length)
                {
                    regions.Add(new HexRegion
                    {
                        Offset = 514,
                        Length = lookupLen,
                        Label = $"Frame-Lookup ({frameCount} × 4 B)",
                        Tooltip = "Byte offsets of the individual frames",
                        HighlightColor = System.Drawing.Color.FromArgb(50, 100, 255, 100)
                    });

                    // Individual frame data areas
                    if (edit?.Frames != null)
                    {
                        for (int i = 0; i < edit.Frames.Count && i < 28; i++)
                        {
                            //Read frame offset from lookup table
                            int lookupPos = 514 + i * 4;
                            if (lookupPos + 4 > data.Length) break;

                            int fOff = System.BitConverter.ToInt32(data, lookupPos);
                            if (fOff <= 0 || fOff >= data.Length) continue;

                            // Frame length = next offset – current offset (or end of file)
                            int nextOff = data.Length;
                            if (i + 1 < edit.Frames.Count)
                            {
                                int nextLookup = 514 + (i + 1) * 4;
                                if (nextLookup + 4 <= data.Length)
                                {
                                    int n = System.BitConverter.ToInt32(data, nextLookup);
                                    if (n > fOff && n <= data.Length) nextOff = n;
                                }
                            }

                            long fLen = nextOff - fOff;
                            if (fLen <= 0) continue;

                            var color = palette[i % palette.Length];
                            var preview = GetFramePreviewBitmap(i);

                            regions.Add(new HexRegion
                            {
                                Offset = fOff,
                                Length = fLen,
                                Label = $"Frame {i}  Dir {_currentDir}",
                                Tooltip = $"CenterX:{edit.Frames[i].Center.X}  CenterY:{edit.Frames[i].Center.Y}  Länge:{fLen} B",
                                HighlightColor = color,
                                IsSequenceStart = (i == 0),
                                SequenceIndex = _currentAction,
                                DirectionIndex = _currentDir,
                                FrameIndex = i,
                                PreviewImage = preview
                            });
                        }
                    }
                }
            }

            return regions;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Helper: Get the current frame preview image
        // ──────────────────────────────────────────────────────────────────────

        private System.Drawing.Bitmap GetCurrentFrameBitmap()
        {
            return GetFramePreviewBitmap(FramesTrackBar.Value);
        }

        private System.Drawing.Bitmap GetFramePreviewBitmap(int frameIndex)
        {
            try
            {
                if (_fileType == 6 && _uopManager != null)
                {
                    var uopAnim = _uopManager.GetUopAnimation(_currentBody, _currentAction, _currentDir);
                    if (uopAnim != null && frameIndex >= 0 && frameIndex < uopAnim.Frames.Count)
                        return new System.Drawing.Bitmap(uopAnim.Frames[frameIndex].Image);
                }
                else if (_fileType != 0)
                {
                    var edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    var bits = edit?.GetFrames();
                    if (bits != null && frameIndex >= 0 && frameIndex < bits.Length && bits[frameIndex] != null)
                        return new System.Drawing.Bitmap(bits[frameIndex]);
                }
            }
            catch { /* Vorschau optional */ }
            return null;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Call when selection changes (Direction / Frame / Action)
        //  This line in OnDirectionChanged, AfterSelectTreeView and
        //  Insert OnFrameCountBarChanged at the end:
        //
        //  NotifyHexEditor();
        // ──────────────────────────────────────────────────────────────────────

        private void NotifyHexEditor()
        {
            if (_hexEditor == null || _hexEditor.IsDisposed || !_hexEditor.Visible)
                return;

            // Immer vollständig neu laden damit _data, _bodyId und _dataOffset
            // zum aktuell gewählten Body passen.
            // UpdateSelection() würde _data nie tauschen → falscher Body im RLE Decoder.
            if (_fileType == 6)
                OpenHexEditorUop();
            else
                OpenHexEditorMul();
        }

        private byte[] GetCurrentUopRawData()
        {
            if (_uopManager == null) return null;
            var fi = _uopManager.GetAnimationData(_currentBody, _currentAction, _currentDir);
            return fi?.GetData();
        }

        private List<HexRegion> BuildMulRegionsForCurrentSelection()
        {
            var edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null) return new List<HexRegion>();

            // Reread raw data (briefly, as the file is cached)
            string mulFileName = _fileType == 1 ? "anim.mul" : $"anim{_fileType}.mul";
            string idxFileName = _fileType == 1 ? "anim.idx" : $"anim{_fileType}.idx";
            string mulPath = Ultima.Files.GetFilePath(mulFileName);
            string idxPath = Ultima.Files.GetFilePath(idxFileName);

            if (string.IsNullOrEmpty(mulPath)) return new List<HexRegion>();

            try
            {
                int entryIndex = _currentBody * 110 * 5 + _currentAction * 5 + _currentDir;
                using var idxFs = new System.IO.FileStream(idxPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                using var idxBr = new System.IO.BinaryReader(idxFs);
                idxFs.Seek((long)entryIndex * 12, System.IO.SeekOrigin.Begin);
                int offset = idxBr.ReadInt32();
                int length = idxBr.ReadInt32();
                if (offset < 0 || length <= 0) return new List<HexRegion>();

                byte[] raw = new byte[length];
                using var mulFs = new System.IO.FileStream(mulPath,
                    System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                mulFs.Seek(offset, System.IO.SeekOrigin.Begin);
                mulFs.Read(raw, 0, raw.Length);

                return BuildMulRegions(raw, edit);


            }
            catch { return new List<HexRegion>(); }
        }
        #endregion

        #region [ Diagnosen ] - Button-Handler zum Aufrufen der Diagnose-Dialogs
        /// <summary>
        /// Displays complete IDX diagnostics for the currently selected body.
        /// Good to see why certain actions are red.
        /// </summary>
        private void btnDiagSingle_Click(object sender, EventArgs e)
        {
            if (_fileType == 0 || _fileType == 6) return;
            MulAnimDiagnostics.ShowSingleDiagDialog(_currentBody, _fileType, this);
        }

        /// <summary>
        /// Compare a functioning body to a broken one.
        /// Shows exactly where the difference lies (BodyConverter, IDX, etc.)
        /// </summary>
        private void btnDiagCompare_Click(object sender, EventArgs e)
        {
            if (_fileType == 0 || _fileType == 6) return;

            // Works with the current body as "broken"
            // Ask about the functioning body
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Comparison: Body {_currentBody} (currently) with which functioning body?\n\n" +
                "Enter the Body ID of the working body:",
                "comparative diagnosis", "0");

            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int workingBody)) return;

            MulAnimDiagnostics.ShowComparisonDialog(workingBody, _currentBody, _fileType, this);
        }

        /// <summary>
        /// Scans ALL bodies of the loaded file and lists all those with problems.
        /// Please note: this may take some time with large files (~1-2 seconds).
        /// </summary>
        private void btnDiagMassScan_Click(object sender, EventArgs e)
        {
            if (_fileType == 0 || _fileType == 6) return;

            var result = MessageBox.Show(
                $"Alle Bodies in anim{_fileType}.mul scannen?\n" +
                "Only bodies with problems (PARTIAL/BROKEN) are output.",
                "Bulk scan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try { MulAnimDiagnostics.ShowMassScanDialog(_fileType, this); }
            finally { Cursor = Cursors.Default; }
        }
        #endregion

        #region [  Action Map Diagnosen Mul ]

        #region [ Diagnosen Mul ]

        private void BtnAnimMap_Click(object sender, EventArgs e)
        {
            if (_fileType == 0 || _fileType == 6) return;
            ShowBodyAnimationMap(_currentBody, _fileType);
        }

        /// <summary>
        /// Full animation map for a body:
        /// Body → Action → Dir → Block info → frameCount → Frame dimensions
        /// Shows what is valid / empty / corrupt and which action name belongs to it.
        /// Resolves physical body index via bodyconv.def for FileType 2-5.
        /// Includes full diagnostics when OOB is detected.
        /// </summary>

        #region [ ShowBodyAnimationMap ]

        /// <summary>
        /// Full animation map for a body:
        /// Body → Action → Dir → Block info → frameCount → Frame dimensions
        /// Shows what is valid / empty / corrupt and which action name belongs to it.
        /// Resolves physical body index via bodyconv.def for FileType 2-5.
        /// Falls back to Translate(), body.def and a full anim*.idx scan when OOB.
        /// Includes extended debug output per entry when checkBoxDiagInfo is checked.
        /// </summary>
        private void ShowBodyAnimationMap(int body, int fileType)
        {
            string mulName = fileType == 1 ? "anim.mul" : $"anim{fileType}.mul";
            string idxName = fileType == 1 ? "anim.idx" : $"anim{fileType}.idx";
            string mulPath = Ultima.Files.GetFilePath(mulName);
            string idxPath = Ultima.Files.GetFilePath(idxName);
            if (string.IsNullOrEmpty(idxPath) || !System.IO.File.Exists(idxPath))
            {
                MessageBox.Show($"{idxName} not found.", "Action Map");
                return;
            }

            // ── Diag-Info flag ───────────────────────────────────────────────────
            bool diagInfoEnabled = checkBoxDiagInfo != null && checkBoxDiagInfo.Checked;

            // ── Action names by type ─────────────────────────────────────────────
            int animLength = Animations.GetAnimLength(body, fileType);
            string typLabel;
            string[] actionNames;

            if (animLength == 22)
            {
                typLabel = "H (Monster)";
                actionNames = new[] {
                    "Walk","Stand","Die1","Die2","Attack1","Attack2","Attack3",
                    "AttackBow","AttackCrossBow","AttackThrow","GetHit","Pillage",
                    "Stomp","Cast2","Cast3","BlockRight","BlockLeft","Idle",
                    "Fidget","Fly","TakeOff","GetHitInAir"
                };
            }
            else if (animLength == 13)
            {
                typLabel = "L (Animal)";
                actionNames = new[] {
                    "Walk","Run","Idle","Eat","Alert","Attack1","Attack2",
                    "GetHit","Die1","Fidget1","Fidget2","LieDown","Die2"
                };
            }
            else
            {
                typLabel = "P (Human/Equipment)";
                actionNames = new[] {
                    "Walk_01","WalkStaff_01","Run_01","RunStaff_01","Idle_01",
                    "Idle_01b","Fidget_Yawn","CombatIdle1H","CombatIdle1Hb",
                    "AttackSlash1H","AttackPierce1H","AttackBash1H","AttackBash2H",
                    "AttackSlash2H","AttackPierce2H","CombatAdvance1H","Spell1",
                    "Spell2","AttackBow","AttackCrossbow","GetHit_Fr_Hi",
                    "Die_Hard_Fwd","Die_Hard_Back","Horse_Walk","Horse_Run",
                    "Horse_Idle","Horse_Attack1H","Horse_AttackBow",
                    "Horse_AttackCrossbow","Horse_Attack2H","Block_Shield",
                    "Punch_Jab","Bow_Lesser","Salute_Armed","Ingest_Eat"
                };
            }

            // ── Resolve physical body index via bodyconv.def ─────────────────────
            int physicalBody = body;
            int physicalFileType = fileType;
            string bodyconvPath = Ultima.Files.GetFilePath("bodyconv.def");
            if (!string.IsNullOrEmpty(bodyconvPath) && File.Exists(bodyconvPath))
            {
                try
                {
                    foreach (string rawLine in File.ReadLines(bodyconvPath))
                    {
                        int ci = rawLine.IndexOf('#');
                        string clean = (ci >= 0 ? rawLine.Substring(0, ci) : rawLine).Trim();
                        if (clean.Length == 0) continue;
                        string[] parts = clean.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) continue;
                        if (!int.TryParse(parts[0], out int lineBody)) continue;
                        if (lineBody != body) continue;

                        for (int targetCol = 1; targetCol <= 4; targetCol++)
                        {
                            if (parts.Length <= targetCol) continue;
                            if (int.TryParse(parts[targetCol], out int mapped) && mapped >= 0)
                            {
                                physicalBody = mapped;
                                physicalFileType = targetCol + 1;
                                break;
                            }
                        }
                        break;
                    }
                }
                catch { }
            }

            // ── IDX file size check ──────────────────────────────────────────────
            long idxFileLen = 0;
            try { idxFileLen = new System.IO.FileInfo(idxPath).Length; } catch { }
            (long idxRangeStart, long idxRangeEnd) = GetIdxBodyRange(physicalBody, animLength, physicalFileType);
            bool willBeOob = idxRangeEnd > idxFileLen;

            // ── Try Animations.Translate for OOB cases ───────────────────────────
            int translatedBody = body;
            int translatedFileType = fileType;
            bool translateAvailable = false;
            bool translateHelps = false;
            try
            {
                Ultima.Animations.Translate(ref translatedBody, ref translatedFileType);
                translateAvailable = true;
                if (translatedBody != body || translatedFileType != fileType)
                {
                    long idxStartTr = (long)(translatedBody * 110 * 5) * 12;
                    long idxEndTr = idxStartTr + (long)(animLength * 5 * 12);
                    string trIdxPath = translatedFileType == 1
                        ? Ultima.Files.GetFilePath("anim.idx")
                        : Ultima.Files.GetFilePath($"anim{translatedFileType}.idx");
                    long trIdxLen = 0;
                    if (!string.IsNullOrEmpty(trIdxPath) && System.IO.File.Exists(trIdxPath))
                        try { trIdxLen = new System.IO.FileInfo(trIdxPath).Length; } catch { }
                    else
                        trIdxLen = idxFileLen;
                    translateHelps = idxEndTr <= trIdxLen;
                }
            }
            catch { }

            // ── Try body.def (BodyTable) for OOB cases ───────────────────────────
            int bodyDefOldId = -1;
            bool bodyDefHelps = false;
            Ultima.BodyTableEntry btEntry = null;
            if (willBeOob && Ultima.BodyTable.Entries != null &&
                Ultima.BodyTable.Entries.TryGetValue(body, out btEntry))
            {
                bodyDefOldId = btEntry.OldId;
                long idxStartBd = (long)(bodyDefOldId * 110 * 5) * 12;
                long idxEndBd = idxStartBd + (long)(animLength * 5 * 12);
                bodyDefHelps = idxEndBd <= idxFileLen;
                if (bodyDefHelps) physicalBody = bodyDefOldId;
            }

            // ── If Translate gives a different file, switch to that ──────────────
            if (willBeOob && translateAvailable && translateHelps
                && translatedFileType != fileType)
            {
                physicalBody = translatedBody;
                physicalFileType = translatedFileType;
                mulName = physicalFileType == 1 ? "anim.mul" : $"anim{physicalFileType}.mul";
                idxName = physicalFileType == 1 ? "anim.idx" : $"anim{physicalFileType}.idx";
                mulPath = Ultima.Files.GetFilePath(mulName);
                idxPath = Ultima.Files.GetFilePath(idxName);
                idxFileLen = 0;
                try { idxFileLen = new System.IO.FileInfo(idxPath).Length; } catch { }
                idxRangeEnd = (long)(physicalBody * 110 * 5) * 12 + (long)(animLength * 5 * 12);
                willBeOob = idxRangeEnd > idxFileLen;
            }
            else if (willBeOob && translateAvailable && translateHelps
                     && translatedFileType == fileType)
            {
                physicalBody = translatedBody;
                idxRangeEnd = (long)(physicalBody * 110 * 5) * 12 + (long)(animLength * 5 * 12);
                willBeOob = idxRangeEnd > idxFileLen;
            }

            // ── Fallback: scan all anim*.idx files + MUL verification with frame size + visible size ─────
            // Bodies like 290 or 631 exist in a different anim file but have no
            // entry in bodyconv.def / body.def and Translate() returns them unchanged.

            string animScanNote = null;
            bool needsRedirect = willBeOob;

            if (!needsRedirect)
            {
                try
                {
                    using var testFs = new System.IO.FileStream(idxPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                    using var testBr = new System.IO.BinaryReader(testFs);
                    long testPos = GetIdxOffset(physicalBody, 0, 0, physicalFileType);
                    if (testPos + 12 <= testFs.Length)
                    {
                        testFs.Seek(testPos, System.IO.SeekOrigin.Begin);
                        int testOff = testBr.ReadInt32();
                        int testLen = testBr.ReadInt32();
                        if (testOff <= 0 || testOff == unchecked((int)0xFFFFFFFF) || testLen <= 0)
                            needsRedirect = true;
                    }
                }
                catch { }
            }

            if (needsRedirect)
            {
                for (int ft = 1; ft <= 5; ft++)
                {
                    if (ft == physicalFileType) continue;
                    string scanIdxName = ft == 1 ? "anim.idx" : $"anim{ft}.idx";
                    string scanIdxPath = Ultima.Files.GetFilePath(scanIdxName);
                    if (string.IsNullOrEmpty(scanIdxPath) || !System.IO.File.Exists(scanIdxPath)) continue;

                    try
                    {
                        long scanIdxLen = new System.IO.FileInfo(scanIdxPath).Length;
                        long scanRangeStart = GetIdxOffset(body, 0, 0, ft);
                        long scanRangeEnd = scanRangeStart + (long)(animLength * 5 * 12);
                        if (scanRangeEnd > scanIdxLen) continue;

                        using var scanFs = new System.IO.FileStream(scanIdxPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                        using var scanBr = new System.IO.BinaryReader(scanFs);
                        scanFs.Seek(scanRangeStart, System.IO.SeekOrigin.Begin);
                        int chkOff = scanBr.ReadInt32();
                        int chkLen = scanBr.ReadInt32();
                        if (chkOff <= 0 || chkOff == unchecked((int)0xFFFFFFFF) || chkLen <= 0) continue;

                        // === MUL-VERIFY + visible size ===
                        string scanMulName = ft == 1 ? "anim.mul" : $"anim{ft}.mul";
                        string scanMulPath = Ultima.Files.GetFilePath(scanMulName);
                        bool verified = false;
                        int verifiedFc = 0;
                        int headerW = 0, headerH = 0;
                        int visibleW = 0, visibleH = 0;
                        uint miniHash = 0;

                        if (!string.IsNullOrEmpty(scanMulPath) && System.IO.File.Exists(scanMulPath))
                        {
                            using var mulVerify = new System.IO.FileStream(scanMulPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                            if (chkOff + 518 <= mulVerify.Length)
                            {
                                mulVerify.Seek(chkOff + 512, System.IO.SeekOrigin.Begin);
                                int fc = mulVerify.ReadByte() | (mulVerify.ReadByte() << 8);
                                if (fc >= 1 && fc <= 256)
                                {
                                    mulVerify.Seek(chkOff + 514, System.IO.SeekOrigin.Begin);
                                    mulVerify.ReadByte(); mulVerify.ReadByte();
                                    int relOff = mulVerify.ReadByte() | (mulVerify.ReadByte() << 8);
                                    int absOff = 512 + relOff;

                                    if (chkOff + absOff + 8 <= mulVerify.Length)
                                    {
                                        mulVerify.Seek(chkOff + absOff, System.IO.SeekOrigin.Begin);
                                        short cx = (short)(mulVerify.ReadByte() | (mulVerify.ReadByte() << 8));
                                        short cy = (short)(mulVerify.ReadByte() | (mulVerify.ReadByte() << 8));
                                        int w = mulVerify.ReadByte() | (mulVerify.ReadByte() << 8);
                                        int h = mulVerify.ReadByte() | (mulVerify.ReadByte() << 8);

                                        headerW = w;
                                        headerH = h;

                                        // Approximate visible size (remove padding – fits perfectly)
                                        visibleW = Math.Max(1, w - 4);
                                        visibleH = Math.Max(1, h - 10);

                                        // Mini-Hash der ersten 64 Bytes
                                        if (chkOff + absOff + 64 <= mulVerify.Length)
                                        {
                                            mulVerify.Seek(chkOff + absOff, System.IO.SeekOrigin.Begin);
                                            byte[] buf = new byte[64];
                                            mulVerify.Read(buf, 0, 64);
                                            for (int i = 0; i < 64; i++)
                                                miniHash = (miniHash * 31) + buf[i];
                                        }

                                        verified = true;
                                        verifiedFc = fc;
                                    }
                                }
                            }
                        }

                        if (!verified) continue;

                        physicalBody = body;
                        physicalFileType = ft;
                        mulName = scanMulName;
                        idxName = scanIdxName;
                        mulPath = scanMulPath;
                        idxPath = scanIdxPath;
                        idxFileLen = scanIdxLen;
                        idxRangeEnd = scanRangeEnd;
                        willBeOob = false;

                        animScanNote = $"anim-scan: body {body} found in {idxName} (FT{ft}) " +
                                       $"→ Verified fc={verifiedFc} ({headerW}×{headerH} visible ~{visibleW}×{visibleH}) " +
                                       $"Hash=0x{miniHash:X8} in {mulName}";
                        break;
                    }
                    catch { }
                }

                if (willBeOob)
                    animScanNote = $"anim-scan: no valid file found for body {body}";
            }

            // ── Write diagnostics file ───────────────────────────────────────────
            string diagPath = null;
            if (diagInfoEnabled)
            {
                var diagSb = new System.Text.StringBuilder();
                diagSb.AppendLine($"=== DIAGNOSTICS: Body {body} FileType {fileType} ===");
                diagSb.AppendLine();
                diagSb.AppendLine($"IDX file : {idxName}");
                diagSb.AppendLine($"IDX file size : {idxFileLen} bytes ({idxFileLen / 12} entries)");
                diagSb.AppendLine($"Physical body : {physicalBody}");
                diagSb.AppendLine($"IDX range needed : [{idxRangeStart}..{idxRangeEnd}] " +
                                  $"({idxRangeEnd - idxRangeStart} bytes)");
                diagSb.AppendLine($"IDX range fits : {(!willBeOob ? "YES ✓" : "NO — OOB ✗")}");
                if (animScanNote != null)
                    diagSb.AppendLine($"Anim-scan result : {animScanNote}");
                diagSb.AppendLine();
                diagSb.AppendLine($"bodyconv.def lookup for body {body}:");
                string bcDiagPath = Ultima.Files.GetFilePath("bodyconv.def");
                if (!string.IsNullOrEmpty(bcDiagPath) && System.IO.File.Exists(bcDiagPath))
                {
                    bool found = false;
                    try
                    {
                        foreach (string rawLine in System.IO.File.ReadLines(bcDiagPath))
                        {
                            int ci = rawLine.IndexOf('#');
                            string clean = (ci >= 0 ? rawLine.Substring(0, ci) : rawLine).Trim();
                            if (clean.Length == 0) continue;
                            string[] parts = clean.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length < 2) continue;
                            if (!int.TryParse(parts[0], out int lb) || lb != body) continue;
                            diagSb.AppendLine($" Line : {string.Join(" ", parts)}");
                            diagSb.AppendLine($" Cols : [0]=body [1]=anim2 [2]=anim3 [3]=anim4 [4]=anim5");
                            found = true;
                            break;
                        }
                    }
                    catch { }
                    if (!found) diagSb.AppendLine(" Not found in bodyconv.def");
                }
                else diagSb.AppendLine(" bodyconv.def not found");
                diagSb.AppendLine();
                diagSb.AppendLine($"body.def (BodyTable) lookup for body {body}:");
                if (bodyDefOldId >= 0 && btEntry != null)
                    diagSb.AppendLine($" Found : {body} → OldId={bodyDefOldId} fits={bodyDefHelps}");
                else
                    diagSb.AppendLine(" Not found in body.def");
                diagSb.AppendLine();
                diagSb.AppendLine($"Animations.Translate({body}, {fileType}):");
                if (translateAvailable)
                    diagSb.AppendLine($" Result: body={translatedBody} fileType={translatedFileType}" +
                                      $" helps={translateHelps}");
                else
                    diagSb.AppendLine(" Not available");

                diagPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"ActionMap_Diag_Body{body}_FT{fileType}.txt");
                System.IO.File.WriteAllText(diagPath, diagSb.ToString());
            }

            // ── Build report ─────────────────────────────────────────────────────
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Action Map: Body {body} [{typLabel}] FileType {fileType} ===");
            sb.AppendLine($"File: {idxName} / {mulName}");
            sb.AppendLine($"Physical body index in IDX: {physicalBody}" +
                          (physicalBody != body
                              ? $" (mapped from {body} via " +
                                (bodyDefHelps ? "body.def" : "bodyconv.def") + ")"
                              : ""));
            if (physicalFileType != fileType)
                sb.AppendLine($"Redirected to FileType {physicalFileType} ({idxName} / {mulName})");
            if (animScanNote != null)
                sb.AppendLine($"ℹ {animScanNote}");
            sb.AppendLine($"AnimLength: {animLength} → {animLength * 5} IDX entries" +
                          $" ({animLength * 5 * 12} bytes in IDX)");
            if (diagInfoEnabled)
                sb.AppendLine("[DiagInfo: ENABLED — extended debug output per entry active]");
            if (willBeOob)
            {
                sb.AppendLine();
                sb.AppendLine($"⚠ WARNING: IDX range [{idxRangeStart}..{idxRangeEnd}] exceeds" +
                              $" IDX file size {idxFileLen} — all entries will be OOB");
                sb.AppendLine($" bodyconv.def : {(physicalBody != body ? $"{body}→{physicalBody}" : "no mapping")}");
                sb.AppendLine($" body.def : {(bodyDefOldId >= 0 ? $"{body}→{bodyDefOldId} (fits={bodyDefHelps})" : "no entry")}");
                sb.AppendLine($" Translate() : {(translateAvailable ? $"{body}→{translatedBody} ft={translatedFileType} (helps={translateHelps})" : "n/a")}");
                sb.AppendLine($" anim-scan : {animScanNote ?? "not run"}");
                sb.AppendLine(diagInfoEnabled && diagPath != null
                    ? $" Diagnostics : {diagPath}"
                    : $" Diagnostics : (enable checkBoxDiagInfo to save diagnostics file)");
            }

            sb.AppendLine();
            sb.AppendLine($"{"Act",-4} {"Name",-24} {"Dir0",10} {"Dir1",10} {"Dir2",10}" +
                          $" {"Dir3",10} {"Dir4",10} {"Status",-8}");
            sb.AppendLine(new string('─', 90));
            int totalOk = 0, totalEmpty = 0, totalCorrupt = 0;

            // ── Extended debug StringBuilder (diagInfoEnabled only) ──────────────
            var extDbgSb = diagInfoEnabled ? new System.Text.StringBuilder() : null;
            if (diagInfoEnabled)
            {
                extDbgSb.AppendLine($"=== EXTENDED DEBUG: Body {body} FileType {fileType} ===");
                extDbgSb.AppendLine($"Physical body: {physicalBody} | File: {idxName}");
                if (animScanNote != null)
                    extDbgSb.AppendLine($"Anim-scan: {animScanNote}");
                extDbgSb.AppendLine();
            }

            try
            {
                using var idxFs = new System.IO.FileStream(idxPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                using var mulFs = (!string.IsNullOrEmpty(mulPath) && System.IO.File.Exists(mulPath))
                    ? new System.IO.FileStream(mulPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)
                    : null;
                using var br = new System.IO.BinaryReader(idxFs);

                for (int action = 0; action < animLength; action++)
                {
                    string actName = action < actionNames.Length ? actionNames[action] : $"Action{action:D2}";
                    var dirInfos = new string[5];
                    var dirDetails = new System.Text.StringBuilder();
                    bool anyValid = false, anyCorrupt = false, anyEmpty = false;

                    for (int dir = 0; dir < 5; dir++)
                    {
                        //long idxPos = (long)(physicalBody * 110 * 5 + action * 5 + dir) * 12;
                        long idxPos = GetIdxOffset(physicalBody, action, dir, physicalFileType);

                        if (idxPos + 12 > idxFs.Length)
                        {
                            dirInfos[dir] = "OOB";
                            anyCorrupt = true;
                            if (diagInfoEnabled)
                            {
                                extDbgSb.AppendLine($"--- Act={action:D2} ({actName}) Dir={dir} ---");
                                extDbgSb.AppendLine($" IDX position : byte {idxPos} → OOB (file size={idxFs.Length})");
                                extDbgSb.AppendLine();
                            }
                            continue;
                        }

                        idxFs.Seek(idxPos, System.IO.SeekOrigin.Begin);
                        int rawOff = br.ReadInt32();
                        int rawLen = br.ReadInt32();
                        int rawExt = br.ReadInt32();

                        // ── Extended debug ────────────────────────────────────────
                        if (diagInfoEnabled)
                        {
                            // ── Extended debug (your complete original block) ─────
                            extDbgSb.AppendLine($"--- Act={action:D2} ({actName}) Dir={dir} ---");
                            extDbgSb.AppendLine($" IDX position : byte {idxPos} (= {physicalBody}*110*5*12 + {action}*5*12 + {dir}*12)");
                            extDbgSb.AppendLine($" IDX raw : off=0x{rawOff:X8} ({rawOff}) len=0x{rawLen:X8} ({rawLen}) ext=0x{rawExt:X8}");
                            extDbgSb.AppendLine($" off==-1 : {rawOff == -1} off==0xFFFFFFFF : {rawOff == unchecked((int)0xFFFFFFFF)} len<=0 : {rawLen <= 0}");

                            // Surrounding bodies — once per action with you=0
                            if (dir == 0)
                            {
                                extDbgSb.AppendLine($" Surrounding IDX entries (physBody±2, Act={action} Dir=0):");
                                for (int scanBody = Math.Max(0, physicalBody - 2); scanBody <= physicalBody + 2; scanBody++)
                                {
                                    long scanPos = GetIdxOffset(scanBody, action, 0, physicalFileType);
                                    if (scanPos + 12 > idxFs.Length)
                                    {
                                        extDbgSb.AppendLine($" Body {scanBody,4}: OOB");
                                        continue;
                                    }
                                    idxFs.Seek(scanPos, System.IO.SeekOrigin.Begin);
                                    int sOff = br.ReadInt32();
                                    int sLen = br.ReadInt32();
                                    br.ReadInt32();
                                    string valid = (sOff > 0 && sOff != unchecked((int)0xFFFFFFFF) && sLen > 0) ? "VALID" : "empty/invalid";
                                    extDbgSb.AppendLine($" Body {scanBody,4}: off=0x{sOff:X8} len={sLen,8} [{valid}]");
                                }
                            }

                            // All anim*.idx cross-scan — einmal bei Act=0 Dir=0
                            if (action == 0 && dir == 0)
                            {
                                extDbgSb.AppendLine($" Scanning all anim*.idx for body {body} Act=0 Dir=0:");
                                for (int ft = 1; ft <= 5; ft++)
                                {
                                    string scanIdxName2 = ft == 1 ? "anim.idx" : $"anim{ft}.idx";
                                    string scanIdxPath2 = Ultima.Files.GetFilePath(scanIdxName2);
                                    if (string.IsNullOrEmpty(scanIdxPath2) || !System.IO.File.Exists(scanIdxPath2))
                                    {
                                        extDbgSb.AppendLine($" FT{ft}: {scanIdxName2} not found");
                                        continue;
                                    }
                                    try
                                    {
                                        using var scanFs2 = new System.IO.FileStream(scanIdxPath2, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                                        using var scanBr2 = new System.IO.BinaryReader(scanFs2);
                                        long scanFileLen2 = scanFs2.Length;
                                        long scanPos2 = GetIdxOffset(body, 0, 0, ft);
                                        if (scanPos2 + 12 <= scanFileLen2)
                                        {
                                            scanFs2.Seek(scanPos2, System.IO.SeekOrigin.Begin);
                                            int sOff = scanBr2.ReadInt32();
                                            int sLen = scanBr2.ReadInt32();
                                            string valid = (sOff > 0 && sOff != unchecked((int)0xFFFFFFFF) && sLen > 0) ? "VALID ✓" : "empty";
                                            extDbgSb.AppendLine($" FT{ft}: {scanIdxName2,-16} body={body} off=0x{sOff:X8} len={sLen,8} [{valid}]");
                                        }
                                        else
                                        {
                                            extDbgSb.AppendLine($" FT{ft}: {scanIdxName2,-16} body={body} → OOB (file={scanFileLen2})");
                                        }
                                    }
                                    catch (Exception ex2)
                                    {
                                        extDbgSb.AppendLine($" FT{ft}: error — {ex2.Message}");
                                    }
                                }

                                extDbgSb.AppendLine($" UOP check for body {body}:");
                                if (_uopManager != null)
                                {
                                    int uopFoundAct = -1, uopFoundDir = -1;
                                    for (int a = 0; a < animLength && uopFoundAct < 0; a++)
                                        for (int d = 0; d < 5 && uopFoundAct < 0; d++)
                                            if (_uopManager.GetAnimationData(body, a, d) != null)
                                            { uopFoundAct = a; uopFoundDir = d; }
                                    extDbgSb.AppendLine(uopFoundAct >= 0 ? $" Found in UOP at Act={uopFoundAct} Dir={uopFoundDir} ✓" : " Not found in UOP");
                                }
                                else
                                {
                                    extDbgSb.AppendLine(" UOP manager not available");
                                }
                            }
                            extDbgSb.AppendLine();
                        }
                        // ── End extended debug ────────────────────────────────────

                        if (rawOff < 0 || rawOff == unchecked((int)0xFFFFFFFF) || rawLen <= 0)
                        {
                            dirInfos[dir] = "empty";
                            anyEmpty = true;
                            continue;
                        }

                        if (mulFs == null || rawOff + 514 > mulFs.Length)
                        {
                            dirInfos[dir] = $"@{rawOff:X}";
                            anyValid = true;
                            continue;
                        }

                        mulFs.Seek(rawOff + 512, System.IO.SeekOrigin.Begin);
                        int fc = mulFs.ReadByte() | (mulFs.ReadByte() << 8);
                        if (fc == 0 || fc > 256)
                        {
                            dirInfos[dir] = $"!fc={fc}";
                            anyCorrupt = true;
                            continue;
                        }

                        if (rawOff + 518 <= mulFs.Length)
                        {
                            mulFs.Seek(rawOff + 514, System.IO.SeekOrigin.Begin);
                            mulFs.ReadByte(); mulFs.ReadByte();
                            int relOff = mulFs.ReadByte() | (mulFs.ReadByte() << 8);
                            int absOff = 512 + relOff;
                            if (rawOff + absOff + 8 <= mulFs.Length)
                            {
                                mulFs.Seek(rawOff + absOff, System.IO.SeekOrigin.Begin);
                                short cx = (short)(mulFs.ReadByte() | (mulFs.ReadByte() << 8));
                                short cy = (short)(mulFs.ReadByte() | (mulFs.ReadByte() << 8));
                                int w = mulFs.ReadByte() | (mulFs.ReadByte() << 8);
                                int h = mulFs.ReadByte() | (mulFs.ReadByte() << 8);
                                dirInfos[dir] = $"fc={fc}";
                                dirDetails.AppendLine($" Dir{dir}: offset=0x{rawOff:X8} len={rawLen,7} frames={fc,3} F0: {w,4}×{h,-4} CX={cx,4} CY={cy,4}");
                                anyValid = true;
                                continue;
                            }
                        }

                        dirInfos[dir] = $"fc={fc}";
                        anyValid = true;
                    }

                    if (anyValid) totalOk++;
                    if (anyEmpty && !anyValid) totalEmpty++;
                    if (anyCorrupt) totalCorrupt++;

                    string status = anyCorrupt ? "CORRUPT" : !anyValid ? "MISSING" : anyEmpty ? "PARTIAL" : "OK";
                    sb.Append($"{action:D2} {actName,-24}");
                    foreach (var d in dirInfos) sb.Append($" {d,10}");
                    sb.AppendLine($" {status,-8}");
                    if (dirDetails.Length > 0) sb.Append(dirDetails);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\nError reading files: {ex.Message}");
            }

            sb.AppendLine();
            sb.AppendLine(new string('─', 90));
            sb.AppendLine($"Summary: OK={totalOk} PARTIAL/MISSING={totalEmpty} CORRUPT={totalCorrupt}");
            string fullText = sb.ToString();

            // ── Save main report ─────────────────────────────────────────────────
            string dumpPath = null;
            if (diagInfoEnabled)
            {
                dumpPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ActionMap_Body{body}_FT{fileType}.txt");
                System.IO.File.WriteAllText(dumpPath, fullText);
            }

            // ── Save extended debug file (only when diagInfoEnabled) ─────────────
            string extDbgPath = null;
            if (diagInfoEnabled && extDbgSb != null)
            {
                extDbgPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"ActionMap_ExtDebug_Body{body}_FT{fileType}.txt");
                System.IO.File.WriteAllText(extDbgPath, extDbgSb.ToString());
            }

            // ── Build dialog ─────────────────────────────────────────────────────
            var dlg = new Form
            {
                Text = $"Action Map — Body {body} [{typLabel}] FileType {fileType}" + (diagInfoEnabled ? " [DiagInfo ON]" : ""),
                Size = new System.Drawing.Size(960, 660),
                MinimumSize = new System.Drawing.Size(700, 400),
                StartPosition = FormStartPosition.CenterParent,
                Icon = this.Icon
            };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = System.Drawing.Color.FromArgb(38, 38, 50)
            };

            var btnCopyText = new Button
            {
                Text = "Copy text",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 90, 150),
                ForeColor = System.Drawing.Color.White,
                Width = 90,
                Height = 26,
                Left = 6,
                Top = 4,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            btnCopyText.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 130, 200);

            var btnCopyScreen = new Button
            {
                Text = "Copy screenshot",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(60, 60, 80),
                ForeColor = System.Drawing.Color.White,
                Width = 120,
                Height = 26,
                Left = 102,
                Top = 4,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            btnCopyScreen.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(90, 90, 110);

            var btnDiag = new Button
            {
                Text = "Open diagnostics",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(80, 50, 20),
                ForeColor = System.Drawing.Color.White,
                Width = 120,
                Height = 26,
                Left = 228,
                Top = 4,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            btnDiag.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 120, 40);
            btnDiag.Click += (s, ev) =>
            {
                try { System.Diagnostics.Process.Start("notepad.exe", diagPath); }
                catch { }
            };

            // ── "Open ExtDebug" button — nur sichtbar wenn diagInfoEnabled ────────
            var btnExtDbg = new Button
            {
                Text = "Open ExtDebug",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(30, 70, 30),
                ForeColor = System.Drawing.Color.White,
                Width = 120,
                Height = 26,
                Left = 354,
                Top = 4,
                Font = new System.Drawing.Font("Segoe UI", 8.5f),
                Visible = diagInfoEnabled && extDbgPath != null
            };
            btnExtDbg.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(60, 160, 60);
            btnExtDbg.Click += (s, ev) =>
            {
                if (extDbgPath != null)
                    try { System.Diagnostics.Process.Start("notepad.exe", extDbgPath); }
                    catch { }
            };

            var lblInfo = new Label
            {
                Text = $"Body {body}  ·  {typLabel}  ·  FileType {fileType}" +
                       $"  ·  {animLength} actions × 5 dirs" +
                       (physicalBody != body ? $"  ·  phys={physicalBody}" : "") +
                       (physicalFileType != fileType ? $"  ·  FT{physicalFileType}" : "") +
                       (animScanNote != null && !willBeOob ? $"  ·  scan→FT{physicalFileType}" : "") +
                       (diagInfoEnabled ? "  ·  DiagInfo ON" : ""),
                ForeColor = diagInfoEnabled
                    ? System.Drawing.Color.FromArgb(100, 220, 130)
                    : System.Drawing.Color.FromArgb(140, 140, 160),
                Font = new System.Drawing.Font("Segoe UI", 8.5f),
                AutoSize = true,
                Left = diagInfoEnabled ? 480 : 358,
                Top = 9
            };

            toolbar.Controls.Add(btnCopyText);
            toolbar.Controls.Add(btnCopyScreen);
            toolbar.Controls.Add(btnDiag);
            toolbar.Controls.Add(btnExtDbg);
            toolbar.Controls.Add(lblInfo);

            var txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
                Font = new System.Drawing.Font("Consolas", 9f),
                BackColor = System.Drawing.Color.FromArgb(28, 28, 32),
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 220)
            };

            foreach (var line in fullText.Split('\n'))
            {
                int start = txt.TextLength;
                txt.AppendText(line + "\n");

                System.Drawing.Color col;
                if (line.Contains("  OK"))
                    col = System.Drawing.Color.FromArgb(100, 220, 130);
                else if (line.Contains("CORRUPT"))
                    col = System.Drawing.Color.FromArgb(255, 100, 100);
                else if (line.Contains("MISSING") || line.Contains("PARTIAL"))
                    col = System.Drawing.Color.FromArgb(255, 200, 80);
                else if (line.TrimStart().StartsWith("Dir"))
                    col = System.Drawing.Color.FromArgb(140, 180, 255);
                else if (line.StartsWith("==="))
                    col = System.Drawing.Color.FromArgb(200, 180, 255);
                else if (line.StartsWith("Summary"))
                    col = System.Drawing.Color.FromArgb(200, 200, 100);
                else if (line.Contains("⚠") || line.Contains("WARNING"))
                    col = System.Drawing.Color.FromArgb(255, 140, 40);
                else if (line.Contains("ℹ") || line.Contains("anim-scan") || line.Contains("Redirected"))
                    col = System.Drawing.Color.FromArgb(100, 210, 255);
                else if (line.Contains("mapped from"))
                    col = System.Drawing.Color.FromArgb(255, 180, 80);
                else if (line.Contains("DiagInfo"))
                    col = System.Drawing.Color.FromArgb(100, 220, 130);
                else if (line.TrimStart().StartsWith("body") ||
                         line.TrimStart().StartsWith("Translate") ||
                         line.TrimStart().StartsWith("Diagnostics"))
                    col = System.Drawing.Color.FromArgb(160, 200, 255);
                else
                    col = System.Drawing.Color.FromArgb(220, 220, 220);

                txt.Select(start, line.Length);
                txt.SelectionColor = col;
            }
            txt.SelectionStart = 0;
            txt.SelectionLength = 0;

            var statusBar = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = $"  Saved: {dumpPath}" +
                       (diagInfoEnabled && extDbgPath != null
                           ? $"  |  ExtDebug: {extDbgPath}" : "") +
                       $"  |  OK={totalOk}" +
                       $"  MISSING/PARTIAL={totalEmpty}  CORRUPT={totalCorrupt}" +
                       (physicalBody != body ? $"  |  mapped: {body}→{physicalBody}" : "") +
                       (physicalFileType != fileType ? $"  |  FT{fileType}→FT{physicalFileType}" : "") +
                       (willBeOob ? "  |  ⚠ OOB — check diagnostics" : ""),
                Font = new System.Drawing.Font("Segoe UI", 8f),
                ForeColor = System.Drawing.Color.FromArgb(140, 140, 160),
                BackColor = System.Drawing.Color.FromArgb(38, 38, 50)
            };

            btnCopyText.Click += (s, ev) =>
            {
                Clipboard.SetText(fullText);
                btnCopyText.Text = "Copied!";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { btnCopyText.Text = "Copy text"; t.Stop(); t.Dispose(); };
                t.Start();
            };

            btnCopyScreen.Click += (s, ev) =>
            {
                var bmp = new System.Drawing.Bitmap(dlg.Width, dlg.Height);
                dlg.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                Clipboard.SetImage(bmp);
                btnCopyScreen.Text = "Copied!";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { btnCopyScreen.Text = "Copy screenshot"; t.Stop(); t.Dispose(); };
                t.Start();
            };

            dlg.Controls.Add(txt);
            dlg.Controls.Add(toolbar);
            dlg.Controls.Add(statusBar);
            dlg.ShowDialog(this);
        }

        /// <summary>
        /// Returns the correct byte offset in anim.idx / anim2.idx etc. for a given
        /// physical body + action + dir combination.
        /// UO anim.idx layout (FileType 1):
        ///   Bodies   0–199 → Human/Equipment  (35 actions × 5 dirs)
        ///   Bodies 200–399 → Animal           (13 actions × 5 dirs)
        ///   Bodies 400+    → Monster          (22 actions × 5 dirs)
        /// For anim2–anim5 the stride is always 22 (Monster-only files).
        /// </summary>
        private static long GetIdxOffset(int body, int action, int dir, int fileType)
        {
            long baseOffset;
            if (fileType == 1)
            {
                if (body < 200)
                    baseOffset = (long)body * 35 * 5 * 12;
                else if (body < 400)
                    baseOffset = (long)(200 * 35 * 5 + (body - 200) * 13 * 5) * 12;
                else
                    baseOffset = (long)(200 * 35 * 5 + 200 * 13 * 5 + (body - 400) * 22 * 5) * 12;
            }
            else // anim2–anim5: Monster only
            {
                baseOffset = (long)body * 22 * 5 * 12;
            }
            return baseOffset + ((long)action * 5 + dir) * 12;
        }

        /// <summary>
        /// Returns the byte range [start, end) in anim.idx for all actions of a body.
        /// </summary>
        private static (long start, long end) GetIdxBodyRange(int body, int animLength, int fileType)
        {
            long start = GetIdxOffset(body, 0, 0, fileType);
            long end = start + (long)animLength * 5 * 12;
            return (start, end);
        }
        #endregion
        #endregion



        #endregion

        #region [ Diagnosen UOP ]
        private void BtnAnimMapUop_Click(object sender, EventArgs e)
        {
            if (_fileType != 6 || _uopManager == null)
            {
                MessageBox.Show("Please load a UOP animation file first.", "Action Map UOP");
                return;
            }
            ShowBodyAnimationMapUop(_currentBody);
        }

        /// <summary>
        /// Full animation map for a UOP body:
        /// Body → Action → Dir → UOP block info → frameCount → Frame dimensions
        /// Shows what is valid / empty / missing and which action name belongs to it.
        /// </summary>

        private void ShowBodyAnimationMapUop(int body)
        {
            if (_uopManager == null)
            {
                MessageBox.Show("UOP manager not initialized.", "Action Map UOP");
                return;
            }

            // ── Action names ─────────────────────────────────────────────────────────
            int animLength = 26;
            string typLabel = "UOP Creature";
            string[] actionNames = new[] {
        "Walk","Run","Idle","Eat","Alert","Attack1","Attack2","GetHit",
        "Die1","Idle2","Fidget","LieDown","Die2","Attack3","AttackBow",
        "AttackCrossBow","AttackThrow","Pillage","Stomp","Cast2","Cast3",
        "BlockRight","BlockLeft","Fly","TakeOff","GetHitInAir"
    };

            // ── Detect actual action count ───────────────────────────────────────────
            int detectedLength = 0;
            for (int a = 0; a < 35; a++)
            {
                bool hasAny = false;
                for (int d = 0; d < 5; d++)
                {
                    if (_uopManager.GetAnimationData(body, a, d) != null)
                    { hasAny = true; break; }
                }
                if (hasAny) detectedLength = a + 1;
            }
            if (detectedLength > 0) animLength = detectedLength;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"=== Action Map UOP: Body {body}  [{typLabel}] ===");
            sb.AppendLine($"UOP Manager: {_uopManager.GetType().Name}");
            sb.AppendLine($"Detected actions: {animLength}  (scanned 0–{animLength - 1})");
            sb.AppendLine();
            sb.AppendLine($"{"Act",-4} {"Name",-24} {"Dir0",12} {"Dir1",12} {"Dir2",12} {"Dir3",12} {"Dir4",12}  {"Status",-8}");
            sb.AppendLine(new string('─', 100));

            int totalOk = 0, totalEmpty = 0, totalCorrupt = 0;

            for (int action = 0; action < animLength; action++)
            {
                string actName = action < actionNames.Length
                    ? actionNames[action] : $"Action{action:D2}";

                var dirInfos = new string[5];
                var dirDetails = new System.Text.StringBuilder();
                bool anyValid = false, anyEmpty = false, anyCorrupt = false;

                for (int dir = 0; dir < 5; dir++)
                {
                    // ── Step 1: get UOP entry ────────────────────────────────────────
                    var fileInfo = _uopManager.GetAnimationData(body, action, dir);
                    if (fileInfo == null)
                    {
                        dirInfos[dir] = "missing";
                        anyEmpty = true;
                        continue;
                    }

                    // ── Step 2: load raw data ────────────────────────────────────────
                    byte[] raw = null;
                    try { raw = fileInfo.GetData(); }
                    catch { }

                    if (raw == null || raw.Length < 30)
                    {
                        dirInfos[dir] = raw == null ? "no data" : "too short";
                        anyCorrupt = true;
                        continue;
                    }

                    // ── Step 3: verify Magic "AMOU" ──────────────────────────────────
                    if (raw[0] != 0x41 || raw[1] != 0x4D || raw[2] != 0x4F || raw[3] != 0x55)
                    {
                        dirInfos[dir] = "!magic";
                        anyCorrupt = true;
                        continue;
                    }

                    // ── Step 4: read frameCount @ Byte 28 (uint16 LE) ───────────────
                    int fc = raw[28] | (raw[29] << 8);

                    if (fc <= 0 || fc > 256)
                    {
                        dirInfos[dir] = $"!fc={fc}";
                        anyCorrupt = true;
                        continue;
                    }

                    // ── Step 5: read Frame[0] dimensions from UOP header ─────────────
                    // [16..17] CX  [18..19] CY  [20..21] W  [22..23] H
                    int cx = (short)(raw[16] | (raw[17] << 8));
                    int cy = (short)(raw[18] | (raw[19] << 8));
                    int w = raw[20] | (raw[21] << 8);
                    int h = raw[22] | (raw[23] << 8);
                    bool gotFrame = (w > 0 && w < 2048 && h > 0 && h < 2048);

                    string filePath = fileInfo.File?.FilePath ?? "(memory)";
                    string fileShort = System.IO.Path.GetFileName(filePath) ?? "(memory)";

                    dirInfos[dir] = $"fc={fc}";
                    dirDetails.AppendLine(
                        $"         Dir{dir}: file={fileShort,-28}  size={raw.Length,7}B" +
                        $"  frames={fc,3}" +
                        (gotFrame ? $"  F0: {w,4}×{h,-4}  CX={cx,4}  CY={cy,4}" : "  F0: n/a"));
                    anyValid = true;
                }

                if (anyValid) totalOk++;
                if (anyEmpty && !anyValid) totalEmpty++;
                if (anyCorrupt) totalCorrupt++;

                string status = anyCorrupt ? "CORRUPT"
                              : !anyValid ? "MISSING"
                              : anyEmpty ? "PARTIAL"
                              : "OK";

                sb.Append($"{action:D2}   {actName,-24}");
                foreach (var d in dirInfos) sb.Append($" {d,12}");
                sb.AppendLine($"  {status,-8}");

                if (dirDetails.Length > 0)
                    sb.Append(dirDetails);
            }

            sb.AppendLine();
            sb.AppendLine(new string('─', 100));
            sb.AppendLine($"Summary:  OK={totalOk}  PARTIAL/MISSING={totalEmpty}  CORRUPT={totalCorrupt}");

            string fullText = sb.ToString();

            // ── Save to temp file ────────────────────────────────────────────────────
            string dumpPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"ActionMapUOP_Body{body}.txt");
            System.IO.File.WriteAllText(dumpPath, fullText);

            // ── Build dialog ─────────────────────────────────────────────────────────
            var dlg = new Form
            {
                Text = $"Action Map UOP — Body {body}  [{typLabel}]",
                Size = new System.Drawing.Size(1060, 660),
                MinimumSize = new System.Drawing.Size(700, 400),
                StartPosition = FormStartPosition.CenterParent,
                Icon = this.Icon
            };

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = System.Drawing.Color.FromArgb(38, 38, 50)
            };

            var btnCopyText = new Button
            {
                Text = "Copy text",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 90, 150),
                ForeColor = System.Drawing.Color.White,
                Width = 90,
                Height = 26,
                Left = 6,
                Top = 4,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            btnCopyText.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 130, 200);

            var btnCopyScreen = new Button
            {
                Text = "Copy screenshot",
                FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(60, 60, 80),
                ForeColor = System.Drawing.Color.White,
                Width = 120,
                Height = 26,
                Left = 102,
                Top = 4,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            btnCopyScreen.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(90, 90, 110);

            var lblInfo = new Label
            {
                Text = $"Body {body}  ·  {typLabel}  ·  {animLength} actions × 5 dirs  ·  UOP",
                ForeColor = System.Drawing.Color.FromArgb(140, 140, 160),
                Font = new System.Drawing.Font("Segoe UI", 8.5f),
                AutoSize = true,
                Left = 230,
                Top = 9
            };

            toolbar.Controls.Add(btnCopyText);
            toolbar.Controls.Add(btnCopyScreen);
            toolbar.Controls.Add(lblInfo);

            var txt = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
                Font = new System.Drawing.Font("Consolas", 9f),
                BackColor = System.Drawing.Color.FromArgb(28, 28, 32),
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 220)
            };

            foreach (var line in fullText.Split('\n'))
            {
                int start = txt.TextLength;
                txt.AppendText(line + "\n");

                System.Drawing.Color col;
                if (line.Contains("  OK")) col = System.Drawing.Color.FromArgb(100, 220, 130);
                else if (line.Contains("CORRUPT")) col = System.Drawing.Color.FromArgb(255, 100, 100);
                else if (line.Contains("MISSING") || line.Contains("PARTIAL")) col = System.Drawing.Color.FromArgb(255, 200, 80);
                else if (line.TrimStart().StartsWith("Dir")) col = System.Drawing.Color.FromArgb(140, 180, 255);
                else if (line.StartsWith("===")) col = System.Drawing.Color.FromArgb(200, 180, 255);
                else if (line.StartsWith("Summary")) col = System.Drawing.Color.FromArgb(200, 200, 100);
                else col = System.Drawing.Color.FromArgb(220, 220, 220);

                txt.Select(start, line.Length);
                txt.SelectionColor = col;
            }
            txt.SelectionStart = 0;
            txt.SelectionLength = 0;

            var statusBar = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = $"  Saved: {dumpPath}  |  OK={totalOk}  MISSING/PARTIAL={totalEmpty}  CORRUPT={totalCorrupt}",
                Font = new System.Drawing.Font("Segoe UI", 8f),
                ForeColor = System.Drawing.Color.FromArgb(140, 140, 160),
                BackColor = System.Drawing.Color.FromArgb(38, 38, 50)
            };

            btnCopyText.Click += (s, ev) =>
            {
                Clipboard.SetText(fullText);
                btnCopyText.Text = "Copied!";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { btnCopyText.Text = "Copy text"; t.Stop(); t.Dispose(); };
                t.Start();
            };

            btnCopyScreen.Click += (s, ev) =>
            {
                var bmp = new System.Drawing.Bitmap(dlg.Width, dlg.Height);
                dlg.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                Clipboard.SetImage(bmp);
                btnCopyScreen.Text = "Copied!";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { btnCopyScreen.Text = "Copy screenshot"; t.Stop(); t.Dispose(); };
                t.Start();
            };

            dlg.Controls.Add(txt);
            dlg.Controls.Add(toolbar);
            dlg.Controls.Add(statusBar);
            dlg.ShowDialog(this);
        }

        #endregion
    }
}