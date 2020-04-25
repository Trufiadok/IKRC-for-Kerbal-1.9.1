using System;
using UnityEngine;

namespace InfernalRoboticsMinimal.Command
{
    public class Interpolator
    {
        public Interpolator()
        {
            IsModulo = true;
            MaxAcceleration = 200f;
            MaxVelocity = 5000f;
            MaxPosition = 180f;
            MinPosition = -180f;
            Velocity = 0f;
            Position = 0f;
            OldPosition = 0f;
            Active = false;
            CmdVelocity = 0f;
            CmdPosition = 0f;
            Initialised = false;
        }

        #region dynamic state

        public float CmdPosition { get; set; }

        public float CmdVelocity { get; set; }

        public bool Active { get; set; }

        public float Position { get; set; }

        public float OldPosition { get; set; }

        public float Velocity { get; set; }

        #endregion dynamic state

        #region config

        public float MinPosition { get; set; }

        public float MaxPosition { get; set; }

        public float MaxVelocity { get; set; }

        public float MaxAcceleration { get; set; }

        public bool IsModulo { get; set; }

        public bool Initialised { get; set; }

        private const float precisionDelta = 0.001f;

        #endregion config

        public float GetPosition()
        {
            return ReduceModulo(Position);
        }

        // incremental Command
        public void SetIncrementalCommand(float cPosDelta, float cVel)
        {
            float oldCmd = Active ? CmdPosition : Position;
            Logger.Log(string.Format("setIncCmd: oldCmd={0}, cPosDelta={1},cVel={2}", oldCmd, cPosDelta, cVel), Logger.Level.SuperVerbose);
            SetCommand(oldCmd + cPosDelta, cVel);
        }

        public void SetCommand(float cPos, float cVel)
        {

            if (cVel != CmdVelocity || cPos != CmdPosition)
            {
                if (IsModulo)
                {
                    if ((cVel != 0f) && (!float.IsPositiveInfinity(cVel)) && (!float.IsNegativeInfinity(cVel))) // modulo & positioning mode:
                    {                                                                     // add full turns if we move fast
                        Position = ReduceModulo(Position);
                        float brakeDist = 0.5f * Velocity * Velocity / (MaxAcceleration * 0.9f);                  // 10% acc reserve for interpolation errors
                        Position -= Math.Sign(Velocity) * (MaxPosition - MinPosition) * (float)Math.Round(brakeDist / (MaxPosition - MinPosition));
                        Logger.Log(string.Format("[Interpolator]: setCommand modulo correction: newPos= {0}", Position), Logger.Level.SuperVerbose);
                    }
                }
                Logger.Log(string.Format("[Interpolator]: setCommand {0}, {1}, (vel={2})\n", cPos, cVel, Velocity), Logger.Level.SuperVerbose);

                CmdVelocity = cVel;
                CmdPosition = cPos;
                Active = true;
            }

            //Debug.Log(string.Format("[TRF] {0} cVel = {1} cPos = {2}", 1, cVel, cPos));
            //Logger.Log(string.Format("[TRF] {0} cVel = {1} cPos = {2}", 1, cVel, cPos), Logger.Level.Debug);

            // make cVel non-negative?
        }

        public float ReduceModulo(float value)
        {
            if (!IsModulo)
                return value;

            float range = MaxPosition - MinPosition;
            float result = (value - MinPosition) % range;
            if (result < 0)        // result of % operation can be negative!
                result += range;

            result += MinPosition;
            return result;
        }

        public void Update(float deltaT)
        {
            if (!Active)
                return;

            OldPosition = Position;

            bool isSpeedMode = IsModulo && (float.IsPositiveInfinity(CmdPosition) || float.IsNegativeInfinity(CmdPosition) || (CmdVelocity == 0f));
            float maxDeltaVel = MaxAcceleration * deltaT;
            float targetPos = CmdPosition;
            if (!IsModulo)
            {
                targetPos = Math.Min(CmdPosition, MaxPosition);
                targetPos = Math.Max(targetPos, MinPosition);
            }
            Logger.Log(string.Format("Update: targetPos={0}, cmdPos={1},min/maxpos={2},{3}", targetPos, CmdPosition, MinPosition, MaxPosition), Logger.Level.SuperVerbose);

            // Stop servo
            if ((Math.Abs(Velocity) < maxDeltaVel) && ((targetPos == Position) || (CmdVelocity == 0f)))
            {
                Active = false;
                Velocity = 0;
                Logger.Log(string.Format("[Interpolator] finished! pos={0}, target={1}", Position, targetPos), Logger.Level.SuperVerbose);
                return;
            }

            float newVel = Math.Min(CmdVelocity, MaxVelocity);
            // Breaking servo
            if (!isSpeedMode)
            {
                var brakeVel = (float)Math.Sqrt(1.8f * MaxAcceleration * Math.Abs(targetPos - Position)); // brake ramp to cmdPos
                newVel = Math.Min(newVel, brakeVel);                                           // (keep 10% acc reserve)
            }
            //newVel *= Math.Sign(CmdPosition - Position);            // direction

            float diffPosition = CmdPosition - Position;
            if (diffPosition >= 180f || (diffPosition < 0f && diffPosition >= -180f))
                newVel *= -1f;
            //else if ((diffPosition < 180f && diffPosition >= 0f) || diffPosition < -180f)
            //    newVel *= 1f;

            newVel = Math.Min(newVel, Velocity + maxDeltaVel); // acceleration limit
            newVel = Math.Max(newVel, Velocity - maxDeltaVel);


            if ((Math.Abs(Velocity) < maxDeltaVel) && (Math.Abs(targetPos - Position) < (2f * newVel * deltaT))) // end conditions
            { // (generous to avoid oscillations)
                Logger.Log(string.Format("pos={0} targetPos={1}, 2f*maxDeltaVel*dalteT={2}", Position, targetPos, (2f * maxDeltaVel * deltaT)), Logger.Level.SuperVerbose);
                Position = targetPos;
                return;
            }

            Velocity = newVel;
            Position += Velocity * deltaT;

            //Debug.Log(string.Format("[TRF] {0} Velocity = {1} Position = {2}", 1, Velocity, Position));

            if (!IsModulo)
            {
                if (Position >= MaxPosition)
                { 			     // hard limit on Endpositions
                    Position = MaxPosition;
                    Velocity = 0f;
                }
                else if (Position <= MinPosition)
                {
                    Position = MinPosition;
                    Velocity = 0f;
                }
            }
            else
            {
                if (isSpeedMode)
                {
                    Position = ReduceModulo(Position);
                }
                else
                {
                    Position = ReduceModulo(Position);
                }
            }
        }

        public string StateToString()
        {
            var result = string.Format("Ipo: act= {0,6:0.0}", Position);
            result += string.Format(", {0,6:0.0}", Velocity);
            result += string.Format(", cmd= {0,6:0.0}", CmdPosition);
            result += string.Format(", {0,6:0.0}", CmdVelocity);
            result += string.Format(", active= {0}", Active);
            return result;
        }

        public override string ToString()
        {
            var result = "Interpolator {";
            result += "\n CmdPosition = " + CmdPosition;
            result += "\n CmdVelocity = " + CmdVelocity;
            result += "\n Active = " + Active;
            result += "\n Position = " + Position;
            result += "\n Velocity = " + Velocity;
            result += "\n MinPosition = " + MinPosition;
            result += "\n MaxPosition = " + MaxPosition;
            result += "\n MaxVelocity = " + MaxVelocity;
            result += "\n MaxAcceleration = " + MaxAcceleration;
            result += "\n IsModulo = " + IsModulo;
            return result + "\n}";
        }
    }
}