using UnityEngine;

namespace CaptainHindsight
{
    public static class Helper
    {
        // Returns an int to set a sorting order - used on SpriteRenderer that doesn't use
        // shaders with Depth Write = Force Enabled
        public static int GetSortingOrder(this Transform transform, float offset = 0)
        {
            return -(int)((transform.position.z + offset) * 100);
        }

        public static void DeleteAllChildGameObjects(Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // Gets a component in a safe way
        public static T GetComponentSafely<T>(this GameObject obj) where T : MonoBehaviour
        {
            T component = obj.GetComponent<T>();
            if (component == null) 
                Log("[Helper] Expected to find component of type " + typeof(T) + " but found none.", obj);
            return component;
        }

        // Gets the angel from a vector and return it as a float
        public static float GetAngelFromVectorFloat(Vector3 direction)
        {
            direction = direction.normalized;
            float n = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;

            return n;
        }

        // Converts a directional vector into an integer
        public static int ConvertDirectionToIndex(Vector3 inputDir)
        {
            // The following integer/direction pairs are being used for movement:
            // 0 = North
            // 1 = West
            // 2 = South
            // 3 = East

            Vector3 normalisedDir = inputDir.normalized;
            float angle = Vector2.SignedAngle(Vector2.up, new Vector2(normalisedDir.x, normalisedDir.z));

            float directionInt;
            if (angle > 0f && angle <= 12f) directionInt = 0;
            else if (angle > 12f && angle <= 150f) directionInt = 1;
            else if (angle > 150f && angle <= 180f) directionInt = 2;
            else if (angle > -180f && angle <= -150f) directionInt = 2;
            else if (angle > -150f && angle <= -12f) directionInt = 3;
            else directionInt = 0;

            //Helper.Log("[Helper] Angle: " + angle + " - Int: " + Mathf.FloorToInt(sliceInt) + ".");
            return Mathf.FloorToInt(directionInt);
        }

        #region Logging
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void Log(object message, Object obj)
        {
            UnityEngine.Debug.Log(message, obj);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogWarning(object message, Object obj)
        {
            UnityEngine.Debug.LogWarning(message, obj);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        static public void LogError(object message, Object obj)
        {
            UnityEngine.Debug.Log(message, obj);
        }
        #endregion
    }
}
