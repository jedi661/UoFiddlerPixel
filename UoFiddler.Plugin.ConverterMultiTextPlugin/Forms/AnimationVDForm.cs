// /***************************************************************************
//  *
//  * $Author: Nikodemus (extended)
//  *
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Models.Uop;
using UoFiddler.Controls.Models.Uop.Imaging;
using UoFiddler.Controls.Uop;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// AnimationVDForm — standalone viewer/editor for .mul Anim files and .vd files.
    /// Supports manual loading via ComboBox (anim.mul) and via a dedicated VD load button.
    /// All frames are rendered exactly like AnimationEditForm (Paint, PictureBox, ListView).
    /// </summary>
    public partial class AnimationVDForm : Form
    {
        // ════════════════════════════════════════════════════════════════
        //  State Fields
        // ════════════════════════════════════════════════════════════════

        /// <summary>Currently selected MUL file type (1–5 for mul, 0 = none).</summary>
        private int _fileType = 0;

        /// <summary>Body/animation ID currently displayed.</summary>
        private int _currentBody = 0;

        /// <summary>Action index currently displayed.</summary>
        private int _currentAction = 0;

        /// <summary>Direction index (0–4) currently displayed.</summary>
        private int _currentDir = 0;

        /// <summary>Pivot point used for rendering on the main AnimationPictureBox.</summary>
        private Point _framePoint;

        /// <summary>Pivot point used for the VD preview PictureBox.</summary>
        private Point _vdFramePoint;

        /// <summary>Zoom factor applied when painting frames.</summary>
        private float _zoomFactor = 1.0f;

        /// <summary>Timer used for the animation playback loop.</summary>
        private System.Windows.Forms.Timer _animTimer;

        /// <summary>Current frame index during playback.</summary>
        private int _playFrameIndex = 0;

        /// <summary>True while the UI is being updated programmatically (prevents re-entrant events).</summary>
        private bool _updatingUi = false;

        // ── VD-specific state ────────────────────────────────────────────
        /// <summary>Decoded frames loaded from a .vd file, grouped by direction.</summary>
        private readonly Dictionary<int, List<Bitmap>> _vdFramesByDir = new Dictionary<int, List<Bitmap>>();

        /// <summary>Currently active .vd direction for the VD preview.</summary>
        private int _vdCurrentDir = 0;

        /// <summary>Detected animation type from the last loaded .vd file.</summary>
        private int _vdAnimType = -1;

        /// <summary>Full path of the currently loaded .vd file.</summary>
        private string _vdFilePath = string.Empty;

        // ── Animation names per file-type ────────────────────────────────
        private readonly string[][] _animNames =
        {
            new string[] // Animal (13 actions)
            {
                "Walk","Run","Idle","Eat","Alert","Attack1","Attack2",
                "GetHit","Die1","Idle2","Fidget","LieDown","Die2"
            },
            new string[] // Monster (22 actions)
            {
                "Walk","Idle","Die1","Die2","Attack1","Attack2","Attack3",
                "AttackBow","AttackCrossBow","AttackThrow","GetHit","Pillage","Stomp",
                "Cast2","Cast3","BlockRight","BlockLeft","Idle2","Fidget","Fly","TakeOff","GetHitInAir"
            },
            new string[] // Human (35 actions)
            {
                "Walk_01","WalkStaff_01","Run_01","RunStaff_01","Idle_01","Idle_02",
                "Fidget_Yawn_Stretch_01","CombatIdle1H_01","CombatIdle1H_02","AttackSlash1H_01",
                "AttackPierce1H_01","AttackBash1H_01","AttackBash2H_01","AttackSlash2H_01",
                "AttackPierce2H_01","CombatAdvance_1H_01","Spell1","Spell2","AttackBow_01",
                "AttackCrossbow_01","GetHit_Fr_Hi_01","Die_Hard_Fwd_01","Die_Hard_Back_01",
                "Horse_Walk_01","Horse_Run_01","Horse_Idle_01","Horse_Attack1H_SlashRight_01",
                "Horse_AttackBow_01","Horse_AttackCrossbow_01","Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01","Punch_Punch_Jab_01","Bow_Lesser_01",
                "Salute_Armed1h_01","Ingest_Eat_01"
            }
        };

        // ════════════════════════════════════════════════════════════════
        //  Constructor
        // ════════════════════════════════════════════════════════════════
        public AnimationVDForm()
        {
            InitializeComponent();

            // Initialise playback timer
            _animTimer = new System.Windows.Forms.Timer { Interval = 100 };
            _animTimer.Tick += AnimTimer_Tick;

            // Pivot defaults — set after layout so size is known
            Load += (s, e) =>
            {
                _framePoint = new Point(MulPreviewPictureBox.Width / 2, MulPreviewPictureBox.Height / 2);
                _vdFramePoint = new Point(VdPreviewPictureBox.Width / 2, VdPreviewPictureBox.Height / 2);
            };

            // Wire up paint handlers
            MulPreviewPictureBox.Paint += MulPreviewPictureBox_Paint;
            VdPreviewPictureBox.Paint += VdPreviewPictureBox_Paint;

            // Size-change — recenter pivot
            MulPreviewPictureBox.SizeChanged += (s, e) => { _framePoint = new Point(MulPreviewPictureBox.Width / 2, MulPreviewPictureBox.Height / 2); MulPreviewPictureBox.Invalidate(); };
            VdPreviewPictureBox.SizeChanged += (s, e) => { _vdFramePoint = new Point(VdPreviewPictureBox.Width / 2, VdPreviewPictureBox.Height / 2); VdPreviewPictureBox.Invalidate(); };

            // ListView owner-draw (thumbnails like AnimationEditForm)
            MulFramesListView.OwnerDraw = true;
            MulFramesListView.DrawItem += FramesListView_DrawItem;
            VdFramesListView.OwnerDraw = true;
            VdFramesListView.DrawItem += VdFramesListView_DrawItem;

            PopulateAnimFileComboBox();
        }

        // ════════════════════════════════════════════════════════════════
        //  ComboBox — Populate anim file list
        // ════════════════════════════════════════════════════════════════

        /// <summary>Fills the SelectFileComboBox with available animation file types.</summary>
        private void PopulateAnimFileComboBox()
        {
            SelectFileComboBox.Items.Clear();
            SelectFileComboBox.Items.Add("Anim (anim.mul / anim.idx)");
            SelectFileComboBox.Items.Add("Anim2 (anim2.mul)");
            SelectFileComboBox.Items.Add("Anim3 (anim3.mul)");
            SelectFileComboBox.Items.Add("Anim4 (anim4.mul)");
            SelectFileComboBox.Items.Add("Anim5 (anim5.mul)");
            SelectFileComboBox.SelectedIndex = 0;
        }

        /// <summary>Fires when the user picks a different animation file in the ComboBox.</summary>
        private void SelectFileComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fileType = SelectFileComboBox.SelectedIndex + 1; // 1-based file type
            LoadMulAnimations();
        }

        // ════════════════════════════════════════════════════════════════
        //  MUL — Load animation list into TreeView
        // ════════════════════════════════════════════════════════════════

        /// <summary>Loads all animations for the currently selected MUL file type into the TreeView.</summary>
        private void LoadMulAnimations()
        {
            if (_fileType == 0) return;

            MulAnimTreeView.BeginUpdate();
            try
            {
                MulAnimTreeView.Nodes.Clear();
                int count = Animations.GetAnimCount(_fileType);
                int validCount = 0;

                for (int i = 0; i < count; i++)
                {
                    int animLength = Animations.GetAnimLength(i, _fileType);
                    string typeChar = animLength == 22 ? "M" : animLength == 13 ? "A" : "H";

                    TreeNode bodyNode = new TreeNode
                    {
                        Tag = i,
                        Text = $"{typeChar}: {i}"
                    };

                    bool anyDefined = false;
                    string[] names = GetAnimNamesForLength(animLength);

                    for (int j = 0; j < animLength; j++)
                    {
                        bool defined = AnimationEdit.IsActionDefined(_fileType, i, j);
                        if (defined) anyDefined = true;

                        string actionName = (j < names.Length) ? names[j] : $"Action {j}";
                        TreeNode actionNode = new TreeNode
                        {
                            Tag = j,
                            Text = $"{j:D2} {actionName}",
                            ForeColor = defined ? Color.Black : Color.Red
                        };
                        bodyNode.Nodes.Add(actionNode);
                    }

                    if (!anyDefined)
                        bodyNode.ForeColor = Color.Red;
                    else
                        validCount++;

                    MulAnimTreeView.Nodes.Add(bodyNode);
                }

                AnimStatusLabel.Text = $"Animations: {validCount} / {count}";
            }
            finally
            {
                MulAnimTreeView.EndUpdate();
            }

            if (MulAnimTreeView.Nodes.Count > 0)
                MulAnimTreeView.SelectedNode = MulAnimTreeView.Nodes[0];
        }

        // ════════════════════════════════════════════════════════════════
        //  MUL — TreeView selection → populate frames
        // ════════════════════════════════════════════════════════════════

        private void MulAnimTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (MulAnimTreeView.SelectedNode == null) return;

            if (MulAnimTreeView.SelectedNode.Parent == null)
            {
                // Body node selected
                _currentBody = (int)MulAnimTreeView.SelectedNode.Tag;
                _currentAction = 0;
            }
            else
            {
                _currentBody = (int)MulAnimTreeView.SelectedNode.Parent.Tag;
                _currentAction = (int)MulAnimTreeView.SelectedNode.Tag;
            }

            PopulateMulFrames();
        }

        /// <summary>Fills MulFramesListView with thumbnails for the current body/action/dir.</summary>
        private void PopulateMulFrames()
        {
            if (_fileType == 0) return;

            MulFramesListView.BeginUpdate();
            try
            {
                MulFramesListView.Clear();
                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null) { MulFramesTrackBar.Maximum = 0; return; }

                Bitmap[] frames = edit.GetFrames();
                if (frames == null || frames.Length == 0) { MulFramesTrackBar.Maximum = 0; return; }

                int maxW = 80, maxH = 110;
                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i] == null) continue;
                    ListViewItem item = new ListViewItem(i.ToString()) { Tag = i };
                    MulFramesListView.Items.Add(item);
                    if (frames[i].Width > maxW) maxW = frames[i].Width;
                    if (frames[i].Height > maxH) maxH = frames[i].Height;
                }

                MulFramesListView.TileSize = new Size(maxW + 6, maxH + 6);

                _updatingUi = true;
                MulFramesTrackBar.Maximum = frames.Length - 1;
                MulFramesTrackBar.Value = 0;

                if (edit.Frames.Count > 0)
                {
                    CenterXNumericUpDown.Value = edit.Frames[0].Center.X;
                    CenterYNumericUpDown.Value = edit.Frames[0].Center.Y;
                }
                _updatingUi = false;
            }
            finally
            {
                MulFramesListView.EndUpdate();
            }

            MulPreviewPictureBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  MUL — Direction TrackBar
        // ════════════════════════════════════════════════════════════════

        private void DirectionTrackBar_ValueChanged(object sender, EventArgs e)
        {
            _currentDir = DirectionTrackBar.Value;
            DirectionLabel.Text = $"Direction: {_currentDir}";
            PopulateMulFrames();
        }

        // ════════════════════════════════════════════════════════════════
        //  MUL — Frames TrackBar / ListView selection
        // ════════════════════════════════════════════════════════════════

        private void MulFramesTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi) return;

            // Sync ListView selection
            if (MulFramesTrackBar.Value < MulFramesListView.Items.Count)
            {
                MulFramesListView.SelectedItems.Clear();
                var item = MulFramesListView.Items[MulFramesTrackBar.Value];
                item.Selected = true;
                item.EnsureVisible();
            }

            // Update center display
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit != null && MulFramesTrackBar.Value < edit.Frames.Count)
            {
                _updatingUi = true;
                CenterXNumericUpDown.Value = edit.Frames[MulFramesTrackBar.Value].Center.X;
                CenterYNumericUpDown.Value = edit.Frames[MulFramesTrackBar.Value].Center.Y;
                _updatingUi = false;
            }

            MulPreviewPictureBox.Invalidate();
        }

        private void MulFramesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) return;
            int idx = e.ItemIndex;
            if (idx >= 0 && idx <= MulFramesTrackBar.Maximum)
            {
                _updatingUi = true;
                MulFramesTrackBar.Value = idx;
                _updatingUi = false;
            }
            MulPreviewPictureBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  MUL — Paint (mirrors AnimationEditForm logic exactly)
        // ════════════════════════════════════════════════════════════════

        private void MulPreviewPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.Clear(Color.LightGray);

            // Crosshair
            e.Graphics.DrawLine(Pens.Black, new Point(_framePoint.X, 0), new Point(_framePoint.X, MulPreviewPictureBox.Height));
            e.Graphics.DrawLine(Pens.Black, new Point(0, _framePoint.Y), new Point(MulPreviewPictureBox.Width, _framePoint.Y));

            if (_fileType == 0) return;
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null) return;

            Bitmap[] frames = edit.GetFrames();
            int frameIdx = MulFramesTrackBar.Value;
            if (frames == null || frameIdx >= frames.Length || frames[frameIdx] == null) return;

            Bitmap bmp = frames[frameIdx];
            int cx = edit.Frames[frameIdx].Center.X;
            int cy = edit.Frames[frameIdx].Center.Y;

            int x = _framePoint.X - (int)(cx * _zoomFactor);
            int y = _framePoint.Y - (int)(cy * _zoomFactor) - (int)(bmp.Height * _zoomFactor);
            int w = (int)(bmp.Width * _zoomFactor);
            int h = (int)(bmp.Height * _zoomFactor);

            e.Graphics.DrawImage(bmp, new Rectangle(x, y, w, h));
        }

        // ════════════════════════════════════════════════════════════════
        //  MUL — OwnerDraw ListView thumbnails
        // ════════════════════════════════════════════════════════════════

        private void FramesListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (_fileType == 0) { e.DrawDefault = true; return; }
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            Bitmap[] frames = edit?.GetFrames();
            int idx = (int)e.Item.Tag;
            if (frames == null || idx >= frames.Length || frames[idx] == null) { e.DrawDefault = true; return; }

            Bitmap bmp = frames[idx];
            e.DrawBackground();
            if (e.Item.Selected)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);

            // Scale to fit tile
            Rectangle dest = GetScaledRect(bmp.Width, bmp.Height, e.Bounds);
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(bmp, dest);
            e.DrawFocusRectangle();
        }

        // ════════════════════════════════════════════════════════════════
        //  VD — Load button
        // ════════════════════════════════════════════════════════════════

        /// <summary>Opens an OpenFileDialog, reads the .vd file and populates the VD TreeView.</summary>
        private void LoadVdButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Open .vd Animation File";
                ofd.Filter = "VD Files (*.vd)|*.vd|All Files (*.*)|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                _vdFilePath = ofd.FileName;

                // ════════════════════════════════════════════════════════════════
                // PRE-CHECK: Detect animation type BEFORE full loading
                // ════════════════════════════════════════════════════════════════
                try
                {
                    using (BinaryReader br = new BinaryReader(File.OpenRead(_vdFilePath)))
                    {
                        // File size validation
                        if (br.BaseStream.Length < 4)
                        {
                            MessageBox.Show("Invalid .vd file: file is too small (< 4 bytes).", "Load Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        short magic = br.ReadInt16();
                        short animType = br.ReadInt16();

                        // Get human-readable type name
                        string typeName = DetectVdAnimationType(animType);
                        string fileName = Path.GetFileNameWithoutExtension(_vdFilePath);

                        // Update UI labels IMMEDIATELY with type info
                        VdTypeLabel.Text = $"Type: {typeName} ({animType})";
                        VdFileLabel.Text = $"File: {Path.GetFileName(_vdFilePath)}";

                        // Debug output
                        Debug.WriteLine($"[LoadVdButton] Pre-check complete:");
                        Debug.WriteLine($"  File: {fileName}");
                        Debug.WriteLine($"  Type: {typeName}");
                        Debug.WriteLine($"  Size: {br.BaseStream.Length} bytes");

                        // ────────────────────────────────────────────────────────
                        // WARNING for unknown types
                        // ────────────────────────────────────────────────────────
                        if (animType == 0)
                        {
                            var result = MessageBox.Show(
                                $"⚠ Unknown Animation Type\n\n" +
                                $"File: {Path.GetFileName(_vdFilePath)}\n" +
                                $"Type Code: {animType}\n\n" +
                                $"This file's animation type is unrecognized.\n" +
                                $"Continue loading anyway?",
                                "Unknown Type",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);  // Default = No

                            if (result != DialogResult.Yes)
                            {
                                VdStatusLabel.Text = "Load cancelled (unknown type)";
                                return;
                            }
                        }

                        // ────────────────────────────────────────────────────────
                        // INFO display for known types
                        // ────────────────────────────────────────────────────────
                        string typeInfo = animType switch
                        {
                            1 => "Animal — 13 actions",
                            2 => "Monster — 22 actions",
                            3 => "Human / Equipment — 35 actions",
                            4 => "Sea Monster / Extended Animal",
                            6 => "UOP Creature",
                            _ => $"Custom Type {animType}"
                        };

                        VdStatusLabel.Text = $"Loading: {Path.GetFileName(_vdFilePath)} ({typeInfo})...";
                    }
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show($"File access error:\n{ioEx.Message}", "Read Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    VdStatusLabel.Text = "Load failed (file access error)";
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read .vd file header:\n{ex.Message}", "Read Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    VdStatusLabel.Text = "Load failed (header read error)";
                    return;
                }

                // ════════════════════════════════════════════════════════════════
                // NOW load the full file with all directions and frames
                // ════════════════════════════════════════════════════════════════
                LoadVdFile(_vdFilePath);
            }
        }
        
        private void LoadVdFile(string path)
        {
            _vdFramesByDir.Clear();
            VdAnimTreeView.Nodes.Clear();
            VdFramesListView.Clear();
            VdPreviewPictureBox.Invalidate();

            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
                {
                    short magic = br.ReadInt16();
                    short animType = br.ReadInt16();
                    _vdAnimType = animType;

                    string typeLabel = DetectVdAnimationType(animType);
                    VdTypeLabel.Text = $"Type: {typeLabel} ({animType})";
                    VdFileLabel.Text = $"File: {Path.GetFileName(path)}";

                    // ── Alle 5 Directions einlesen ────────────────────────
                    int totalFrames = 0;
                    for (int dir = 0; dir < 5; dir++)
                    {
                        List<Bitmap> dirFrames = ReadVdDirection(br);
                        _vdFramesByDir[dir] = dirFrames;
                        totalFrames += dirFrames.Count;
                    }

                    // ── Pseudo-Index Tree aufbauen ────────────────────────
                    //  Root = Dateiname + AnimType
                    //  Ebene 1 = Direction 0-4
                    //    Ebene 2 (optional) = Frame 0-N (als Info-Nodes)
                    // ─────────────────────────────────────────────────────
                    VdAnimTreeView.BeginUpdate();
                    try
                    {
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        TreeNode rootNode = new TreeNode($"{fileName}  [{typeLabel}]") { Tag = -1 };

                        // Richtungsnamen aus AnimType ableiten
                        // (spiegelt genau MulAnimTreeView wider)
                        int animLen = animType switch
                        {
                            1 => 13,
                            2 => 22,
                            3 => 35,
                            _ => 13   // Fallback: Animal
                        };

                        string[] actionNames = GetAnimNamesForLength(animLen);

                        for (int dir = 0; dir < 5; dir++)
                        {
                            var dirFrames = _vdFramesByDir[dir];
                            int fCount = dirFrames.Count;

                            // Direction-Node  (Tag = dir, damit TrackBar sync funktioniert)
                            TreeNode dirNode = new TreeNode($"Dir {dir}  ({fCount} Frames)")
                            {
                                Tag = dir,
                                ForeColor = fCount > 0
                                    ? System.Drawing.Color.FromArgb(100, 220, 130)
                                    : System.Drawing.Color.Red
                            };

                            // Frame-Subnodes  (Tag = frame-Index, negativ kodiert um dir-Nodes zu unterscheiden)
                            for (int f = 0; f < fCount; f++)
                            {
                                bool hasPixels = dirFrames[f] != null;
                                // Action-Name: wenn mehr Frames als Actions → einfach Frame-Nr anzeigen
                                string actionName = (f < actionNames.Length) ? actionNames[f] : $"Frame {f}";
                                TreeNode frameNode = new TreeNode($"  {f:D2}  {actionName}")
                                {
                                    Tag = EncodeFrameTag(dir, f),
                                    ForeColor = hasPixels
                                        ? System.Drawing.Color.FromArgb(200, 200, 220)
                                        : System.Drawing.Color.Gray
                                };
                                dirNode.Nodes.Add(frameNode);
                            }

                            rootNode.Nodes.Add(dirNode);
                        }

                        VdAnimTreeView.Nodes.Add(rootNode);
                        rootNode.Expand();
                        // Direction 0 direkt aufklappen
                        if (rootNode.Nodes.Count > 0)
                            rootNode.Nodes[0].Expand();
                    }
                    finally
                    {
                        VdAnimTreeView.EndUpdate();
                    }

                    VdStatusLabel.Text = $"Geladen: {totalFrames} Frames in 5 Directions.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der .vd-Datei:\n{ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                VdStatusLabel.Text = "Laden fehlgeschlagen.";
            }

            // Direction 0 als Standard
            _vdCurrentDir = 0;
            _updatingUi = true;
            VdDirectionTrackBar.Value = 0;
            _updatingUi = false;
            PopulateVdFrames();
        }

        /// <summary>
        /// Reads one direction block from the binary reader.
        /// Mirrors AnimationEdit.LoadFromVD frame decoding.
        /// </summary>       

        private List<Bitmap> ReadVdDirection(BinaryReader br)
        {
            var frames = new List<Bitmap>();
            try
            {
                const int palette_size = 256;
                ushort[] palette = new ushort[palette_size];
                for (int i = 0; i < palette_size; i++)
                    palette[i] = br.ReadUInt16();

                int frameCount = br.ReadInt32();

                // Plausibilitätsprüfung gegen korrupte Daten
                if (frameCount < 0 || frameCount > 4096)
                    return frames;

                int[] lookups = new int[frameCount];
                for (int i = 0; i < frameCount; i++)
                    lookups[i] = br.ReadInt32();

                long dataStart = br.BaseStream.Position;

                for (int f = 0; f < frameCount; f++)
                {
                    br.BaseStream.Seek(dataStart + lookups[f], SeekOrigin.Begin);

                    short centerX = br.ReadInt16();
                    short centerY = br.ReadInt16();
                    ushort width = br.ReadUInt16();
                    ushort height = br.ReadUInt16();

                    if (width == 0 || height == 0)
                    {
                        frames.Add(null);
                        continue;
                    }

                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height),
                                                   ImageLockMode.WriteOnly,
                                                   PixelFormat.Format32bppArgb);
                    unsafe
                    {
                        int* line = (int*)bd.Scan0;
                        int delta = bd.Stride / 4;

                        // ╔══ FIX: uint statt int — verhindert Overflow bei Bit 31 ══╗
                        uint header = br.ReadUInt32();

                        while (header != 0x7FFF7FFF)
                        {
                            // Bitfelder exakt wie im Original UOFiddler AnimationEdit
                            int runY = (int)((header >> 12) & 0x3FF);
                            int runX = (int)(header & 0xFFF);
                            int count = (int)((header >> 22) & 0x3FF);

                            // Bounds-Check damit kein Zugriff außerhalb der Bitmap
                            if (runY >= height || runX >= width || count <= 0)
                            {
                                header = br.ReadUInt32();
                                continue;
                            }

                            int maxPixels = width - runX;
                            if (count > maxPixels) count = maxPixels;

                            int* cur = line + (runY * delta) + runX;

                            for (int p = 0; p < count; p++)
                            {
                                byte paletteIdx = br.ReadByte();
                                ushort c16 = palette[paletteIdx];

                                if (c16 == 0)
                                {
                                    cur++;
                                    continue;
                                }

                                int r = ((c16 >> 10) & 0x1F) << 3;
                                int g = ((c16 >> 5) & 0x1F) << 3;
                                int b = (c16 & 0x1F) << 3;
                                *cur++ = (255 << 24) | (r << 16) | (g << 8) | b;
                            }

                            header = br.ReadUInt32();
                        }
                    }

                    bmp.UnlockBits(bd);
                    frames.Add(bmp);
                }
            }
            catch (EndOfStreamException)
            {
                // Partielle Daten — was dekodiert wurde zurückgeben
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ReadVdDirection] Exception: {ex.Message}");
            }

            return frames;
        }

        /// <summary>Returns a human-readable animation type string from the .vd animType value.</summary>
        private string DetectVdAnimationType(int animType)
        {
            return animType switch
            {
                0 => "Unknown",
                1 => "Animal (13 actions)",
                2 => "Monster (22 actions)",
                3 => "Human / Equipment (35 actions)",
                4 => "Sea Monster / Extended Animal",
                6 => "UOP Creature",
                _ => $"Custom ({animType})"
            };
        }

        private void VdAnimTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (VdAnimTreeView.SelectedNode == null) return;

            object tag = VdAnimTreeView.SelectedNode.Tag;
            if (tag == null) return;

            long tagVal = Convert.ToInt64(tag);

            if (tagVal >= 0 && tagVal < 5)
            {
                // Direction-Node ausgewählt
                int dir = (int)tagVal;
                _vdCurrentDir = dir;
                _updatingUi = true;
                VdDirectionTrackBar.Value = dir;
                VdDirectionLabel.Text = $"Direction: {dir}";
                _updatingUi = false;
                PopulateVdFrames();
            }
            else if (tagVal < 0)
            {
                // Frame-Node ausgewählt → Direction + Frame direkt springen
                DecodeFrameTag(tagVal, out int dir, out int frameIdx);
                if (dir != _vdCurrentDir)
                {
                    _vdCurrentDir = dir;
                    _updatingUi = true;
                    VdDirectionTrackBar.Value = dir;
                    VdDirectionLabel.Text = $"Direction: {dir}";
                    _updatingUi = false;
                    PopulateVdFrames();
                }
                // Frame direkt anspringen
                if (frameIdx >= 0 && frameIdx <= VdFramesTrackBar.Maximum)
                {
                    _updatingUi = true;
                    VdFramesTrackBar.Value = frameIdx;
                    _updatingUi = false;
                }
                VdPreviewPictureBox.Invalidate();
            }
            // tagVal == -1 → Root-Node, nichts tun
        }

        // ────────────────────────────────────────────────────────────────
        // NEUE HILFSMETHODEN — Tag-Kodierung für Frame-Nodes
        //  Dir 0-4, Frame 0-4095 → eindeutiger negativer long
        // ────────────────────────────────────────────────────────────────

        private static long EncodeFrameTag(int dir, int frame)
        {
            // Negativ kodiert: -(dir * 10000 + frame + 1)
            // Damit Tag >= 0 immer ein Direction-Node ist
            return -((long)dir * 10000 + frame + 1);
        }

        private static void DecodeFrameTag(long tag, out int dir, out int frame)
        {
            long val = -tag - 1;
            dir = (int)(val / 10000);
            frame = (int)(val % 10000);
        }

        // ════════════════════════════════════════════════════════════════
        //  VD — Direction TrackBar
        // ════════════════════════════════════════════════════════════════

        private void VdDirectionTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi) return;
            _vdCurrentDir = VdDirectionTrackBar.Value;
            VdDirectionLabel.Text = $"Direction: {_vdCurrentDir}";

            // Sync tree selection
            foreach (TreeNode root in VdAnimTreeView.Nodes)
                foreach (TreeNode child in root.Nodes)
                    if (child.Tag is int t && t == _vdCurrentDir)
                    { VdAnimTreeView.SelectedNode = child; break; }

            PopulateVdFrames();
        }

        // ════════════════════════════════════════════════════════════════
        //  VD — Populate frames ListView
        // ════════════════════════════════════════════════════════════════

        private void PopulateVdFrames()
        {
            VdFramesListView.BeginUpdate();
            try
            {
                VdFramesListView.Clear();
                if (!_vdFramesByDir.TryGetValue(_vdCurrentDir, out var frames) || frames == null)
                {
                    VdFramesTrackBar.Maximum = 0;
                    return;
                }

                int maxW = 80, maxH = 110;
                for (int i = 0; i < frames.Count; i++)
                {
                    if (frames[i] == null) continue;
                    ListViewItem item = new ListViewItem(i.ToString()) { Tag = i };
                    VdFramesListView.Items.Add(item);
                    if (frames[i].Width > maxW) maxW = frames[i].Width;
                    if (frames[i].Height > maxH) maxH = frames[i].Height;
                }

                VdFramesListView.TileSize = new Size(maxW + 6, maxH + 6);
                VdFramesTrackBar.Maximum = frames.Count > 0 ? frames.Count - 1 : 0;
                VdFramesTrackBar.Value = 0;
            }
            finally
            {
                VdFramesListView.EndUpdate();
            }

            VdPreviewPictureBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  VD — Frames TrackBar / ListView
        // ════════════════════════════════════════════════════════════════

        private void VdFramesTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi) return;
            int idx = VdFramesTrackBar.Value;
            if (idx < VdFramesListView.Items.Count)
            {
                VdFramesListView.SelectedItems.Clear();
                var item = VdFramesListView.Items[idx];
                item.Selected = true;
                item.EnsureVisible();
            }
            VdPreviewPictureBox.Invalidate();
        }

        private void VdFramesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) return;
            int idx = e.ItemIndex;
            if (idx >= 0 && idx <= VdFramesTrackBar.Maximum)
            {
                _updatingUi = true;
                VdFramesTrackBar.Value = idx;
                _updatingUi = false;
            }
            VdPreviewPictureBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  VD — Paint (mirrors MUL paint logic)
        // ════════════════════════════════════════════════════════════════

        private void VdPreviewPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.Clear(Color.LightGray);

            // Crosshair
            e.Graphics.DrawLine(Pens.DarkGray, new Point(_vdFramePoint.X, 0), new Point(_vdFramePoint.X, VdPreviewPictureBox.Height));
            e.Graphics.DrawLine(Pens.DarkGray, new Point(0, _vdFramePoint.Y), new Point(VdPreviewPictureBox.Width, _vdFramePoint.Y));

            if (!_vdFramesByDir.TryGetValue(_vdCurrentDir, out var frames) || frames == null) return;

            int frameIdx = VdFramesTrackBar.Value;
            if (frameIdx >= frames.Count || frames[frameIdx] == null) return;

            Bitmap bmp = frames[frameIdx];
            int w = (int)(bmp.Width * _zoomFactor);
            int h = (int)(bmp.Height * _zoomFactor);
            // Center the image on the pivot (no center-offset available from raw vd read without storing it separately)
            int x = _vdFramePoint.X - w / 2;
            int y = _vdFramePoint.Y - h;

            e.Graphics.DrawImage(bmp, new Rectangle(x, y, w, h));
        }

        // ════════════════════════════════════════════════════════════════
        //  VD — OwnerDraw ListView thumbnails
        // ════════════════════════════════════════════════════════════════

        private void VdFramesListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (!_vdFramesByDir.TryGetValue(_vdCurrentDir, out var frames)) { e.DrawDefault = true; return; }
            int idx = (int)e.Item.Tag;
            if (idx >= frames.Count || frames[idx] == null) { e.DrawDefault = true; return; }

            Bitmap bmp = frames[idx];
            e.DrawBackground();
            if (e.Item.Selected)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);

            Rectangle dest = GetScaledRect(bmp.Width, bmp.Height, e.Bounds);
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(bmp, dest);
            e.DrawFocusRectangle();
        }

        // ════════════════════════════════════════════════════════════════
        //  Playback (MUL)
        // ════════════════════════════════════════════════════════════════

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (_animTimer.Enabled)
            {
                _animTimer.Stop();
                PlayButton.Text = "▶ Play";
                return;
            }
            _playFrameIndex = 0;
            _animTimer.Start();
            PlayButton.Text = "■ Stop";
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            if (_fileType == 0) { _animTimer.Stop(); return; }
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            Bitmap[] frames = edit?.GetFrames();
            if (frames == null || frames.Length == 0) { _animTimer.Stop(); return; }

            _playFrameIndex = (_playFrameIndex + 1) % frames.Length;
            _updatingUi = true;
            MulFramesTrackBar.Value = _playFrameIndex;
            _updatingUi = false;
            MulPreviewPictureBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  Search — jump to body index
        // ════════════════════════════════════════════════════════════════

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(SearchTextBox.Text.Trim(), out int targetId))
            {
                MessageBox.Show("Please enter a valid integer animation ID.", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (TreeNode node in MulAnimTreeView.Nodes)
            {
                if (node.Tag is int id && id == targetId)
                {
                    MulAnimTreeView.SelectedNode = node;
                    node.EnsureVisible();
                    return;
                }
            }
            MessageBox.Show($"Animation ID {targetId} not found.", "Search",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) SearchButton_Click(sender, e);
        }

        // ════════════════════════════════════════════════════════════════
        //  Import VD into current MUL slot
        // ════════════════════════════════════════════════════════════════

        private void ImportVdButton_Click(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                MessageBox.Show("Please select an animation file (MUL) first.", "Import",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MulAnimTreeView.SelectedNode == null)
            {
                MessageBox.Show("Please select an animation body in the tree first.", "Import",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Import .vd into current animation slot";
                ofd.Filter = "VD Files (*.vd)|*.vd";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (BinaryReader br = new BinaryReader(File.OpenRead(ofd.FileName)))
                    {
                        short magic = br.ReadInt16();
                        short animType = br.ReadInt16();

                        int currentType = Animations.GetAnimLength(_currentBody, _fileType);

                        // Type mismatch check (same as AnimationEditForm)
                        if (animType != currentType)
                        {
                            var dlgRes = MessageBox.Show(
                                $"The .vd file has animation type {animType}, but the current slot expects type {currentType}.\n\nImport anyway?",
                                "Type Mismatch", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dlgRes != DialogResult.Yes) return;
                        }

                        AnimationEdit.LoadFromVD(_fileType, _currentBody, br);
                    }

                    Options.ChangedUltimaClass["Animations"] = true;
                    PopulateMulFrames();
                    MessageBox.Show($"Successfully imported into body slot {_currentBody}.", "Import",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import failed:\n{ex.Message}", "Import Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Export current MUL slot → VD
        // ════════════════════════════════════════════════════════════════

        private void ExportVdButton_Click(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                MessageBox.Show("Please select an animation file (MUL) first.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export animation to .vd";
                sfd.Filter = "VD Files (*.vd)|*.vd";
                sfd.FileName = $"anim{_fileType}_0x{_currentBody:X4}.vd";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    AnimationEdit.ExportToVD(_fileType, _currentBody, sfd.FileName, 1.0);
                    MessageBox.Show($"Exported to:\n{sfd.FileName}", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed:\n{ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Save MUL
        // ════════════════════════════════════════════════════════════════

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_fileType == 0) return;

            AnimationEdit.Save(_fileType, Options.OutputPath);
            Options.ChangedUltimaClass["Animations"] = false;
            MessageBox.Show($"Animation saved to:\n{Options.OutputPath}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ════════════════════════════════════════════════════════════════
        //  Delete selected action / body from MUL
        // ════════════════════════════════════════════════════════════════

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_fileType == 0 || MulAnimTreeView.SelectedNode == null) return;

            bool isBody = (MulAnimTreeView.SelectedNode.Parent == null);
            string subject = isBody ? $"body {_currentBody}" : $"action {_currentAction} of body {_currentBody}";

            var result = MessageBox.Show($"Delete {subject}?", "Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            if (isBody)
            {
                int animLen = Animations.GetAnimLength(_currentBody, _fileType);
                for (int j = 0; j < animLen; j++)
                    for (int d = 0; d < 5; d++)
                        AnimationEdit.GetAnimation(_fileType, _currentBody, j, d)?.ClearFrames();

                MulAnimTreeView.SelectedNode.ForeColor = Color.Red;
            }
            else
            {
                for (int d = 0; d < 5; d++)
                    AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, d)?.ClearFrames();

                MulAnimTreeView.SelectedNode.ForeColor = Color.Red;
            }

            Options.ChangedUltimaClass["Animations"] = true;
            PopulateMulFrames();
        }

        // ════════════════════════════════════════════════════════════════
        //  Center X / Y NumericUpDown
        // ════════════════════════════════════════════════════════════════

        private void CenterXNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi || _fileType == 0) return;
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null || MulFramesTrackBar.Value >= edit.Frames.Count) return;
            edit.Frames[MulFramesTrackBar.Value].Center = new Point(
                (int)CenterXNumericUpDown.Value,
                edit.Frames[MulFramesTrackBar.Value].Center.Y);
            Options.ChangedUltimaClass["Animations"] = true;
            MulPreviewPictureBox.Invalidate();
        }

        private void CenterYNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi || _fileType == 0) return;
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null || MulFramesTrackBar.Value >= edit.Frames.Count) return;
            edit.Frames[MulFramesTrackBar.Value].Center = new Point(
                edit.Frames[MulFramesTrackBar.Value].Center.X,
                (int)CenterYNumericUpDown.Value);
            Options.ChangedUltimaClass["Animations"] = true;
            MulPreviewPictureBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  Zoom ComboBox
        // ════════════════════════════════════════════════════════════════

        private void ZoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = ZoomComboBox.Text.Replace("%", "").Trim();
            if (float.TryParse(text, out float pct))
            {
                _zoomFactor = pct / 100f;
                if (_zoomFactor <= 0f) _zoomFactor = 1f;
                MulPreviewPictureBox.Invalidate();
                VdPreviewPictureBox.Invalidate();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Helpers
        // ════════════════════════════════════════════════════════════════

        private string[] GetAnimNamesForLength(int animLength)
        {
            if (animLength == 13) return _animNames[0];
            if (animLength == 22) return _animNames[1];
            return _animNames[2]; // human / 35
        }

        /// <summary>Returns a centred, aspect-ratio-preserving destination Rectangle inside <paramref name="bounds"/>.</summary>
        private static Rectangle GetScaledRect(int srcW, int srcH, Rectangle bounds)
        {
            if (srcW <= 0 || srcH <= 0) return bounds;
            float scaleW = (float)bounds.Width / srcW;
            float scaleH = (float)bounds.Height / srcH;
            float scale = Math.Min(scaleW, scaleH);
            int w = (int)(srcW * scale);
            int h = (int)(srcH * scale);
            int x = bounds.X + (bounds.Width - w) / 2;
            int y = bounds.Y + (bounds.Height - h) / 2;
            return new Rectangle(x, y, w, h);
        }

        // ════════════════════════════════════════════════════════════════
        //  Form Close — dispose bitmaps
        // ════════════════════════════════════════════════════════════════

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();

            foreach (var kvp in _vdFramesByDir)
                foreach (var bmp in kvp.Value)
                    bmp?.Dispose();
            _vdFramesByDir.Clear();

            base.OnFormClosed(e);
        }
    }
}