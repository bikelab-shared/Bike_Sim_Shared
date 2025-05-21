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

using UnityEngine;

namespace MotionSystems
{
	class ForceSeatMI_Vehicle : ForceSeatMI_ITelemetryInterface
	{
		const float FSMI_VT_ACC_LOW_PASS_FACTOR = 0.5f;
		const float FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR = 0.5f;

		private bool m_firstCall       = true;
		private float m_prevSurgeSpeed = 0;
		private float m_prevSwaySpeed  = 0;
		private float m_prevHeaveSpeed = 0;
		private Vector3 m_prevAngles   = new Vector3();
		private Rigidbody m_rb         = null;
		private sbyte m_gearNumber     = 0;

		public ForceSeatMI_Vehicle(Rigidbody rb)
		{
			m_rb = rb;
		}

		public virtual void Begin()
		{
			m_firstCall = true;
		}

		public virtual void End()
		{
			m_firstCall = true;
		}

		public virtual void Update(float deltaTime, ref FSMI_Telemetry telemetry)
		{
			var velocity      = m_rb.transform.InverseTransformDirection(m_rb.velocity);
			var rotation      = m_rb.transform.rotation;
			var localRotation = m_rb.transform.localRotation;

			telemetry.rpm        = 0;
			telemetry.maxRpm     = 0;
			telemetry.speed      = velocity.magnitude * 3.6f; // km/h
			telemetry.surgeSpeed = velocity.z;
			telemetry.swaySpeed  = velocity.x;
			telemetry.heaveSpeed = velocity.y;
			telemetry.roll       = -Mathf.Deg2Rad * (localRotation.eulerAngles.z > 180 ? localRotation.eulerAngles.z - 360 : localRotation.eulerAngles.z);
			telemetry.pitch      = -Mathf.Deg2Rad * (localRotation.eulerAngles.x > 180 ? localRotation.eulerAngles.x - 360 : localRotation.eulerAngles.x);
			telemetry.yaw        =  Mathf.Deg2Rad * (localRotation.eulerAngles.y > 180 ? localRotation.eulerAngles.y - 360 : localRotation.eulerAngles.y);

			if (m_firstCall)
			{
				m_firstCall = false;

				telemetry.surgeAcceleration = 0;
				telemetry.swayAcceleration  = 0;
				telemetry.heaveAcceleration = 0;
				telemetry.pitchSpeed        = 0;
				telemetry.rollSpeed         = 0;
				telemetry.yawSpeed          = 0;
			}
			else
			{
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.surgeAcceleration, (telemetry.surgeSpeed - m_prevSurgeSpeed) / deltaTime, FSMI_VT_ACC_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.swayAcceleration,  (telemetry.swaySpeed - m_prevSwaySpeed)   / deltaTime, FSMI_VT_ACC_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.heaveAcceleration, (telemetry.heaveSpeed - m_prevHeaveSpeed) / deltaTime, FSMI_VT_ACC_LOW_PASS_FACTOR);

				var deltaAngles = new Vector3(
					Mathf.DeltaAngle(m_rb.transform.eulerAngles.x, m_prevAngles.x),
					Mathf.DeltaAngle(m_rb.transform.eulerAngles.y, m_prevAngles.y),
					Mathf.DeltaAngle(m_rb.transform.eulerAngles.z, m_prevAngles.z)
					);

				ForceSeatMI_Utils.LowPassFilter(ref telemetry.rollSpeed,  deltaAngles.z / deltaTime, FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.pitchSpeed, deltaAngles.x / deltaTime, FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR);
				ForceSeatMI_Utils.LowPassFilter(ref telemetry.yawSpeed,   deltaAngles.y / deltaTime, FSMI_VT_ANGLES_SPEED_LOW_PASS_FACTOR);
			}

			m_prevSurgeSpeed = telemetry.surgeSpeed;
			m_prevSwaySpeed  = telemetry.swaySpeed;
			m_prevHeaveSpeed = telemetry.heaveSpeed;
			m_prevAngles.x   = m_rb.transform.eulerAngles.x;
			m_prevAngles.y   = m_rb.transform.eulerAngles.y;
			m_prevAngles.z   = m_rb.transform.eulerAngles.z;

			telemetry.gearNumber = m_gearNumber;
		}

		public virtual void Pause(bool paused)
		{
			m_firstCall = true;
		}

		public void SetGearNumber(int gearNumber)
		{
			m_gearNumber = (sbyte)gearNumber;
		}
	}
}
