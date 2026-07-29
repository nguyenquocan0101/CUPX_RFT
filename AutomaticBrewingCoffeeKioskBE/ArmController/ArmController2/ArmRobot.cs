
using System;
using System.IO;
using System.Threading;
using fairino;

namespace ArmController2
{
    internal class ArmRobot
    {
        public Robot robot;
        private int rrpc;

        public int automationMode = 0;
        public int mannualMode = 1;
        // Thông số an toàn
        private double defaultSpeed = 20;
        private float vel = 100.0f;
        private float acc = 50.0f;
        private float ovl = 50.0f;

        public ArmRobot(string ip)
        {
            robot = new Robot();
            //thiết lập kết nối
            robot.SetReconnectParam(true, 100, 100);
            rrpc = robot.RPC(ip);

            robot.SetSpeed((int)defaultSpeed); //set tốc độ mặc định
            SetMode(automationMode);
            robot.RobotEnable(1);
        }
        #region Xử lý trạng thái
        /// <summary>
        /// Điều chỉnh mode của robot
        /// </summary>
        /// <param name="mode">0 - Automation | 1 - Mannual </param>
        /// <returns></returns>
        public int SetMode(int mode)
        {
            return robot.Mode(mode);
        }

        public bool IsRunning()
        {
            byte state = 2;
            robot.GetRobotMotionDone(ref state);
            if (state == 0) return true;

            return false;
        }
        #endregion

        #region Xử lý lỗi
        public void ClearError() => robot.ResetAllError();

        #endregion

        #region Lấy tạo độ khớp và tọa độ trong không gian
        /// <summary>
        /// Lấy tọa độ khớp hiện tại của cánh tay robot (J1-6)
        /// </summary>
        /// <returns>J1-6</returns>
        public double[] GetCurrentJointPos()
        {
            double[] jointPosition = new double[6];
            JointPos currentJointPos = new JointPos(0, 0, 0, 0, 0, 0);
            robot.GetActualJointPosDegree(0, ref currentJointPos);

            jointPosition[0] = currentJointPos.jPos[0];
            jointPosition[1] = currentJointPos.jPos[1];
            jointPosition[2] = currentJointPos.jPos[2];
            jointPosition[3] = currentJointPos.jPos[3];
            jointPosition[4] = currentJointPos.jPos[4];
            jointPosition[5] = currentJointPos.jPos[5];

            return jointPosition;
        }
        /// <summary>
        /// Xử lý thuận động học để tính ra tọa độ trong không gian từ tọa độ khớp hiện tại 
        /// J1-6 -> X, Y, Z, Rx, Ry, Rz
        /// </summary>
        /// <param name="jointPos">tạo độ khớp hiện tại</param>
        /// <returns>X, Y, Z, Rx, Ry, Rz</returns>
        public double[] GetForwardKinDesPose(JointPos jointPos)
        {
            double[] desPose = new double[6];
            DescPose targetDescPose = new DescPose(0, 0, 0, 0, 0, 0);
            robot.GetForwardKin(jointPos, ref targetDescPose);

            desPose[0] = targetDescPose.tran.x; // X
            desPose[1] = targetDescPose.tran.y; // Y
            desPose[2] = targetDescPose.tran.z; // Z
            desPose[3] = targetDescPose.rpy.rx; // RX
            desPose[4] = targetDescPose.rpy.ry; // RY
            desPose[5] = targetDescPose.rpy.rz; // RZ

            return desPose;
        }

        public double[] GetForwardKinDesPose(double[] jointPos)
        {
            double[] desPose = new double[6];
            DescPose targetDescPose = new DescPose(0, 0, 0, 0, 0, 0);
            JointPos jointPosObj = new JointPos(
                jointPos[0],
                jointPos[1],
                jointPos[2],
                jointPos[3],
                jointPos[4],
                jointPos[5]
            );
            robot.GetForwardKin(jointPosObj, ref targetDescPose);

            desPose[0] = targetDescPose.tran.x; // X
            desPose[1] = targetDescPose.tran.y; // Y
            desPose[2] = targetDescPose.tran.z; // Z
            desPose[3] = targetDescPose.rpy.rx; // RX
            desPose[4] = targetDescPose.rpy.ry; // RY
            desPose[5] = targetDescPose.rpy.rz; // RZ

            return desPose;
        }


        #endregion

