/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using UoFiddler.Plugin.Enhanced.EcUltima;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// A sub editor of the Enhanced Client tab.
    /// </summary>
    public interface IEcEditor
    {
        /// <summary>
        /// Called when a client folder was loaded (or with null when it was closed).
        /// Editors load their data lazily when they become visible.
        /// </summary>
        void SetClient(EcClient client);

        /// <summary>
        /// Called when the editor's tab is shown.
        /// </summary>
        void OnActivated();
    }
}
