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

using System;
using System.Runtime.InteropServices;

namespace MotionSystems
{
	/*
	 * This interface can used to get motion data from application or game.
	 *
	 * When the ForceSeatMI telemetry is implemented in application, any motion software is able to gather simulation data and
	 * control motion platform (2DOF, 3DOF or 6DOF). This structure should be filled by the application and sent to motion processing 
	 * system. It is require to fill "structSize" and "mask", other fields are OPTIONAL. It means that the application does not
	 * have to support or provide all parameters mentioned below, but it is good to provide as much as possible.
	 *
	 * FIELDS LEGEND:
	 * ==============
	 * All values are in local vehicle coordinates.
	 *
	 *   YAW   - rotation along vertical axis,
	 *          > 0 when vehicle front is rotating right,
	 *          < 0 when vehicle front is rotating left
	 *   PITCH - rotation along lateral axis,
	 *          > 0 when vehicle front is rotating up
	 *          > 0 when vehicle front is rotating down
	 *   ROLL  - rotation along longitudinal axis,
	 *          > 0 when vehicle highest point is rotating right,
	 *          < 0 when vehicle highest point is rotating left
	 *   SWAY  - transition along lateral axis,
	 *          > 0 when vehicle is moving right,
	 *          < 0 when vehicle is moving left
	 *   HEAVE - transition along vertical axis,
	 *          > 0 when vehicle is moving up,
	 *          < 0 when vehicle is moving down
	 *   SURGE - transition along longitudinal axis,
	 *          > 0 when vehicle is moving forward,
	 *          < 0 when vehicle is moving backward
	 *
	 * Please check below link for details description of yaw, pitch and roll:
	 * http://en.wikipedia.org/wiki/Ship_motions
	 */
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FSMI_Telemetry
	{
		// Put here sizeof(FSMI_Telemetry).
		// NOTE: This field is mandatory.
		public byte structSize;

		// Set bits in the mask to tell the motion software which fields in this stucture are provided by your application.
		public uint mask;

		// Only single bit is used in current version.
		//  (state & 0x01) == 0 -> no pause
		//  (state & 0x01) == 1 -> paused
		// NOTE: If the field is provided, set FSMI_TEL_BIT.STATE in the mask.
		public byte state;

		// Engine RPM.
		// NOTE: If the field is provided, set FSMI_TEL_BIT.RPM in the mask.
		public uint rpm;

		// Vehicle speed in m/s, can be < 0 for reverse.
		// NOTE: If this field is provided, set FSMI_TEL_BIT.SPEED in the mask.
		public float speed;

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.YAW_PITCH_ROLL in the mask.
		public float yaw;      // yaw in radians
		public float pitch;    // vehicle pitch in radians
		public float roll;     // vehicle roll in radians

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.YAW_PITCH_ROLL_ACCELERATION in the mask.
		public float yawAcceleration;   // radians/s^2
		public float pitchAcceleration; // radians/s^2
		public float rollAcceleration;  // radians/s^2

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.YAW_PITCH_ROLL_SPEED in the mask.
		public float yawSpeed;   // radians/s
		public float pitchSpeed; // radians/s
		public float rollSpeed;  // radians/s

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.SWAY_HEAVE_SURGE_ACCELERATION in the mask.
		public float swayAcceleration;  // m/s^2
		public float heaveAcceleration; // m/s^2
		public float surgeAcceleration; // m/s^2

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.SWAY_HEAVE_SURGE_SPEED in the mask.
		public float swaySpeed;  // m/s
		public float heaveSpeed; // m/s
		public float surgeSpeed; // m/s

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.PEDALS_POSITION in the mask.
		public byte gasPedalPosition;   // 0 to 100 (in percent)
		public byte brakePedalPosition; // 0 to 100 (in percent)
		public byte clutchPedalPosition; // 0 to 100 (in percent)

		// Current gear number, -1 for reverse, 0 for neutral, 1, 2, 3, ...
		// NOTE: If the field is provided, set FSMI_TEL_BIT.GEAR_NUMBER in the mask.
		public sbyte gearNumber;

		// Ground type, e.g. grass, dirt, gravel, please check FSMI_GroundType for details.
		// NOTE: If below fields are provided, set FSMI_TEL_BIT.GROUND_TYPE in the mask.
		public byte leftSideGroundType;
		public byte rightSideGroundType;

		// NOTE: If below fields are provided, set FSMI_TEL_BIT.COLLISION in the mask.
		public float collisionForce; // in Newtons (N), zero when there is no collision
		public float collisionYaw;   // yaw, pitch and roll for start point of collision force vector, end point is always in vehicle center
		public float collisionPitch;
		public float collisionRoll;

		// Global position,.
		// NOTE: If below fields are provided, set FSMI_TEL_BIT.GLOBAL_POSITION in the mask.
		public float globalPositionX; // right
		public float globalPositionY; // up
		public float globalPositionZ; // forward

		// Simulation time in milliseconds, can be relative (e.g. 0 means means application has just started).
		// NOTE: If the field is provided, set FSMI_TEL_BIT.TIME in the mask.
		public uint timeMs;

		// State of buttons, gun triggers, etc. It is passed directly to the motion platform.
		// NOTE: If the field is provided, set FSMI_TEL_BIT.TRIGGERS in the mask.
		public byte triggers;

		// Engine maximum RPM, it is used to simulate rev limiter.
		// NOTE: If the field is provided, set FSMI_TEL_BIT.MAX_RPM in the mask.
		public uint maxRpm;

		// Combination of FSMI_Flags.
		// NOTE: If the field is provided, set FSMI_TEL_BIT.FLAGS in the mask.
		public uint flags;

		// Custom field that can be used in script in ForceSeatPM to trigger user defined actions.
		// NOTE: If below fields are provided, set FSMI_TEL_BIT.AUX in the mask.
		public float aux1;
		public float aux2;
		public float aux3;
		public float aux4;
		public float aux5;
		public float aux6;
		public float aux7;
		public float aux8;

		// Extra values that will be added at the end of signal processing. Those values will NOT be
		// filtered, smoothed or processed in any way. They might be used to generate additional
		// vibrations or custom effects.
		// NOTE: If felds below are provided, set FSMI_TEL_BIT.EXTRA_YAW_PITCH_ROLL_SWAY_HEAVE_SURGE in the mask.
		public float extraYaw;   // rad
		public float extraPitch; // rad
		public float extraRoll;  // rad
		public float extraSway;  // mm
		public float extraHeave; // mm
		public float extraSurge; // mm
	}

