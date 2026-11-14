/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * AnimationEditForm – connection to the animation list and ServUO:
 *  - remembers which animations were changed in this session
 *  - after saving: offers to add the changed animations to Animationlist.xml
 *  - menu "Add to animation list…" and "ServUO script & setup…" for the selected animation
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationEditForm
    {
        // (file type, index in the file) of all animations changed in this session
        private readonly HashSet<(int FileType, int Index)> _editedAnimations = new HashSet<(int, int)>();

        private AnimationListAddForm _animationListAddForm;

        /// <summary>Replaces <c>Options.ChangedUltimaClass["Animations"] = true</c>: also remembers the animation</summary>
        private void MarkAnimationChanged()
        {
            Options.ChangedUltimaClass["Animations"] = true;
            if (_fileType > 0 && _currentBody >= 0)
            {
                _editedAnimations.Add((_fileType, _currentBody));
            }
        }

        private void InitAnimationListIntegration()
        {
            var addItem = new ToolStripMenuItem("Add to animation list…", null, (s, e) => ShowAnimationListAdd(new[] { CurrentCandidate() }))
            {
                ToolTipText = "Add the selected animation with name and type to Animationlist.xml (AnimationList tab)"
            };
            var addEditedItem = new ToolStripMenuItem("Edited animations → animation list…", null, (s, e) => OfferEditedAnimationsForList(true))
            {
                ToolTipText = "All animations changed in this session that are not in the animation list yet"
            };
            var servUoItem = new ToolStripMenuItem("ServUO script & setup…", null, (s, e) => ShowServUoSetup())
            {
                ToolTipText = "Create a ServUO script and check Bodyconv.def, mobtypes.txt and bodyTable.cfg for the selected animation"
            };

            MiscToolStripButton.DropDownItems.Add(new ToolStripSeparator());
            MiscToolStripButton.DropDownItems.Add(addItem);
            MiscToolStripButton.DropDownItems.Add(addEditedItem);
            MiscToolStripButton.DropDownItems.Add(servUoItem);

            ContextMenuStripTreeView.Items.Add(new ToolStripSeparator());
            ContextMenuStripTreeView.Items.Add(new ToolStripMenuItem("Add to animation list…", null, (s, e) => ShowAnimationListAdd(new[] { CurrentCandidate() })));
            ContextMenuStripTreeView.Items.Add(new ToolStripMenuItem("ServUO script & setup…", null, (s, e) => ShowServUoSetup()));
        }

        /// <summary>Real body id of an animation of a file (-1 = anim2..anim5 index without Bodyconv.def entry)</summary>
        private static int TrueBody(int fileType, int index)
        {
            if (fileType == UOP_FILE_TYPE || fileType == 1)
            {
                return index;
            }

            return BodyConverter.GetTrueBody(fileType, index);
        }

        private AnimationCandidate CurrentCandidate()
        {
            if (_fileType <= 0)
            {
                return null;
            }

            return new AnimationCandidate
            {
                FileType = _fileType,
                Index = _currentBody,
                Body = TrueBody(_fileType, _currentBody)
            };
        }

        private List<AnimationCandidate> EditedCandidatesNotInList()
        {
            HashSet<int> inList = new HashSet<int>(AnimationListStore.Load().Select(e => e.Body));
            return _editedAnimations
                .Select(a => new AnimationCandidate { FileType = a.FileType, Index = a.Index, Body = TrueBody(a.FileType, a.Index) })
                .Where(c => c.Body < 0 || !inList.Contains(c.Body))
                .OrderBy(c => c.FileType).ThenBy(c => c.Index)
                .ToList();
        }

        /// <summary>
        /// After saving: asks whether the changed animations that are not in the animation list should be added.
        /// </summary>
        private void OfferEditedAnimationsForList(bool always = false)
        {
            List<AnimationCandidate> candidates;
            try
            {
                candidates = EditedCandidatesNotInList();
            }
            catch (Exception)
            {
                return;
            }

            if (candidates.Count == 0)
            {
                if (always)
                {
                    MessageBox.Show(this, "All animations changed in this session are already in the animation list.", "Animation list",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return;
            }

            if (!always && MessageBox.Show(this,
                    $"{candidates.Count} changed animation(s) are not in the animation list (AnimationList tab):\n\n" +
                    string.Join("\n", candidates.Take(10).Select(c => "  " + c)) + (candidates.Count > 10 ? "\n  …" : string.Empty) +
                    "\n\nAdd them now (name and type)?",
                    "Animation list", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            ShowAnimationListAdd(candidates);
        }

        private void ShowAnimationListAdd(IEnumerable<AnimationCandidate> candidates)
        {
            List<AnimationCandidate> list = candidates?.Where(c => c != null).ToList() ?? new List<AnimationCandidate>();
            if (list.Count == 0)
            {
                MessageBox.Show(this, "Please select an animation file and an animation first.", "Animation list",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_animationListAddForm != null && !_animationListAddForm.IsDisposed)
            {
                _animationListAddForm.Close();
            }

            _animationListAddForm = new AnimationListAddForm(AnimationListControl.ActionNames, list, null);
            _animationListAddForm.Show(this);
        }

        private void ShowServUoSetup()
        {
            AnimationCandidate candidate = CurrentCandidate();
            if (candidate == null)
            {
                MessageBox.Show(this, "Please select an animation file and an animation first.", "ServUO",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int body = candidate.Body;
            if (body < 0)
            {
                MessageBox.Show(this,
                    $"{candidate.FileLabel} index {candidate.Index} has no body id in Bodyconv.def yet.\n\n" +
                    "Enter the body id the server should use in the next dialog – the setup check creates the Bodyconv.def line.",
                    "ServUO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                body = 0;
            }

            AnimationListEntry entry = body > 0 ? AnimationListStore.Find(body) : null;
            int type = entry == null
                ? (body > 0 ? AnimationListStore.SuggestType(body, AnimationListControl.ActionNames) : 0)
                : entry.IsEquipment ? 4 : entry.Type;
            string name = entry?.Name ?? AnimationListStore.SuggestName(body);

            new ServUOSetupForm(body, type, name, candidate).Show(this);
        }
    }
}
