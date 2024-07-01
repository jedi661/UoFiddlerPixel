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
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Ultima;
using UoFiddler.Controls.Classes;

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
        private Point _framePoint;
        private Point _additionalFramePoint; // Hinzugefügt
        private bool _showOnlyValid;
        private static bool _drawEmpty;
        private static bool _drawFull;
        private static readonly Color _whiteConvert = Color.FromArgb(255, 255, 255, 255);

        private static readonly Pen _blackUnDrawTransparent = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
        private static readonly Pen _blackUnDrawOpaque = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        private static Pen _blackUndraw = _blackUnDrawOpaque;

        private static readonly SolidBrush _whiteUnDrawTransparent = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
        private static readonly SolidBrush _whiteUnDrawOpaque = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        private static SolidBrush _whiteUnDraw = _whiteUnDrawOpaque;

        private static readonly Color _greyConvert = Color.FromArgb(255, 170, 170, 170);

        private static bool _lockButton;

        private bool isAnimationVisible = false; // Second animation
        private AnimIdx additionalAnimation = null; // Second animation

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

        #region [ AnimationEditForm ]
        public AnimationEditForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            SelectFileToolStripComboBox.SelectedIndex = 0;
            FramesListView.MultiSelect = true;

            _fileType = 0;
            _currentDir = 0;
            _framePoint = new Point(AnimationPictureBox.Width / 2, AnimationPictureBox.Height / 2);
            _showOnlyValid = false;
            _loaded = false;
        }
        #endregion

        #region [ _animNames ]
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
            } //human
        };
        #endregion

        #region [ OnLoad ]
        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["AnimationEdit"] = true;

            AnimationListTreeView.BeginUpdate();
            try
            {
                AnimationListTreeView.Nodes.Clear();
                if (_fileType != 0)
                {
                    int count = Animations.GetAnimCount(_fileType);
                    TreeNode[] nodes = new TreeNode[count];
                    int animationCount = 0; // Counting variable for animations
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
                                Text = string.Format("{0:D2} {1}", j, _animNames[animLength == 22 ? 1 : animLength == 13 ? 0 : 2][j])
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
                            animationCount++; // Increment the count variable when an animation is found
                        }
                        else
                        {
                            if (_showOnlyValid)
                            {
                                continue;
                            }
                            //node.ForeColor = Color.Red;
                            // If checkBoxIDBlue is checked, set the color to Blue. Otherwise, set it to Red.
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

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
        }
        #endregion

        #region [ OnFilePathChangeEvent ]
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
            OnLoad(null);
        }
        #endregion

        #region [ TreeNode GetNode ]
        private TreeNode GetNode(int tag)
        {
            return _showOnlyValid
                ? AnimationListTreeView.Nodes.Cast<TreeNode>().FirstOrDefault(node => (int)node.Tag == tag)
                : AnimationListTreeView.Nodes[tag];
        }
        #endregion

        #region [ SetPaletteBox ]
        private unsafe void SetPaletteBox()
        {
            if (_fileType == 0)
            {
                return;
            }

            // TODO: why is bitmapWidth constant and height is taken from picturebox?
            // TODO: looks like the value is the same as array size for pallete in AnimIdx
            const int bitmapWidth = 256;
            int bitmapHeight = PalettePictureBox.Height;

            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format16bppArgb1555);
            if (edit != null)
            {
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
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

                bmp.UnlockBits(bd);
            }

            PalettePictureBox.Image?.Dispose();
            PalettePictureBox.Image = bmp;
        }
        #endregion

        #region [ AfterSelectTreeView ]
        private void AfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            if (AnimationListTreeView.SelectedNode == null)
            {
                return;
            }

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

                        CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                        CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
                    }
                    //Soulblighter Modification
                    else
                    {
                        FramesTrackBar.Maximum = 0;
                        FramesTrackBar.Value = 0;
                        FramesTrackBar.Invalidate();
                    }
                    //End of Soulblighter Modification
                }
            }
            finally
            {
                FramesListView.EndUpdate();
            }

            AnimationPictureBox.Invalidate();
            SetPaletteBox();
        }
        #endregion

        #region [ DrawFrameItem ]
        private void DrawFrameItem(object sender, DrawListViewItemEventArgs e)
        {
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);

            Bitmap[] currentBits = edit.GetFrames();

            Bitmap bmp = currentBits[(int)e.Item.Tag];

            // Check if the current item is selected
            if (FramesListView.SelectedItems.Contains(e.Item))
            {
                // Change background color of selected item
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
            }

            var penColor = FramesListView.SelectedItems.Contains(e.Item) ? Color.Red : Color.Gray;

            e.Graphics.DrawRectangle(new Pen(penColor), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, bmp.Width, bmp.Height);

            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
        }
        #endregion

        #region [ OnAnimChanged ]
        private void OnAnimChanged(object sender, EventArgs e)
        {
            if (SelectFileToolStripComboBox.SelectedIndex == _fileType)
            {
                return;
            }

            _fileType = SelectFileToolStripComboBox.SelectedIndex;
            OnLoad(this, EventArgs.Empty);
        }
        #endregion

        #region [ OnDirectionChanged ]        
        private void OnDirectionChanged(object sender, EventArgs e)
        {
            _currentDir = DirectionTrackBar.Value;

            if (isAnimationVisible)
            {
                // Show animation based on selected gender
                string selectedGender = comboBoxMenWoman.SelectedItem.ToString();
                int animId = selectedGender == "men" ? 400 : 401;

                // Get the corresponding animation based on the DirectionTrackBar value
                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);

                // Adjust the position of the additional animation
                AdjustAdditionalAnimationPosition();

                // Redraw the PictureBox to reflect the changes
                AnimationPictureBox.Invalidate();
            }

            AfterSelectTreeView(null, null);
        }
        #endregion

        #region [ AnimationPictureBox_OnSizeChanged ]
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
                if (additionalBits?.Length > 0 && additionalBits[FramesTrackBar.Value] != null)
                {
                    // Get the offsets for the current direction and frame
                    int[] offsets = Offsets[_currentDir][FramesTrackBar.Value];
                    int xOffset = offsets[0];
                    int yOffset = offsets[1];

                    // Define separate x and y coordinates for the additional animation
                    int additionalX = _framePoint.X + xOffset;
                    int additionalY = _framePoint.Y + yOffset;

                    // Store the adjusted position
                    _additionalFramePoint = new Point(additionalX, additionalY);
                }
            }
        }
        #endregion

        #region [ AnimationPictureBox_OnPaintFrame ] 
        private void AnimationPictureBox_OnPaintFrame(object sender, PaintEventArgs e)
        {
            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
            {
                return;
            }

            Bitmap[] currentBits = edit.GetFrames();

            e.Graphics.Clear(Color.LightGray);
            e.Graphics.DrawLine(Pens.Black, new Point(_framePoint.X, 0), new Point(_framePoint.X, AnimationPictureBox.Height));
            e.Graphics.DrawLine(Pens.Black, new Point(0, _framePoint.Y), new Point(AnimationPictureBox.Width, _framePoint.Y));

            // Men Woman Animation alignment
            if (isAnimationVisible && additionalAnimation != null)
            {
                AdjustAdditionalAnimationPosition(); // Ensure this method is called

                Bitmap[] additionalBits = additionalAnimation.GetFrames();
                if (additionalBits?.Length > 0 && additionalBits[FramesTrackBar.Value] != null)
                {
                    // Draw the additional animation at the specified position
                    e.Graphics.DrawImage(additionalBits[FramesTrackBar.Value], _additionalFramePoint.X, _additionalFramePoint.Y);
                }
            }
            // Men Woman Animation alignment

            if (currentBits?.Length > 0 && currentBits[FramesTrackBar.Value] != null)
            {
                int varW;
                int varH;
                if (!_drawEmpty)
                {
                    varW = 0;
                    varH = 0;
                }
                else
                {
                    varW = currentBits[FramesTrackBar.Value].Width;
                    varH = currentBits[FramesTrackBar.Value].Height;
                }

                int varFw;
                int varFh;
                if (!_drawFull)
                {
                    varFw = 0;
                    varFh = 0;
                }
                else
                {
                    varFw = currentBits[FramesTrackBar.Value].Width;
                    varFh = currentBits[FramesTrackBar.Value].Height;
                }

                int x = _framePoint.X - edit.Frames[FramesTrackBar.Value].Center.X;
                int y = _framePoint.Y - edit.Frames[FramesTrackBar.Value].Center.Y - currentBits[FramesTrackBar.Value].Height;

                using (var whiteTransparent = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                {
                    e.Graphics.FillRectangle(whiteTransparent, new Rectangle(x, y, varFw, varFh));
                }

                e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, varW, varH));
                e.Graphics.DrawImage(currentBits[FramesTrackBar.Value], x, y);

                //e.Graphics.DrawLine(Pens.Red, new Point(0, 335-(int)numericUpDown1.Value), new Point(animationPictureBox.Width, 335-(int)numericUpDown1.Value));                
            }

            // Draw Reference Point Arrow
            Point[] arrayPoints = {
            new Point(418 - (int)RefXNumericUpDown.Value, 335 - (int)RefYNumericUpDown.Value),
            new Point(418 - (int)RefXNumericUpDown.Value, 352 - (int)RefYNumericUpDown.Value),
            new Point(422 - (int)RefXNumericUpDown.Value, 348 - (int)RefYNumericUpDown.Value),
            new Point(425 - (int)RefXNumericUpDown.Value, 353 - (int)RefYNumericUpDown.Value),
            new Point(427 - (int)RefXNumericUpDown.Value, 352 - (int)RefYNumericUpDown.Value),
            new Point(425 - (int)RefXNumericUpDown.Value, 347 - (int)RefYNumericUpDown.Value),
            new Point(430 - (int)RefXNumericUpDown.Value, 347 - (int)RefYNumericUpDown.Value)
        };

            e.Graphics.FillPolygon(_whiteUnDraw, arrayPoints);
            e.Graphics.DrawPolygon(_blackUndraw, arrayPoints);
        }
        #endregion

        #region  [ OnFrameCountBarChange ]
        private void OnFrameCountBarChanged(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit != null && edit.Frames.Count >= FramesTrackBar.Value)
            {
                CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
            }

            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ OnCenterXValueChanged ]
        private void OnCenterXValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null || edit.Frames.Count < FramesTrackBar.Value)
                {
                    return;
                }

                FrameEdit frame = edit.Frames[FramesTrackBar.Value];
                if (CenterXNumericUpDown.Value == frame.Center.X)
                {
                    return;
                }

                frame.ChangeCenter((int)CenterXNumericUpDown.Value, frame.Center.Y);
                Options.ChangedUltimaClass["Animations"] = true;
                AnimationPictureBox.Invalidate();
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }
        #endregion

        #region [ OnCenterYValueChanged ]
        private void OnCenterYValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null || edit.Frames.Count < FramesTrackBar.Value)
                {
                    return;
                }

                FrameEdit frame = edit.Frames[FramesTrackBar.Value];
                if (CenterYNumericUpDown.Value == frame.Center.Y)
                {
                    return;
                }

                frame.ChangeCenter(frame.Center.X, (int)CenterYNumericUpDown.Value);
                Options.ChangedUltimaClass["Animations"] = true;
                AnimationPictureBox.Invalidate();
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }
        #endregion

        #region [ OnClickExtractImages ]
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
                body = (int)AnimationListTreeView.SelectedNode.Tag;
                action = -1;
            }
            else
            {
                body = (int)AnimationListTreeView.SelectedNode.Parent.Tag;
                action = (int)AnimationListTreeView.SelectedNode.Tag;
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
        #endregion

        #region [ OnClickRemoveAction ]
        private void OnClickRemoveAction(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
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
        #endregion

        #region [ OnClickSave ]
        private void OnClickSave(object sender, EventArgs e)
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

        #region [ OnClickRemoveFrame ]        
        private void OnClickRemoveFrame(object sender, EventArgs e)
        {
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
        #endregion

        #region [ OnClickReplace ]
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
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
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
        #endregion

        #region [ OnClickAdd ]
        private void OnClickAdd(object sender, EventArgs e)
        {
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

                                        CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                                        CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;

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

                                        CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
                                        CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;

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
        #endregion

        #region [ AddImageAtCertainIndex ]
        private void AddImageAtCertainIndex(int frameCount, Bitmap[] bitBmp, Bitmap bmp, FrameDimension dimension, AnimIdx edit)
        {
            // Return an Image at a certain index
            for (int index = 0; index < frameCount; index++)
            {
                bitBmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                bmp.SelectActiveFrame(dimension, index);
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
        }
        #endregion

        #region [ OnClickExtractPalette ]
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
        #endregion

        #region [ OnClickImportPalette ]
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
                    string line;
                    ushort[] palette = new ushort[0x100];
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        {
                            continue;
                        }

                        palette[i++] = ushort.Parse(line);

                        if (palette[i++] == 32768)
                        {
                            palette[i++] = 32769;
                        }

                        if (i >= 0x100)
                        {
                            break;
                        }
                    }
                    edit.ReplacePalette(palette);
                }
                SetPaletteBox();
                FramesListView.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
            }
        }
        #endregion

        #region [ OnClickImportFromVD ]
        private void OnClickImportFromVD(object sender, EventArgs e)
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
                dialog.Filter = "vd files (*.vd)|*.vd";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int animLength = Animations.GetAnimLength(_currentBody, _fileType);
                int currentType;
                if (animLength == 22)
                {
                    currentType = 0;
                }
                else
                {
                    currentType = animLength == 13 ? 1 : 2;
                }

                using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader bin = new BinaryReader(fs))
                    {
                        int fileType = bin.ReadInt16();
                        int animType = bin.ReadInt16();

                        // Update the label with the file type and animation type
                        toolStripStatusLabelVDAminInfo.Text = $"File Type: {fileType}, Animation Type: {animType}";

                        if (fileType != 6)
                        {
                            MessageBox.Show("Not an Anim File.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            toolStripStatusLabelVDAminInfo.Text += " - Not an Anim File.";
                            return;
                        }

                        if (animType != currentType) //Error Message
                        {
                            MessageBox.Show($"The selected .vd file has an animation type of {animType} (Got: {animType}), but the program expects an animation type of {currentType} (Expected: {currentType}). This results in a 'Wrong Anim Id ( Type )' error. Please check the .vd file and ensure it has the correct animation type.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            toolStripStatusLabelVDAminInfo.Text += $" - Wrong Anim Id ( Type ). Expected: {currentType}, Got: {animType}. Selected slot: {_currentBody}";
                            return;
                        }

                        if (fileType != 6)
                        {
                            MessageBox.Show("Not an Anim File.", "Import", MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            return;
                        }

                        if (animType != currentType)
                        {
                            MessageBox.Show("Wrong Anim Id ( Type )", "Import", MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        AnimationEdit.LoadFromVD(_fileType, _currentBody, bin);
                    }
                }

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

                //import Message
                Options.ChangedUltimaClass["Animations"] = true;
                AfterSelectTreeView(this, null);
                MessageBox.Show("successfully imported animation to the slot", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                toolStripStatusLabelVDAminInfo.Text += $" - Selected slot: {_currentBody}";
            }
        }
        #endregion

        #region [ OnClickExportToVD ]
        private void OnClickExportToVD(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"anim{_fileType}_0x{_currentBody:X}.vd");
            AnimationEdit.ExportToVD(_fileType, _currentBody, fileName);

            MessageBox.Show($"Animation saved to {Options.OutputPath}", "Export", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickShowOnlyValid ]
        private void OnClickShowOnlyValid(object sender, EventArgs e)
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

        #region [ SameCenterButton ]
        private void SameCenterButton_Click(object sender, EventArgs e)
        {
            // TODO: there is no undo for same center button
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null || edit.Frames.Count < FramesTrackBar.Value)
                {
                    return;
                }

                FrameEdit[] frame = new FrameEdit[edit.Frames.Count];
                for (int index = 0; index < edit.Frames.Count; index++)
                {
                    frame[index] = edit.Frames[index];
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
        #endregion

        #region [ FromGifToolStripMenuItem ]
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
        #endregion

        #region [ ReferencePointX ]
        private void ReferencePointX(object sender, EventArgs e)
        {
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ ReferencePointY ]
        private void ReferencePointY(object sender, EventArgs e)
        {
            AnimationPictureBox.Invalidate();
        }
        #endregion        

        #region [ AnimationPictureBox_MouseClick ]
        private void AnimationPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_lockButton || !ToolStripLockButton.Enabled)
            {
                return;
            }

            RefXNumericUpDown.Value = 418 - e.X;
            RefYNumericUpDown.Value = 335 - e.Y;

            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ TxtSendData_KeyDown ]
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
        #endregion

        #region [ TxtSendData_KeyDown2 ]
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
        #endregion

        #region [ ToolStripLockButton ]
        private void ToolStripLockButton_Click(object sender, EventArgs e)
        {
            _lockButton = !_lockButton;
            RefXNumericUpDown.Enabled = !_lockButton;
            RefYNumericUpDown.Enabled = !_lockButton;
        }
        #endregion

        #region [ AllDirectionsAddToolStripMenuItem ]
        // Add in all Directions
        private void AllDirectionsAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose 5 GIFs to add";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif;)|*.gif;";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        DirectionTrackBar.Enabled = false;
                        if (dialog.FileNames.Length == 5)
                        {
                            DirectionTrackBar.Value = 0;

                            AddFilesAllDirections(dialog);
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

                            AddFilesAllDirections(dialog);
                        }
                        DirectionTrackBar.Enabled = true;
                    }
                }
            }

            // Refresh List
            _currentDir = DirectionTrackBar.Value;
            AfterSelectTreeView(null, null);
        }
        #endregion

        #region [ AddFilesAllDirections ]
        private void AddFilesAllDirections(OpenFileDialog dialog)
        {
            for (int w = 0; w < dialog.FileNames.Length; w++)
            {
                if (w >= 5)
                {
                    continue;
                }

                AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);

                if (edit != null)
                {
                    // Gif Especial Properties
                    if (dialog.FileNames[w].Contains(".gif"))
                    {
                        // dialog.Filename replaced by dialog.FileNames[w]
                        using (Bitmap bmpTemp = new Bitmap(dialog.FileNames[w]))
                        {
                            Bitmap bitmap = new Bitmap(bmpTemp);

                            FrameDimension dimension = new FrameDimension(bitmap.FrameDimensionsList[0]);

                            // Number of frames
                            int frameCount = bitmap.GetFrameCount(dimension);

                            ProgressBar.Maximum = frameCount;

                            bitmap.SelectActiveFrame(dimension, 0);

                            UpdateGifPalette(bitmap, edit);

                            Bitmap[] bitBmp = new Bitmap[frameCount];

                            AddImageAtCertainIndex(frameCount, bitBmp, bitmap, dimension, edit);

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

                if ((w < 4) && (w < dialog.FileNames.Length - 1))
                {
                    DirectionTrackBar.Value++;
                }
            }
        }
        #endregion

        #region [ DrawEmptyRectangleToolStripButton ]
        private void DrawEmptyRectangleToolStripButton_Click(object sender, EventArgs e)
        {
            _drawEmpty = !_drawEmpty;
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ DrawFullRectangleToolStripButton ]
        private void DrawFullRectangleToolStripButton_Click(object sender, EventArgs e)
        {
            _drawFull = !_drawFull;
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ AnimationEdit_FormClosing ]
        private void AnimationEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            AnimationTimer.Enabled = false;
            _drawFull = false;
            _drawEmpty = false;
            _lockButton = false;
            _loaded = false;

            ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
        }
        #endregion

        #region [ AnimationTimer_Tick ]
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
        #endregion

        #region [ ToolStripButtonPlayAnimation ]
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
        #endregion

        #region [ AnimationSpeedTrackBar_ValueChanged ]
        private void AnimationSpeedTrackBar_ValueChanged(object sender, EventArgs e)
        {
            AnimationTimer.Interval = 50 + (AnimationSpeedTrackBar.Value * 30);
        }
        #endregion

        #region [ DrawReferencialPointToolStripButton ]
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
        #endregion

        #region [ AllDirectionsAddWithCanvasToolStripMenuItem_Click ]
        // All Directions with Canvas
        private void AllDirectionsAddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = true;
                        dialog.Title = "Choose 5 GIFs to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
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
        #endregion

        #region [ AddSelectedFiles ]
        private void AddSelectedFiles(OpenFileDialog dialog, Color customConvert)
        {
            for (int w = 0; w < dialog.FileNames.Length; w++)
            {
                if (w >= 5)
                {
                    continue;
                }

                // dialog.Filename replaced by dialog.FileNames[w]
                Bitmap bmp = new Bitmap(dialog.FileNames[w]);

                // TODO: fix checking file extension
                // Gif Especial Properties
                if (!dialog.FileNames[w].Contains(".gif"))
                {
                    continue;
                }

                AddAnimationX1(customConvert, bmp);

                if ((w < 4) && (w < dialog.FileNames.Length - 1))
                {
                    DirectionTrackBar.Value++;
                }
            }
        }
        #endregion

        #region [ AddAnimationX1 ]
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
            CenterXNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.X;
            CenterYNumericUpDown.Value = edit.Frames[FramesTrackBar.Value].Center.Y;
            Options.ChangedUltimaClass["Animations"] = true;
        }
        #endregion

        #region [ AddWithCanvasToolStripMenuItem_Click ]
        //Add with Canvas
        private void AddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
        #endregion

        #region [ OnClickGeneratePalette ]
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
        #endregion

        #region [ unsafe Bitmap ConvertBmpAnim ]
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
        #endregion

        #region [ OnClickExportAllToVD ]
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
        #endregion

        #region [ CbSaveCoordinates_CheckedChanged ]
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
        #endregion

        #region [ SetButton_Click ]
        private void SetButton_Click(object sender, EventArgs e)
        {
            DirectionTrackBar.Value = 0;
            DirectionTrackBar.Enabled = false;
            for (int i = 0; i <= DirectionTrackBar.Maximum; i++)
            {
                try
                {
                    if (_fileType != 0)
                    {
                        AnimIdx edit = AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                        if (edit != null && edit.Frames.Count >= FramesTrackBar.Value)
                        {
                            foreach (var editFrame in edit.Frames)
                            {
                                editFrame.ChangeCenter(_animCx[i], _animCy[i]);
                                // TODO: check if we can invalidate pictture box only after the loop
                                Options.ChangedUltimaClass["Animations"] = true;
                                AnimationPictureBox.Invalidate();
                            }
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    // TODO: add logging or fix?
                    // ignored
                }

                if (DirectionTrackBar.Value < DirectionTrackBar.Maximum)
                {
                    DirectionTrackBar.Value++;
                }
                else
                {
                    DirectionTrackBar.Value = 0;
                }
            }
            DirectionTrackBar.Enabled = true;
        }
        #endregion

        #region [ AddDirectionsAddWithCanvasUniqueImageToolStripMenuItem ]
        // Add Directions with Canvas ( CV5 style GIF )
        private void AddDirectionsAddWithCanvasUniqueImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                                    {
                                        Bitmap bitmap = new Bitmap(bmpTemp);

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
        #endregion

        #region [ Cv5AnimIdxPositions ]
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
        #endregion

        #region [ Bitmap ConvertBmpAnimCv5 ]
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
        #endregion        

        #region [ Cv5CanvasAlgorithm ]
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
        #endregion

        #region [ GetInitialFrameIndex ]
        private static int GetInitialFrameIndex(int frameCount, int frameDivider, int position)
        {
            return frameCount / frameDivider * position;
        }
        #endregion

        #region [ GetMaximumFrameIndex ]
        private static int GetMaximumFrameIndex(int frameCount, int frameDivider, int position)
        {
            return frameCount / frameDivider * (position + 1);
        }
        #endregion

        #region [ Cv5ProcessFrames ]
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
        #endregion

        #region [ CbLockColorControls_CheckedChanged ]
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
        #endregion

        #region [ AllDirectionsAddWithCanvasKRFrameEditorColorCorrectorToolStripMenuItem_Click ]
        // All directions Add KRFrameViewer
        private void AllDirectionsAddWithCanvasKRFrameEditorColorCorrectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in KRFrameViewer Style ) to add";
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
                                //Gif Especial Properties
                                if (dialog.FileName.Contains(".gif"))
                                {
                                    using (Bitmap bmpTemp = new Bitmap(dialog.FileName))
                                    {
                                        Bitmap bitmap = new Bitmap(bmpTemp);

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

                                        KrCanvasAlgorithm(bitBmp, frameCount, dimension, customConvert);

                                        edit = KrAnimIdxPositions(frameCount, bitBmp, dimension, edit, bitmap);

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
        #endregion

        #region [ AnimIdx KrAnimIdxPositions ]
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
        #endregion

        #region [ Bitmap ConvertBmpAnimKr ]
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


                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != 54965 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }
        #endregion

        #region [ KrCanvasAlgorithm ]
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
        #endregion

        #region [ KrProcessFrames ]
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
        #endregion

        #region [ SetPaletteButton ]
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
        #endregion

        // TODO: check why there is no RadioButton1_CheckedChanged event for selector 1?

        #region [ RadioButton2_CheckedChanged ]
        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(2);
        }
        #endregion

        #region [ RadioButton3_CheckedChanged ]
        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(3);
        }
        #endregion

        #region [ RadioButton4_CheckedChanged ]
        private void RadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(4);
        }
        #endregion

        #region [ RadioButton5_CheckedChanged ]
        private void RadioButton5_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(5);
        }
        #endregion

        #region [ RadioButton6_CheckedChanged ]
        private void RadioButton6_CheckedChanged(object sender, EventArgs e)
        {
            ConvertAndSetPalette(6);
        }
        #endregion

        #region [ ApplyButton ]
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            ConvertAndSetPaletteWithReducer();
        }
        #endregion

        #region [ ConvertAndSetPaletteWithReducer ]
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
        #endregion

        #region [ ConvertAndSetPalette ]
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
        #endregion

        #region [ UpdateGifPalette ]
        public void UpdateGifPalette(Bitmap bit, AnimIdx animIdx)
        {
            using (MemoryStream imageStreamSource = new MemoryStream())
            {
                ImageConverter ic = new ImageConverter();
                byte[] btImage = (byte[])ic.ConvertTo(bit, typeof(byte[]));
                imageStreamSource.Write(btImage, 0, btImage.Length);
                GifBitmapDecoder decoder = new GifBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapPalette pal = decoder.Palette;
                int i;
                for (i = 0; i < 0x100; i++)
                {
                    animIdx.Palette[i] = 0;
                }

                try
                {
                    i = 0;
                    while (i < 0x100) //&& i < pal.Colors.Count)
                    {
                        int red = pal.Colors[i].R / 8;
                        int green = pal.Colors[i].G / 8;
                        int blue = pal.Colors[i].B / 8;

                        int contaFinal = (0x400 * red) + (0x20 * green) + blue + 0x8000;
                        if (contaFinal == 0x8000)
                        {
                            contaFinal = 0x8001;
                        }

                        animIdx.Palette[i] = (ushort)contaFinal;
                        i++;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // TODO: ignored?
                }
                catch (ArgumentOutOfRangeException)
                {
                    // TODO: ignored?
                }

                for (i = 0; i < 0x100; i++)
                {
                    if (animIdx.Palette[i] < 0x8000)
                    {
                        animIdx.Palette[i] = 0x8000;
                    }
                }
            }
        }
        #endregion

        #region [ UpdateImagePalette ]
        public unsafe void UpdateImagePalette(Bitmap bit, AnimIdx animIdx)
        {
            int count = 0;
            var bmp = new Bitmap(bit);
            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int i = 0;
            while (i < 0x100)
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

                    if (count >= 0x100)
                    {
                        break;
                    }
                }

                for (i = 0; i < 0x100; i++)
                {
                    if (animIdx.Palette[i] < 0x8000)
                    {
                        animIdx.Palette[i] = 0x8000;
                    }
                }

                if (count >= 0x100)
                {
                    break;
                }

                y++;
                line += delta;
            }
        }
        #endregion

        #region [ PaletteConverter ]
        public void PaletteConverter(int selector, AnimIdx animIdx)
        {
            int i;
            for (i = 0; i < 0x100; i++)
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

            for (i = 0; i < 0x100; i++)
            {
                if (animIdx.Palette[i] < 0x8000)
                {
                    animIdx.Palette[i] = 0x8000;
                }
            }
        }
        #endregion

        #region [ PaletteReducer ]
        public void PaletteReducer(int redP, int greenP, int blueP, AnimIdx animIdx)
        {
            int i;
            redP /= 8;
            greenP /= 8;
            blueP /= 8;
            for (i = 0; i < 0x100; i++)
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

            for (i = 0; i < 0x100; i++)
            {
                if (animIdx.Palette[i] < 0x8000)
                {
                    animIdx.Palette[i] = 0x8000;
                }
            }
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

        private void mirrorImageToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ Find IDs ]
        private async void FindFreeIDSlotsToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ Edit Ultima Online Bodyconv.def and mobtypes.txt ]

        private EditUoBodyconvMobtypes editUoBodyconvMobtypesForm;

        private void editUoBodyconMobtypesToolStripMenuItem_Click(object sender, EventArgs e)
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
                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);
                buttonShow.Text = "Hide";
            }

            isAnimationVisible = !isAnimationVisible;
            AnimationPictureBox.Invalidate();
        }
        #endregion

        #region [ ShowAdditionalAnimation ] animation man woman
        private void ShowAdditionalAnimation(int animId)
        {
            // Retrieve and display AnimationImage via animId
            Image animationImage = GetAnimationFrame(animId);
            if (animationImage != null)
            {
                AnimationPictureBox.Image = animationImage;
            }
            else
            {
                MessageBox.Show($"Animation with ID {animId} could not be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region [ Image GetAnimationFrame ] animation man woman
        private Image GetAnimationFrame(int animId)
        {
            //Logic to get and return animation image based on animId
            var edit = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);
            Bitmap[] frames = edit?.GetFrames();
            if (frames != null && frames.Length > 0)
            {
                return frames[0];
            }
            return null;
        }
        #endregion

        #region [ comboBoxMenWoman_SelectedIndexChanged ] animation man woman
        private void comboBoxMenWoman_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isAnimationVisible)
            {
                string selectedGender = comboBoxMenWoman.SelectedItem.ToString();
                int animId = selectedGender == "men" ? 400 : 401;
                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, _currentDir);
                AnimationPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ DirectionTrackBar_ValueChanged ] animation man woman
        private void DirectionTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (isAnimationVisible)
            {
                // Show animation based on selected gender
                string selectedGender = comboBoxMenWoman.SelectedItem.ToString();
                int animId = selectedGender == "men" ? 400 : 401;

                // Get the corresponding animation based on the DirectionTrackBar value
                additionalAnimation = AnimationEdit.GetAnimation(_fileType, animId, _currentAction, DirectionTrackBar.Value);

                // Redraw the PictureBox to reflect the changes
                AnimationPictureBox.Invalidate();
            }
        }
        #endregion

        #region [ btnUp ]
        private void btnUp_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;
            Offsets[direction][frame][1]--;
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btnDown ]
        private void btnDown_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;
            Offsets[direction][frame][1]++;
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btnLeft ]
        private void btnLeft_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;
            Offsets[direction][frame][0]--;
            AnimationPictureBox.Invalidate(); // Redraw image
        }
        #endregion

        #region [ btnRight ]
        private void btnRight_Click(object sender, EventArgs e)
        {
            int direction = DirectionTrackBar.Value;
            int frame = FramesTrackBar.Value;
            Offsets[direction][frame][0]++;
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
    }
}