	public enum FSMI_GroundType
	{
		UknownGround      = 0,
		TarmacGround      = 1,
		GrassGround       = 2,
		DirtGround        = 3,
		GravelGround      = 4,
		RumbleStripGround = 5
	}

	public struct FSMI_Flags
	{
		public const int ShiftLight       = (1 << 0);
		public const int AbsIsWorking     = (1 << 1);
		public const int EspIsWorking     = (1 << 2);
		public const int FrontWheelDrive  = (1 << 3);
		public const int RearWheelDrive   = (1 << 4);
	}

	public struct FSMI_TEL_BIT
	{
		public const int STATE                                              = (1 << 1);
		public const int RPM                                                = (1 << 2);
		public const int SPEED                                              = (1 << 3);

		public const int YAW_PITCH_ROLL                                     = (1 << 4);
		public const int YAW_PITCH_ROLL_ACCELERATION                        = (1 << 5);
		public const int YAW_PITCH_ROLL_SPEED                               = (1 << 6);
		public const int SWAY_HEAVE_SURGE_ACCELERATION                      = (1 << 7);
		public const int SWAY_HEAVE_SURGE_SPEED                             = (1 << 8);

		public const int PEDALS_POSITION                                    = (1 << 9);
		public const int GEAR_NUMBER                                        = (1 << 10);
		public const int GROUND_TYPE                                        = (1 << 11);
		public const int COLLISION                                          = (1 << 12);

		public const int GLOBAL_POSITION                                    = (1 << 13);
		public const int TIME                                               = (1 << 14);
		public const int TRIGGERS                                           = (1 << 15);

		public const int MAX_RPM                                            = (1 << 16);
		public const int FLAGS                                              = (1 << 17);
		public const int AUX                                                = (1 << 18);
		public const int EXTRA_YAW_PITCH_ROLL_SWAY_HEAVE_SURGE              = (1 << 19);
	}
}
