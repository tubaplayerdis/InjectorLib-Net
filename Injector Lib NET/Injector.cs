using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Injector_Lib_NET
{
    public class Injector
    {
        private readonly int _processId;
        private readonly string _libraryPath;

        /// <summary>
        /// Does not inject a DLL. Use the Inject() function to inject.
        /// Creates an injector object using the specified processId and libraryPath.
        /// </summary>
        /// <param name="processId">Process ID of the process to inject</param>
        /// <param name="libraryPath">Path to the library to be injected</param>
        public Injector(int processId, string libraryPath)
        {
            _processId = processId;
            _libraryPath = libraryPath;
        }

        /// <summary>
        /// Does not inject a DLL. Use the Inject() function to inject.
        /// Creates an injector object using the specified processId and libraryPath.
        /// </summary>
        /// <param name="processName">Name of the process to inject</param>
        /// <param name="libraryPath">Path to the library to be injected</param>
        public Injector(string processName, string libraryPath)
        {
            _processId = InjectorStatics.GetProcessIdFromName(processName);
            _libraryPath = libraryPath;
        }

        /// <summary>
        /// Injects the library into the process that was previously specified.
        /// </summary>
        /// <returns>Whether the injection was successful</returns>
        public bool Inject()
        {
            return InjectorStatics.InjectLibrary(_libraryPath,  _processId);
        }

        /// <summary>
        /// Whether the previusly specified library is injected into previously specified program.
        /// </summary>
        /// <returns>Whether the previusly specified library is injected into previously specified program.</returns>
        public bool IsInjected()
        {
            string? libraryName = InjectorStatics.GetLibraryNameFromPath(_libraryPath);
            if(libraryName == null) return false;
            return InjectorStatics.IsLibraryInjected(libraryName, _processId);
        }

        /// <summary>
        /// Uninject the previously specified library from the previously specified program.
        /// </summary>
        /// <returns>Whether the un-injection succeeded</returns>
        public bool Uninject()
        {
            return InjectorStatics.UninjectLibrary(_libraryPath, _processId);
        }
    }
}