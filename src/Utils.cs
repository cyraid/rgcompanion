//========[[ Imports ]]=======================================================//

using PixelFormat = System.Drawing.Imaging.PixelFormat;

using Vestris.ResourceLib;

using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System;

using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

//========[[ Namespace ]]=====================================================//

namespace RGCompanion {

  #region [[Exceptions]]

  //**************************************************************************//
  //========[[ RetryException Class
  //**************************************************************************//

  /**
   * <summary>An Exception to use for Retrying.</summary>
   */
  public class RetryException : Exception {
    public RetryException(string msg) : base(msg) {}
  } // Class //

  #endregion

  #region [[Generic Utilities]]

  //**************************************************************************//
  //========[[ Utils Class
  //**************************************************************************//

  public static class Utils {

    //========[[ Public Class Methods ]]======================================//

    /**
     * <summary>Calls an action, and retries if action raises an exception.</summary>
     * 
     * <param name="action">The Action to call up to <paramref name="numAttempts"/> times.</param>
     * <param name="numAttempts">The number of attemps to retry if failed.</param>
     * <param name="delayMS">The delay to wait before attempting to try again (in milliseconds).</param>
     * 
     */
    public static void RetryWithException(
      Action action, int numAttempts = 3, int delayMS = 1000
    ) {

      var retries = numAttempts;

      while (retries-- > 0) {
        try {
          
          action();
          return;

        } catch {
          
          // Action Failed, delay before retry. //
          
          if (delayMS > 0)
            Thread.Sleep(delayMS);

        } // Try, Catch //
      } // While //

      throw new RetryException($"Failed after retrying {numAttempts} times.");

    } // Procedure //

  } // Class //

  #endregion

  #region [[Dialog Utilities]]

  //**************************************************************************//
  //========[[ DialogUtils Class
  //**************************************************************************//

  public static class DialogUtils {

    //========[[ Public Class Methods ]]======================================//

    /**
     * <summary>Displays an Information Dialog with a given message.</summary>
     */
    public static void Info(string msg) {
      MessageBox.Show(msg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    } // Procedure //

    /**
     * <summary>Displays an Error Dialog with a given message.</summary>
     */
    public static void Error(string msg) {
      MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    } // Procedure //

  } // Class //

  #endregion

  #region [[Bitmap Utilities]]

  //**************************************************************************//
  //========[[ BitmapUtils Class
  //**************************************************************************//

  public static class BitmapUtils {

    //========[[ Internal Constants ]]========================================//

    internal static readonly int BMP_FILE_HEADER_SIZE = Marshal.SizeOf<Gdi32.BITMAPFILEHEADER>();

    //========[[ Public Class Methods ]]======================================//

    /**
     * <summary>Converts a Windows Bitmap to a Device Independent Bitmap needed
     * for the Vestris Library.</summary> 
     * 
     * <param name="bmp">The Windows Bitmap to convert to a Device Independent Bitmap (DiB)</param>
     * 
     * <returns>A Device Independent Bitmap (DiB) for the Vestris Library.</returns>
     * 
     */
    public static DeviceIndependentBitmap ConvertBitmapToDib(Bitmap bmp) {

      // Save the Bitmap to a Memory Stream //

      var bmpMemStream = new MemoryStream();

      bmp.Save(bmpMemStream, ImageFormat.Bmp);

      // Copy our Bitmap Data to a Device Independent Bitmap (DiB) .. //

      var bmpFileData = bmpMemStream.ToArray();
      var bmpData = new byte[bmpFileData.Length - BMP_FILE_HEADER_SIZE];

      // .. but we're only interested in the Bitmap part, not the File Header //

      Array.Copy(bmpFileData, BMP_FILE_HEADER_SIZE, bmpData, 0, bmpData.Length);

      return new DeviceIndependentBitmap(bmpData);

    } // Function //

    /**
     * <summary>Converts a Device Independent Bitmap needed to a Windows Bitmap
     * for the Vestris Library.</summary> 
     * 
     * <param name="diBmp">The Device Independent Bitmap (DiB) to convert to a Windows Bitmap.</param>
     *
     * <returns>A Windows Bitmap.</returns>
     * 
     */
    public static Bitmap ConvertDibToBitmap(DeviceIndependentBitmap diBmp) {
      return (Bitmap)Image.FromStream(new MemoryStream(diBmp.Data));
    } // Function //

    /**
     * <summary>Creates a Windows Bitmap with the size given
     * (Width and Height will be the same).</summary> 
     * 
     * <param name="size">The Size for the Bitmap (eg. 256 for a 256x256).</param>
     * 
     * <returns>A Windows Bitmap.</returns>
     * 
     */
    public static Bitmap CreateIconBitmap(int size) {

      // Create a Bitmap at the requested Image Size //
      // Note: We use height * 2 because the Vestris Library requires it. //

      return new Bitmap(size, size * 2, PixelFormat.Format32bppArgb);

    } // Function //

  } // Class //

  #endregion

} // Namespace //
