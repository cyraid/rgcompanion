//========[[ Imports ]]=======================================================//

using Image = System.Drawing.Image;

using Vestris.ResourceLib;

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

//========[[ Namespace ]]=====================================================//

namespace RGCompanion.ResourceUtils {

  #region [[Exceptions]]

  //**************************************************************************//
  //========[[ ProcessException Class
  //**************************************************************************//

  /**
   * <summary>An Exception to be used for Processing.</summary>
   */
  public class ProcessException : Exception {
    public ProcessException(string msg) : base(msg) {}
  } // Class //

  #endregion

  //**************************************************************************//
  //========[[ ResourceProcessor Class
  //**************************************************************************//

  public class ResourceProcessor {

    //========[[ Public Constants ]]==========================================//

    /**
     * <summary>Icon Image Sizes to include in exe (for both Width and Height for 1:1).</summary>
     */
    public readonly int[] IMAGE_SIZES = new int[] { 256, 128, 64, 48, 32, 16 };

    //========[[ Internal Constants ]]========================================//

    internal const int IDI_ICON = 0x101;
    internal const int SWF_BLOB = 0x200;
    internal const int IDS_TITLE = 0x201;

    //========[[ Public Fields ]]=============================================//

    #region [[Processing Parameters]]

    public string title = "";
    public string iconFileName = "";
    public string exeFileName = "";
    public string swfFileName = "";
    public string targetFileName = "";

    public IProgress<double>? valueProgress;
    public IProgress<string>? statusProgress;

    public CancellationToken? cancelToken;

    #endregion

    //========[[ Private Fields ]]============================================//

    private int curStep = 0;
    private int maxSteps = 1;

    //========[[ Public Methods ]]============================================//

    /**
     * <summary>Process the Resources into the final Output File.</summary>
     */
    public void Process() {

      // Begin Process //

      curStep = 0;
      maxSteps = 5;

      valueProgress?.Report(0.0);
      statusProgress?.Report("Starting..");

      // Process Resources //

      RunStep("Validating", () => ValidateData());
      RunStep("Copying", () => CopyFile());
      RunStep("Replacing Embedded SWF", () => ReplaceSWF());
      RunStep("Replacing Icon", () => ReplaceIcon());
      RunStep("Replacing Title", () => ReplaceTitle());

    } // Procedure //

    //========[[ Private Methods ]]===========================================//

    #region [[Helper Methods]]

    /**
     * <summary>Checks if an EXE has an Embedded SWF BLOB
     * (specific for Ruffle Game).</summary>
     */
    private bool HasSWF(string exeFileName) {

      // Iterate through Resources and find SWF_BLOB //

      using (var vi = new ResourceInfo()) {

        vi.Load(exeFileName);

        foreach (ResourceId id in vi.ResourceTypes) {
          foreach (Resource resource in vi.Resources[id]) {

            if (resource.Name.Id.ToInt64() == SWF_BLOB)
              return true;

          } // ForEach //
        } // ForEach //
      } // Using //

      return false;

    } // Function //

    /**
     * <summary>Get List of Converted/Resized DiB Bitmaps from the Icon File.</summary>
     */
    private List<DeviceIndependentBitmap> GetImagesFromIcon(string iconFileName) {

      var iconImages = new List<DeviceIndependentBitmap>();
      var iconFile = new IconFile(iconFileName);

      // Sort Icons (largest first) //
      // Note: When Icon Width/Height are 0, they are 256 //

      iconFile.Icons.Sort((src, dest) => src.Width == 0 || src.Width > dest.Width ? -1 : 1);

      foreach (var size in IMAGE_SIZES) {

        // Find matching Icon with size //

        var icon = iconFile.Icons.Find((icon) => size == 256 && icon.Width == 0 || size == icon.Width);

        DeviceIndependentBitmap diBmp;

        if (icon != null) {

          diBmp = icon.Image;

        } else {

          var destBmp = BitmapUtils.CreateIconBitmap(size);
          var srcBmp = BitmapUtils.ConvertDibToBitmap(iconFile.Icons[0].Image);

          DrawIconImage(srcBmp, destBmp, size);

          diBmp = BitmapUtils.ConvertBitmapToDib(destBmp);

        } // Else //

        iconImages.Add(diBmp);

      } // ForEach //

      return iconImages;

    } // Function //

