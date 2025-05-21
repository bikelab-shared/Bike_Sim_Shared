/*
 * Copyright (C) 2012-2021 Motion Systems
 * 
 * This file is part of ForceSeat motion system.
 *
 * www.motionsystems.eu
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

// If for some reason Microsoft.Win32.Registry is not available, use hardcoded path:
// C:\Program Files (x86)\MotionSystems\ForceSeatPM
// or change in Unity player Api Compatibility Level to .NET 4.x
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace MotionSystems
{
	///
	/// Wrapper for ForceSeatMI native DLL
	///
	public class ForceSeatMI: IDisposable
	{
		public ForceSeatMI()
		{
			LoadAndCreate();
		}

		public void Dispose()
		{
			Close();
		}

		public bool IsLoaded()
		{
			return m_api != IntPtr.Zero;
		}

		///
		/// Call this function when the SIM is ready for sending data to the motion platform.
		///
		public bool BeginMotionControl()
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiBeginMotionControl(m_api) != 0;
		}

		///
		/// Call this function to when the SIM does not want to send any more data to the motion platform.
		///
		public bool EndMotionControl()
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiEndMotionControl(m_api) != 0;
		}

		///
		/// It gets current status of the motion platform. It can be called at any time.
		///
		public bool GetPlatformInfoEx(ref FSMI_PlatformInfo info, uint platformInfoStructSize, uint timeout)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiGetPlatformInfoEx(m_api, ref info, platformInfoStructSize, timeout) != 0;
		}

		///
		/// It sends updated telemetry information to ForceSeatPM for further processing. 
		/// Make sure that 'mask' and 'state' fields are set correctly.
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		public bool SendTelemetry(ref FSMI_Telemetry telemetry)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTelemetry(m_api, ref telemetry) != 0;
		}

		///
		/// It sends updated telemetry information to ForceSeatPM for further processing. 
		/// Make sure that 'mask' and 'state' fields are set correctly.
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		/// NOTE: sfx and audioEffects are optional.
		///
		public bool SendTelemetry2(ref FSMI_Telemetry telemetry,
		                           ref FSMI_SFX sfx, 
		                           ref FSMI_TactileAudioBasedFeedbackEffects audioEffects)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTelemetry2(m_api, ref telemetry, ref sfx, ref audioEffects) != 0;
		}

		///
		/// Use this function if you want to specify position of the top table (top frame) in logical units (percent of maximum rotation and translation).
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		public bool SendTopTablePosLog(ref FSMI_TopTablePositionLogical position)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTopTablePosLog(m_api, ref position) != 0;
		}

		///
		/// Use this function if you want to specify position of the top table (top frame) in logical units (percent of maximum rotation and translation).
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		/// NOTE: sfx and audioEffects are optional.
		///
		public bool SendTopTablePosLog2(ref FSMI_TopTablePositionLogical position,
		                                ref FSMI_SFX sfx, 
		                                ref FSMI_TactileAudioBasedFeedbackEffects audioEffects)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTopTablePosLog2(m_api, ref position, ref sfx, ref audioEffects) != 0;
		}

		///
		/// Use this function if you want to specify position of the top table (top frame) in physical units (radians and milimeters).
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		public bool SendTopTablePosPhy(ref FSMI_TopTablePositionPhysical position)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTopTablePosPhy(m_api, ref position) != 0;
		}

		///
		/// Use this function if you want to specify position of the top table (top frame) in physical units (radians and milimeters).
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		/// NOTE: sfx and audioEffects are optional.
		///
		public bool SendTopTablePosPhy2(ref FSMI_TopTablePositionPhysical position,
		                                ref FSMI_SFX sfx, 
		                                ref FSMI_TactileAudioBasedFeedbackEffects audioEffects)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTopTablePosPhy2(m_api, ref position, ref sfx, ref audioEffects) != 0;
		}

		///
		/// Use this function if you want to specify transformation matrix for the top table (top frame). 
		/// It is recommended only for 6DoF in cases when rotation center is not in default point (0, 0, 0).
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		public bool SendTopTableMatrixPhy(ref FSMI_TopTableMatrixPhysical matrix)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTopTableMatrixPhy(m_api, ref matrix) != 0;
		}

		///
		/// Use this function if you want to specify transformation matrix for the top table (top frame). 
		/// It is recommended only for 6DoF in cases when rotation center is not in default point (0, 0, 0).
		/// Make sure to call ForceSeatMI_BeginMotionControl before this function is called.
		///
		/// NOTE: sfx and audioEffects are optional.
		///
		public bool SendTopTableMatrixPhy2(ref FSMI_TopTableMatrixPhysical matrix, 
		                                   ref FSMI_SFX sfx, 
		                                   ref FSMI_TactileAudioBasedFeedbackEffects audioEffects)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTopTableMatrixPhy2(m_api, ref matrix, ref sfx, ref audioEffects) != 0;
		}

		///
		/// Call this function to generate vibrations on connected tactile transcuder.
		/// 
		///  DEPRECATED: It is recommended to use ForceSeatMI_SendTelemetry2, ForceSeatMI_SendTopTablePosLog2,
		///              ForceSeatMI_SendTopTablePosPhy2 or ForceSeatMI_SendTopTableMatrixPhy2 as replacement.
		///
		public bool SendTactileFeedbackEffects(ref FSMI_TactileAudioBasedFeedbackEffects audioEffects)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSendTactileFeedbackEffects(m_api, ref audioEffects) != 0;
		}

		///
		/// Call this function to set required profile in ForceSeatPM application.
		///
		public bool ActivateProfile(string profileName)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiActivateProfile(m_api, profileName) != 0;
		}

		///
		/// Call this function to set application ID.
		///
		public bool SetAppID(string appId)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiSetAppID(m_api, appId) != 0;
		}

		///
		/// Call this function to park the motion platform.
		///
		public bool Park(FSMI_ParkMode parkMode)
		{
			if (m_api == IntPtr.Zero) return false;
			return m_fsmiPark(m_api, (byte)parkMode) != 0;
		}

		#region Internals
		private IntPtr m_api = IntPtr.Zero;
		private IntPtr m_apiDll = IntPtr.Zero;

		~ForceSeatMI()
		{
			// Just in case it is not deleted
			Close();
		}

		private Delegate LoadFunction<T>(string functionName)
		{
			var addr = GetProcAddress(m_apiDll, functionName);
			if (addr == IntPtr.Zero) 
			{
				return null;
			}
			return Marshal.GetDelegateForFunctionPointer(addr, typeof(T));
		}

		private void LoadAndCreate()
		{
			bool is64Bits = IntPtr.Size > 4;
			
			string registryPath = is64Bits 
				? "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\MotionSystems\\ForceSeatPM" 
				: "HKEY_LOCAL_MACHINE\\SOFTWARE\\MotionSystems\\ForceSeatPM";
			
			string dllName = is64Bits 
				? "ForceSeatMI64.dll" 
				: "ForceSeatMI32.dll";

			// If for some reason Microsoft.Win32.Registry is not available, use hardcoded path:
			// C:\Program Files (x86)\MotionSystems\ForceSeatPM
			// or change in Unity player Api Compatibility Level to .NET 4.x

			// Let's check if there is ForceSeatPM installed, if yes there is ForceSeatMIxx.dll that can be used
			string installationPath = (string)Registry.GetValue(registryPath, "InstallationPath", null);
			if (installationPath != null)
			{
				m_apiDll = LoadLibrary(installationPath + "\\" + dllName);
			}

			// If there is still not ForceSeatMIxx.dll found, then let's try in standard search path
			if (m_apiDll == IntPtr.Zero)
			{
				m_apiDll = LoadLibrary(dllName);
			}

			if (m_apiDll != IntPtr.Zero) 
			{
				m_fsmiCreate                     = (ForceSeatMI_Create_Delegate)                    LoadFunction<ForceSeatMI_Create_Delegate>                    ("ForceSeatMI_Create");
				m_fsmiDelete                     = (ForceSeatMI_Delete_Delegate)                    LoadFunction<ForceSeatMI_Delete_Delegate>                    ("ForceSeatMI_Delete");
				m_fsmiBeginMotionControl         = (ForceSeatMI_BeginMotionControl_Delegate)        LoadFunction<ForceSeatMI_BeginMotionControl_Delegate>        ("ForceSeatMI_BeginMotionControl");
				m_fsmiEndMotionControl           = (ForceSeatMI_EndMotionControl_Delegate)          LoadFunction<ForceSeatMI_EndMotionControl_Delegate>          ("ForceSeatMI_EndMotionControl");
				m_fsmiGetPlatformInfoEx          = (ForceSeatMI_GetPlatformInfoEx_Delegate)         LoadFunction<ForceSeatMI_GetPlatformInfoEx_Delegate>         ("ForceSeatMI_GetPlatformInfoEx");
				m_fsmiSendTelemetry              = (ForceSeatMI_SendTelemetry_Delegate)             LoadFunction<ForceSeatMI_SendTelemetry_Delegate>             ("ForceSeatMI_SendTelemetry");
				m_fsmiSendTelemetry2             = (ForceSeatMI_SendTelemetry2_Delegate)            LoadFunction<ForceSeatMI_SendTelemetry2_Delegate>            ("ForceSeatMI_SendTelemetry2");
				m_fsmiSendTopTablePosLog         = (ForceSeatMI_SendTopTablePosLog_Delegate)        LoadFunction<ForceSeatMI_SendTopTablePosLog_Delegate>        ("ForceSeatMI_SendTopTablePosLog");
				m_fsmiSendTopTablePosLog2        = (ForceSeatMI_SendTopTablePosLog2_Delegate)       LoadFunction<ForceSeatMI_SendTopTablePosLog2_Delegate>       ("ForceSeatMI_SendTopTablePosLog2");
				m_fsmiSendTopTablePosPhy         = (ForceSeatMI_SendTopTablePosPhy_Delegate)        LoadFunction<ForceSeatMI_SendTopTablePosPhy_Delegate>        ("ForceSeatMI_SendTopTablePosPhy");
				m_fsmiSendTopTablePosPhy2        = (ForceSeatMI_SendTopTablePosPhy2_Delegate)       LoadFunction<ForceSeatMI_SendTopTablePosPhy2_Delegate>       ("ForceSeatMI_SendTopTablePosPhy2");
				m_fsmiSendTopTableMatrixPhy      = (ForceSeatMI_SendTopTableMatrixPhy_Delegate)     LoadFunction<ForceSeatMI_SendTopTableMatrixPhy_Delegate>     ("ForceSeatMI_SendTopTableMatrixPhy");
				m_fsmiSendTopTableMatrixPhy2     = (ForceSeatMI_SendTopTableMatrixPhy2_Delegate)    LoadFunction<ForceSeatMI_SendTopTableMatrixPhy2_Delegate>    ("ForceSeatMI_SendTopTableMatrixPhy2");
				m_fsmiSendTactileFeedbackEffects = (ForceSeatMI_SendTactileFeedbackEffects_Delegate)LoadFunction<ForceSeatMI_SendTactileFeedbackEffects_Delegate>("ForceSeatMI_SendTactileFeedbackEffects");
				m_fsmiActivateProfile            = (ForceSeatMI_ActivateProfile_Delegate)           LoadFunction<ForceSeatMI_ActivateProfile_Delegate>           ("ForceSeatMI_ActivateProfile");
				m_fsmiSetAppID                   = (ForceSeatMI_SetAppID_Delegate)                  LoadFunction<ForceSeatMI_SetAppID_Delegate>                  ("ForceSeatMI_SetAppID");
				m_fsmiPark                       = (ForceSeatMI_Park_Delegate)                      LoadFunction<ForceSeatMI_Park_Delegate>                      ("ForceSeatMI_Park");

				if (m_fsmiCreate                     != null && 
					m_fsmiDelete                     != null && 
					m_fsmiBeginMotionControl         != null && 
					m_fsmiEndMotionControl           != null &&
					m_fsmiGetPlatformInfoEx          != null &&
					m_fsmiSendTelemetry              != null &&
					m_fsmiSendTelemetry2             != null &&
					m_fsmiSendTopTablePosLog         != null &&
					m_fsmiSendTopTablePosLog2        != null &&
					m_fsmiSendTopTablePosPhy         != null &&
					m_fsmiSendTopTablePosPhy2        != null &&
					m_fsmiSendTopTableMatrixPhy      != null &&
					m_fsmiSendTopTableMatrixPhy2     != null &&
					m_fsmiSendTactileFeedbackEffects != null &&
					m_fsmiActivateProfile            != null &&
					m_fsmiSetAppID                   != null &&
					m_fsmiPark                       != null)
				{
					m_api = m_fsmiCreate();
				}
			}
		}

		private void Close()
		{
			if (m_api != IntPtr.Zero)
			{
				m_fsmiDelete(m_api);
				m_api = IntPtr.Zero;
			}

			m_fsmiCreate                     = null;
			m_fsmiDelete                     = null;
			m_fsmiBeginMotionControl         = null;
			m_fsmiSendTelemetry              = null;
			m_fsmiSendTelemetry2             = null;
			m_fsmiEndMotionControl           = null;
			m_fsmiGetPlatformInfoEx          = null;
			m_fsmiSendTopTablePosLog         = null;
			m_fsmiSendTopTablePosLog2        = null;
			m_fsmiSendTopTablePosPhy         = null;
			m_fsmiSendTopTablePosPhy2        = null;
			m_fsmiSendTopTableMatrixPhy      = null;
			m_fsmiSendTopTableMatrixPhy2     = null;
			m_fsmiSendTactileFeedbackEffects = null;
			m_fsmiActivateProfile            = null;
			m_fsmiSetAppID                   = null;
			m_fsmiPark                       = null;

			if (m_apiDll != IntPtr.Zero)
			{
				FreeLibrary(m_apiDll);
				m_apiDll = IntPtr.Zero;
			}
		}
		#endregion

		#region DLLImports
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string libname);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate IntPtr ForceSeatMI_Create_Delegate                  ();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void ForceSeatMI_Delete_Delegate                    (IntPtr api);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_BeginMotionControl_Delegate        (IntPtr api);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_EndMotionControl_Delegate          (IntPtr api);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_GetPlatformInfoEx_Delegate         (IntPtr api, ref FSMI_PlatformInfo info, uint platformInfoStructSize, uint timeout);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTelemetry_Delegate             (IntPtr api, ref FSMI_Telemetry info);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTelemetry2_Delegate            (IntPtr api, ref FSMI_Telemetry info, ref FSMI_SFX sfx, ref FSMI_TactileAudioBasedFeedbackEffects audioEffects);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTopTablePosLog_Delegate        (IntPtr api, ref FSMI_TopTablePositionLogical position);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTopTablePosLog2_Delegate        (IntPtr api, ref FSMI_TopTablePositionLogical position, ref FSMI_SFX sfx, ref FSMI_TactileAudioBasedFeedbackEffects audioEffects);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTopTablePosPhy_Delegate        (IntPtr api, ref FSMI_TopTablePositionPhysical position);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTopTablePosPhy2_Delegate        (IntPtr api, ref FSMI_TopTablePositionPhysical position, ref FSMI_SFX sfx, ref FSMI_TactileAudioBasedFeedbackEffects audioEffects);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTopTableMatrixPhy_Delegate     (IntPtr api, ref FSMI_TopTableMatrixPhysical matrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTopTableMatrixPhy2_Delegate     (IntPtr api, ref FSMI_TopTableMatrixPhysical matrix, ref FSMI_SFX sfx, ref FSMI_TactileAudioBasedFeedbackEffects audioEffects);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SendTactileFeedbackEffects_Delegate(IntPtr api, ref FSMI_TactileAudioBasedFeedbackEffects audioEffects);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_ActivateProfile_Delegate(IntPtr api, string profileName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_SetAppID_Delegate(IntPtr api, string appId);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate byte ForceSeatMI_Park_Delegate(IntPtr api, byte parkMode);

		private ForceSeatMI_Create_Delegate                     m_fsmiCreate;
		private ForceSeatMI_Delete_Delegate                     m_fsmiDelete;
		private ForceSeatMI_BeginMotionControl_Delegate         m_fsmiBeginMotionControl;
		private ForceSeatMI_EndMotionControl_Delegate           m_fsmiEndMotionControl;
		private ForceSeatMI_GetPlatformInfoEx_Delegate          m_fsmiGetPlatformInfoEx;
		private ForceSeatMI_SendTelemetry_Delegate              m_fsmiSendTelemetry;
		private ForceSeatMI_SendTelemetry2_Delegate             m_fsmiSendTelemetry2;
		private ForceSeatMI_SendTopTablePosLog_Delegate         m_fsmiSendTopTablePosLog;
		private ForceSeatMI_SendTopTablePosLog2_Delegate        m_fsmiSendTopTablePosLog2;
		private ForceSeatMI_SendTopTablePosPhy_Delegate         m_fsmiSendTopTablePosPhy;
		private ForceSeatMI_SendTopTablePosPhy2_Delegate        m_fsmiSendTopTablePosPhy2;
		private ForceSeatMI_SendTopTableMatrixPhy_Delegate      m_fsmiSendTopTableMatrixPhy;
		private ForceSeatMI_SendTopTableMatrixPhy2_Delegate     m_fsmiSendTopTableMatrixPhy2;
		private ForceSeatMI_SendTactileFeedbackEffects_Delegate m_fsmiSendTactileFeedbackEffects;
		private ForceSeatMI_ActivateProfile_Delegate            m_fsmiActivateProfile;
		private ForceSeatMI_SetAppID_Delegate                   m_fsmiSetAppID;
		private ForceSeatMI_Park_Delegate                       m_fsmiPark;
		#endregion
	}
}
