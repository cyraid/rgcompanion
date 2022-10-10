//========[[ Imports ]]=======================================================//

using System.ComponentModel;
using System.Runtime.CompilerServices;

//========[[ Namespace ]]=====================================================//

namespace RGCompanion {

  #region [[Object Extensions]]

  //**************************************************************************//
  //========[[ ObjectExtensions Class
  //**************************************************************************//

  public static class ObjectExtensions {

    //========[[ Public Class Methods ]]======================================//

    /**
     * <summary>A Helper Method to be called from Setting a Property, which will notify
     * the GUI the Property Changed.</summary>
     * 
     * <param name="eventHandler">The Event Handler for INotifyPropertyChanged, usually PropertyChanged.</param>
     * <param name="prop">A reference to the Property.</param>
     * <param name="newValue">The new Value for the Property.</param>
     * <param name="propName">Automatically filled in by the Compiler in a Property Setter.</param>
     * 
     */
    public static void NotifyPropertyChanged<T>(
      this object self, PropertyChangedEventHandler? eventHandler,
      ref T prop, T newValue, [CallerMemberName] string? propName = null
    ) {

      if (prop != null && prop.Equals(newValue))
        return;

      prop = newValue;
      eventHandler?.Invoke(self, new PropertyChangedEventArgs(propName));

    } // Procedure //

  } // Class //

  #endregion

} // Namespace //