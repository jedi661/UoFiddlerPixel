/***************************************************************************
 *
 * $Author: Nikodemus
 * 
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UserControls
{
    public partial class ConverterMultiTextControl : UserControl
    {
        readonly Timer _timer = new Timer();

        //One Form
        private bool _isFormOpen = false;

        public ConverterMultiTextControl()
        {
            InitializeComponent();


            _timer.Interval = 1000;

            // Add the event handler
            _timer.Tick += new EventHandler(Timer_Tick);

            // Start the timer
            _timer.Start();
        }

        #region [ Timer_Tick ]
        void Timer_Tick(object sender, EventArgs e)
        {
            // Update the text of the ToolStripStatusLabel with the current time
            toolStripStatusLabelTime.Text = DateTime.Now.ToString("HH:mm:ss") + " Uhr";
        }
        #endregion

        #region [ ButtonGraficCutterForm_Click ]
        private void ButtonGraficCutterForm_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.ButtonGraficCutterForm, "Graphic Cutter");

            GraphicCutterForm form = new GraphicCutterForm();
            form.FormClosed += GraphicCutterForm_FormClosed;
            form.Show();
            _isFormOpen = true;

            ButtonGraficCutterForm.Enabled = false;
        }
        private void GraphicCutterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            ButtonGraficCutterForm.Enabled = true;
        }
        #endregion

        #region [ TextureCutter_Click ]
        private void TextureCutter_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.TextureCutter, "Texture Cutter");

            TextureCutter form = new TextureCutter();
            form.FormClosed += TextureCutter_FormClosed;
            form.Show();
            _isFormOpen = true;

            TextureCutter.Enabled = false;
        }

        private void TextureCutter_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            TextureCutter.Enabled = true;
        }
        #endregion

        #region [ BtDecriptClient ]
        private void BtDecriptClient_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtDecriptClient, "Decript");

            DecriptClientForm form = new DecriptClientForm();
            form.FormClosed += DecriptClientForm_FormClosed;
            form.Show();
            _isFormOpen = true;

            form.Enabled = true;
        }

        private void DecriptClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            BtDecriptClient.Enabled = true;
        }
        #endregion

        #region [ BtMapMaker_Click ]
        private void BtMapMaker_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtMapMaker, "Map Maker");

            MapMaker form = new MapMaker();
            form.FormClosed += MapMaker_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtMapMaker.Enabled = false;
        }
        private void MapMaker_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            BtMapMaker.Enabled = true;
        }
        #endregion

        #region [ BtAnimationVDEditorForm ]
        // Own instance instead of the shared _isFormOpen flag, so the VD editor
        // does not block the other tool windows (and vice versa).
        private AnimationVDEditorForm _animationVDEditorForm;

        private void BtAnimationVDEditorForm_Click(object sender, EventArgs e)
        {
            // already open: just bring it to the front
            if (_animationVDEditorForm != null && !_animationVDEditorForm.IsDisposed)
            {
                if (_animationVDEditorForm.WindowState == FormWindowState.Minimized)
                {
                    _animationVDEditorForm.WindowState = FormWindowState.Normal;
                }
                _animationVDEditorForm.BringToFront();
                _animationVDEditorForm.Activate();
                return;
            }

            _animationVDEditorForm = new AnimationVDEditorForm();
            _animationVDEditorForm.FormClosed += (s, args) => _animationVDEditorForm = null;
            _animationVDEditorForm.Show();
        }
        #endregion

        #region [ BtAnimationVDForm ]
        private void BtAnimationVDForm_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtAnimationVDForm, "VD Animation");

            AnimationVDForm form = new AnimationVDForm();
            form.FormClosed += AnimationVDForm_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtAnimationVDForm.Enabled = false;
        }
        private void AnimationVDForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtAnimationVDForm.Enabled = true; // Enable the button again.
            }
        }
        #endregion

        #region [ BtAnimationEditFormButton_Click ]
        private void BtAnimationEditFormButton_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtAnimationEditFormButton, "Animation Edit");

            AnimationEditFormButton form = new AnimationEditFormButton();
            form.FormClosed += AnimationEditFormButton_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtAnimationEditFormButton.Enabled = false;
        }
        private void AnimationEditFormButton_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtAnimationEditFormButton.Enabled = true; // Enable the button again.
            }
        }
        #endregion

        #region [ BtGumpsEdit ]
        private void BtGumpsEdit_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }


            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtGumpsEdit, "Gump Edit");

            GumpsEdit form = new GumpsEdit();
            form.FormClosed += GumpsEditClosed;
            form.Show();
            _isFormOpen = true;

            BtGumpsEdit.Enabled = false;
        }
        #endregion

        #region [ GumpsEditClosed ]
        private void GumpsEditClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtGumpsEdit.Enabled = true;
            }
        }
        #endregion

        #region [ AltitudeButton ]
        private void BtAltitudeTool_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtAltitudeTool, "Altitude Tool – map heights ↔ image");

            AltitudeToolForm form = new AltitudeToolForm();
            form.FormClosed += AltitudeTool_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtAltitudeTool.Enabled = false;
        }
        private void AltitudeTool_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            BtAltitudeTool.Enabled = true;
        }
        #endregion

        #region [ Copy Replace Map ]
        private void BtMapReplace_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtMapReplace, "Copy Replace Map");
            MapReplaceNewForm form = new MapReplaceNewForm();
            form.FormClosed += BtMapReplace_FormClosed;
            form.Show();
            _isFormOpen = true;
            this.BtMapReplace.Enabled = false;
        }

        private void BtMapReplace_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                this.BtMapReplace.Enabled = true;
            }
        }
        #endregion

        #region [ Art Mul Creator ]

        private void BtArtMul_Click(object sender, EventArgs e)
        {
            if (_isFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtArtMul, "ART Mul IDX Creator");

            var artMulIdxCreatorForm = new ARTMulIDXCreator();
            artMulIdxCreatorForm.FormClosed += ArtMulIdxCreatorFormClosed;
            artMulIdxCreatorForm.Show();
            _isFormOpen = true;

            BtArtMul.Enabled = false;
        }

        private void ArtMulIdxCreatorFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtArtMul.Enabled = true;
            }
        }
        #endregion

        #region [ ScriptCreator ]
        private void BtScriptCreator_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btScriptCreator, "Script Creator");

            var scriptCreatorForm = new ScriptCreator();
            scriptCreatorForm.FormClosed += ScriptCreatorFormClosed;
            scriptCreatorForm.Show();
            _isFormOpen = true;

            btScriptCreator.Enabled = false;
        }

        private void ScriptCreatorFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                btScriptCreator.Enabled = true;
            }
        }
        #endregion

        #region [ UOArtMerge ]
        private void BtUOArtMerge_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtUOArtMerge, "UO Art Merge");

            var uoArtMergeForm = new UOArtMergeForm();
            uoArtMergeForm.FormClosed += UOArtMergeFormClosed;
            uoArtMergeForm.Show();
            _isFormOpen = true;

            BtUOArtMerge.Enabled = false;
        }

        private void UOArtMergeFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtUOArtMerge.Enabled = true;
            }
        }
        #endregion

        #region [ Gump ID Rechner ]
        private void BtGumpIDRechner_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtGumpIDRechner, "Gump ID Rechner");

            var gumpIDRechnerForm = new GumpIDRechner();
            gumpIDRechnerForm.FormClosed += GumpIDRechnerFormClosed;
            gumpIDRechnerForm.Show();
            _isFormOpen = true;

            BtGumpIDRechner.Enabled = false;
        }

        private void GumpIDRechnerFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtGumpIDRechner.Enabled = true;
            }
        }
        #endregion

        #region [ IsoTiloSlicer ]
        private void BtIsoTiloSlicer_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtIsoTiloSlicer, "Iso Tilo Slicer");

            var isoTiloSlicerForm = new IsoTiloSlicer();
            isoTiloSlicerForm.FormClosed += IsoTiloSlicerFormClosed;
            isoTiloSlicerForm.Show();
            _isFormOpen = true;

            BtIsoTiloSlicer.Enabled = false;
        }

        private void IsoTiloSlicerFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtIsoTiloSlicer.Enabled = true;
            }
        }

        #endregion

        #region [ UOMap ]
        private bool _isUOMapFormOpen = false;

        private void UOMap_Click(object sender, EventArgs e)
        {
            if (_isUOMapFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.UOMap, "UOMap");

            var uomapForm = new UOMap();
            uomapForm.FormClosed += UOMap_FormClosed;
            uomapForm.Show();
            _isUOMapFormOpen = true;

            UOMap.Enabled = false;
        }

        private void UOMap_FormClosed(object sender, EventArgs e)
        {
            if (_isUOMapFormOpen)
            {
                _isUOMapFormOpen = false;
                UOMap.Enabled = true;
            }
        }
        #endregion

        #region [ DragonMapStudio ]
        private DragonMapStudio.DragonMapStudioForm _dragonMapStudioForm;

        private void BtDragonMapStudio_Click(object sender, EventArgs e)
        {
            // only one window: bring the open one to the front
            if (_dragonMapStudioForm != null && !_dragonMapStudioForm.IsDisposed)
            {
                if (_dragonMapStudioForm.WindowState == FormWindowState.Minimized)
                {
                    _dragonMapStudioForm.WindowState = FormWindowState.Normal;
                }
                _dragonMapStudioForm.BringToFront();
                _dragonMapStudioForm.Activate();
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(BtDragonMapStudio, "Dragon Map Studio – Karten malen, generieren und als MUL / UOP erzeugen");

            _dragonMapStudioForm = new DragonMapStudio.DragonMapStudioForm();
            _dragonMapStudioForm.FormClosed += (s, args) => _dragonMapStudioForm = null;
            _dragonMapStudioForm.Show();
        }
        #endregion

        #region [ BtTileArtForm ]
        private bool _isTileArtFormOpen = false;
        private void BtTileArtForm_Click(object sender, EventArgs e)
        {
            if (_isTileArtFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtTileArtForm, "TileArt");

            var tileArtForm = new TileArtForm();
            tileArtForm.FormClosed += TileArtForm_FormClosed;
            tileArtForm.Show();
            _isTileArtFormOpen = true;

            BtTileArtForm.Enabled = false;
        }

        private void TileArtForm_FormClosed(object sender, EventArgs e)
        {
            if (_isTileArtFormOpen)
            {
                _isTileArtFormOpen = false;
                BtTileArtForm.Enabled = true;
            }
        }
        #endregion

        #region [ Transitions ]
        private bool _isTransitionsFormOpen = false;

        private void BtTransitions_Click(object sender, EventArgs e)
        {
            if (_isTransitionsFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtTransitions, "TransitionsForm");

            var transitionsForm = new TransitionsForm();
            transitionsForm.FormClosed += TransitionsForm_FormClosed;
            transitionsForm.Show();
            _isTransitionsFormOpen = true;

            BtTransitions.Enabled = false;
        }

        private void TransitionsForm_FormClosed(object sender, EventArgs e)
        {
            if (_isTransitionsFormOpen)
            {
                _isTransitionsFormOpen = false;
                BtTransitions.Enabled = true;
            }
        }

        #endregion

        #region [ BtConverter ]
        private bool _isConverterFormOpen = false;

        private void BtConverter_Click(object sender, EventArgs e)
        {
            if (_isConverterFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtConverter, "Converter");

            var converterForm = new UoFiddler.Plugin.ConverterMultiTextPlugin.Forms.ConverterForm();
            converterForm.FormClosed += ConverterForm_FormClosed;
            converterForm.Show();
            _isConverterFormOpen = true;

            BtConverter.Enabled = false;
        }

        private void ConverterForm_FormClosed(object sender, EventArgs e)
        {
            if (_isConverterFormOpen)
            {
                _isConverterFormOpen = false;
                BtConverter.Enabled = true;
            }
        }
        #endregion

        #region [ ButtonParticleGray_Click ]
        private ParticleGrayForm particleGrayFormInstance;

        private void ButtonParticleGray_Click(object sender, EventArgs e)
        {
            if (particleGrayFormInstance == null || particleGrayFormInstance.IsDisposed)
            {
                // Create and configure the tooltip
                ToolTip toolTipParticleGray = new ToolTip();
                toolTipParticleGray.AutoPopDelay = 5000;
                toolTipParticleGray.InitialDelay = 1000;
                toolTipParticleGray.ReshowDelay = 500;
                toolTipParticleGray.ShowAlways = true;
                toolTipParticleGray.SetToolTip(this.buttonParticleGray, "Particle Gray Form");

                // Open the form
                particleGrayFormInstance = new ParticleGrayForm();
                particleGrayFormInstance.FormClosed += ParticleGrayFormClosed;
                particleGrayFormInstance.Show();
            }
            else
            {
                particleGrayFormInstance.BringToFront();
            }
        }

        private void ParticleGrayFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                buttonParticleGray.Enabled = true;
                particleGrayFormInstance = null;
            }
        }
        #endregion
    }
}
