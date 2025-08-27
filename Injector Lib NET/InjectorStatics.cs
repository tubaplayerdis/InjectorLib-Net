using System.Diagnostics;
using System.Runtime.InteropServices;
using static Injector_Lib_NET.InjectorNatives;

namespace Injector_Lib_NET
{
    public class InjectorStatics
    {

        /// <summary>
        /// Gets the process ID of a process given the name
        /// </summary>
        /// <param name="processName">Name of the process</param>
        /// <returns>-1 if the process was not found, the process ID otherwise</returns>
        public static int GetProcessIdFromName(string processName)
        {
            Process[] allProcess = Process.GetProcesses();
            foreach (Process process in allProcess)
            {
                if (process.ProcessName == processName) return process.Id;
            }

            return -1;
        }

        /// <summary>
        /// Gets the library name from a path. paths can use / or \\ formatting
        /// </summary>
        /// <param name="libraryPath">The path of the library</param>
        /// <returns>null if the path is not valid, and the name of the library otherwise</returns>
        public static string? GetLibraryNameFromPath(string libraryPath)
        {
            String path = libraryPath;
            var split = path.LastIndexOf('/');
            if (split == -1)
            {
                split = path.LastIndexOf('\\');
            }

            if (split == -1) return null;

            return path.Substring(split + 1);
        }

        /// <summary>
        /// Injects a dynamic link library into the given process.
        /// </summary>
        /// <param name="libraryPath">Path to the dynamic link library to inject</param>
        /// <param name="processId">Process ID of the process to inject into</param>
        /// <returns>Whether injection was successful</returns>
        public static bool InjectLibrary(string libraryPath, int processId)
        {
            IntPtr hProcess = InjectorNatives.OpenProcess(
                InjectorNatives.PROCESS_CREATE_THREAD | InjectorNatives.PROCESS_QUERY_INFORMATION |
                InjectorNatives.PROCESS_VM_OPERATION | InjectorNatives.PROCESS_VM_WRITE |
                InjectorNatives.PROCESS_VM_READ, false, processId);

            if (hProcess == IntPtr.Zero) return false;

            IntPtr loadLibraryAddr =
                InjectorNatives.GetProcAddress(InjectorNatives.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddr == IntPtr.Zero) return false;

            IntPtr allocMemAddress = InjectorNatives.VirtualAllocEx(hProcess, IntPtr.Zero,
                (uint)((libraryPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                InjectorNatives.MEM_COMMIT | InjectorNatives.MEM_RESERVE, InjectorNatives.PAGE_READWRITE);

            if (allocMemAddress == IntPtr.Zero) return false;

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(libraryPath);
            bool writeResult =
                InjectorNatives.WriteProcessMemory(hProcess, allocMemAddress, bytes, (uint)bytes.Length, out _);
            if (!writeResult) return false;

            IntPtr threadHandle = InjectorNatives.CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr,
                allocMemAddress, 0, IntPtr.Zero);
            if (threadHandle == IntPtr.Zero) return false;

            InjectorNatives.CloseHandle(threadHandle);
            InjectorNatives.CloseHandle(hProcess);
            return true;
        }

        /// <summary>
        /// Checks whether the specified dynamic link library is injected (loaded) in the specified program
        /// </summary>
        /// <param name="libraryName">Name of the dynamic link library to check</param>
        /// <param name="processId">Process ID of the program to check in.</param>
        /// <returns></returns>
        public static bool IsLibraryInjected(string libraryName, int processId)
        {
            Process process = Process.GetProcessById(processId);
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == libraryName) return true;
            }

            return false;
        }


        /// <summary>
        /// Un-injects a dynamic link library from the specified program
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static bool UninjectLibrary(string libraryName, int processId)
        {
            ProcessModule? libraryModule = null;
            Process process = Process.GetProcessById(processId);
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName == libraryName) libraryModule = module;
            }

            if (libraryModule == null) return false;

            IntPtr hProcess = InjectorNatives.OpenProcess(
                InjectorNatives.PROCESS_CREATE_THREAD | InjectorNatives.PROCESS_QUERY_INFORMATION |
                InjectorNatives.PROCESS_VM_OPERATION | InjectorNatives.PROCESS_VM_WRITE |
                InjectorNatives.PROCESS_VM_READ, false, processId);

            if (hProcess == IntPtr.Zero) return false;

            IntPtr freeLibraryAddr =
                InjectorNatives.GetProcAddress(InjectorNatives.GetModuleHandle("kernel32.dll"), "FreeLibrary");
            if (freeLibraryAddr == IntPtr.Zero) return false;

            IntPtr threadHandle = InjectorNatives.CreateRemoteThread(hProcess, IntPtr.Zero, 0, freeLibraryAddr,
                libraryModule.BaseAddress, 0, IntPtr.Zero);
            if (threadHandle == IntPtr.Zero) return false;

            InjectorNatives.WaitForSingleObject(threadHandle, InjectorNatives.INFINITE);

            InjectorNatives.CloseHandle(threadHandle);
            InjectorNatives.CloseHandle(hProcess);

            return true;
        }

    }
}