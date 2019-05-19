using CommandLine;

namespace FEM.Models
{
    class CLIOptions
    {
        [Option('j', "json", Required = false, HelpText = "Extract parameters from command line?")]
        public string JSON { get; set; }

        [Option('m', "mesh-only", Required = false, HelpText = "Should return mesh only?")]
        public bool MeshOnly {get; set; }
    }
}