    /**
     * <summary>Get List of Converted/Resized DiB Bitmaps based on the Sizes Array.</summary>
     */
    private List<DeviceIndependentBitmap> GetImagesFromImage(string imageFileName) {

      var iconImages = new List<DeviceIndependentBitmap>();
      var origImage = Image.FromFile(imageFileName);

      foreach (var size in IMAGE_SIZES) {

        var destBmp = BitmapUtils.CreateIconBitmap(size);

        DrawIconImage(origImage, destBmp, size);
        iconImages.Add(BitmapUtils.ConvertBitmapToDib(destBmp));

      } // ForEach //

      return iconImages;

    } // Function //

    private void DrawIconImage(Image srcBmp, Image destBmp, int size) {

      // Draw the Original Image to the Bottom of the Dest Image (resizing as necessary) //

      using var graphics = Graphics.FromImage(destBmp);

      graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
      graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
      graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
      graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

      graphics.DrawImage(srcBmp, 0, size, size, size);

    } // Procedure //

    #endregion

    #region [[Step Helper Methods]]

    /**
     * <summary>Call when beginning a Step, not needed if using
     * <seealso cref="RunStep">RunStep()</seealso></summary>
     */
    private void BeginStep(string status) {
      statusProgress?.Report($"{status}..");
    } // Procedure //

    /**
     * <summary>Call when ending a Step, not needed if using
     * <seealso cref="RunStep">RunStep()</seealso></summary>
     */
    private void EndStep() {

      var progress = Math.Clamp(++curStep / (double)maxSteps, 0.0d, 1.0d);

      if (progress >= 1.0d)
        statusProgress?.Report("Completed");

      valueProgress?.Report(progress);
      cancelToken?.ThrowIfCancellationRequested();

    } // Procedure //

    /**
     * <summary>Run a Step: Call an Action and set the Status.</summary>
     */
    private void RunStep(string status, Action action) {

      BeginStep(status);

      try {
        action();
        EndStep();
      } catch {
        throw new ProcessException($"Failed while {status}");
      } // Try, Catch //

    } // Procedure //

    #endregion

    #region [[Processing Steps]]

    private void ValidateData() {

      // Check if Files Exist //

      if (!File.Exists(iconFileName))
        throw new ProcessException("Icon File does not exist");

      if (!File.Exists(swfFileName))
        throw new ProcessException("SWF File does not exist");

      if (!File.Exists(exeFileName))
        throw new ProcessException("Ruffle Game Executable does not exist");

      // Check if the exe has Embedded SWF //

      if (!HasSWF(exeFileName))
        throw new ProcessException("No placeholder SWF inside Ruffle Game Executable found");

    } // Procedure //

    private void CopyFile() {

      // Error if the target and original exe are the same! //

      if (exeFileName.Equals(targetFileName))
        throw new ProcessException("Target executable cannot be the same executable as Source");

      // Attempt to copy the exe //

      try {
        File.Copy(exeFileName, targetFileName, true);
      } catch (IOException) {
        throw new ProcessException("Failed to write target (Copying Ruffle Game Executable)");
      } // Try, Catch //

    } // Procedure //

    private void ReplaceSWF() {

      var dataRes = new GenericResource(
        new ResourceId(Kernel32.ResourceTypes.RT_RCDATA), new ResourceId(SWF_BLOB),
        ResourceUtil.USENGLISHLANGID
      );

      dataRes.Data = File.ReadAllBytes(swfFileName);

      Utils.RetryWithException(() => dataRes.SaveTo(targetFileName));

    } // Procedure //

    private void ReplaceIcon() {

      var iconImages = iconFileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase)
        ? GetImagesFromIcon(iconFileName)
        : GetImagesFromImage(iconFileName);

      // Create Icon Resources to replace the Icon //

      var iconDirRes = new IconDirectoryResource() {
        Name = new ResourceId(IDI_ICON),
        Language = ResourceUtil.USENGLISHLANGID
      };

      for (var i = 0; i < iconImages.Count; i++) {

        var iconRes = new IconResource() {
          Id = (ushort)(i + 1),
          Name = new ResourceId(IDI_ICON),
          Image = iconImages[i],
          Language = iconDirRes.Language
        };

        iconDirRes.Icons.Add(iconRes);

      } // For //

      // Replace the Icon Resources //

      Utils.RetryWithException(() => iconDirRes.DeleteFrom(targetFileName));
      Utils.RetryWithException(() => iconDirRes.SaveTo(targetFileName));

    } // Procedure //

    private void ReplaceTitle() {

      var strRes = new StringResource(StringResource.GetBlockId(IDS_TITLE));

      strRes[IDS_TITLE] = title;
      strRes.Language = ResourceUtil.USENGLISHLANGID;

      Utils.RetryWithException(() => strRes.SaveTo(targetFileName));

    } // Procedure //

    #endregion

  } // Class //

} // Namespace //
