//========[[ Global Variables ]]==============================================//

var doc = fl.getDocumentDOM();
var output = fl.outputPanel;

//========[[ Functions ]]=====================================================//

function log(msg) {
  output.trace("[TestMovie]: " + msg);
} // Function //

function error(msg) {
  output.trace("[TestMovie] !! Error !!: " + msg);
} // Function //

function testMovie() {

  var swfPath = doc.path.replace(".fla", ".swf");
  var swfURI = "file:///" + swfPath.replace(/\\/g, "/");
  var rufflePath = "C:\\Program Files\\Ruffle Game Companion\\assets\\ruffle_game.exe";
  var ruffleURI = "file:///" + rufflePath.replace(/\\/g, "/");

  if (swfPath == doc.path) {
    error("FLA Path (" + doc.path + ") should not equal SWF Path (" + swfPath + ")");
    return;
  } // IF //

  doc.exportSWF(swfURI, true);

  if (!FLfile.exists(ruffleURI)) {
    error("Ruffle Game Path (" + rufflePath + ") does not exist.");
    return;
  } // IF //

  if (!FLfile.exists(swfURI)) {
    error("Published SWF Path (" + swfPath + ") does not exist.");
    return;
  } // IF //

  log("Running " + rufflePath + " with SWF " + swfPath);

  var res = TestMovie.runCommand("C:\\Windows\\System32", "cmd.exe", "/c \"\"" + rufflePath +
    "\" \"" + swfPath + "\" --fullscreen\"", 0, 1);

  if (!res)
    error("Failed to Test Movie (error: " + res + ")");

} // Function //

//========[[ Main ]]==========================================================//

output.clear();

if (!doc.path) {

  error("No Output Path detected");

} else if (!doc.canTestMovie()) {

  error("Cannot Test Movie.");

} else {

  testMovie();

} // Else //
