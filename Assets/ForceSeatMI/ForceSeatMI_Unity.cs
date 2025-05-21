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

using System.Runtime.InteropServices;

namespace MotionSystems
{
	class ForceSeatMI_Unity
	{
		public struct ExtraParameters
		{
			public float yaw;
			public float pitch;
			public float roll;
			public float sway; 
			public float heave;
			public float surge;
		}

		private ForceSeatMI m_api = null;
		private FSMI_Telemetry m_telemetry;
		private ForceSeatMI_ITelemetryInterface m_telemetryObject = null;

		public ForceSeatMI_Unity()
		{
			m_api = new ForceSeatMI();

			m_telemetry.mask = 0;
			m_telemetry.structSize = (byte)Marshal.SizeOf(m_telemetry);
			m_telemetry.state = FSMI_State.NO_PAUSE;
			m_telemetry.mask = FSMI_TEL_BIT.STATE |
							   FSMI_TEL_BIT.RPM |
							   FSMI_TEL_BIT.MAX_RPM |
							   FSMI_TEL_BIT.SPEED |
							   FSMI_TEL_BIT.YAW_PITCH_ROLL |
							   FSMI_TEL_BIT.YAW_PITCH_ROLL_SPEED |
							   FSMI_TEL_BIT.SWAY_HEAVE_SURGE_ACCELERATION |
							   FSMI_TEL_BIT.SWAY_HEAVE_SURGE_SPEED |
							   FSMI_TEL_BIT.GEAR_NUMBER;
		}

		public bool ActivateProfile(string profileName)
		{
			return m_api.ActivateProfile(profileName);
		}
		
		public bool SetAppID(string appId)
		{
			return m_api.SetAppID(appId);
		}

		public void SetTelemetryObject(ForceSeatMI_ITelemetryInterface telemetryObject)
		{
			m_telemetryObject = telemetryObject;

			if (null == m_telemetryObject)
			{
				Pause(true);
			}
		}

		public void Begin()
		{
			m_api.BeginMotionControl();

			if (null != m_telemetryObject)
			{
				m_telemetryObject.Begin();
			}
		}

		public void End()
		{
			if (null  != m_telemetryObject)
			{
				m_telemetryObject.End();
			}

			if (m_api.IsLoaded())
			{
				m_api.EndMotionControl();
				m_api.Dispose();
			}
		}

		public void Update(float deltaTime)
		{
			if (null != m_telemetryObject)
			{
				m_telemetryObject.Update(deltaTime, ref m_telemetry);
			}

			m_api.SendTelemetry(ref m_telemetry);
		}

		public void Pause(bool paused)
		{
			if (null != m_telemetryObject)
			{
				m_telemetryObject.Pause(paused);
			}

			if (paused)
			{
				FSMI_Telemetry pauseTelemetry = new FSMI_Telemetry();
				pauseTelemetry.mask           = FSMI_TEL_BIT.STATE;
				pauseTelemetry.structSize     = (byte)Marshal.SizeOf(pauseTelemetry);
				pauseTelemetry.state          = paused ? (byte)FSMI_State.PAUSE : (byte)FSMI_State.NO_PAUSE;

				m_api.SendTelemetry(ref pauseTelemetry);
			}
		}

		public void AddExtra(ExtraParameters parameters)
		{
			m_telemetry.mask      |= FSMI_TEL_BIT.EXTRA_YAW_PITCH_ROLL_SWAY_HEAVE_SURGE;
			m_telemetry.extraYaw   = parameters.yaw;
			m_telemetry.extraPitch = parameters.pitch;
			m_telemetry.extraRoll  = parameters.roll;
			m_telemetry.extraSway  = parameters.sway;
			m_telemetry.extraHeave = parameters.heave;
			m_telemetry.extraSurge = parameters.surge;
		}
	}
}
