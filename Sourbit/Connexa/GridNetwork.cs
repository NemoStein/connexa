using System.Collections.Generic;
using Sourbit.Locus;
using UnityEditor;
using UnityEngine;

namespace Sourbit.Connexa
{
    public class GridNetwork : MonoBehaviour
    {
        public GridHelper[] Grids;
        public List<GridConnection> Connections;

        DirectedGraph Graph;

        void Awake()
        {
            Graph = new DirectedGraph();

            if (Grids != null && Connections != null)
            {
                foreach (var grid in Grids)
                {
                    var weights = new float[grid.Columns * grid.Rows];
                    for (var i = 0; i < grid.Columns * grid.Rows; i++)
                    {
                        weights[i] = grid.DefaultWeight;
                    }

                    Graph.CreateGrid(grid.Columns, grid.Rows, weights, grid.AllowDiagonals);
                }

                foreach (var connection in Connections)
                {
                    Graph.Connect(connection.Origin, connection.Target, connection.Weigth);

                    if (!connection.OneWay)
                    {
                        Graph.Connect(connection.Target, connection.Origin, connection.Weigth);
                    }
                }
            }
        }

        public List<int> FindPathBetween(int entry, int exit)
        {
            return Graph.Find(entry, exit);
        }

        public int FindClosestNode(Vector3 position, out bool found)
        {
            found = default;
            int closestNode = default;
            int indexOffset = default;

            if (Grids != null)
            {
                var closestDistance = float.PositiveInfinity;

                foreach (var grid in Grids)
                {
                    var cell = grid.PointToCell(position, out var hasCell);
                    if (hasCell)
                    {
                        found = true;
                        var distance = Vector3.Distance(grid.CellToPoint(cell), position);

                        if (closestDistance > distance)
                        {
                            closestDistance = distance;
                            closestNode = cell.x * grid.Rows + cell.y + indexOffset;
                        }
                    }

                    indexOffset += grid.Columns * grid.Rows;
                }
            }

            return closestNode;
        }

        public Vector3 GetNodePosition(int node, out bool found)
        {
            found = default;
            var nodePosition = new Vector3();

            if (node < 0 && node >= Graph.Nodes)
            {
                return nodePosition;
            }

            if (Grids != null)
            {
                var index = node;
                foreach (var grid in Grids)
                {
                    var gridSize = grid.Rows * grid.Columns;
                    if (index - gridSize < 0)
                    {
                        var x = index / grid.Rows;
                        var y = index % grid.Rows;

                        nodePosition = grid.CellToPoint(x, y);
                        found = true;
                        return nodePosition;
                    }

                    index -= gridSize;
                }
            }

            return nodePosition;
        }

        public GridHelper GetNodeGrid(int node, out bool found)
        {
            found = default;

            if (node < 0 && node >= Graph.Nodes)
            {
                return default;
            }

            if (Grids != null)
            {
                var index = node;
                foreach (var grid in Grids)
                {
                    var gridSize = grid.Rows * grid.Columns;
                    if (index - gridSize < 0)
                    {
                        found = true;
                        return grid;
                    }

                    index -= gridSize;
                }
            }

            return default;
        }

        void OnDrawGizmos()
        {
            foreach (var connection in Connections)
            {
                var scale = 0.35f;
                var origin = GetNodePosition(connection.Origin, out var originFound);
                var target = GetNodePosition(connection.Target, out var targetFound);

                if (originFound && targetFound)
                {
                    origin += (target - origin).normalized * scale + Vector3.up * 0.1f;
                    target += (origin - target).normalized * scale + Vector3.up * 0.1f;

                    var size = Vector3.one * 0.15f;

                    Gizmos.color = new Color(0.9f, 1, 0.4f, 0.75f);
                    Gizmos.DrawLine(origin, target);
                    Gizmos.DrawCube(target, size);

                    if (!connection.OneWay)
                    {
                        Gizmos.DrawCube(origin, size);
                    }
                }
            }
        }
    }

    [CustomEditor(typeof(GridNetwork))]
    public class GridNetworkEditor : Editor
    {
        int ActiveNode = -1;

        public void OnSceneGUI()
        {
            var network = target as GridNetwork;
            var index = 0;

            if (ActiveNode != -1)
            {
                var grid = network.GetNodeGrid(ActiveNode, out _);
                var position = network.GetNodePosition(ActiveNode, out _);

                Handles.DrawSolidDisc(position, grid.transform.up, 0.25f);
            }

            if (network.Grids != null)
            {
                foreach (var grid in network.Grids)
                {
                    for (var column = 0; column < grid.Columns; column++)
                    {
                        for (var row = 0; row < grid.Rows; row++)
                        {
                            var position = grid.CellToPoint(column, row);
                            var rotation = grid.transform.rotation * Quaternion.Euler(-90, 0, 0);

                            if (Handles.Button(position, rotation, 0.35f, 0.5f, Handles.CircleHandleCap))
                            {
                                if (index != ActiveNode)
                                {
                                    if (Event.current.control && ActiveNode >= 0)
                                    {
                                        var delete = false;
                                        foreach (var connection in network.Connections)
                                        {
                                            if ((connection.Origin == ActiveNode && connection.Target == index) ||
                                                (connection.Origin == index && connection.Target == ActiveNode))
                                            {
                                                delete = true;
                                                network.Connections.Remove(connection);
                                                break;
                                            }
                                        }

                                        if (!delete)
                                        {
                                            network.Connections.Add(new()
                                            {
                                                Origin = ActiveNode,
                                                Target = index,
                                                Weigth = grid.DefaultWeight,
                                                OneWay = Event.current.shift
                                            });
                                        }
                                    }
                                }

                                ActiveNode = index;
                                return;
                            }

                            index++;
                        }
                    }
                }

                SceneView.currentDrawingSceneView.Repaint();
            }
        }
    }
}
