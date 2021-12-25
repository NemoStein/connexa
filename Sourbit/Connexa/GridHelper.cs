using UnityEngine;

namespace Sourbit.Connexa
{
    public class GridHelper : MonoBehaviour
    {
        [Min(1)] public int Columns = 1;
        [Min(1)] public int Rows = 1;

        public Vector2Int PointToCell(Vector3 position, out bool found)
        {
            var offset = new Vector3(Columns / 2f, 0, Rows / 2f);
            var projection = transform.InverseTransformPoint(Vector3.ProjectOnPlane(position, transform.up)) + offset;

            var x = Mathf.FloorToInt(projection.x);
            var y = Mathf.FloorToInt(projection.z);

            found = x >= 0 && x < Columns && y >= 0 && y < Rows;
            return new Vector2Int(x, y);
        }

        public Vector3 CellToPoint(Vector2Int value)
        {
            var offset = new Vector3(Columns / 2f, 0, Rows / 2f);
            return transform.TransformPoint(new Vector3(value.x + 0.5f, 0, value.y + 0.5f) - offset);
        }

        public Vector3 CellToPoint(int x, int y)
        {
            return CellToPoint(new Vector2Int(x, y));
        }

        void OnDrawGizmos()
        {
            var offset = new Vector3(Columns / 2f, 0, Rows / 2f);

            Gizmos.color = new Color(0.9f, 1, 0.4f, 0.75f);

            for (var column = 0; column <= Columns; column++)
            {
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector3(column, 0, 0) - offset),
                    transform.TransformPoint(new Vector3(column, 0, Rows) - offset)
                );
            }

            for (var row = 0; row <= Rows; row++)
            {
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector3(0, 0, row) - offset),
                    transform.TransformPoint(new Vector3(Columns, 0, row) - offset)
                );
            }

            Gizmos.DrawLine(
                transform.position,
                transform.position + transform.up
            );
        }

        void OnDrawGizmosSelected()
        {
            var xAxisColor = new Color(1, 0, 0, 1);
            var yAxisColor = new Color(0, 1, 0, 1);
            var guideColor = new Color(1, 1, 1, 1);

            var offset = new Vector3(Columns / 2f, 0, Rows / 2f);

            for (var column = 0; column <= Columns; column++)
            {
                Gizmos.color = column == 0 ? yAxisColor : guideColor;
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector3(column, 0, 0) - offset),
                    transform.TransformPoint(new Vector3(column, 0, Rows) - offset)
                );
            }

            for (var row = 0; row <= Rows; row++)
            {
                Gizmos.color = row == 0 ? xAxisColor : guideColor;
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector3(0, 0, row) - offset),
                    transform.TransformPoint(new Vector3(Columns, 0, row) - offset)
                );
            }

            Gizmos.color = guideColor;
            Gizmos.DrawLine(
                transform.position,
                transform.position + transform.up
            );
        }
    }
}