        #region Di chuyển robot
        public bool RunAvailable(JointPos jointPos, DescPose descPose)
        {
            bool hasSolution = false;
            robot.GetInverseKinHasSolution(0, descPose, jointPos, ref hasSolution);
            return hasSolution;
        }

        /// <summary>
        /// Di chuyển robot đến tọa độ khớp J1-6, có thể truyền tọa độ trong không gian.
        /// Cần kiểm tra giữa J1-6 và Tọa độ trong không gian có thể di chuyển được không bằng RunAvailable(JointPos jointPos, DescPose descPose)
        /// </summary>
        /// <param name="jointPos"></param>
        /// <param name="descPose"></param>
        /// <returns></returns>
        public int MoveJ(JointPos jointPos, DescPose? descPose = null)
        {
            int tool = 0;
            int user = 0;
            float blendT = 0.0f;
            byte flag = 0;
            ExaxisPos ePos = new ExaxisPos(0, 0, 0, 0);
            DescPose offset = new DescPose();

            DescPose refDescPose = new DescPose(0, 0, 0, 0, 0, 0);
            if (descPose == null)
            {
                robot.GetForwardKin(jointPos, ref refDescPose);
            }
            else refDescPose = descPose.Value;
            return robot.MoveJ(jointPos, refDescPose, tool, user, vel, acc, ovl, ePos, blendT, flag, offset);
        }

        /// <summary>
        /// Từ tạo độ khớp, đi nhưng vẫn giữ nguyên
        /// </summary>
        /// <returns></returns>
        public int MoveFowardWithStableRotation(JointPos targetJointPos)
        {
            DescPose targetDesPose = new DescPose(0, 0, 0, 0, 0, 0);
            robot.GetForwardKin(targetJointPos, ref targetDesPose);

            targetDesPose.rpy.rx = 90; // cố định Rx = 90
            targetDesPose.rpy.ry = 0; // giữ nguyên


            robot.GetInverseKin(0, targetDesPose, -1, ref targetJointPos);
            int tool = 0;
            int user = 0;
            float blendT = 0.0f;
            byte flag = 0;


            bool hasSolution = false;
            robot.GetInverseKinHasSolution(0, targetDesPose, targetJointPos, ref hasSolution);

            ExaxisPos ePos = new ExaxisPos(0, 0, 0, 0);
            DescPose offset = new DescPose();

            return robot.MoveJ(targetJointPos, targetDesPose, tool, user, vel, acc, ovl, ePos, blendT, flag, offset);
        }

        public int MoveInverseWithStableRotation(DescPose targetDesPose, JointPos currentJointPos)
        {
            targetDesPose.rpy.rx = 90; // Cố định Rx
            targetDesPose.rpy.ry = 0;  // Cố định Ry

            var targetJointPos = new JointPos(0, 0, 0, 0, 0, 0);
            bool hasSolution = false;
            bool found = false;

            // Thử các giá trị RZ từ -90 đến 90 để tìm RZ hợp lệ
            for (int rz = -90; rz <= 90; rz += 5)
            {
                targetDesPose.rpy.rz = rz;

                // Dùng inverse kinematics để tính toán
                int result = robot.GetInverseKinRef(0, targetDesPose, currentJointPos, ref targetJointPos);

                // Kiểm tra thật sự có solution không
                robot.GetInverseKinHasSolution(0, targetDesPose, targetJointPos, ref hasSolution);

                if (result == 0 && hasSolution)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("Not able to move");
                return -1;
            }

            // Nếu tìm được, thì MoveJ
            int tool = 0;
            int user = 0;
            float blendT = 0.0f;
            byte flag = 0;
            ExaxisPos ePos = new ExaxisPos(0, 0, 0, 0);
            DescPose offset = new DescPose();

            return robot.MoveJ(targetJointPos, targetDesPose, tool, user, vel, acc, ovl, ePos, blendT, flag, offset);
        }



        #endregion

        #region Quỹ đạo

        /// <summary>
        /// Tính thời gian ghi tối đa (tính bằng giây) dựa trên số điểm và chu kỳ lấy mẫu
        /// </summary>
        /// <param name="maxPoints">Số điểm tối đa có thể ghi</param>
        /// <param name="samplingPeriodMs">Chu kỳ lấy mẫu (ms) - chỉ được là 2, 4 hoặc 8</param>
        /// <returns>Thời gian tối đa (giây)</returns>
        private static double CalculateMaxRecordingTime(int preriodMs)
        {
            int maxPoints = 20000;
            if (preriodMs != 2 && preriodMs != 4 && preriodMs != 8)
                throw new ArgumentException("Sampling period must be 2, 4, or 8 milliseconds.");

            double totalMilliseconds = maxPoints * preriodMs;
            return totalMilliseconds / 1000.0;
        }

