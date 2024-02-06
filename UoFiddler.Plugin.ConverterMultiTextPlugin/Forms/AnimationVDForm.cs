// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{

    public partial class AnimationVDForm : Form
    {
        private byte[] radarColorBuffer;
        private byte[] mapBuffer;
        private const int BlockSize = 196;
        private const int BlockH = 8;
        private const int BlockV = 8;
        private const int MapBlockH = 896;
        private const int MapBlockV = 512;
        private const int ViewStrtH = 0;
        private const int ViewStrtV = 0;
        private const int ViewSizeH = 896;
        private const int ViewSizeV = 512;

        public AnimationVDForm()
        {
            InitializeComponent();
        }
    }
}
