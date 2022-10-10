//========[[ Imports ]]=======================================================//

using Microsoft.Win32;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

using RGCompanion.ResourceUtils;

//========[[ Namespace ]]=====================================================//

namespace RGCompanion.Forms {

  //**************************************************************************//
  //========[[ MainWindow Class
  //**************************************************************************//

  public partial class MainWindow : Window, INotifyPropertyChanged {

    //========[[ Public Properties ]]=========================================//

    public bool ProcessGroupEnabled {
      get => _processGroupEnabled;
      set => this.NotifyPropertyChanged(PropertyChanged, ref _processGroupEnabled, value);
    } // Property //

    //========[[ Public Events ]]=============================================//

    public event PropertyChangedEventHandler? PropertyChanged;

    //========[[ Private Fields ]]============================================//

    private bool _processGroupEnabled = true;
    private CancellationTokenSource cancelTokenSrc = new CancellationTokenSource();

    //========[[ Public Methods ]]============================================//

    public MainWindow() {
      
      InitializeComponent();
      DataContext = this;

      var ver = Assembly.GetExecutingAssembly().GetName().Version;
      var product = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product;

      // Replace the Window Title with the Product Name / Version //

      if (ver != null)
        Title = $"{product} [{ver.Major}.{ver.Minor}]";

      LoadSettings();

    } // Constructor //

    //========[[ Protected Methods ]]=========================================//

    protected override void OnClosed(EventArgs e) {
      base.OnClosed(e);
      SaveSettings();
    } // Procedure //

    //========[[ Event Methods ]]=============================================//

    private void iconButton_Click(object sender, RoutedEventArgs e) {

      var fileDialog = new OpenFileDialog {
        FileName = "favicon",
        DefaultExt = ".ico",
        Filter = "Image Files (.ico,.png,.jpg)|*.ico;*.png;*.jpg;*.jpeg"
      };

      if (fileDialog.ShowDialog() == true)
        iconInputFile.Text = fileDialog.FileName;

    } // Procedure //

    private void swfButton_Click(object sender, RoutedEventArgs e) {

      var fileDialog = new OpenFileDialog {
        FileName = "game",
        DefaultExt = ".swf",
        Filter = "SWF Files (.swf)|*.swf"
      };

      if (fileDialog.ShowDialog() == true)
        swfInputFile.Text = fileDialog.FileName;

    } // Procedure //

    private void targetButton_Click(object sender, RoutedEventArgs e) {

      var fileDialog = new SaveFileDialog {
        FileName = "game_player_out",
        DefaultExt = ".exe",
        Filter = "EXE Files (.exe)|*.exe"
      };

      if (fileDialog.ShowDialog() == true)
        targetInputFile.Text = fileDialog.FileName;

    } // Procedure //

    private void iconInputFile_TextChanged(object sender, TextChangedEventArgs e) {
      CheckCanApply();
    } // Procedure //

    private void swfInputFile_TextChanged(object sender, TextChangedEventArgs e) {
      CheckCanApply();
    } // Procedure //

    private void targetInputFile_TextChanged(object sender, TextChangedEventArgs e) {
      CheckCanApply();
    } // Procedure //

    private void titleText_TextChanged(object sender, TextChangedEventArgs e) {
      CheckCanApply();
    } // Procedure //

    private async void applyButton_Click(object sender, RoutedEventArgs e) {

      try {

        progressBar.Visibility = Visibility.Visible;
        applyButton.IsEnabled = false;
        ProcessGroupEnabled = false;

        // Setup Resource Processor //

        var resProcessor = new ResourceProcessor {
          title = titleText.Text,
          iconFileName = iconInputFile.Text,
          exeFileName = Properties.Settings.Default.RuffleGamePath,
          swfFileName = swfInputFile.Text,
          targetFileName = targetInputFile.Text,
          valueProgress = new Progress<double>((value) => UpdateProgress(value)),
          statusProgress = new Progress<string>((value) => UpdateStatus(value)),
          cancelToken = cancelTokenSrc.Token
        };

        // Execute Resource Processor (and await the result) //

        await Task.Run(() => resProcessor.Process());

      } catch (ProcessException ex) {

        // Error during the ResourceProcessor Processing //

        DialogUtils.Error(ex.Message);
        
        progressBar.Value = 0;
        statusLabel.Content = $"Error ({ex.Message})";

      } catch (OperationCanceledException) {

        // User Cancelled the Processing //

      } // Try, Catch //

      applyButton.IsEnabled = true;
      ProcessGroupEnabled = true;

    } // Procedure //

    private void exitButton_Click(object sender, RoutedEventArgs e) {
      cancelTokenSrc.Cancel();
      Application.Current.Shutdown();
    } // Procedure //

    //========[[ Private Methods ]]===========================================//

    private void CheckCanApply() {
      applyButton.IsEnabled = titleText.Text != "" && iconInputFile.Text != ""
        && swfInputFile.Text != "" && targetInputFile.Text != "";
    } // Procedure //

    private void UpdateProgress(double progress) {
      progressBar.Value = progress;
    } // Procedure //

    private void UpdateStatus(string status) {
      statusLabel.Content = status;
    } // Procedure //

    private void LoadSettings() {

      var settings = Properties.Settings.Default;

      titleText.Text = settings.LastTitle;
      iconInputFile.Text = settings.LastIconPath;
      swfInputFile.Text = settings.LastSWFPath;
      targetInputFile.Text = settings.LastOutputPath;

    } // Procedure //

    private void SaveSettings() {

      var settings = Properties.Settings.Default;

      settings.LastTitle = titleText.Text;
      settings.LastIconPath = iconInputFile.Text;
      settings.LastSWFPath = swfInputFile.Text;
      settings.LastOutputPath = targetInputFile.Text;

      settings.Save();
      
    } // Procedure //

  } // Class //

} // Namespace //