        public double[] GetTeachingPoint(string name)
        {
            var data = new Double[20];
            if (string.IsNullOrEmpty(name))
                return data;


            var rs = robot.GetRobotTeachingPoint(name, ref data);
            return data;
        }
        Timer recordTimer;
        double elapsedSeconds = 0;

        public void Record(string recordName)
        {
            if (recordTimer != null)
            {
                recordTimer.Dispose();
                recordTimer = null;
                elapsedSeconds = 0;
            }

            StartRecord(recordName);
            recordTimer = new Timer(OnRecordTimerElapsed, null, 0, 1000);
        }
        private void OnRecordTimerElapsed(object state)
        {
            elapsedSeconds++;
            //Console.WriteLine($"Đã ghi được: {elapsedSeconds} giây");

            if (elapsedSeconds >= CalculateMaxRecordingTime(2))
            {
                //Console.WriteLine("Đã đạt thời gian ghi tối đa. Dừng timer.");
                if (recordTimer != null)
                {
                    recordTimer.Dispose();
                    recordTimer = null;
                    StopRecord();
                }
                elapsedSeconds = 0;
            }
        }

        private void StartRecord(string recordName)
        {
            int type = 1; //lưu theo j1-6
            string name = recordName;
            int period_ms = 2;
            UInt16 di_choose = 0;
            UInt16 do_choose = 0;
            //set param cho quy đạo
            robot.SetTPDParam(type, name, period_ms, di_choose, do_choose);
            //vào chế độ manual
            SetMode(mannualMode);
            Thread.Sleep(1000);
            robot.DragTeachSwitch(1); //bật chế độ drag teach
            //bắt đầu ghi
            robot.SetTPDStart(type, name, period_ms, di_choose, do_choose);
        }

        private void StopRecord()
        {
            //Dừng ghi
            robot.SetWebTPDStop();
            //thoát chế độ teach drag
            robot.DragTeachSwitch(0);
        }

        public void LoadRecord(string recordName)
        {
            int tool = 0;
            int user = 0;
            float blendT = -1.0f;
            int config = -1;
            byte blend = 1;

            DescPose desc_pose = new DescPose();
            //lấy điểm bắt đầu của arm trong record
            robot.GetTPDStartPose(recordName, ref desc_pose);
            robot.SetTrajectoryJSpeed(100.0f);
            //load record lên
            robot.LoadTPD(recordName);
            //di chuyển cánh tay đến điểm bắt đầu của record
            robot.MoveCart(desc_pose, tool, user, vel, acc, ovl, blendT, config);
            robot.MoveTPD(recordName, blend, 100.0f);
        }
        #endregion

        #region RunScript
        public int RunScript(string name)
        {
            //robot.RobotEnable(1);
            //robot.Mode(0);
            string luaFile = $"{name}.lua";
            //string pointDb = $"{name}.db";
            string basePath = Directory.GetCurrentDirectory(); //bin//Release   hoặc bin/Debug
            string scriptPath = Path.Combine(basePath, "scripts", luaFile);
            //string pointDbPath = Path.Combine(basePath, "scripts", $"{name}", pointDb);
            Console.WriteLine(scriptPath);
            //Console.WriteLine(pointDbPath);
            int rtn = 0;
            int errorCode = -1;
            string errorStr = "";
            //rtn = robot.PointTableUpLoad(pointDbPath);
            //Console.WriteLine("Upload pointtable");
            //rtn = robot.PointTableSwitch(pointDb, ref errorStr);
            //if (!string.IsNullOrEmpty(errorStr)) return errorCode;
           // Console.WriteLine("Switch pointtable");
            rtn = robot.LuaUpload(scriptPath, ref errorStr);
            //Console.WriteLine("Upload lua file");
            //if (!string.IsNullOrEmpty(errorStr)) return errorCode;
            //rtn = robot.PointTableUpdateLua(pointDb, luaFile, ref errorStr);
            //Console.WriteLine("Map");
            //if (!string.IsNullOrEmpty(errorStr)) return errorCode;
            robot.Mode(0);
            rtn = robot.ProgramLoad($"/fruser/{luaFile}");
            Console.WriteLine("Run");
            rtn = robot.ProgramRun();
            return rtn;
        }
        #endregion


    }
}
