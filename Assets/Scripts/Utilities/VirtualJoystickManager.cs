
using UnityEngine;
 
    /// <summary>
    /// 虚拟摇杆管理器
    /// </summary>
    public class VirtualJoystickManager : SingletonMonobehaviour<VirtualJoystickManager> {
        
        [Header("是否始终可视")] public bool alwaysVisible; //是否始终可视
        [Header("是否限制虚拟摇杆位置")] public bool restrictVirtualJoystickPos; //是否限制虚拟摇杆位置
 
        [Header("虚拟摇杆物体")] [Header("==========")]
        public GameObject virtualJoystick; //虚拟摇杆父物体
 
        public GameObject inside; //内环
        public GameObject outside; //外环
 
        [Header("最大拖拽距离")] [Header("==========")]
        public float maxDragLength; //最大拖拽距离
 
        private Vector2 virtualJoystickCenter; //虚拟轴中心
        private Vector2 virtualJoystickStartPos; //虚拟轴位置

        private void Start() {
            //设置虚拟轴的位置
            virtualJoystickStartPos = virtualJoystick.transform.position;
        }
 
        private void Update() {
            //如果限制虚拟轴位置并且虚拟轴位置超出了限制范围则不进行任何操作
            if (restrictVirtualJoystickPos && JudgeIsValidRange() == false) {
                return;
            }
 
            //更新显示
            UpdateShow();
 
            //更新虚拟摇杆位置
            if (Input.GetMouseButtonDown(0)) {
                UpdateVirtualJoystickPos();
            }
            else if (Input.GetMouseButtonUp(0)) {
                outside.transform.position = virtualJoystickStartPos;
                inside.transform.position = virtualJoystickStartPos;
            }
 
            //更新内环位置(限制拖拽范围)
            if (Input.GetMouseButton(0)) {
                UpdateInsidePos();
            }
        }
 
        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateShow() {
            if (alwaysVisible) {
                inside.SetActive(true);
                outside.SetActive(true);
            }
            else if (alwaysVisible == false) {
                if (Input.GetMouseButtonDown(0)) {
                    inside.SetActive(true);
                    outside.SetActive(true);
                }
 
                if (Input.GetMouseButtonUp(0)) {
                    inside.SetActive(false);
                    outside.SetActive(false);
                }
            }
        }
 
        /// <summary>
        /// 更新虚拟摇杆位置
        /// </summary>
        private void UpdateVirtualJoystickPos() {
            //得到虚拟轴的中心位置
            virtualJoystickCenter = Input.mousePosition;
 
            //设置虚拟轴的位置
            virtualJoystick.transform.position = virtualJoystickCenter;
        }
 
        /// <summary>
        /// 更新内环位置
        /// </summary>
        private void UpdateInsidePos() {
            inside.transform.position = Input.mousePosition;
            if (Vector2.Distance(inside.transform.position, virtualJoystickCenter) > maxDragLength) {
                Vector2 normalizedPos = ((Vector2)inside.transform.position - virtualJoystickCenter).normalized;
                inside.transform.position = normalizedPos * maxDragLength + virtualJoystickCenter;
            }
        }
 
        /// <summary>
        /// 判断是否为有效的范围
        /// </summary>
        /// <returns>是否为有效的范围</returns>
        private bool JudgeIsValidRange() {
            if (inside.activeInHierarchy) {
                return true;
            }
 
            Vector2 v = Input.mousePosition;
            if (v.x > (float)Screen.width / 2 || v.x < 0) {
                return false;
            } 
            if (v.y > (float)Screen.height / 2 || v.y < 0) {
                return false;
            }
 
            return true;
        }
 
        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        /// <param name="screenPos">屏幕坐标位置</param>
        /// <param name="camera">相机</param>
        /// <returns>转换后的世界坐标</returns>
        public static Vector2 ScreenToWorld(Vector2 screenPos, Camera camera = null) {
            if (camera == null) {
                camera = Camera.main;
            }
 
            Vector2 _screenPos = new Vector2(screenPos.x, screenPos.y);
            Vector2 v = camera.ScreenToWorldPoint(_screenPos);
            return v;
        }
 
        /// <summary>
        /// 得到运动的方向和长度
        /// </summary>
        /// <returns>方向和长度</returns>
        public Vector2 GetDirAndLength() {
            return (Vector2)inside.transform.position - virtualJoystickCenter;
        }
    }